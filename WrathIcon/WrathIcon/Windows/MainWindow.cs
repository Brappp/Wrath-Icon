using Dalamud.Interface.Windowing;
using Dalamud.Interface.Textures.TextureWraps;
using ImGuiNET;
using System.Numerics;
using WrathIcon.Utilities;
using WrathIcon.Core;

namespace WrathIcon
{
    public class MainWindow : Window
    {
        private IDalamudTextureWrap? iconOnTexture;
        private IDalamudTextureWrap? iconOffTexture;
        private bool wrathState;
        private readonly Configuration config;
        private readonly IWrathStateManager wrathStateManager;
        private readonly TextureManager textureManager;

        public MainWindow(string iconOnUrl, string iconOffUrl, Configuration config, IWrathStateManager wrathStateManager, TextureManager textureManager)
            : base("WrathIconMainWindow", ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoBackground)
        {
            this.config = config;
            this.wrathStateManager = wrathStateManager;
            this.textureManager = textureManager;

            LoadTextures(iconOnUrl, iconOffUrl);

            IsOpen = true; // Always open the window
        }

        private async void LoadTextures(string iconOnUrl, string iconOffUrl)
        {
            try
            {
                iconOnTexture = await textureManager.LoadTextureAsync(iconOnUrl);
                iconOffTexture = await textureManager.LoadTextureAsync(iconOffUrl);
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
            // Calculate the window size with extra width and height for padding
            Vector2 imageSize = new Vector2(config.SelectedImageSize + 20, config.SelectedImageSize + 20); // Add padding
            ImGui.SetNextWindowSize(imageSize, ImGuiCond.Always);

            // Begin the transparent, undecorated window
            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero); // Remove padding
            ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0.0f);     // Remove border
            if (ImGui.Begin("WrathIconMainWindow", ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoBackground))
            {
                var currentIcon = wrathState ? iconOnTexture : iconOffTexture;

                if (currentIcon != null)
                {
                    // Calculate position to center the icon in the window
                    Vector2 iconSize = new Vector2(config.SelectedImageSize, config.SelectedImageSize);
                    Vector2 cursorPosition = (imageSize - iconSize) * 0.5f; // Center icon
                    ImGui.SetCursorPos(cursorPosition);

                    if (config.IsLocked)
                    {
                        // Render as a button when locked
                        if (ImGui.ImageButton(currentIcon.ImGuiHandle, iconSize))
                        {
                            Plugin.CommandManager.ProcessCommand("/wrath auto");
                        }
                    }
                    else
                    {
                        // Render as an image when unlocked
                        ImGui.Image(currentIcon.ImGuiHandle, iconSize);
                    }
                }
                else
                {
                    ImGui.Text("Loading...");
                }

                ImGui.End();
            }
            ImGui.PopStyleVar(2); // Restore style variables
        }
    }
}
