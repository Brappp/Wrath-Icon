using System;
using System.Threading.Tasks;

namespace WrathIcon.Core.Services
{
    public interface IWrathService : IDisposable
    {
        bool IsAutoRotationEnabled { get; }
        bool? IsBurstHeld { get; }
        bool IsInitialized { get; }

        event Action<bool>? StateChanged;
        event Action<bool?>? BurstStateChanged;

        Task<bool> ToggleAutoRotationAsync();
        void ToggleBurst();

        void StartMonitoring();
        void StopMonitoring();
    }
} 