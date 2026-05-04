using System.IO;

namespace WrathIcon.Utilities
{
    public static class Constants
    {
        // Plugin Information
        public const string PluginName = "WrathIcon";
        public const string CommandName = "/wrathicon";
        public const string WindowSystemName = "Wrath Status Icon";
        
        // Window Names
        public const string MainWindowName = "WrathIconMainWindow";
        public const string ConfigWindowName = "Wrath Icon Configuration";
        
        private static string? iconOnPath;
        private static string? iconOffPath;

        public static string IconOnPath => iconOnPath ??= ResolveIconPath("icon-on.png");
        public static string IconOffPath => iconOffPath ??= ResolveIconPath("icon-off.png");

        private static string ResolveIconPath(string fileName)
        {
            var assemblyDir = Plugin.PluginInterface.AssemblyLocation.Directory?.FullName ?? "";
            return Path.Combine(assemblyDir, "images", fileName);
        }

        // IPC Method Names for WrathCombo integration
        public const string IpcGetAutoRotationState = "WrathCombo.GetAutoRotationState";
        public const string IpcGetComboState = "WrathCombo.GetComboState";
        
        // Chat Commands
        public const string WrathAutoCommand = "/wrath auto";
        public const string WrathBurstCommand = "/wrath burst";
        
        // Timing
        public const int StateCheckIntervalMs = 500;
        public const int ToggleDelayMs = 1000;
        
        // UI Defaults
        public const int DefaultIconSize = 64;
        public const float DefaultWindowX = 100.0f;
        public const float DefaultWindowY = 100.0f;
        public const int MinIconSize = 16;
        public const int MaxIconSize = 256;
        
        // Available Icon Sizes
        public static readonly int[] AvailableIconSizes = { 16, 24, 32, 48, 64, 80, 96, 128 };
    }
} 