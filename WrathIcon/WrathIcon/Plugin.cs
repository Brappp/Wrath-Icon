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
                PluginLog.Debug("Logged in at plugin load, initializing.");
                Initialize();
                isInitialized = true;

                if (mainWindow != null)
                {
                    mainWindow.SetOpen(true); // Ensure UI is visible
                }
            }
            else
            {
                PluginLog.Debug("Waiting for login...");
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
                mainWindow = new MainWindow(config, textureManager);
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

            PluginLog.Information("WrathIcon Plugin fully initialized.");
        }

        private void OpenConfigWindow()
        {
            if (configWindow != null)
                configWindow.IsOpen = true;
        }

        private void OpenMainWindow()
        {
            if (mainWindow != null)
            {
                mainWindow.SetOpen(true);
            }
        }

        private void OnLogin()
        {
            if (!isInitialized)
            {
                PluginLog.Information("Login detected, initializing plugin.");
                Initialize();
                isInitialized = true;
            }

            if (mainWindow != null)
            {
                mainWindow.SetOpen(true);
            }
        }

        private void OnLogout(int type, int code)
        {
            PluginLog.Information($"Logout detected. Type: {type}, Code: {code}");

            if (mainWindow != null)
            {
                mainWindow.SetOpen(false);
            }

            isInitialized = false;
        }

        private void OnCommand(string command, string args)
        {
            if (mainWindow != null)
            {
                bool newState = !mainWindow.IsOpen;
                mainWindow.SetOpen(newState);
            }
        }

        private void DrawUI()
        {
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
