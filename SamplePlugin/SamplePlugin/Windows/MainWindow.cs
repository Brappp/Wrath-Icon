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

        public MainWindow(string iconOnUrl, string iconOffUrl)
            : base("", ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
        {
            SizeConstraints = new WindowSizeConstraints
            {
                MinimumSize = new Vector2(64, 64),
                MaximumSize = new Vector2(64, 64)
            };

            LoadTextures(iconOnUrl, iconOffUrl);
        }

        /// <summary>
        /// Asynchronously loads the on/off textures.
        /// </summary>
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

        /// <summary>
        /// Updates the Wrath state and refreshes the image.
        /// </summary>
        public void UpdateWrathState(bool isEnabled)
        {
            wrathState = isEnabled;
            IsOpen = true; // Ensure the window remains open
        }

        /// <summary>
        /// Draws the current Wrath state icon in the window.
        /// </summary>
        public override void Draw()
        {
            var currentIcon = wrathState ? iconOnTexture : iconOffTexture;

            if (currentIcon != null)
            {
                Vector2 imageSize = new(64, 64);
                ImGui.Image(currentIcon.ImGuiHandle, imageSize);
            }
            else
            {
                ImGui.Text("Loading...");
            }
        }
    }
}
