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
* @file OutpostStatistics.cs
* @date Created: 2007/05/13
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
using ProjectXenocide.Model.Geoscape;
using ProjectXenocide.UI.Dialogs;


#endregion

namespace ProjectXenocide.Model.Geoscape.Outposts
{
    /// <summary>
    /// Represents the capabilities of an outpost
    /// </summary>
    [Serializable]
    public class OutpostStatistics
    {
        /// <summary>
        /// Record that base has a functioning radar of specified type
        /// </summary>
        /// <param name="facilityId">type of radar</param>
        public void AddRadar(String facilityId)
        {
            // add to list of radars, sorted in order of decreasing range
            RadarInfo radar = new RadarInfo(facilityId);
            int i = 0;
            while (i < radars.Count)
            {
                if (radars[i].Range < radar.Range)
                {
                    break;
                }
                ++i;
            }
            radars.Insert(i, radar);
        }

        /// <summary>
        /// Record that base no longer has a functioning radar of specified type
        /// </summary>
        /// <param name="facilityId">type of radar</param>
        public void RemoveRadar(String facilityId)
        {
            radars.RemoveAll(delegate(RadarInfo r) { return r.FacilityId == facilityId; });
        }

        /// <summary>
        /// Update radars, to decide they can detect (as opposed to track ufos that have been detected)
        /// for the next 30 minutes
        /// </summary>
        public void UpdateRadarDetection()
        {
            foreach (RadarInfo radar in radars)
            {
                radar.UpdateCanDetect();
            }
        }

        /// <summary>
        /// Is a UFO showing on the Outpost's radar?
        /// </summary>
        /// <param name="outpostPosition">Position of outpost owning the radars</param>
        /// <param name="ufoPosition">Position of UFO to check</param>
        /// <param name="alreadySeen">was UFO visible last tick?</param>
        /// <returns>true if UFO is showing</returns>
        public bool IsOnRadar(GeoPosition outpostPosition, GeoPosition ufoPosition, bool alreadySeen)
        {
            foreach (RadarInfo radar in radars)
            {
                // if we're outside range, stop at this point.
                // radars are checked in order of decreasing range
                if (!outpostPosition.IsWithin(ufoPosition, radar.Range))
                {
                    return false;
                }
                
                // otherwise, if radar is working we've seen ufo
                if (radar.CanDetect || alreadySeen)
                {
                    return true;
                }
            }

            // if get here, isn't on radar
            return false;
        }

        /// <summary>
        /// Can any of the radar units in the base decode UFO transmissions?
        /// </summary>
        /// <returns>true if at least one can</returns>
        public bool CanDecodeTransmissions()
        {
            bool decoder = false;
            foreach (RadarInfo radarInfo in radars)
            {
                decoder |= (radarInfo.Radar.CanDecodeTransmissions);
            }
            return decoder;
        }

        /// <summary>
        /// Owning outpost has been destroyed (by the aliens)
        /// </summary>
        public void OnOutpostDestroyed()
        {
            Capacities.OnOutpostDestroyed();
        }

        /// <summary>
        /// Details of a functioning Radar facility in the outpost
        /// </summary>
        [Serializable]
        private class RadarInfo
        {
            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="facilityId">The particular type of radar this is</param>
            public RadarInfo(String facilityId)
            {
                this.facilityId = facilityId;
                this.range      = (float)GeoPosition.KilometersToRadians(Radar.Range);
            }

            /// <summary>
            /// Update canDetect, to decide if it can detect UFOs (as opposed to track ufos that have been detected)
            /// for the next 30 minutes
            /// </summary>
            public void UpdateCanDetect()
            {
                canDetect = (Xenocide.Rng.RollDice(Radar.Accuracy));
            }

            #region Fields

            /// <summary>
            /// The range of the radar, in radians
            /// </summary>
            public float Range { get { return range; } }

            /// <summary>
            /// Can radar pick up a UFO during this 30 minute time slice?
            /// (used to determine accuracy)
            /// </summary>
            public bool CanDetect { get { return canDetect; } }

