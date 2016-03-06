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
* @file UnitTestCrewBuilder.cs
* @date Created: 2008/02/05
* @author File creator: David Teviotdale
* @author Credits: none
*/
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Globalization;

using Microsoft.Xna.Framework;

namespace ProjectXenocide.Model.Battlescape
{
    /// <summary>
    /// Code to test the CrewBuilder class.
    /// </summary>
    public partial class CrewBuilder
    {
        #region UnitTests

        /// <summary>
        /// Run set of tests
        /// </summary>
        [Conditional("DEBUG")]
        public static void RunTests()
        {
            TestStaffOutpost();
        }

        /// <summary>
        /// Test StaffOutpost()
        /// </summary>
        [Conditional("DEBUG")]
        public static void TestStaffOutpost()
        {
            // build an easy staff
            int[] random = { 4 };
            Xenocide.Rng.RigDice(random);
            Team team = StaffOutpost().CreateCrew(Race.Grey, Difficulty.Easy, 100);
            Debug.Assert((16 <= team.Combatants.Count) && (team.Combatants.Count <= 18));


            // build a hard staff
            Xenocide.Rng.RigDice(random);
            team = StaffOutpost().CreateCrew(Race.Grey, Difficulty.Sadistic, 100);
            Debug.Assert((26 <= team.Combatants.Count) && (team.Combatants.Count <= 28));

            // try a "shot up" ufo, should still be at least minimum staff
            Xenocide.Rng.RigDice(random);
            team = StaffOutpost().CreateCrew(Race.Grey, Difficulty.Sadistic, 0);
            Debug.Assert((7 <= team.Combatants.Count) && (team.Combatants.Count <= 26));
        }

        #endregion UnitTests
    }
}
