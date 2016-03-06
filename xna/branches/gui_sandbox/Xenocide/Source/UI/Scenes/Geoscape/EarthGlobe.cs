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
* @file EarthGlobe.cs
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
    class EarthGlobe
    {
        Texture2D         texture;
        SphereMesh        sphereMesh;
        VertexDeclaration basicEffectVertexDeclaration;
        VertexBuffer      vertexBuffer;
        IndexBuffer       indexBuffer;

        /// <summary>
        /// Load/create the graphics resources the globe needs
        /// </summary>
        /// <param name="content">content manager that fetches the content</param>
        /// <param name="device">the display</param>
        public void LoadGraphicsContent(ContentManager content, GraphicsDevice device)
        {
            texture = content.Load<Texture2D>("Content\\Textures\\HPC-earth-map");
            InitializeSphere(device);
            basicEffectVertexDeclaration = new VertexDeclaration(
                device, VertexPositionNormalTexture.VertexElements);
        }

        private void InitializeSphere(GraphicsDevice device)
        {
            sphereMesh   = new SphereMesh(15);
            vertexBuffer = sphereMesh.getVertexBuffer(device);
            indexBuffer  = sphereMesh.getIndexBuffer(device);
        }

        /// <summary>
        /// Draw the world on the device
        /// </summary>
        /// <param name="device">Device to render the globe to</param>
        /// <param name="worldMatrix">Where to put the globle in the scene</param>
        /// <param name="effect">effect to use to draw the globe</param>
        public void Draw(GraphicsDevice device, Matrix worldMatrix, BasicEffect effect)
        {
            device.VertexDeclaration = basicEffectVertexDeclaration;

            device.Vertices[0].SetSource(
                 vertexBuffer, 0, VertexPositionNormalTexture.SizeInBytes);
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
                     sphereMesh.TotalVertexes,
                     0,
                     sphereMesh.TotalFaces
                 );

                pass.End();
            }
            effect.End();
        }

    }
}
