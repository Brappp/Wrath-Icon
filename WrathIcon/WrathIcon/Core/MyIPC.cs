using System;
using Dalamud.Plugin;
using Dalamud.Plugin.Ipc;

namespace WrathIcon.Core
{
    public static class WrathLeaseManager
    {
        private static ICallGateSubscriber<Guid, object>? ReleaseControlSubscriber;

        public static void Init(IDalamudPluginInterface pluginInterface)
        {
            if (pluginInterface == null)
            {
                Plugin.PluginLog.Error("[WrathLeaseManager] PluginInterface is null! Cannot initialize IPC.");
                return;
            }

            try
            {
                // Get IPC method for releasing control
                ReleaseControlSubscriber = pluginInterface.GetIpcSubscriber<Guid, object>("WrathCombo.ReleaseControl");

                Plugin.PluginLog.Information("[WrathLeaseManager] Successfully initialized IPC lease manager.");
            }
            catch (Exception e)
            {
                Plugin.PluginLog.Error($"[WrathLeaseManager] Failed to initialize IPC lease manager: {e.Message}");
            }
        }

        public static void ReleaseLease(Guid lease)
        {
            if (ReleaseControlSubscriber == null)
            {
                Plugin.PluginLog.Warning("[WrathLeaseManager] Attempted to release lease, but IPC is not initialized.");
                return;
            }

            try
            {
                ReleaseControlSubscriber.InvokeAction(lease);
                Plugin.PluginLog.Information("[WrathLeaseManager] Released Wrath Auto-Rotation lease.");
            }
            catch (Exception e)
            {
                Plugin.PluginLog.Error($"[WrathLeaseManager] Failed to release lease: {e.Message}");
            }
        }
    }
}
