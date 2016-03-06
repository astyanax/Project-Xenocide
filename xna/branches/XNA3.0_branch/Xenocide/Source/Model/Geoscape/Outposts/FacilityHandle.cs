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

using ProjectXenocide.Model.StaticData.Facilities;
using ProjectXenocide.Model.Geoscape.GeoEvents;

#endregion

namespace ProjectXenocide.Model.Geoscape.Outposts
{
    /// <summary>
    /// Represents a facility in an outpost
    /// </summary>
    [Serializable]
    public class FacilityHandle
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="facilityId">id of faciilty, for type of facility</param>
        /// <param name="x">left row of facility</param>
        /// <param name="y">top row of facility</param>
        /// <param name="prebuilt">is facility already built</param>
        public FacilityHandle(string facilityId, int x, int y, bool prebuilt)
        {
            FacilityInfo info = Xenocide.StaticTables.FacilityList[facilityId];
            Debug.Assert(null != info);
            this.facilityIndex = Xenocide.StaticTables.FacilityList.IndexOf(facilityId);
            this.x = (SByte)x;
            this.y = (SByte)y;
            if (!prebuilt && (0 < info.BuildDays))
            {
                DateTime finish  = Xenocide.GameState.GeoData.GeoTime.Time + new TimeSpan(info.BuildDays, 0, 0, 0);
                this.appointment = new FacilityConstructedAppointment(finish, this);
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="facilityId">id of faciilty, for type of facility</param>
        /// <param name="x">left row of facility</param>
        /// <param name="y">top row of facility</param>
        public FacilityHandle(string facilityId, int x, int y)
            : this(facilityId, x, y, true)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="facilityId">id of faciilty, for type of facility</param>
        public FacilityHandle(string facilityId)
            : this(facilityId, -1, -1, false)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="facilityIndex">index into FacilityList, for the type of facility</param>
        public FacilityHandle(int facilityIndex)
            : this(Xenocide.StaticTables.FacilityList[facilityIndex].Id)
        {
        }

        /// <summary>
        /// Update outpost statistics to reflect begining constuction of this facility
        /// </summary>
        /// <param name="statistics">statistics of outpost that building facility will modify</param>
        public void StartBuilding(OutpostStatistics statistics)
        {
            FacilityInfo.StartBuilding(statistics);
            if (null == appointment)
            {
                // facility is prebuilt, so just make it complete
                FacilityInfo.FinishedBuilding(statistics);
            }
            else
            {
                // let the completion appointment know what to do with it
                appointment.Statistics = statistics;
                Xenocide.GameState.GeoData.GeoTime.Add(appointment);
            }
        }

        /// <summary>
        /// Update outpost statistics to reflect finishing constuction of this facility
        /// </summary>
        /// <param name="statistics">statistics of outpost that building facility will modify</param>
        public void FinishedBuilding(OutpostStatistics statistics)
        {
            FacilityInfo.FinishedBuilding(statistics);
            GeoEvent geoevent = new FacilityFinishedGeoEvent(this);
            Xenocide.GameState.GeoData.QueueEvent(geoevent);
            appointment = null;
        }

        /// <summary>
        /// Update outpost to reflect destroying this facility
        /// </summary>
        /// <param name="statistics">statistics of outpost that destroying facility will modify</param>
        public void Destroy(OutpostStatistics statistics)
        {
            FacilityInfo.Destroy(statistics, !IsUnderConstruction);
            
            // if we've got an appointment for when base is finished, kill the appointment
            if (null != appointment)
            {
                Xenocide.GameState.GeoData.GeoTime.Scheduler.Remove(appointment);
            }
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
            get { return (null == appointment) ? 0.0 : appointment.SecondsLeft(Xenocide.GameState.GeoData.GeoTime.Time); }
        }

        /// <summary>
        /// Does this Facility acutally have a position in the base
        /// </summary>
        public bool HasPosition { get { return (0 <= x) && (0 <= y); } }

        /// <summary>
        /// Is this facility still under construction?
        /// </summary>
        public bool IsUnderConstruction { get { return (null != appointment); } }

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
        /// Event that fires when facility is scheduled to be finished.
        /// </summary>
        private FacilityConstructedAppointment appointment;

        #endregion Fields
    }
}
