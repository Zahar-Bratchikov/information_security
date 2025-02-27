using System;
using System.IO;
using System.Windows;
using Microsoft.Win32;

namespace CryptoApp
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void CaesarButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;
                string text = File.ReadAllText(filePath);
                if (text.Length < 2000)
                {
                    MessageBox.Show("The file must contain at least 2000 characters.");
                    return;
                }

                int key = 3; // Replace with user input for key
                string encryptedText = CaesarEncrypt(text, key);
                string encryptedFilePath = "encC_" + Path.GetFileName(filePath);
                File.WriteAllText(encryptedFilePath, encryptedText);

                string decryptedText = CaesarDecrypt(encryptedText, key);
                string decryptedFilePath = "decC_" + Path.GetFileName(filePath);
                File.WriteAllText(decryptedFilePath, decryptedText);

                OutputTextBox.Text = "Original:\n" + text.Substring(0, 100) + "\n\n";
                OutputTextBox.Text += "Encrypted:\n" + encryptedText.Substring(0, 100) + "\n\n";
                OutputTextBox.Text += "Decrypted:\n" + decryptedText.Substring(0, 100) + "\n\n";
            }
        }

        private void VigenereButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;
                string text = File.ReadAllText(filePath);
                if (text.Length < 2000)
                {
                    MessageBox.Show("The file must contain at least 2000 characters.");
                    return;
                }

                string key = "KEY"; // Replace with user input for key
                string encryptedText = VigenereEncrypt(text, key);
                string encryptedFilePath = "encV_" + Path.GetFileName(filePath);
                File.WriteAllText(encryptedFilePath, encryptedText);

                string decryptedText = VigenereDecrypt(encryptedText, key);
                string decryptedFilePath = "decV_" + Path.GetFileName(filePath);
                File.WriteAllText(decryptedFilePath, decryptedText);

                OutputTextBox.Text = "Original:\n" + text.Substring(0, 100) + "\n\n";
                OutputTextBox.Text += "Encrypted:\n" + encryptedText.Substring(0, 100) + "\n\n";
                OutputTextBox.Text += "Decrypted:\n" + decryptedText.Substring(0, 100) + "\n\n";
            }
        }

        private string CaesarEncrypt(string input, int key)
        {
            char[] buffer = input.ToCharArray();
            for (int i = 0; i < buffer.Length; i++)
            {
                char letter = buffer[i];
                letter = (char)(letter + key);
                if (letter > 'z')
                {
                    letter = (char)(letter - 26);
                }
                else if (letter < 'a')
                {
                    letter = (char)(letter + 26);
                }
                buffer[i] = letter;
            }
            return new string(buffer);
        }

        private string CaesarDecrypt(string input, int key)
        {
            return CaesarEncrypt(input, -key);
        }

        private string VigenereEncrypt(string input, string key)
        {
            string result = string.Empty;
            for (int i = 0; i < input.Length; i++)
            {
                char c = (char)((input[i] + key[i % key.Length]) % 256);
                result += c;
            }
            return result;
        }

        private string VigenereDecrypt(string input, string key)
        {
            string result = string.Empty;
            for (int i = 0; i < input.Length; i++)
            {
                char c = (char)((input[i] - key[i % key.Length] + 256) % 256);
                result += c;
            }
            return result;
        }
    }
}