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
* @file Terrain.cs
* @date Created: 2008/01/01
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
    /// <summary>Extra checks/processing to do when running IsLineOfSight</summary>
    public enum VisibilityChecks
    {
        /// <summary>No extra processing</summary>
        None,

        /// <summary>Doing line of sight checks, combatants will block line of sight</summary>
        LineOfFire,

        /// <summary>Update the visibility matrix with the cells that are visible</summary>
        MarkVisible,
    }

    /// <summary>
    ///  The combatants are going to fight in an environment
    /// The environment is modeled as a 3D array of cubical "cells"
    /// </summary>
    [Serializable]
    public partial class Terrain
    {
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="mission">The type of mission. e.g. Attacking landed UFO, Terror site, etc.</param>
        public Terrain(Mission mission)
        {
            Debug.Assert(null != mission);  // shut up unused variable warning

            //ToDo: build terrain using mission information
            // for moment, just use hard coded one.
        }

        /// <summary>
        /// Return the index to the texture to use
        /// </summary>
        /// <param name="pos">position of cell</param>
        /// <param name="side">face of cell to get texture for</param>
        /// <returns>index to texture to use when drawing cell's face</returns>
        public int GetFaceTexture(Vector3 pos, Side side)
        {
            switch (side)
            {
                case Side.Bottom:
                    return groundFaces[GetCell(pos).Ground].TextureId;

                case Side.North:
                case Side.West:
                case Side.South:
                case Side.East:
                    return GetWallFace(pos, side).TextureId;

                // ceilings are never textured
                case Side.Top:
                default:
                    Debug.Assert(false);
                    return 0;
            }
        }

        /// <summary>
        /// Return the face of the specified 
        /// </summary>
        /// <param name="pos">position of cell</param>
        /// <param name="side">face of cell to get texture for</param>
        /// <returns>index to texture to use when drawing cell's face</returns>
        public WallFace GetWallFace(Vector3 pos, Side side)
        {
            int index = 0;
            switch (side)
            {
                case Side.North:
                    index = GetCell(pos).North;
                    break;

                case Side.West:
                    index = GetCell(pos).West;
                    break;

                case Side.South:
                    if (pos.Z < length - 1)
                    {
                        index = GetCell(pos + new Vector3(0, 0, 1)).North;
                    }
                    break;

                case Side.East:
                    if (pos.X < width - 1)
                    {
                        index = GetCell(pos + new Vector3(1, 0, 0)).West;
                    }
                    break;

                default:
                    Debug.Assert(false);
                    break;
            }
            return wallFaces[index];
        }

        /// <summary>Adjusts a position to be center of cell's floor</summary>
        /// <param name="pos">position, must be co-ords of cell's bottom north west corner</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId = "0#",
            Justification="We're not idiots")]
        public void ToFloorCenter(ref Vector3 pos)
        {
            Debug.Assert(pos.X == (int)pos.X);
            Debug.Assert(pos.Y == (int)pos.Y);
            Debug.Assert(pos.Z == (int)pos.Z);
            pos.Y += GroundHeight(pos);
            pos.X += 0.5f;
            pos.Z += 0.5f;
        }

        /// <summary>
        /// Get how far above the level's "ground" the cell's floor is
        /// </summary>
        /// <param name="pos">position of cell</param>
        /// <returns>height above level's ground, in vertical cell units</returns>
        public float GroundHeight(Vector3 pos)
        {
            return groundFaces[GetCell(pos).Ground].Height;
        }

        /// <summary>
        /// Return the face of the specified 
        /// </summary>
        /// <param name="x">cell, running west(0) to east</param>
        /// <param name="y">cell, running bottom(0) to top</param>
        /// <param name="z">cell, running north(0) to south</param>
        /// <returns>The Ground Face of requested cell</returns>
        public GroundFace GetGroundFace(int x, int y, int z)
        {
            return groundFaces[GetCell(x, y, z).Ground];
        }

        /// <summary>Record a change in position of a combatant</summary>
        /// <param name="combatant">the combatant</param>
        /// <param name="old">combatant's old position</param>
        /// <param name="newPos">combatant's new position</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
            Justification="Will throw if combatant is null")]
        public void MoveCombatant(Combatant combatant, MoveData old, MoveData newPos)
        {
            Debug.Assert(combatant.CombatantId == cells[CellIndex(old)].CombatantId);
            Debug.Assert(!cells[CellIndex(newPos)].IsOccupied);
            cells[CellIndex(old)].CombatantId    = 0;
            cells[CellIndex(newPos)].CombatantId = combatant.CombatantId;
            UpdateViewMatrix(combatant);
        }

        /// <summary>Remove a combatant from the terrain</summary>
        /// <param name="combatant">combatant to remove</param>
        /// <remarks>Remove dead/unconscious from line of sight and pathfinding data</remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
            Justification="will throw if combatant is null")]
        public void RemoveCombatant(Combatant combatant)
        {
            int index = CellIndex(combatant.Position);
            Debug.Assert(combatant.CombatantId == cells[index].CombatantId);
            cells[index].CombatantId = 0;
        }

        /// <summary>
        /// Find if there's a combatant in a cell on the Battlescape
        /// </summary>
        /// <param name="position">location of cell</param>
        /// <returns>CombatantId if found, 0 if nothing there</returns>
        public int FindCombatantAt(Vector3 position)
        {
            return GetCell(position).CombatantId;
        }

        /// <summary>
        /// Put the alien troops at their starting positions
        /// </summary>
        /// <param name="team">alien team to place</param>
        public void DeployAlienTeam(Team team)
        {
            DeployTeam(team, GroundFlag.AlienStart);
        }

        /// <summary>
        /// Put the XCorp troops at their starting positions
        /// </summary>
        /// <param name="team">XCorp team to place</param>
        public void DeployXCorpTeam(Team team)
        {
            DeployTeam(team, GroundFlag.XCorpStart);
        }

        /// <summary>
        /// Calculate shortest angle to turn through to get from a heading to angle facing a cell
        /// </summary>
        /// <param name="startHeading">starting heading, in radians</param>
        /// <param name="deltaX">number of cells west (-ve) or east (+ve) target cell is from start point</param>
        /// <param name="deltaZ">number of cells north (-ve) or south(+ve) target cell is from start point</param>
        /// <returns>shortest angle, in radians</returns>
        public static double CalcTurnAngle(double startHeading, int deltaX, int deltaZ)
        {
            if ((0 == deltaX) && (0 == deltaZ))
            {
                // special case, no turn
                return 0.0;
            }
            else
            {
                double angle = Math.Atan2(-deltaZ, deltaX) - startHeading;
                // check for case where going the other way is shorter.
                if (Math.PI < Math.Abs(angle))
                {
                    angle = ((-2 * Math.PI) + Math.Abs(angle)) * Math.Sign(angle);
                }
                // if angle is sufficiently small, ignore it
                if (Math.Abs(angle) < 0.00001f)
                {
                    angle = 0;
                }
                return angle;
            }
        }

        /// <summary>calculate which opposing combatants can see/are seen by a combatant</summary>
        /// <param name="combatant">to do calcuations for</param>
        /// <param name="enemies">the opposing combatants</param>
        public void UpdateVisibility(Combatant combatant, IList<Combatant> enemies)
        {
            // flag to mark combatants visibility to enemies
            int combatantFlag = (1 << combatant.PlaceInTeam);

            int i = -1;
            foreach (Combatant enemy in enemies)
            {
                bool      enemyVisible     = false;
                bool      combatantVisible = false;
                int       enemyFlag        = (1 << ++i);
                // to be visible, must be in range and have a line of sight
                if ((Vector3.DistanceSquared(combatant.Position, enemy.Position) < Combatant.VisionRangeSquared) &&
                    IsLineOfSight(combatant.Position, enemy.Position))
                {
                    // Ignore Vertical component of difference
                    Vector3 diff     = enemy.Position - combatant.Position;
                    bool    vertical = (Math.Abs(diff.X) + Math.Abs(diff.Z)) < 0.5f;
                    diff.Y = 0;
                    diff.Normalize();

                    // combatant must be facing enemy for enemy to be visible
                    if ((Combatant.DotFieldOfView < Vector3.Dot(diff, combatant.HeadingVector)) || vertical)
                    {
                        enemyVisible = true;
                    }

                    // enemy must be facing combatant for combatant to be visible
                    if ((Vector3.Dot(diff, enemy.HeadingVector) < -Combatant.DotFieldOfView) || vertical)
                    {
                        combatantVisible = true;
                    }
                }

                if (enemyVisible)
                {
                    combatant.OpponentsInView  |= enemyFlag;
                    enemy.OponentsViewing      |= combatantFlag;
                }
                else
                {
                    combatant.OpponentsInView &= ~enemyFlag;
                    enemy.OponentsViewing     &= ~combatantFlag;
                }

                if (combatantVisible && enemy.CanTakeOrders)
                {
                    enemy.OpponentsInView     |= combatantFlag;
                    combatant.OponentsViewing |= enemyFlag;
                }
                else
                {
                    enemy.OpponentsInView     &= ~combatantFlag;
                    combatant.OponentsViewing &= ~enemyFlag;
                }
            }
        }

        /// <summary>Is there an unobstructed path between two points?</summary>
        /// <param name="start">start point</param>
        /// <param name="end">end point</param>
        /// <returns>true if path exists</returns>
        public bool IsLineOfSight(Vector3 start, Vector3 end)
        {
            Vector3 dummy = new Vector3();
            return IsLineOfSight(start, end, VisibilityChecks.None, ref dummy);
        }

        /// <summary>Is there an unobstructed path between two points?</summary>
        /// <param name="start">start point</param>
        /// <param name="end">end point</param>
        /// <param name="extraChecks">extra processing to do of each cell passed through</param>
        /// <param name="lastPos">the last cell checked</param>
        /// <returns>true if path exists</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity",
            Justification="Performance need outweights complexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId = "3#",
            Justification = "We're not idiots")]
        public bool IsLineOfSight(Vector3 start, Vector3 end, VisibilityChecks extraChecks, ref Vector3 lastPos)
        {
            // make sure start and end are in centre of cells
            start          = new Vector3((int)start.X + 0.5f, (int)start.Y + 0.5f, (int)start.Z + 0.5f);
            end            = new Vector3((int)end.X + 0.5f,   (int)end.Y + 0.5f,   (int)end.Z + 0.5f);
            Vector3 delta  = end - start;
            Vector3 abs    = new Vector3(Math.Abs(delta.X),  Math.Abs(delta.Y),  Math.Abs(delta.Z));
            Vector3 sign   = new Vector3(0 <= delta.X ? 1 : -1, 0 <= delta.Y ? 1 : -1, 0 <= delta.Z ? 1 : -1);
            int     max    = (int)Math.Max(Math.Max(abs.X, abs.Y), abs.Z);
            // add small increment to step to prevent underflow in float quantization
            Vector3 step   = (delta / max) + (new Vector3(0.001f, 0.001f, 0.001f) * sign);
            Vector3 oldPos = start;
            lastPos = start;
            if ((abs.X <= abs.Y) && (abs.Z <= abs.Y))
            {
                // major component of line is going (mostly) up or down
                for (int y = 0; y < max; ++y)
                {
                    oldPos = lastPos;
                    lastPos = lastPos + step;

                    // check if floor/ceiling blocked sight
                    //... Only times we can see through a floor are (a) there isn't one, (b) it's a grav lift
                    int levelOffset = (-1 == sign.Y) ? 0 : 1;
                    GroundFlag flags = GetGroundFace((int)oldPos.X, (int)oldPos.Y + levelOffset, (int)oldPos.Z).Flags;
                    flags &= GroundFlag.FloorHeight70 | GroundFlag.DownGravLift;
                    if ((0 != flags) && (GroundFlag.DownGravLift != flags))
                    {
                        return false;
                    }

                    // if we crossed an east/west or up/down wall, check for block
                    if ((int)lastPos.Z != (int)oldPos.Z)
                    {
                        if (0 != (GetWallFace(lastPos, (1 == sign.Z) ? Side.North : Side.South).Flags & WallFlag.Opaque))
                        {
                            return false;
                        }
                    }
                    if ((int)lastPos.X != (int)oldPos.X)
                    {
                        if (0 != (GetWallFace(lastPos, (1 == sign.X) ? Side.West : Side.East).Flags & WallFlag.Opaque))
                        {
                            return false;
                        }
                    }

                    // and check new cell doesn't block view
                    if (0 != (GetGroundFace((int)lastPos.X, (int)lastPos.Y, (int)lastPos.Z).Flags & GroundFlag.ViewBlocking))
                    {
                        return false;
                    }

                    // ToDo: Perhaps, check intermediate cells we crossed between oldPos and NewPos
                    // to see if they are view blocking. This would have happened if X or Z indicate
                    // we've crossed an east/west or north/south border as well as the up/down one we
                    // had to cross.  Personally, I think this check is not needed.

                    // do any additional checks/processing on the new cell
                    if (!LineOfSightCellChecks(ref lastPos, extraChecks, (y == (max - 1))))
                    {
                        return false;
                    }
                }
            }
            else if ((abs.Y <= abs.X) && (abs.Z <= abs.X))
            {
                // major component of line is going (mostly) east or west
                for (int x = 0; x < max; ++x)
                {
                    oldPos = lastPos;
                    lastPos = lastPos + step;
                    // check if east/west wall blocked sight
                    if (0 != (GetWallFace(oldPos, (1 == sign.X) ? Side.East : Side.West).Flags & WallFlag.Opaque))
                    {
                        return false;
                    }

                    // if we crossed an east/west or up/down wall, check for block
                    if ((int)lastPos.Z != (int)oldPos.Z)
                    {
                        if (0 != (GetWallFace(lastPos, (1 == sign.Z) ? Side.North : Side.South).Flags & WallFlag.Opaque))
                        {
                            return false;
                        }
                    }
                    if ((int)lastPos.Y != (int)oldPos.Y)
                    {
                        // we only have floors, not ceilings, so adjust cell we need to examine
                        int levelOffset = (-1 == sign.Y) ? 1 : 0;
                        GroundFlag flags = GetGroundFace((int)lastPos.X, (int)lastPos.Y + levelOffset, (int)lastPos.Z).Flags;

                        // Only times we can see through a floor are (a) there isn't one, (b) it's a grav lift
                        flags &= GroundFlag.FloorHeight70 | GroundFlag.DownGravLift;
                        if ((0 != flags) && (GroundFlag.DownGravLift != flags))
                        {
                            return false;
                        }
                    }

                    // and check new cell doesn't block view
                    if (0 != (GetGroundFace((int)lastPos.X, (int)lastPos.Y, (int)lastPos.Z).Flags & GroundFlag.ViewBlocking))
                    {
                        return false;
                    }

                    // ToDo: Perhaps, check intermediate cells we crossed between oldPos and NewPos
                    // to see if they are view blocking. This would have happened if Z or Y indicate
                    // we've crossed a north/south or up/down border as well as the east/west one we
                    // had to cross.  Personally, I think this check is not needed.

                    // do any additional checks/processing on the new cell
                    if (!LineOfSightCellChecks(ref lastPos, extraChecks, (x == (max - 1))))
                    {
                        return false;
                    }
                }
            }
            else
            {
                // major component of line is going (mostly) north or south
                for (int z = 0; z < max; ++z)
                {
                    oldPos = lastPos;
                    lastPos = lastPos + step;
                    // check if north/south wall blocked sight
                    if (0 != (GetWallFace(oldPos, (1 == sign.Z) ? Side.South : Side.North).Flags & WallFlag.Opaque))
                    {
                        return false;
                    }

                    // if we crossed an east/west or up/down wall, check for block
                    if ((int)lastPos.X != (int)oldPos.X)
                    {
                        if (0 != (GetWallFace(lastPos, (1 == sign.X) ? Side.West : Side.East).Flags & WallFlag.Opaque))
                        {
                            return false;
                        }
                    }
                    if ((int)lastPos.Y != (int)oldPos.Y)
                    {
                        // we only have floors, not ceilings, so adjust cell we need to examine
                        int levelOffset = (-1 == sign.Y) ? 1 : 0;
                        GroundFlag flags = GetGroundFace((int)lastPos.X, (int)lastPos.Y + levelOffset, (int)lastPos.Z).Flags;

                        // Only times we can see through a floor are (a) there isn't one, (b) it's a grav lift
                        flags &= GroundFlag.FloorHeight70 | GroundFlag.DownGravLift;
                        if ((0 != flags) && (GroundFlag.DownGravLift != flags))
                        {
                            return false;
                        }
                    }

                    // and check new cell doesn't block view
                    if (0 != (GetGroundFace((int)lastPos.X, (int)lastPos.Y, (int)lastPos.Z).Flags & GroundFlag.ViewBlocking))
                    {
                        return false;
                    }

                    // ToDo: Perhaps, check intermediate cells we crossed between oldPos and NewPos
                    // to see if they are view blocking. This would have happened if X or Y indicate
                    // we've crossed an east/west or up/down border as well as the north/south one we
                    // had to cross.  Personally, I think this check is not needed.

                    // do any additional checks/processing on the new cell
                    if (!LineOfSightCellChecks(ref lastPos, extraChecks, (z == (max - 1))))
                    {
                        return false;
                    }
                }
            }
            // if get here, sight wasn't blocked
            return true;
        }

        /// <summary>Return CellProperties of cell at given position</summary>
        /// <param name="pos">position of cell</param>
        /// <returns>the properties</returns>
        public CellProperties CellProperty(Vector3 pos)
        {
            return GetCell(pos).Properties;
        }

        /// <summary>
        /// Put the troops in a team at their starting positions
        /// </summary>
        /// <param name="team">team to place</param>
        /// <param name="tileFlag">marker indicating cell can be used as a start position for this team</param>
        private void DeployTeam(Team team, GroundFlag tileFlag)
        {
            List<Vector3> positions = new List<Vector3>();
            // Get list of possible start positions
            for (int x = 0; x < Width; ++x)
            {
                for (int z = 0; z < Length; ++z)
                {
                    for (int y = 0; y < Levels; ++y)
                    {
                        if (0 != (GetGroundFace(x, y, z).Flags & tileFlag))
                        {
                            positions.Add(new Vector3(x, y, z));
                        }
                    }
                }
            }

            //ToDo: a sweep, placing the 2 x 2 sized combatants first

            // assign combatants to positions at random
            int i = 0;
            foreach (Combatant combatant in team.Combatants)
            {
                int index = Xenocide.Rng.Next(positions.Count);
                combatant.PlaceInTeam = i;
                combatant.Position    = positions[index] + new Vector3(0.5f, 0.0f, 0.5f);
                cells[CellIndex(positions[index])].CombatantId = combatant.CombatantId;
                positions.RemoveAt(index);
                ++i;
                if (Team.XCorp == combatant.TeamId)
                {
                    UpdateViewMatrix(combatant);
                }
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
            Justification="Will throw if facedata is null")]
        public void SetCells(int x, int y, int z, string facedata)
        {
            int i = 0;
            int j = 0;
            while (i < facedata.Length)
            {
                SetCell(x + j, y, z, facedata.Substring(i, 6));
                ++j;
                i += 7;
            }
        }

        /// <summary>Mark the cells that the combatant is currently viewing</summary>
        /// <param name="combatant">combatant who's view we're using</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic",
            Justification = "ToDo: function needs implementation")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "combatant",
            Justification = "ToDo: function needs implementation")]
        private void UpdateViewMatrix(Combatant combatant)
        {
            // ToDo: implement
        }

        /// <summary>
        /// Set a cell of the terrain to state specified by a string
        /// </summary>
        /// <param name="x">position of cell to set</param>
        /// <param name="y">position of cell to set</param>
        /// <param name="z">position of cell to set</param>
        /// <param name="facedata"></param>
        private void SetCell(int x, int y, int z, string facedata)
        {
            cells[CellIndex(x, y, z)] = new Cell(facedata);
        }

        /// <summary>
        /// Get cell at specified position
        /// </summary>
        /// <param name="pos">position of cell</param>
        /// <returns>the cell</returns>
        private Cell GetCell(Vector3 pos)
        {
            return cells[CellIndex(pos)];
        }

        /// <summary>
        /// Get cell at specified position
        /// </summary>
        /// <param name="x">cell, running west(0) to east</param>
        /// <param name="y">cell, running bottom(0) to top</param>
        /// <param name="z">cell, running north(0) to south</param>
        /// <returns>the cell</returns>
        private Cell GetCell(int x, int y, int z)
        {
            return cells[CellIndex(x, y, z)];
        }

        /// <summary>Convert X, Y and Z co-ords into index into the cells array</summary>
        /// <param name="pos">cell's position in 3D space</param>
        /// <returns>Index of element in cells having details of this position</returns>
        private int CellIndex(MoveData pos)
        {
            return CellIndex(pos.X, pos.Y, pos.Z);
        }

        /// <summary>
        /// Convert X, Y and Z co-ords into index into the cells array
        /// </summary>
        /// <param name="pos">cell's position in 3D space</param>
        /// <returns>Index of element in cells having details of this position</returns>
        public int CellIndex(Vector3 pos)
        {
            return CellIndex((int)pos.X, (int)pos.Y, (int)pos.Z);
        }

        /// <summary>
        /// Convert X, Y and Z co-ords into index into the cells array
        /// </summary>
        /// <param name="x">cell, running west(0) to east</param>
        /// <param name="y">cell, running bottom(0) to top</param>
        /// <param name="z">cell, running north(0) to south</param>
        /// <returns>Index of element in cells having details of this position</returns>
        public int CellIndex(int x, int y, int z)
        {
            Debug.Assert(IsOnTerrain(x, y, z));
            return (((y * length) + z) * width) + x;
        }

        /// <summary>
        /// Initialize the array holding the data on number of cells
        /// </summary>
        /// <param name="x">number of cells, running west(0) to east</param>
        /// <param name="y">number of cells, running bottom(0) to top</param>
        /// <param name="z">number of cells, running north(0) to south</param>
        private void InitCells(int x, int y, int z)
        {
            Debug.Assert((0 < x) && (x <= 128));
            Debug.Assert((0 < y) && (y <= 8));
            Debug.Assert((0 < z) && (z <= 128));
            width = x;
            length = z;
            levels = y;
            cells = new Cell[x * y * z];
        }

        /// <summary>
        /// Check if a position is actually on the terrain
        /// </summary>
        /// <param name="x">position, running west(0) to east</param>
        /// <param name="y">position, running bottom(0) to top</param>
        /// <param name="z">position, running north(0) to south</param>
        /// <returns>true if position is on the terrain</returns>
        public bool IsOnTerrain(int x, int y, int z)
        {
            return (0 <= x) && (x < Width) && (0 <= y) && (y < Levels) && (0 <= z) && (z < Length);
        }

        /// <summary>
        /// Check if a position is actually on the terrain
        /// </summary>
        /// <param name="v">position</param>
        /// <returns>true if position is on the terrain</returns>
        public bool IsOnTerrain(Vector3 v)
        {
            return IsOnTerrain((int)v.X, (int)v.Y, (int)v.Z);
        }

        /// <summary>Additional processing of each cell line of sight passes through</summary>
        /// <param name="cell">location of cell to check</param>
        /// <param name="extraChecks">extra processing to do of each cell passed through</param>
        /// <param name="lastCell">is this the last cell of the path?</param>
        /// <returns>true if cells does not block sight line</returns>
        private bool LineOfSightCellChecks(ref Vector3 cell, VisibilityChecks extraChecks, bool lastCell)
        {
            switch (extraChecks)
            {
                case VisibilityChecks.None:
                    return true;

                case VisibilityChecks.LineOfFire:
                    // the last cell of the ray will be occupied with the target
                    return (!cells[CellIndex(cell)].IsOccupied || lastCell);

                case VisibilityChecks.MarkVisible:
                    cells[CellIndex(cell)].Properties |= CellProperties.Seen;
                    return true;

                default:
                    Debug.Assert(false);
                    return false;
            }
        }

        #region Fields

        /// <summary>
        /// number of cells, running west(0) to east
        /// </summary>
        public int Width { get { return width; } }

        /// <summary>
        /// number of cells, running bottom(0) to top
        /// </summary>
        public int Levels { get { return levels; } }

        /// <summary>
        /// number of cells, running north(0) to south
        /// </summary>
        public int Length { get { return length; } }

        /// <summary>Access a cell making up the terrain</summary>
        /// <param name="cellIndex">position of cell</param>
        /// <returns>the cell</returns>
        public Cell this[int cellIndex]
        {
            get { return cells[cellIndex]; }
            set { cells[cellIndex] = value; }
        }

        /// <summary>Pathfinder to use for this terrain</summary>
        public Pathfinder Pathfinder
        {
            get
            {
                if (null == pathfinder)
                {
                    pathfinder = new Pathfinder(this);
                }
                return pathfinder;
            }
        }

        /// <summary>
        /// number of cells, running west(0) to east
        /// </summary>
        private int width;

        /// <summary>
        /// number of cells, running bottom(0) to top
        /// </summary>
        private int levels;

        /// <summary>
        /// number of cells, running north(0) to south
        /// </summary>
        private int length;

        /// <summary>
        /// The cells making up the terrain
        /// </summary>
        private Cell[] cells;

        /// <summary>
        /// The Ground faces indexed by Cell.Ground
        /// </summary>
        private List<GroundFace> groundFaces = new List<GroundFace>();

        /// <summary>
        /// The Wall faces indexed by Cell.North and Cell.West
        /// </summary>
        private List<WallFace> wallFaces = new List<WallFace>();

        /// <summary>Pathfinder to use for this terrain</summary>
        [NonSerialized]
        private Pathfinder pathfinder;

        #endregion Fields
    }
}
