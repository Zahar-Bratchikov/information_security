using System.Collections.Generic;
using System.Linq;

namespace FrequencyCryptanalysis
{
    public static class FrequencyAnalysis
    {
        // Подсчитывает частотный анализ букв (учитываются только символы из русского алфавита)
        public static Dictionary<char, int> GetLetterFrequency(string text)
        {
            var freq = new Dictionary<char, int>();
            foreach (char ch in text.ToLower())
            {
                if (AlphabetHelper.IsInAlphabet(ch))
                {
                    if (!freq.ContainsKey(ch))
                        freq[ch] = 0;
                    freq[ch]++;
                }
            }
            return freq;
        }

        // Подсчитывает частотный анализ биграмм (учитываются только буквы русского алфавита)
        public static Dictionary<string, int> GetBigramFrequency(string text)
        {
            var freq = new Dictionary<string, int>();
            string filtered = new string(text.ToLower().Where(ch => AlphabetHelper.IsInAlphabet(ch)).ToArray());
            for (int i = 0; i < filtered.Length - 1; i++)
            {
                string bigram = filtered.Substring(i, 2);
                if (!freq.ContainsKey(bigram))
                    freq[bigram] = 0;
                freq[bigram]++;
            }
            return freq;
        }

        // Возвращает топ-N элементов по частоте из словаря
        public static Dictionary<TKey, int> GetTopN<TKey>(Dictionary<TKey, int> dict, int n)
        {
            return dict.OrderByDescending(kv => kv.Value)
                       .Take(n)
                       .ToDictionary(kv => kv.Key, kv => kv.Value);
        }
    }

    public static class AlphabetHelper
    {
        public static readonly string Alphabet = "абвгдежзийклмнопрстуфхцчшщъыьэюяё";

        public static bool IsInAlphabet(char ch)
        {
            return Alphabet.IndexOf(ch) >= 0;
        }
    }
}