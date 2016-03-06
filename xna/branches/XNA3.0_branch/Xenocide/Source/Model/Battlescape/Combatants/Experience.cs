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
* @file Experience.cs
* @date Created: 2008/02/25
* @author File creator: David Teviotdale
* @author Credits: none
*/
#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

#endregion

namespace ProjectXenocide.Model.Battlescape.Combatants
{
    /// <summary>Record doing things (during a battlescape mission) that qualify as learning experience</summary>
    [Serializable]
    public class Experience
    {
        /// <summary>Acts that qualify as learning opportunities</summary>
        public enum Act
        {
            /// <summary>Fired a weapon and hit the target</summary>
            ShotHitTarget,

            /// <summary>Killed a combatant</summary>
            KilledTarget,
        }

        /// <summary>Record that combatant did something that counts as a "learning experience"</summary>
        /// <param name="act">what was done</param>
        public void RecordAchievement(Act act)
        {
            ++acts[(int)act];
        }

        /// <summary>At end of mission, update combatant's stats to reflect the experience gained</summary>
        /// <param name="combatant">combatant to update</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "combatant",
            Justification = "ToDo: function needs implementation")]
        public void UpdateStats(Combatant combatant)
        {
            //ToDo: implement
        }

        #region Fields

        /// <summary>Number of times each type of act was done this mission</summary>
        private int[] acts = new int[Enum.GetValues(typeof(Act)).Length];

        #endregion Fields
    }
}
