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
        }

        private void OnFrameworkUpdate(IFramework framework)
        {
            // Check if the player is logged in and initialization hasn't already occurred
            if (ClientState.IsLoggedIn && !isInitialized)
            {
                Initialize();
                isInitialized = true;

                // Unsubscribe from the Framework Update event
                Framework.Update -= OnFrameworkUpdate;
            }
        }

        private void Initialize()
        {
            // Load or initialize configuration
            config = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            config.Initialize(PluginInterface);

            var iconOnUrl = "https://raw.githubusercontent.com/Brappp/Wrath_Auto_Tracker/main/SamplePlugin/Data/icon-on.png";
            var iconOffUrl = "https://raw.githubusercontent.com/Brappp/Wrath_Auto_Tracker/main/SamplePlugin/Data/icon-off.png";

            PluginLog.Information("[Debug] Plugin initializing...");

            // Initialize windows
            mainWindow = new MainWindow(iconOnUrl, iconOffUrl, config, this);
            configWindow = new ConfigWindow(config);

            // Register windows
            RegisterWindow(mainWindow);
            RegisterWindow(configWindow);

            // Initialize WrathStateChecker
            wrathStateChecker = new WrathStateChecker(this);

            // Subscribe to chat messages
            ChatGui.ChatMessage += wrathStateChecker.ChatMessageHandler;

            // Handle Wrath state changes
            wrathStateChecker.OnWrathStateChanged += state =>
            {
                PluginLog.Debug($"WrathStateChecker.OnWrathStateChanged triggered with state: {(state ? "Enabled" : "Disabled")}");
                mainWindow.UpdateWrathState(state);
            };

            // Add command handler
            CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
            {
                HelpMessage = "Toggle Wrath Status Icon UI"
            });

            // Register UI callbacks
            PluginInterface.UiBuilder.Draw += DrawUI;
            PluginInterface.UiBuilder.OpenConfigUi += OpenConfigWindow;
            PluginInterface.UiBuilder.OpenMainUi += OpenMainWindow;

            // Automatically set the state
            InitializeWrathState();

            PluginLog.Information("Plugin initialized.");
        }

        private void RegisterWindow(Window window)
        {
            PluginLog.Debug($"Registering window: {window.WindowName}");
            WindowSystem.AddWindow(window);
        }

        private void OnCommand(string command, string args)
        {
            PluginLog.Debug("Command triggered to toggle ConfigWindow.");
            configWindow.Toggle(); // Open or close the configuration window
        }

        private async void InitializeWrathState()
        {
            PluginLog.Debug("Initializing Wrath state with /wrath auto command.");

            // Run the command twice to set the plugin's state
            CommandManager.ProcessCommand("/wrath auto");
            await Task.Delay(500); // Add a small delay between commands
            CommandManager.ProcessCommand("/wrath auto");

            PluginLog.Debug("Wrath state initialization complete.");
        }

        private void OpenConfigWindow()
        {
            if (!configWindow.IsOpen)
                configWindow.Toggle(); // Open or close the configuration window
        }

        private void OpenMainWindow()
        {
            if (!mainWindow.IsOpen)
                mainWindow.Toggle(); // Open or close the main window
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
            // Unsubscribe from UI callbacks
            PluginInterface.UiBuilder.OpenConfigUi -= OpenConfigWindow;
            PluginInterface.UiBuilder.OpenMainUi -= OpenMainWindow;

            // Unsubscribe from chat messages
            ChatGui.ChatMessage -= wrathStateChecker.ChatMessageHandler;

            // Dispose textures
            foreach (var texture in TextureCache.Values)
            {
                if (texture is IDalamudTextureWrap wrap)
                {
                    wrap.Dispose();
                }
            }

            // Remove command handler
            CommandManager.RemoveHandler(CommandName);

            // Unsubscribe UI events
            PluginInterface.UiBuilder.Draw -= DrawUI;

            // Remove all windows
            PluginLog.Debug("Removing all windows...");
            WindowSystem.RemoveAllWindows();
        }
    }
}
