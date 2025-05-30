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
        
        // Local Texture Paths - using images directory like WrathCombo
        public static string IconOnPath 
        { 
            get 
            {
                var assemblyDir = Plugin.PluginInterface.AssemblyLocation.Directory?.FullName ?? "";
                var path = Path.Combine(assemblyDir, "images", "icon-on.png");
                Logger.Debug($"IconOnPath: Assembly={assemblyDir}, Resolved={path}");
                return path;
            }
        }
        
        public static string IconOffPath 
        { 
            get 
            {
                var assemblyDir = Plugin.PluginInterface.AssemblyLocation.Directory?.FullName ?? "";
                var path = Path.Combine(assemblyDir, "images", "icon-off.png");
                Logger.Debug($"IconOffPath: Assembly={assemblyDir}, Resolved={path}");
                return path;
            }
        }
        
        // IPC Method Names for WrathCombo integration
        public const string IpcGetAutoRotationState = "WrathCombo.GetAutoRotationState";
        public const string IpcSetAutoRotationState = "WrathCombo.SetAutoRotationState";
        
        // Chat Commands
        public const string WrathAutoCommand = "/wrath auto";
        
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