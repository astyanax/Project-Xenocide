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

namespace Xenocide.UI.Scenes.Geoscape
{
    /// <summary>
    /// The skybox (obviously)
    /// </summary>
    class SkyBox
    {
        private Texture2D               texture;
        public  VertexDeclaration       basicEffectVertexDeclaration;
        private VertexPositionTexture[] meshVertices;
        private short[]                 meshIndices;
        VertexBuffer                    vertexBuffer;
        IndexBuffer                     indexBuffer; 

         /// <summary>
        /// Constructor (obviously)
        /// </summary>
        public SkyBox() { }

        /// <summary>
        /// Load/create the graphic resources needed by the skybox
        /// </summary>
        /// <param name="content">content manager that fetches the content</param>
        /// <param name="device">the display</param>
        public void LoadGraphicsContent(ContentManager content, GraphicsDevice device)
        {
            texture = content.Load<Texture2D>("Content\\Textures\\skybox");
            InitializeMesh();
            constructVertexBuffer(device);
            constructIndexBuffer(device);
            basicEffectVertexDeclaration = new VertexDeclaration(
                device, VertexPositionTexture.VertexElements);
        }

        private void InitializeMesh()
        {
            Vector3 topLeftFront = new Vector3(-10.0f, 10.0f, 10.0f);
            Vector3 bottomLeftFront = new Vector3(-10.0f, -10.0f, 10.0f);
            Vector3 topRightFront = new Vector3(10.0f, 10.0f, 10.0f);
            Vector3 bottomRightFront = new Vector3(10.0f, -10.0f, 10.0f);
            Vector3 topLeftBack = new Vector3(-10.0f, 10.0f, -10.0f);
            Vector3 topRightBack = new Vector3(10.0f, 10.0f, -10.0f);
            Vector3 bottomLeftBack = new Vector3(-10.0f, -10.0f, -10.0f);
            Vector3 bottomRightBack = new Vector3(10.0f, -10.0f, -10.0f);

            Vector2 textureTopLeft = new Vector2(0.0f, 0.0f);
            Vector2 textureTopRight = new Vector2(1.0f, 0.0f);
            Vector2 textureBottomLeft = new Vector2(0.0f, 1.0f);
            Vector2 textureBottomRight = new Vector2(1.0f, 1.0f);

            meshVertices = new VertexPositionTexture[24];

            // Front face
            meshVertices[0]  = new VertexPositionTexture(topLeftFront,     textureTopRight);
            meshVertices[1]  = new VertexPositionTexture(bottomLeftFront,  textureBottomRight);
            meshVertices[2]  = new VertexPositionTexture(topRightFront,    textureTopLeft);
            meshVertices[3]  = new VertexPositionTexture(bottomRightFront, textureBottomLeft);

            // Back face 
            meshVertices[4]  = new VertexPositionTexture(topLeftBack,     textureTopLeft);
            meshVertices[5]  = new VertexPositionTexture(topRightBack,    textureTopRight);
            meshVertices[6]  = new VertexPositionTexture(bottomLeftBack,  textureBottomLeft);
            meshVertices[7]  = new VertexPositionTexture(bottomRightBack, textureBottomRight);

            // Top face
            meshVertices[8]  = new VertexPositionTexture(topLeftFront,  textureBottomLeft);
            meshVertices[9]  = new VertexPositionTexture(topRightBack,  textureTopRight);
            meshVertices[10] = new VertexPositionTexture(topLeftBack,   textureTopLeft);
            meshVertices[11] = new VertexPositionTexture(topRightFront, textureBottomRight);

            // Bottom face 
            meshVertices[12] = new VertexPositionTexture(bottomLeftFront,  textureTopLeft);
            meshVertices[13] = new VertexPositionTexture(bottomLeftBack,   textureBottomLeft);
            meshVertices[14] = new VertexPositionTexture(bottomRightBack,  textureBottomRight);
            meshVertices[15] = new VertexPositionTexture(bottomRightFront, textureTopRight);

            // Left face
            meshVertices[16] = new VertexPositionTexture(topLeftFront,    textureTopLeft);
            meshVertices[17] = new VertexPositionTexture(bottomLeftBack,  textureBottomRight);
            meshVertices[18] = new VertexPositionTexture(bottomLeftFront, textureBottomLeft);
            meshVertices[19] = new VertexPositionTexture(topLeftBack,     textureTopRight);
            // Right face 
            meshVertices[20] = new VertexPositionTexture(topRightFront, textureTopRight);
            meshVertices[21] = new VertexPositionTexture(bottomRightFront, textureBottomRight);
            meshVertices[22] = new VertexPositionTexture(bottomRightBack, textureBottomLeft);
            meshVertices[23] = new VertexPositionTexture(topRightBack, textureTopLeft);

            meshIndices = new short[36]{
                // front face
                0, 2, 1, 2, 3, 1,

                // back face
                4, 6, 5, 6, 7, 5,

                // top face
                10, 9, 8, 9, 11, 8,

                // bottom face
                13, 12, 14, 12, 15, 14,

                // left face
                16, 18, 19, 18, 17, 19,

                // right face
                23, 22, 20, 22, 21, 20
            };
        }

        /// <summary>
        /// construct a vertex buffer that can be used to draw the skybox
        /// </summary>
        public void constructVertexBuffer(GraphicsDevice device)
        {
            vertexBuffer = new VertexBuffer(
                device,
                VertexPositionTexture.SizeInBytes * meshVertices.Length,
                ResourceUsage.None,
                ResourceManagementMode.Automatic
                );
            vertexBuffer.SetData<VertexPositionTexture>(meshVertices);
        }

        /// <summary>
        /// construct the triangle list that can be used to draw the skybox.
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
        /// Draw the skybox on the device
        /// </summary>
        /// <param name="device">Device to render the skybox to</param>
        /// <param name="worldMatrix">Where to put the skybox in the scene</param>
        /// <param name="effect">effect to use to draw the skybox</param>
        public void Draw(GraphicsDevice device, Matrix worldMatrix, BasicEffect effect)
        {
            device.VertexDeclaration = basicEffectVertexDeclaration;

            device.Vertices[0].SetSource(
                 vertexBuffer, 0, VertexPositionTexture.SizeInBytes);
            device.Indices = indexBuffer;

            effect.World = worldMatrix;
            effect.Texture = texture;
            effect.TextureEnabled = true;

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
}
