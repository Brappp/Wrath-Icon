using Dalamud.Game.ClientState;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using WrathIcon.Core;
using WrathIcon.Utilities;
using Dalamud.IoC;
using Dalamud.Plugin.Services;
using Dalamud.Game.Command;

namespace WrathIcon
{
    public sealed class Plugin : IDalamudPlugin
    {
        private const string CommandName = "/wrathicon";
        public readonly WindowSystem WindowSystem = new("Wrath Status Icon");

        private MainWindow mainWindow;
        private ConfigWindow configWindow;
        private Configuration config;
        private TextureManager textureManager;

        [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
        [PluginService] internal static ITextureProvider TextureProvider { get; private set; } = null!;
        [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;
        [PluginService] internal static IPluginLog PluginLog { get; private set; } = null!;
        [PluginService] internal static IClientState ClientState { get; private set; } = null!;

        public string Name => "Wrath Status Icon";

        public Plugin()
        {
            PluginLog.Information("Initializing WrathIcon Plugin...");

            WrathIPC.Init(PluginInterface);

            if (WrathIPC.IsInitialized)
                PluginLog.Information("WrathIPC initialized successfully.");
            else
                PluginLog.Error("WrathIPC failed to initialize.");

            Initialize();

            ClientState.Login += OnLogin;
            ClientState.Logout += OnLogout;
        }

        private void Initialize()
        {
            config = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            config.Initialize(PluginInterface);

            textureManager = new TextureManager(TextureProvider);
            mainWindow = new MainWindow(config, textureManager);
            configWindow = new ConfigWindow(config);

            WindowSystem.AddWindow(mainWindow);
            WindowSystem.AddWindow(configWindow);

            CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
            {
                HelpMessage = "Toggle Wrath Status Icon UI"
            });

            PluginInterface.UiBuilder.Draw += DrawUI;
            PluginInterface.UiBuilder.OpenMainUi += OpenMainWindow;
            PluginInterface.UiBuilder.OpenConfigUi += OpenConfigWindow;

            PluginLog.Information("Plugin initialized.");
        }

        private void OnLogin()
        {
            PluginLog.Information("Player logged in - Opening MainWindow.");
            mainWindow.IsOpen = true;
        }

        private void OnLogout(int type, int code)
        {
            PluginLog.Information("Player logged out - Closing MainWindow.");
            mainWindow.IsOpen = false;
        }

        private void OnCommand(string command, string args)
        {
            mainWindow.IsOpen = !mainWindow.IsOpen;
        }

        private void OpenMainWindow()
        {
            mainWindow.IsOpen = true;
        }

        private void OpenConfigWindow()
        {
            configWindow.IsOpen = true;
        }

        private void DrawUI()
        {
            WindowSystem.Draw();
        }

        public void Dispose()
        {
            ClientState.Login -= OnLogin;
            ClientState.Logout -= OnLogout;

            CommandManager.RemoveHandler(CommandName);
            PluginInterface.UiBuilder.Draw -= DrawUI;
            PluginInterface.UiBuilder.OpenMainUi -= OpenMainWindow;
            PluginInterface.UiBuilder.OpenConfigUi -= OpenConfigWindow;

            WindowSystem.RemoveAllWindows();
        }
    }
}
