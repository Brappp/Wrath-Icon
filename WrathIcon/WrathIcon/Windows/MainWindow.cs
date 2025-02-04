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

        private bool needsRedraw = true;

        public MainWindow(Configuration config, TextureManager textureManager)
            : base("WrathIconMainWindow",
                   ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoResize |
                   ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse |
                   ImGuiWindowFlags.NoBackground)
        {
            this.config = config;
            this.textureManager = textureManager;
            LoadTextures();

            IsOpen = true;

            RespectCloseHotkey = false;
        }

        private async void LoadTextures()
        {
            try
            {
                iconOnTexture = await textureManager.LoadTextureAsync("https://raw.githubusercontent.com/Brappp/Wrath_Auto_Tracker/main/WrathIcon/Data/icon-on.png");
                iconOffTexture = await textureManager.LoadTextureAsync("https://raw.githubusercontent.com/Brappp/Wrath_Auto_Tracker/main/WrathIcon/Data/icon-off.png");
                Plugin.PluginLog.Information("WrathIcon textures loaded successfully.");
            }
            catch
            {
                Plugin.PluginLog.Error("Failed to load textures.");
            }

            MarkDirty(); // Ensure UI updates after textures are loaded
        }

        public override void Draw()
        {
            float currentTime = (float)ImGui.GetTime();
            if (currentTime - lastCheckTime > CheckInterval)
            {
                bool newState = WrathIPC.GetAutoRotationState();
                if (newState != wrathState)
                {
                    wrathState = newState;
                    needsRedraw = true;
                }
                lastCheckTime = currentTime;
            }

            if (!needsRedraw && !IsOpen) return;
            needsRedraw = false;

            Vector2 windowSize = new Vector2(config.SelectedImageSize + 20, config.SelectedImageSize + 20);
            ImGui.SetNextWindowSize(windowSize, ImGuiCond.Always);

            ImGuiWindowFlags windowFlags = ImGuiWindowFlags.NoDecoration |
                                           ImGuiWindowFlags.NoResize |
                                           ImGuiWindowFlags.NoScrollbar |
                                           ImGuiWindowFlags.NoScrollWithMouse |
                                           ImGuiWindowFlags.NoBackground;

            if (config.IsLocked)
            {
                ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);
                ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0.0f);
                windowFlags |= ImGuiWindowFlags.NoMove;
            }

            if (ImGui.Begin("WrathIconMainWindow", windowFlags))
            {
                var currentIcon = wrathState ? iconOnTexture : iconOffTexture;

                if (currentIcon != null)
                {
                    Vector2 iconSize = new Vector2(config.SelectedImageSize, config.SelectedImageSize);
                    Vector2 fixedPosition = (windowSize - iconSize) * 0.5f;
                    fixedPosition.X = (float)System.Math.Floor(fixedPosition.X + 0.5f);
                    fixedPosition.Y = (float)System.Math.Floor(fixedPosition.Y + 0.5f);
                    ImGui.SetCursorPos(fixedPosition);

                    ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(1.0f, 1.0f, 1.0f, 0.0f));
                    ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(1.0f, 1.0f, 1.0f, 0.0f));
                    ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(1.0f, 1.0f, 1.0f, 0.0f));
                    ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, Vector2.Zero);

                    if (ImGui.ImageButton(currentIcon.ImGuiHandle, iconSize))
                    {
                        WrathAutoManager.ToggleWrathAuto();
                        MarkDirty();
                    }

                    ImGui.PopStyleVar();
                    ImGui.PopStyleColor(3);

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
                            MarkDirty();
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

        public void MarkDirty()
        {
            needsRedraw = true;
        }

        public void SetOpen(bool open)
        {
            if (IsOpen != open)
            {
                IsOpen = open;
                MarkDirty();
            }
        }
    }
}
