using Dalamud.Interface.Windowing;
using Dalamud.Interface.Textures.TextureWraps;
using Dalamud.Bindings.ImGui;
using System.Numerics;
using WrathIcon.Core.Services;
using WrathIcon.Utilities;
using System;
using System.Threading.Tasks;
using Dalamud.Interface;
using Dalamud.Interface.ManagedFontAtlas;
using Dalamud.Game.ClientState.Conditions;

namespace WrathIcon.Windows
{
    public class MainWindow : Window, IDisposable
    {
        private const ImGuiWindowFlags BaseFlags =
            ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoResize |
            ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse |
            ImGuiWindowFlags.NoBackground;

        private IDalamudTextureWrap? iconOnTexture;
        private IDalamudTextureWrap? iconOffTexture;
        private readonly Configuration config;
        private readonly ITextureService textureService;
        private readonly IWrathService wrathService;
        private bool texturesLoaded = false;
        private bool wasDragging = false;
        private IFontHandle? scaledIconFont;
        private int loadedIconFontSize = -1;

        public MainWindow(Configuration config, ITextureService textureService, IWrathService wrathService)
            : base(Constants.MainWindowName, BaseFlags)
        {
            this.config = config ?? throw new ArgumentNullException(nameof(config));
            this.textureService = textureService ?? throw new ArgumentNullException(nameof(textureService));
            this.wrathService = wrathService ?? throw new ArgumentNullException(nameof(wrathService));

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

                var onTextureTask = textureService.LoadTextureAsync(Constants.IconOnPath);
                var offTextureTask = textureService.LoadTextureAsync(Constants.IconOffPath);

                var timeoutTask = Task.Delay(5000);
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
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to load local textures", ex);
                texturesLoaded = false;
            }
        }

        public override bool DrawConditions()
        {
            return !Plugin.Condition[ConditionFlag.WatchingCutscene]
                && !Plugin.Condition[ConditionFlag.WatchingCutscene78]
                && !Plugin.Condition[ConditionFlag.OccupiedInCutSceneEvent]
                && !Plugin.Condition[ConditionFlag.BetweenAreas]
                && !Plugin.Condition[ConditionFlag.BetweenAreas51]
                && !Plugin.Condition[ConditionFlag.LoggingOut];
        }

