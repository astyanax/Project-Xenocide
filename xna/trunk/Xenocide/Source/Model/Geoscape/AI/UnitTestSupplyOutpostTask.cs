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
* @file UnitTestSupplyOutpostTask.cs
* @date Created: 2007/08/27
* * @author File creator: David Teviotdale
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
    /// <remarks>Unit Tests for the SupplyOutpostTask class</remarks>
    public partial class SupplyOutpostTask : InvasionTask
    {
        #region UnitTests

        /// <summary>
        /// Run set of tests
        /// </summary>
        [Conditional("DEBUG")]
        public static void RunTests()
        {
            SupplyOutpostTaskTest();
        }

        /// <summary>
        /// Exercise a SupplyOutpostTask
        /// </summary>
        [Conditional("DEBUG")]
        private static void SupplyOutpostTaskTest()
        {
            Xenocide.GameState.SetToStartGameCondition();
            Overmind         overmind = Xenocide.GameState.GeoData.Overmind;
            GeoPosition      position = new GeoPosition();

            OutpostAlienSite  outpost     = new OutpostAlienSite(position, Race.Grey);
            SupplyOutpostTask task        = overmind.Tasks[0] as SupplyOutpostTask;
            double            twelvehours = 12 * 3600 * 1000.0;

            overmind.DiableStartOfMonth();
            overmind.AddSite(outpost);
            Debug.Assert(0 <= overmind.Sites.IndexOf(outpost));
            Debug.Assert(0 <= overmind.Tasks.IndexOf(task));

            // lauch the UFOs
            for (int i = 0; i < 5; ++i)
            {
                TestReleaseUfo(task);
                // Pump the UFO
                Ufo ufo0 = task.Ufos[0];

                // UFO should be freighter in all cases
                Debug.Assert(ufo0.ItemInfo.Id == "ITEM_UFO_ALIEN_FREIGHTER");

                // fly to outpost
                ufo0.Update(twelvehours);

                // unload
                Debug.Assert(!ufo0.Mission.Success);
                task.TestWaitForUfoToSucceed(ufo0);

                // depart
                task.TestWaitForUfoToDepart(ufo0);
            }

            // kill UFO before it unloads, get bigger UFO.
            TestReleaseUfo(task);
            Ufo ufo = task.Ufos[0];
            ufo.OnDestroyed();
            TestReleaseUfo(task);
            ufo = task.Ufos[0];
            Debug.Assert(ufo.ItemInfo.Id == "ITEM_UFO_INTIMIDATOR");

            // kill bigger UFO after unload, should still reset
            task.TestWaitForUfoToSucceed(ufo);
            ufo.OnDestroyed();
            TestReleaseUfo(task);
            ufo = task.Ufos[0];
            Debug.Assert(ufo.ItemInfo.Id == "ITEM_UFO_ALIEN_FREIGHTER");

            // BUG: if kill two UFOs, dies when 3rd finishes mission
            ufo.OnDestroyed();
            TestReleaseUfo(task);
            ufo = task.Ufos[0];
            ufo.OnDestroyed();
            TestReleaseUfo(task);
            ufo = task.Ufos[0];
            Debug.Assert(ufo.ItemInfo.Id == "ITEM_UFO_JUGGERNAUT");
            task.TestWaitForUfoToDepart(ufo);
            Debug.Assert(0 <= overmind.Sites.IndexOf(outpost));
            Debug.Assert(0 <= overmind.Tasks.IndexOf(task));
            // BUG: locks up at this point, because there's no appointment for the next launch
            TestReleaseUfo(task);
            ufo = task.Ufos[0];

            // kill 3 UFOs, outpost should die
            ufo.OnDestroyed();
            TestReleaseUfo(task);
            ufo = task.Ufos[0];
            ufo.OnDestroyed();
            TestReleaseUfo(task);
            ufo = task.Ufos[0];
            ufo.OnDestroyed();

            Debug.Assert(-1 == overmind.Sites.IndexOf(outpost));
            Debug.Assert(-1 == overmind.Tasks.IndexOf(task));
        }

        #endregion UnitTests
    }
}
