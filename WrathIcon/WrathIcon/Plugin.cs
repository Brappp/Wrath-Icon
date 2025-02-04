using Dalamud.Game.ClientState;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using WrathIcon.Core;
using WrathIcon.Utilities;
using Dalamud.IoC;
using Dalamud.Plugin.Services;
using Dalamud.Game.Command;
using Dalamud.Game;
using System.Linq;

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

        private bool isInitialized = false;

        [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
        [PluginService] internal static ITextureProvider TextureProvider { get; private set; } = null!;
        [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;
        [PluginService] internal static IPluginLog PluginLog { get; private set; } = null!;
        [PluginService] internal static IClientState ClientState { get; private set; } = null!;
        [PluginService] internal static IFramework Framework { get; private set; } = null!;

        public string Name => "Wrath Status Icon";

        public Plugin()
        {
            PluginLog.Information("Initializing WrathIcon Plugin...");

            WrathIPC.Init(PluginInterface);
            if (WrathIPC.IsInitialized)
                PluginLog.Information("WrathIPC initialized successfully.");
            else
                PluginLog.Error("WrathIPC failed to initialize.");

            if (ClientState.IsLoggedIn)
            {
                PluginLog.Debug("Already logged in at plugin load, initializing.");
                Initialize();
                isInitialized = true;

                if (mainWindow != null)
                {
                    mainWindow.IsOpen = true;
                    PluginLog.Debug("Main window opened at plugin load.");
                }
            }
            else
            {
                PluginLog.Debug("Not logged in at plugin load, waiting for login.");
                Framework.Update += OnFrameworkUpdate;
            }

            ClientState.Login += OnLogin;
            ClientState.Logout += OnLogout;
            PluginInterface.UiBuilder.OpenConfigUi += OpenConfigWindow;
        }

        private void OnFrameworkUpdate(IFramework framework)
        {
            if (ClientState.IsLoggedIn && !isInitialized)
            {
                PluginLog.Debug("Login detected via framework update, initializing.");
                Initialize();
                isInitialized = true;
                Framework.Update -= OnFrameworkUpdate;
            }
        }

        private void Initialize()
        {
            config = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            config.Initialize(PluginInterface);

            textureManager = new TextureManager(TextureProvider);

            if (mainWindow == null)
            {
                mainWindow = new MainWindow(config, textureManager) { IsOpen = false };
                WindowSystem.AddWindow(mainWindow);
            }

            if (configWindow == null)
            {
                configWindow = new ConfigWindow(config);
                WindowSystem.AddWindow(configWindow);
            }

            CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
            {
                HelpMessage = "Toggle Wrath Status Icon UI"
            });

            PluginInterface.UiBuilder.Draw += DrawUI;
            PluginInterface.UiBuilder.OpenMainUi += OpenMainWindow;

            PluginLog.Information("WrathIcon Plugin initialized.");
        }

        private void OpenConfigWindow()
        {
            if (configWindow != null)
            {
                configWindow.IsOpen = true;
                PluginLog.Debug("Config window opened.");
            }
        }

        private void OpenMainWindow()
        {
            if (mainWindow != null)
            {
                mainWindow.IsOpen = true;
                PluginLog.Debug("Main window opened.");
            }
        }

        private void OnLogin()
        {
            PluginLog.Debug("Login detected.");

            if (!isInitialized)
            {
                Initialize();
                isInitialized = true;
            }

            if (mainWindow != null)
            {
                mainWindow.IsOpen = true;
                PluginLog.Debug("Main window opened on login.");
            }
            else
            {
                PluginLog.Error("Main window is null on login.");
            }
        }

        private void OnLogout(int type, int code)
        {
            PluginLog.Debug($"Logout detected. Type: {type}, Code: {code}");

            if (mainWindow != null)
            {
                mainWindow.IsOpen = false;
                PluginLog.Debug("Main window closed due to logout.");
            }
        }

        private void OnCommand(string command, string args)
        {
            if (mainWindow != null)
            {
                mainWindow.IsOpen = !mainWindow.IsOpen;
            }
        }

        private void DrawUI()
        {
            PluginLog.Debug("Drawing WrathIcon UI...");
            WindowSystem.Draw();
        }

        public void Dispose()
        {
            PluginInterface.UiBuilder.Draw -= DrawUI;
            PluginInterface.UiBuilder.OpenMainUi -= OpenMainWindow;
            PluginInterface.UiBuilder.OpenConfigUi -= OpenConfigWindow;

            CommandManager.RemoveHandler(CommandName);

            ClientState.Login -= OnLogin;
            ClientState.Logout -= OnLogout;
            Framework.Update -= OnFrameworkUpdate;

            WindowSystem.RemoveAllWindows();

            PluginLog.Information("WrathIcon Plugin disposed.");
        }
    }
}
