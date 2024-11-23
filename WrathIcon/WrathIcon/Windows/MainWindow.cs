using Dalamud.Interface.Windowing;
using Dalamud.Interface.Textures.TextureWraps;
using ImGuiNET;
using System.Numerics;
using WrathIcon.Core;
using WrathIcon.Utilities;

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
            Vector2 windowSize = new Vector2(config.SelectedImageSize + 20, config.SelectedImageSize + 20);

            ImGuiStylePtr style = ImGui.GetStyle();
            Vector2 framePadding = style.FramePadding;

            Vector2 targetCenter = Vector2.Zero;

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
                targetCenter = new Vector2(config.WindowX, config.WindowY);

                Vector2 lockedPosition = targetCenter - (windowSize * 0.5f) - framePadding;

                ImGui.SetNextWindowPos(lockedPosition, ImGuiCond.Always);
            }

            ImGui.SetNextWindowSize(windowSize, ImGuiCond.Always);

            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);
            ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0.0f); 
            ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 0.0f);   

            ImGuiWindowFlags windowFlags = ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoResize |
                                           ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse |
                                           ImGuiWindowFlags.NoBackground;

            if (ImGui.Begin("WrathIconMainWindow", windowFlags))
            {
                var currentIcon = wrathState ? iconOnTexture : iconOffTexture;

                if (currentIcon != null)
                {
                    Vector2 iconSize = new Vector2(config.SelectedImageSize, config.SelectedImageSize);
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
                    ImGui.Text("Loading...");
                }

                ImGui.End();
            }

            ImGui.PopStyleVar(3);
        }
    }
}
