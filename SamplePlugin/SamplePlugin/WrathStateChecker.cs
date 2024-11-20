using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using System;

namespace SamplePlugin
{
    public class WrathStateChecker
    {
        private readonly Plugin plugin; // Reference to the plugin
        private bool isWrathEnabled = false; // Current Wrath state

        public event Action<bool>? OnWrathStateChanged;

        public WrathStateChecker(Plugin plugin)
        {
            this.plugin = plugin;
        }

        /// <summary>
        /// Handles chat messages to toggle Wrath state.
        /// </summary>
        public void ChatMessageHandler(XivChatType type, int a2, ref SeString sender, ref SeString message, ref bool isHandled)
        {
            Plugin.PluginLog.Debug($"ChatMessageHandler triggered with type: {type}, message: {message.TextValue}");

            if (message.TextValue.Contains("Auto-Rotation set to ON"))
            {
                SetWrathState(true);
            }
            else if (message.TextValue.Contains("Auto-Rotation set to OFF"))
            {
                SetWrathState(false);
            }
        }

        /// <summary>
        /// Sets the Wrath state and triggers an event.
        /// </summary>
        private void SetWrathState(bool isEnabled)
        {
            if (isWrathEnabled != isEnabled)
            {
                isWrathEnabled = isEnabled;

                Plugin.PluginLog.Debug($"Wrath state updated internally to: {(isEnabled ? "Enabled" : "Disabled")}");

                OnWrathStateChanged?.Invoke(isWrathEnabled);
            }
        }

        public bool IsWrathEnabled() => isWrathEnabled;
    }
}
