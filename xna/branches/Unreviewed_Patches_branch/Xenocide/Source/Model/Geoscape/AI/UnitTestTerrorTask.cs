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
* @file UnitTestTerrorTask.cs
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
    /// <remarks>Unit Tests for the TerrorTask class</remarks>
    public partial class TerrorTask : InvasionTask
    {
        #region UnitTests

        /// <summary>
        /// Run set of tests
        /// </summary>
        [Conditional("DEBUG")]
        public static void RunTests()
        {
            TerrorTaskTest();
        }

        /// <summary>
        /// Exercise a TerrorTask.
        /// </summary>
        [Conditional("DEBUG")]
        private static void TerrorTaskTest()
        {
            Xenocide.GameState.SetToStartGameCondition();
            Overmind         overmind = Xenocide.GameState.GeoData.Overmind;
            InvasionTask     task     = overmind.TaskFactory.CreateTerrorTask(overmind);

            overmind.DiableStartOfMonth();
            overmind.AddTask(task);
            Debug.Assert(1 == overmind.Tasks.Count);
            // lauch the UFOs
            double twelvehours = 12 * 3600 * 1000.0;
            for (int i = 0; i < 5; ++i)
            {
                TestReleaseUfo(task);

                // Pump the UFO
                Ufo ufo = overmind.Ufos[0];
                for (int j = 0; j < 10; ++j)
                {
                    ufo.Update(twelvehours);
                }

                // pump game time. if this is last mission in set, should have a terror site
                Debug.Assert(0 == overmind.Sites.Count);
                Xenocide.GameState.GeoData.GeoTime.AddMilliseconds(1L * 3600 * 1000);
                int terrorSites = (3 == i) ? 1 : 0;
                Debug.Assert(terrorSites == overmind.Sites.Count);

                // should clear after 12 hours.
                Xenocide.GameState.GeoData.GeoTime.AddMilliseconds(12L * 3600 * 1000);
                Debug.Assert(0 == overmind.Sites.Count);
            }
            // task should still be running
            Debug.Assert(1 == overmind.Tasks.Count);
        }

        #endregion UnitTests
    }
}
