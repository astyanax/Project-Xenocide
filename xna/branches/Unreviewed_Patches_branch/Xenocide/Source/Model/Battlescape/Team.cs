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
* @file Team.cs
* @date Created: 2008/01/05
* @author File creator: David Teviotdale
* @author Credits: none
*/
#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

using Microsoft.Xna.Framework;

using ProjectXenocide.Utils;

using ProjectXenocide.Model.Battlescape.Combatants;

#endregion

namespace ProjectXenocide.Model.Battlescape
{
    /// <summary>
    /// One of the sides (X-Corp, Alien or civilians) fighting on the battlescape
    /// </summary>
    [Serializable]
    public class Team
    {
        /// <summary>Perform actions that are done at start of turn</summary>
        public void OnStartTurn()
        {
            foreach (Combatant combatant in combatants)
            {
                combatant.OnStartTurn();
            }
        }

        /// <summary>Have all team members been defeated</summary>
        /// <returns>true if team has been defeated</returns>
        public bool IsDefeated()
        {
            foreach (Combatant combatant in combatants)
            {
                if (combatant.CanTakeOrders)
                {
                    return false;
                }
            }
            // if got here, all team members defeated
            return true;
        }

        #region Fields

        /// <summary>Index to alien team</summary>
        public const int Aliens = 0;

        /// <summary>Index to X-Corp team</summary>
        public const int XCorp = 1;

        /// <summary>Index to civilian team</summary>
        public const int Civilians = 2;

        /// <summary>The combatants this side has to fight with</summary>
        public IList<Combatant> Combatants { get { return combatants; } }

        /// <summary>The combatants this side has to fight with</summary>
        private List<Combatant> combatants = new List<Combatant>();

        #endregion Fields
    }
}
