using System;
using System.IO;

using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using NLog;

using ProjectXenocide.Model;
using ProjectXenocide.Utils;

namespace ProjectXenocide.UI.Scenes.Geoscape.Skybox
{
    /// <summary>
    /// Chooses and builds the sky renderer for the configured <see cref="SkyboxMode"/>.
    /// </summary>
    /// <remarks>
    /// Both strategies consume the same equirectangular panorama, so the source
    /// art never changes - only how it is projected. Cubemap baking is deferred
    /// until it is actually requested, so the default (equirectangular) path pays
    /// no conversion cost.
    /// </remarks>
    public static class SkyboxFactory
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private const string PanoramaPath = @"Content/Textures/Geoscape/skybox.png";
        private const int BakedCubeFaceSize = 1024;

        /// <summary>Build the sky renderer for the current settings.</summary>
        public static ISkybox Create(ContentManager content, GraphicsDevice device)
        {
            var sphere = new SkyboxSphere(device);
            Texture2D panorama = LoadPanorama(device);

            if (ResolveMode(SkyboxSettings.Mode) == SkyboxMode.Cubemap)
            {
                try
                {
                    Effect cubeEffect = content.Load<Effect>(@"Shaders/skyboxcube");
                    TextureCube cube = CubeMapBaker.Bake(device, panorama, BakedCubeFaceSize);
                    Logger.Info("Skybox: using baked cubemap ({0}px faces)", BakedCubeFaceSize);
                    return new CubemapSkybox(cubeEffect, cube, sphere);
                }
                catch (Exception ex)
                {
                    Logger.Warn(ex, "Skybox: cubemap unavailable, falling back to equirectangular");
                }
            }

            Effect effect = content.Load<Effect>(@"Shaders/skybox");
            return new EquirectangularSkybox(effect, panorama, sphere);
        }

        /// <summary>
        /// Resolve <see cref="SkyboxMode.Auto"/>. The equirectangular sphere is
        /// the default because it needs no conversion and matches the source art
        /// exactly; Auto is the extension point if a higher-quality cube source
        /// is ever added.
        /// </summary>
        private static SkyboxMode ResolveMode(SkyboxMode mode)
        {
            return (mode == SkyboxMode.Auto) ? SkyboxMode.Equirectangular : mode;
        }

        private static Texture2D LoadPanorama(GraphicsDevice device)
        {
            if (ContentCache.TryGetTexture(PanoramaPath, out var cached))
            {
                return cached;
            }

            using (var stream = File.OpenRead(PanoramaPath))
            {
                var texture = Texture2D.FromStream(device, stream);
                ContentCache.StoreTexture(PanoramaPath, texture);
                return texture;
            }
        }
    }
}
