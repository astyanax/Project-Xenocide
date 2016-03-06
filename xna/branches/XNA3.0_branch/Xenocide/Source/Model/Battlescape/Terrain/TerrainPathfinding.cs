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
* @file TerrainPathfinding.cs
* @date Created: 2008/01/19
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

namespace ProjectXenocide.Model.Battlescape
{
    /// <summary>
    /// The pathfinding functions associated with a terrain
    /// </summary>
    public partial class Terrain
    {
        /// <summary>
        /// Get list of cells that can be directly reached from a given cell
        /// </summary>
        /// <param name="x">location of starting cell</param>
        /// <param name="y">location of starting cell</param>
        /// <param name="z">location of starting cell</param>
        /// <param name="canFly">include cells that can only be reached by flying?</param>
        /// <param name="neighbours">return: list of cells that can be reached from starting cell</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2233:OperationsShouldNotOverflow", MessageId = "y+1")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2233:OperationsShouldNotOverflow", MessageId = "y-1")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2233:OperationsShouldNotOverflow", MessageId = "x+1")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2233:OperationsShouldNotOverflow", MessageId = "x-1")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2233:OperationsShouldNotOverflow", MessageId = "z+1")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2233:OperationsShouldNotOverflow", MessageId = "z-1")]
        public void ListAccessbleNeighbours(int x, int y, int z, bool canFly, IList<MoveData> neighbours)
        {
            neighbours.Clear();
            if (CanMoveUp(x, y, z, canFly))
            {
                neighbours.Add(new MoveData(x, y + 1, z, VerticalMoveCost));
            }
            if (CanMoveDown(x, y, z))
            {
                neighbours.Add(new MoveData(x, y - 1, z, VerticalMoveCost));
            }
            // north
            int result = CanMoveHorizontal(x, y, z, canFly, x, z - 1, HasSouthWall);
            if (2 != result)
            {
                neighbours.Add(new MoveData(x, y + result, z - 1, HorizontalMoveCost));
            }
            // south
            if (2 != (result = CanMoveHorizontal(x, y, z, canFly, x, z + 1, HasNorthWall)))
            {
                neighbours.Add(new MoveData(x, y + result, z + 1, HorizontalMoveCost));
            }
            // east
            if (2 != (result = CanMoveHorizontal(x, y, z, canFly, x + 1, z, HasWestWall)))
            {
                neighbours.Add(new MoveData(x + 1, y + result, z, HorizontalMoveCost));
            }
            // west
            if (2 != (result = CanMoveHorizontal(x, y, z, canFly, x - 1, z, HasEastWall)))
            {
                neighbours.Add(new MoveData(x - 1, y + result, z, HorizontalMoveCost));
            }
            // northeast
            if (2 != (result = CanMoveHorizontal(x, y, z, canFly, x + 1, z - 1, NorthEastBlocked)))
            {
                neighbours.Add(new MoveData(x + 1, y + result, z - 1, DiagonalMoveCost));
            }
            // northwest
            if (2 != (result = CanMoveHorizontal(x, y, z, canFly, x - 1, z - 1, NorthWestBlocked)))
            {
                neighbours.Add(new MoveData(x - 1, y + result, z - 1, DiagonalMoveCost));
            }
            // southwest
            if (2 != (result = CanMoveHorizontal(x, y, z, canFly, x - 1, z + 1, SouthWestBlocked)))
            {
                neighbours.Add(new MoveData(x - 1, y + result, z + 1, DiagonalMoveCost));
            }
            // southeast
            if (2 != (result = CanMoveHorizontal(x, y, z, canFly, x + 1, z + 1, SouthEastBlocked)))
            {
                neighbours.Add(new MoveData(x + 1, y + result, z + 1, DiagonalMoveCost));
            }
        }

