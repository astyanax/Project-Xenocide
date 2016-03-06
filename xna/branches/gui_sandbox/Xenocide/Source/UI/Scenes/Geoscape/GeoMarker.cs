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
* @file GeoMarker.cs
* @date Created: 2007/01/29
* @author File creator: dteviot
* @author Credits: none
*/
#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

using Xenocide.Model.Geoscape;

#endregion

namespace Xenocide.UI.Scenes.Geoscape
{
    /// <summary>
    /// Used to draw a shape at a position of interest on the Globe
    /// </summary>
    class GeoMarker
    {
        public  VertexDeclaration             basicEffectVertexDeclaration;
        private VertexPositionNormalTexture[] meshVertices;
        private short[]                       meshIndices;
        VertexBuffer                          vertexBuffer;
        IndexBuffer                           indexBuffer;

        /// <summary>
        /// Load/create the graphic resources needed by the GeoMarker
        /// </summary>
        /// <param name="device">the display</param>
        public void LoadGraphicsContent(GraphicsDevice device)
        {
            // construct the shape
            InitializeMesh();
            constructVertexBuffer(device);
            constructIndexBuffer(device);
            basicEffectVertexDeclaration = new VertexDeclaration(
                device, VertexPositionNormalTexture.VertexElements);
        }

        /// <summary>
        /// Creates the GeoMarker's mesh
        /// </summary>
        private void InitializeMesh()
        {
            Vector3 top             = new Vector3( 0.00f,  0.00f, 0.0f);
            Vector3 baseTopLeft     = new Vector3(-0.05f,  0.05f, 0.5f);
            Vector3 baseTopRight    = new Vector3( 0.05f,  0.05f, 0.5f);

            Vector3 baseBottomLeft  = new Vector3(-0.05f, -0.05f, 0.5f);
            Vector3 baseBottomRight = new Vector3( 0.05f, -0.05f, 0.5f);

            meshVertices = new VertexPositionNormalTexture[5];
            meshVertices[0] = makeVertex(top);
            meshVertices[1] = makeVertex(baseTopLeft);
            meshVertices[2] = makeVertex(baseTopRight);
            meshVertices[3] = makeVertex(baseBottomLeft);
            meshVertices[4] = makeVertex(baseBottomRight);

            meshIndices = new short[18]{
                // top,   left      bottom    right
                0, 1, 2,  0, 3, 1,  0, 4, 3,  0, 2, 4,
                // base
                2, 1, 3,  3, 4, 2
            };
        }

        /// <summary>
        /// Construct a VertexPositionNormalTexture for this position
        /// </summary>
        /// <param name="position">Position of the vertex</param>
        /// <returns>The vertex</returns>
        private static VertexPositionNormalTexture makeVertex(Vector3 position)
        {
            // we're using this Vertext type because there isn't a VertexPositionNormal type
            // but we're not using the texture (at this point in time)
            Vector2 texture = new Vector2(0.0f, 0.0f);
            return new VertexPositionNormalTexture(position, Vector3.Normalize(position), texture);
        }

        /// <summary>
        /// construct a vertex buffer that can be used to draw the GeoMarker
        /// </summary>
        public void constructVertexBuffer(GraphicsDevice device)
        {
            vertexBuffer = new VertexBuffer(
                device,
                VertexPositionNormalTexture.SizeInBytes * meshVertices.Length,
                ResourceUsage.None,
                ResourceManagementMode.Automatic
                );
            vertexBuffer.SetData<VertexPositionNormalTexture>(meshVertices);
        }

        /// <summary>
        /// construct the triangle list that can be used to draw the GeoMarker.
        /// </summary>
        public void constructIndexBuffer(GraphicsDevice device)
        {
            indexBuffer = new IndexBuffer(
                device,
                sizeof(short) * meshIndices.Length,
                ResourceUsage.None,
                ResourceManagementMode.Automatic,
                IndexElementSize.SixteenBits
                );
            indexBuffer.SetData<short>(meshIndices);
        }

        /// <summary>
        /// Initialize the Basic effect prior to drawing a number of GeoMarkers
        /// </summary>
        /// <param name="device"></param>
        /// <param name="effect"></param>
        public void setupEffect(GraphicsDevice device, BasicEffect effect)
        {
            device.VertexDeclaration = basicEffectVertexDeclaration;

            device.Vertices[0].SetSource(
                 vertexBuffer, 0, VertexPositionNormalTexture.SizeInBytes);
            device.Indices = indexBuffer;

            effect.TextureEnabled = false;
        }
        
        /// <summary>
        /// Draw the GeoMarker on the device
        /// </summary>
        /// <param name="device">Device to render the GeoMarker to</param>
        /// <param name="geoposition">GeoMarker's location on the globe</param>
        /// <param name="effect">effect to use to draw the GeoMarker</param>
        public void Draw(GraphicsDevice device, Vector3 geoposition, BasicEffect effect)
        {
            effect.World = geopositionToWorld(geoposition);

            effect.Begin();
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Begin();

                device.DrawIndexedPrimitives(
                     PrimitiveType.TriangleList,
                     0,
                     0,
                     meshVertices.Length,
                     0,
                     meshIndices.Length / 3
                 );

                pass.End();
            }
            effect.End();
        }

        /// <summary>
        /// Converts a position on the globe into the World matrix for positioning the GeoMarker
        /// </summary>
        /// <param name="geoposition">position on globe (in polar radians)</param>
        /// <returns>World matrix used by Draw</returns>
        public static Matrix geopositionToWorld(Vector3 geoposition)
        {
            // make sure geopositon is on world
            geoposition.Z = 1.0f;

            Vector3 pos = GeoPosition.PolarToCartesian(geoposition);
            return Matrix.CreateRotationX(-geoposition.Y)
                * Matrix.CreateRotationY(geoposition.X)
                * Matrix.CreateTranslation(pos.X, pos.Y, pos.Z);
        }
    }
}
