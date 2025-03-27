using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FrequencyCryptanalysis
{
    public static class VigenereAnalysis
    {
        // Определяет длину ключа методом индекса совпадений
        public static int DetermineKeyLength(string cipherText, int maxKeyLength = 20)
        {
            cipherText = new string(cipherText.ToLower().Where(ch => AlphabetHelper.IsInAlphabet(ch)).ToArray());
            double bestIC = 0;
            int bestKeyLength = 1;
            for (int keyLen = 1; keyLen <= maxKeyLength; keyLen++)
            {
                double icSum = 0;
                for (int i = 0; i < keyLen; i++)
                {
                    string subtext = "";
                    for (int j = i; j < cipherText.Length; j += keyLen)
                        subtext += cipherText[j];
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

        // Вычисляет индекс совпадений для заданного текста
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

        // Определяет ключ шифротекста, зная длину ключа, используя частотный анализ
        public static string DetermineKey(string cipherText, int keyLength, Dictionary<char, int> largeTextLetterFrequencies)
        {
            cipherText = new string(cipherText.ToLower().Where(ch => AlphabetHelper.IsInAlphabet(ch)).ToArray());
            StringBuilder keyBuilder = new StringBuilder();
            for (int i = 0; i < keyLength; i++)
            {
                string subtext = "";
                for (int j = i; j < cipherText.Length; j += keyLength)
                    subtext += cipherText[j];
                int shift = DetermineShiftForSubtext(subtext, largeTextLetterFrequencies);
                keyBuilder.Append(AlphabetHelper.Alphabet[shift]);
            }
            return keyBuilder.ToString();
        }

        // Вычисляет сдвиг для подстроки на основе частотного анализа 
        // (ожидается, что открытый текст должен иметь "о" как наиболее частую букву)
        public static int DetermineShiftForSubtext(string subText, Dictionary<char, int> largeTextLetterFrequencies)
        {
            if (string.IsNullOrEmpty(subText))
                return 0;
            var letterFreq = FrequencyAnalysis.GetLetterFrequency(subText);
            if (!letterFreq.Any())
                return 0;
            char mostFrequent = letterFreq.OrderByDescending(kv => kv.Value).First().Key;
            int idxSub = AlphabetHelper.Alphabet.IndexOf(mostFrequent);
            int idxO = AlphabetHelper.Alphabet.IndexOf('о');
            return (idxSub - idxO + AlphabetHelper.Alphabet.Length) % AlphabetHelper.Alphabet.Length;
        }

        // Дешифрует шифротекст Виженера по заданному ключу
        public static string Decrypt(string cipherText, string key)
        {
            return VigenereCipher(cipherText, key, AlphabetHelper.Alphabet, decrypt: true);
        }

        // Метод шифрования/дешифрования Виженера
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

        // Генерирует алфавит для шифра Виженера (русский алфавит с буквой ё).
        // Если randomize установлен, перемешивает буквы, иначе возвращает упорядоченный алфавит.
        public static string GenerateAlphabet(bool randomize)
        {
            char[] alph = AlphabetHelper.Alphabet.ToCharArray();
            if (randomize)
            {
                Random rand = new Random();
                alph = alph.OrderBy(x => rand.Next()).ToArray();
            }
            return new string(alph);
        }

        // Генерирует квадрат Виженера для заданного алфавита и ключа.
        // Первый столбец остается фиксированным – отсортированный алфавит.
        // Если randomizeOthers == true, то для каждой строки остальные буквы (кроме первой) формируются
        // случайным образом (алгоритм Фишера–Йетса), иначе используется циклический сдвиг.
        public static string GenerateVigenereSquare(string alphabet, string key, bool randomizeOthers = false)
        {
            // Фиксированный первый столбец: отсортированный алфавит.
            char[] sortedAlphabetArr = alphabet.ToCharArray();
            Array.Sort(sortedAlphabetArr);
            string fixedColumn = new string(sortedAlphabetArr);

            StringBuilder square = new StringBuilder();
            int len = fixedColumn.Length;
            Random rand = new Random();

            for (int i = 0; i < len; i++)
            {
                // Выводим фиксированную букву (первый столбец)
                char fixedLetter = fixedColumn[i];
                square.Append(fixedLetter);
                square.Append(" | ");

                // Формируем список оставшихся букв (исключая фиксированную)
                List<char> remaining = new List<char>();
                foreach (char ch in sortedAlphabetArr)
                {
                    if (ch != fixedLetter)
                        remaining.Add(ch);
                }

                if (randomizeOthers)
                {
                    // Перемешиваем оставшиеся буквы алгоритмом Фишера–Йетса.
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
                    // Выполняем циклический сдвиг на i позиций.
                    int shift = i % remaining.Count;
                    remaining = remaining.Skip(shift).Concat(remaining.Take(shift)).ToList();
                }

                foreach (char ch in remaining)
                {
                    square.Append(ch);
                }
                square.AppendLine();
            }
            return square.ToString();
        }
    }
}