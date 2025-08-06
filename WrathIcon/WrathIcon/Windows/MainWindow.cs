using Dalamud.Interface.Windowing;
using Dalamud.Interface.Textures.TextureWraps;
using Dalamud.Bindings.ImGui;
using System.Numerics;
using WrathIcon.Core.Services;
using WrathIcon.Utilities;
using System;
using System.Threading.Tasks;
using Dalamud.Interface;

namespace WrathIcon.Windows
{
    public class MainWindow : Window
    {
        private IDalamudTextureWrap? iconOnTexture;
        private IDalamudTextureWrap? iconOffTexture;
        private bool wrathState;
        private readonly Configuration config;
        private readonly ITextureService textureService;
        private readonly IWrathService wrathService;
        private bool texturesLoaded = false;

        public MainWindow(Configuration config, ITextureService textureService, IWrathService wrathService)
            : base(Constants.MainWindowName,
                   ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoResize |
                   ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse |
                   ImGuiWindowFlags.NoBackground)
        {
            this.config = config ?? throw new ArgumentNullException(nameof(config));
            this.textureService = textureService ?? throw new ArgumentNullException(nameof(textureService));
            this.wrathService = wrathService ?? throw new ArgumentNullException(nameof(wrathService));

            // Subscribe to state changes
            this.wrathService.StateChanged += OnWrathStateChanged;
            this.config.ConfigurationChanged += OnConfigurationChanged;

            // Initialize current state
            wrathState = this.wrathService.IsAutoRotationEnabled;

            LoadTexturesAsync();

            IsOpen = config.AutoShowOnLogin;
            RespectCloseHotkey = false;

            Logger.Debug("MainWindow initialized");
        }

        private async void LoadTexturesAsync()
        {
            try
            {
                Logger.Debug("Loading local textures...");
                
                // Load textures in parallel for better performance
                var onTextureTask = textureService.LoadTextureAsync(Constants.IconOnPath);
                var offTextureTask = textureService.LoadTextureAsync(Constants.IconOffPath);

                // Wait for both with a reasonable timeout
                var timeoutTask = Task.Delay(5000); // 5 second timeout
                var completedTask = await Task.WhenAny(
                    Task.WhenAll(onTextureTask, offTextureTask),
                    timeoutTask
                );

                if (completedTask == timeoutTask)
                {
                    Logger.Warning("Texture loading timed out after 5 seconds, using FontAwesome fallback");
                    texturesLoaded = false;
                }
                else
                {
                    iconOnTexture = await onTextureTask;
                    iconOffTexture = await offTextureTask;
                    texturesLoaded = iconOnTexture != null && iconOffTexture != null;
                    
                    if (texturesLoaded)
                    {
                        Logger.Info("Local textures loaded successfully");
                    }
                    else
                    {
                        Logger.Warning("Failed to load PNG textures, will use FontAwesome icons as fallback");
                    }
                }

                MarkDirty();
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to load local textures", ex);
                texturesLoaded = false;
                MarkDirty();
            }
        }

        private void OnWrathStateChanged(bool newState)
        {
            Logger.Debug($"Wrath state changed: {wrathState} -> {newState}");
            wrathState = newState;
            MarkDirty();
        }

        private void OnConfigurationChanged(Configuration newConfig)
        {
            MarkDirty();
        }

        public override void Draw()
        {
            if (!IsOpen) 
                return;

            var windowSize = CalculateWindowSize();
            ImGui.SetNextWindowSize(windowSize, ImGuiCond.Always);

            // Set window position - use different conditions based on lock state
            if (config.IsLocked)
            {
                // When locked, force the position to stay at the configured location
                ImGui.SetNextWindowPos(new Vector2(config.WindowX, config.WindowY), ImGuiCond.Always);
            }
            else
            {
                // When unlocked, only set position on first use or if position changed
                ImGui.SetNextWindowPos(new Vector2(config.WindowX, config.WindowY), ImGuiCond.FirstUseEver);
            }

            ImGuiWindowFlags windowFlags = ImGuiWindowFlags.NoDecoration |
                                           ImGuiWindowFlags.NoResize |
                                           ImGuiWindowFlags.NoScrollbar |
                                           ImGuiWindowFlags.NoScrollWithMouse |
                                           ImGuiWindowFlags.NoBackground;

            // Add NoMove flag when locked
            if (config.IsLocked)
            {
                windowFlags |= ImGuiWindowFlags.NoMove;
                ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);
                ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0.0f);
            }

            if (ImGui.Begin(Constants.MainWindowName, windowFlags))
            {
                // Update window position in config if not locked
                if (!config.IsLocked)
                {
                    var currentPos = ImGui.GetWindowPos();
                    if (Math.Abs(currentPos.X - config.WindowX) > 1.0f || Math.Abs(currentPos.Y - config.WindowY) > 1.0f)
                    {
                        config.SetWindowPosition(currentPos.X, currentPos.Y);
                    }
                }

                if (texturesLoaded)
                {
                    DrawInterface();
                }
                else
                {
                    DrawFontAwesomeInterface();
                }

                ImGui.End();
            }

