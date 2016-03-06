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
* @file SkyBox.cs
* @date Created: 2007/01/28
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

namespace ProjectXenocide.UI.Scenes.Geoscape
{
    /// <summary>
    /// The skybox (obviously)
    /// </summary>
    public class SkyBox : IDisposable
    {
        private Texture2D texture;
        private Effect effect;

        private VertexBuffer vertices;
        private IndexBuffer indices;
        private VertexDeclaration vertexDecl;

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
                if (indices != null)
                {
                    indices.Dispose();
                    indices = null;
                }
                if (vertices != null)
                {
                    vertices.Dispose();
                    vertices = null;
                }
                if (vertexDecl != null)
                {
                    vertexDecl.Dispose();
                    vertexDecl = null;
                }
            }
        }

        #endregion IDisposable

        #region IDrawable Members

        /// <summary>
        /// Render the skybox
        /// </summary>
        /// <param name="device">the render device</param>
        /// <param name="worldViewProjection">combined World View Projection </param>
        /// <param name="ambient">intensity to give the skybox</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
            Justification="Will throw if device is null")]
        public void Draw(GraphicsDevice device, Matrix worldViewProjection, float ambient)
        {
            if (vertices == null)
                return;

            effect.Begin();
            effect.Parameters["worldViewProjection"].SetValue(worldViewProjection);

            device.VertexDeclaration = vertexDecl;
            device.Vertices[0].SetSource(vertices, 0,
                vertexDecl.GetVertexStrideSize(0));

            device.Indices = indices;

            effect.Parameters["baseTexture"].SetValue(texture);
            effect.Parameters["ambient"].SetValue(ambient);
            effect.Techniques[0].Passes[0].Begin();

            device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 24, 0, 12);
            effect.Techniques[0].Passes[0].End();

            effect.End();
        }

        #endregion

        /// <summary>
        /// Setup Graphics resources used by skybox
        /// </summary>
        /// <param name="content">content manager that will look after the resources</param>
        /// <param name="device">the graphics device</param>
        public void LoadContent(ContentManager content, GraphicsDevice device)
        {
            texture = Texture2D.FromFile(device, @"Content\Textures\Geoscape\skybox.png");
            effect = content.Load<Effect>(@"Content\Shaders\skybox");

            vertexDecl = new VertexDeclaration(device,
                new VertexElement[] {
                    new VertexElement(0,0,VertexElementFormat.Vector3,
                           VertexElementMethod.Default,
                            VertexElementUsage.Position,0),
                    new VertexElement(0,sizeof(float)*3,VertexElementFormat.Vector2,
                           VertexElementMethod.Default,
                            VertexElementUsage.TextureCoordinate,0)});


            vertices = new VertexBuffer(device,
                                typeof(VertexPositionTexture),
                                4 * 6,
                                BufferUsage.WriteOnly);

            VertexPositionTexture[] data = new VertexPositionTexture[4 * 6];

            Vector3 vExtents = new Vector3(10, 10, 10);
            //back
            data[0].Position = new Vector3(vExtents.X, -vExtents.Y, -vExtents.Z);
            data[0].TextureCoordinate.X = 2.0f / 3.0f; data[0].TextureCoordinate.Y = 0.5f;
            data[1].Position = new Vector3(vExtents.X, vExtents.Y, -vExtents.Z);
            data[1].TextureCoordinate.X = 2.0f / 3.0f; data[1].TextureCoordinate.Y = 0.0f;
            data[2].Position = new Vector3(-vExtents.X, vExtents.Y, -vExtents.Z);
            data[2].TextureCoordinate.X = 1.0f / 3.0f; data[2].TextureCoordinate.Y = 0.0f;
            data[3].Position = new Vector3(-vExtents.X, -vExtents.Y, -vExtents.Z);
            data[3].TextureCoordinate.X = 1.0f / 3.0f; data[3].TextureCoordinate.Y = 0.5f;

            //front
            data[4].Position = new Vector3(-vExtents.X, -vExtents.Y, vExtents.Z);
            data[4].TextureCoordinate.X = 2.0f / 3.0f; data[4].TextureCoordinate.Y = 1.0f;
            data[5].Position = new Vector3(-vExtents.X, vExtents.Y, vExtents.Z);
            data[5].TextureCoordinate.X = 2.0f / 3.0f; data[5].TextureCoordinate.Y = 0.5f;
            data[6].Position = new Vector3(vExtents.X, vExtents.Y, vExtents.Z);
            data[6].TextureCoordinate.X = 1.0f / 3.0f; data[6].TextureCoordinate.Y = 0.5f;
            data[7].Position = new Vector3(vExtents.X, -vExtents.Y, vExtents.Z);
            data[7].TextureCoordinate.X = 1.0f / 3.0f; data[7].TextureCoordinate.Y = 1.0f;

            //bottom
            data[8].Position = new Vector3(-vExtents.X, -vExtents.Y, -vExtents.Z);
            data[8].TextureCoordinate.X = 1.0f / 3.0f; data[8].TextureCoordinate.Y = 0.5f;
            data[9].Position = new Vector3(-vExtents.X, -vExtents.Y, vExtents.Z);
            data[9].TextureCoordinate.X = 1.0f / 3.0f; data[9].TextureCoordinate.Y = 1.0f;
            data[10].Position = new Vector3(vExtents.X, -vExtents.Y, vExtents.Z);
            data[10].TextureCoordinate.X = 0.0f; data[10].TextureCoordinate.Y = 1.0f;
            data[11].Position = new Vector3(vExtents.X, -vExtents.Y, -vExtents.Z);
            data[11].TextureCoordinate.X = 0.0f; data[11].TextureCoordinate.Y = 0.5f;

            //top
            data[12].Position = new Vector3(vExtents.X, vExtents.Y, -vExtents.Z);
            data[12].TextureCoordinate.X = 2.0f / 3.0f; data[12].TextureCoordinate.Y = 0.5f;
            data[13].Position = new Vector3(vExtents.X, vExtents.Y, vExtents.Z);
            data[13].TextureCoordinate.X = 2.0f / 3.0f; data[13].TextureCoordinate.Y = 1.0f;
            data[14].Position = new Vector3(-vExtents.X, vExtents.Y, vExtents.Z);
            data[14].TextureCoordinate.X = 1.0f; data[14].TextureCoordinate.Y = 1.0f;
            data[15].Position = new Vector3(-vExtents.X, vExtents.Y, -vExtents.Z);
            data[15].TextureCoordinate.X = 1.0f; data[15].TextureCoordinate.Y = 0.5f;


            //left
            data[16].Position = new Vector3(-vExtents.X, vExtents.Y, -vExtents.Z);
            data[16].TextureCoordinate.X = 1.0f / 3.0f; data[16].TextureCoordinate.Y = 0.0f;
            data[17].Position = new Vector3(-vExtents.X, vExtents.Y, vExtents.Z);
            data[17].TextureCoordinate.X = 0.0f; data[17].TextureCoordinate.Y = 0.0f;
            data[18].Position = new Vector3(-vExtents.X, -vExtents.Y, vExtents.Z);
            data[18].TextureCoordinate.X = 0.0f; data[18].TextureCoordinate.Y = 0.5f;
            data[19].Position = new Vector3(-vExtents.X, -vExtents.Y, -vExtents.Z);
            data[19].TextureCoordinate.X = 1.0f / 3.0f; data[19].TextureCoordinate.Y = 0.5f;

            //right
            data[20].Position = new Vector3(vExtents.X, -vExtents.Y, -vExtents.Z);
            data[20].TextureCoordinate.X = 2.0f / 3.0f; data[20].TextureCoordinate.Y = 0.5f;
            data[21].Position = new Vector3(vExtents.X, -vExtents.Y, vExtents.Z);
            data[21].TextureCoordinate.X = 1.0f; data[21].TextureCoordinate.Y = 0.5f;
            data[22].Position = new Vector3(vExtents.X, vExtents.Y, vExtents.Z);
            data[22].TextureCoordinate.X = 1.0f; data[22].TextureCoordinate.Y = 0.0f;
            data[23].Position = new Vector3(vExtents.X, vExtents.Y, -vExtents.Z);
            data[23].TextureCoordinate.X = 2.0f / 3.0f; data[23].TextureCoordinate.Y = 0.0f;

            vertices.SetData<VertexPositionTexture>(data);


            indices = new IndexBuffer(device,
                                typeof(short), 6 * 6,
                                BufferUsage.WriteOnly);

            short[] ib = new short[6 * 6];

            for (int x = 0; x < 6; x++)
            {
                ib[x * 6 + 0] = (short)(x * 4 + 0);
                ib[x * 6 + 2] = (short)(x * 4 + 1);
                ib[x * 6 + 1] = (short)(x * 4 + 2);

                ib[x * 6 + 3] = (short)(x * 4 + 2);
                ib[x * 6 + 5] = (short)(x * 4 + 3);
                ib[x * 6 + 4] = (short)(x * 4 + 0);
            }

            indices.SetData<short>(ib);
        }
    }
}
