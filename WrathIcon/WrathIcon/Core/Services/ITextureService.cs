using System;
using System.Threading.Tasks;
using Dalamud.Interface.Textures.TextureWraps;

namespace WrathIcon.Core.Services
{
    public interface ITextureService : IDisposable
    {
        Task<IDalamudTextureWrap?> LoadTextureAsync(string path);
        Task<IDalamudTextureWrap?> LoadIconTextureAsync(string iconType);
        void ClearCache();
    }
} 