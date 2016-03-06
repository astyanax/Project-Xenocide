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
* @file UnitTestRetaliationTask.cs
* @date Created: 2007/09/01
* @author File creator: dteviot
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
using ProjectXenocide.Model.Geoscape.Outposts;

#endregion

namespace ProjectXenocide.Model.Geoscape.AI
{
    /// <remarks>Unit Tests for the RetaliationTask class</remarks>
    public partial class RetaliationTask : InvasionTask
    {
        #region UnitTests

        /// <summary>
        /// Run set of tests
        /// </summary>
        [Conditional("DEBUG")]
        public static void RunTests()
        {
            RetaliationTaskTest();
        }

        /// <summary>
        /// Exercise a RetaliationTask
        /// </summary>
        /// <remarks>This will fail, because most UFOs can destory the outpost now.</remarks>
        [Conditional("DEBUG")]
        private static void RetaliationTaskTest()
        {
            Xenocide.GameState.SetToStartGameCondition();
            GeoPosition     position = new GeoPosition();
            Outpost         outpost  = null;
            Overmind        overmind = Xenocide.GameState.GeoData.Overmind;
            RetaliationTask task     = overmind.TaskFactory.CreateRetaliationTask(overmind, position);

            // no outpost case
            task.SelectOutpost();
            Debug.Assert(null == task.outpost);

            overmind.DiableStartOfMonth();
            overmind.AddTask(task);
            Debug.Assert(1 == overmind.Tasks.Count);
            // lauch the UFOs
            double twelvehours = 12 * 3600 * 1000.0;
            for (int i = 0; i < 8; ++i)
            {
                // ensure final mission will locate outpost
                if (7 == i)
                {
                    // values are start position, race for UFO & detection
                    int[] random = { 10, 10, 1 };
                    Xenocide.Rng.RigDice(random);
                }
                else
                {
                    int[] random = { 10, 10, 50 };
                    Xenocide.Rng.RigDice(random);
                }

                TestReleaseUfo(task);
                Ufo                ufo     = overmind.Ufos[0];
                RetaliationMission mission = ufo.Mission as RetaliationMission;

                // if first mission, should be research, because there's no outpost
                if (0 == i)
                {
                    Debug.Assert(null == mission);

                    // add outpost for next time
                    outpost = new Outpost(position, "dummy");
                    Xenocide.GameState.GeoData.Outposts.Add(outpost);
                }
                else
                {
                    Debug.Assert(null != task.outpost);
                }

                // if outpost is located, must be a RetaliationMission
                Debug.Assert(!task.locatedOutpost || (null != mission));

                // figure out number of times we need to pump UFO
                int numMissionStages = 13;
                if (null != mission)
                {
                    numMissionStages = task.locatedOutpost ? 2 : 12;
                }

                // Pump the UFO
                for (int j = 0; j < numMissionStages; ++j)
                {
                    // for retaliation mission, UFO finds outpost as last stage
                    if (null != mission)
                    {
                        if ((numMissionStages - 2) == j)
                        {
                            Debug.Assert(false == mission.LocatedOutpost);
                        }
                        else if ((numMissionStages - 1) == j)
                        {
                            Debug.Assert(true == mission.LocatedOutpost);
                        }
                    }
                    // if final mission, assume it destroyed the base
                    if ((7 == i) && ((numMissionStages - 1) == j))
                    {
                        Xenocide.GameState.GeoData.Outposts.Remove(outpost);
                    }

                    ufo.Update(twelvehours);

                    // if any retailation mission but last, task knows outpost's location
                    // last mission kills outpost, so will not know location
                    if ((null != mission) && ((numMissionStages - 1) == j))
                    {
                        Debug.Assert(task.locatedOutpost == (i < 7));
                    }
                }

            }
            // task should have ended
            Debug.Assert(0 == overmind.Tasks.Count);
        }

        #endregion UnitTests
    }
}
