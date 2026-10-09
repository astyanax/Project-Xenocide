using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ProjectXenocide.UI.Scenes.Geoscape.Skybox
{
    /// <summary>
    /// The unit sphere the sky is drawn on.
    /// </summary>
    /// <remarks>
    /// Only positions are generated - no normals or texture coordinates. The sky
    /// shaders derive the sampling direction from the vertex's object-space
    /// position, which works because the sphere has radius 1 and is centred on
    /// the camera, so a point on its surface is also the direction to sample.
    ///
    /// The sphere is tessellated by <paramref name="slices"/> (segments around
    /// the equator) and <paramref name="stacks"/> (segments from pole to pole).
    /// Because sampling is done per pixel from the direction, even a coarse
    /// sphere looks perfectly smooth - tessellation only affects silhouette
    /// smoothness (irrelevant for a sky) not the mapping.
    /// </remarks>
    public sealed class SkyboxSphere : IDisposable
    {
        /// <summary>Vertex buffer holding the sphere's positions.</summary>
        public VertexBuffer VertexBuffer { get; }

        /// <summary>Index buffer (triangle list).</summary>
        public IndexBuffer IndexBuffer { get; }

        /// <summary>Number of triangles to draw.</summary>
        public int PrimitiveCount { get; }

        /// <summary>
        /// Build a radius-1 sphere centred on the origin.
        /// </summary>
        /// <param name="device">Graphics device used to create the buffers.</param>
        /// <param name="slices">Longitudinal segments (around Y).</param>
        /// <param name="stacks">Latitudinal segments (pole to pole).</param>
        public SkyboxSphere(GraphicsDevice device, int slices = 32, int stacks = 16)
        {
            var vertices = new VertexPosition[(stacks + 1) * (slices + 1)];
            int v = 0;
            for (int stack = 0; stack <= stacks; ++stack)
            {
                // phi goes 0 (north pole, +Y) to PI (south pole, -Y).
                double phi = Math.PI * stack / stacks;
                float y = (float)Math.Cos(phi);
                float r = (float)Math.Sin(phi);

                for (int slice = 0; slice <= slices; ++slice)
                {
                    // theta goes 0..2PI around the Y axis (longitude).
                    double theta = Math.PI * 2.0 * slice / slices;
                    float x = r * (float)Math.Cos(theta);
                    float z = r * (float)Math.Sin(theta);
                    vertices[v++] = new VertexPosition(new Vector3(x, y, z));
                }
            }

            // Two triangles per quad. The pole quads are degenerate (their top or
            // bottom edge collapses to a point) but that is harmless.
            var indices = new short[stacks * slices * 6];
            int i = 0;
            int stride = slices + 1;
            for (int stack = 0; stack < stacks; ++stack)
            {
                for (int slice = 0; slice < slices; ++slice)
                {
                    short topLeft = (short)((stack * stride) + slice);
                    short topRight = (short)(topLeft + 1);
                    short bottomLeft = (short)(((stack + 1) * stride) + slice);
                    short bottomRight = (short)(bottomLeft + 1);

                    indices[i++] = topLeft; indices[i++] = bottomLeft; indices[i++] = topRight;
                    indices[i++] = topRight; indices[i++] = bottomLeft; indices[i++] = bottomRight;
                }
            }

            PrimitiveCount = indices.Length / 3;

            VertexBuffer = new VertexBuffer(device, VertexPosition.VertexDeclaration, vertices.Length, BufferUsage.WriteOnly);
            VertexBuffer.SetData(vertices);

            IndexBuffer = new IndexBuffer(device, IndexElementSize.SixteenBits, indices.Length, BufferUsage.WriteOnly);
            IndexBuffer.SetData(indices);
        }

        /// <summary>Release the vertex and index buffers.</summary>
        public void Dispose()
        {
            VertexBuffer?.Dispose();
            IndexBuffer?.Dispose();
        }
    }
}
