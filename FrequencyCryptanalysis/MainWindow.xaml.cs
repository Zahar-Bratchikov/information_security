using LiveCharts;
using LiveCharts.Wpf;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace FrequencyCryptanalysis
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        // Charts binding properties
        private SeriesCollection _letterSeries;
        public SeriesCollection LetterSeries
        {
            get => _letterSeries;
            set { _letterSeries = value; OnPropertyChanged(nameof(LetterSeries)); }
        }

        private string[] _letterLabels;
        public string[] LetterLabels
        {
            get => _letterLabels;
            set { _letterLabels = value; OnPropertyChanged(nameof(LetterLabels)); }
        }

        private SeriesCollection _bigramSeries;
        public SeriesCollection BigramSeries
        {
            get => _bigramSeries;
            set { _bigramSeries = value; OnPropertyChanged(nameof(BigramSeries)); }
        }

        private string[] _bigramLabels;
        public string[] BigramLabels
        {
            get => _bigramLabels;
            set { _bigramLabels = value; OnPropertyChanged(nameof(BigramLabels)); }
        }

        // Loaded texts and frequency dictionaries
        private string cryptoText = "";
        private string largeText = "";
        private Dictionary<char, int> largeTextLetterFrequencies = new Dictionary<char, int>();
        private Dictionary<string, int> largeTextBigramFrequencies = new Dictionary<string, int>();

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        // Loading crypt text
        private void LoadCryptFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog { Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*" };
            if (dlg.ShowDialog() == true)
            {
                FilePathTextBox.Text = dlg.FileName;
                try
                {
                    cryptoText = File.ReadAllText(dlg.FileName, Encoding.UTF8);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при загрузке файла: " + ex.Message);
                }
            }
        }

        // Processing crypto text (decoding)
        private void ProcessCryptoText_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(FilePathTextBox.Text) && File.Exists(FilePathTextBox.Text))
            {
                cryptoText = File.ReadAllText(FilePathTextBox.Text, Encoding.UTF8);
            }
            if (string.IsNullOrWhiteSpace(cryptoText))
            {
                CryptoResultTextBox.Text = "Текст не загружен.";
                return;
            }
            if (string.IsNullOrWhiteSpace(largeText))
            {
                MessageBox.Show("Сначала загрузите большой текст для анализа частот.");
                return;
            }
            // Update frequency dictionaries
            largeTextLetterFrequencies = FrequencyAnalysis.GetLetterFrequency(largeText);
            largeTextBigramFrequencies = FrequencyAnalysis.GetBigramFrequency(largeText);
            string mode = ((ComboBoxItem)ModeComboBox.SelectedItem).Content.ToString();
            StringBuilder sb = new StringBuilder();
            if (mode == "Цезарь")
            {
                int key = CaesarAnalysis.DetermineKey(cryptoText, largeTextLetterFrequencies);
                sb.AppendLine($"Предполагаемый ключ: {key}");
                sb.AppendLine();
                string plainText = CaesarAnalysis.Decrypt(cryptoText, key);
                sb.AppendLine("Расшифрованный текст (Цезарь):");
                sb.AppendLine(plainText);
            }
            else if (mode == "Виженер")
            {
                if (RandomizeAlphabet?.IsChecked ?? false)
                {
                    sb.AppendLine("При включенной рандомизации алфавита автоматическое определение ключа невозможно.");
                    sb.AppendLine("Используйте режим шифрования с ручным вводом ключа для корректной дешифровки.");
                }
                else
                {
                    int keyLength = VigenereAnalysis.DetermineKeyLength(cryptoText);
                    sb.AppendLine($"Определённая длина ключа: {keyLength}");
                    // Pass both letter and bigram frequencies
                    string key = VigenereAnalysis.DetermineKey(cryptoText, keyLength, largeTextLetterFrequencies, largeTextBigramFrequencies);
                    sb.AppendLine($"Предполагаемый ключ: {key}");
                    sb.AppendLine();
                    string plainText = VigenereAnalysis.Decrypt(cryptoText, key);
                    sb.AppendLine("Расшифрованный текст (Виженер):");
                    sb.AppendLine(plainText);
                }
            }
            CryptoResultTextBox.Text = sb.ToString();
        }

        // Loading large text (for frequency analysis)
        private void LoadLargeTextFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog { Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*" };
            if (dlg.ShowDialog() == true)
            {
                LargeTextFilePathTextBox.Text = dlg.FileName;
                try
                {
                    largeText = File.ReadAllText(dlg.FileName, Encoding.UTF8);
                    largeTextLetterFrequencies = FrequencyAnalysis.GetLetterFrequency(largeText);
                    largeTextBigramFrequencies = FrequencyAnalysis.GetBigramFrequency(largeText);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при загрузке файла: " + ex.Message);
                }
            }
        }

        // Drawing charts for frequencies
        private void DrawCharts_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(largeText))
            {
                MessageBox.Show("Сначала загрузите большой текст для анализа частот.");
                return;
            }
            ProcessFrequencyAnalysis();
        }

        private void ProcessFrequencyAnalysis()
        {
            if (string.IsNullOrWhiteSpace(largeText))
            {
                MessageBox.Show("Текст не загружен.");
                return;
            }
            var letterFreq = FrequencyAnalysis.GetLetterFrequency(largeText);
            var topLetters = FrequencyAnalysis.GetTopN(letterFreq, 10).ToList();
            var bigramFreq = FrequencyAnalysis.GetBigramFrequency(largeText);
            var topBigrams = FrequencyAnalysis.GetTopN(bigramFreq, 10).ToList();
            if (!topLetters.Any() || !topBigrams.Any())
            {
                MessageBox.Show("Недостаточно данных для построения графиков.");
                return;
            }
            LetterLabels = topLetters.Select(x => x.Key.ToString()).ToArray();
            OnPropertyChanged(nameof(LetterLabels));
            var letterValues = topLetters.Select(x => (double)x.Value).ToArray();
            LetterSeries = new SeriesCollection
            {
                new ColumnSeries { Title = "Буквы", Values = new ChartValues<double>(letterValues) }
            };
            OnPropertyChanged(nameof(LetterSeries));

            BigramLabels = topBigrams.Select(x => x.Key.ToString()).ToArray();
            OnPropertyChanged(nameof(BigramLabels));
            var bigramValues = topBigrams.Select(x => (double)x.Value).ToArray();
            BigramSeries = new SeriesCollection
            {
                new ColumnSeries { Title = "Биграммы", Values = new ChartValues<double>(bigramValues) }
            };
            OnPropertyChanged(nameof(BigramSeries));
        }

        // File open for encryption tab
        private void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog { Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*" };
            if (dlg.ShowDialog() == true)
            {
                InputFilePath.Text = dlg.FileName;
            }
        }

        // Caesar cipher encryption/decryption
        private void CaesarProcess_Click(object sender, RoutedEventArgs e)
        {
            string filePath = InputFilePath.Text;
            if (string.IsNullOrWhiteSpace(CaesarKey.Text))
            {
                MessageBox.Show("Поле ключа для шифра Цезаря не должно быть пустым.");
                return;
            }
            if (!int.TryParse(CaesarKey.Text, out int key))
            {
                MessageBox.Show("Ключ Цезаря должен быть целым числом.");
                return;
            }
            string encFilePath = $"encC_{Path.GetFileNameWithoutExtension(filePath)}.txt";
            string decFilePath = $"decC_{Path.GetFileNameWithoutExtension(filePath)}.txt";
            if (!File.Exists(filePath))
            {
                MessageBox.Show("Входной файл не существует.");
                return;
            }
            string text = File.ReadAllText(filePath, Encoding.UTF8);
            if (text.Length < 2000)
            {
                MessageBox.Show("Входной файл должен содержать не менее 2000 символов.");
                return;
            }
            string encryptedText = CaesarAnalysis.CaesarCipher(text, key);
            File.WriteAllText(encFilePath, encryptedText, Encoding.UTF8);
            string decryptedText = CaesarAnalysis.CaesarCipher(encryptedText, -key);
            File.WriteAllText(decFilePath, decryptedText, Encoding.UTF8);
            OutputTextBox.Text = $"Оригинал: {File.ReadLines(filePath, Encoding.UTF8).First()}\n" +
                                 $"Зашифрованный: {File.ReadLines(encFilePath, Encoding.UTF8).First()}\n" +
                                 $"Расшифрованный: {File.ReadLines(decFilePath, Encoding.UTF8).First()}";
        }

        // Vigenere cipher encryption/decryption
        private void VigenereProcess_Click(object sender, RoutedEventArgs e)
        {
            string filePath = InputFilePath.Text;
            if (string.IsNullOrWhiteSpace(VigenereKey.Text))
            {
                MessageBox.Show("Поле ключа для шифра Виженера не должно быть пустым.");
                return;
            }
            string key = VigenereKey.Text;
            bool randomizeAlphabetFlag = RandomizeAlphabet?.IsChecked ?? false;
            string encFilePath = $"encV_{Path.GetFileNameWithoutExtension(filePath)}.txt";
            string decFilePath = $"decV_{Path.GetFileNameWithoutExtension(filePath)}.txt";
            if (!File.Exists(filePath))
            {
                MessageBox.Show("Входной файл не существует.");
                return;
            }
            string text = File.ReadAllText(filePath, Encoding.UTF8);
            if (text.Length < 2000)
            {
                MessageBox.Show("Входной файл должен содержать не менее 2000 символов.");
                return;
            }
            string alphabet = VigenereAnalysis.GenerateAlphabet(randomizeAlphabetFlag);
            string encryptedText = VigenereAnalysis.VigenereCipher(text, key, alphabet);
            File.WriteAllText(encFilePath, encryptedText, Encoding.UTF8);
            string decryptedText = VigenereAnalysis.VigenereCipher(encryptedText, key, alphabet, decrypt: true);
            File.WriteAllText(decFilePath, decryptedText, Encoding.UTF8);
            OutputTextBox.Text = $"Оригинал: {File.ReadLines(filePath, Encoding.UTF8).First()}\n" +
                                 $"Зашифрованный: {File.ReadLines(encFilePath, Encoding.UTF8).First()}\n" +
                                 $"Расшифрованный: {File.ReadLines(decFilePath, Encoding.UTF8).First()}";
            OutputTextBox.Text += $"\n\nКвадрат Виженера:\n{VigenereAnalysis.GenerateVigenereSquare(alphabet, key, randomizeOthers: randomizeAlphabetFlag)}";
        }
    }
}