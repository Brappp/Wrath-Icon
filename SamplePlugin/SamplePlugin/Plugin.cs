using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using System.Threading.Tasks;
using ECommons.Logging;
using Dalamud.Plugin.Services;
using ECommons;

namespace SamplePlugin;

public sealed class Plugin : IDalamudPlugin
{
    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;
    [PluginService] internal static Dalamud.Plugin.Services.IChatGui ChatGui { get; private set; } = null!;

    private const string CommandName = "/wrathicon";
    private readonly WindowSystem windowSystem;
    private readonly MainWindow mainWindow;

    public string Name => "Wrath Status Icon";

    public Plugin()
    {
        // Initialize ECommons
        ECommonsMain.Init(PluginInterface, this);

        // Initialize MainWindow and add it to the WindowSystem
        mainWindow = new MainWindow();
        windowSystem = new WindowSystem(Name);
        windowSystem.AddWindow(mainWindow);

        // Register the slash command
        CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "Toggle Wrath Status Icon UI"
        });

        // Hook into Dalamud's UI drawing
        PluginInterface.UiBuilder.Draw += DrawUI;

        // Log startup
        PluginLog.Information("Plugin initialized.");

        // Sync with Wrath at startup
        SyncWithWrath();
    }

    private void OnCommand(string command, string args)
    {
        // Toggle the display of the main UI
        mainWindow.IsOpen = !mainWindow.IsOpen;
    }

    private void DrawUI()
    {
        windowSystem.Draw();
    }

    private async void SyncWithWrath()
    {
        // Send /wrath auto twice to sync Wrath's Auto-Rotation state
        CommandManager.ProcessCommand("/wrath auto");
        await Task.Delay(500); // Add a small delay between commands
        CommandManager.ProcessCommand("/wrath auto");

        PluginLog.Information("Sent /wrath auto twice to sync with Wrath.");
    }

    public void Dispose()
    {
        // Dispose ECommons
        ECommonsMain.Dispose();

        // Clean up resources
        CommandManager.RemoveHandler(CommandName);
        PluginInterface.UiBuilder.Draw -= DrawUI;
        windowSystem.RemoveAllWindows();
    }
}