        /// <summary>
        /// Check if a combatant can move upwards from a specified cell
        /// </summary>
        /// <param name="x">location of starting cell</param>
        /// <param name="y">location of starting cell</param>
        /// <param name="z">location of starting cell</param>
        /// <param name="canFly">allowed to use flying to get to destination cell?</param>
        /// <returns>true if can move upwards</returns>
        private bool CanMoveUp(int x, int y, int z, bool canFly)
        {
            // if we're at the topmost level, then we can't go any higher
            if ((Levels - 1) == y)
            {
                return false;
            }

            // can't go up if there's a unit in the cell above this one
            if (GetCell(x, y + 1, z).IsOccupied)
            {
                return false;
            }

            // if this is an up grav lift, then we can go up
            if (0 != (GetGroundFace(x, y, z).Flags & GroundFlag.UpGravLift))
            {
                return true;
            }

            // otherwise, we can only go up if we're a flying unit, and there is no floor above us
            return canFly && (GroundFlag.None == (GetGroundFace(x, y + 1, z).Flags &
                (GroundFlag.FloorHeight70 | GroundFlag.Blocked)));
        }

        /// <summary>
        /// Check if a combatant can move downwards from a specified cell
        /// </summary>
        /// <param name="x">location of starting cell</param>
        /// <param name="y">location of starting cell</param>
        /// <param name="z">location of starting cell</param>
        /// <returns>true if can move downwards</returns>
        private bool CanMoveDown(int x, int y, int z)
        {
            // if we're at the bottom level, then we can't go any higher
            if (0 == y)
            {
                return false;
            }

            // can't go down if there's a unit in the cell below this one
            if (GetCell(x, y - 1, z).IsOccupied)
            {
                return false;
            }

            // we can go down if this is a down grav lift
            GroundFlag flags = GetGroundFace(x, y, z).Flags;
            if (0 != (flags & GroundFlag.DownGravLift))
            {
                return true;
            }

            // otherwise, we can go down if, this cell has no floor and and cell below isn't blocked
            return (GroundFlag.None == ((flags & GroundFlag.FloorHeight70)
                | (GetGroundFace(x, y - 1, z).Flags & GroundFlag.Blocked)));
        }

        /// <summary>
        /// Do common "horizonal" move checks
        /// </summary>
        /// <param name="sx">location of starting cell</param>
        /// <param name="sy">location of starting cell</param>
        /// <param name="sz">location of starting cell</param>
        /// <param name="canFly">allowed to use flying to get to destination cell?</param>
        /// <param name="dx">location of destination cell</param>
        /// <param name="dz">location of destination cell</param>
        /// <param name="wallTest">test to use to see if there's a wall in the way</param>
        /// <returns>2 if can't move, otherwise, change in level, e.g. 1 = up a level</returns>
        private int CanMoveHorizontal(int sx, int sy, int sz, bool canFly, int dx, int dz, CellTest wallTest)
        {
            // check destination isn't outside the map
            if (!IsOnTerrain(dx, sy, dz))
            {
                return 2;
            }

            // check can move horizontally on cell
            GroundFace ground = GetGroundFace(sx, sy, sz);
            if (!ground.CanMoveOff(canFly))
            {
                return 2;
            }

            // if we're at top of a stair, then we try going up one level
            int  dy      = sy;
            if ((GroundFlag.FloorHeight70 == (ground.Flags & GroundFlag.FloorHeight70)) &&
                (GroundFlag.FloorHeight0 == (GetGroundFace(dx, dy + 1, dz).Flags & GroundFlag.FloorHeight70)))
            {
                ++dy;
                Debug.Assert(dy < levels);
            }

            // can we enter the cell?
            // cell needs to be not blocked.
            // and can't go from ground level tile to 70% tile
            // and not have a west wall
            GroundFlag destGround = GetGroundFace(dx, dy, dz).Flags;
            if ((0 != (destGround & GroundFlag.Blocked))
                || ((GroundFlag.FloorHeight70 == (destGround & GroundFlag.FloorHeight70)) && (0 == (ground.Flags & GroundFlag.FloorHeight30)))
                || wallTest(new Vector3(sx, dy, sz), new Vector3(dx, dy, dz)))
            {
                return 2;
            }
            else
            {
                // if there's not floor and we're not a flying unit, fall down
                if (!canFly)
                {
                    while ((0 == (destGround & GroundFlag.FloorHeight70)) && (0 < dy))
                    {
                        --dy;
                        destGround = GetGroundFace(dx, dy, dz).Flags;
                    }
                }

                // check destination isn't already occupied
                if (GetCell(dx, dy, dz).IsOccupied)
                {
                    return 2;
                }

                return (dy - sy);
            }
        }

        /// <summary>Test if cells at given locations satisfies specified condition</summary>
        /// <param name="start">position of starting cell</param>
        /// <param name="end">position of ending cell</param>
        /// <returns>true if condition satisfied</returns>
        private delegate bool CellTest(Vector3 start, Vector3 end);

