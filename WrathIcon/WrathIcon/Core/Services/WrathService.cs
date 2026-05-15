using System;
using System.Threading;
using System.Threading.Tasks;
using WrathIcon.Utilities;

namespace WrathIcon.Core.Services
{
    public class WrathService : IWrathService
    {
        private readonly Timer stateCheckTimer;
        private bool lastKnownState;
        private bool? lastKnownBurstHeld;
        private bool? lastKnownWrathAvailable;
        private bool disposed = false;

        public bool IsAutoRotationEnabled => lastKnownState;
        public bool? IsBurstHeld => lastKnownBurstHeld;
        public bool IsInitialized => WrathIPC.IsInitialized;

        public WrathService()
        {
            Logger.Info("Initializing WrathService");
            stateCheckTimer = new Timer(CheckStateCallback, null, Timeout.Infinite, Timeout.Infinite);
        }

        public void StartMonitoring()
        {
            if (disposed)
                return;

            Logger.Debug("Starting Wrath state monitoring");
            stateCheckTimer.Change(TimeSpan.Zero, TimeSpan.FromMilliseconds(Constants.StateCheckIntervalMs));
        }

        public void StopMonitoring()
        {
            if (disposed)
                return;

            Logger.Debug("Stopping Wrath state monitoring");
            stateCheckTimer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        public async Task<bool> ToggleAutoRotationAsync()
        {
            if (!IsInitialized || !WrathIPC.IsWrathAvailable)
            {
                Logger.Warning("Attempted to toggle auto-rotation but WrathCombo is not available");
                return false;
            }

            try
            {
                Logger.Debug($"Toggling auto-rotation from {(lastKnownState ? "enabled" : "disabled")}");

                await Tasks.RunOnMainThreadAsync(() =>
                {
                    ExecuteChatCommand(Constants.WrathAutoCommand);
                });

                return true;
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to toggle auto-rotation", ex);
                return false;
            }
        }

        private void CheckStateCallback(object? state)
        {
            if (disposed)
                return;

            Tasks.RunOnMainThread(CheckState);
        }

        private void CheckState()
        {
            if (disposed || !IsInitialized)
                return;

            try
            {
                var available = WrathIPC.IsWrathAvailable;
                if (available != lastKnownWrathAvailable)
                {
                    Logger.Info($"WrathCombo availability changed: {lastKnownWrathAvailable} -> {available}");
                    if (available)
                        WrathReflection.Reset();
                    lastKnownWrathAvailable = available;
                }

                if (!available)
                {
                    lastKnownState = false;
                    lastKnownBurstHeld = null;
                    return;
                }

                var currentState = WrathIPC.GetAutoRotationState();
                if (currentState != lastKnownState)
                {
                    Logger.Debug($"Wrath state changed: {lastKnownState} -> {currentState}");
                    lastKnownState = currentState;
                }

                var jobId = Plugin.ObjectTable.LocalPlayer?.ClassJob.RowId;
                lastKnownBurstHeld = jobId.HasValue ? WrathIPC.IsBurstHeld(jobId.Value) : null;
            }
            catch (Exception ex)
            {
                Logger.Error("Error checking Wrath state", ex);
            }
        }

        public void ToggleBurst()
        {
            if (!IsInitialized || !WrathIPC.IsWrathAvailable)
            {
                Logger.Warning("Attempted to toggle burst but WrathCombo is not available");
                return;
            }

            Tasks.RunOnMainThread(() => ExecuteChatCommand(Constants.WrathBurstCommand));
        }

        private static void ExecuteChatCommand(string command)
        {
            if (Plugin.CommandManager != null)
            {
                Plugin.CommandManager.ProcessCommand(command);
                Logger.Debug($"Executed chat command: {command}");
            }
            else
            {
                Logger.Error("Failed to execute chat command - CommandManager is null");
            }
        }

        public void Dispose()
        {
            if (disposed)
                return;

            Logger.Debug("Disposing WrathService");
            
            stateCheckTimer?.Dispose();
            disposed = true;
        }
    }
} 