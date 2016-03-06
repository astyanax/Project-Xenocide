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
* @file UnitTestUfo.cs
* @date Created: 2007/12/26
* @author File creator: David Teviotdale
* @author Credits: none
*/
#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;
using System.Diagnostics;


using ProjectXenocide.Utils;

using ProjectXenocide.Model.StaticData;
using ProjectXenocide.Model.Geoscape.AI;
using ProjectXenocide.Model.Geoscape.Outposts;
using ProjectXenocide.Model.Geoscape.GeoEvents;
using ProjectXenocide.Model.StaticData.Items;
using ProjectXenocide.Model.StaticData.Facilities;

#endregion

namespace ProjectXenocide.Model.Geoscape.Vehicles
{
    /// <summary>
    /// A craft owned by the alien
    /// </summary>
    public partial class Ufo : Craft
    {
        #region UnitTests

        /// <summary>
        /// Run set of tests
        /// </summary>
        [Conditional("DEBUG")]
        public static void RunTests()
        {
            VisibilityFlyByTest();
            ResearchMissionTest();
        }

        /// <summary>
        /// Get UFO to fly past an outpost's (and a craft's) radar area
        /// </summary>
        [Conditional("DEBUG")]
        private static void VisibilityFlyByTest()
        {
            // build base
            Outpost outpost = OutpostInventory.ConstructTestOutpost();
            FacilityHandle radar = new FacilityHandle("FAC_TACHYON_EMISSIONS_DETECTOR", 2, 2);
            outpost.Floorplan.AddFacility(radar);
            Xenocide.GameState.SetToStartGameCondition();
            Xenocide.GameState.GeoData.Outposts.Add(outpost);

            // do base flyby
            GeoPosition origin = new GeoPosition();
            GeoPosition start  = new GeoPosition((float)Math.PI * 0.4f,  0);
            GeoPosition end    = new GeoPosition((float)Math.PI * -0.4f, 0);
            Ufo ufo = new Ufo("ITEM_UFO_RECON", start, null);
            ufo.Mission = new ResearchMission(ufo, end, 1);

            float radarRange = (float)GeoPosition.KilometersToRadians((radar.FacilityInfo as ScanFacilityInfo).Range);
            while (!end.IsWithin(ufo.Position, (float)GeoPosition.KilometersToRadians(20)))
            {
                ufo.Update(60000);
                Debug.Assert(ufo.IsKnownToXCorp == origin.IsWithin(ufo.Position, radarRange));
            }

            // do craft flyby
            //... create craft
            GeoPosition craftPos = new GeoPosition((float)Math.PI, 0);
            Aircraft aircraft = (Aircraft)Xenocide.StaticTables.ItemList["ITEM_XC-1_GRYPHON"].Manufacture();
            outpost.Inventory.Add(aircraft, false);
            aircraft.Mission.Abort();
            aircraft.Mission = new InterceptMission(aircraft, ufo);
            aircraft.Position = craftPos;

            Debug.Assert(1 == ufo.Hunters.Count);
            bool wasInRange = false;

            //.. do flyby
            end = new GeoPosition((float)Math.PI * -0.6f, 0);
            ufo.Position = new GeoPosition((float)Math.PI * 0.6f, 0);
            ufo.Mission.Abort();
            ufo.Mission = new ResearchMission(ufo, end, 1);
            radarRange = (float)GeoPosition.KnotsToRadians(700);
            while (!end.IsWithin(ufo.Position, (float)GeoPosition.KilometersToRadians(20)))
            {
                ufo.Update(60000);
                Debug.Assert(ufo.IsKnownToXCorp == craftPos.IsWithin(ufo.Position, radarRange));

                // check that after ufo moves out of craft's range, craft looses tracking
                if (wasInRange && !ufo.IsKnownToXCorp)
                {
                    Debug.Assert(0 == ufo.Hunters.Count);
                }
                wasInRange = ufo.IsKnownToXCorp;
            }
        }

        /// <summary>
        /// Exercise a Research Mission.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        [Conditional("DEBUG")]
        private static void ResearchMissionTest()
        {
            GeoPosition start = new GeoPosition();
            GeoPosition end = new GeoPosition((float)Math.PI * -0.4f, 0);
            Ufo ufo = new Ufo("ITEM_UFO_RECON", start, null);
            ufo.Mission = new ResearchMission(ufo, end, 3);

            double twelvehours = 12 * 3600 * 1000.0;

            // UFO should proceed to first site and land
            ufo.Update(twelvehours);
            Debug.Assert(ufo.Position.Equals(end) && 
                (ufo.Mission.State.GetType().Name == "WaitState"));

            // ufo should take off
            ufo.Update(twelvehours);
            Debug.Assert(ufo.Position.Equals(end) && 
                (ufo.Mission.State.GetType().Name == "MoveToPositionState"));

            // should move to second landing site
            ufo.Update(twelvehours);
            GeoPosition pos2 = ufo.Position;
            Debug.Assert((0.01 < ufo.Position.Distance(end)) &&
                (ufo.Mission.State.GetType().Name == "WaitState"));

            // ufo should take off
            ufo.Update(twelvehours);
            Debug.Assert(ufo.Position.Equals(pos2) &&
                (ufo.Mission.State.GetType().Name == "MoveToPositionState"));

            // ufo move to final landing site
            ufo.Update(twelvehours);
            GeoPosition pos3 = ufo.Position;
            Debug.Assert((0.01 < ufo.Position.Distance(pos2)) &&
                (ufo.Mission.State.GetType().Name == "WaitState"));

            // ufo should take off
            ufo.Update(twelvehours);
            Debug.Assert(ufo.Position.Equals(pos3) &&
                (ufo.Mission.State.GetType().Name == "MoveToPositionState"));

            // ufo should depart
            // Note, it throws because the UFO has no InvasionTask.
            // and trying to stub up an InvasionTask is hard
            try
            {
                ufo.Update(twelvehours);
            }
            catch
            {
            }
        }

        #endregion UnitTests
    }
}
