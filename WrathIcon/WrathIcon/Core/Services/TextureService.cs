using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;
using Dalamud.Interface.Textures.TextureWraps;
using Dalamud.Plugin.Services;
using WrathIcon.Utilities;

namespace WrathIcon.Core.Services
{
    public class TextureService : ITextureService
    {
        private readonly ITextureProvider textureProvider;
        private readonly ConcurrentDictionary<string, IDalamudTextureWrap?> cache = new();
        private bool disposed = false;

        public TextureService(ITextureProvider textureProvider)
        {
            this.textureProvider = textureProvider ?? throw new ArgumentNullException(nameof(textureProvider));
        }

        public async Task<IDalamudTextureWrap?> LoadTextureAsync(string path)
        {
            if (disposed)
                throw new ObjectDisposedException(nameof(TextureService));

            if (string.IsNullOrWhiteSpace(path))
            {
                Logger.Warning("Attempted to load texture with null or empty path");
                return null;
            }

            Logger.Debug($"Attempting to load texture from: {path}");

            if (cache.TryGetValue(path, out var cachedTexture))
            {
                Logger.Debug($"Returning cached texture for: {path}");
                return cachedTexture;
            }

            try
            {
                IDalamudTextureWrap? texture = null;

                if (File.Exists(path))
                {
                    var fileInfo = new FileInfo(path);
                    Logger.Debug($"File exists: {path} (Size: {fileInfo.Length} bytes)");
                    
                    // Check if it's actually a valid image file
                    if (fileInfo.Length < 10)
                    {
                        Logger.Error($"File too small to be a valid PNG: {path} ({fileInfo.Length} bytes)");
                        return null;
                    }

                    // Use QoLBar's approach: CreateFromImageAsync with File.OpenRead
                    try
                    {
                        Logger.Debug($"Loading texture using CreateFromImageAsync: {path}");
                        using var fileStream = File.OpenRead(path);
                        texture = await textureProvider.CreateFromImageAsync(fileStream);
                        Logger.Debug($"Successfully loaded texture using CreateFromImageAsync: {path}");
                    }
                    catch (Exception ex)
                    {
                        Logger.Error($"Exception in CreateFromImageAsync: {ex.Message}");
                        texture = null;
                    }
                    
                    if (texture != null)
                    {
                        Logger.Info($"Successfully loaded texture from local file: {path}");
                    }
                    else
                    {
                        Logger.Error($"CreateFromImageAsync returned null for file: {path}");
                    }
                }
                else
                {
                    Logger.Error($"Texture file not found: {path}");
                    Logger.Debug($"Directory exists: {Directory.Exists(Path.GetDirectoryName(path))}");
                    return null;
                }

                cache[path] = texture;
                return texture;
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to load texture from {path}", ex);
                cache[path] = null; // Cache the failure to avoid repeated attempts
                return null;
            }
        }

        public async Task<IDalamudTextureWrap?> LoadIconTextureAsync(string iconType)
        {
            string localPath;

            // Determine paths based on icon type
            if (iconType.Equals("on", StringComparison.OrdinalIgnoreCase))
            {
                localPath = Constants.IconOnPath;
            }
            else if (iconType.Equals("off", StringComparison.OrdinalIgnoreCase))
            {
                localPath = Constants.IconOffPath;
            }
            else
            {
                Logger.Error($"Unknown icon type: {iconType}");
                return null;
            }

            Logger.Debug($"Loading {iconType} icon from local file: {localPath}");
            return await LoadTextureAsync(localPath);
        }

        public void ClearCache()
        {
            Logger.Info("Clearing texture cache");
            
            foreach (var texture in cache.Values)
            {
                texture?.Dispose();
            }
            
            cache.Clear();
        }

        public void Dispose()
        {
            if (disposed)
                return;

            ClearCache();
            disposed = true;
            
            Logger.Debug("TextureService disposed");
        }
    }
} 