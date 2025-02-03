using System;
using System.Threading.Tasks;
using Dalamud.Plugin.Services;
using Dalamud.Game.Command;
using WrathIcon.Core;

namespace WrathIcon
{
    public static class WrathAutoManager
    {
        public static async void ToggleWrathAuto()
        {
            if (!WrathIPC.IsInitialized)
            {
                Plugin.PluginLog.Warning("[WrathAutoManager] Attempted toggle but IPC is not initialized.");
                return;
            }

            // Get current Wrath Auto-Rotation state
            bool isEnabled = WrathIPC.GetAutoRotationState();
            Plugin.PluginLog.Information($"Current Wrath Auto-Rotation state: {(isEnabled ? "Enabled" : "Disabled")}");

            // Use CommandManager to execute Wrath's chat command
            ExecuteChatCommand("/wrath auto");
            Plugin.PluginLog.Information("Executed chat command: /wrath auto");

            // Wait briefly before syncing state
            await Task.Delay(1000);

            // Sync UI state with Wrath's actual state
            bool newState = WrathIPC.GetAutoRotationState();
            Plugin.PluginLog.Information($"Wrath Auto-Rotation state synced to: {(newState ? "Enabled" : "Disabled")}");
        }

        private static void ExecuteChatCommand(string command)
        {
            if (Plugin.CommandManager != null)
            {
                Plugin.CommandManager.ProcessCommand(command);
                Plugin.PluginLog.Debug($"Sent chat command: {command}");
            }
            else
            {
                Plugin.PluginLog.Error("Failed to execute chat command. CommandManager is null.");
            }
        }
    }
}
