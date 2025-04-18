namespace multi_threaded_hashing.Models
{
    /// <summary>
    /// Настройки для операции брутфорса (перебора)
    /// </summary>
    public class BruteForceSettings
    {
        /// <summary>
        /// Целевой хеш, который нужно подобрать
        /// </summary>
        public required string TargetHash { get; set; }

        /// <summary>
        /// Алгоритм хеширования, используемый для вычисления хеша
        /// </summary>
        public HashAlgorithm Algorithm { get; set; }

        /// <summary>
        /// Количество потоков для параллельной обработки
        /// </summary>
        public int ThreadCount { get; set; }

        /// <summary>
        /// Алфавит для генерации комбинаций
        /// </summary>
        public required string Alphabet { get; set; }

        /// <summary>
        /// Минимальная длина пароля
        /// </summary>
        public int MinLength { get; set; }

        /// <summary>
        /// Максимальная длина пароля
        /// </summary>
        public int MaxLength { get; set; }
    }
}