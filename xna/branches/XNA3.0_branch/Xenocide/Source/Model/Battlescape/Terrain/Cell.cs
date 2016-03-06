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
* @file Cell.cs
* @date Created: 2008/02/18
* @author File creator: David Teviotdale
* @author Credits: none
*/
#endregion

using System;
using System.Text;
using System.Globalization;

namespace ProjectXenocide.Model.Battlescape
{
    /// <summary>
    /// The sides (faces) of a terrain cell
    /// </summary>
    public enum Side
    {
        /// <summary>The north wall, facing south</summary>
        North,

        /// <summary></summary>
        East,

        /// <summary></summary>
        West,

        /// <summary>The south wall, facing north</summary>
        South,

        /// <summary>Ground face, normal is upwards</summary>
        Bottom,

        /// <summary>Ceiling face</summary>
        Top,
    }

    /// <summary>
    /// Set of bit flags, describing the properties of terrain cell's ground face
    /// </summary>
    [Flags]
    public enum GroundFlag
    {
        /// <summary>empty tile</summary>
        None = 0,

        /// <summary>Cell's floor is at height 0 for this level</summary>
        FloorHeight0 = 0x1,

        /// <summary>Cell's floor is at height 0.3 units for this level</summary>
        FloorHeight30 = 0x2,

        /// <summary>Cell's floor is at height 0.7 units for this level</summary>
        FloorHeight70 = 0x3,

        /// <summary>Combatants can't enter this cell</summary>
        Blocked = 0x4,

        /// <summary>Combatants can't see through this cell</summary>
        ViewBlocking = 0x8,

        /// <summary>non-flying combatants can use this to go up a level</summary>
        UpGravLift = 0x10,

        /// <summary>non-flying combatants can use this to go down a level</summary>
        DownGravLift = 0x20,

        /// <summary>Is this cell a start position for Aliens?</summary>
        AlienStart = 0x40,

        /// <summary>Is this cell a start and end position for X-Corp? combatants</summary>
        XCorpStart = 0x80,
    }

    /// <summary>
    /// Details of the ground face of a cell
    /// </summary>
    [Serializable]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes")]
    public struct GroundFace
    {
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="flags">Properties of this face</param>
        /// <param name="textureId">(Index of) texture to draw on face</param>
        public GroundFace(GroundFlag flags, int textureId)
        {
            this.flags = flags;
            this.textureId = textureId;
        }

        /// <summary>
        /// Check if a combatant can move horizontally off a cell with this GroundFace
        /// </summary>
        /// <returns>true if can move</returns>
        public bool CanMoveOff(bool canFly)
        {
            // if cell is blocked, we can't move
            bool notBlocked = (0 == (flags & GroundFlag.Blocked));

            // we can leave cell if it has a floor, or we can fly
            return notBlocked && (canFly || (0 != (flags & GroundFlag.FloorHeight70)));
        }

        /// <summary>Is this a tile that lets an X-Corp soldier exit the battlescape?</summary>
        public bool IsExitTile { get { return 0 != (flags & GroundFlag.XCorpStart); } }

        #region Fields

        /// <summary>
        /// Get how far above the level's "ground" the cell's floor is
        /// </summary>
        public float Height
        {
            get
            {
                switch (flags & GroundFlag.FloorHeight70)
                {
                    case GroundFlag.FloorHeight30:
                        return 0.33f;

                    case GroundFlag.FloorHeight70:
                        return 0.67f;
                }
                return 0.0f;
            }
        }

        /// <summary>Properties of this face</summary>
        public GroundFlag Flags { get { return flags; } set { flags = value; } }

        /// <summary>(Index of) texture to draw on face</summary>
        public int TextureId { get { return textureId; } set { textureId = value; } }

        /// <summary>Properties of this face</summary>
        private GroundFlag flags;

        /// <summary>(Index of) texture to draw on face</summary>
        private int textureId;

        #endregion Fields
    }

    /// <summary>
    /// Set of bit flags, describing the properties of terrain cell's wall face
    /// </summary>
    [Flags]
    public enum WallFlag
    {
        /// <summary>Is combatant's line of sight blocked by the face?</summary>
        None = 0,

