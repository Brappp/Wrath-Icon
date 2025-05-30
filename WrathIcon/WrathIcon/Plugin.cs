using Dalamud.Game.ClientState;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using WrathIcon.Core;
using WrathIcon.Core.Services;
using WrathIcon.Utilities;
using WrathIcon.Windows;
using Dalamud.IoC;
using Dalamud.Plugin.Services;
using Dalamud.Game.Command;
using Dalamud.Game;
using System;

namespace WrathIcon
{
    public sealed class Plugin : IDalamudPlugin
    {
        public readonly WindowSystem WindowSystem = new(Constants.WindowSystemName);
        private readonly ServiceContainer services;
        
        private MainWindow? mainWindow;
        private ConfigWindow? configWindow;
        private Configuration? config;
        private IWrathService? wrathService;
        private ITextureService? textureService;
        
        private bool isInitialized = false;
        private bool disposed = false;

        [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
        [PluginService] internal static ITextureProvider TextureProvider { get; private set; } = null!;
        [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;
        [PluginService] internal static IPluginLog PluginLog { get; private set; } = null!;
        [PluginService] internal static IClientState ClientState { get; private set; } = null!;
        [PluginService] internal static IFramework Framework { get; private set; } = null!;

        public string Name => Constants.PluginName;

        public Plugin()
        {
            try
            {
                Logger.Info("Initializing WrathIcon Plugin...");
                
                services = new ServiceContainer();
                InitializeServices();
                InitializeIPC();
                
                if (ClientState.IsLoggedIn)
                {
                    Logger.Debug("Already logged in, initializing immediately");
                    Initialize();
                }
                else
                {
                    Logger.Debug("Waiting for login...");
                    Framework.Update += OnFrameworkUpdate;
                }

                RegisterEventHandlers();
                Logger.Info("WrathIcon Plugin constructor completed");
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to initialize plugin", ex);
                throw;
            }
        }

        private void InitializeServices()
        {
            try
            {
                // Load and register configuration
                config = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
                config.Initialize(PluginInterface);
                services.Register(config);

                // Register services
                services.Register<ITextureService>(() => new TextureService(TextureProvider));
                services.Register<IWrathService>(() => new WrathService());

                Logger.Debug("Services registered successfully");
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to initialize services", ex);
                throw;
            }
        }

        private void InitializeIPC()
        {
            try
            {
                WrathIPC.Init(PluginInterface);
                
                if (WrathIPC.IsInitialized)
                    Logger.Info("WrathIPC initialized successfully");
                else
                    Logger.Warning("WrathIPC failed to initialize - Wrath plugin may not be available");
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to initialize IPC", ex);
            }
        }

        private void RegisterEventHandlers()
        {
            ClientState.Login += OnLogin;
            ClientState.Logout += OnLogout;
            PluginInterface.UiBuilder.OpenConfigUi += OpenConfigWindow;
        }

        private void OnFrameworkUpdate(IFramework framework)
        {
            if (ClientState.IsLoggedIn && !isInitialized)
            {
                Logger.Debug("Login detected via framework update, initializing");
                Initialize();
                Framework.Update -= OnFrameworkUpdate;
            }
        }

        private void Initialize()
        {
            if (isInitialized || disposed)
                return;

            try
            {
                Logger.Info("Initializing plugin components...");

                // Get services
                textureService = services.Get<ITextureService>();
                wrathService = services.Get<IWrathService>();

                // Initialize windows
                InitializeWindows();
                
                // Register command
                RegisterCommand();
                
                // Register UI handlers
                RegisterUIHandlers();

                // Start monitoring
                wrathService.StartMonitoring();

                isInitialized = true;
                Logger.Info("WrathIcon Plugin fully initialized");
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to initialize plugin components", ex);
            }
        }

        private void InitializeWindows()
        {
            if (config == null || textureService == null || wrathService == null)
            {
                throw new InvalidOperationException("Required services not available");
            }

            mainWindow = new MainWindow(config, textureService, wrathService);
            configWindow = new ConfigWindow(config);

            WindowSystem.AddWindow(mainWindow);
            WindowSystem.AddWindow(configWindow);

            Logger.Debug("Windows initialized and added to WindowSystem");
        }

        private void RegisterCommand()
        {
            CommandManager.AddHandler(Constants.CommandName, new CommandInfo(OnCommand)
            {
                HelpMessage = "Toggle Wrath Status Icon UI"
            });
            
            Logger.Debug($"Command {Constants.CommandName} registered");
        }

        private void RegisterUIHandlers()
        {
            PluginInterface.UiBuilder.Draw += DrawUI;
            PluginInterface.UiBuilder.OpenMainUi += OpenMainWindow;
            PluginInterface.UiBuilder.OpenConfigUi += OpenConfigWindow;
            
            Logger.Debug("UI handlers registered");
        }

        private void OnCommand(string command, string args)
        {
            try
            {
                if (mainWindow != null)
                {
                    bool newState = !mainWindow.IsOpen;
                    mainWindow.SetOpen(newState);
                    Logger.Debug($"Command executed: window visibility set to {newState}");
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error executing command", ex);
            }
        }

        private void OpenConfigWindow()
        {
            try
            {
                if (configWindow != null)
                {
                    configWindow.IsOpen = true;
                    Logger.Debug("Config window opened");
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error opening config window", ex);
            }
        }

        private void OpenMainWindow()
        {
            try
            {
                if (mainWindow != null)
                {
                    mainWindow.SetOpen(true);
                    Logger.Debug("Main window opened");
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error opening main window", ex);
            }
        }

        private void OnLogin()
        {
            try
            {
                if (!isInitialized)
                {
                    Logger.Info("Login detected, initializing plugin");
                    Initialize();
                }

                if (mainWindow != null && config?.AutoShowOnLogin == true)
                {
                    mainWindow.SetOpen(true);
                    Logger.Debug("Auto-showing main window on login");
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error handling login", ex);
            }
        }

        private void OnLogout(int type, int code)
        {
            try
            {
                Logger.Info($"Logout detected. Type: {type}, Code: {code}");

                if (mainWindow != null)
                {
                    mainWindow.SetOpen(false);
                    Logger.Debug("Hiding main window on logout");
                }

                wrathService?.StopMonitoring();
            }
            catch (Exception ex)
            {
                Logger.Error("Error handling logout", ex);
            }
        }

        private void DrawUI()
        {
            try
            {
                WindowSystem.Draw();
            }
            catch (Exception ex)
            {
                Logger.Error("Error drawing UI", ex);
            }
        }

        public void Dispose()
        {
            if (disposed)
                return;

            try
            {
                Logger.Info("Disposing WrathIcon Plugin...");

                // Unregister UI handlers
                PluginInterface.UiBuilder.Draw -= DrawUI;
                PluginInterface.UiBuilder.OpenMainUi -= OpenMainWindow;
                PluginInterface.UiBuilder.OpenConfigUi -= OpenConfigWindow;

                // Unregister command
                CommandManager.RemoveHandler(Constants.CommandName);

                // Unregister event handlers
                ClientState.Login -= OnLogin;
                ClientState.Logout -= OnLogout;
                Framework.Update -= OnFrameworkUpdate;

                // Clean up windows
                WindowSystem.RemoveAllWindows();
                mainWindow?.Dispose();
                configWindow?.Dispose();

                // Dispose services
                services?.Dispose();

                disposed = true;
                isInitialized = false;

                Logger.Info("WrathIcon Plugin disposed successfully");
            }
            catch (Exception ex)
            {
                Logger.Error("Error during plugin disposal", ex);
            }
        }
    }
}
