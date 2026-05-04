using System;
using Dalamud.Configuration;
using Dalamud.Plugin;
using WrathIcon.Utilities;

namespace WrathIcon
{
    [Serializable]
    public class Configuration : IPluginConfiguration
    {
        public int Version { get; set; } = 1;

        public bool IsLocked { get; set; } = false;
        public int SelectedImageSize { get; set; } = Constants.DefaultIconSize;
        public float WindowX { get; set; } = Constants.DefaultWindowX;
        public float WindowY { get; set; } = Constants.DefaultWindowY;
        public bool AutoShowOnLogin { get; set; } = true;
        public bool ShowTooltips { get; set; } = true;
        public bool ShowBurstButton { get; set; } = false;

        [NonSerialized]
        private IDalamudPluginInterface? pluginInterface;

        public void Initialize(IDalamudPluginInterface pluginInterface)
        {
            this.pluginInterface = pluginInterface ?? throw new ArgumentNullException(nameof(pluginInterface));
        }

        public void Save()
        {
            if (pluginInterface == null)
            {
                throw new InvalidOperationException("Configuration not initialized. Call Initialize() first.");
            }

            pluginInterface.SavePluginConfig(this);
        }

        public void SetWindowPosition(float x, float y)
        {
            WindowX = x;
            WindowY = y;
            Save();
        }

        public void SetImageSize(int size)
        {
            SelectedImageSize = Math.Clamp(size, Constants.MinIconSize, Constants.MaxIconSize);
            Save();
        }

        public void SetLocked(bool locked)
        {
            IsLocked = locked;
            Save();
        }

        public void SetAutoShowOnLogin(bool value)
        {
            AutoShowOnLogin = value;
            Save();
        }

        public void SetShowTooltips(bool value)
        {
            ShowTooltips = value;
            Save();
        }

        public void SetShowBurstButton(bool value)
        {
            ShowBurstButton = value;
            Save();
        }
    }
}
