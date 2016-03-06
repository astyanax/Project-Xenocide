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
* @file LineMesh.cs
* @date Created: 2008/01/04
* @author File creator: David Teviotdale
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

#endregion

namespace ProjectXenocide.UI.Scenes
{
    /// <summary>
    /// Handles drawing a mesh that's a set of lines in 3D space
    /// </summary>
    public class LineMesh : IDisposable
    {
        #region IDisposable

        /// <summary>
        /// Implement IDisposable
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Implement IDisposable
        /// </summary>
        /// <param name="disposing">false when called from a finalizer</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                DisposeBuffers();
                if (null != basicEffectVertexDeclaration)
                {
                    basicEffectVertexDeclaration.Dispose();
                    basicEffectVertexDeclaration = null;
                }
            }
        }

        #endregion IDisposable

        /// <summary>
        /// Load/create the graphic resources needed by the grid
        /// </summary>
        /// <param name="device">the display</param>
        /// <param name="builder">Thing that knows how what lines to draw</param>
        public void LoadContent(GraphicsDevice device, LineMeshBuilder builder)
        {
            basicEffectVertexDeclaration = new VertexDeclaration(
                device, VertexPositionColor.VertexElements);
            BuildMesh(device, builder);
        }

        /// <summary>
        /// Load the buffers with the set of lines to draw
        /// </summary>
        /// <param name="device"></param>
        /// <param name="builder">Thing that knows how what lines to draw</param>
        public void BuildMesh(GraphicsDevice device, LineMeshBuilder builder)
        {
            List<VertexPositionColor> verts = new List<VertexPositionColor>();
            List<short> indexes = new List<short>();
            builder.Build(verts, indexes);
            DisposeBuffers();
            constructVertexBuffer(device, verts);
            constructIndexBuffer(device, indexes);
        }

        /// <summary>
        /// construct a vertex buffer that can be used to draw the grid
        /// </summary>
        /// <param name="device"></param>
        /// <param name="verts"></param>
        private void constructVertexBuffer(GraphicsDevice device, List<VertexPositionColor> verts)
        {
            numVertices = verts.Count;
            if (0 < numVertices)
            {
                vertexBuffer = new VertexBuffer(
                    device,
                    VertexPositionColor.SizeInBytes * verts.Count,
                    BufferUsage.None
                    );
                vertexBuffer.SetData<VertexPositionColor>(verts.ToArray());
            }
        }

        /// <summary>
        /// construct the line list that can be used to draw the grid.
        /// </summary>
        /// <param name="device"></param>
        /// <param name="indexes"></param>
        private void constructIndexBuffer(GraphicsDevice device, List<short> indexes)
        {
            numLines = indexes.Count / 2;
            if (0 < numLines)
            {
                indexBuffer = new IndexBuffer(
                    device,
                    sizeof(short) * indexes.Count,
                    BufferUsage.None,
                    IndexElementSize.SixteenBits
                    );
                indexBuffer.SetData<short>(indexes.ToArray());
            }
        }

        /// <summary>
        /// Set up a BasicEffect for drawing the grid
        /// </summary>
        /// <param name="effect">effect to use to draw the grid</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
            Justification = "will throw if BasicEffect is null")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic",
            Justification = "What's going on is more clear if this is NOT a static function")]
        public void ConfigureEffect(BasicEffect effect)
        {
            effect.LightingEnabled = false;
            effect.DiffuseColor = new Vector3(1.0f, 1.0f, 1.0f);
            effect.Alpha = 1.0f;
            effect.World = Matrix.Identity;
            effect.TextureEnabled = false;
            effect.VertexColorEnabled = true;
        }

        /// <summary>
        /// Draw the grid on the device
        /// </summary>
        /// <param name="device">Device to render the grid to</param>
        /// <param name="effect">effect to use to draw the grid</param>
        public void Draw(GraphicsDevice device, Effect effect)
        {
            Draw(device, effect, numLines);
        }

        /// <summary>
        /// Draw the first lineCount lines of the mesh on the device
        /// </summary>
        /// <param name="device">Device to render the grid to</param>
        /// <param name="effect">effect to use to draw the grid</param>
        /// <param name="lineCount">Number of lines to draw</param>
        public void Draw(GraphicsDevice device, Effect effect, int lineCount)
        {
            if (0 < lineCount)
            {
                Debug.Assert(lineCount <= numLines);
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
                         numVertices,
                         0,
                         lineCount
                     );

                    pass.End();
                }
                effect.End();
            }
        }

        /// <summary>Release the buffers cleanly</summary>
        private void DisposeBuffers()
        {
            if (null != vertexBuffer)
            {
                vertexBuffer.Dispose();
                vertexBuffer = null;
            }
            if (null != indexBuffer)
            {
                indexBuffer.Dispose();
                indexBuffer = null;
            }
        }

        #region Fields

        /// <summary>Defines the vertex structures we're using</summary>
        private VertexDeclaration basicEffectVertexDeclaration;

        /// <summary>Vertices making up the lines</summary>
        private VertexBuffer vertexBuffer;

        /// <summary>Order to draw the lines</summary>
        private IndexBuffer indexBuffer;

        /// <summary>Number of vertices</summary>
        private int numVertices;

        /// <summary>Number of lines</summary>
        private int numLines;

        #endregion Fields
    }
}
