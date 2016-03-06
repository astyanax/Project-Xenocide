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
* @file PathMeshBuilder.cs
* @date Created: 2008/01/01
* @author File creator: David Teviotdale
* @author Credits: none
*/
#endregion

#region Using Statements

using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ProjectXenocide.Model.Battlescape;

#endregion

namespace ProjectXenocide.UI.Scenes.Battlescape
{
    /// <summary>
    /// Draw a Path made by the PathFinder
    /// </summary>
    public class PathMeshBuilder : LineMeshBuilder
    {
        /// <summary>
        /// Ctor
        /// </summary>
        public PathMeshBuilder()
        {
        }

        /// <summary>
        /// Fill the lists that define the lines
        /// </summary>
        /// <param name="verts">the endpoints of the lines</param>
        /// <param name="indexes">order to draw lines</param>
        public override void Build(IList<VertexPositionColor> verts, IList<short> indexes)
        {
            dirty = false;
            short line = -1;
            for (int i = 0; i < path.Count - 1; ++i)
            {
                verts.Add(MakeVertex(path[i]));
                verts.Add(MakeVertex(path[i + 1]));
                indexes.Add(++line);
                indexes.Add(++line);
            }
        }

        private static VertexPositionColor MakeVertex(MoveData cell)
        {
            // move co-ordinates from cell corner to middle of floor
            Vector3 v = new Vector3(cell.X + 0.5f, cell.Y + 0.1f, cell.Z + 0.5f);
            v += new Vector3(0, Xenocide.GameState.Battlescape.Terrain.GroundHeight(v), 0);
            return new VertexPositionColor(BattlescapeScene.CellToWorld(v), Color.Red);
        }

        #region Fields

        /// <summary>The path to draw</summary>
        public IList<MoveData> Path
        {
            get { return path; }
            set { path = value; dirty = true; }
        }

        /// <summary>do we need to regenerate the path mesh?</summary>
        public bool Dirty { get { return dirty; } }

        /// <summary>do we need to regenerate the path mesh</summary>
        private bool dirty;

        /// <summary>Path to draw on display</summary>
        private IList<MoveData> path = new List<MoveData>();

        #endregion Fields
    }
}
