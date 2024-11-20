using Dalamud.Interface.Windowing;
using Dalamud.Interface.Textures.TextureWraps;
using ImGuiNET;
using System.Numerics;

namespace SamplePlugin
{
    public class MainWindow : Window
    {
        private IDalamudTextureWrap? iconOnTexture;
        private IDalamudTextureWrap? iconOffTexture;
        private bool wrathState;
        private readonly Configuration config;
        private readonly Plugin plugin;

        // Default size for the images
        private const int DefaultImageSize = 64;

        public MainWindow(string iconOnUrl, string iconOffUrl, Configuration config, Plugin plugin)
            : base("", ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
        {
            this.config = config;
            this.plugin = plugin;

            LoadTextures(iconOnUrl, iconOffUrl);

            IsOpen = true; // Always open the window
        }

        private async void LoadTextures(string iconOnUrl, string iconOffUrl)
        {
            try
            {
                iconOnTexture = await Plugin.LoadTextureAsync(iconOnUrl);
                iconOffTexture = await Plugin.LoadTextureAsync(iconOffUrl);
            }
            catch
            {
                Plugin.PluginLog.Error("Failed to load textures. Check the URLs or connection.");
            }
        }

        public void UpdateWrathState(bool isEnabled)
        {
            wrathState = isEnabled;
        }

        public override void Draw()
        {
            // Dynamically set window flags based on lock state
            var windowFlags = config.IsLocked
                ? ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse
                : ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse;

            // Calculate image size based on the selected configuration
            Vector2 imageSize = new Vector2(config.SelectedImageSize, config.SelectedImageSize);

            // Dynamically calculate padding for locked state
            Vector2 buttonPadding = Vector2.Zero;
            if (config.IsLocked)
            {
                // Use ImGui style variables to fetch the padding for buttons
                buttonPadding = ImGui.GetStyle().FramePadding;
            }

            // Set the window size dynamically to fit the image and padding
            Vector2 windowSize = imageSize + buttonPadding * 2;
            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);
            ImGui.SetNextWindowSize(windowSize, ImGuiCond.Always);

            if (ImGui.Begin("###WrathIconMainWindow", windowFlags))
            {
                var currentIcon = wrathState ? iconOnTexture : iconOffTexture;

                if (currentIcon != null)
                {
                    if (config.IsLocked)
                    {
                        // Render as a button with padding
                        if (ImGui.ImageButton(currentIcon.ImGuiHandle, imageSize))
                        {
                            Plugin.CommandManager.ProcessCommand("/wrath auto");
                            Plugin.PluginLog.Debug("Wrath auto command executed via MainWindow button.");
                        }
                    }
                    else
                    {
                        // Render as an image without padding
                        ImGui.Image(currentIcon.ImGuiHandle, imageSize);
                    }
                }
                else
                {
                    ImGui.Text("Loading...");
                }

                ImGui.End();
            }

            ImGui.PopStyleVar(); // Restore default padding
        }

    }
}
