using System;
using System.Collections.Generic;
using System.IO;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using NLog;

namespace ProjectXenocide.Utils
{
    /// <summary>
    /// Stores a preloaded 3D model paired with its precomputed world-transform scaling matrix.
    ///
    /// Each model in X-Net has vastly different dimensions — a soldier is a few units tall while
    /// an aircraft spans tens of units. To display them consistently at the same visible size in the
    /// fixed viewport, a scaling matrix is computed that translates and uniformly scales the model
    /// so its bounding sphere fits exactly within a unit sphere (radius = 1) centered at the origin.
    ///
    /// Computing this matrix requires walking all mesh sub-spheres with bone transforms applied
    /// (Util.CalcBoundingSphere), which is expensive for complex models. Storing the precomputed
    /// matrix alongside the model avoids recomputing it every time the model is displayed.
    /// </summary>
    public struct CachedModel
    {
        public Microsoft.Xna.Framework.Graphics.Model Model { get; set; }
        /// <summary>World transform that translates and uniformly scales the model to fit a unit sphere.</summary>
        public Matrix ScalingMatrix { get; set; }
    }

    public static class ContentCache
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static readonly Dictionary<string, object> _cache = new();
        private static bool _isPreloaded;

        public static bool IsPreloaded => _isPreloaded;

        public static void PreloadGeoscapeContent(GraphicsDevice device)
        {
            if (_isPreloaded) return;

            using (Profile.Time("ContentCache.PreloadGeoscape"))
            {
                LoadTextureCached(device, @"Content/Textures/Geoscape/EarthDiffuseMap.jpg",
                    @"Content/Textures/Geoscape/_LEGACY_EarthDiffuseMap.jpg");
                LoadTextureCached(device, @"Content/Textures/Geoscape/EarthNightMap.jpg",
                    @"Content/Textures/Geoscape/_LEGACY_EarthNightMap.png");
                LoadTextureCached(device, @"Content/Textures/Geoscape/EarthNormalMap.png", null);
                LoadTextureCached(device, @"Content/Textures/Geoscape/skybox.png", null);
            }

            _isPreloaded = true;
        }

        public static void PreloadXNetModels(ContentManager content, IEnumerable<string> modelNames)
        {
            foreach (var modelName in modelNames)
            {
                var key = "Model|Models/" + modelName;
                if (_cache.ContainsKey(key)) continue;

                try
                {
                    using (Profile.Time("ContentCache.PreloadXNet:" + modelName))
                    {
                        var model = content.Load<Microsoft.Xna.Framework.Graphics.Model>("Models/" + modelName);
                        var cached = new CachedModel
                        {
                            Model = model,
                            ScalingMatrix = ComputeScalingMatrix(model)
                        };
                        _cache[key] = cached;
                    }
                }
                catch (ContentLoadException)
                {
                    Logger.Warn("ContentCache: Failed to preload model " + modelName);
                }
            }
        }

        public static CachedModel? GetCachedModel(string modelName)
        {
            var key = "Model|Models/" + modelName;
            if (_cache.TryGetValue(key, out var obj) && obj is CachedModel cached)
                return cached;
            return null;
        }

        public static Texture2D GetTexture(string primaryPath)
        {
            return _cache.TryGetValue("Tex|" + primaryPath, out var obj) ? obj as Texture2D : null;
        }

        public static bool TryGetTexture(string primaryPath, out Texture2D texture)
        {
            texture = GetTexture(primaryPath);
            return texture != null;
        }

        public static void StoreTexture(string path, Texture2D texture)
        {
            _cache["Tex|" + path] = texture;
        }

        public static void Clear()
        {
            foreach (var obj in _cache.Values)
            {
                (obj as IDisposable)?.Dispose();
            }
            _cache.Clear();
            _isPreloaded = false;
        }

        private static void LoadTextureCached(GraphicsDevice device, string primaryPath, string fallbackPath)
        {
            var key = "Tex|" + primaryPath;
            if (_cache.ContainsKey(key)) return;

            string loadPath = primaryPath;
            if (!File.Exists(primaryPath) && fallbackPath != null)
            {
                loadPath = fallbackPath;
            }

            using (var fs = File.OpenRead(loadPath))
            {
                var tex = Texture2D.FromStream(device, fs);
                _cache[key] = tex;
            }
        }

        /// <summary>
        /// Precomputes the scaling matrix for a model during content preloading.
        /// See XNetScene.CalculateScalingMatrix for the detailed explanation of the transform.
        /// </summary>
        private static Matrix ComputeScalingMatrix(Microsoft.Xna.Framework.Graphics.Model model)
        {
            BoundingSphere sphere = Util.CalcBoundingSphere(model);
            return Matrix.CreateTranslation(-sphere.Center)
                 * Matrix.CreateScale(1.0f / sphere.Radius);
        }
    }
}
