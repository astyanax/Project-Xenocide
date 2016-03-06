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
* @file MoveData.cs
* @date Created: 2008/01/19
* @author File creator: David Teviotdale
* @author Credits: none
*/
#endregion

using System;
using System.Text;
using System.Diagnostics;
using System.Globalization;

using Microsoft.Xna.Framework;

namespace ProjectXenocide.Model.Battlescape
{
    /// <summary>
    /// Information on a movement that can be made on the battlescape
    /// </summary>
    public struct MoveData
    {
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="x">column co-ordinate of destination cell</param>
        /// <param name="y">level co-ordinate of destination cell</param>
        /// <param name="z">row co-ordinate of destination cell</param>
        /// <param name="cost">cost (in TU) to move to destination cell</param>
        public MoveData(int x, int y, int z, int cost)
        {
            data = 0;
            this.X    = x;
            this.Y    = y;
            this.Z    = z;
            this.Cost = cost;
        }

        /// <summary>Ctor</summary>
        /// <param name="v">vector holding co-ordinates of destination cell</param>
        public MoveData(Vector3 v) : this((int)v.X, (int)v.Y, (int)v.Z, 0) { }

        /// <summary>Return co-ordinates as a Vector3</summary>
        public Vector3 Vector3 { get { return new Vector3(X, Y, Z); } }

        /// <summary>
        /// Returns if the objects are equal
        /// </summary>
        /// <param name="obj">The object to compare</param>
        /// <returns>True if equals, false otherwise</returns>
        public override bool Equals(object obj)
        {
            return this == (MoveData)obj;
        }

        /// <summary>
        /// Equals operator
        /// </summary>
        /// <param name="left">left operator parameter</param>
        /// <param name="right">right operator parameter</param>
        /// <returns></returns>
        public static bool operator ==(MoveData left, MoveData right)
        {
            // we only check the x, y, & z elements, we ignore the cost
            return 0 == ((left.data ^ right.data) & 0x000fffff);
        }

        /// <summary>
        /// Distinct operator
        /// </summary>
        /// <param name="left">left operator parameter</param>
        /// <param name="right">right operator parameter</param>
        /// <returns></returns>
        public static bool operator !=(MoveData left, MoveData right)
        {
            return !(left == right);
        }

        /// <summary>
        /// Hashcode for the object.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            // we only check the x, y, & z elements, we ignore the cost
            return (int)(data & 0x000fffff);
        }

        /// <summary>Replace default ToString()</summary>
        /// <returns>description of MoveData</returns>
        public override string ToString()
        {
            return String.Format(CultureInfo.InvariantCulture, "x={0}, y={1}, z={2}, cost={3}", X, Y, Z, Cost);
        }

        #region Fields

        /// <summary>
        /// column co-ordinate of destination cell
        /// </summary>
        public int X
        {
            get { return (int)(data & 0xff); }
            set { Debug.Assert((0 <= value) && (value < 256)); data = (data & (UInt32)0xffffff00) | (UInt32)value; }
        }

        /// <summary>
        /// level co-ordinate of destination cell
        /// </summary>
        public int Y
        {
            get { return (int)((data >> 16) & 0xf); }
            set { Debug.Assert((0 <= value) && (value < 8)); data = (data & (UInt32)0xfff0ffff) | (UInt32)(value << 16); }
        }

        /// <summary>
        /// Row co-ordinate of destination cell
        /// </summary>
        public int Z
        {
            get { return (int)((data >> 8) & 0xff); }
            set { Debug.Assert((0 <= value) && (value < 256)); data = (data & (UInt32)0xffff00ff) | (UInt32)(value << 8); }
        }

        /// <summary>
        /// cost (in TU) to move to destination cell
        /// </summary>
        public int Cost
        {
            get { return (int)((data >> 20) & 0xfff); }
            set { Debug.Assert((0 <= value) && (value < 0xfff)); data = (data & (UInt32)0x000fffff) | (UInt32)(value << 20); }
        }

        /// <summary>the packed co-ords</summary>
        private UInt32 data;

        #endregion Fields
    }
}

