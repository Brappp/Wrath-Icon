using Dalamud.Interface.Windowing;
using ImGuiNET;
using System.Linq;
using System;

namespace WrathIcon
{
    public class ConfigWindow : Window
    {
        private readonly Configuration config;

        public ConfigWindow(Configuration config)
            : base("Wrath Icon Config")
        {
            this.config = config;
        }

        public override void Draw()
        {
            ImGui.Text("Wrath Icon Configuration");

            // Dropdown for selecting image size
            ImGui.Text("Select Icon Size:");
            int[] sizes = { 32, 48, 64, };
            int currentIndex = Array.IndexOf(sizes, config.SelectedImageSize);
            if (ImGui.Combo("##IconSizeDropdown", ref currentIndex, sizes.Select(s => $"{s}x{s}").ToArray(), sizes.Length))
            {
                config.SelectedImageSize = sizes[currentIndex];
                config.Save();
            }

            if (ImGui.IsItemHovered())
            {
                ImGui.SetTooltip("Choose the size of the Wrath icon displayed on the UI.");
            }

            // Lock/Unlock checkbox
            bool isLocked = config.IsLocked;
            if (ImGui.Checkbox("Lock Window", ref isLocked))
            {
                config.IsLocked = isLocked;
                config.Save();
            }

            if (ImGui.IsItemHovered())
            {
                ImGui.SetTooltip("Lock the window in place to prevent accidental movement.");
            }
        }
    }
}
