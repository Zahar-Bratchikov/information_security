using System.Collections.Generic;
using System.Threading.Tasks;
using BruteForceAnalyzer.Models;

namespace BruteForceAnalyzer.Services.Interfaces
{
    public interface IDeviceService
    {
        Task<IEnumerable<Device>> GetAvailableDevicesAsync();
        Task<Device> GetDeviceInfoAsync(string deviceId);
    }
} 