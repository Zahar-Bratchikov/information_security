using multi_threaded_hashing.Models;

namespace multi_threaded_hashing.Services.Interfaces
{
    public interface IBruteForceService
    {
        event EventHandler<BruteForceProgressEventArgs>? ProgressChanged;

        Task<string> StartBruteForceAsync(BruteForceSettings settings, CancellationToken cancellationToken);

        void StopBruteForce();
    }
}