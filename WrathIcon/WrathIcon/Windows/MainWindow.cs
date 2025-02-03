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
            : base("WrathIconMainWindow",
                   ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoResize |
                   ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse |
                   ImGuiWindowFlags.NoBackground)
        {
            this.config = config;
            this.textureManager = textureManager;

            // Load textures for the on/off states
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
            // Define the window size based on the configured image size (with extra padding)
            Vector2 windowSize = new Vector2(config.SelectedImageSize + 20, config.SelectedImageSize + 20);
            Vector2 targetCenter = new Vector2(config.WindowX, config.WindowY);

            // When the window is unlocked, update the config's stored position from the current window center.
            if (!config.IsLocked)
            {
                Vector2 currentWindowPos = ImGui.GetWindowPos();
                Vector2 currentWindowSize = ImGui.GetWindowSize();
                targetCenter = currentWindowPos + (currentWindowSize * 0.5f);
                config.WindowX = targetCenter.X;
                config.WindowY = targetCenter.Y;
                config.Save();
            }
            else
            {
                // When locked, center the window around the target center.
                Vector2 lockedPosition = targetCenter - (windowSize * 0.5f);
                ImGui.SetNextWindowPos(lockedPosition, ImGuiCond.Always);
            }

            // Force the window to have exactly the defined size and prevent any resizing.
            ImGui.SetNextWindowSize(windowSize, ImGuiCond.Always);
            ImGui.SetNextWindowSizeConstraints(windowSize, windowSize);

            // If locked, remove window padding/border to avoid any shifts.
            if (config.IsLocked)
            {
                ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);
                ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0.0f);
            }

            ImGuiWindowFlags windowFlags = ImGuiWindowFlags.NoDecoration |
                                           ImGuiWindowFlags.NoResize |
                                           ImGuiWindowFlags.NoScrollbar |
                                           ImGuiWindowFlags.NoScrollWithMouse |
                                           ImGuiWindowFlags.NoBackground;
            if (config.IsLocked)
                windowFlags |= ImGuiWindowFlags.NoMove;

            if (ImGui.Begin("WrathIconMainWindow", windowFlags))
            {
                // Update the wrathState periodically so the correct icon is chosen.
                float currentTime = (float)ImGui.GetTime();
                if (currentTime - lastCheckTime > CheckInterval)
                {
                    wrathState = WrathIPC.GetAutoRotationState();
                    lastCheckTime = currentTime;
                }
                var currentIcon = wrathState ? iconOnTexture : iconOffTexture;

                if (currentIcon != null)
                {
                    // Define the icon size (without any scaling animation)
                    Vector2 iconSize = new Vector2(config.SelectedImageSize, config.SelectedImageSize);
                    Vector2 scaledSize = iconSize; // No scaling animations; always use iconSize

                    // Center the icon within the window.
                    Vector2 fixedPosition = (windowSize - scaledSize) * 0.5f;
                    fixedPosition.X = (float)System.Math.Floor(fixedPosition.X + 0.5f);
                    fixedPosition.Y = (float)System.Math.Floor(fixedPosition.Y + 0.5f);
                    ImGui.SetCursorPos(fixedPosition);

                    // Apply transparent button styling (no visual feedback/animations)
                    ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(1.0f, 1.0f, 1.0f, 0.0f));
                    ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(1.0f, 1.0f, 1.0f, 0.0f));
                    ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(1.0f, 1.0f, 1.0f, 0.0f));

                    // Remove internal frame padding that might clip the image.
                    ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, Vector2.Zero);

                    if (ImGui.ImageButton(currentIcon.ImGuiHandle, scaledSize))
                    {
                        WrathAutoManager.ToggleWrathAuto();
                    }

                    ImGui.PopStyleVar(); 
                    ImGui.PopStyleColor(3);

                    // If unlocked, allow dragging of the window and update the stored position.
                    if (!config.IsLocked)
                    {
                        ImGui.SetItemAllowOverlap();
                        if (ImGui.IsItemActive() && ImGui.IsMouseDragging(ImGuiMouseButton.Left))
                        {
                            Vector2 dragDelta = ImGui.GetMouseDragDelta(ImGuiMouseButton.Left);
                            config.WindowX += dragDelta.X;
                            config.WindowY += dragDelta.Y;
                            ImGui.ResetMouseDragDelta();
                            config.Save();
                        }
                    }
                }
                else
                {
                    ImGui.Text("Loading...");
                }
                ImGui.End();
            }

            if (config.IsLocked)
            {
                ImGui.PopStyleVar(2);
            }
        }
    }
}
