using System.Linq;
using System.Text;

namespace FrequencyCryptanalysis
{
    public static class CaesarAnalysis
    {
        // Define the alphabet constant here
        private const string Alphabet = "абвгдежзийклмнопрстуфхцчшщъыьэюяё";

        // Determines the key based on letter frequency comparing the cipher text and a reference frequency
        public static int DetermineKey(string cipherText, System.Collections.Generic.Dictionary<char, int> refLetterFreq)
        {
            string filtered = new string(cipherText.ToLower().Where(ch => Alphabet.IndexOf(ch) >= 0).ToArray());
            var freq = FrequencyAnalysis.GetLetterFrequency(filtered);
            if (!freq.Any())
                return 0;
            // Most frequent letter in the cipher text
            char mostFreqCipher = freq.OrderByDescending(kv => kv.Value).First().Key;
            // Most frequent letter in the reference text
            char mostFreqReference = refLetterFreq.OrderByDescending(x => x.Value).First().Key;
            int idxCipher = Alphabet.IndexOf(mostFreqCipher);
            int idxReference = Alphabet.IndexOf(mostFreqReference);
            return (idxCipher - idxReference + Alphabet.Length) % Alphabet.Length;
        }

        public static string Decrypt(string cipherText, int key)
        {
            return CaesarCipher(cipherText, -key);
        }

        // Caesar cipher shift method for both encryption and decryption
        public static string CaesarCipher(string text, int shift)
        {
            StringBuilder sb = new StringBuilder();
            int alphLen = Alphabet.Length;
            foreach (char ch in text)
            {
                if (Alphabet.IndexOf(char.ToLower(ch)) == -1)
                {
                    sb.Append(ch);
                    continue;
                }
                int idx = Alphabet.IndexOf(char.ToLower(ch));
                int newIndex = (idx + shift) % alphLen;
                if (newIndex < 0)
                    newIndex += alphLen;
                char newChar = Alphabet[newIndex];
                sb.Append(char.IsUpper(ch) ? char.ToUpper(newChar) : newChar);
            }
            return sb.ToString();
        }
    }
}