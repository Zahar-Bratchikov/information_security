namespace multi_threaded_hashing.Models
{
    /// <summary>
    /// Представляет информацию о хеш-функции
    /// </summary>
    public class HashFunction
    {
        /// <summary>
        /// Название хеш-функции
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Тип алгоритма хеширования
        /// </summary>
        public HashAlgorithm Algorithm { get; set; }

        /// <summary>
        /// Размер выходного значения в битах
        /// </summary>
        public int OutputSize { get; set; }

        /// <summary>
        /// Описание хеш-функции
        /// </summary>
        public string Description { get; set; } = string.Empty;
    }
}
