using System;
using System.Threading;
using System.Threading.Tasks;
using BruteForceAnalyzer.Models;

namespace BruteForceAnalyzer.Services.Interfaces
{
    public interface IBruteForceService
    {
        event EventHandler<BruteForceProgressEventArgs>? ProgressChanged;

        Task<string> StartBruteForceAsync(BruteForceSettings settings, CancellationToken cancellationToken);

        void StopBruteForce();
    }
} 