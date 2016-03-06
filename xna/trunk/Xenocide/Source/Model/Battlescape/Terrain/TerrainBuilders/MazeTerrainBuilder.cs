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
* @file MazeTerrainBuilder.cs
* @date Created: 2008/03/14
* @author File creator: Oded Coster
* @author Credits: Algorithm based on http://www.aarg.net/~minam/dungeon_design.html
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
    public partial class Terrain
    {

        /// <summary>Build a dungeon like terrain</summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible",
            Justification = "We can handle nesting")]
        public class MazeTerrainBuilder : TerrainBuilder
        {
            /// <summary>
            /// Ctor
            /// </summary>
            public MazeTerrainBuilder()
                : base()
            {
            }

            /// <summary></summary>
            protected override void BuildCellsCore()
            {
                BuildDefaultFaces();

                int rows = 48;
                int cols = 48;
                int levels = 1;

                InitialiseCells(cols, levels, rows);
                GenerateMaze(cols, levels, rows);

                // validate (all north west cells should be visited)
                for (int i = 0; i < visitedCells.Length; ++i)
                {
                    Debug.Assert(visitedCells[i] == ((0 == (i % 2)) && (0 == ((i / Terrain.Width) % 2))));
                }

                MakeSparse(rows, cols, levels);
                Looping(rows, cols, levels);
                PlaceRandomRooms(rows, cols, levels);
            }

            /// <summary>Start with a 2 x 2 grid of cells (each cell with 4 walls) and start areas
            /// for X-Corp and Aliens.</summary>
            /// <remarks>must be 2x2, to allow for terrorist units</remarks>
            /// <param name="cols">map size, running west/east</param>
            /// <param name="levels">map size, running down/up</param>
            /// <param name="rows">map size, running north/south</param>
            private void InitialiseCells(int cols, int levels, int rows)
            {
                Debug.Assert(0 == (cols % 2));
                Debug.Assert(0 == (rows % 2));
                InitCells(cols, levels, rows);
                for (int y = 0; y < levels; ++y)
                {
                    // Default whole level to blue tiles
                    BlockFill(0, y, 0, cols, rows, new Cell(blueFloor, 0, 0));

                    // north/south walls
                    for (int x = 0; x < cols; x += 2)
                    {
                        AddWestWall(x, y, 0, rows, defaultWall);
                    }

                    // east/west walls
                    for (int z = 0; z < rows; z += 2)
                    {
                        AddNorthWall(0, y, z, cols, defaultWall);
                    }
                }

                // add start area for X-Corp and aliens
                // start areas are 8 units east/west, by 4 units north/south
                BlockFill(0, 0, 0, startAreaCols, startAreaRows, new Cell(7, 0, 0));
                BlockFill(cols - startAreaCols, 0, rows - startAreaRows, 8, 4, new Cell(8, 0, 0));
            }

            /// <summary>Convert dead ends into loops</summary>
            /// <param name="cols">map size, running west/east</param>
            /// <param name="levels">map size, running down/up</param>
            /// <param name="rows">map size, running north/south</param>
            private void Looping(int rows, int cols, int levels)
            {
                Debug.Assert(deadendToLoopPercentage > 0 && deadendToLoopPercentage < 101, "deadendToLoopPercentage should be between 1 and 100");
                for (int y = 0; y < levels; ++y)
                {
                    for (int x = 0; x < cols; x += 2)
                    {
                        for (int z = 0; z < rows; z += 2)
                        {
                            int j = Terrain.CellIndex(x, y, z);
                            if ((CountExits(j) == 1) && (D100() < deadendToLoopPercentage))
                                {
                                    DeadEndToLoop(j);
                                }
                        }
                    }
                }
            }

            /// <summary>Place rooms (open areas) at random on map</summary>
            /// <param name="rows">map size, running north/south</param>
            /// <param name="cols">map size, running west/east</param>
            /// <param name="levels">map size, running down/up</param>
            private void PlaceRandomRooms(int rows, int cols, int levels)
            {
                int maxRoomsPerLevel = (rows * cols / roomDensity);
                for (int y = 0; y < levels; ++y)
                {
                    for (int i = 0; i < maxRoomsPerLevel; ++i)
                    {
                        PlaceRandomRoom(rows, cols, y);
                    }
                }
            }

            /// <summary>Try to convert a dead end cell into a loop</summary>
            /// <param name="cellIndex">index to north west corner of cell</param>
            private void DeadEndToLoop(int cellIndex)
            {
                List<Side> directions = GetLoopDirections(cellIndex);
                if (0 < directions.Count)
                {
                    RemoveWall(cellIndex, directions[mazeRandom.Next(directions.Count)]);
                }
            }

            /// <summary>Put a room at a random location on the map</summary>
            /// <param name="rows">map size, running north/south</param>
            /// <param name="cols">map size, running west/east</param>
            /// <param name="level">map size, running down/up</param>
            private void PlaceRandomRoom(int rows, int cols, int level)
            {
                // room dimensions
                int width  = 4 + (mazeRandom.Next(3) * 2);
                int length = 4 + (mazeRandom.Next(3) * 2);

                // north west corner of room
                int west  = mazeRandom.Next((cols - width)/ 2) * 2;
                int north = mazeRandom.Next((rows - length)/ 2) * 2;

                // make sure room doesn't overwrite the start areas
                if ((north < startAreaRows) && (west < startAreaCols))
                {
                    west += startAreaCols;
                }
                if ((Terrain.length - startAreaRows < north) && (Terrain.width - startAreaCols < west))
                {
                    west -= startAreaCols;
                }

                // check that room will be connected to maze
                for (int z = north; z < north + length; ++z)
                {
                    int j = Terrain.CellIndex(0, level, z);
                    for (int x = west; x < west + width; ++x)
                    {
                        int index = j + x;
                        if (visitedCells[index])
                        {
                            // will be connected.  Build the room
                            CreateRoom(west, level, north, width, length);
                            return;
                        }
                    }
                }
            }

            /// <summary>Carve a "room" into the maze</summary>
            /// <param name="west">north west corner of the room</param>
            /// <param name="level">level of map room is on</param>
            /// <param name="north">north west corner of the room</param>
            /// <param name="width">length of room along west/east axis</param>
            /// <param name="length">length of room along north/south axis</param>
            private void CreateRoom(int west, int level, int north, int width, int length)
            {
                for (int z = north; z < north + length; ++z)
                {
                    int j = Terrain.CellIndex(0, level, z);
                    for (int x = west; x < west + width; ++x)
                    {
                        int index = j + x;
                        Cell cell = Terrain.cells[index];
                        cell.Ground = blueFloor;

                        // only remove the internal walls.  Leave the external ones
                        if (west != x)
                        {
                            cell.West = noWall;
                        }
                        if (north != z)
                        {
                            cell.North = noWall;
                        }
                        Terrain.cells[index] = cell;
                    }
                }
            }

            /// <summary>Wall off dead end 2x2 cells in maze</summary>
            /// <param name="cols">map size, running west/east</param>
            /// <param name="levels">map size, running down/up</param>
            /// <param name="rows">map size, running north/south</param>
            private void MakeSparse(int rows, int cols, int levels)
            {
                for (int i = 0; i < sparsnessRepeat; i++)
                {
                    for (int y = 0; y < levels; ++y)
                    {
                        for (int x = 0; x < cols; x += 2)
                        {
                            for (int z = 0; z < rows; z += 2)
                            {
                                int j = Terrain.CellIndex(x, y, z);
                                if (CountExits(j) == 1)
                                {
                                    //Block all directions
                                    BlockCell(j);
                                }
                            }
                        }
                    }
                }
            }

            /// <summary>Block off a cell from rest of the maze</summary>
            /// <param name="j">index to north west corner of cell</param>
            /// <returns>number of connections</returns>
            private void BlockCell(int j)
            {
                //Add walls in all directions
                Terrain.cells[j].North = defaultWall;
                Terrain.cells[j + 1].North = defaultWall;
                Terrain.cells[j].West = defaultWall;
                Terrain.cells[j + Terrain.width].West = defaultWall;

                if (IsNotOnSouthEdge(j))
                {
                    Terrain.cells[j + (Terrain.width * 2)].North = defaultWall;
                    Terrain.cells[j + (Terrain.width * 2) + 1].North = defaultWall;
                }

                if (IsNotOnEastEdge(j))
                {
                    Terrain.cells[j + 2].West = defaultWall;
                    Terrain.cells[j + Terrain.width + 2].West = defaultWall;
                }

                //Make sure cell is blocked
                Terrain.cells[j].Ground = blockedFloor;
                Terrain.cells[j + 1].Ground = blockedFloor;
                Terrain.cells[j + Terrain.width].Ground = blockedFloor;
                Terrain.cells[j + Terrain.width + 1].Ground = blockedFloor;

                visitedCells[j] = false;
            }

            /// <summary>Count number of connections a cell has with it's adjacent cells</summary>
            /// <param name="j">index to cell</param>
            /// <returns>number of connections</returns>
            private int CountExits(int j)
            {
                int i = 0;

                if (IsNotOnNorthEdge(j) && (Terrain.cells[j].North == noWall))
                    ++i;

                if (IsNotOnEastEdge(j) && (Terrain.cells[j + 2].West == noWall))
                    ++i;

                if (IsNotOnWestEdge(j) && (Terrain.cells[j].West == noWall))
                    ++i;

                if (IsNotOnSouthEdge(j) && (Terrain.cells[j + (Terrain.width * 2)].North == noWall))
                    ++i;

                return i;
            }

            private void GenerateMaze(int cols, int levels, int rows)
            {

                //Todo - currently only handles one level (levels = 1)
                Debug.Assert(levels == 1, "Cannot handle multiple level mazes");

                int maxCells = cols * levels * rows;

                //Step 1
                SetAllCellsToUnvisited(maxCells);

                // Step 2
                int x = mazeRandom.Next(Terrain.Width / 2) * 2;
                int z = mazeRandom.Next(Terrain.Length / 2) * 2;
                int currentCellIndex = Terrain.CellIndex(x, 0, z);
                Side direction = Side.Bottom;
                while (true)
                {
                    borderCells.Add(currentCellIndex);
                    visitedCells[currentCellIndex] = true;
                    VerifyCellIndexValid(currentCellIndex);

                    // case 3, can't go anywhere, start from existing maze
                    List<Side> directions = GetUnvisitedDirections(currentCellIndex);
                    while (0 == directions.Count)
                    {
                        // if no unvisted cells adjacent to visited ones, maze is done
                        if (0 == borderCells.Count)
                        {
                            return;
                        }

                        // pick a border cell at random
                        int index = mazeRandom.Next(borderCells.Count);
                        currentCellIndex = borderCells[index];
                        VerifyCellIndexValid(currentCellIndex);

                        // if it has no unvisited neighbours, discard and try again
                        directions = GetUnvisitedDirections(currentCellIndex);
                        if (0 == directions.Count)
                        {
                            borderCells[index] = borderCells[borderCells.Count - 1];
                            borderCells.RemoveAt(borderCells.Count - 1);
                        }
                    }

                    // if last used direction was good, see if we contine using it
                    // else, pick new direction
                    if (!directions.Contains(direction) || linearization <= D100())
                    {
                        direction = GetRandomDirection(directions);
                    }

                    //Step 4 - build corridor in direction and set the cell in direction to current cell
                    RemoveWall(currentCellIndex, direction);

                    switch(direction)
                    {
                        case Side.North:
                            currentCellIndex -= (Terrain.width * 2);
                            break;
                        case Side.South:
                            currentCellIndex += (Terrain.width * 2);
                            break;
                        case Side.East:
                            currentCellIndex += 2;
                            break;
                        case Side.West:
                            currentCellIndex -= 2;
                            break;
                    }
                }

                //Todo for multiple levels, add "steps" and grav lifts
            }

            /// <summary>Remove specified wall from 2x2 cell</summary>
            /// <param name="currentCellIndex">north west corner of cell</param>
            /// <param name="direction">wall of cell to remove</param>
            private void RemoveWall(int currentCellIndex, Side direction)
            {
                switch (direction)
                {
                    case Side.North:
                        Terrain.cells[currentCellIndex].North = noWall;
                        Terrain.cells[currentCellIndex + 1].North = noWall;
                        break;

                    case Side.South:
                        Terrain.cells[currentCellIndex + (Terrain.width * 2)].North = noWall;
                        Terrain.cells[currentCellIndex + (Terrain.width * 2) + 1].North = noWall;
                        break;

                    case Side.West:
                        Terrain.cells[currentCellIndex].West = noWall;
                        Terrain.cells[currentCellIndex + Terrain.width].West = noWall;
                        break;

                    case Side.East:
                        Terrain.cells[currentCellIndex + 2].West = noWall;
                        Terrain.cells[currentCellIndex + Terrain.width + 2].West = noWall;
                        break;

                    default:
                        Debug.Assert(false);
                        break;
                }
            }

            /// <summary>
            /// Return list of all directions that lead to unvisited cells from a given cell
            /// </summary>
            /// <param name="cellIndex">location of given cell</param>
            /// <returns>available directions</returns>
            private List<Side> GetUnvisitedDirections(int cellIndex)
            {
                List<Side> directions = new List<Side>();
                if (IsNotOnNorthEdge(cellIndex) && !VisitedNorth(cellIndex))
                    directions.Add(Side.North);
                if (IsNotOnEastEdge(cellIndex) && !VisitedEast(cellIndex))
                    directions.Add(Side.East);
                if (IsNotOnWestEdge(cellIndex) && !VisitedWest(cellIndex))
                    directions.Add(Side.West);
                if (IsNotOnSouthEdge(cellIndex) && !VisitedSouth(cellIndex))
                    directions.Add(Side.South);
                return directions;
            }

            /// <summary>Return list of all directions that could be used to from a loop from a given cell</summary>
            /// <param name="cellIndex">location of given cell</param>
            /// <returns>available directions</returns>
            private List<Side> GetLoopDirections(int cellIndex)
            {
                List<Side> directions = new List<Side>();
                if (IsNotOnNorthEdge(cellIndex) && VisitedNorth(cellIndex) && HasNorthWall(cellIndex))
                    directions.Add(Side.North);
                if (IsNotOnEastEdge(cellIndex) && VisitedEast(cellIndex) && HasEastWall(cellIndex))
                    directions.Add(Side.East);
                if (IsNotOnWestEdge(cellIndex) && VisitedWest(cellIndex) && HasWestWall(cellIndex))
                    directions.Add(Side.West);
                if (IsNotOnSouthEdge(cellIndex) && VisitedSouth(cellIndex) && HasSouthWall(cellIndex))
                    directions.Add(Side.South);
                return directions;
            }

            private bool IsNotOnNorthEdge(int cellIndex)
            {
                cellIndex %= (Terrain.Width * Terrain.Length);
                return ((Terrain.Width * 2) <= cellIndex);
            }

            private bool IsNotOnSouthEdge(int cellIndex)
            {
                cellIndex %= (Terrain.Width * Terrain.Length);
                return (cellIndex < ((Terrain.Length - 2) * Terrain.Width));
            }

            private bool IsNotOnWestEdge(int cellIndex)
            {
                //west edge test
                return (cellIndex % Terrain.Width) != 0;
            }

            private bool IsNotOnEastEdge(int cellIndex)
            {
                //east edge test
                return (cellIndex % Terrain.Width) < (Terrain.Width - 2);
            }

            /// <summary>Test if cell has a wall on the north side</summary>
            /// <param name="cellIndex">location of given cell</param>
            /// <returns>true if cell has a north wall</returns>
            private bool HasNorthWall(int cellIndex)
            {
                return Terrain.cells[cellIndex].North != noWall;
            }

            /// <summary>Test if cell has a wall on the south side</summary>
            /// <param name="cellIndex">location of given cell</param>
            /// <returns>true if cell has a south wall</returns>
            private bool HasSouthWall(int cellIndex)
            {
                return Terrain.cells[cellIndex + (Terrain.Width * 2)].North != noWall;
            }

            /// <summary>Test if cell has a wall on the west side</summary>
            /// <param name="cellIndex">location of given cell</param>
            /// <returns>true if cell has a west wall</returns>
            private bool HasWestWall(int cellIndex)
            {
                return Terrain.cells[cellIndex].West != noWall;
            }

            /// <summary>Test if cell has a wall on the est side</summary>
            /// <param name="cellIndex">location of given cell</param>
            /// <returns>true if cell has an east wall</returns>
            private bool HasEastWall(int cellIndex)
            {
                return Terrain.cells[cellIndex + 2].West != noWall;
            }

            /// <summary>Test if cell to north of given cell has been visited</summary>
            /// <param name="cellIndex">location of given cell</param>
            /// <returns>true if cell to north has been visited</returns>
            private bool VisitedNorth(int cellIndex)
            {
                return visitedCells[cellIndex - (Terrain.Width * 2)];
            }

            /// <summary>Test if cell to south of given cell has been visited</summary>
            /// <param name="cellIndex">location of given cell</param>
            /// <returns>true if cell to south has been visited</returns>
            private bool VisitedSouth(int cellIndex)
            {
                return visitedCells[cellIndex + (Terrain.Width * 2)];
            }

            /// <summary>Test if cell to west of given cell has been visited</summary>
            /// <param name="cellIndex">location of given cell</param>
            /// <returns>true if cell to west has been visited</returns>
            private bool VisitedWest(int cellIndex)
            {
                return visitedCells[cellIndex - 2];
            }

            /// <summary>Test if cell to east of given cell has been visited</summary>
            /// <param name="cellIndex">location of given cell</param>
            /// <returns>true if cell to east has been visited</returns>
            private bool VisitedEast(int cellIndex)
            {
                return visitedCells[cellIndex + 2];
            }

            private Side GetRandomDirection(List<Side> directions)
            {
                Debug.Assert(0 < directions.Count);
                return directions[mazeRandom.Next(directions.Count)];
            }


            private void SetAllCellsToUnvisited(int size)
            {
                visitedCells = new bool[size];
            }

            /// <summary>Roll a percentage between 0 and 99</summary>
            /// <returns>the percentage</returns>
            /// <remarks>essentially, do a dungeons and dragons D100 roll</remarks>
            private int D100()
            {
                return mazeRandom.Next(100);
            }

            /// <summary>Verify that cell index is north west corner of a cell</summary>
            /// <param name="cellIndex">Index to verify</param>
            [Conditional("DEBUG")]
            private void VerifyCellIndexValid(int cellIndex)
            {
                Debug.Assert(0 == (cellIndex % 2));
                Debug.Assert(0 == ((cellIndex / Terrain.Width)% 2));
            }

            #region Properties
            #endregion

            #region constants

            /// <summary>We're only using one type of wall at moment</summary>
            private const int defaultWall = 5;

            /// <summary>There is no wall</summary>
            private const int noWall = 0;

            /// <summary>Index to tile indicating cell blocked, combatants can't enter</summary>
            private const int blockedFloor = 4;

            /// <summary>Index to tile indicating cell is accessable</summary>
            private const int blueFloor = 11;

            /// <summary>% chance that maze will NOT pick a random direction for next cell</summary>
            /// <remarks>Basically, how non-twisty is maze, smaller number == more twisty maze</remarks>
            private const int linearization = 60;

            /// <summary>Number of "remove dead end" passes</summary>
            private int sparsnessRepeat = 1;

            /// <summary>% chance that dead ends will turned into loops (if possible)</summary>
            private int deadendToLoopPercentage = 50;

            /// <summary>Dimensions of X-Corp and alien forces start areas</summary>
            private int startAreaRows = 4;

            /// <summary>Dimensions of X-Corp and alien forces start areas</summary>
            private int startAreaCols = 8;

            /// <summary>One "room" per roomDensity cells</summary>
            private int roomDensity = 288;

            #endregion

            #region Fields

            /// <summary>list of cells that have been visited (i.e. are part of the maze)</summary>
            private bool[] visitedCells;

            /// <summary>index to cells that have been visited, and may have unvisited cells adjacent</summary>
            private List<int> borderCells = new List<int>();

            /// <summary>RNG used to create maze</summary>
            Random mazeRandom = new Random();

            #endregion
        }
    }
}
