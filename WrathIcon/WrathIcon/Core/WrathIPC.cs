using System;
using Dalamud.Plugin;
using Dalamud.Plugin.Ipc;
using Dalamud.IoC;

namespace WrathIcon.Core
{
    public static class WrathIPC
    {
        [PluginService] private static IDalamudPluginInterface PluginInterface { get; set; } = null!;
        private static ICallGateSubscriber<bool>? GetAutoRotationStateSubscriber;
        private static ICallGateSubscriber<Guid, bool, object>? SetAutoRotationStateSubscriber;
        private static ICallGateSubscriber<string, string, Guid?>? RegisterForLeaseSubscriber;
        private static ICallGateSubscriber<Guid, object>? ReleaseControlSubscriber;

        public static bool IsInitialized { get; private set; } = false;
        public static Guid? CurrentLease { get; private set; }
        private static bool? lastLoggedState = null; // Prevents unnecessary spam logging

        public static void Init(IDalamudPluginInterface pluginInterface)
        {
            if (pluginInterface == null)
            {
                Plugin.PluginLog.Error("[WrathIPC] PluginInterface is null. IPC cannot be initialized.");
                return;
            }

            try
            {
                PluginInterface = pluginInterface;

                GetAutoRotationStateSubscriber = PluginInterface.GetIpcSubscriber<bool>("WrathCombo.GetAutoRotationState");
                SetAutoRotationStateSubscriber = PluginInterface.GetIpcSubscriber<Guid, bool, object>("WrathCombo.SetAutoRotationState");
                RegisterForLeaseSubscriber = PluginInterface.GetIpcSubscriber<string, string, Guid?>("WrathCombo.RegisterForLease");
                ReleaseControlSubscriber = PluginInterface.GetIpcSubscriber<Guid, object>("WrathCombo.ReleaseControl");

                IsInitialized = true;
                Plugin.PluginLog.Information("[WrathIPC] IPC successfully initialized.");
            }
            catch (Exception e)
            {
                Plugin.PluginLog.Error($"[WrathIPC] Failed to initialize IPC: {e.Message}");
            }
        }

        public static bool GetAutoRotationState()
        {
            if (!IsInitialized || GetAutoRotationStateSubscriber == null)
            {
                Plugin.PluginLog.Warning("[WrathIPC] GetAutoRotationState attempted but IPC is not initialized.");
                return false;
            }

            try
            {
                bool state = GetAutoRotationStateSubscriber.InvokeFunc();

                // Only log if the state changes
                if (state != lastLoggedState)
                {
                    Plugin.PluginLog.Information($"[WrathIPC] Auto-Rotation state changed to: {(state ? "Enabled" : "Disabled")}");
                    lastLoggedState = state;
                }

                return state;
            }
            catch (Exception e)
            {
                Plugin.PluginLog.Error($"[WrathIPC] Error retrieving Auto-Rotation state: {e.Message}");
                return false;
            }
        }

        public static void SetAutoRotationState(Guid lease, bool enabled)
        {
            if (!IsInitialized || SetAutoRotationStateSubscriber == null)
            {
                Plugin.PluginLog.Warning("[WrathIPC] Attempted to set Auto-Rotation state but IPC is not initialized.");
                return;
            }

            try
            {
                SetAutoRotationStateSubscriber.InvokeAction(lease, enabled);
                Plugin.PluginLog.Information($"[WrathIPC] Sent IPC command to {(enabled ? "enable" : "disable")} Wrath Auto-Rotation.");
            }
            catch (Exception e)
            {
                Plugin.PluginLog.Error($"[WrathIPC] Error setting Auto-Rotation state: {e.Message}");
            }
        }

        public static void ReleaseControl(Guid lease)
        {
            if (!IsInitialized || ReleaseControlSubscriber == null)
                return;

            try
            {
                ReleaseControlSubscriber.InvokeAction(lease);
                Plugin.PluginLog.Information($"[WrathIPC] Released control of Wrath Auto-Rotation for lease {lease}.");
                CurrentLease = null;
            }
            catch (Exception e)
            {
                Plugin.PluginLog.Error($"[WrathIPC] Failed to release Wrath control: {e.Message}");
            }
        }
    }
}
