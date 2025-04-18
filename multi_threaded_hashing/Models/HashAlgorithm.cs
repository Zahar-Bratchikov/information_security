namespace multi_threaded_hashing.Models
{
    /// <summary>
    /// Перечисление поддерживаемых алгоритмов хеширования
    /// </summary>
    public enum HashAlgorithm
    {
        /// <summary>
        /// MD5 - 128-битный алгоритм хеширования
        /// </summary>
        MD5,

        /// <summary>
        /// SHA1 - 160-битный алгоритм хеширования
        /// </summary>
        SHA1,

        /// <summary>
        /// SHA256 - 256-битный алгоритм хеширования
        /// </summary>
        SHA256,

        /// <summary>
        /// SHA512 - 512-битный алгоритм хеширования
        /// </summary>
        SHA512
    }
}