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
* @file TeamAI.cs
* @date Created: 2008/03/09
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
    /// The Team level AI logic for one of the teams (X-Corp, Alien) on the battlescape
    /// </summary>
    [Serializable]
    public class TeamAI
    {
        /// <summary>Ctor</summary>
        /// <param name="battlescape">the battlescape the teams are fighting on</param>
        /// <param name="team">the forces this AI controls</param>
        /// <param name="enemy">the forces this AI fights</param>
        public TeamAI(Battle battlescape, Team team, Team enemy)
        {
            this.battlescape = battlescape;
            this.team        = team;
            this.enemy       = enemy;

            // equip each combatant with its own AI
            foreach (Combatant c in team.Combatants)
            {
                c.Battlescape = battlescape;
                c.AI = new CombatantAI(this, c);
            }
        }

        /// <summary>Called when it's this team's time to act on the battlescape</summary>
        /// <remarks>Don't confuse this with the actual start of the turn, when nothing has happened</remarks>
        public void OnStartTeamTurn()
        {
            currentCombatant = 0;
        }

        /// <summary>Update AI based on passage of time</summary>
        /// <param name="seconds">length of time that has passed</param>
        /// <returns>false if AI has nothing more to do this turn</returns>
        public bool Update(double seconds)
        {
            // cycled through combatants until find one that can do something
            while (currentCombatant < team.Combatants.Count)
            {
                Combatant combatat = team.Combatants[currentCombatant];
                if (combatat.CanTakeOrders && combatat.AI.Update(seconds))
                {
                    // combatant still has stuff to do
                    return true;
                }
                ++currentCombatant;
            }
            // if get here, nothing more to do
            return false;
        }

        #region Fields

        /// <summary>the battlescape the teams are fighting on</summary>
        public Battle Battlescape { get { return battlescape; } }

        /// <summary>the forces this AI controls</summary>
        public Team Team { get { return team; } }

        /// <summary>the forces this AI fights</summary>
        public Team Enemy { get { return enemy; } }

        /// <summary>the battlescape the teams are fighting on</summary>
        private Battle battlescape;

        /// <summary>the forces this AI controls</summary>
        private Team team;

        /// <summary>the forces this AI fights</summary>
        private Team enemy;

        /// <summary>combatant that currently has the AI's "focus"</summary>
        private int currentCombatant;

        #endregion
    }
}
