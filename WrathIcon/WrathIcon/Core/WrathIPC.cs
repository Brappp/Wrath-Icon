using System;
using Dalamud.Plugin;
using Dalamud.Plugin.Ipc;
using Dalamud.IoC;
using WrathIcon.Utilities;

namespace WrathIcon.Core
{
    public static class WrathIPC
    {
        [PluginService] private static IDalamudPluginInterface PluginInterface { get; set; } = null!;
        private static ICallGateSubscriber<bool>? GetAutoRotationStateSubscriber;

        public static bool IsInitialized { get; private set; } = false;
        private static bool? lastLoggedState = null; // Prevents unnecessary spam logging

        public static void Init(IDalamudPluginInterface pluginInterface)
        {
            if (pluginInterface == null)
            {
                Logger.Error("PluginInterface is null. IPC cannot be initialized.");
                return;
            }

            try
            {
                PluginInterface = pluginInterface;

                GetAutoRotationStateSubscriber = PluginInterface.GetIpcSubscriber<bool>(Constants.IpcGetAutoRotationState);

                IsInitialized = true;
                Logger.Info("IPC successfully initialized");
            }
            catch (Exception e)
            {
                Logger.Error("Failed to initialize IPC", e);
            }
        }

        public static bool GetAutoRotationState()
        {
            if (!IsInitialized || GetAutoRotationStateSubscriber == null)
            {
                Logger.Warning("GetAutoRotationState attempted but IPC is not initialized");
                return false;
            }

            try
            {
                bool state = GetAutoRotationStateSubscriber.InvokeFunc();

                // Only log if the state changes
                if (state != lastLoggedState)
                {
                    Logger.Debug($"Auto-Rotation state: {(state ? "Enabled" : "Disabled")}");
                    lastLoggedState = state;
                }

                return state;
            }
            catch (Exception e)
            {
                Logger.Error("Error retrieving Auto-Rotation state", e);
                return false;
            }
        }
    }
}
