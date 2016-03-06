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
* @file Battlescape/MockMission.cs
* @date Created: 2008/01/19
* @author File creator: David Teviotdale
* @author Credits: none
*/
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

using ProjectXenocide.Model.StaticData;
using ProjectXenocide.Model.StaticData.Battlescape;
using ProjectXenocide.Model.Geoscape;
using ProjectXenocide.Model.Geoscape.Outposts;
using ProjectXenocide.Model.Battlescape.Combatants;
using ProjectXenocide.Model.Geoscape.Vehicles;

namespace ProjectXenocide.Model.Battlescape
{
    /// <summary>
    /// A dummy mission class, used for Unit Testing
    /// </summary>
    [Serializable]
    public class MockMission : Mission
    {
        /// <summary>Ctor</summary>
        public MockMission()
            :this(new Terrain.TestTerrainBuilder(TestTerrain.Standard))
        {
        }

        /// <summary>Ctor</summary>
        /// <param name="builder">constructs the cells making up the terrain</param>
        public MockMission(Terrain.TerrainBuilder builder)
            : base(null as Aircraft)
        {
            this.builder = builder;
        }

        /// <summary>Text to show on the start mission dialog</summary>
        /// <returns>message to show</returns>
        public override string MakeStartMissionText() { return String.Empty; }

        /// <summary>Battlescape terrain creation steps that are specific to this mission type</summary>
        /// <param name="terrain">The terrain we're creating</param>
        protected override void CreateTerrainCore(Terrain terrain)
        {
            builder.BuildCells(terrain);
        }

        /// <summary>Create the XCorp side for the battlescape</summary>
        /// <returns>The XCorp force</returns>
        public override Team CreateXCorpTeam()
        {
            Team team = new Team();
            team.Combatants.Add(Xenocide.StaticTables.CombatantFactory.MakeXCorpSoldier());
            team.Combatants[0].Stats[Statistic.TimeUnits] = 2000;
            return team;
        }

        /// <summary>Create the Alien force for the battlescape</summary>
        /// <returns>The alien force</returns>
        public override Team CreateAlienTeam()
        {
            Team team = new Team();
            team.Combatants.Add(Xenocide.StaticTables.CombatantFactory.MakeAlien(Race.Cloak, AlienRank.Soldier));
            team.Combatants[0].Stats[Statistic.TimeUnits] = 2000;
            return team;
        }

        #region fields

        /// <summary>Build the cells that represent a terrain</summary>
        [NonSerialized]
        private Terrain.TerrainBuilder builder;

        #endregion fields
    }
}
