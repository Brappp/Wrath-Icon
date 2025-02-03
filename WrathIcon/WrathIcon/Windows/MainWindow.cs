using Dalamud.Interface.Windowing;
using Dalamud.Interface.Textures;
using ImGuiNET;
using System.Numerics;
using WrathIcon.Core;
using WrathIcon.Utilities;
using Dalamud.Interface.Textures.TextureWraps;

namespace WrathIcon
{
    public class MainWindow : Window
    {
        private IDalamudTextureWrap? iconOnTexture;
        private IDalamudTextureWrap? iconOffTexture;
        private bool wrathState;
        private readonly Configuration config;
        private readonly TextureManager textureManager;
        private float lastCheckTime = 0f;
        private const float CheckInterval = 1.5f; // Interval for state checking

        public MainWindow(Configuration config, TextureManager textureManager)
            : base("WrathIconMainWindow", ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoBackground)
        {
            this.config = config;
            this.textureManager = textureManager;

            LoadTextures(
                "https://raw.githubusercontent.com/Brappp/Wrath_Auto_Tracker/main/WrathIcon/Data/icon-on.png",
                "https://raw.githubusercontent.com/Brappp/Wrath_Auto_Tracker/main/WrathIcon/Data/icon-off.png"
            );

            IsOpen = true;
            RespectCloseHotkey = false;
        }

        private async void LoadTextures(string iconOnUrl, string iconOffUrl)
        {
            try
            {
                iconOnTexture = await textureManager.LoadTextureAsync(iconOnUrl);
                iconOffTexture = await textureManager.LoadTextureAsync(iconOffUrl);
                Plugin.PluginLog.Information("WrathIcon textures loaded successfully.");
            }
            catch
            {
                Plugin.PluginLog.Error("Failed to load textures.");
            }
        }

        public override void Draw()
        {
            Vector2 windowSize = new Vector2(config.SelectedImageSize + 20, config.SelectedImageSize + 20);
            Vector2 targetCenter = new Vector2(config.WindowX, config.WindowY);

            // Limit state checking to avoid frequent updates
            float currentTime = (float)ImGui.GetTime();
            if (currentTime - lastCheckTime > CheckInterval)
            {
                wrathState = WrathIPC.GetAutoRotationState();
                lastCheckTime = currentTime;
            }

            var currentIcon = wrathState ? iconOnTexture : iconOffTexture;

            ImGui.SetNextWindowSize(windowSize, ImGuiCond.Always);
            ImGui.SetNextWindowPos(targetCenter, ImGuiCond.Appearing, new Vector2(0.5f, 0.5f));

            if (ImGui.Begin("WrathIconMainWindow", ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoResize |
                                                 ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse |
                                                 ImGuiWindowFlags.NoBackground))
            {
                if (currentIcon != null)
                {
                    Vector2 baseSize = new Vector2(config.SelectedImageSize, config.SelectedImageSize);
                    Vector2 scaledSize = baseSize;

                    // Apply scaling when hovered or clicked
                    if (ImGui.IsItemHovered()) scaledSize *= 1.1f;
                    if (ImGui.IsItemActive()) scaledSize *= 1.05f;

                    // Center icon
                    Vector2 fixedPosition = (windowSize - scaledSize) * 0.5f;
                    ImGui.SetCursorPos(fixedPosition);

                    // Transparent button styles
                    ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(1.0f, 1.0f, 1.0f, 0.0f));
                    ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(1.0f, 1.0f, 1.0f, 0.0f));
                    ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(1.0f, 1.0f, 1.0f, 0.0f));

                    if (ImGui.ImageButton(currentIcon.ImGuiHandle, scaledSize))
                    {
                        WrathAutoManager.ToggleWrathAuto();
                    }

                    ImGui.PopStyleColor(3); 
                }
                else
                {
                    ImGui.Text("Loading...");
                }

                ImGui.End();
            }
        }
    }
}
