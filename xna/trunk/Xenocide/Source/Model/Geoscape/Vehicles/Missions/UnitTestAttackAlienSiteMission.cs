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
* @file UnitTestAttackAlienSiteMission.cs
* @date Created: 2007/08/26
* @author File creator: dteviot
* @author Credits: none
*/
#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

using ProjectXenocide.Model.Geoscape.AI;
using ProjectXenocide.Model.Geoscape.Geography;
using ProjectXenocide.Model.Geoscape.Outposts;

#endregion

namespace ProjectXenocide.Model.Geoscape.Vehicles
{
    /// <remarks>Unit Tests for the AttackAlienSiteMission class</remarks>
    public partial class AttackAlienSiteMission : Mission
    {
        #region UnitTests

        /// <summary>
        /// Run set of tests
        /// </summary>
        [Conditional("DEBUG")]
        public static void RunTests()
        {
            AttackAlienSiteMissionTest();
        }

        /// <summary>
        /// Exercise an Attack Alien Site Mission.
        /// </summary>
        [Conditional("DEBUG")]
        private static void AttackAlienSiteMissionTest()
        {
            Xenocide.GameState.SetToStartGameCondition();

            // create terror site
            City city = Xenocide.GameState.GeoData.Planet.AllCities[0];
            TerrorMissionAlienSite site = new TerrorMissionAlienSite(city, Race.Grey);

            // create X-Corp outpost and craft
            GeoPosition outpostPos = city.Position.RandomLocationDistantBykm(1000);
            Outpost outpost = new Outpost(outpostPos, "Base1");
            outpost.SetupPlayersFirstBase();

            // send condor & gryphons against terror site
            Craft gryphon1 = outpost.Fleet[0];
            gryphon1.Mission.Abort();
            gryphon1.Mission = new AttackAlienSiteMission(gryphon1, site);

            Craft gryphon2 = outpost.Fleet[1];
            gryphon2.Mission.Abort();
            gryphon2.Mission = new AttackAlienSiteMission(gryphon2, site);

            Craft condor = outpost.Fleet[2];
            condor.Mission.Abort();
            condor.Mission = new AttackAlienSiteMission(condor, site);

            Debug.Assert(gryphon1.Position.Equals(outpost.Position) &&
                (gryphon1.Mission.State.GetType().Name == "MoveToPositionState"));
            Debug.Assert(gryphon2.Position.Equals(outpost.Position) &&
                (gryphon2.Mission.State.GetType().Name == "MoveToPositionState"));
            Debug.Assert(condor.Position.Equals(outpost.Position) &&
                (condor.Mission.State.GetType().Name == "MoveToPositionState"));

            Debug.Assert(site.Inbound.Count == 3);

            // aborting mission removes craft from inbound
            gryphon2.Mission.Abort();
            Debug.Assert(site.Inbound.Count == 2);

            double twelvehours = 12 * 3600 * 1000.0;

            // craft should proceed to terror site
            condor.Update(twelvehours);
            Debug.Assert(condor.Position.Equals(city.Position) &&
                (condor.Mission.State.GetType().Name == "ReturnToBaseState"));

            // ufo should go home
            condor.Update(twelvehours);
            Debug.Assert(condor.Position.Equals(outpost.Position) &&
                (condor.Mission.State.GetType().Name == "InBaseState"));

            // destroying terror site should send the gryphon heading home
            site.OnSiteDestroyed();
            Debug.Assert(gryphon1.Mission.State.GetType().Name == "ReturnToBaseState");
        }

        #endregion UnitTests
    }
}
