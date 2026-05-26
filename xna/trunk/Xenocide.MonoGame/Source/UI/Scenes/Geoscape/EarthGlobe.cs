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
using System.Globalization;
using System.IO;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using NLog;

using ProjectXenocide.Utils;

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
                new VertexElement(0,VertexElementFormat.Vector3,
                                             VertexElementUsage.Position,0),
                new VertexElement(sizeof(float)*3,VertexElementFormat.Vector2,
                                             VertexElementUsage.TextureCoordinate,0),
                new VertexElement(sizeof(float)*5,VertexElementFormat.Vector3,
                                            VertexElementUsage.Normal,0),
                new VertexElement(sizeof(float)*8,VertexElementFormat.Vector3,
                                            VertexElementUsage.Tangent,0),
                new VertexElement(sizeof(float)*11,VertexElementFormat.Vector3,
                                            VertexElementUsage.Binormal,0),
            };

        public static readonly VertexDeclaration VertexDeclaration = new VertexDeclaration(VertexElements);

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
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        Texture2D diffuseTexture;
        Texture2D nightTexture;
        Texture2D normapMapTexture;

        SphereMesh sphereMesh;
        VertexBuffer vertexBuffer;
        IndexBuffer indexBuffer;


        /// <summary>
        /// Load/create the graphics resources the globe needs
        /// </summary>
        /// <param name="device">the display</param>
        public void LoadContent(GraphicsDevice device)
        {
            using (Profile.Time("EarthGlobe.LoadContent"))
            {
                diffuseTexture = LoadTextureCached(device, @"Content/Textures/Geoscape/EarthDiffuseMap.jpg",
                    @"Content/Textures/Geoscape/_LEGACY_EarthDiffuseMap.jpg");
                nightTexture = LoadTextureCached(device, @"Content/Textures/Geoscape/EarthNightMap.jpg",
                    @"Content/Textures/Geoscape/_LEGACY_EarthNightMap.png");
                normapMapTexture = LoadTextureCached(device, @"Content/Textures/Geoscape/EarthNormalMap.png", null);

                sphereMesh = new SphereMesh(15);
                vertexBuffer = sphereMesh.CreateVertexBuffer(device);
                indexBuffer = sphereMesh.CreateIndexBuffer(device);
            }
        }

        private static Texture2D LoadTextureCached(GraphicsDevice device, string primaryPath, string fallbackPath)
        {
            if (ContentCache.TryGetTexture(primaryPath, out var cached))
                return cached;

            var tex = TryLoadTexture(primaryPath, fallbackPath, device);
            ContentCache.StoreTexture(primaryPath, tex);
            return tex;
        }

        private static Texture2D TryLoadTexture(string primaryPath, string fallbackPath, GraphicsDevice device)
        {
            string loadPath = primaryPath;
            if (!File.Exists(primaryPath) && fallbackPath != null)
            {
                Logger.Warn("EarthGlobe: {0} not found, falling back to {1}", primaryPath, fallbackPath);
                loadPath = fallbackPath;
            }

            try
            {
                using (var fs = File.OpenRead(loadPath))
                    return Texture2D.FromStream(device, fs);
            }
            catch (Exception ex)
            {
                Logger.Warn("EarthGlobe: Failed to load {0}: {1}", loadPath, ex.Message);
                if (fallbackPath != null && loadPath != fallbackPath && File.Exists(fallbackPath))
                {
                    Logger.Warn("EarthGlobe: Falling back to {0}", fallbackPath);
                    using (var fs = File.OpenRead(fallbackPath))
                        return Texture2D.FromStream(device, fs);
                }
                throw;
            }
        }

        /// <summary>
        /// Draw the world on the device
        /// </summary>
        /// <param name="device">Device to render the globe to</param>
        /// <param name="effect">effect to use to draw the globe</param>
        public void Draw(GraphicsDevice device, Effect effect)
        {
            device.SetVertexBuffer(vertexBuffer);
            device.Indices = indexBuffer;

            effect.Parameters["GeoscapeTexture"].SetValue(diffuseTexture);
            effect.Parameters["NightTexture"].SetValue(nightTexture);
            effect.Parameters["NormalMapTexture"].SetValue(normapMapTexture);

            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();

                device.DrawIndexedPrimitives(
                     PrimitiveType.TriangleList,
                     0,
                     0,
                     sphereMesh.TotalVertexes,
                     0,
                     sphereMesh.TotalFaces
                 );
            }
        }

    }
}
