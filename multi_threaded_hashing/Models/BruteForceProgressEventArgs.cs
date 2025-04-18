namespace multi_threaded_hashing.Models
{
    /// <summary>
    /// Аргументы события прогресса брутфорса
    /// </summary>
    public class BruteForceProgressEventArgs : EventArgs
    {
        /// <summary>
        /// Процент выполнения (0-100)
        /// </summary>
        public int Progress { get; set; }

        /// <summary>
        /// Текущая проверяемая комбинация
        /// </summary>
        public string CurrentAttempt { get; set; } = string.Empty;

        /// <summary>
        /// Прошедшее время с начала операции
        /// </summary>
        public TimeSpan ElapsedTime { get; set; }

        /// <summary>
        /// Общее количество проверенных комбинаций
        /// </summary>
        public long TotalAttempts { get; set; }
    }
}