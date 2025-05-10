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
        [DllImport("kernel32.dll")]
        private static extern IntPtr GetCurrentThread();

        [DllImport("kernel32.dll")]
        private static extern IntPtr SetThreadAffinityMask(IntPtr hThread, IntPtr dwThreadAffinityMask);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetCurrentProcess();

        [DllImport("kernel32.dll")]
        private static extern bool GetProcessAffinityMask(IntPtr hProcess, out IntPtr lpProcessAffinityMask, out IntPtr lpSystemAffinityMask);

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
                    return false;
                IntPtr affinityMask = (IntPtr)(1L << coreIndex);
                if (!GetProcessAffinityMask(GetCurrentProcess(), out IntPtr processAffinityMask, out _))
                    return false;
                if ((affinityMask.ToInt64() & processAffinityMask.ToInt64()) == 0)
                    return false;
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
        public static bool SetOptimalThreadAffinity(int threadIndex, int totalThreads)
        {
            try
            {
                int availableCores = Environment.ProcessorCount;
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
        public static bool ResetThreadAffinity()
        {
            try
            {
                if (!GetProcessAffinityMask(GetCurrentProcess(), out IntPtr processAffinityMask, out _))
                    return false;
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