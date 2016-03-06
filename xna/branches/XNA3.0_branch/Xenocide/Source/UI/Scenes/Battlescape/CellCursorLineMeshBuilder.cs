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
* @file CellCursor.cs
* @date Created: 2008/01/04
* @author File creator: David Teviotdale
* @author Credits: none
*/
#endregion

#region Using Statements

using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace ProjectXenocide.UI.Scenes.Battlescape
{
    /// <summary>
    /// Basically, a cube, drawn to show the cell the mouse is currently over
    /// </summary>
    public class CellCursorLineMeshBuilder : LineMeshBuilder
    {
        /// <summary>Ctor</summary>
        /// <param name="gridColor">Color to make the grid</param>
        public CellCursorLineMeshBuilder(Color gridColor)
        {
            this.gridColor = gridColor;
        }

        /// <summary>
        /// Fill the lists that define the lines
        /// </summary>
        /// <param name="verts">the endpoints of the lines</param>
        /// <param name="indexes">order to draw lines</param>
        public override void Build(IList<VertexPositionColor> verts, IList<short> indexes)
        {
            // Horizontal lines
            short j = 0;
            for (int y = 1; -7 <= y; --y)
            {
                verts.Add(MakeVertex(new Vector3(0.0f, y, 0.0f), gridColor));
                verts.Add(MakeVertex(new Vector3(1.0f, y, 0.0f), gridColor));
                verts.Add(MakeVertex(new Vector3(1.0f, y, 1.0f), gridColor));
                verts.Add(MakeVertex(new Vector3(0.0f, y, 1.0f), gridColor));
                indexes.Add(j);
                indexes.Add(++j);
                indexes.Add(j);
                indexes.Add(++j);
                indexes.Add(j);
                indexes.Add(++j);
                indexes.Add(j);
                indexes.Add((short)(j - 3));
                ++j;
                if (y != -7)
                {
                    indexes.Add((short)(j - 4));
                    indexes.Add(j);
                    indexes.Add((short)(j - 3));
                    indexes.Add((short)(j + 1));
                    indexes.Add((short)(j - 2));
                    indexes.Add((short)(j + 2));
                    indexes.Add((short)(j - 1));
                    indexes.Add((short)(j + 3));
                }
            }
        }

        /// <summary>
        /// Construct a vertex element
        /// </summary>
        /// <param name="pos">position of vertex, in cell space</param>
        /// <param name="color">vertex's color</param>
        /// <returns>vertex element</returns>
        private static VertexPositionColor MakeVertex(Vector3 pos, Color color)
        {
            return new VertexPositionColor(BattlescapeScene.CellToWorld(pos), color);
        }

        #region Fields

        /// <summary>Color to make the grid</summary>
        private Color gridColor;

        #endregion Fields
    }
}
