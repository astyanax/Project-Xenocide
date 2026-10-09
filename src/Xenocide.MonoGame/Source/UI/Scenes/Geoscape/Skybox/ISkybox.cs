using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ProjectXenocide.UI.Scenes.Geoscape.Skybox
{
    /// <summary>
    /// A background sky. Implementations differ only in how the panorama texture
    /// is projected (equirectangular panorama vs. a cube map), so they can be
    /// swapped interchangeably without the scene knowing the difference.
    /// </summary>
    public interface ISkybox : IDisposable
    {
        /// <summary>
        /// Draw the sky for the current camera.
        /// </summary>
        /// <param name="device">The graphics device.</param>
        /// <param name="view">The camera's view matrix (translation is ignored).</param>
        /// <param name="projection">The camera's projection matrix.</param>
        void Draw(GraphicsDevice device, Matrix view, Matrix projection);
    }
}
