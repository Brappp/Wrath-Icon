using System;

namespace WrathIcon.Utilities
{
    public static class Logger
    {
        public static void Info(string message) => Plugin.PluginLog.Information($"[WrathIcon] {message}");
        public static void Warning(string message) => Plugin.PluginLog.Warning($"[WrathIcon] {message}");
        public static void Error(string message, Exception? ex = null) => Plugin.PluginLog.Error(ex, $"[WrathIcon] {message}");
        public static void Debug(string message) => Plugin.PluginLog.Debug($"[WrathIcon] {message}");
    }
} 