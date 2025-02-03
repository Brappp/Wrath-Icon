using System;

namespace WrathIcon.Core
{
    public interface IWrathStateManager
    {
        event Action<bool> OnWrathStateChanged;
        void RefreshWrathState();
        bool IsWrathEnabled();
    }
}
