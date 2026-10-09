using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ProjectXenocide.UI.Scenes.Geoscape.Skybox
{
    /// <summary>
    /// Converts a 2:1 equirectangular panorama into a cube map once, at load time.
    /// </summary>
    /// <remarks>
    /// This exists so the two skybox strategies can share one source image: the
    /// equirectangular renderer samples the panorama directly, while the cube
    /// renderer samples this baked cube. Doing it on the CPU is deterministic and
    /// avoids depending on render-target-cube support.
    ///
    /// MATHS
    /// -----
    /// For each cube face and each texel we need the direction that points at it;
    /// that direction is then converted to equirectangular UVs (the same
    /// atan2/acos mapping used in skybox.fx) and the panorama is sampled there.
    ///
    /// The face-to-direction mapping below follows the OpenGL cube-map layout
    /// (the layout the hardware uses when it samples via <c>texCUBE</c>), with
    /// a = 2u-1 and b = 2v-1 in [-1, 1]:
    ///
    ///   face | direction
    ///   +X   | ( 1, -b, -a)
    ///   -X   | (-1, -b,  a)
    ///   +Y   | ( a,  1,  b)
    ///   -Y   | ( a, -1, -b)
    ///   +Z   | ( a, -b,  1)
    ///   -Z   | (-a, -b, -1)
    ///
    /// If the cube ever looks mirrored in game, this table (and nothing else) is
    /// what needs flipping.
    /// </remarks>
    public static class CubeMapBaker
    {
        /// <summary>Bake a cube map from an equirectangular panorama.</summary>
        /// <param name="device">Graphics device.</param>
        /// <param name="panorama">The source 2:1 panorama (already on the GPU).</param>
        /// <param name="faceSize">Edge length, in texels, of each cube face.</param>
        /// <returns>The baked cube map.</returns>
        public static TextureCube Bake(GraphicsDevice device, Texture2D panorama, int faceSize)
        {
            int width = panorama.Width;
            int height = panorama.Height;
            var source = new Color[width * height];
            panorama.GetData(source);

            var cube = new TextureCube(device, faceSize, false, SurfaceFormat.Color);
            var face = new Color[faceSize * faceSize];

            for (int f = 0; f < 6; ++f)
            {
                for (int y = 0; y < faceSize; ++y)
                {
                    float v = (y + 0.5f) / faceSize;
                    for (int x = 0; x < faceSize; ++x)
                    {
                        float u = (x + 0.5f) / faceSize;

                        Vector3 dir = DirectionFromFace(f, u, v);

                        // Direction -> equirectangular UV (identical to skybox.fx).
                        float su = (float)((Math.Atan2(dir.Z, dir.X) / (2.0 * Math.PI)) + 0.5);
                        float sv = (float)(Math.Acos(Math.Clamp(dir.Y, -1.0, 1.0)) / Math.PI);

                        face[(y * faceSize) + x] = SampleBilinear(source, width, height, su, sv);
                    }
                }

                cube.SetData((CubeMapFace)f, face);
            }

            return cube;
        }

        /// <summary>Direction pointing at the given (u,v) on the given face.</summary>
        private static Vector3 DirectionFromFace(int face, float u, float v)
        {
            float a = (2.0f * u) - 1.0f;
            float b = (2.0f * v) - 1.0f;

            switch (face)
            {
                case 0: return new Vector3(1.0f, -b, -a);   // +X
                case 1: return new Vector3(-1.0f, -b, a);   // -X
                case 2: return new Vector3(a, 1.0f, b);     // +Y
                case 3: return new Vector3(a, -1.0f, -b);   // -Y
                case 4: return new Vector3(a, -b, 1.0f);    // +Z
                default: return new Vector3(-a, -b, -1.0f); // -Z
            }
        }

        /// <summary>Bilinear sample with longitude wrap (U) and latitude clamp (V).</summary>
        private static Color SampleBilinear(Color[] source, int width, int height, float u, float v)
        {
            float fx = (u * width) - 0.5f;
            float fy = (v * height) - 0.5f;

            int x0 = (int)Math.Floor(fx);
            int y0 = (int)Math.Floor(fy);
            float tx = fx - x0;
            float ty = fy - y0;

            Color c00 = Texel(source, width, height, x0, y0);
            Color c10 = Texel(source, width, height, x0 + 1, y0);
            Color c01 = Texel(source, width, height, x0, y0 + 1);
            Color c11 = Texel(source, width, height, x0 + 1, y0 + 1);

            return Color.Lerp(Color.Lerp(c00, c10, tx), Color.Lerp(c01, c11, tx), ty);
        }

        private static Color Texel(Color[] source, int width, int height, int x, int y)
        {
            x = ((x % width) + width) % width;        // longitude wraps
            y = Math.Clamp(y, 0, height - 1);          // latitude clamps at poles
            return source[(y * width) + x];
        }
    }
}
