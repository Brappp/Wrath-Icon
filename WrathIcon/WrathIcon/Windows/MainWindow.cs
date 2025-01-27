using System.IO;
using Dalamud.Interface.Windowing;
using Dalamud.Interface.Textures.TextureWraps;
using ImGuiNET;
using System.Numerics;
using ECommons.DalamudServices;
using ECommons.ImGuiMethods;
using WrathIcon.Core;
using WrathIcon.Utilities;

namespace WrathIcon
{
    public class MainWindow : Window
    {
        private IDalamudTextureWrap? iconOffTexture;
        private IDalamudTextureWrap? iconOnTexture;
        private bool wrathState;
        private readonly Configuration config;
        private readonly IWrathStateManager wrathStateManager;
        private readonly TextureManager textureManager;

        public MainWindow(Configuration config, IWrathStateManager wrathStateManager, TextureManager textureManager)
            : base("WrathIconMainWindow", ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoBackground)
        {
            this.config = config;
            this.wrathStateManager = wrathStateManager;
            this.textureManager = textureManager;

            LoadTextures();

            IsOpen = true;
            RespectCloseHotkey = false;
        }

        private void LoadTextures()
        {
            try
            {
                var assemblyPath = Svc.PluginInterface.AssemblyLocation.Directory?.FullName!;
                var iconOffPath = Path.Combine(assemblyPath, "images\\icon-off.png");
                var iconOnPath = Path.Combine(assemblyPath, "images\\icon-on.png");

                ThreadLoadImageHandler.TryGetTextureWrap(iconOffPath, out iconOffTexture);
                ThreadLoadImageHandler.TryGetTextureWrap(iconOnPath, out iconOnTexture);
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
            Vector2 iconSize = new Vector2(config.SelectedImageSize, config.SelectedImageSize);
            Vector2 windowSize = iconSize + new Vector2(20, 20);

            if (!config.IsLocked)
            {
                Vector2 currentWindowPos = ImGui.GetWindowPos();
                Vector2 currentWindowSize = ImGui.GetWindowSize();
                Vector2 currentCenter = currentWindowPos + (currentWindowSize * 0.5f);

                config.WindowX = currentCenter.X;
                config.WindowY = currentCenter.Y;
                config.Save();
            }
            else
            {
                Vector2 lockedPosition = new Vector2(config.WindowX, config.WindowY) - (windowSize * 0.5f);
                ImGui.SetNextWindowPos(lockedPosition, ImGuiCond.Always);
            }

            ImGui.SetNextWindowSize(windowSize);

            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);
            ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0.0f);
            ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(3, 3));

            ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.2f, 0.2f, 0.2f, 1.0f));
            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(0.25f, 0.25f, 0.3f, 1.0f));
            ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(0.3f, 0.3f, 0.35f, 1.0f));

            ImGuiWindowFlags windowFlags = ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoResize |
                                           ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse |
                                           ImGuiWindowFlags.NoBackground;

            if (config.IsLocked)
            {
                windowFlags |= ImGuiWindowFlags.NoMove;
            }

            if (ImGui.Begin("WrathIconMainWindow", windowFlags))
            {
                var currentIcon = wrathState ? iconOnTexture : iconOffTexture;

                if (currentIcon != null)
                {
                    Vector2 fixedPosition = (windowSize - iconSize) * 0.5f;
                    ImGui.SetCursorPos(fixedPosition);

                    if (ImGui.ImageButton(currentIcon.ImGuiHandle, iconSize))
                    {
                        if (config.IsLocked)
                        {
                            Plugin.CommandManager.ProcessCommand("/wrath auto");
                        }
                    }

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
                    Vector2 loadingTextSize = new Vector2(100, 20);
                    ImGui.SetCursorPos((windowSize - loadingTextSize) * 0.5f);
                    ImGui.Text("Loading...");
                }

                ImGui.End();
            }

            ImGui.PopStyleColor(3);
            ImGui.PopStyleVar(3);
        }
    }
}