using System;
using Dalamud.Configuration;
using Dalamud.Plugin;
using Newtonsoft.Json;
using WrathIcon.Utilities;

namespace WrathIcon
{
    [Serializable]
    public class Configuration : IPluginConfiguration
    {
        public int Version { get; set; } = 1;
        
        [JsonProperty] public bool IsLocked { get; set; } = false;
        [JsonProperty] public int SelectedImageSize { get; set; } = Constants.DefaultIconSize;
        [JsonProperty] public float WindowX { get; set; } = Constants.DefaultWindowX;
        [JsonProperty] public float WindowY { get; set; } = Constants.DefaultWindowY;
        public bool AutoShowOnLogin { get; set; } = true;

        [NonSerialized]
        private IDalamudPluginInterface? pluginInterface;

        // Events for configuration changes
        public event Action<Configuration>? ConfigurationChanged;

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
            ConfigurationChanged?.Invoke(this);
        }

        // Helper methods for common operations
        public void SetWindowPosition(float x, float y)
        {
            WindowX = x;
            WindowY = y;
            Save();
        }

        public void SetImageSize(int size)
        {
            SelectedImageSize = Math.Max(Constants.MinIconSize, Math.Min(Constants.MaxIconSize, size));
            Save();
        }

        public void SetLocked(bool locked)
        {
            IsLocked = locked;
            Save();
        }
    }
}