            if (config.IsLocked)
            {
                ImGui.PopStyleVar(2);
            }
        }

        private Vector2 CalculateWindowSize()
        {
            var iconSize = config.SelectedImageSize;
            var padding = 10f;
            var baseHeight = iconSize + padding * 2;
            
            return new Vector2(iconSize + padding * 2, baseHeight);
        }

        private void DrawInterface()
        {
            var iconSize = new Vector2(config.SelectedImageSize, config.SelectedImageSize);
            
            if (texturesLoaded && iconOnTexture != null && iconOffTexture != null)
            {
                DrawMainIcon(iconSize);
            }
            else
            {
                // Use FontAwesome as immediate fallback while textures load or if they fail
                DrawFontAwesomeInterface();
            }
        }

        private void DrawFontAwesomeInterface()
        {
            // Use FontAwesome icons as fallback
            var iconSize = new Vector2(config.SelectedImageSize, config.SelectedImageSize);
            var icon = wrathState ? FontAwesomeIcon.Play : FontAwesomeIcon.Stop;
            var buttonColor = wrathState ? new Vector4(0.0f, 1.0f, 0.0f, 0.3f) : new Vector4(1.0f, 0.0f, 0.0f, 0.3f);
            
            // Center the icon
            var availableWidth = ImGui.GetContentRegionAvail().X;
            var iconPosX = (availableWidth - iconSize.X) * 0.5f;
            if (iconPosX > 0) ImGui.SetCursorPosX(ImGui.GetCursorPosX() + iconPosX);

            ImGui.PushStyleColor(ImGuiCol.Button, buttonColor);
            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(buttonColor.X, buttonColor.Y, buttonColor.Z, buttonColor.W + 0.1f));
            ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(buttonColor.X, buttonColor.Y, buttonColor.Z, buttonColor.W + 0.2f));

            ImGui.PushFont(UiBuilder.IconFont);
            if (ImGui.Button(icon.ToIconString(), iconSize))
            {
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await wrathService.ToggleAutoRotationAsync();
                    }
                    catch (Exception ex)
                    {
                        Logger.Error("Failed to toggle auto-rotation", ex);
                    }
                });
            }
            ImGui.PopFont();

            ImGui.PopStyleColor(3);

            if (config.ShowTooltips && ImGui.IsItemHovered())
            {
                ImGui.SetTooltip($"Auto-Rotation: {(wrathState ? "Enabled" : "Disabled")}\nClick to toggle");
            }

            // Handle dragging if not locked
            if (!config.IsLocked)
            {
                HandleWindowDragging();
            }
        }

        private void DrawMainIcon(Vector2 iconSize)
        {
            var currentIcon = wrathState ? iconOnTexture : iconOffTexture;
            
            if (currentIcon == null) 
            {
                Logger.Warning($"Icon texture is null! wrathState={wrathState}");
                return;
            }

            // Center the icon in its allocated space
            var availableWidth = ImGui.GetContentRegionAvail().X;
            var iconPosX = (availableWidth - iconSize.X) * 0.5f;
            if (iconPosX > 0) ImGui.SetCursorPosX(ImGui.GetCursorPosX() + iconPosX);

            // Make button transparent
            ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(1.0f, 1.0f, 1.0f, 0.0f));
            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(1.0f, 1.0f, 1.0f, 0.1f));
            ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(1.0f, 1.0f, 1.0f, 0.2f));
            ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, Vector2.Zero);

            if (ImGui.ImageButton(currentIcon.Handle, iconSize))
            {
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await wrathService.ToggleAutoRotationAsync();
                    }
                    catch (Exception ex)
                    {
                        Logger.Error("Failed to toggle auto-rotation", ex);
                    }
                });
            }

            ImGui.PopStyleVar();
            ImGui.PopStyleColor(3);

            if (config.ShowTooltips && ImGui.IsItemHovered())
            {
                ImGui.SetTooltip($"Auto-Rotation: {(wrathState ? "Enabled" : "Disabled")}\nClick to toggle");
            }

            // Handle dragging if not locked
            if (!config.IsLocked)
            {
                HandleWindowDragging();
            }
        }

        private void HandleWindowDragging()
        {
            // Don't handle dragging if window is locked
            if (config.IsLocked) 
                return;
                
            // Only handle dragging on the icon itself
            if (ImGui.IsItemActive() && ImGui.IsMouseDragging(ImGuiMouseButton.Left))
            {
                Vector2 dragDelta = ImGui.GetMouseDragDelta(ImGuiMouseButton.Left);
                var newX = config.WindowX + dragDelta.X;
                var newY = config.WindowY + dragDelta.Y;
                config.SetWindowPosition(newX, newY);
                ImGui.ResetMouseDragDelta();
            }
        }

        public void MarkDirty()
        {
            // No longer needed since we always redraw, but keeping for compatibility
        }

        public void SetOpen(bool open)
        {
            if (IsOpen != open)
            {
                IsOpen = open;
                Logger.Debug($"MainWindow visibility set to: {open}");
                MarkDirty();
            }
        }

        public void Dispose()
        {
            wrathService.StateChanged -= OnWrathStateChanged;
            config.ConfigurationChanged -= OnConfigurationChanged;
            Logger.Debug("MainWindow disposed");
        }
    }
}
