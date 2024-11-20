using Dalamud.Interface.Textures;
using Dalamud.Interface.Textures.TextureWraps;
using Dalamud.Interface.Windowing;
using ECommons.ImGuiMethods;
using ImGuiNET;
using System;
using System.IO;

namespace SamplePlugin;

public class MainWindow : Window, IDisposable
{
    private readonly string iconOnPath;
    private readonly string iconOffPath;

    private IDalamudTextureWrap? iconOnTexture;
    private IDalamudTextureWrap? iconOffTexture;

    private bool wrathState;

    public MainWindow() : base("Wrath Status Icon")
    {
        try
        {
            // Attempt to determine the base directory
            var basePath = AppContext.BaseDirectory;
            if (string.IsNullOrEmpty(basePath))
            {
                // Fallback for debugging or unexpected runtime issues
                basePath = "E:\\Github\\Wrath_Auto_Tracker\\SamplePlugin\\SamplePlugin\\bin\\x64\\Debug";
            }

            // Define paths to resources
            var resourcesDirectory = Path.Combine(basePath, "Resources");
            iconOnPath = Path.Combine(resourcesDirectory, "icon-on.png");
            iconOffPath = Path.Combine(resourcesDirectory, "icon-off.png");

            // Log resolved paths for debugging
            ECommons.Logging.PluginLog.Information($"Base Path Resolved: {basePath}");
            ECommons.Logging.PluginLog.Information($"Resources Directory: {resourcesDirectory}");
            ECommons.Logging.PluginLog.Information($"ON Icon Path: {iconOnPath}");
            ECommons.Logging.PluginLog.Information($"OFF Icon Path: {iconOffPath}");

            // Attempt to load icons
            LoadIcons();
        }
        catch (Exception ex)
        {
            // Log detailed error information
            ECommons.Logging.PluginLog.Error($"Initialization failed. BasePath: {AppContext.BaseDirectory}");
            ECommons.Logging.PluginLog.Error($"Exception: {ex}");
            throw;
        }
    }


    private void LoadIcons()
    {
        if (!ThreadLoadImageHandler.TryGetTextureWrap(iconOnPath, out iconOnTexture))
        {
            ECommons.Logging.PluginLog.Warning($"Failed to load ON icon. Path attempted: {iconOnPath}");
        }
        else
        {
            ECommons.Logging.PluginLog.Information($"Successfully loaded ON icon from: {iconOnPath}");
        }

        if (!ThreadLoadImageHandler.TryGetTextureWrap(iconOffPath, out iconOffTexture))
        {
            ECommons.Logging.PluginLog.Warning($"Failed to load OFF icon. Path attempted: {iconOffPath}");
        }
        else
        {
            ECommons.Logging.PluginLog.Information($"Successfully loaded OFF icon from: {iconOffPath}");
        }
    }


    public override void Draw()
    {
        ImGui.Text("Wrath Auto-Rotation Status:");

        // Display the appropriate icon based on the Wrath state
        var currentIcon = wrathState ? iconOnTexture : iconOffTexture;

        if (currentIcon != null)
        {
            ImGui.Image(currentIcon.ImGuiHandle, new System.Numerics.Vector2(64, 64));
        }
        else
        {
            ImGui.TextColored(new System.Numerics.Vector4(1, 0, 0, 1), "Icon not loaded.");
        }

        // Toggle Wrath state (for testing)
        if (ImGui.Button("Toggle Wrath State"))
        {
            wrathState = !wrathState;
        }
    }

    public void Dispose()
    {
        // Dispose textures when done
        iconOnTexture?.Dispose();
        iconOffTexture?.Dispose();
    }
}
