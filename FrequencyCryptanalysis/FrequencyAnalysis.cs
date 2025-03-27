using System.Collections.Generic;
using System.Linq;

namespace FrequencyCryptanalysis
{
    public static class FrequencyAnalysis
    {
        // Define the alphabet constant here (not extracted to a separate file)
        private const string Alphabet = "абвгдежзийклмнопрстуфхцчшщъыьэюяё";

        // Returns letter frequency (only letters in Alphabet are considered)
        public static Dictionary<char, int> GetLetterFrequency(string text)
        {
            var freq = new Dictionary<char, int>();
            foreach (char ch in text.ToLower())
            {
                if (Alphabet.IndexOf(ch) >= 0)
                {
                    if (!freq.ContainsKey(ch))
                        freq[ch] = 0;
                    freq[ch]++;
                }
            }
            return freq;
        }

        // Returns bigram frequency (only letters in Alphabet are used)
        public static Dictionary<string, int> GetBigramFrequency(string text)
        {
            var freq = new Dictionary<string, int>();
            string filtered = new string(text.ToLower().Where(ch => Alphabet.IndexOf(ch) >= 0).ToArray());
            for (int i = 0; i < filtered.Length - 1; i++)
            {
                string bigram = filtered.Substring(i, 2);
                if (!freq.ContainsKey(bigram))
                    freq[bigram] = 0;
                freq[bigram]++;
            }
            return freq;
        }

        // Returns the top N items by frequency
        public static Dictionary<TKey, int> GetTopN<TKey>(Dictionary<TKey, int> dict, int n)
        {
            return dict.OrderByDescending(kv => kv.Value)
                       .Take(n)
                       .ToDictionary(kv => kv.Key, kv => kv.Value);
        }
    }
}