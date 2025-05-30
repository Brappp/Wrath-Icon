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
        private bool disposed = false;

        public bool IsAutoRotationEnabled => lastKnownState;
        public bool IsInitialized => WrathIPC.IsInitialized;

        public event Action<bool>? StateChanged;

        public WrathService()
        {
            Logger.Info("Initializing WrathService");
            
            // Initialize the current state
            ThreadSafeExecutor.RunOnMainThread(() =>
            {
                lastKnownState = WrathIPC.GetAutoRotationState();
            });

            // Start monitoring state changes
            stateCheckTimer = new Timer(CheckStateCallback, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(Constants.StateCheckIntervalMs));
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
            if (!IsInitialized)
            {
                Logger.Warning("Attempted to toggle auto-rotation but IPC is not initialized");
                return false;
            }

            try
            {
                Logger.Debug($"Toggling auto-rotation from {(lastKnownState ? "enabled" : "disabled")}");

                // Execute the toggle command on the main thread
                await ThreadSafeExecutor.RunOnMainThreadAsync(() =>
                {
                    ExecuteChatCommand(Constants.WrathAutoCommand);
                });

                // Wait for the state to update
                await Task.Delay(Constants.ToggleDelayMs);

                // Force a state check
                await ThreadSafeExecutor.RunOnMainThreadAsync(() =>
                {
                    CheckState();
                });

                Logger.Debug($"Auto-rotation toggled to {(lastKnownState ? "enabled" : "disabled")}");
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

            ThreadSafeExecutor.RunOnMainThread(CheckState);
        }

        private void CheckState()
        {
            if (disposed || !IsInitialized)
                return;

            try
            {
                // Check auto-rotation state
                var currentState = WrathIPC.GetAutoRotationState();
                if (currentState != lastKnownState)
                {
                    Logger.Debug($"Wrath state changed: {lastKnownState} -> {currentState}");
                    lastKnownState = currentState;
                    StateChanged?.Invoke(currentState);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error checking Wrath state", ex);
            }
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
            StateChanged = null;
            disposed = true;
        }
    }
} 