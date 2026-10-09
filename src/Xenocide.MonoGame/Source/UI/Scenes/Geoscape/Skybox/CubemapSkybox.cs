using Microsoft.Xna.Framework.Graphics;

namespace ProjectXenocide.UI.Scenes.Geoscape.Skybox
{
    /// <summary>
    /// Sky drawn from a cube map. Kept as an interchangeable alternative to
    /// <see cref="EquirectangularSkybox"/>; the cube is baked from the same
    /// panorama at load time (see <see cref="CubeMapBaker"/>).
    /// </summary>
    /// <remarks>
    /// The hardware samples the cube directly from the view direction
    /// (<c>skyboxcube.fx</c>), so there is no per-pixel trigonometry. A cube map
    /// also avoids the equirectangular projection's pole compression because the
    /// six faces redistribute the texels evenly.
    /// </remarks>
    public sealed class CubemapSkybox : SkyboxBase
    {
        private readonly TextureCube cube;

        /// <summary>
        /// Construct the renderer.
        /// </summary>
        /// <param name="effect">The compiled <c>skyboxcube.fx</c> effect.</param>
        /// <param name="cube">The baked cube map.</param>
        /// <param name="sphere">The sky sphere geometry.</param>
        public CubemapSkybox(Effect effect, TextureCube cube, SkyboxSphere sphere)
            : base(effect, cube, sphere, "SkyCube")
        {
            this.cube = cube;
        }

        /// <inheritdoc/>
        public override void Dispose()
        {
            base.Dispose();
            // Unlike the shared equirectangular panorama, the baked cube is owned
            // by this renderer, so release it.
            cube?.Dispose();
        }
    }
}