        /// <summary>Test if cell at end location has a north wall</summary>
        /// <param name="start">position of starting cell</param>
        /// <param name="end">position of ending cell</param>
        /// <returns>true if cell has wall</returns>
        private bool HasNorthWall(Vector3 start, Vector3 end)
        {
            return (0 != (GetWallFace(end, Side.North).Flags & WallFlag.Solid));
        }

        /// <summary>Test if cell at end location has a south wall</summary>
        /// <param name="start">position of starting cell</param>
        /// <param name="end">position of ending cell</param>
        /// <returns>true if cell has wall</returns>
        private bool HasSouthWall(Vector3 start, Vector3 end)
        {
            return (0 != (GetWallFace(start, Side.North).Flags & WallFlag.Solid));
        }

        /// <summary>Test if cell at given location has an east wall</summary>
        /// <param name="start">position of starting cell</param>
        /// <param name="end">position of ending cell</param>
        /// <returns>true if cell has wall</returns>
        private bool HasEastWall(Vector3 start, Vector3 end)
        {
            return (0 != (GetWallFace(start, Side.West).Flags & WallFlag.Solid));
        }

        /// <summary>Test if cell at end location has a west wall</summary>
        /// <param name="start">position of starting cell</param>
        /// <param name="end">position of ending cell</param>
        /// <returns>true if cell has wall</returns>
        private bool HasWestWall(Vector3 start, Vector3 end)
        {
            return (0 != (GetWallFace(end, Side.West).Flags & WallFlag.Solid));
        }

        /// <summary>Test if there are walls blocking movement from start to end cell</summary>
        /// <param name="start">position of starting cell</param>
        /// <param name="end">position of ending cell</param>
        /// <returns>true if walls block movement</returns>
        private bool NorthEastBlocked(Vector3 start, Vector3 end)
        {
            return (0 != ((GetWallFace(start, Side.North).Flags | GetWallFace(end, Side.West).Flags |
                GetWallFace(start, Side.East).Flags  | GetWallFace(end, Side.South).Flags) & WallFlag.Solid));
        }

        /// <summary>Test if there are walls blocking movement from start to end cell</summary>
        /// <param name="start">position of starting cell</param>
        /// <param name="end">position of ending cell</param>
        /// <returns>true if walls block movement</returns>
        private bool NorthWestBlocked(Vector3 start, Vector3 end)
        {
            return (0 != ((GetWallFace(start, Side.North).Flags | GetWallFace(end, Side.East).Flags |
                GetWallFace(start, Side.West).Flags  | GetWallFace(end, Side.South).Flags) & WallFlag.Solid));
        }

        /// <summary>Test if there are walls blocking movement from start to end cell</summary>
        /// <param name="start">position of starting cell</param>
        /// <param name="end">position of ending cell</param>
        /// <returns>true if walls block movement</returns>
        private bool SouthWestBlocked(Vector3 start, Vector3 end)
        {
            return (0 != ((GetWallFace(start, Side.South).Flags | GetWallFace(end, Side.East).Flags |
                GetWallFace(start, Side.West).Flags | GetWallFace(end, Side.North).Flags) & WallFlag.Solid));
        }

        /// <summary>Test if there are walls blocking movement from start to end cell</summary>
        /// <param name="start">position of starting cell</param>
        /// <param name="end">position of ending cell</param>
        /// <returns>true if walls block movement</returns>
        private bool SouthEastBlocked(Vector3 start, Vector3 end)
        {
            return (0 != ((GetWallFace(start, Side.South).Flags | GetWallFace(end, Side.West).Flags |
                GetWallFace(start, Side.East).Flags | GetWallFace(end, Side.North).Flags) & WallFlag.Solid));
        }

        #region Constants

        /// <summary>Cost to move up/down</summary>
        public const int VerticalMoveCost = 2;

        /// <summary>Cost to move north/south/east/west</summary>
        public const int HorizontalMoveCost = 2;

        /// <summary>Cost to move northeast/northwest/southeast/southwest</summary>
        /// <remarks>ideally, this should be 1.414 x HorizontalMoveCost, but that's too big</remarks>
        public const int DiagonalMoveCost = 3;

        #endregion Constants

    }
}
