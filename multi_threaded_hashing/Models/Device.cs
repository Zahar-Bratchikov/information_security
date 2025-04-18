namespace multi_threaded_hashing.Models
{
    /// <summary>
    /// Представляет устройство (процессор), используемое для вычислений
    /// </summary>
    public class Device
    {
        /// <summary>
        /// Уникальный идентификатор устройства
        /// </summary>
        public required string Id { get; set; }

        /// <summary>
        /// Название устройства
        /// </summary>
        public required string Name { get; set; }

        /// <summary>
        /// Описание устройства
        /// </summary>
        public required string Description { get; set; }

        /// <summary>
        /// Флаг доступности устройства для вычислений
        /// </summary>
        public bool IsAvailable { get; set; }

        /// <summary>
        /// Оценка производительности устройства
        /// </summary>
        public int PerformanceScore { get; set; }
    }
}