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
* @file UnitTestInvasionTask.cs
* @date Created: 2007/08/28
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
    /// <remarks>Unit Tests for the InvasionTask classes</remarks>
    abstract public partial class InvasionTask
    {
        #region UnitTests

        /// <summary>
        /// Pump time until UFO releases
        /// </summary>
        /// <param name="task">Task that should own the ufo</param>
        public static void TestReleaseUfo(InvasionTask task)
        {
            // bump time by a day, until UFO launches
            while (0 == task.Ufos.Count)
            {
                Xenocide.GameState.GeoData.GeoTime.AddMilliseconds(24L * 3600 * 1000);
            }
        }

        public void TestWaitForUfoToDepart(Ufo ufo)
        {
            double onehour = 1000 * 60 * 60;
            for (int i = 0; i < 31 * 24; ++i)
            {
                ufo.Update(onehour);
                if (!Ufos.Contains(ufo))
                {
                    return;
                }
            }
            // if get here, UFO did not depart within a month.
            Debug.Assert(false);
        }

        public void TestWaitForUfoToSucceed(Ufo ufo)
        {
            double onehour = 1000 * 60 * 60;
            for (int i = 0; i < 31 * 24; ++i)
            {
                ufo.Update(onehour);
                if (ufo.Mission.Success)
                {
                    return;
                }
            }
            // if get here, mission wasn't a success
            Debug.Assert(false);
        }

        #endregion UnitTests
    }
}
