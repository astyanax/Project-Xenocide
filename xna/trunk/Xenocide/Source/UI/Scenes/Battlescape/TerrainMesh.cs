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
* @file TerrainMesh.cs
* @date Created: 2008/01/01
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

using ProjectXenocide.Model.Battlescape;

#endregion

namespace ProjectXenocide.UI.Scenes.Battlescape
{

    /// <summary>
    /// The 3D model of the landscape that combatants are going to fight on
    /// </summary>
    public class TerrainMesh : IDisposable
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
                if (basicEffectVertexDeclaration != null)
                {
                    basicEffectVertexDeclaration.Dispose();
                    basicEffectVertexDeclaration = null;
                }
            }
        }

        #endregion IDisposable

        /// <summary>
        /// Load/create the graphic resources needed by the terrain
        /// </summary>
        /// <param name="device">the display</param>
        /// <param name="terrain">the terrain to build a mesh for</param>
        public void LoadContent(GraphicsDevice device, Terrain terrain)
        {
            textureAtlas = TextureAtlas.DefaultAtlas(device);
            ConstructMesh(device, terrain);
            basicEffectVertexDeclaration = new VertexDeclaration(
                device, VertexPositionNormalTexture.VertexElements);
        }

        /// <summary>
        /// Set up a BasicEffect for drawing the mesh
        /// </summary>
        /// <param name="effect">effect to use to draw the mesh</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
            Justification = "will throw if effect is null")]
        public void ConfigureEffect(BasicEffect effect)
        {
            effect.DiffuseColor = new Vector3(1.0f, 1.0f, 1.0f);
            effect.Texture = textureAtlas.Bitmap;
            effect.TextureEnabled = true;
        }

        /// <summary>
        /// Draw the terrain on the device
        /// </summary>
        /// <param name="device">Device to render the terrain</param>
        /// <param name="effect">effect to use to draw the terrain</param>
        /// <param name="topLevel">topmost level of terrain to draw</param>
        public void Draw(GraphicsDevice device, Effect effect, int topLevel)
        {
            device.VertexDeclaration = basicEffectVertexDeclaration;
            for (int i = 0; i <= topLevel; ++i)
            {
                if (0 < numQuads[i])
                {
                    device.Vertices[0].SetSource(vertexBuffers[i], 0, VertexPositionNormalTexture.SizeInBytes);
                    device.Indices = indexBuffers[i];

                    effect.Begin();
                    foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                    {
                        pass.Begin();

                        device.DrawIndexedPrimitives(
                             PrimitiveType.TriangleList,
                             0,
                             0,
                             numQuads[i] * 4,
                             0,
                             numQuads[i] * 2
                         );

                        pass.End();
                    }
                    effect.End();
                }
            }
        }

        /// <summary>
        /// Build the mesh we're going to draw on the display
        /// </summary>
        /// <param name="device">Device to render the terrain</param>
        /// <param name="terrain">the terrain to build a mesh for</param>
        public void ConstructMesh(GraphicsDevice device, Terrain terrain)
        {
            numQuads.Clear();
            DisposeBuffers();
            List<VertexPositionNormalTexture> verts = new List<VertexPositionNormalTexture>();
            List<int> indexes = new List<int>();
            for (int y = 0; y < terrain.Levels; ++y)
            {
                verts.Clear();
                indexes.Clear();
                for (int z = 0; z < terrain.Length; ++z)
                {
                    for (int x = 0; x < terrain.Width; ++x)
                    {
                        Vector3 pos = new Vector3(x, y, z);
                        // don't draw hidden cells
                        if (0 != (terrain.CellProperty(pos) & CellProperties.Seen))
                        {
                            for (int side = (int)Side.North; side <= (int)Side.Bottom; ++side)
                            {
                                AddFaceToMesh(pos, side, verts, indexes);
                            }
                        }
                    }
                }
                ConstructVertexBuffer(device, verts);
                ConstructIndexBuffer(device, indexes);
            }
        }

        /// <summary>
        /// Add a quad that represents the specifed face of specified cell to the mesh
        /// </summary>
        /// <param name="pos">position of cell</param>
        /// <param name="side">face of cell</param>
        /// <param name="verts">the mesh's vertices</param>
        /// <param name="indexes">the mesh's indices</param>
        private void AddFaceToMesh(Vector3 pos, int side, List<VertexPositionNormalTexture> verts,
            List<int> indexes)
        {
            Terrain terrain = Xenocide.GameState.Battlescape.Terrain;
            int textureId = terrain.GetFaceTexture(pos, (Side)side);
            if (0 != textureId)
            {
                TextureAtlas.Coord uv = textureAtlas.TextureCoords[textureId];
                // note when drawing south or east faces, need to flip texture because we're looking at back side of quad
                // and if drawing ground, adjust height
                switch ((Side)side)
                {
                    case Side.East:
                    case Side.South:
                        uv.FlipHorizontal();
                        break;

                    case Side.Bottom:
                        pos += new Vector3(0, terrain.GroundHeight(pos), 0);
                        break;
                }

                verts.Add(MakeVertex(pos + offset[side, 0], normals[side], uv.LeftBottom));
                verts.Add(MakeVertex(pos + offset[side, 1], normals[side], uv.LeftTop));
                verts.Add(MakeVertex(pos + offset[side, 2], normals[side], uv.RightTop));
                verts.Add(MakeVertex(pos + offset[side, 3], normals[side], uv.RightBottom));
                BindVerticesToQuad(verts, indexes);
            }
        }

        /// <summary>
        /// Construct a vertex element
        /// </summary>
        /// <param name="pos">position of vertex, in cell space</param>
        /// <param name="normal">vertex's normal</param>
        /// <param name="texture">vertex's uv texture co-ords</param>
        /// <returns>vertex element</returns>
        private static VertexPositionNormalTexture MakeVertex(Vector3 pos, Vector3 normal, Vector2 texture)
        {
            return new VertexPositionNormalTexture(BattlescapeScene.CellToWorld(pos), normal, texture);
        }

        /// <summary>
        /// Bind the last 4 vertices in the supplied list into a quad
        /// </summary>
        /// <remarks>the quads must have been added in clockwise winding order</remarks>
        /// <param name="verts"></param>
        /// <param name="indexes"></param>
        private static void BindVerticesToQuad(List<VertexPositionNormalTexture> verts, List<int> indexes)
        {
            int i = (verts.Count - 4);
            indexes.Add(i);
            indexes.Add(++i);
            indexes.Add(++i);
            indexes.Add(i);
            indexes.Add(++i);
            indexes.Add(i - 3);
        }

        /// <summary>
        /// build vertex buffer used to draw the mesh
        /// </summary>
        private void ConstructVertexBuffer(GraphicsDevice device, List<VertexPositionNormalTexture> verts)
        {
            numQuads.Add(verts.Count / 4);
            if (0 < verts.Count)
            {
                vertexBuffers.Add(new VertexBuffer(
                    device,
                    VertexPositionNormalTexture.SizeInBytes * verts.Count,
                    BufferUsage.None
                ));
                vertexBuffers[vertexBuffers.Count - 1].SetData<VertexPositionNormalTexture>(verts.ToArray());
            }
            else
            {
                vertexBuffers.Add(null);
            }
        }

        /// <summary>
        /// Build indexed triangle list used to draw the mesh
        /// </summary>
        private void ConstructIndexBuffer(GraphicsDevice device, List<int> indexes)
        {
            if (0 < indexes.Count)
            {
                indexBuffers.Add(new IndexBuffer(
                    device,
                    sizeof(int) * indexes.Count,
                    BufferUsage.None,
                    IndexElementSize.ThirtyTwoBits
                ));
                indexBuffers[indexBuffers.Count - 1].SetData<int>(indexes.ToArray());
            }
            else
            {
                indexBuffers.Add(null);
            }
        }

        /// <summary>Release the buffers cleanly</summary>
        private void DisposeBuffers()
        {
            for (int i = 0; i < indexBuffers.Count; ++i)
            {
                if (null != indexBuffers[i])
                {
                    indexBuffers[i].Dispose();
                }
            }
            for (int i = 0; i < vertexBuffers.Count; ++i)
            {
                if (null != vertexBuffers[i])
                {
                    vertexBuffers[i].Dispose();
                }
            }
            indexBuffers.Clear();
            vertexBuffers.Clear();
        }

        #region Fields

        /// <summary>
        /// the triangles making up the meshs for each level
        /// </summary>
        private List<IndexBuffer> indexBuffers = new List<IndexBuffer>();

        /// <summary>
        /// The vertices making up the meshs for each level
        /// </summary>
        private List<VertexBuffer> vertexBuffers = new List<VertexBuffer>();

        /// <summary>
        /// The textures we will paint onto the mesh
        /// </summary>
        private TextureAtlas textureAtlas;

        /// <summary>
        /// Used to tell the Graphics system what the structure of the vertices in memory
        /// </summary>
        private VertexDeclaration basicEffectVertexDeclaration;

        /// <summary>
        /// Number of quads in each mesh
        /// </summary>
        private List<int> numQuads = new List<int>();

        /// <summary>
        /// Normal for each of the faces
        /// </summary>
        private static readonly Vector3[] normals =
        {
            new Vector3(0,  0,  1),  // north wall
            new Vector3(-1, 0,  0),  // east wall
            new Vector3(1,  0,  0),  // west wall
            new Vector3(0,  0, -1),  // south wall
            new Vector3(0,  1,  0)   // ground
        };

        /// <summary>
        /// The co-ordinates of the vertices of each face, relative to the cell's bottom north west corner
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1814:PreferJaggedArraysOverMultidimensional", MessageId = "Member",
            Justification = "FxCop false positive")]
        private static readonly Vector3[,] offset =
        {
            { new Vector3(0,  0,  0), new Vector3(0,  1,  0), new Vector3(1,  1,  0), new Vector3(1,  0,  0) }, // north
            { new Vector3(1,  0,  0), new Vector3(1,  1,  0), new Vector3(1,  1,  1), new Vector3(1,  0,  1) }, // east
            { new Vector3(0,  0,  1), new Vector3(0,  1,  1), new Vector3(0,  1,  0), new Vector3(0,  0,  0) }, // west
            { new Vector3(1,  0,  1), new Vector3(1,  1,  1), new Vector3(0,  1,  1), new Vector3(0,  0,  1) }, // south
            { new Vector3(0,  0,  1), new Vector3(0,  0,  0), new Vector3(1,  0,  0), new Vector3(1,  0,  1) }  // ground
        };

        #endregion Fields
    }
}
