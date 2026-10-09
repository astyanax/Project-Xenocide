using Microsoft.Xna.Framework.Graphics;

namespace ProjectXenocide.UI.Scenes.Geoscape.Skybox
{
    /// <summary>
    /// Sky drawn by sampling a single 2:1 equirectangular panorama directly
    /// (no cube conversion). This is the default and the "new, better" path.
    /// </summary>
    /// <remarks>
    /// The heavy lifting is in <c>skybox.fx</c>: per pixel the view direction is
    /// converted to longitude/latitude and sampled from <c>SkyMap</c>.
    /// </remarks>
    public sealed class EquirectangularSkybox : SkyboxBase
    {
        /// <summary>
        /// Construct the renderer.
        /// </summary>
        /// <param name="effect">The compiled <c>skybox.fx</c> effect.</param>
        /// <param name="panorama">The 2:1 equirectangular texture.</param>
        /// <param name="sphere">The sky sphere geometry.</param>
        public EquirectangularSkybox(Effect effect, Texture2D panorama, SkyboxSphere sphere)
            : base(effect, panorama, sphere, "SkyMap")
        {
        }
    }
}
