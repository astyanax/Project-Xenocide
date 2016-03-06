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
* @file Battle.cs
* @date Created: 2007/12/30
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
using ProjectXenocide.Model.StaticData.Items;

#endregion

namespace ProjectXenocide.Model.Battlescape
{
    /// <summary>
    /// Root node that holds everything on the Battlescape
    /// </summary>
    /// <remarks>Ideally I'd call this Battlescape, but that gives naming problems</remarks>
    [Serializable]
    public class Battle
    {
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="mission">The type of mission. e.g. Attacking landed UFO, Terror site, etc.</param>
        public Battle(Mission mission)
        {
            this.mission = mission;
            this.terrain = mission.CreateTerrain();

            // create the Alien, X-Corp, and Civilian Forces
            teams.Add(mission.CreateAlienTeam());
            teams.Add(mission.CreateXCorpTeam());
            teams.Add(mission.CreateCivilianTeam());

            terrain.DeployAlienTeam(teams[Team.Aliens]);
            terrain.DeployXCorpTeam(teams[Team.XCorp]);

            // initialise X-Corp soldiers
            foreach (Combatant soldier in teams[Team.XCorp].Combatants)
            {
                UpdateVisibility(soldier);
                soldier.Battlescape = this;
            }

            alienAI = new TeamAI(this, teams[Team.Aliens], teams[Team.XCorp]);
        }

        /// <summary>
        /// Find if there's a combatant, belonging to specified team, in a cell on the Battlescape
        /// </summary>
        /// <param name="position">location of cell</param>
        /// <returns>Combatant if found, null if nothing there</returns>
        public Combatant FindCombatantAt(Vector3 position)
        {
            int id = terrain.FindCombatantAt(position);
            return (0 == id) ? null : teams[(id >> 6) - 1].Combatants[id & 31];
        }

        /// <summary>Perform actions that are done at start of turn</summary>
        public void OnStartTurn()
        {
            foreach (Team team in teams)
            {
                team.OnStartTurn();
            }
        }

        /// <summary>calculate which opposing combatants can see/are seen by a combatant</summary>
        /// <param name="combatant">to do calcuations for</param>
        public void UpdateVisibility(Combatant combatant)
        {
            // for X-Corp and civs the aliens are the enemy.  For aliens, X-Corp are enemy
            Team enemyTeam = teams[Team.Aliens];
            if (Team.Aliens == combatant.TeamId)
            {
                enemyTeam = teams[Team.XCorp];
            }
            terrain.UpdateVisibility(combatant, enemyTeam.Combatants);
        }

        /// <summary>Mostly null references, so garbage collector gets everything</summary>
        public void PostMissionCleanup()
        {
            foreach (Combatant combatant in teams[Team.XCorp].Combatants)
            {
                combatant.PostMissionCleanup();
            }
        }

        /// <summary>Adds an item to a position</summary>
        public void AddToGround(Item item, Vector3 position)
        {
            List<Item> items = null;
            if (!groundContents.TryGetValue(position, out items))
            {
                items = new List<Item>();
                groundContents.Add(position, items);
            }
            items.Add(item);
        }

        /// <summary>Removes an item from a position</summary>
        public void RemoveFromGround(Item item, Vector3 position)
        {
            Debug.Assert(groundContents.ContainsKey(position));
            groundContents[position].Remove(item);
        }

        /// <summary>Lists the items at a position</summary>
        public IEnumerable<Item> ListGroundContents(Vector3 position)
        {
            List<Item> items = null;
            if (!groundContents.TryGetValue(position, out items))
            {
                items = new List<Item>();
            }
            return items;
        }

        #region Fields

        /// <summary>The type of mission. e.g. Attacking landed UFO, Terror site, etc.</summary>
        public Mission Mission { get { return mission; } }

        /// <summary>The "landscape" the battle will be fought on</summary>
        public Terrain Terrain { get { return terrain; } }

        /// <summary>The sides that are fighting (X-Corp, Alien and Civilians)</summary>
        public IList<Team> Teams { get { return teams; } }

        /// <summary>The trajectory of projectile in transit on the battlescape</summary>
        public Trajectory Trajectory { get { return trajectory; } set { trajectory = value; } }

        /// <summary>The alien forces AI</summary>
        public TeamAI AlienAI { get { return alienAI; } }

        /// <summary>The type of mission. e.g. Attacking landed UFO, Terror site, etc.</summary>
        private Mission mission;

        /// <summary>The "landscape" the battle will be fought on</summary>
        private Terrain terrain;

        /// <summary>The sides that are fighting (X-Corp, Alien and Civilians)</summary>
        private List<Team> teams = new List<Team>();

        /// <summary>The alien forces AI</summary>
        private TeamAI alienAI;

        /// <summary>The trajectory of projectile in transit on the battlescape</summary>
        /// <remarks>Can't save a game while projectile is moving</remarks>
        [NonSerialized]
        private Trajectory trajectory;

        /// <summary>The items on the ground</summary>
        private Dictionary<Vector3, List<Item>> groundContents = new Dictionary<Vector3, List<Item>>();

        #endregion
    }
}
