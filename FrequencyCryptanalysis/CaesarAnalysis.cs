using System.Linq;
using System.Text;

namespace FrequencyCryptanalysis
{
    public static class CaesarAnalysis
    {
        // Определяет сдвиг для шифра Цезаря на основе частотного анализа шифротекста
        public static int DetermineKey(string cipherText, System.Collections.Generic.Dictionary<char, int> largeTextLetterFrequencies)
        {
            cipherText = new string(cipherText.ToLower().Where(ch => AlphabetHelper.IsInAlphabet(ch)).ToArray());
            var freq = FrequencyAnalysis.GetLetterFrequency(cipherText);
            if (!freq.Any()) return 0;
            char mostFrequent = freq.OrderByDescending(kv => kv.Value).First().Key;
            int idxText = AlphabetHelper.Alphabet.IndexOf(mostFrequent);
            int idxO = AlphabetHelper.Alphabet.IndexOf('о');
            return (idxText - idxO + AlphabetHelper.Alphabet.Length) % AlphabetHelper.Alphabet.Length;
        }

        // Дешифрует текст Цезаря, используя сдвиг
        public static string Decrypt(string cipherText, int key)
        {
            return CaesarCipher(cipherText, -key);
        }

        // Метод шифрования/дешифрования шифра Цезаря
        public static string CaesarCipher(string text, int shift)
        {
            StringBuilder sb = new StringBuilder();
            int alphLen = AlphabetHelper.Alphabet.Length;
            foreach (char ch in text)
            {
                if (!AlphabetHelper.IsInAlphabet(char.ToLower(ch)))
                {
                    sb.Append(ch);
                    continue;
                }
                int idx = AlphabetHelper.Alphabet.IndexOf(char.ToLower(ch));
                int newIndex = (idx + shift) % alphLen;
                if (newIndex < 0) newIndex += alphLen;
                char newChar = AlphabetHelper.Alphabet[newIndex];
                sb.Append(char.IsUpper(ch) ? char.ToUpper(newChar) : newChar);
            }
            return sb.ToString();
        }
    }
}