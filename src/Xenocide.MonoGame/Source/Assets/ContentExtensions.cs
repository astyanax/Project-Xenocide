using System.IO;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace ProjectXenocide.Assets
{
    public static class ContentExtensions
    {
        public static T Load<T>(this ContentManager content, ShaderId id)
        {
            return content.Load<T>(AssetRegistry.ShaderPath(id));
        }

        public static T Load<T>(this ContentManager content, FontId id)
        {
            return content.Load<T>(AssetRegistry.FontPath(id));
        }

        public static Texture2D LoadTexture(this ContentManager content, TextureId id)
        {
            return content.Load<Texture2D>(AssetRegistry.TextureContentPath(id));
        }

        public static Texture2D LoadTextureFromFile(GraphicsDevice device, TextureId id)
        {
            var path = AssetRegistry.TextureFilePath(id);
            using (var fs = File.OpenRead(path))
                return Texture2D.FromStream(device, fs);
        }

        public static Texture2D LoadUIBackground(GraphicsDevice device, UIBackgroundId id)
        {
            var path = AssetRegistry.UIBackgroundPath(id);
            return Texture2D.FromFile(device, path);
        }
    }
}
