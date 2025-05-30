using Dalamud.Interface.Windowing;
using ImGuiNET;
using System.Linq;
using System;
using WrathIcon.Utilities;

namespace WrathIcon.Windows
{
    public class ConfigWindow : Window
    {
        private readonly Configuration config;

        public ConfigWindow(Configuration config)
            : base(Constants.ConfigWindowName)
        {
            this.config = config ?? throw new ArgumentNullException(nameof(config));
            
            Size = new System.Numerics.Vector2(450, 300);
            SizeCondition = ImGuiCond.FirstUseEver;
            
            Logger.Debug("ConfigWindow initialized");
        }

        public override void Draw()
        {
            ImGui.Text("Wrath Icon Configuration");
            ImGui.Separator();

            DrawIconSizeSettings();
            ImGui.Spacing();
            
            DrawWindowSettings();
            ImGui.Spacing();
            
            DrawBehaviorSettings();
            ImGui.Spacing();
            
            DrawStatusDisplay();
        }

        private void DrawIconSizeSettings()
        {
            ImGui.Text("Icon Settings");
            
            // Dropdown for selecting image size
            ImGui.Text("Icon Size:");
            int currentIndex = Array.IndexOf(Constants.AvailableIconSizes, config.SelectedImageSize);
            if (currentIndex == -1) currentIndex = 2; // Default to 32 if not found
            
            if (ImGui.Combo("##IconSizeDropdown", ref currentIndex, 
                Constants.AvailableIconSizes.Select(s => $"{s}x{s}").ToArray(), 
                Constants.AvailableIconSizes.Length))
            {
                config.SetImageSize(Constants.AvailableIconSizes[currentIndex]);
            }

            if (config.ShowTooltips && ImGui.IsItemHovered())
            {
                ImGui.SetTooltip("Choose the size of the Wrath icon displayed on the UI.");
            }
        }

        private void DrawWindowSettings()
        {
            ImGui.Text("Window Settings");
            
            // Lock/Unlock checkbox
            bool isLocked = config.IsLocked;
            if (ImGui.Checkbox("Lock Window Position", ref isLocked))
            {
                config.SetLocked(isLocked);
            }

            if (config.ShowTooltips && ImGui.IsItemHovered())
            {
                ImGui.SetTooltip("Lock the window in place to prevent accidental movement.");
            }

            // Reset position button
            if (ImGui.Button("Reset Window Position"))
            {
                config.SetWindowPosition(Constants.DefaultWindowX, Constants.DefaultWindowY);
                Logger.Info("Window position reset to default");
            }

            if (config.ShowTooltips && ImGui.IsItemHovered())
            {
                ImGui.SetTooltip("Reset the window position to the default location.");
            }
        }

        private void DrawBehaviorSettings()
        {
            ImGui.Text("Behavior Settings");
            
            // Auto show on login
            bool autoShow = config.AutoShowOnLogin;
            if (ImGui.Checkbox("Show on Login", ref autoShow))
            {
                config.AutoShowOnLogin = autoShow;
                config.Save();
            }

            if (config.ShowTooltips && ImGui.IsItemHovered())
            {
                ImGui.SetTooltip("Automatically show the icon when you log into the game.");
            }

            // Show tooltips checkbox
            bool showTooltips = config.ShowTooltips;
            if (ImGui.Checkbox("Show Tooltips", ref showTooltips))
            {
                config.ShowTooltips = showTooltips;
                config.Save();
            }

            if (config.ShowTooltips && ImGui.IsItemHovered())
            {
                ImGui.SetTooltip("Enable or disable helpful tooltips throughout the interface.");
            }
        }

        private void DrawStatusDisplay()
        {
            ImGui.Text("Current Status:");
            ImGui.Text($"Position: ({config.WindowX:F1}, {config.WindowY:F1})");
            ImGui.Text($"Icon Size: {config.SelectedImageSize}x{config.SelectedImageSize}");
            ImGui.Text($"Window Locked: {(config.IsLocked ? "Yes" : "No")}");
            ImGui.Text($"Tooltips: {(config.ShowTooltips ? "Enabled" : "Disabled")}");
        }

        public void Dispose()
        {
            Logger.Debug("ConfigWindow disposed");
        }
    }
}
