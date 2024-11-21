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

        public MainWindow(string iconOnUrl, string iconOffUrl, Configuration config, Plugin plugin)
            : base("WrathIconMainWindow", ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
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
                Plugin.PluginLog.Error("Failed to load textures.");
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
                ? ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoScrollbar |
                  ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoDecoration
                : ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse |
                  ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoDecoration;

            // Calculate image size based on the selected configuration
            Vector2 imageSize = new Vector2(config.SelectedImageSize, config.SelectedImageSize);

            // Dynamically calculate button padding for locked state
            Vector2 buttonPadding = ImGui.GetStyle().FramePadding;

            // Additional padding offsets for locked mode (if necessary)
            Vector2 extraPadding = config.IsLocked ? new Vector2(2.0f, 2.0f) : Vector2.Zero;

            // Calculate the total effective size of the button or image
            Vector2 effectiveSize = imageSize + buttonPadding * 4 + extraPadding;

            // Set the window size dynamically to fit the effective size
            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero); // Remove padding
            ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0.0f);     // Remove border
            ImGui.SetNextWindowSize(effectiveSize, ImGuiCond.Always);

            // Static and unique window name
            if (ImGui.Begin("WrathIconMainWindow", windowFlags))
            {
                var currentIcon = wrathState ? iconOnTexture : iconOffTexture;

                if (currentIcon != null)
                {
                    // Adjust cursor position based on padding and offsets
                    Vector2 cursorPosition = buttonPadding + (config.IsLocked ? extraPadding : Vector2.Zero);
                    ImGui.SetCursorPos(cursorPosition);

                    if (config.IsLocked)
                    {
                        // Render as a button in locked mode
                        if (ImGui.ImageButton(currentIcon.ImGuiHandle, imageSize))
                        {
                            Plugin.CommandManager.ProcessCommand("/wrath auto");
                        }
                    }
                    else
                    {
                        // Render as an image in unlocked mode
                        ImGui.Image(currentIcon.ImGuiHandle, imageSize);
                    }
                }
                else
                {
                    ImGui.Text("Loading...");
                }

                ImGui.End();
            }

            ImGui.PopStyleVar(2); // Restore padding and border settings
        }





    }
}
