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
* @file Grid.cs
* @date Created: 2007/04/23
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

#endregion

namespace Xenocide.UI.Scenes.Facility
{
    /// <summary>
    /// A grid marking the cells in a human base that can hold a facility
    /// </summary>
    internal class Grid
    {
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="cellsWide">Size of gird (in cells) along width of display</param>
        /// <param name="cellsHigh">Size of gird (in cells) along height of display</param>
        public Grid(int cellsWide, int cellsHigh) 
        {
            this.cellsWide = cellsWide;
            this.cellsHigh = cellsHigh;
        }

        /// <summary>
        /// Load/create the graphic resources needed by the grid
        /// </summary>
        /// <param name="device">the display</param>
        public void LoadGraphicsContent(GraphicsDevice device)
        {
            InitializeMesh();
            constructVertexBuffer(device);
            constructIndexBuffer(device);
            basicEffectVertexDeclaration = new VertexDeclaration(
                device, VertexPositionColor.VertexElements);
        }

        private void InitializeMesh()
        {
            // number of lines in the grid
            int numLines = cellsWide + cellsHigh + 2;
            meshVertices = new VertexPositionColor[numLines * 2];
            meshIndices  = new short[numLines * 2];

            // limits of grid
            float maxX = (cellsWide / 2.0f);
            float maxZ = (cellsHigh / 2.0f);

            // Horizontal lines
            short line = -1;
            for (float z = -maxZ; z <= maxZ; z += 1.0f)
            {
                meshVertices[++line] = new VertexPositionColor(new Vector3(-maxX, 0.0f, z), gridColor);
                meshIndices[line] = line;
                meshVertices[++line] = new VertexPositionColor(new Vector3(maxX, 0.0f, z), gridColor);
                meshIndices[line] = line;
            }

            // "vertical" lines
            for (float x = -maxX; x <= maxX; x += 1.0f)
            {
                meshVertices[++line] = new VertexPositionColor(new Vector3(x, 0.0f, -maxZ), gridColor);
                meshIndices[line] = line;
                meshVertices[++line] = new VertexPositionColor(new Vector3(x, 0.0f, maxZ), gridColor);
                meshIndices[line] = line;
            }
        }

        /// <summary>
        /// construct a vertex buffer that can be used to draw the grid
        /// </summary>
        public void constructVertexBuffer(GraphicsDevice device)
        {
            vertexBuffer = new VertexBuffer(
                device,
                VertexPositionColor.SizeInBytes * meshVertices.Length,
                ResourceUsage.None,
                ResourceManagementMode.Automatic
                );
            vertexBuffer.SetData<VertexPositionColor>(meshVertices);
        }

        /// <summary>
        /// construct the line list that can be used to draw the grid.
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
        /// Set up a BasicEffect for drawing the grid
        /// </summary>
        /// <param name="effect">effect to use to draw the grid</param>
        public void ConfigureEffect(BasicEffect effect)
        {
            effect.LightingEnabled = false;
            effect.DiffuseColor = gridColor.ToVector3();
            effect.Alpha = 1.0f;
            effect.World = Matrix.Identity;
            effect.TextureEnabled = false;
        }
        
        /// <summary>
        /// Draw the grid on the device
        /// </summary>
        /// <param name="device">Device to render the grid to</param>
        /// <param name="effect">effect to use to draw the grid</param>
        public void Draw(GraphicsDevice device, BasicEffect effect)
        {
            device.VertexDeclaration = basicEffectVertexDeclaration;

            device.Vertices[0].SetSource(
                 vertexBuffer, 0, VertexPositionColor.SizeInBytes);
            device.Indices = indexBuffer;

            effect.Begin();
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Begin();

                device.DrawIndexedPrimitives(
                     PrimitiveType.LineList,
                     0,
                     0,
                     meshVertices.Length,
                     0,
                     meshIndices.Length / 2
                 );

                pass.End();
            }
            effect.End();
        }

        #region Fields

        /// <summary>
        /// Size of gird (in cells) along width of display
        /// </summary>
        private int cellsWide;

        /// <summary>
        /// Size of gird (in cells) along height of display
        /// </summary>
        private int cellsHigh;

        /// <summary>
        /// Color to give the lines of the grid
        /// </summary>
        private Color gridColor = Color.Red;

        
        private VertexDeclaration basicEffectVertexDeclaration;

        /// <summary>
        /// Vertices making up the grid
        /// </summary>
        private VertexPositionColor[] meshVertices;

        /// <summary>
        /// Lines making up the grid
        /// </summary>
        private short[] meshIndices;

        /// <summary>
        /// Vertices making up the grid
        /// </summary>
        private VertexBuffer vertexBuffer;

        /// <summary>
        /// Lines making up the grid
        /// </summary>
        private IndexBuffer indexBuffer; 

        #endregion Fields
    }
}
