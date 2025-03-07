using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;

namespace CryptoApp
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Обработчик для кнопки выполнения метода Цезаря.
        /// Считывает входной файл, шифрует и дешифрует текст методом Цезаря,
        /// и выводит результаты в текстовое поле.
        /// </summary>
        private void CaesarProcess_Click(object sender, RoutedEventArgs e)
        {
            // Получение пути к файлу и ключа Цезаря от пользователя
            string filePath = InputFilePath.Text;

            // Проверка, что поле ключа не пустое
            if (string.IsNullOrWhiteSpace(CaesarKey.Text))
            {
                MessageBox.Show("Поле ключа для шифра Цезаря не должно быть пустым.");
                return;
            }

            int key = int.Parse(CaesarKey.Text);
            string encFilePath = $"encC_{Path.GetFileNameWithoutExtension(filePath)}.txt";
            string decFilePath = $"decC_{Path.GetFileNameWithoutExtension(filePath)}.txt";

            // Проверка существования входного файла
            if (!File.Exists(filePath))
            {
                MessageBox.Show("Входной файл не существует.");
                return;
            }

            // Чтение содержимого входного файла
            string text = File.ReadAllText(filePath);
            if (text.Length < 2000)
            {
                MessageBox.Show("Входной файл должен содержать не менее 2000 символов.");
                return;
            }

            // Шифрование текста методом Цезаря
            string encryptedText = CaesarCipher(text, key);
            File.WriteAllText(encFilePath, encryptedText);

            // Дешифрование текста методом Цезаря
            string decryptedText = CaesarCipher(encryptedText, -key);
            File.WriteAllText(decFilePath, decryptedText);

            // Вывод первых строк оригинального, зашифрованного и расшифрованного текста в поле вывода
            OutputTextBox.Text = $"Оригинал: {File.ReadLines(filePath).First()}\n" +
                                 $"Зашифрованный: {File.ReadLines(encFilePath).First()}\n" +
                                 $"Расшифрованный: {File.ReadLines(decFilePath).First()}";
        }

        /// <summary>
        /// Обработчик для кнопки выполнения метода Виженера.
        /// Считывает входной файл, шифрует и дешифрует текст методом Виженера,
        /// выводит результаты в текстовое поле и строит квадрат Виженера с дополнительными строками.
        /// </summary>
        private void VigenereProcess_Click(object sender, RoutedEventArgs e)
        {
            // Получение пути к файлу, ключа Виженера и опции рандомизации алфавита от пользователя
            string filePath = InputFilePath.Text;

            // Проверка, что поле ключа не пустое
            if (string.IsNullOrWhiteSpace(VigenereKey.Text))
            {
                MessageBox.Show("Поле ключа для шифра Виженера не должно быть пустым.");
                return;
            }
            string key = VigenereKey.Text;

            bool randomizeAlphabet = (bool)RandomizeAlphabet.IsChecked;
            string encFilePath = $"encV_{Path.GetFileNameWithoutExtension(filePath)}.txt";
            string decFilePath = $"decV_{Path.GetFileNameWithoutExtension(filePath)}.txt";

            // Проверка существования входного файла
            if (!File.Exists(filePath))
            {
                MessageBox.Show("Входной файл не существует.");
                return;
            }

            // Чтение содержимого входного файла
            string text = File.ReadAllText(filePath);
            if (text.Length < 2000)
            {
                MessageBox.Show("Входной файл должен содержать не менее 2000 символов.");
                return;
            }

            // Генерация алфавита (упорядоченного или случайного)
            string alphabet = GenerateAlphabet(randomizeAlphabet);

            // Шифрование текста методом Виженера
            string encryptedText = VigenereCipher(text, key, alphabet);
            File.WriteAllText(encFilePath, encryptedText);

            // Дешифрование текста методом Виженера
            string decryptedText = VigenereCipher(encryptedText, key, alphabet, decrypt: true);
            File.WriteAllText(decFilePath, decryptedText);

            // Вывод первых строк оригинального, зашифрованного и расшифрованного текста в поле вывода
            OutputTextBox.Text = $"Оригинал: {File.ReadLines(filePath).First()}\n" +
                                 $"Зашифрованный: {File.ReadLines(encFilePath).First()}\n" +
                                 $"Расшифрованный: {File.ReadLines(decFilePath).First()}";

            // Вывод квадрата Виженера в поле вывода.
            // Квадрат не выводит строку с ключом; между первым столбцом и остальными добавлен вертикальный разделитель
            OutputTextBox.Text += $"\n\nКвадрат Виженера:\n{GenerateVigenereSquare(alphabet, key)}";
        }

        /// <summary>
        /// Шифрует или дешифрует входной текст методом Цезаря.
        /// </summary>
        /// <param name="text">Входной текст для обработки.</param>
        /// <param name="shift">Значение сдвига для метода Цезаря.</param>
        /// <returns>Обработанный текст после применения метода Цезаря.</returns>
        private string CaesarCipher(string text, int shift)
        {
            StringBuilder result = new StringBuilder();
            foreach (char ch in text)
            {
                if (char.IsLetter(ch))
                {
                    // Определяем смещение в зависимости от регистра буквы
                    char offset = char.IsUpper(ch) ? 'A' : 'a';
                    result.Append((char)(((ch + shift - offset) % 26 + 26) % 26 + offset));
                }
                else
                {
                    result.Append(ch);
                }
            }
            return result.ToString();
        }

        /// <summary>
        /// Шифрует или дешифрует входной текст методом Виженера.
        /// </summary>
        /// <param name="text">Входной текст для обработки.</param>
        /// <param name="key">Ключ для метода Виженера.</param>
        /// <param name="alphabet">Алфавит, используемый для метода Виженера.</param>
        /// <param name="decrypt">Указывает, осуществляется ли дешифрование (по умолчанию false).</param>
        /// <returns>Обработанный текст после применения метода Виженера.</returns>
        private string VigenereCipher(string text, string key, string alphabet, bool decrypt = false)
        {
            StringBuilder result = new StringBuilder();
            int keyIndex = 0;

            foreach (char ch in text)
            {
                if (char.IsLetter(ch))
                {
                    // Получаем индекс текущей буквы в алфавите
                    int textIndex = alphabet.IndexOf(char.ToUpper(ch));
                    // Определяем сдвиг, используя соответствующую букву ключа (ключ повторяется циклически)
                    int keyShift = alphabet.IndexOf(char.ToUpper(key[keyIndex % key.Length]));
                    if (decrypt)
                    {
                        keyShift = -keyShift;
                    }
                    char offset = char.IsUpper(ch) ? 'A' : 'a';
                    int newIndex = (textIndex + keyShift + 26) % 26;
                    // Преобразуем зашифрованную/расшифрованную букву в нужный регистр
                    result.Append((char)(alphabet[newIndex] + (char.IsUpper(ch) ? 0 : 32)));
                    keyIndex++;
                }
                else
                {
                    result.Append(ch);
                }
            }
            return result.ToString();
        }

        /// <summary>
        /// Генерирует алфавит для метода Виженера.
        /// </summary>
        /// <param name="randomize">Указывает, требуется ли рандомизация алфавита (по умолчанию false).</param>
        /// <returns>Сгенерированный алфавит в виде строки.</returns>
        private string GenerateAlphabet(bool randomize = false)
        {
            char[] alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
            if (randomize)
            {
                Random rand = new Random();
                alphabet = alphabet.OrderBy(x => rand.Next()).ToArray();
            }
            return new string(alphabet);
        }

        /// <summary>
        /// Генерирует квадрат Виженера.
        /// Первый столбец – английский алфавит, обозначающий сдвиг каждой строки. 
        /// </summary>
        /// <param name="alphabet">Алфавит для построения квадрата.</param>
        /// <param name="key">Ключ, который используется для шифрования (используется в процедуре шифрования, но вывод не производится).</param>
        /// <returns>Строковое представление квадрата Виженера.</returns>
        private string GenerateVigenereSquare(string alphabet, string key)
        {
            StringBuilder square = new StringBuilder();
            int columns = alphabet.Length; // Обычно 26

            // Для каждой строки квадрата (26 строк)
            for (int i = 0; i < columns; i++)
            {
                // Первый столбец – буква английского алфавита, обозначающая текущий сдвиг (A, B, C, ...)
                square.Append((char)('A' + i));
                // Вертикальный разделитель между первым столбцом и остальными
                square.Append(" | ");
                // Выводим сдвинутый алфавит согласно текущему сдвигу
                for (int j = 0; j < columns; j++)
                {
                    square.Append(alphabet[(i + j) % columns]);
                }
                square.AppendLine();
            }
            return square.ToString();
        }

        /// <summary>
        /// Обработчик для кнопки открытия файла.
        /// Открывает диалоговое окно для выбора файла и устанавливает выбранный путь в текстовое поле.
        /// </summary>
        private void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Файлы текста (*.txt)|*.txt|Все файлы (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                InputFilePath.Text = openFileDialog.FileName;
            }
        }
    }
}