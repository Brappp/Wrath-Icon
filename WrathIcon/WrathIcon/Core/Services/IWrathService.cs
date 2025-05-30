using System;
using System.Threading.Tasks;

namespace WrathIcon.Core.Services
{
    public interface IWrathService : IDisposable
    {
        bool IsAutoRotationEnabled { get; }
        bool IsInitialized { get; }
        
        event Action<bool>? StateChanged;
        
        Task<bool> ToggleAutoRotationAsync();
        
        void StartMonitoring();
        void StopMonitoring();
    }
} 