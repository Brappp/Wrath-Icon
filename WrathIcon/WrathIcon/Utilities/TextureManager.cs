using Dalamud.Interface.Textures.TextureWraps;
using Dalamud.Plugin.Services;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace WrathIcon.Utilities
{
    public class TextureManager
    {
        private readonly ITextureProvider _textureProvider;
        private static readonly HttpClient HttpClient = new();
        private static readonly ConcurrentDictionary<string, IDalamudTextureWrap?> Cache = new();

        public TextureManager(ITextureProvider textureProvider)
        {
            _textureProvider = textureProvider;
        }

        public async Task<IDalamudTextureWrap?> LoadTextureAsync(string path)
        {
            if (Cache.TryGetValue(path, out var cachedTexture))
            {
                return cachedTexture;
            }

            try
            {
                if (path.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                {
                    var response = await HttpClient.GetAsync(path);
                    response.EnsureSuccessStatusCode();
                    var imageBytes = await response.Content.ReadAsByteArrayAsync();

                    var texture = await _textureProvider.CreateFromImageAsync(imageBytes);
                    Cache[path] = texture;
                    return texture;
                }
                else if (File.Exists(path))
                {
                    var texture = _textureProvider.GetFromFile(path) as IDalamudTextureWrap;
                    Cache[path] = texture;
                    return texture;
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to load texture from {path}", ex);
            }

            return null;
        }
    }
}
