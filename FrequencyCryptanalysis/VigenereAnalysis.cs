using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FrequencyCryptanalysis
{
    public static class VigenereAnalysis
    {
        private const string Alphabet = "абвгдежзийклмнопрстуфхцчшщъыьэюяё";

        // Определяет длину ключа, используя метод индекса совпадения
        public static int DetermineKeyLength(string cipherText, int maxKeyLength = 33)
        {
            string filtered = new string(cipherText.ToLower().Where(ch => Alphabet.IndexOf(ch) >= 0).ToArray());
            double bestIC = 0;
            int bestKeyLength = 1;
            for (int keyLen = 1; keyLen <= maxKeyLength; keyLen++)
            {
                double icSum = 0;
                for (int i = 0; i < keyLen; i++)
                {
                    string subtext = "";
                    for (int j = i; j < filtered.Length; j += keyLen)
                        subtext += filtered[j];
                    icSum += CalculateIC(subtext);
                }
                double avgIC = icSum / keyLen;
                if (avgIC > bestIC)
                {
                    bestIC = avgIC;
                    bestKeyLength = keyLen;
                }
            }
            return bestKeyLength;
        }

        // Вычисляет индекс совпадения для данного текста
        private static double CalculateIC(string text)
        {
            var freq = FrequencyAnalysis.GetLetterFrequency(text);
            int N = text.Length;
            if (N <= 1)
                return 0;
            double ic = 0;
            foreach (var count in freq.Values)
                ic += count * (count - 1);
            return (double)ic / (N * (N - 1));
        }

        // Определяет ключ для шифра Виженера, комбинируя анализ частот букв и биграмм.
        public static string DetermineKey(string cipherText, int keyLength,
            Dictionary<char, int> refLetterFreq, Dictionary<string, int> refBigramFreq)
        {
            string filtered = new string(cipherText.ToLower().Where(ch => Alphabet.IndexOf(ch) >= 0).ToArray());
            StringBuilder keyBuilder = new StringBuilder();
            for (int i = 0; i < keyLength; i++)
            {
                string subtext = "";
                for (int j = i; j < filtered.Length; j += keyLength)
                    subtext += filtered[j];
                int shift = DetermineShiftForSubtextCombined(subtext, refLetterFreq, refBigramFreq);
                keyBuilder.Append(Alphabet[shift]);
            }
            return keyBuilder.ToString();
        }

        // Определяет наилучший сдвиг для данной подстроки, используя комбинированную метрику ошибок для букв и биграмм.
        public static int DetermineShiftForSubtextCombined(string subText,
            Dictionary<char, int> refLetterFreq, Dictionary<string, int> refBigramFreq,
            double weightLetter = 1.0, double weightBigram = 1.0)
        {
            if (string.IsNullOrEmpty(subText))
                return 0;
            int alphLength = Alphabet.Length;
            double bestScore = double.MaxValue;
            int bestShift = 0;
            for (int shift = 0; shift < alphLength; shift++)
            {
                string shifted = ApplyShift(subText, shift);
                var subLetterFreq = FrequencyAnalysis.GetLetterFrequency(shifted);
                var subBigramFreq = FrequencyAnalysis.GetBigramFrequency(shifted);

                double letterError = 0;
                foreach (var kv in refLetterFreq)
                {
                    char letter = kv.Key;
                    int refCount = kv.Value;
                    int subCount = subLetterFreq.ContainsKey(letter) ? subLetterFreq[letter] : 0;
                    letterError += Math.Abs(refCount - subCount);
                }

                double bigramError = 0;
                foreach (var kv in refBigramFreq)
                {
                    string bigram = kv.Key;
                    int refCount = kv.Value;
                    int subCount = subBigramFreq.ContainsKey(bigram) ? subBigramFreq[bigram] : 0;
                    bigramError += Math.Abs(refCount - subCount);
                }

                double totalError = weightLetter * letterError + weightBigram * bigramError;
                if (totalError < bestScore)
                {
                    bestScore = totalError;
                    bestShift = shift;
                }
            }
            return bestShift;
        }

        // Применяет обратный сдвиг к тексту, используя Alphabet
        private static string ApplyShift(string text, int shift)
        {
            StringBuilder sb = new StringBuilder();
            int alphLen = Alphabet.Length;
            foreach (char ch in text)
            {
                int idx = Alphabet.IndexOf(ch);
                if (idx == -1)
                {
                    sb.Append(ch);
                    continue;
                }
                int newIndex = (idx - shift + alphLen) % alphLen;
                sb.Append(Alphabet[newIndex]);
            }
            return sb.ToString();
        }

        public static string Decrypt(string cipherText, string key)
        {
            return VigenereCipher(cipherText, key, Alphabet, decrypt: true);
        }

        // Метод шифра Виженера для шифрования/дешифрования
        public static string VigenereCipher(string text, string key, string alphabet, bool decrypt = false)
        {
            StringBuilder result = new StringBuilder();
            int keyIndex = 0;
            int alphLen = alphabet.Length;
            foreach (char ch in text)
            {
                char lowerCh = char.ToLower(ch);
                int textIndex = alphabet.IndexOf(lowerCh);
                if (textIndex == -1)
                {
                    result.Append(ch);
                    continue;
                }
                char lowerKeyChar = char.ToLower(key[keyIndex % key.Length]);
                int keyShift = alphabet.IndexOf(lowerKeyChar);
                if (keyShift == -1)
                {
                    result.Append(ch);
                    continue;
                }
                if (decrypt)
                    keyShift = -keyShift;
                int newIndex = (textIndex + keyShift) % alphLen;
                if (newIndex < 0)
                    newIndex += alphLen;
                char newChar = alphabet[newIndex];
                result.Append(char.IsUpper(ch) ? char.ToUpper(newChar) : newChar);
                keyIndex++;
            }
            return result.ToString();
        }

        // Генерирует алфавит для шифра Виженера; если randomize равно true, буквы перемешиваются.
        public static string GenerateAlphabet(bool randomize)
        {
            char[] alph = Alphabet.ToCharArray();
            if (randomize)
            {
                Random rand = new Random();
                alph = alph.OrderBy(x => rand.Next()).ToArray();
            }
            return new string(alph);
        }

        // Генерирует квадрат Виженера, используя заданный алфавит и ключ.
        public static string GenerateVigenereSquare(string alphabet, string key, bool randomizeOthers = false)
        {
            char[] sortedAlphabetArr = alphabet.ToCharArray();
            Array.Sort(sortedAlphabetArr);
            string fixedColumn = new string(sortedAlphabetArr);

            StringBuilder square = new StringBuilder();
            int len = fixedColumn.Length;
            Random rand = new Random();

            for (int i = 0; i < len; i++)
            {
                char fixedLetter = fixedColumn[i];
                square.Append(fixedLetter);
                square.Append(" | ");
                List<char> remaining = new List<char>();
                foreach (char ch in sortedAlphabetArr)
                {
                    if (ch != fixedLetter)
                        remaining.Add(ch);
                }
                if (randomizeOthers)
                {
                    for (int j = remaining.Count - 1; j > 0; j--)
                    {
                        int k = rand.Next(j + 1);
                        char temp = remaining[j];
                        remaining[j] = remaining[k];
                        remaining[k] = temp;
                    }
                }
                else
                {
                    int shift = i % remaining.Count;
                    remaining = remaining.Skip(shift).Concat(remaining.Take(shift)).ToList();
                }
                foreach (char ch in remaining)
                    square.Append(ch);
                square.AppendLine();
            }
            return square.ToString();
        }
    }
}