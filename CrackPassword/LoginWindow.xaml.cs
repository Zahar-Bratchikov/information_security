using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Microsoft.Win32; // Не забудьте добавить эту директиву для использования OpenFileDialog

namespace CrackPassword
{
    // Окно для логина с дополнительным функционалом подбора пароля (Dictionary и Brute Force)
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
            cmbCrackType.SelectedIndex = 0; // По умолчанию — Dictionary Attack
        }

        // Обработка нажатия кнопки Login
        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string name = txtName.Text;
            string password = txtPassword.Password;
            var user = UserManager.GetUser(name);
            if (user == null)
            {
                MessageBox.Show("User not found.");
                return;
            }
            if (user.IsBlocked)
            {
                MessageBox.Show("This user is blocked.");
                return;
            }
            string hashedPassword = UserManager.ComputeHash(password);
            if (user.PasswordHash != hashedPassword)
            {
                loginAttempts++;
                if (loginAttempts >= 3)
                {
                    MessageBox.Show("Too many incorrect attempts. Exiting application.");
                    Application.Current.Shutdown();
                }
                else
                {
                    MessageBox.Show("Incorrect password.");
                }
                return;
            }
            // При успешном входе открываем MainWindow
            MainWindow mainWindow = new MainWindow(user);
            mainWindow.Show();
            this.Close();
        }

        // Открытие диалога выбора файла словаря
        private void BtnBrowseDictionary_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
            openFileDialog.Title = "Select Dictionary File";
            if (openFileDialog.ShowDialog() == true)
            {
                txtDictionaryPath.Text = openFileDialog.FileName;
            }
        }

        // Запуск процесса подбора пароля
        private async void BtnCrackPassword_Click(object sender, RoutedEventArgs e)
        {
            if (isCracking)
            {
                MessageBox.Show("Cracking already in progress.");
                return;
            }
            isCracking = true;
            crackCancellationTokenSource = new CancellationTokenSource();
            crackingStopwatch = new Stopwatch();
            var user = UserManager.GetUser(txtName.Text);
            if (user == null)
            {
                MessageBox.Show("User not found.");
                isCracking = false;
                return;
            }
            string targetPasswordHash = user.PasswordHash;
            string crackType = ((ComboBoxItem)cmbCrackType.SelectedItem).Content.ToString();

            // Если выбран метод Dictionary Attack, считываем путь из TextBox
            string dictionaryPath;
            if (crackType.StartsWith("Dictionary"))
            {
                dictionaryPath = string.IsNullOrWhiteSpace(txtDictionaryPath.Text)
                    ? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "dictionary.txt")
                    : txtDictionaryPath.Text;
            }
            else
            {
                dictionaryPath = "";
            }

            if (!int.TryParse(txtMaxLength.Text, out int maxLength) || maxLength <= 0)
            {
                MessageBox.Show("Invalid max length.");
                isCracking = false;
                return;
            }

            string crackedPassword = null;
            crackingStopwatch.Start();
            attempts = 0;

            if (crackType.StartsWith("Dictionary"))
            {
                crackedPassword = await DictionaryAttack(targetPasswordHash, dictionaryPath, crackCancellationTokenSource.Token);
            }
            else if (crackType.StartsWith("Brute Force"))
            {
                string charset = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@#$%^&*()_+";
                crackedPassword = await BruteForceCrack(targetPasswordHash, charset, maxLength, crackCancellationTokenSource.Token);
            }

            crackingStopwatch.Stop();
            TimeSpan ts = crackingStopwatch.Elapsed;
            if (crackedPassword != null)
            {
                MessageBox.Show($"Password cracked! The password is: {crackedPassword}\nTime: {ts.TotalSeconds:F3} sec\nSpeed: {(double)attempts / ts.TotalSeconds:F2} attempts/sec");
                txtPassword.Password = crackedPassword;
            }
            else
            {
                MessageBox.Show($"Password not cracked.\nTime: {ts.TotalSeconds:F3} sec\nSpeed: {(double)attempts / ts.TotalSeconds:F2} attempts/sec");
            }
            isCracking = false;
        }

        // Подбор пароля с помощью словаря с транслитерацией
        private async Task<string> DictionaryAttack(string targetHash, string dictionaryPath, CancellationToken cancellationToken)
        {
            return await Task.Run(() =>
            {
                if (!File.Exists(dictionaryPath))
                {
                    Dispatcher.Invoke(() =>
                    {
                        MessageBox.Show($"Dictionary file not found: {dictionaryPath}");
                    });
                    return null;
                }
                string[] dictionary = File.ReadAllLines(dictionaryPath);
                foreach (string word in dictionary)
                {
                    if (cancellationToken.IsCancellationRequested)
                        return null;
                    string transliteratedWord = Translit(word.Trim().ToLower());
                    string hash = UserManager.ComputeHash(transliteratedWord);
                    attempts++;
                    if (attempts % 1000 == 0)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            txtPassword.Password = transliteratedWord;
                        });
                    }
                    if (hash == targetHash)
                        return transliteratedWord;
                }
                return null;
            }, cancellationToken);
        }

        // Подбор пароля методом полного перебора (Brute Force)
        private async Task<string> BruteForceCrack(string targetHash, string charset, int maxLength, CancellationToken cancellationToken)
        {
            return await Task.Run(() =>
            {
                for (int length = 1; length <= maxLength; length++)
                {
                    foreach (string combination in GetCombinations(charset, length, cancellationToken))
                    {
                        if (cancellationToken.IsCancellationRequested)
                            return null;
                        string hash = UserManager.ComputeHash(combination);
                        attempts++;
                        if (attempts % 100000 == 0)
                        {
                            Dispatcher.Invoke(() =>
                            {
                                txtPassword.Password = combination;
                            });
                        }
                        if (hash == targetHash)
                            return combination;
                    }
                }
                return null;
            }, cancellationToken);
        }

        // Рекурсивная генерация комбинаций символов с проверкой отмены
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
                {'а', 'f'}, {'б', ','}, {'в', 'd'}, {'г', 'u'}, {'д', 'l'},
                {'е', 't'}, {'ё', '`'}, {'ж', ';'}, {'з', 'p'}, {'и', 'b'},
                {'й', 'q'}, {'к', 'r'}, {'л', 'k'}, {'м', 'v'}, {'н', 'y'},
                {'о', 'j'}, {'п', 'g'}, {'р', 'h'}, {'с', 'c'}, {'т', 'n'},
                {'у', 'e'}, {'ф', 'a'}, {'х', '['}, {'ц', 'w'}, {'ч', 'x'},
                {'ш', 'i'}, {'щ', 'o'}, {'ь', 'm'}, {'ы', 's'}, {'ъ', ']'},
                {'э', '\''}, {'ю', '.'}, {'я', 'z'}
            };
            var sb = new StringBuilder(input.Length);
            foreach (char c in input)
                sb.Append(map.ContainsKey(c) ? map[c] : c);
            return sb.ToString();
        }

        // Отмена процесса подбора пароля
        private void BtnCancelCrack_Click(object sender, RoutedEventArgs e)
        {
            if (isCracking && crackCancellationTokenSource != null)
            {
                crackCancellationTokenSource.Cancel();
                isCracking = false;
            }
        }

        // При закрытии окна отменяем все запущенные асинхронные процессы
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