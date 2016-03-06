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
* @file TestTerrainBuilder.cs
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
    /// <summary>The various testing terrains</summary>
    public enum TestTerrain
    {
        /// <summary>Basic test map</summary>
        Standard,

        /// <summary>Used for testing line of sight</summary>
        LineOfSight,

        /// <summary>Simple one story house</summary>
        House,

        /// <summary>Barracks facility</summary>
        Barracks,
    }

    /// <summary>
    ///  The combatants are going to fight in an environment
    /// The environment is modeled as a 3D array of cubical "cells"
    /// </summary>
    public partial class Terrain
    {
        /// <summary>Fills in the cells that define various terrains for test purposes</summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible",
            Justification = "We can handle nesting")]
        public class TestTerrainBuilder : TerrainBuilder
        {
            /// <summary>
            /// Ctor
            /// </summary>
            /// <param name="testTerrain">Type of terrain to build</param>
            public TestTerrainBuilder(TestTerrain testTerrain)
                : base()
            {
                this.testTerrain = testTerrain;
            }

            /// <summary>Build the cells describing the Terrain</summary>
            protected override void BuildCellsCore()
            {
                switch (testTerrain)
                {
                    case TestTerrain.Standard:
                        BuildStandardTerrain();
                        break;

                    case TestTerrain.LineOfSight:
                        BuildLineOfSightTerrain();
                        break;

                    case TestTerrain.House:
                        BuildHouseTerrain();
                        break;

                    case TestTerrain.Barracks:
                        BuildBarracksTerrain();
                        break;

                    default:
                        Debug.Assert(false);
                        break;
                }
            }

            /// <summary>Build simple terrain used for most tests</summary>
            private void BuildStandardTerrain()
            {
                BuildDefaultFaces();
                InitCells(4, 3, 3);

                // ground floor
                SetCells(0, 0, 0, "070000,010000,050000,040000");
                SetCells(0, 0, 1, "010000,010000,010001,040000");
                SetCells(0, 0, 2, "020000,030000,010000,030000");

                // 2nd floor
                SetCells(0, 1, 0, "030000,020000,060000,000000");
                SetCells(0, 1, 1, "000000,000000,010000,080000");
                SetCells(0, 1, 2, "000000,000000,010000,000000");

                // 3rd floor
                SetCells(0, 2, 0, "000000,010000,010000,010000");
                SetCells(0, 2, 1, "010000,010000,010101,010001");
                SetCells(0, 2, 2, "010000,010000,010100,010000");
            }

            /// <summary>Build simple terrain used for line of sight tests</summary>
            private void BuildLineOfSightTerrain()
            {
                BuildDefaultFaces();
                int rows = 40;
                int cols = 40;

                InitCells(cols, 8, rows);

                // Bottom level is grass, rest of levels are air
                for (int i = 0; i < Terrain.levels; ++i)
                {
                    BlockFill(0, i, 0, cols, rows, new Cell((byte)((0 == i) ? 10 : 0), 0, 0));
                }

                // add start area for X-Corp
                // start areas are 8 units east/west, by 4 units north/south
                BlockFill(0, Terrain.levels - 1, 0, 1, 1, new Cell(7, 0, 0));

                // add start area for alien
                BlockFill(0, Terrain.levels - 1, 1, 1, 1, new Cell(8, 0, 0));

                // West wall near north west corner
                BlockFill(1, 0, 1, 1, 1, new Cell(10, 0, 1));

                // North wall near south west corner
                BlockFill(1, 0, rows - 1, 1, 1, new Cell(10, 1, 0));


                // North wall in middle of screen
                BlockFill(cols / 2, 4, rows / 2, 1, 1, new Cell(0, 1, 0));
            }

            /// <summary>Construct a simple terrain that mimics a single story house</summary>
            private void BuildHouseTerrain()
            {
                BuildDefaultFaces();
                InitCells(11, 1, 10);

                // ground floor
                SetCells(0, 0, 0, "080000,010101,010100,010100,010100,010100,010100,010100,010100,010100,0a0001");
                SetCells(0, 0, 1, "0a0000,010001,010000,010000,010000,010000,010000,010000,010000,010000,0a0001");
                SetCells(0, 0, 2, "0a0000,010001,010000,010000,010000,010000,010000,010000,010000,010000,0a0001");
                SetCells(0, 0, 3, "0a0000,010001,010000,010000,010000,010000,010000,010000,010000,010000,0a0001");
                SetCells(0, 0, 4, "0a0000,0a0100,0a0100,0a0100,010001,010000,010000,010000,010000,010000,0a0001");
                SetCells(0, 0, 5, "0a0000,0a0000,0a0000,0a0000,010101,010100,010000,010001,010100,010100,0a0001");
                SetCells(0, 0, 6, "0a0000,0a0000,0a0000,0a0000,010001,010000,010000,010001,010000,010000,0a0001");
                SetCells(0, 0, 7, "090000,090000,090000,090000,010000,010000,010000,010001,010000,010000,0a0001");
                SetCells(0, 0, 8, "0a0000,0a0000,0a0000,0a0000,010001,010000,010000,010001,010000,010000,0a0001");
                SetCells(0, 0, 9, "0a0000,0a0000,0a0000,0a0000,0a0100,0a0100,0a0100,0a0100,0a0100,0a0100,0a0000");
            }

            /// <summary>Build simple terrain used for line of sight tests</summary>
            private void BuildBarracksTerrain()
            {
                BuildDefaultFaces();
                InitCells(24, 1, 24);
                BuildBarracksAt(0, 0, 0);
            }

            /// <summary>Construct a set of cells that aproximately follow shape of a barracks facility</summary>
            /// <param name="x">co-ords of bottom north west corner of facility</param>
            /// <param name="y">co-ords of bottom, north west corner of facility</param>
            /// <param name="z">co-ords of bottom, north west corner of facility</param>
            private void BuildBarracksAt(int x, int y, int z)
            {
                // if co-ords are off terrain, an exception will be thrown
                //                                                                                            |                           |
                SetCells(x, y,   z, "040101,040100,040100,040100,040100,040100,040100,040100,040100,040100,010001,010000,010000,010000,040101,040100,040100,040100,040100,040100,040100,040100,040100,040100");
                SetCells(x, y, ++z, "040001,040000,040000,040000,040000,040000,040000,040000,040000,040000,010001,010000,010000,010000,040001,040000,040000,040000,040000,040000,040000,040000,040000,040000");
                SetCells(x, y, ++z, "040001,040000,040000,040000,040000,040000,040000,040000,040000,040000,010001,010000,010000,010000,040001,040000,040000,040000,040000,040000,040000,040000,040000,040000");

                SetCells(x, y, ++z, "040001,010101,010100,010100,010100,010100,010100,010100,010100,010100,010000,010000,010000,010000,010100,010100,010100,010100,010100,010100,010100,010100,010100,040001");
                SetCells(x, y, ++z, "040001,010001,010000,010000,010000,010000,010000,010000,010000,010000,010000,010000,010000,010000,010000,010000,010000,010000,010000,010000,010000,010000,010000,040001");

                SetCells(x, y, ++z, "040001,040100,040100,040100,040100,040100,040100,040100,040100,040100,010001,010000,010000,010000,040101,040100,040100,040100,040100,040100,040100,040100,040100,040000");
                SetCells(x, y, ++z, "040001,040000,040000,040000,040000,040000,040000,040000,040000,040000,010001,010000,010000,010000,040001,040000,040000,040000,040000,040000,040000,040000,040000,040000");
                SetCells(x, y, ++z, "040001,040000,040000,040000,040000,040000,040000,040000,040000,040000,010001,010000,010000,010000,040001,040000,040000,040000,040000,040000,040000,040000,040000,040000");
                SetCells(x, y, ++z, "040001,040000,040000,040000,040000,040000,040000,040000,040000,040000,010001,010000,010000,010000,040001,040000,040000,040000,040000,040000,040000,040000,040000,040000");
                SetCells(x, y, ++z, "040001,040000,040000,040000,040000,040000,040000,040000,040000,040000,070001,010000,010000,080000,040001,040000,040000,040000,040000,040000,040000,040000,040000,040000");

                SetCells(x, y, ++z, "010100,010100,010100,010100,010100,010100,010100,010100,010100,010100,010000,010000,010000,010000,010100,010100,010100,010100,010100,010100,010100,010100,010100,010100");
                SetCells(x, y, ++z, "010000,010000,010000,010000,010000,010000,010000,010000,010000,010000,010000,010000,010000,010000,010000,010000,010000,010000,010000,010000,010000,010000,010000,010000");
                SetCells(x, y, ++z, "010000,010000,010000,010000,010000,010000,010000,010000,010000,010000,010000,010000,010000,010000,010000,010000,010000,010000,010000,010000,010000,010000,010000,010000");
                SetCells(x, y, ++z, "010000,010000,010000,010000,010000,010000,010000,010000,010000,010000,010000,010000,010000,010000,010000,010000,010000,010000,010000,010000,010000,010000,010000,010000");

                SetCells(x, y, ++z, "040101,040100,040100,040100,040100,040100,040100,040100,040100,040100,010001,010000,010000,010000,010000,010000,010000,010000,010000,010000,010000,010000,010000,040101");
                SetCells(x, y, ++z, "040001,040000,040000,040000,040000,040000,040000,040000,040000,040000,010001,010000,010000,010000,010000,010000,010000,010000,010000,010000,010000,010000,010000,040001");
                SetCells(x, y, ++z, "040001,040000,040000,040000,040000,040000,040000,040000,040000,040000,010001,010000,010000,010000,010000,010000,010000,010000,010000,010000,010000,010000,010000,040001");
                SetCells(x, y, ++z, "040001,040000,040000,040000,040000,040000,040000,040000,040000,040000,010001,010000,010000,010000,010000,020000,020000,020000,020000,020000,020000,010000,010000,040001");
                SetCells(x, y, ++z, "040001,040000,040000,040000,040000,040000,040000,040000,040000,040000,010001,010000,010000,010000,010000,020000,020000,020000,020000,020000,020000,010000,010000,040001");

                SetCells(x, y, ++z, "040001,010101,010100,010100,010100,010100,010100,010100,010100,010100,010000,010000,010000,010000,010000,010000,010000,010000,010000,010000,010000,010000,010000,040001");
                SetCells(x, y, ++z, "040001,010001,010000,010000,010000,010000,010000,010000,010000,010000,010000,010000,010000,010000,010000,010000,010000,010000,010000,010000,010000,010000,010000,040001");

                SetCells(x, y, ++z, "040001,040100,040100,040100,040100,040100,040100,040100,040100,040100,010001,010000,010000,010000,040101,040100,040100,040100,040100,040100,040100,040100,040100,040000");
                SetCells(x, y, ++z, "040001,040000,040000,040000,040000,040000,040000,040000,040000,040000,010001,010000,010000,010000,040001,040000,040000,040000,040000,040000,040000,040000,040000,040000");
                SetCells(x, y, ++z, "040001,040000,040000,040000,040000,040000,040000,040000,040000,040000,010001,010000,010000,010000,040001,040000,040000,040000,040000,040000,040000,040000,040000,040000");
            }



            #region Fields

            /// <summary>Type of terrain to build</summary>
            private TestTerrain testTerrain;

            #endregion Fields
        }
    }
}