            /// <summary>
            /// The particular type of radar this is
            /// </summary>
            public String FacilityId { get { return facilityId; } }

            /// <summary>
            /// The range of the radar, in radians
            /// </summary>
            private float range;

            /// <summary>
            /// Can radar pick up a UFO during this 30 minute time slice?
            /// (used to determine accuracy)
            /// </summary>
            private bool canDetect = true;

            /// <summary>
            /// The particular type of radar this is
            /// </summary>
            private String facilityId;

            /// <summary>
            /// Full details on this type of facility
            /// </summary>
            public ScanFacilityInfo Radar
            {
                get { return Xenocide.StaticTables.FacilityList[facilityId] as ScanFacilityInfo; } 
            }

            /// <summary>
            /// Change value of canDetect, used for UnitTesting
            /// </summary>
            [Conditional("DEBUG")]
            public void TestSetCanDetect(bool turnOff)
            {
                canDetect = turnOff;
            }

            #endregion Fields
        }

        #region Fields
        /// <summary>
        /// The storage limits of the base
        /// </summary>
        public OutpostCapacities Capacities { get { return capacities; } }

        /// <summary>
        /// The storage limits of the base
        /// </summary>
        private OutpostCapacities capacities = new OutpostCapacities();

        /// <summary>
        /// The operational radars in the base
        /// </summary>
        private List<RadarInfo> radars = new List<RadarInfo>();

        #endregion Fields

        #region UnitTests

        /// <summary>
        /// Run set of tests
        /// </summary>
        [Conditional("DEBUG")]
        public static void RunTests()
        {
            AddRemoveRadar();
            TestUfoDetection();
        }

        /// <summary>
        /// Test adding and removing radar facilities
        /// </summary>
        [Conditional("DEBUG")]
        public static void AddRemoveRadar()
        {
            OutpostStatistics stats = new OutpostStatistics();
            stats.AddRadar("FAC_LONG_RANGE_NEUDAR");
            stats.AddRadar("FAC_TACHYON_EMISSIONS_DETECTOR");
            stats.AddRadar("FAC_SHORT_RANGE_NEUDAR");

            // check are in decreasing range order
            Debug.Assert((stats.radars[2].Range < stats.radars[1].Range) && (stats.radars[1].Range < stats.radars[0].Range));

            stats.RemoveRadar("FAC_LONG_RANGE_NEUDAR");
            stats.RemoveRadar("FAC_TACHYON_EMISSIONS_DETECTOR");
            stats.RemoveRadar("FAC_SHORT_RANGE_NEUDAR");
            Debug.Assert(0 == stats.radars.Count);
        }

        /// <summary>
        /// Test detection of UFOs
        /// </summary>
        [Conditional("DEBUG")]
        public static void TestUfoDetection()
        {
            OutpostStatistics stats = new OutpostStatistics();
            stats.AddRadar("FAC_LONG_RANGE_NEUDAR");

            // UFO outside range
            GeoPosition outpost      = new GeoPosition(0, 0);
            GeoPosition inRange      = new GeoPosition(0, (float)GeoPosition.KilometersToRadians(500));
            GeoPosition outsideRange = new GeoPosition(0, (float)GeoPosition.KilometersToRadians(5000));

            // test UFO inside and outside range
            Debug.Assert(!stats.IsOnRadar(outpost, outsideRange, true));
            Debug.Assert( stats.IsOnRadar(outpost, inRange,      true));

            // Try case where first radar isn't working
            stats.radars[0].TestSetCanDetect(false);
            Debug.Assert(!stats.IsOnRadar(outpost, inRange, false));
            Debug.Assert( stats.IsOnRadar(outpost, inRange, true));

            // Try case where first radar isn't working, but second one is
            stats.AddRadar("FAC_SHORT_RANGE_NEUDAR");
            Debug.Assert( stats.IsOnRadar(outpost, inRange, false));
        }

        #endregion UnitTests
    }
}
