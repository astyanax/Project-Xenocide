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
* @file FacilityHandle.cs
* @date Created: 2007/04/29
* @author File creator: dteviot
* @author Credits: none
*/
#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

using Microsoft.Xna.Framework;

using Xenocide.Model.StaticData.Facilities;

#endregion

namespace Xenocide.Model.Geoscape.HumanBases
{
    /// <summary>
    /// Represents a facility in a human base
    /// </summary>
    [Serializable]
    public class FacilityHandle
    {
        /// <summary>
        /// Constructor 
        /// </summary>
        /// <param name="facilityIndex">index into FacilityList, for the type of facility</param>
        /// <param name="x">left row of facility</param>
        /// <param name="y">top row of facility</param>
        public FacilityHandle(int facilityIndex, int x, int y)
        {
            FacilityInfo info = Xenocide.StaticTables.FacilityList[facilityIndex];
            Debug.Assert(null != info);
            this.facilityIndex = facilityIndex;
            this.x = (SByte)x;
            this.y = (SByte)y;
            this.TimeToBuild = info.BuildDays * 86400.0;
        }

        /// <summary>
        /// Constructor 
        /// </summary>
        /// <param name="facilityId">id of faciilty, for type of facility</param>
        /// <param name="x">left row of facility</param>
        /// <param name="y">top row of facility</param>
        public FacilityHandle(string facilityId, int x, int y)
            : this(Xenocide.StaticTables.FacilityList.IndexOf(facilityId), x, y)
        {
        }

        /// <summary>
        /// Constructor 
        /// </summary>
        /// <param name="facilityIndex">index into FacilityList, for the type of facility</param>
        public FacilityHandle(int facilityIndex)
            : this(facilityIndex, -1, -1)
        {
        }

        /// <summary>
        /// Constructor 
        /// </summary>
        /// <param name="facilityId">id of faciilty, for type of facility</param>
        public FacilityHandle(string facilityId)
            : this(facilityId, -1, -1)
        {
        }

        #region Fields

        /// <summary>
        /// The type of facility
        /// </summary>
        public FacilityInfo FacilityInfo { get { return Xenocide.StaticTables.FacilityList[facilityIndex]; } }

        /// <summary>
        /// X co-ordinate of top right cell of facility in base
        /// </summary>
        public int X 
        {
            get { return x; }
            set { x = (SByte)value; }
        }

        /// <summary>
        /// Y co-ordinate of top right cell of facility in base
        /// </summary>
        public int Y
        {
            get { return y; }
            set { y = (SByte)value; }
        }

        /// <summary>
        /// Seconds left before facility is built
        /// </summary>
        public double TimeToBuild
        {
            get { return timeToBuild; }
            set { timeToBuild = value; }
        }

        /// <summary>
        /// Does this Facility acutally have a position in the base
        /// </summary>
        public bool HasPosition { get { return (0 <= x) && (0 <= y); } }

        /// <summary>
        /// Is this facility still under construction?
        /// </summary>
        public bool IsUnderConstruction { get { return 0.0 < timeToBuild; } }

        /// <summary>
        /// index into FacilityList, for the type of facility
        /// </summary>
        private int facilityIndex;

        /// <summary>
        /// X co-ordinate of top right cell of facility in base
        /// </summary>
        private SByte x;

        /// <summary>
        /// Y co-ordinate of top right cell of facility in base
        /// </summary>
        private SByte y;

        /// <summary>
        /// Seconds left before facility is built
        /// </summary>
        private double timeToBuild;

        #endregion Fields
    }
}
