using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Microsoft.Win32;

namespace CrackPassword
{
    /// <summary>
    /// Логика взаимодействия для LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        private int loginAttempts = 0;
        private CancellationTokenSource crackCancellationTokenSource;
        private Stopwatch crackingStopwatch;
        private long attempts;
        private bool isCracking = false;

        public LoginWindow()
        {
            InitializeComponent();
            cmbCrackType.SelectedIndex = 0;
            // Поле имени оставляем пустым, чтобы пользователь ввёл нужное имя
        }

        // Обработчик кнопки Login – выполняется аутентификация с использованием хэширования пароля
        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string name = txtName.Text;
            string password = txtPassword.Password;
            if (CheckLogin(name, password))
            {
                User currentUser = UserManager.GetUser(name);
                if (currentUser == null)
                {
                    MessageBox.Show("User not found.");
                    return;
                }
                MainWindow mainWindow = new MainWindow(currentUser);
                mainWindow.Show();
                this.Close();
            }
            //else
            //{
            //    loginAttempts++;
            //    if (loginAttempts >= 3)
            //    {
            //        MessageBox.Show("Слишком много неверных попыток. Приложение будет закрыто.");
            //        Application.Current.Shutdown();
            //    }
            //    else
            //    {
            //        MessageBox.Show("Неверный пароль.");
            //    }
            //}
        }

        // Метод для проверки пользователя. Введённый пароль хэшируется и сравнивается с сохранённым хэшом.
        private bool CheckLogin(string username, string password)
        {
            User user = UserManager.GetUser(username);
            if (user == null)
                return false;
            // Если пользователь заблокирован, вход не разрешается
            if (user.IsBlocked)
            {
                MessageBox.Show("User is blocked.");
                return false;
            }
            return UserManager.VerifyPassword(password, user.Password);
        }

        // Обработчик выбора файла словаря для Dictionary Attack
        private void BtnBrowseDictionary_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Текстовые файлы (*.txt)|*.txt|Все файлы (*.*)|*.*";
            openFileDialog.Title = "Выберите файл словаря";
            if (openFileDialog.ShowDialog() == true)
            {
                txtDictionaryPath.Text = openFileDialog.FileName;
            }
        }

        // Метод, имитирующий нажатие кнопки Login при переборе кандидатов (автоматизация)
        private async Task<bool> TryLoginCandidate(string candidatePassword)
        {
            return await Dispatcher.InvokeAsync(async () =>
            {
                txtPassword.Password = candidatePassword;
                BtnLogin_Click(null, null);
                await Task.Delay(100);
                return !this.IsVisible;
            }).Task.Unwrap();
        }

        // Обработчик перебора паролей (Dictionary Attack или Brute Force)
        private async void BtnCrackPassword_Click(object sender, RoutedEventArgs e)
        {
            if (isCracking)
            {
                MessageBox.Show("Процесс подбора уже запущен.");
                return;
            }
            isCracking = true;
            crackCancellationTokenSource = new CancellationTokenSource();
            crackingStopwatch = new Stopwatch();

            string crackType = ((ComboBoxItem)cmbCrackType.SelectedItem).Content.ToString();
            string dictionaryPath = "";

            if (crackType.StartsWith("Dictionary"))
            {
                dictionaryPath = string.IsNullOrWhiteSpace(txtDictionaryPath.Text)
                    ? System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "dictionary.txt")
                    : txtDictionaryPath.Text;
            }

            if (!int.TryParse(txtMaxLength.Text, out int maxLength) || maxLength <= 0)
            {
                MessageBox.Show("Некорректное значение максимальной длины.");
                isCracking = false;
                return;
            }

            string crackedPassword = null;
            crackingStopwatch.Start();
            attempts = 0;

            if (crackType.StartsWith("Dictionary"))
            {
                crackedPassword = await DictionaryAttack(dictionaryPath, crackCancellationTokenSource.Token);
            }
            else if (crackType.StartsWith("Brute Force"))
            {
                string charset = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@#$%^&*()_+";
                crackedPassword = await BruteForceCrack(charset, maxLength, crackCancellationTokenSource.Token);
            }

            crackingStopwatch.Stop();
            TimeSpan ts = crackingStopwatch.Elapsed;

            if (crackedPassword != null)
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    MessageBox.Show(
                        $"Пароль подобран! Это: {crackedPassword}\n" +
                        $"Время: {ts.TotalSeconds:F3} сек\n" +
                        $"Скорость: {(double)attempts / ts.TotalSeconds:F2} попыток/сек",
                        "Результаты подбора");
                }), DispatcherPriority.ApplicationIdle);
            }
            else
            {
                MessageBox.Show($"Пароль не подобран.\nВремя: {ts.TotalSeconds:F3} сек\n" +
                                $"Скорость: {(double)attempts / ts.TotalSeconds:F2} попыток/сек");
            }
            isCracking = false;
        }

        // Словарный перебор (Dictionary Attack)
        private async Task<string> DictionaryAttack(string dictionaryPath, CancellationToken cancellationToken)
        {
            if (!File.Exists(dictionaryPath))
            {
                Dispatcher.Invoke(() =>
                {
                    MessageBox.Show($"Файл словаря не найден: {dictionaryPath}");
                });
                return null;
            }

            string[] dictionary = File.ReadAllLines(dictionaryPath);
            foreach (string word in dictionary)
            {
                if (cancellationToken.IsCancellationRequested)
                    return null;
                string candidate = Translit(word.Trim().ToLower());
                attempts++;
                bool success = await TryLoginCandidate(candidate);
                if (success)
                    return candidate;
            }
            return null;
        }

        // Полный перебор (Brute Force)
        private async Task<string> BruteForceCrack(string charset, int maxLength, CancellationToken cancellationToken)
        {
            for (int length = 1; length <= maxLength; length++)
            {
                foreach (string combination in GetCombinations(charset, length, cancellationToken))
                {
                    if (cancellationToken.IsCancellationRequested)
                        return null;
                    attempts++;
                    bool success = await TryLoginCandidate(combination);
                    if (success)
                        return combination;
                }
            }
            return null;
        }

        // Генерация комбинаций символов рекурсивно
        private IEnumerable<string> GetCombinations(string charset, int length, CancellationToken cancellationToken)
        {
            if (length == 1)
            {
                foreach (char c in charset)
                {
                    if (cancellationToken.IsCancellationRequested)
                        yield break;
                    yield return c.ToString();
                }
            }
            else
            {
                foreach (string prefix in GetCombinations(charset, length - 1, cancellationToken))
                {
                    if (cancellationToken.IsCancellationRequested)
                        yield break;
                    foreach (char c in charset)
                    {
                        if (cancellationToken.IsCancellationRequested)
                            yield break;
                        yield return prefix + c;
                    }
                }
            }
        }

        // Функция транслитерации (русская раскладка -> латиница)
        private string Translit(string input)
        {
            var map = new Dictionary<char, char>
            {
                { 'а', 'f' }, { 'б', ',' }, { 'в', 'd' }, { 'г', 'u' }, { 'д', 'l' },
                { 'е', 't' }, { 'ё', '`' }, { 'ж', ';' }, { 'з', 'p' }, { 'и', 'b' },
                { 'й', 'q' }, { 'к', 'r' }, { 'л', 'k' }, { 'м', 'v' }, { 'н', 'y' },
                { 'о', 'j' }, { 'п', 'g' }, { 'р', 'h' }, { 'с', 'c' }, { 'т', 'n' },
                { 'у', 'e' }, { 'ф', 'a' }, { 'х', '[' }, { 'ц', 'w' }, { 'ч', 'x' },
                { 'ш', 'i' }, { 'щ', 'o' }, { 'ь', 'm' }, { 'ы', 's' }, { 'ъ', ']' },
                { 'э', '\'' }, { 'ю', '.' }, { 'я', 'z' }
            };
            var sb = new System.Text.StringBuilder(input.Length);
            foreach (char c in input)
                sb.Append(map.ContainsKey(c) ? map[c] : c);
            return sb.ToString();
        }

        // Отмена процесса перебора пароля
        private void BtnCancelCrack_Click(object sender, RoutedEventArgs e)
        {
            if (isCracking && crackCancellationTokenSource != null)
            {
                crackCancellationTokenSource.Cancel();
                isCracking = false;
            }
        }

        // При закрытии окна отменяются все запущенные процессы
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            if (crackCancellationTokenSource != null)
            {
                crackCancellationTokenSource.Cancel();
                crackCancellationTokenSource.Dispose();
            }
        }
    }
}