using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace multi_threaded_hashing.Services
{
    /// <summary>
    /// Класс для работы с привязкой потоков к конкретным ядрам процессора
    /// </summary>
    public static class ThreadAffinityHelper
    {
        #region Native Methods
        
        [DllImport("kernel32.dll")]
        private static extern IntPtr GetCurrentThread();
        
        [DllImport("kernel32.dll")]
        private static extern IntPtr SetThreadAffinityMask(IntPtr hThread, IntPtr dwThreadAffinityMask);
        
        [DllImport("kernel32.dll")]
        private static extern uint GetCurrentThreadId();
        
        [DllImport("kernel32.dll")]
        private static extern IntPtr GetCurrentProcess();
        
        [DllImport("kernel32.dll")]
        private static extern bool GetProcessAffinityMask(IntPtr hProcess, out IntPtr lpProcessAffinityMask, out IntPtr lpSystemAffinityMask);
        
        #endregion
        
        /// <summary>
        /// Максимальное количество поддерживаемых ядер для привязки
        /// </summary>
        private const int MaxCores = 12;
        
        /// <summary>
        /// Привязывает текущий поток к указанному ядру процессора
        /// </summary>
        /// <param name="coreIndex">Индекс ядра (0-based)</param>
        /// <returns>True если привязка успешна, иначе false</returns>
        public static bool SetThreadAffinity(int coreIndex)
        {
            try
            {
                if (coreIndex < 0 || coreIndex >= Environment.ProcessorCount)
                {
                    return false;
                }
                
                // Создаем маску для указанного ядра
                IntPtr affinityMask = (IntPtr)(1L << coreIndex);
                
                // Получаем доступные для процесса ядра
                if (!GetProcessAffinityMask(GetCurrentProcess(), out IntPtr processAffinityMask, out _))
                {
                    return false;
                }
                
                // Проверяем, доступно ли указанное ядро для этого процесса
                if ((affinityMask.ToInt64() & processAffinityMask.ToInt64()) == 0)
                {
                    return false;
                }
                
                // Устанавливаем привязку для текущего потока
                IntPtr result = SetThreadAffinityMask(GetCurrentThread(), affinityMask);
                return result != IntPtr.Zero;
            }
            catch
            {
                return false;
            }
        }
        
        /// <summary>
        /// Привязывает текущий поток к оптимальному ядру на основе индекса потока
        /// </summary>
        /// <param name="threadIndex">Индекс потока</param>
        /// <param name="totalThreads">Общее количество потоков</param>
        /// <returns>True если привязка успешна, иначе false</returns>
        public static bool SetOptimalThreadAffinity(int threadIndex, int totalThreads)
        {
            try
            {
                int availableCores = Environment.ProcessorCount;
                
                // Если потоков меньше или равно количеству ядер,
                // распределяем равномерно по доступным ядрам
                if (totalThreads <= availableCores)
                {
                    return SetThreadAffinity(threadIndex % availableCores);
                }
                
                // Если потоков больше, чем ядер, делаем циклическое распределение
                int coreIndex = threadIndex % availableCores;
                return SetThreadAffinity(coreIndex);
            }
            catch
            {
                return false;
            }
        }
        
        /// <summary>
        /// Сбрасывает привязку текущего потока, позволяя ему выполняться на любом доступном ядре
        /// </summary>
        /// <returns>True если операция успешна, иначе false</returns>
        public static bool ResetThreadAffinity()
        {
            try
            {
                // Получаем доступные для процесса ядра
                if (!GetProcessAffinityMask(GetCurrentProcess(), out IntPtr processAffinityMask, out _))
                {
                    return false;
                }
                
                // Устанавливаем привязку ко всем доступным ядрам
                IntPtr result = SetThreadAffinityMask(GetCurrentThread(), processAffinityMask);
                return result != IntPtr.Zero;
            }
            catch
            {
                return false;
            }
        }
    }
} 