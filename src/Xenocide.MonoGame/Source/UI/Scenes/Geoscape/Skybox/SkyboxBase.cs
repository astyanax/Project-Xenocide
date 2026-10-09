using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ProjectXenocide.UI.Scenes.Geoscape.Skybox
{
    /// <summary>
    /// Shared behaviour for the sky renderers: camera-centred sphere, no parallax,
    /// no depth interaction, and the two effect parameters both shaders share.
    /// </summary>
    /// <remarks>
    /// The camera's translation is removed from the view matrix before it is
    /// handed to the shader. That is what makes the sky feel "infinitely far
    /// away": rotating the camera turns the sky, but moving the camera does not
    /// shift it (no parallax). The sphere itself sits at the origin, so in this
    /// rotation-only space the camera is effectively at the origin too.
    /// </remarks>
    public abstract class SkyboxBase : ISkybox
    {
        private readonly Effect effect;
        private readonly Texture texture;
        private readonly SkyboxSphere sphere;
        private readonly string textureParameterName;

        protected SkyboxBase(Effect effect, Texture texture, SkyboxSphere sphere, string textureParameterName)
        {
            this.effect = effect;
            this.texture = texture;
            this.sphere = sphere;
            this.textureParameterName = textureParameterName;
        }

        /// <summary>The effect that samples the sky texture.</summary>
        protected Effect Effect => effect;

        /// <inheritdoc/>
        public void Draw(GraphicsDevice device, Matrix view, Matrix projection)
        {
            // Remove translation (keep rotation) so the sky only follows the
            // camera's orientation. `view` is a value type, so this copy is local.
            view.Translation = Vector3.Zero;
            effect.Parameters["RotationProjection"].SetValue(view * projection);
            effect.Parameters[textureParameterName].SetValue(texture);

            // Depth off: the sky never occludes anything and needs no depth test
            // (the vertex shader already places it at the far plane). Cull off so
            // we see the inside of the sphere regardless of winding.
            device.DepthStencilState = DepthStencilState.None;
            device.RasterizerState = RasterizerState.CullNone;

            device.SetVertexBuffer(sphere.VertexBuffer);
            device.Indices = sphere.IndexBuffer;

            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, sphere.PrimitiveCount);
            }

            // The scene re-establishes its own raster/depth state before drawing
            // the globe, so nothing needs restoring here.
        }

        /// <summary>Release the sphere buffers. The effect and texture are shared.</summary>
        public virtual void Dispose()
        {
            sphere?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
