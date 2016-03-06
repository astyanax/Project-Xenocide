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
* @file RandomCityTerrainBuilder.cs
* @date Created: 2008/03/08
* @author File creator: David Teviotdale
* @author Credits: none
*/
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Globalization;

using Microsoft.Xna.Framework;

using ProjectXenocide.Model.Battlescape.Combatants;

namespace ProjectXenocide.Model.Battlescape
{
    /// <summary>
    ///  The combatants are going to fight in an environment
    /// The environment is modeled as a 3D array of cubical "cells"
    /// </summary>
    public partial class Terrain
    {
        /// <summary>Fills in the cells to define a "random city" terrain</summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible",
            Justification="We can handle nesting")]
        public class RandomCityTerrainBuilder : TerrainBuilder
        {
            /// <summary>
            /// Ctor
            /// </summary>
            public RandomCityTerrainBuilder()
                : base()
            {
            }

            /// <summary>Build the cells describing the Terrain</summary>
            protected override void BuildCellsCore()
            {
                BuildDefaultFaces();
                //            int rows = 120;
                //            int cols = 120;
                int rows = 24;
                int cols = 24;

                InitCells(cols, 1, rows);

                // Default whole level to grass
                BlockFill(0, 0, 0, cols, rows, new Cell(10, 0, 0));

                // add start area for X-Corp
                // start areas are 8 units east/west, by 4 units north/south
                BlockFill(0, 0, 0, 8, 4, new Cell(7, 0, 0));

                // add start area for aliens
                BlockFill(cols - 8, 0, rows - 4, 8, 4, new Cell(8, 0, 0));

                // scatter buildings across the terrain
                Random random = new Random(2);
                for (int x = 0; x < cols; x += 8)
                {
                    for (int z = 4; z < rows - 4; z += 8)
                    {
                        if (random.Next(7) < 4)
                        {
                            AddBuilding(x + 1, 0, z + 1, 6, 6, 5);
                        }
                    }
                }
            }

            /// <summary>
            /// Add a "building" to the terrain
            /// </summary>
            /// <param name="x">north west corner of building</param>
            /// <param name="y">north west corner of building</param>
            /// <param name="z">north west corner of building</param>
            /// <param name="columns">length of building, running east/west</param>
            /// <param name="rows">length of building, running north/south</param>
            /// <param name="wall">type of wall</param>
            private void AddBuilding(int x, int y, int z, int columns, int rows, byte wall)
            {
                // body of building
                BlockFill(x, y, z, columns, rows, new Cell(4, 0, 0));

                // walls
                AddNorthWall(x, y, z, columns, wall);
                AddNorthWall(x, y, z + rows, columns, wall);
                AddWestWall(x, y, z, rows, wall);
                AddWestWall(x + columns, y, z, rows, wall);
            }
        }
    }
}
