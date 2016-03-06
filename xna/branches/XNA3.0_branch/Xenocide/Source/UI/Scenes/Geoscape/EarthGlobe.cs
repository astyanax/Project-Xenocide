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
using System.Globalization;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

#endregion

namespace ProjectXenocide.UI.Scenes.Geoscape
{
    /// <summary>
    /// This is a custom vertex to use with normal mapping on the Earth Normal Mapped Shader
    /// </summary>
    [Serializable]    
    public struct GlobeVertex
    {
        /// <summary>
        /// The position of the vertex
        /// </summary>
        private Vector3 Position;
        /// <summary>
        /// Texture Coordinate
        /// </summary>
        private Vector2 TexCoord;
        /// <summary>
        /// The Normal of the vertex
        /// </summary>
        private Vector3 Normal;
        /// <summary>
        /// The Tangent of the vertex
        /// </summary>
        private Vector3 Tangent;
        /// <summary>
        /// The Binormal of the vertex
        /// </summary>
        private Vector3 Binormal;

        /// <summary>
        /// Definition of the vertex element to create the approapriate binding to the shaders.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2105:ArrayFieldsShouldNotBeReadOnly")]
        public static readonly VertexElement[] VertexElements =
           new VertexElement[] { 
                new VertexElement(0,0,VertexElementFormat.Vector3,
                                             VertexElementMethod.Default,VertexElementUsage.Position,0),
                new VertexElement(0,sizeof(float)*3,VertexElementFormat.Vector2,
                                             VertexElementMethod.Default,VertexElementUsage.TextureCoordinate,0),
                new VertexElement(0,sizeof(float)*5,VertexElementFormat.Vector3,
                                            VertexElementMethod.Default,VertexElementUsage.Normal,0),
                new VertexElement(0,sizeof(float)*8,VertexElementFormat.Vector3,
                                            VertexElementMethod.Default,VertexElementUsage.Tangent,0),
                new VertexElement(0,sizeof(float)*11,VertexElementFormat.Vector3,
                                            VertexElementMethod.Default,VertexElementUsage.Binormal,0),                
            };

        /// <summary>
        /// The contructor of this Vertex.
        /// </summary>
        /// <param name="position">The position of the vertex</param>
        /// <param name="normal">The normal of the vertex</param>
        /// <param name="tangent">The tangent of the vertex</param>
        /// <param name="binormal">The binormal of the vertex</param>
        /// <param name="texCoord">The UV coordinates</param>
        public GlobeVertex(Vector3 position, Vector3 normal, Vector3 tangent, Vector3 binormal, Vector2 texCoord)
        {
            Position = position;
            TexCoord = texCoord;
            Normal = normal;
            Tangent = tangent;
            Binormal = binormal;
        }


        /// <summary>
        /// Distinct operator
        /// </summary>
        /// <param name="left">left operator parameter</param>
        /// <param name="right">right operator parameter</param>
        /// <returns></returns>
        public static bool operator !=(GlobeVertex left, GlobeVertex right)
        {
            return left.GetHashCode() != right.GetHashCode();
        }

        /// <summary>
        /// Equals operator
        /// </summary>
        /// <param name="left">left operator parameter</param>
        /// <param name="right">right operator parameter</param>
        /// <returns></returns>
        public static bool operator ==(GlobeVertex left, GlobeVertex right)
        {
            return left.GetHashCode() == right.GetHashCode();
        }

        /// <summary>
        /// Returns if the objects are the equals
        /// </summary>
        /// <param name="obj">The object to compare</param>
        /// <returns>True if equals, false otherwise</returns>
        public override bool Equals(object obj)
        {
            return this == (GlobeVertex)obj;
        }

        /// <summary>
        /// The size in bytes of this vertex
        /// </summary>
        public static int SizeInBytes 
        { 
            get { return sizeof(float) * 14; } 
        }

        /// <summary>
        /// Hashcode for the object.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return Position.GetHashCode() | TexCoord.GetHashCode() | Normal.GetHashCode() | Tangent.GetHashCode() | Binormal.GetHashCode();
        }

        /// <summary>
        /// Simplified ToString method.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "{0},{1},{2}", Position.X, Position.Y, Position.Z);
        }
    }

    class EarthGlobe
    {
        Texture2D diffuseTexture;
        Texture2D nightTexture;
        Texture2D normapMapTexture;
        
        SphereMesh sphereMesh;
        VertexDeclaration basicEffectVertexDeclaration;
        VertexBuffer vertexBuffer;
        IndexBuffer indexBuffer;


        /// <summary>
        /// Load/create the graphics resources the globe needs
        /// </summary>
        /// <param name="device">the display</param>
        public void LoadContent(GraphicsDevice device)
        {
            diffuseTexture = Texture2D.FromFile(Xenocide.Instance.GraphicsDevice, @"Content\Textures\Geoscape\EarthDiffuseMap.jpg");
            nightTexture = Texture2D.FromFile(Xenocide.Instance.GraphicsDevice, @"Content\Textures\Geoscape\EarthNightMap.png");
            normapMapTexture = Texture2D.FromFile(Xenocide.Instance.GraphicsDevice, @"Content\Textures\Geoscape\EarthNormalMap.png");

            sphereMesh = new SphereMesh(15);
            vertexBuffer = sphereMesh.CreateVertexBuffer(device);
            indexBuffer = sphereMesh.CreateIndexBuffer(device);
            
            basicEffectVertexDeclaration = new VertexDeclaration(device, GlobeVertex.VertexElements);
        }

        /// <summary>
        /// Draw the world on the device
        /// </summary>
        /// <param name="device">Device to render the globe to</param>
        /// <param name="effect">effect to use to draw the globe</param>
        public void Draw(GraphicsDevice device, Effect effect)
        {
            device.VertexDeclaration = basicEffectVertexDeclaration;

            device.Vertices[0].SetSource( vertexBuffer, 0, GlobeVertex.SizeInBytes);
            device.Indices = indexBuffer;

            effect.Parameters["GeoscapeTexture"].SetValue(diffuseTexture);
            effect.Parameters["NightTexture"].SetValue(nightTexture);
            effect.Parameters["NormalMapTexture"].SetValue(normapMapTexture);

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
