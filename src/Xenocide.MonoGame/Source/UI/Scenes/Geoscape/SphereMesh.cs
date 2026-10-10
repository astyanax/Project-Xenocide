#region Copyright
/*
--------------------------------------------------------------------------------
This source file is part of Xenocide
  by  Project Xenocide Team

For the latest info on Xenocide, see http://www.projectxenocide.com/

This work is licensed under the Creative Commons
Attribution-NonCommercial-ShareAlike 2.5 License.

To view a copy of this license, visit
http://creativecommons.org/licenses/by-nc-sa/2.5/
or send a letter to Creative Commons, 543 Howard Street, 5th Floor,
San Francisco, California, 94105, USA.
--------------------------------------------------------------------------------
*/

/*
* @file SphereMesh.cs
* @date Created: 2007/01/25
* @author File creator: David Teviotdale
* @author Credits: none
*/
#endregion

using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ProjectXenocide.UI.Scenes.Geoscape
{
    /// <summary>
    /// Generates a latitude/longitude ("UV") sphere of <see cref="GlobeVertex"/>es
    /// for the Earth globe.
    /// </summary>
    /// <remarks>
    /// <paramref name="slices"/> segments go around the equator and
    /// <paramref name="stacks"/> bands run from pole to pole, mapping the
    /// equirectangular textures directly (u = longitude, v = latitude).
    ///
    /// This replaces the original hand-rolled sphere, whose pole was a single
    /// 4-triangle fan and whose coarse tessellation produced visible facets and a
    /// "pyramid"/concentric-square artifact at the poles. Here the pole is a fan
    /// of <paramref name="slices"/> triangles, so the texture converges smoothly.
    ///
    /// The tangent frame keeps the original convention
    /// (tangent = position x UnitX, binormal = position x tangent) so the normal
    /// map behaves as before; only the resolution changes.
    /// </remarks>
    sealed class SphereMesh
    {
        private readonly GlobeVertex[] vertexes;
        private readonly short[] triangleListIndices;

        /// <summary>
        /// Build the sphere.
        /// </summary>
        /// <param name="slices">Longitudinal segments (around the Y axis).</param>
        /// <param name="stacks">Latitudinal bands (pole to pole).</param>
        public SphereMesh(int slices = 96, int stacks = 48)
        {
            slices = Math.Max(3, slices);
            stacks = Math.Max(2, stacks);

            int columns = slices + 1;
            int rows = stacks + 1;
            vertexes = new GlobeVertex[rows * columns];

            for (int stack = 0; stack <= stacks; ++stack)
            {
                // v goes 0 at the north pole to 1 at the south pole.
                float v = (float)stack / stacks;
                double latitude = Math.PI * v;
                float y = (float)Math.Cos(latitude);
                float r = (float)Math.Sin(latitude);

                for (int slice = 0; slice <= slices; ++slice)
                {
                    // u goes 0..1 the whole way around (the last column repeats the
                    // first so the texture wraps without a seam).
                    float u = (float)slice / slices;
                    double longitude = Math.PI * 2.0 * u;
                    // Note the negated X: this matches the original sphere's
                    // coordinate convention (together with the scene's -90 degree
                    // Y rotation) so the texture is not mirrored east<->west.
                    float x = -r * (float)Math.Cos(longitude);
                    float z = r * (float)Math.Sin(longitude);

                    Vector3 position = new Vector3(x, y, z);
                    Vector3 tangent = Vector3.Cross(position, Vector3.UnitX);
                    Vector3 binormal = Vector3.Cross(position, tangent);

                    vertexes[(stack * columns) + slice] =
                        new GlobeVertex(position, position, tangent, binormal, new Vector2(u, v));
                }
            }

            triangleListIndices = new short[stacks * slices * 6];
            int i = 0;
            for (int stack = 0; stack < stacks; ++stack)
            {
                for (int slice = 0; slice < slices; ++slice)
                {
                    short topLeft = (short)((stack * columns) + slice);
                    short topRight = (short)(topLeft + 1);
                    short bottomLeft = (short)(((stack + 1) * columns) + slice);
                    short bottomRight = (short)(bottomLeft + 1);

                    // Two triangles per quad. The winding is not critical because
                    // the globe is drawn with back-face culling disabled.
                    triangleListIndices[i++] = topLeft;
                    triangleListIndices[i++] = bottomLeft;
                    triangleListIndices[i++] = topRight;

                    triangleListIndices[i++] = topRight;
                    triangleListIndices[i++] = bottomLeft;
                    triangleListIndices[i++] = bottomRight;
                }
            }
        }

        /// <summary>
        /// return a vertex buffer that can be used to draw the sphere
        /// </summary>
        public VertexBuffer CreateVertexBuffer(GraphicsDevice device)
        {
            VertexBuffer vertexBuffer = new VertexBuffer(
                device,
                GlobeVertex.VertexDeclaration,
                vertexes.Length,
                BufferUsage.None
                );

            vertexBuffer.SetData(vertexes);
            return vertexBuffer;
        }

        /// <summary>
        /// return indexed triangle list that can be used to draw the sphere.
        /// </summary>
        public IndexBuffer CreateIndexBuffer(GraphicsDevice device)
        {
            IndexBuffer indexBuffer = new IndexBuffer(
                device,
                IndexElementSize.SixteenBits,
                triangleListIndices.Length,
                BufferUsage.None
                );
            indexBuffer.SetData(triangleListIndices);
            return indexBuffer;
        }

        public int TotalVertexes { get { return vertexes.Length; } }
        public int TotalIndexes { get { return triangleListIndices.Length; } }
        public int TotalFaces { get { return TotalIndexes / 3; } }
    }
}
