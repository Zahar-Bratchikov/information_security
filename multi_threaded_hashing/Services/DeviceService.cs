using multi_threaded_hashing.Models;
using multi_threaded_hashing.Services.Interfaces;
using System.Runtime.InteropServices;

namespace multi_threaded_hashing.Services
{
    public class DeviceService : IDeviceService
    {
        private readonly Interfaces.ILogger _logger;
        private readonly List<Device> _devices;

        public DeviceService(Interfaces.ILogger logger)
        {
            _logger = logger;
            _devices = new List<Device>
            {
                new Device
                {
                    Id = "1",
                    Name = "CPU",
                    Description = "Основной процессор",
                    IsAvailable = true,
                    PerformanceScore = 100
                },
                new Device
                {
                    Id = "2",
                    Name = "GPU",
                    Description = "Графический процессор",
                    IsAvailable = true,
                    PerformanceScore = 200
                }
            };
        }

        public async Task<IEnumerable<Device>> GetAvailableDevicesAsync()
        {
            try
            {
                var processorCount = Environment.ProcessorCount;

                // Обновляем информацию о процессоре
                var cpuDevice = _devices.FirstOrDefault(d => d.Name == "CPU");
                if (cpuDevice != null)
                {
                    cpuDevice.Description = $"Процессор ({processorCount} ядер)";
                    cpuDevice.PerformanceScore = processorCount * 100;
                }

                return await Task.FromResult(_devices.FindAll(d => d.IsAvailable));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка при получении устройств: {ex.Message}");
                return await Task.FromResult(Enumerable.Empty<Device>());
            }
        }

        public async Task<string> GetProcessorInfoAsync()
        {
            try
            {
                string processorInfo = Environment.GetEnvironmentVariable("PROCESSOR_IDENTIFIER")
                    ?? "Информация о процессоре недоступна";
                return await Task.FromResult(processorInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка при получении информации о процессоре: {ex.Message}");
                return await Task.FromResult("Ошибка при получении информации о процессоре");
            }
        }

        public async Task<int> GetProcessorCountAsync()
        {
            try
            {
                return await Task.FromResult(Environment.ProcessorCount);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка при получении количества ядер процессора: {ex.Message}");
                return await Task.FromResult(0);
            }
        }

        public async Task<string> GetOsInfoAsync()
        {
            try
            {
                string osDescription = RuntimeInformation.OSDescription;
                string osArchitecture = RuntimeInformation.OSArchitecture.ToString();
                string frameworkDescription = RuntimeInformation.FrameworkDescription;

                string osInfo = $"{osDescription} ({osArchitecture}), {frameworkDescription}";
                return await Task.FromResult(osInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка при получении информации об ОС: {ex.Message}");
                return await Task.FromResult("Ошибка при получении информации об ОС");
            }
        }
    }
}