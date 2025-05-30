# WrathIcon Plugin - Clean Architecture

## Overview
The WrathIcon plugin has been completely restructured to follow modern software architecture principles, providing better maintainability, testability, and performance.

## Architecture Principles

### 1. **Service-Based Architecture**
- **Separation of Concerns**: Each service has a single responsibility
- **Dependency Injection**: Services are injected rather than directly instantiated
- **Interface-Based Design**: All services implement interfaces for better testability

### 2. **Event-Driven State Management**
- **Reactive Updates**: UI responds to state changes via events
- **Reduced Polling**: More efficient state monitoring (500ms vs 1.5s)
- **Thread Safety**: All Dalamud operations stay on the main thread

### 3. **Centralized Configuration**
- **Type Safety**: Strongly typed configuration with validation
- **Event Notifications**: Configuration changes trigger UI updates
- **Helper Methods**: Convenient methods for common operations

## Directory Structure

```
WrathIcon/
├── Core/
│   ├── Services/           # Business logic services
│   │   ├── IWrathService.cs       # Wrath auto-rotation interface
│   │   ├── WrathService.cs        # Wrath auto-rotation implementation
│   │   ├── ITextureService.cs     # Texture management interface
│   │   └── TextureService.cs      # Texture management implementation
│   └── WrathIPC.cs        # IPC communication with WrathCombo
├── Windows/               # UI components
│   ├── MainWindow.cs      # Main icon display window
│   └── ConfigWindow.cs    # Configuration interface
├── Utilities/             # Helper classes and utilities
│   ├── Constants.cs       # Centralized constants
│   ├── Logger.cs          # Centralized logging
│   ├── ServiceContainer.cs # Simple dependency injection
│   └── ThreadSafeExecutor.cs # Main thread safety utilities
├── Configuration.cs       # Plugin configuration
└── Plugin.cs             # Main plugin entry point
```

## Key Components

### Services

#### **IWrathService / WrathService**
- Manages Wrath auto-rotation state
- Provides event-driven state notifications
- Handles toggle operations asynchronously
- Ensures thread safety for all operations

#### **ITextureService / TextureService**
- Manages texture loading and caching
- Supports both local files and remote URLs
- Implements proper disposal patterns
- Thread-safe texture operations

### Utilities

#### **ServiceContainer**
- Simple dependency injection container
- Supports both instance and factory registration
- Automatic disposal of disposable services
- Type-safe service resolution

#### **ThreadSafeExecutor**
- Ensures all Dalamud operations run on main thread
- Provides both synchronous and asynchronous execution
- Prevents threading issues with Dalamud APIs

#### **Logger**
- Centralized logging with consistent formatting
- Supports different log levels (Info, Warning, Error, Debug)
- Includes exception logging with stack traces

#### **Constants**
- Centralizes all magic strings and configuration values
- Prevents typos and makes refactoring easier
- Documents available options (icon sizes, URLs, etc.)

## Key Improvements

### **Performance**
- **Faster State Monitoring**: 500ms intervals instead of 1.5s
- **Efficient Texture Caching**: Prevents redundant downloads
- **Event-Driven Updates**: Only redraws when necessary
- **Async Operations**: Non-blocking texture loading and state changes

### **Maintainability**
- **Clear Separation**: Business logic separated from UI
- **Consistent Patterns**: All services follow similar patterns
- **Centralized Constants**: Easy to modify URLs, timings, etc.
- **Comprehensive Logging**: Easy debugging and troubleshooting

### **Thread Safety**
- **Main Thread Enforcement**: All Dalamud operations on main thread
- **Safe Async Operations**: Proper async/await patterns
- **Event Safety**: Thread-safe event handling

### **Error Handling**
- **Graceful Degradation**: Plugin continues working if components fail
- **Detailed Logging**: Comprehensive error information
- **Exception Safety**: Try-catch blocks around all operations

## Configuration Features

### **Enhanced Settings**
- **Auto Show on Login**: Configurable window visibility
- **Debug Logging**: Toggle detailed logging
- **Window Position**: Persistent window positioning
- **Icon Size Validation**: Clamped between min/max values

### **Helper Methods**
```csharp
config.SetWindowPosition(x, y);    // Updates position and saves
config.SetImageSize(size);         // Validates and saves size
config.SetLocked(locked);          // Updates lock state and saves
```

## Usage Examples

### **Service Registration**
```csharp
services.Register<ITextureService>(() => new TextureService(TextureProvider));
services.Register<IWrathService>(() => new WrathService());
```

### **Event Handling**
```csharp
wrathService.StateChanged += OnWrathStateChanged;
config.ConfigurationChanged += OnConfigurationChanged;
```

### **Thread-Safe Operations**
```csharp
await ThreadSafeExecutor.RunOnMainThreadAsync(() => {
    // Dalamud operations here
});
```

## Benefits

1. **Easier Testing**: Services can be mocked and tested independently
2. **Better Performance**: More efficient state monitoring and UI updates
3. **Improved Reliability**: Better error handling and thread safety
4. **Easier Maintenance**: Clear structure and separation of concerns
5. **Future-Proof**: Easy to add new features and modify existing ones

## Migration Notes

- All existing functionality is preserved
- Configuration is automatically migrated
- No user-visible changes to behavior
- Improved performance and reliability
- Better error messages and logging

This architecture provides a solid foundation for future enhancements while maintaining the existing user experience. 