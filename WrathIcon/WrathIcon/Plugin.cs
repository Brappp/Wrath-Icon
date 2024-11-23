using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using System;
using System.Threading.Tasks;
using Dalamud.Plugin.Services;
using Dalamud.Game;
using Dalamud.Game.ClientState;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.IoC;
using WrathIcon.Core;
using WrathIcon.Utilities;

namespace WrathIcon
{
    public sealed class Plugin : IDalamudPlugin
    {
        private const string CommandName = "/wrathicon";
        public readonly WindowSystem WindowSystem = new("Wrath Status Icon");

        private MainWindow mainWindow;
        private ConfigWindow configWindow;
        private IWrathStateManager wrathStateManager;
        private Configuration config;
        private TextureManager textureManager;

        private bool isInitialized = false;

        [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
        [PluginService] internal static ITextureProvider TextureProvider { get; private set; } = null!;
        [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;
        [PluginService] internal static IPluginLog PluginLog { get; private set; } = null!;
        [PluginService] internal static IChatGui ChatGui { get; private set; } = null!;
        [PluginService] internal static IFramework Framework { get; private set; } = null!;
        [PluginService] internal static IClientState ClientState { get; private set; } = null!;

        public string Name => "Wrath Status Icon";

        public Plugin()
        {
            Framework.Update += OnFrameworkUpdate;

            ClientState.Login += OnLogin;
            ClientState.Logout += OnLogout;

            ClientState.TerritoryChanged += OnTerritoryChanged;
        }

        private void OnFrameworkUpdate(IFramework framework)
        {
            if (ClientState.IsLoggedIn && !isInitialized)
            {
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
            wrathStateManager = new WrathStateChecker(this);

            var iconOnUrl = "https://raw.githubusercontent.com/Brappp/Wrath_Auto_Tracker/main/WrathIcon/Data/icon-on.png";
            var iconOffUrl = "https://raw.githubusercontent.com/Brappp/Wrath_Auto_Tracker/main/WrathIcon/Data/icon-off.png";

            PluginLog.Information("[Debug] Plugin initializing...");

            mainWindow = new MainWindow(iconOnUrl, iconOffUrl, config, wrathStateManager, textureManager)
            {
                IsOpen = ClientState.IsLoggedIn 
            };
            configWindow = new ConfigWindow(config);

            RegisterWindow(mainWindow);
            RegisterWindow(configWindow);

            ChatGui.ChatMessage += HandleChatMessage;

            wrathStateManager.OnWrathStateChanged += state =>
            {
                PluginLog.Debug($"WrathStateChecker.OnWrathStateChanged triggered with state: {(state ? "Enabled" : "Disabled")}");
                mainWindow.UpdateWrathState(state);
            };

            CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
            {
                HelpMessage = "Toggle Wrath Status Icon UI"
            });

            PluginInterface.UiBuilder.Draw += DrawUI;
            PluginInterface.UiBuilder.OpenConfigUi += OpenConfigWindow;
            PluginInterface.UiBuilder.OpenMainUi += OpenMainWindow;

            InitializeWrathState();

            PluginLog.Information("Plugin initialized.");
        }

        private void OnTerritoryChanged(ushort territoryId)
        {
            PluginLog.Debug($"Territory changed to: {territoryId}");

            if (mainWindow != null && !mainWindow.IsOpen)
            {
                PluginLog.Debug("Reopening MainWindow due to territory change.");
                mainWindow.IsOpen = true;
            }
        }

        private void RegisterWindow(Window window)
        {
            PluginLog.Debug($"Registering window: {window.WindowName}");
            WindowSystem.AddWindow(window);
        }

        private void OnLogin()
        {
            PluginLog.Debug("Login detected.");

            if (mainWindow != null)
            {
                mainWindow.IsOpen = true;
                PluginLog.Debug("MainWindow shown due to login.");
            }
        }

        private void OnLogout(int type, int code)
        {
            PluginLog.Debug($"Logout detected. Type: {type}, Code: {code}");

            if (mainWindow != null)
            {
                mainWindow.IsOpen = false;
                PluginLog.Debug("MainWindow hidden due to logout.");
            }
        }

        private void OnCommand(string command, string args)
        {
            PluginLog.Debug("Command triggered to toggle ConfigWindow.");
            configWindow.Toggle();
        }

        private void HandleChatMessage(XivChatType type, int timestamp, ref SeString sender, ref SeString message, ref bool isHandled)
        {
            PluginLog.Debug($"Chat message received. Type: {type}, Message: {message.TextValue}");

            wrathStateManager.HandleChatMessage(message.TextValue);
        }

        private async void InitializeWrathState()
        {
            PluginLog.Debug("Initializing Wrath state with /wrath auto command.");

            CommandManager.ProcessCommand("/wrath auto");
            await Task.Delay(500);
            CommandManager.ProcessCommand("/wrath auto");

            PluginLog.Debug("Wrath state initialization complete.");
        }

        private void OpenConfigWindow()
        {
            if (!configWindow.IsOpen)
                configWindow.Toggle();
        }

        private void OpenMainWindow()
        {
            if (!mainWindow.IsOpen)
                mainWindow.Toggle();
        }

        private void DrawUI()
        {
            WindowSystem.Draw();
        }

        public void Dispose()
        {
            PluginInterface.UiBuilder.OpenConfigUi -= OpenConfigWindow;
            PluginInterface.UiBuilder.OpenMainUi -= OpenMainWindow;

            ChatGui.ChatMessage -= HandleChatMessage;

            ClientState.Login -= OnLogin;
            ClientState.Logout -= OnLogout;
            ClientState.TerritoryChanged -= OnTerritoryChanged;

            CommandManager.RemoveHandler(CommandName);
            PluginInterface.UiBuilder.Draw -= DrawUI;
            WindowSystem.RemoveAllWindows();
        }
    }
}