        /// <summary>Is combatant's line of sight blocked by the face?</summary>
        Opaque = 0x1,

        /// <summary>Does cell have a wall blocking travel to the west?</summary>
        Solid = 0x2,
    }

    /// <summary>
    /// Details of one of the wall faces of a cell
    /// </summary>
    [Serializable]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes")]
    public struct WallFace
    {
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="flags">Properties of this face</param>
        /// <param name="textureId">(Index of) texture to draw on face</param>
        public WallFace(WallFlag flags, int textureId)
        {
            this.flags = flags;
            this.textureId = textureId;
        }

        #region Fields

        /// <summary>Properties of this face</summary>
        public WallFlag Flags { get { return flags; } set { flags = value; } }

        /// <summary>(Index of) texture to draw on face</summary>
        public int TextureId { get { return textureId; } set { textureId = value; } }

        /// <summary>Properties of this face</summary>
        private WallFlag flags;

        /// <summary>(Index of) texture to draw on face</summary>
        private int textureId;

        #endregion Fields
    }

    /// <summary>Assorted attribues of cell</summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1028:EnumStorageShouldBeInt32")]
    [Flags]
    public enum CellProperties : byte
    {
        /// <summary>Cell has no special properties</summary>
        None = 0,

        /// <summary>Cell has (at some stage) been viewed by an X-Corp soldier</summary>
        Seen = 1,

        /// <summary>Cell contains smoke</summary>
        Smoke = 2,

        /// <summary>A Proximity mine is "watching" this cell</summary>
        ProximityMine = 4,

        // we don't have XCorp or Alien watching flags,
        // those are bitfields to indicate soldier doing the observing
    }

    /// <summary>
    /// A terrian element
    /// </summary>
    [Serializable]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes",
        Justification="Will never test for equality")]
    public struct Cell
    {
        /// <summary>Ctor</summary>
        /// <param name="ground">Index to cell's ground face</param>
        /// <param name="north">Index to cell's north face</param>
        /// <param name="west">Index to cell's west face</param>
        public Cell(byte ground, byte north, byte west)
        {
            this.ground      = ground;
            this.north       = north;
            this.west        = west;
            this.combatantId = 0;

            // ToDo: when implement visibility, default flags are 'None'.
            this.properties  = CellProperties.Seen;
        }

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="celldata">the byte values encoded in 2 digit hex values</param>
        public Cell(string celldata)
            :
            this(ToHex(celldata, 0), ToHex(celldata, 2), ToHex(celldata, 4))
        {
        }

        /// <summary>
        /// Takes 2 charaters from a string, treats them as hex and converts into a byte value
        /// </summary>
        /// <param name="s">string to extract chars from</param>
        /// <param name="offset">to first character in string to convert</param>
        /// <returns>byte value</returns>
        private static byte ToHex(string s, int offset)
        {
            return byte.Parse(s.Substring(offset, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        }

        #region Fields

        /// <summary>Index to cell's ground face</summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode",
            Justification = "ToDo: Code still under development.  Remove this attribute when finshed")]
        public byte Ground { get { return ground; } set { ground = value; } }

        /// <summary>Index to cell's north face</summary>
        public byte North { get { return north; } set { north = value; } }

        /// <summary>Index to cell's west face</summary>
        public byte West { get { return west; } set { west = value; } }

        /// <summary>Combatant that is occupying this cell, or 0 if there is no combatant</summary>
        public byte CombatantId { get { return combatantId; } set { combatantId = value; } }

        /// <summary>Is there a combatant in this cell?</summary>
        public bool IsOccupied { get { return (0 != combatantId); } }

        /// <summary>Assorted attribues of cell</summary>
        public CellProperties Properties { get { return properties; } set { properties = value; } }

        /// <summary>Index to cell's ground face</summary>
        private byte ground;

        /// <summary>Index to cell's north face</summary>
        private byte north;

        /// <summary>Index to cell's west face</summary>
        private byte west;

        /// <summary>Combatant that is occupying this cell, or 0 if there is no combatant</summary>
        private byte combatantId;

        /// <summary>Assorted attribues of cell</summary>
        private CellProperties properties;

        #endregion Fields
    }
}
