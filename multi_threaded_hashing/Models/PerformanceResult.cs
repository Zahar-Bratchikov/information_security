namespace multi_threaded_hashing.Models
{
    /// <summary>
    /// Результаты теста производительности
    /// </summary>
    public class PerformanceResult
    {
        /// <summary>
        /// Идентификатор устройства, на котором выполнялся тест
        /// </summary>
        public string DeviceId { get; set; } = string.Empty;

        /// <summary>
        /// Название устройства
        /// </summary>
        public string DeviceName { get; set; } = string.Empty;

        /// <summary>
        /// Алгоритм хеширования, использованный в тесте
        /// </summary>
        public HashAlgorithm Algorithm { get; set; }

        /// <summary>
        /// Количество потоков, использованных в тесте
        /// </summary>
        public int ThreadCount { get; set; }

        /// <summary>
        /// Продолжительность выполнения теста в миллисекундах
        /// </summary>
        public double Duration { get; set; }

        /// <summary>
        /// Количество операций в секунду
        /// </summary>
        public double OperationsPerSecond { get; set; }
    }
}