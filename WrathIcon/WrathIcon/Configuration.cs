using Dalamud.Configuration;
using Dalamud.Plugin;
using System.Numerics;

public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 1;
    public bool IsLocked { get; set; } = false;
    public int SelectedImageSize { get; set; } = 64; 
    public float WindowX { get; set; } = 100.0f; 
    public float WindowY { get; set; } = 100.0f; 

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
