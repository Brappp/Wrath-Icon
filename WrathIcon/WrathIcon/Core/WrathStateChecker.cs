using System;
using WrathIcon.Core;

namespace WrathIcon
{
    public class WrathStateChecker : IWrathStateManager
    {
        private readonly Plugin plugin; // Reference to the plugin
        private bool isWrathEnabled = false; // Current Wrath state

        public event Action<bool>? OnWrathStateChanged;

        public WrathStateChecker(Plugin plugin)
        {
            this.plugin = plugin;
        }

        public void HandleChatMessage(string message)
        {
            Plugin.PluginLog.Debug($"WrathStateChecker.HandleChatMessage triggered with message: {message}");

            if (message.Contains("Auto-Rotation set to ON"))
            {
                SetWrathState(true);
            }
            else if (message.Contains("Auto-Rotation set to OFF"))
            {
                SetWrathState(false);
            }
        }

        public bool IsWrathEnabled() => isWrathEnabled;

        private void SetWrathState(bool isEnabled)
        {
            if (isWrathEnabled != isEnabled)
            {
                isWrathEnabled = isEnabled;

                Plugin.PluginLog.Debug($"Wrath state updated internally to: {(isEnabled ? "Enabled" : "Disabled")}");

                OnWrathStateChanged?.Invoke(isWrathEnabled);
            }
        }
    }
}
