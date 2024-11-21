using Dalamud.Configuration;
using Dalamud.Plugin;
using System.Numerics;

public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 1;

    public bool IsLocked { get; set; } = false;

    // Add a property to store the selected size
    public int SelectedImageSize { get; set; } = 64; // Default size: 64x64

    private IDalamudPluginInterface pluginInterface;

    public void Initialize(IDalamudPluginInterface pluginInterface)
    {
        this.pluginInterface = pluginInterface;
    }

    public void Save()
    {
        pluginInterface.SavePluginConfig(this);
    }
}
