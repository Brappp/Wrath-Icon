using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Plugin;
using Dalamud.Plugin.Ipc;
using Dalamud.Plugin.Ipc.Exceptions;
using WrathIcon.Utilities;

namespace WrathIcon.Core
{
    public static class WrathIPC
    {
        private static IDalamudPluginInterface PluginInterface = null!;
        private static ICallGateSubscriber<bool>? GetAutoRotationStateSubscriber;
        private static ICallGateSubscriber<string, Dictionary<string, bool>?>? GetComboStateSubscriber;

        public static bool IsInitialized { get; private set; } = false;
        private static bool? lastLoggedState = null;

        /// <summary>True when WrathCombo's IPC endpoints are currently registered (i.e. the plugin is loaded/enabled).</summary>
        public static bool IsWrathAvailable
        {
            get
            {
                if (!IsInitialized || GetComboStateSubscriber == null)
                    return false;

                try
                {
                    return GetComboStateSubscriber.HasFunction;
                }
                catch
                {
                    return false;
                }
            }
        }

        public static void Init(IDalamudPluginInterface pluginInterface)
        {
            if (pluginInterface == null)
            {
                Logger.Error("PluginInterface is null. IPC cannot be initialized.");
                return;
            }

            try
            {
                PluginInterface = pluginInterface;

                GetAutoRotationStateSubscriber = PluginInterface.GetIpcSubscriber<bool>(Constants.IpcGetAutoRotationState);
                GetComboStateSubscriber = PluginInterface.GetIpcSubscriber<string, Dictionary<string, bool>?>(Constants.IpcGetComboState);

                IsInitialized = true;
                Logger.Info("IPC successfully initialized");
            }
            catch (Exception e)
            {
                Logger.Error("Failed to initialize IPC", e);
            }
        }

        /// <summary>Returns true if burst is currently held (the job's first burst preset is disabled), false if running, or null if unknown.</summary>
        public static bool? IsBurstHeld(uint jobId)
        {
            if (!IsInitialized || GetComboStateSubscriber == null)
                return null;

            var map = WrathReflection.BurstPresetsByJobId;
            if (map == null || !map.TryGetValue(jobId, out var presets) || presets.Length == 0)
                return null;

            try
            {
                var state = GetComboStateSubscriber.InvokeFunc(presets[0]);
                if (state == null || state.Count == 0)
                    return null;

                var enabled = state.First().Value;
                return !enabled;
            }
            catch (IpcNotReadyError)
            {
                return null;
            }
            catch (Exception ex)
            {
                Logger.Error($"Error querying burst state for {presets[0]}", ex);
                return null;
            }
        }

        public static bool GetAutoRotationState()
        {
            if (!IsInitialized || GetAutoRotationStateSubscriber == null)
            {
                Logger.Warning("GetAutoRotationState attempted but IPC is not initialized");
                return false;
            }

            try
            {
                bool state = GetAutoRotationStateSubscriber.InvokeFunc();

                if (state != lastLoggedState)
                {
                    Logger.Debug($"Auto-Rotation state: {(state ? "Enabled" : "Disabled")}");
                    lastLoggedState = state;
                }

                return state;
            }
            catch (IpcNotReadyError)
            {
                return false;
            }
            catch (Exception e)
            {
                Logger.Error("Error retrieving Auto-Rotation state", e);
                return false;
            }
        }
    }
}
