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
* @file BuildTimes.cs
* @date Created: 2007/04/30
* @author File creator: dteviot
* @author Credits: none
*/
#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

using ProjectXenocide.Model.Geoscape.Outposts;

#endregion

namespace ProjectXenocide.UI.Scenes.Facility
{
    /// <summary>
    /// Overlay for facilities, giving build time remaining
    /// </summary>
    class BuildTimes
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="floorplan">The facilities in the base</param>
        public BuildTimes(Floorplan floorplan)
        {
            this.floorplan = floorplan;
        }

        /// <summary>
        /// Load/create the graphic resources needed by the build time quads
        /// </summary>
        /// <param name="content">content manager that fetches the content</param>
        /// <param name="device">the display</param>
        public void LoadContent(ContentManager content, GraphicsDevice device)
        {
            texture = content.Load<Texture2D>(@"Content\Textures\OutpostLayout\BuildTimes");
            InitializeMesh();
            if (0 < meshVertices.Length)
            {
                constructVertexBuffer(device);
                constructIndexBuffer(device);
                basicEffectVertexDeclaration = new VertexDeclaration(
                    device, VertexPositionTexture.VertexElements);
            }
        }

        /// <summary>
        /// Consturct mesh holding set of quads holding build times
        /// </summary>
        private void InitializeMesh()
        {
            int numQuads = CountNumQuadsNeeded();
            meshVertices = new VertexPositionTexture[numQuads * 4];
            meshIndices = new short[numQuads * 6];

            int count = 0;
            foreach (FacilityHandle handle in floorplan.Facilities)
            {
                if (handle.IsUnderConstruction)
                {
                    AddQuadToMesh(handle, count);
                    ++count;
                }
            }
        }

        /// <summary>
        /// Set up a BasicEffect for drawing the build time remaining quads
        /// </summary>
        /// <param name="effect">effect to use to draw the quads</param>
        public void ConfigureEffect(BasicEffect effect)
        {
            effect.LightingEnabled = false;
            effect.DiffuseColor = new Vector3(1.0f, 1.0f, 1.0f);
            effect.Alpha = 0.5f;
            effect.Texture = texture;
            effect.TextureEnabled = true;
        }

        /// <summary>
        /// Draw the build times on the device
        /// </summary>
        /// <param name="device">Device to render the build times to</param>
        /// <param name="effect">effect to use to draw the build times</param>
        public void Draw(GraphicsDevice device, BasicEffect effect)
        {
            if (0 < meshVertices.Length)
            {
                device.VertexDeclaration = basicEffectVertexDeclaration;

                device.Vertices[0].SetSource(
                     vertexBuffer, 0, VertexPositionTexture.SizeInBytes);
                device.Indices = indexBuffer;

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
        }

        /// <summary>
        /// Add the polys (needed for this facility) to the mesh
        /// </summary>
        /// <param name="handle">Facility to provide a build time for</param>
        /// <param name="quadNum">Where to put the polys in the mesh</param>
        private void AddQuadToMesh(FacilityHandle handle, int quadNum)
        {
            // compute position in scene for quad
            float left = (floorplan.CellsWide * -0.5f) + handle.X + (handle.FacilityInfo.XSize * 0.5f) - 0.5f;
            float top = (floorplan.CellsHigh * -0.5f) + handle.Y + (handle.FacilityInfo.YSize * 0.5f) - 0.5f;
            float height = 0.5f;
            Vector3 topLeft = new Vector3(left, height, top);
            Vector3 topRight = new Vector3(left + 1.0f, height, top);
            Vector3 bottomLeft = new Vector3(left, height, top + 1.0f);
            Vector3 bottomRight = new Vector3(left + 1.0f, height, top + 1.0f);

            // constants for BuildTime texture map
            const int texturesPerRow = 7;               // Bitmap has 7 textures per row
            const float bitmapPixelLength = 256.0f;          // Bitmap is 256 x 256
            const int texturePixelLength = 35;              // each texture is 35 pixels high and wide
            const float textureLength = texturePixelLength / bitmapPixelLength;

            // figure out texture for quad 
            int textureIndex = (int)Math.Ceiling(handle.TimeToBuild / 86400.0) - 1;
            Debug.Assert(textureIndex <= 49);
            float texTop = (texturePixelLength + 1) * (textureIndex / texturesPerRow) / bitmapPixelLength;
            float texLeft = (texturePixelLength + 1) * (textureIndex % texturesPerRow) / bitmapPixelLength;
            Vector2 textureTopLeft = new Vector2(texLeft, texTop);
            Vector2 textureTopRight = new Vector2(texLeft + textureLength, texTop);
            Vector2 textureBottomLeft = new Vector2(texLeft, texTop + textureLength);
            Vector2 textureBottomRight = new Vector2(texLeft + textureLength, texTop + textureLength);

            // Add vertices to Mesh
            int vIndex = quadNum * 4;
            meshVertices[vIndex] = new VertexPositionTexture(topLeft, textureTopLeft);
            meshVertices[vIndex + 1] = new VertexPositionTexture(bottomLeft, textureBottomLeft);
            meshVertices[vIndex + 2] = new VertexPositionTexture(topRight, textureTopRight);
            meshVertices[vIndex + 3] = new VertexPositionTexture(bottomRight, textureBottomRight);

            // Add vertices to Mesh
            int iIndex = quadNum * 6;
            meshIndices[iIndex] = (short)(vIndex);
            meshIndices[iIndex + 1] = (short)(vIndex + 2);
            meshIndices[iIndex + 2] = (short)(vIndex + 1);
            meshIndices[iIndex + 3] = (short)(vIndex + 2);
            meshIndices[iIndex + 4] = (short)(vIndex + 3);
            meshIndices[iIndex + 5] = (short)(vIndex + 1);
        }

        /// <summary>
        /// Figure out the number of quads (holding build time) we need to draw
        /// </summary>
        /// <returns>Number of quads</returns>
        private int CountNumQuadsNeeded()
        {
            int count = 0;
            foreach (FacilityHandle handle in floorplan.Facilities)
            {
                if (handle.IsUnderConstruction)
                {
                    ++count;
                }
            }
            return count;
        }

        /// <summary>
        /// construct a vertex buffer that can be used to draw the quads
        /// </summary>
        private void constructVertexBuffer(GraphicsDevice device)
        {
            vertexBuffer = new VertexBuffer(
                device,
                VertexPositionTexture.SizeInBytes * meshVertices.Length,
                BufferUsage.None
                );
            vertexBuffer.SetData<VertexPositionTexture>(meshVertices);
        }

        /// <summary>
        /// construct the triangle list that can be used to draw the quads.
        /// </summary>
        private void constructIndexBuffer(GraphicsDevice device)
        {
            indexBuffer = new IndexBuffer(
                device,
                sizeof(short) * meshIndices.Length,
                BufferUsage.None,
                IndexElementSize.SixteenBits
                );
            indexBuffer.SetData<short>(meshIndices);
        }

        #region Fields

        private Texture2D texture;
        private VertexDeclaration basicEffectVertexDeclaration;
        private VertexPositionTexture[] meshVertices;
        private short[] meshIndices;
        private VertexBuffer vertexBuffer;
        private IndexBuffer indexBuffer;

        /// <summary>
        /// The facilities in the base
        /// </summary>
        private Floorplan floorplan;

        #endregion Fields
    }
}
