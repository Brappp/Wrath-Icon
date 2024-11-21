using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Dalamud.Plugin.Services;
using Dalamud.Game;
using Dalamud.Game.ClientState;
using Dalamud.Game.Text;
using Dalamud.Interface.Textures.TextureWraps;
using Dalamud.IoC;

namespace WrathIcon
{
    public sealed class Plugin : IDalamudPlugin
    {
        private const string CommandName = "/wrathicon";
        public readonly WindowSystem WindowSystem = new("Wrath Status Icon");

        private MainWindow mainWindow;
        private ConfigWindow configWindow;
        private WrathStateChecker wrathStateChecker;
        private Configuration config;

        private bool isInitialized = false;

        [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
        [PluginService] internal static ITextureProvider TextureProvider { get; private set; } = null!;
        [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;
        [PluginService] internal static IPluginLog PluginLog { get; private set; } = null!;
        [PluginService] internal static IChatGui ChatGui { get; private set; } = null!;
        [PluginService] internal static IFramework Framework { get; private set; } = null!;
        [PluginService] internal static IClientState ClientState { get; private set; } = null!;

        private static readonly ConcurrentDictionary<string, IDalamudTextureWrap?> TextureCache = new();
        private static HttpClient httpClient = new();

        public string Name => "Wrath Status Icon";

        public Plugin()
        {
            // Subscribe to the Framework Update event to delay initialization
            Framework.Update += OnFrameworkUpdate;

            // Subscribe to login and logout events
            ClientState.Login += OnLogin;
            ClientState.Logout += OnLogout;
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

            var iconOnUrl = "https://raw.githubusercontent.com/Brappp/Wrath_Auto_Tracker/main/WrathIcon/Data/icon-on.png";
            var iconOffUrl = "https://raw.githubusercontent.com/Brappp/Wrath_Auto_Tracker/main/WrathIcon/Data/icon-off.png";

            PluginLog.Information("[Debug] Plugin initializing...");

            mainWindow = new MainWindow(iconOnUrl, iconOffUrl, config, this)
            {
                IsOpen = ClientState.IsLoggedIn // Show only if logged in
            };
            configWindow = new ConfigWindow(config);

            RegisterWindow(mainWindow);
            RegisterWindow(configWindow);

            wrathStateChecker = new WrathStateChecker(this);
            ChatGui.ChatMessage += wrathStateChecker.ChatMessageHandler;

            wrathStateChecker.OnWrathStateChanged += state =>
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

        private void RegisterWindow(Window window)
        {
            PluginLog.Debug($"Registering window: {window.WindowName}");
            WindowSystem.AddWindow(window);
        }

        private void OnLogin()
        {
            PluginLog.Debug("Login detected.");

            // Ensure the main window is shown upon login
            if (mainWindow != null)
            {
                mainWindow.IsOpen = true;
                PluginLog.Debug("MainWindow shown due to login.");
            }
        }

        private void OnLogout(int type, int code)
        {
            PluginLog.Debug($"Logout detected. Type: {type}, Code: {code}");

            // Ensure the main window is hidden upon logout
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

        public void UpdateWrathState(bool isEnabled)
        {
            PluginLog.Debug($"Plugin.UpdateWrathState called with state: {(isEnabled ? "Enabled" : "Disabled")}");
            mainWindow.UpdateWrathState(isEnabled);
        }

        private void DrawUI()
        {
            WindowSystem.Draw();
        }

        public static async Task<IDalamudTextureWrap?> LoadTextureAsync(string path)
        {
            if (TextureCache.TryGetValue(path, out var cachedTexture))
            {
                return cachedTexture;
            }

            PluginLog.Information($"[Debug] Loading texture from: {path}");

            try
            {
                if (path.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                {
                    var response = await httpClient.GetAsync(path);
                    response.EnsureSuccessStatusCode();
                    var imageBytes = await response.Content.ReadAsByteArrayAsync();

                    var texture = TextureProvider.CreateFromImageAsync(imageBytes).Result as IDalamudTextureWrap;
                    TextureCache[path] = texture;
                    return texture;
                }
                else if (File.Exists(path))
                {
                    var texture = TextureProvider.GetFromFile(path) as IDalamudTextureWrap;
                    TextureCache[path] = texture;
                    return texture;
                }
            }
            catch (Exception ex)
            {
                PluginLog.Error($"[Debug] Error loading texture: {ex.Message}");
            }

            return null;
        }

        public void Dispose()
        {
            PluginInterface.UiBuilder.OpenConfigUi -= OpenConfigWindow;
            PluginInterface.UiBuilder.OpenMainUi -= OpenMainWindow;

            ChatGui.ChatMessage -= wrathStateChecker.ChatMessageHandler;

            ClientState.Login -= OnLogin;
            ClientState.Logout -= OnLogout;

            foreach (var texture in TextureCache.Values)
            {
                if (texture is IDalamudTextureWrap wrap)
                {
                    wrap.Dispose();
                }
            }

            CommandManager.RemoveHandler(CommandName);
            PluginInterface.UiBuilder.Draw -= DrawUI;
            WindowSystem.RemoveAllWindows();
        }
    }
}
