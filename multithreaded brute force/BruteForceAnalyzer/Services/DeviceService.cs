using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BruteForceAnalyzer.Models;
using BruteForceAnalyzer.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace BruteForceAnalyzer.Services
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

        public List<Device> GetAvailableDevices()
        {
            try
            {
                var devices = new List<Device>();
                var processorCount = Environment.ProcessorCount;

                // Добавляем CPU как устройство
                devices.Add(new Device
                {
                    Id = "CPU",
                    Name = "CPU",
                    Description = $"Процессор ({processorCount} ядер)",
                    IsAvailable = true,
                    PerformanceScore = processorCount * 1000 // Простая оценка производительности
                });

                return devices;
            }
            catch (Exception ex)
            {
                _logger.LogError("Ошибка при получении CPU устройств", ex);
                return new List<Device>();
            }
        }

        public async Task<IEnumerable<Device>> GetAvailableDevicesAsync()
        {
            return await Task.FromResult(_devices.FindAll(d => d.IsAvailable));
        }

        public async Task<Device> GetDeviceInfoAsync(string deviceId)
        {
            var device = _devices.Find(d => d.Id == deviceId);
            if (device == null)
            {
                throw new ArgumentException($"Устройство с ID {deviceId} не найдено");
            }
            return await Task.FromResult(device);
        }
    }
} 