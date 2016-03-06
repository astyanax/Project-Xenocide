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
* @file TerrainBuilder.cs
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
        /// <summary>Fills in the cells that define a terrain</summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible",
            Justification = "We can handle nesting")]
        public abstract class TerrainBuilder
        {
            /// <summary>
            /// Ctor
            /// </summary>
            /// <remarks>defined to shut up FxCop</remarks>
            protected TerrainBuilder() { }

            /// <summary>Build the cells describing the Terrain</summary>
            /// <param name="theTerrain">The terrain being constructed</param>
            public void BuildCells(Terrain theTerrain)
            {
                Terrain = theTerrain;
                BuildCellsCore();
            }

            /// <summary>Build the cells describing the Terrain</summary>
            protected abstract void BuildCellsCore();

            /// <summary>A basic set of tiles</summary>
            protected void BuildDefaultFaces()
            {
                // minimal set of ground tiles
                Terrain.groundFaces.Clear();
                Terrain.groundFaces.Add(new GroundFace(GroundFlag.None,          (int)CellFaceTexture.None));
                Terrain.groundFaces.Add(new GroundFace(GroundFlag.FloorHeight0,  (int)CellFaceTexture.GroundGreyTile));
                Terrain.groundFaces.Add(new GroundFace(GroundFlag.FloorHeight30, (int)CellFaceTexture.GroundGreyTile));
                Terrain.groundFaces.Add(new GroundFace(GroundFlag.FloorHeight70, (int)CellFaceTexture.GroundGreyTile));
                Terrain.groundFaces.Add(new GroundFace(GroundFlag.Blocked,       (int)CellFaceTexture.Water));

                Terrain.groundFaces.Add(new GroundFace(GroundFlag.FloorHeight0 | GroundFlag.UpGravLift,   (int)CellFaceTexture.GravLift));
                Terrain.groundFaces.Add(new GroundFace(GroundFlag.FloorHeight0 | GroundFlag.DownGravLift, (int)CellFaceTexture.GravLift));
                Terrain.groundFaces.Add(new GroundFace(GroundFlag.FloorHeight0 | GroundFlag.XCorpStart,   (int)CellFaceTexture.XCorpStart));
                Terrain.groundFaces.Add(new GroundFace(GroundFlag.FloorHeight0 | GroundFlag.AlienStart,   (int)CellFaceTexture.XCorpStart));

                // additional ground tiles
                Terrain.groundFaces.Add(new GroundFace(GroundFlag.FloorHeight0, (int)CellFaceTexture.GroundBrickPath));
                Terrain.groundFaces.Add(new GroundFace(GroundFlag.FloorHeight0, (int)CellFaceTexture.GroundGrass));
                Terrain.groundFaces.Add(new GroundFace(GroundFlag.FloorHeight0, (int)CellFaceTexture.GroundBlueTile));

                // minimal set of wall tiles
                Terrain.wallFaces.Clear();
                Terrain.wallFaces.Add(new WallFace(WallFlag.None,                    (int)CellFaceTexture.None));
                Terrain.wallFaces.Add(new WallFace(WallFlag.Solid | WallFlag.Opaque, (int)CellFaceTexture.WallWallpapered));
                Terrain.wallFaces.Add(new WallFace(WallFlag.Opaque,                  (int)CellFaceTexture.Door));
                Terrain.wallFaces.Add(new WallFace(WallFlag.Solid,                   (int)CellFaceTexture.Window));

                // additional wall tiles
                Terrain.wallFaces.Add(new WallFace(WallFlag.Solid | WallFlag.Opaque, (int)CellFaceTexture.WallWhiteMetal));
                Terrain.wallFaces.Add(new WallFace(WallFlag.Solid | WallFlag.Opaque, (int)CellFaceTexture.WallYellowMetal));
            }

            /// <summary>
            /// Fill an area of the terrain with a given cell
            /// </summary>
            /// <param name="x">north west corner of fill area</param>
            /// <param name="y">north west corner of fill area</param>
            /// <param name="z">north west corner of fill area</param>
            /// <param name="columns">number of cells, running east/west</param>
            /// <param name="rows">number of cells, running north/south</param>
            /// <param name="fill">what to put into the cells</param>
            protected void BlockFill(int x, int y, int z, int columns, int rows, Cell fill)
            {
                for (int i = 0; i < columns; ++i)
                {
                    for (int j = 0; j < rows; ++j)
                    {
                        Terrain.cells[Terrain.CellIndex(x + i, y, z + j)] = fill;
                    }
                }
            }

            /// <summary>
            /// Give a row of cells a north wall
            /// </summary>
            /// <param name="x">west most cell of row</param>
            /// <param name="y">west most cell of row</param>
            /// <param name="z">west most cell of row</param>
            /// <param name="wallLength">number of cells</param>
            /// <param name="wall">type of wall</param>
            protected void AddNorthWall(int x, int y, int z, int wallLength, byte wall)
            {
                for (int i = 0; i < wallLength; ++i)
                {
                    int index = Terrain.CellIndex(x + i, y, z);
                    Cell c = Terrain.cells[index];
                    c.North = wall;
                    Terrain.cells[index] = c;
                }
            }

            /// <summary>
            /// Give a column of cells a west wall
            /// </summary>
            /// <param name="x">north most cell of column</param>
            /// <param name="y">north most cell of column</param>
            /// <param name="z">north most cell of column</param>
            /// <param name="wallLength">number of cells</param>
            /// <param name="wall">type of wall</param>
            protected void AddWestWall(int x, int y, int z, int wallLength, byte wall)
            {
                for (int i = 0; i < wallLength; ++i)
                {
                    int index = Terrain.CellIndex(x, y, z + i);
                    Cell c = Terrain.cells[index];
                    c.West = wall;
                    Terrain.cells[index] = c;
                }
            }

            /// <summary>
            /// Set strip of the terrain to state specified by a string
            /// </summary>
            /// <param name="x">position of first cell in strip</param>
            /// <param name="y">position of first cell in strip</param>
            /// <param name="z">position of first cell in strip</param>
            /// <param name="facedata">data for cells, comma separated</param>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
                Justification = "Will throw if facedata is null")]
            protected void SetCells(int x, int y, int z, string facedata)
            {
                Terrain.SetCells(x, y, z, facedata);
            }

            /// <summary>
            /// Initialize the array holding the data on number of cells
            /// </summary>
            /// <param name="x">number of cells, running west(0) to east</param>
            /// <param name="y">number of cells, running bottom(0) to top</param>
            /// <param name="z">number of cells, running north(0) to south</param>
            protected void InitCells(int x, int y, int z)
            {
                Terrain.InitCells(x, y, z);
            }

            #region Fields

            /// <summary>The terrain being constructed</summary>
            public Terrain Terrain { get { return terrain; } set { terrain = value; } }

            /// <summary>The terrain being constructed</summary>
            private Terrain terrain;

            #endregion Fields
        }
    }
}
