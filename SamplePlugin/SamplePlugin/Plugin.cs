using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Dalamud.Plugin.Services;
using Dalamud.Game.Text;
using Dalamud.Interface.Textures.TextureWraps;

namespace SamplePlugin
{
    public sealed class Plugin : IDalamudPlugin
    {
        private const string CommandName = "/wrathicon";
        public readonly WindowSystem WindowSystem = new("Wrath Status Icon");

        private MainWindow mainWindow;
        private WrathStateChecker wrathStateChecker;

        [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
        [PluginService] internal static ITextureProvider TextureProvider { get; private set; } = null!;
        [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;
        [PluginService] internal static IPluginLog PluginLog { get; private set; } = null!;
        [PluginService] internal static IChatGui ChatGui { get; private set; } = null!;

        private static readonly ConcurrentDictionary<string, IDalamudTextureWrap?> TextureCache = new();
        private static HttpClient httpClient = new();

        public string Name => "Wrath Status Icon";

        public Plugin()
        {
            var iconOnUrl = "https://raw.githubusercontent.com/Brappp/Wrath_Auto_Tracker/main/SamplePlugin/Data/icon-on.png";
            var iconOffUrl = "https://raw.githubusercontent.com/Brappp/Wrath_Auto_Tracker/main/SamplePlugin/Data/icon-off.png";

            PluginLog.Information("[Debug] Plugin initializing...");

            mainWindow = new MainWindow(iconOnUrl, iconOffUrl);
            WindowSystem.AddWindow(mainWindow);

            wrathStateChecker = new WrathStateChecker(this);

            // Subscribe to chat messages
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
            PluginInterface.UiBuilder.OpenMainUi += ToggleMainUI;

            PluginLog.Information("Plugin initialized.");
        }

        private void OnCommand(string command, string args)
        {
            PluginLog.Debug("Command triggered to toggle MainWindow.");
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
            // Unsubscribe from chat messages
            ChatGui.ChatMessage -= wrathStateChecker.ChatMessageHandler;

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

        public void ToggleMainUI()
        {
            mainWindow.Toggle();
        }
    }
}
