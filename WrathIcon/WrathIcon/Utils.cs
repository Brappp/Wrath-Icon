using Dalamud.Interface.Textures.TextureWraps;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net.Http;
using System.Threading;

namespace WrathIcon
{
    public static class Utils
    {
        private static readonly ConcurrentDictionary<string, ImageLoadingResult> CachedTextures = new();
        private static volatile bool ThreadRunning = false;
        private static readonly HttpClient httpClient = new();

        public static bool TryGetTextureWrap(string url, out IDalamudTextureWrap? textureWrap)
        {
            if (!CachedTextures.TryGetValue(url, out var result))
            {
                result = new ImageLoadingResult();
                CachedTextures[url] = result;
                BeginThreadIfNotRunning();
            }

            textureWrap = result.TextureWrap;
            return textureWrap != null;
        }

        private static void BeginThreadIfNotRunning()
        {
            if (ThreadRunning) return;

            ThreadRunning = true;
            new Thread(() =>
            {
                while (CachedTextures.Any(x => !x.Value.IsCompleted))
                {
                    var pending = CachedTextures.FirstOrDefault(x => !x.Value.IsCompleted);
                    if (pending.Key != null)
                    {
                        pending.Value.IsCompleted = true;

                        if (pending.Key.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                        {
                            try
                            {
                                var response = httpClient.GetAsync(pending.Key).Result;
                                response.EnsureSuccessStatusCode();
                                var imageBytes = response.Content.ReadAsByteArrayAsync().Result;

                                // Use the correct type for texture
                                var texture = Plugin.TextureProvider.CreateFromImageAsync(imageBytes).Result as IDalamudTextureWrap;

                                if (texture != null)
                                {
                                    pending.Value.TextureWrap = texture;
                                }
                            }
                            catch (Exception ex)
                            {
                                Plugin.PluginLog.Error($"[Utils] Failed to load texture from {pending.Key}: {ex.Message}");
                            }
                        }
                    }

                    Thread.Sleep(100);
                }

                ThreadRunning = false;
            })
            {
                IsBackground = true
            }.Start();
        }
    }

    internal class ImageLoadingResult
    {
        internal IDalamudTextureWrap? TextureWrap;
        internal bool IsCompleted = false;
    }
}
