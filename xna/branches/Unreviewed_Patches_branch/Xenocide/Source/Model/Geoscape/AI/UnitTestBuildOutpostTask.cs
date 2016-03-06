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
* @file UnitTestBuildOutpostTask.cs
* @date Created: 2007/08/27
* * @author File creator: dteviot
* @author Credits: none
*/
#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

using ProjectXenocide.Model.Geoscape;
using ProjectXenocide.Model.Geoscape.Vehicles;
using ProjectXenocide.Model.Geoscape.Geography;

#endregion

namespace ProjectXenocide.Model.Geoscape.AI
{
    /// <remarks>Unit Tests for the InfiltrationTask class</remarks>
    public partial class BuildOutpostTask : InvasionTask
    {
        #region UnitTests

        /// <summary>
        /// Run set of tests
        /// </summary>
        [Conditional("DEBUG")]
        public static void RunTests()
        {
            BuildOutpostTaskTest();
        }

        /// <summary>
        /// Exercise an BuildOutpostTask
        /// </summary>
        [Conditional("DEBUG")]
        private static void BuildOutpostTaskTest()
        {
            Xenocide.GameState.SetToStartGameCondition();
            Overmind         overmind = Xenocide.GameState.GeoData.Overmind;
            PlanetRegion     region   = Xenocide.GameState.GeoData.Planet.AllRegions[0];
            BuildOutpostTask task     = overmind.TaskFactory.Create(AlienMission.Outpost, overmind, region) as BuildOutpostTask;

            overmind.DiableStartOfMonth();
            overmind.AddTask(task);
            Debug.Assert(1 == overmind.Tasks.Count);
            // lauch the UFOs
            double twelvehours = 12 * 3600 * 1000.0;

            // launch the UFOs, except for one that builds the base
            for (int i = 0; i < 5; ++i)
            {
                TestReleaseUfo(task);

                // Pump the UFO
                Ufo ufo0 = overmind.Ufos[0];
                int stepsInMission = (i < 3) ? 13 : 3;
                for (int j = 0; j < stepsInMission; ++j)
                {
                    ufo0.Update(twelvehours);
                }
            }

            // now the final UFO
            TestReleaseUfo(task);
            Ufo ufo = task.Ufos[0];
            Debug.Assert(ufo.ItemInfo.Id == "ITEM_UFO_JUGGERNAUT");

            ufo.Update(twelvehours);
            ufo.Update(twelvehours);

            // Confirm no outpost at this point in time
            Debug.Assert(0 == overmind.Sites.Count);

            // kill UFO after unload, but before finish last stage. Outpost should still be built
            ufo.OnDestroyed();
            Debug.Assert(1 == overmind.Sites.Count);

            // Build task should now be over, but should now be a supply task
            Debug.Assert(1 == overmind.Tasks.Count);
            Debug.Assert(overmind.Tasks[0].GetType().Name == "SupplyOutpostTask");
        }

        #endregion UnitTests
    }
}
