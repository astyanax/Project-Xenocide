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

#endregion

namespace ProjectXenocide.UI.Scenes.Facility
{
    /// <summary>
    /// A grid marking the cells in a X-Corp outpost that can hold a facility
    /// </summary>
    internal class Grid : LineMeshBuilder
    {
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="cellsWide">Size of grid (in cells) along width of display</param>
        /// <param name="cellsHigh">Size of grid (in cells) along height of display</param>
        public Grid(int cellsWide, int cellsHigh) 
        {
            this.cellsWide = cellsWide;
            this.cellsHigh = cellsHigh;
        }

        /// <summary>
        /// Fill the lists that define the lines
        /// </summary>
        /// <param name="verts">the endpoints of the lines</param>
        /// <param name="indexes">order to draw lines</param>
        public override void Build(IList<VertexPositionColor> verts, IList<short> indexes)
        {
            Color gridColor = Color.Red;

            // limits of grid
            float maxX = (cellsWide / 2.0f);
            float maxZ = (cellsHigh / 2.0f);

            // Horizontal lines
            short line = -1;
            for (float z = -maxZ; z <= maxZ; z += 1.0f)
            {
                verts.Add(new VertexPositionColor(new Vector3(-maxX, 0.0f, z), gridColor));
                verts.Add(new VertexPositionColor(new Vector3( maxX, 0.0f, z), gridColor));
                indexes.Add(++line);
                indexes.Add(++line);
            }

            // "vertical" lines
            for (float x = -maxX; x <= maxX; x += 1.0f)
            {
                verts.Add(new VertexPositionColor(new Vector3(x, 0.0f, -maxZ), gridColor));
                verts.Add(new VertexPositionColor(new Vector3(x, 0.0f,  maxZ), gridColor));
                indexes.Add(++line);
                indexes.Add(++line);
            }
        }

        #region Fields

        /// <summary>
        /// Size of grid (in cells) along width of display
        /// </summary>
        private int cellsWide;

        /// <summary>
        /// Size of grid (in cells) along height of display
        /// </summary>
        private int cellsHigh;

        #endregion Fields
    }
}
