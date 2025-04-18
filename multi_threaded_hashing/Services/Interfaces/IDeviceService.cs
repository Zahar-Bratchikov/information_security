using System.Collections.Generic;
using System.Threading.Tasks;
using multi_threaded_hashing.Models;

namespace multi_threaded_hashing.Services.Interfaces
{
    public interface IDeviceService
    {
        Task<IEnumerable<Device>> GetAvailableDevicesAsync();
        Task<string> GetProcessorInfoAsync();
        Task<int> GetProcessorCountAsync();
        Task<string> GetOsInfoAsync();
    }
}