        public override void PreDraw()
        {
            EnsureIconFont(config.SelectedImageSize);

            Size = CalculateWindowSize();
            SizeCondition = ImGuiCond.Always;

            Position = new Vector2(config.WindowX, config.WindowY);
            PositionCondition = ImGuiCond.Always;

            Flags = config.IsLocked ? BaseFlags | ImGuiWindowFlags.NoMove : BaseFlags;

            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);
            ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0.0f);
        }

        public override void PostDraw()
        {
            ImGui.PopStyleVar(2);
        }

        public override void Draw()
        {
            var iconSize = new Vector2(config.SelectedImageSize, config.SelectedImageSize);

            if (texturesLoaded && iconOnTexture != null && iconOffTexture != null)
                DrawAutoRotationImage(iconSize);
            else
                DrawAutoRotationFontAwesome(iconSize);

            HandleWindowDragging();

            if (config.ShowBurstButton)
            {
                ImGui.SameLine();
                DrawBurstButton(iconSize);
                HandleWindowDragging();
            }
        }

        private Vector2 CalculateWindowSize()
        {
            var iconSize = config.SelectedImageSize;
            var buttonCount = config.ShowBurstButton ? 2 : 1;
            var spacing = (buttonCount - 1) * ImGui.GetStyle().ItemSpacing.X;

            return new Vector2(
                iconSize * buttonCount + spacing,
                iconSize);
        }

        private void TriggerAutoRotationToggle()
        {
            _ = Task.Run(async () =>
            {
                try { await wrathService.ToggleAutoRotationAsync(); }
                catch (Exception ex) { Logger.Error("Failed to toggle auto-rotation", ex); }
            });
        }

        private void DrawAutoRotationFontAwesome(Vector2 iconSize)
        {
            var enabled = wrathService.IsAutoRotationEnabled;
            var icon = enabled ? FontAwesomeIcon.Play : FontAwesomeIcon.Stop;
            var tint = enabled ? new Vector4(0.0f, 1.0f, 0.0f, 0.3f) : new Vector4(1.0f, 0.0f, 0.0f, 0.3f);

            if (DrawIconButton("##autorot", icon, iconSize, tint))
                TriggerAutoRotationToggle();

            if (config.ShowTooltips && ImGui.IsItemHovered())
                ImGui.SetTooltip($"Auto-Rotation: {(enabled ? "Enabled" : "Disabled")}\nClick to toggle");
        }

        private void DrawAutoRotationImage(Vector2 iconSize)
        {
            var enabled = wrathService.IsAutoRotationEnabled;
            var currentIcon = enabled ? iconOnTexture : iconOffTexture;
            if (currentIcon == null)
            {
                Logger.Warning($"Icon texture is null! enabled={enabled}");
                return;
            }

            ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(1.0f, 1.0f, 1.0f, 0.0f));
            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(1.0f, 1.0f, 1.0f, 0.1f));
            ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(1.0f, 1.0f, 1.0f, 0.2f));
            ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, Vector2.Zero);

            if (ImGui.ImageButton(currentIcon.Handle, iconSize))
                TriggerAutoRotationToggle();

            ImGui.PopStyleVar();
            ImGui.PopStyleColor(3);

            if (config.ShowTooltips && ImGui.IsItemHovered())
                ImGui.SetTooltip($"Auto-Rotation: {(enabled ? "Enabled" : "Disabled")}\nClick to toggle");
        }

        private void DrawBurstButton(Vector2 iconSize)
        {
            var burstHeld = wrathService.IsBurstHeld;
            FontAwesomeIcon icon;
            Vector4 tint;
            string status;

            if (burstHeld == true)
            {
                icon = FontAwesomeIcon.Pause;
                tint = new Vector4(0.85f, 0.20f, 0.20f, 0.45f);
                status = "Burst: HELD";
            }
            else if (burstHeld == false)
            {
                icon = FontAwesomeIcon.Fire;
                tint = new Vector4(1.00f, 0.55f, 0.00f, 0.50f);
                status = "Burst: ACTIVE";
            }
            else
            {
                icon = FontAwesomeIcon.Fire;
                tint = new Vector4(0.40f, 0.40f, 0.40f, 0.30f);
                status = "Burst: unknown (job not mapped or WrathCombo unavailable)";
            }

            if (DrawIconButton("##burst", icon, iconSize, tint))
            {
                try { wrathService.ToggleBurst(); }
                catch (Exception ex) { Logger.Error("Failed to toggle burst", ex); }
            }

            if (config.ShowTooltips && ImGui.IsItemHovered())
                ImGui.SetTooltip($"{status}\nClick to /wrath burst (experimental)");
        }

        private bool DrawIconButton(string id, FontAwesomeIcon icon, Vector2 size, Vector4 tint)
        {
            ImGui.PushStyleColor(ImGuiCol.Button, tint);
            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(tint.X, tint.Y, tint.Z, Math.Min(1f, tint.W + 0.15f)));
            ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(tint.X, tint.Y, tint.Z, Math.Min(1f, tint.W + 0.30f)));

            bool clicked;
            if (scaledIconFont is { Available: true })
            {
                using (scaledIconFont.Push())
                    clicked = ImGui.Button(icon.ToIconString() + id, size);
            }
            else
            {
                ImGui.PushFont(UiBuilder.IconFont);
                clicked = ImGui.Button(icon.ToIconString() + id, size);
                ImGui.PopFont();
            }

            ImGui.PopStyleColor(3);
            return clicked;
        }

        private void EnsureIconFont(int targetButtonSize)
        {
            if (loadedIconFontSize == targetButtonSize && scaledIconFont != null)
                return;

            scaledIconFont?.Dispose();

            var sizePx = MathF.Max(8f, targetButtonSize * 0.7f);
            scaledIconFont = Plugin.PluginInterface.UiBuilder.FontAtlas.NewDelegateFontHandle(
                e => e.OnPreBuild(tk =>
                {
                    var cfg = new SafeFontConfig { SizePx = sizePx };
                    tk.AddFontAwesomeIconFont(cfg);
                }));
            loadedIconFontSize = targetButtonSize;
        }

        private void HandleWindowDragging()
        {
            if (config.IsLocked)
                return;

            if (ImGui.IsItemActive() && ImGui.IsMouseDragging(ImGuiMouseButton.Left))
            {
                Vector2 dragDelta = ImGui.GetMouseDragDelta(ImGuiMouseButton.Left);
                config.WindowX += dragDelta.X;
                config.WindowY += dragDelta.Y;
                ImGui.ResetMouseDragDelta();
                wasDragging = true;
            }
            else if (wasDragging && !ImGui.IsMouseDown(ImGuiMouseButton.Left))
            {
                config.Save();
                wasDragging = false;
            }
        }

        public void SetOpen(bool open)
        {
            if (IsOpen != open)
            {
                IsOpen = open;
                Logger.Debug($"MainWindow visibility set to: {open}");
            }
        }

        public void Dispose()
        {
            scaledIconFont?.Dispose();
            scaledIconFont = null;
            Logger.Debug("MainWindow disposed");
        }
    }
}
