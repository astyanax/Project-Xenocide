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
* @file UnitTestInfiltrationTask.cs
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
    public partial class InfiltrationTask : InvasionTask
    {
        #region UnitTests

        /// <summary>
        /// Run set of tests
        /// </summary>
        [Conditional("DEBUG")]
        public static void RunTests()
        {
            InfiltrationTaskTest();
        }

        /// <summary>
        /// Exercise an InfiltrationTask
        /// </summary>
        [Conditional("DEBUG")]
        private static void InfiltrationTaskTest()
        {
            Xenocide.GameState.SetToStartGameCondition();
            Overmind         overmind = Xenocide.GameState.GeoData.Overmind;
            PlanetRegion     region   = Xenocide.GameState.GeoData.Planet.AllRegions[0];
            InfiltrationTask task     = overmind.TaskFactory.Create(AlienMission.Infiltration, overmind, region) as InfiltrationTask;

            overmind.DiableStartOfMonth();
            overmind.AddTask(task);
            Debug.Assert(1 == overmind.Tasks.Count);
            // lauch the UFOs
            double twelvehours = 12 * 3600 * 1000.0;
            for (int i = 0; i < 9; ++i)
            {
                task.LaunchUfo();

                // Pump the UFO
                Ufo ufo = overmind.Ufos[0];
                for (int j = 0; j < 6; ++j)
                {
                    ufo.Update(twelvehours);
                }

                // first UFO is destroyed before it finishes it's mission
                if (0 == i)
                {
                    ufo.OnDestroyed();
                }
                else
                {
                    // this should now complete the UFO's mission
                    task.TestWaitForUfoToDepart(ufo);
                }
            }
            // task should now be over
            Debug.Assert(0 == overmind.Tasks.Count);
        }

        #endregion UnitTests
    }
}
