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
* @file Battlescape/Mission.cs
* @date Created: 2007/12/30
* @author File creator: David Teviotdale
* @author Credits: none
*/
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using ProjectXenocide.Utils;
using ProjectXenocide.Model.StaticData;
using ProjectXenocide.Model.StaticData.Battlescape;
using ProjectXenocide.Model.Geoscape;
using ProjectXenocide.Model.Geoscape.Outposts;
using ProjectXenocide.Model.Geoscape.Vehicles;
using ProjectXenocide.Model.Battlescape.Combatants;
using Xenocide.Resources;

using ScoreEntry = ProjectXenocide.Utils.Pair<string, int>;

// tracks "number killed/captured" and "total points"
using KillAndPoints = ProjectXenocide.Utils.Pair<int, int>;

// tracks "number killed/captured", "total points" for each "alien type"
using CasualtyList = System.Collections.Generic.Dictionary<string, ProjectXenocide.Utils.Pair<int, int>>;


namespace ProjectXenocide.Model.Battlescape
{
    /// <summary>The ways a battlescape mission can finish</summary>
    public enum BattleFinish
    {
        /// <summary>The Alien forces won</summary>
        AlienVictory,

        /// <summary>The X-Corp forces won</summary>
        XCorpVictory,

        /// <summary>Mission was aborted</summary>
        Aborted,
    }

    /// <summary>
    /// Base class for type of Battlescape mission, e.g. Attacking landed UFO, Terror site, Alien Outpost, etc.
    /// </summary>
    [Serializable]
    public abstract partial class Mission
    {
        /// <summary>
        /// X-Corp Craft that is landing troops
        /// </summary>
        /// <param name="aircraft"></param>
        protected Mission(Aircraft aircraft)
        {
            this.aircraft = aircraft;
        }

        /// <summary>
        /// Ctor used for XCorpOutpostMission
        /// </summary>
        /// <param name="outpost"></param>
        protected Mission(Outpost outpost)
        {
            this.outpost = outpost;
        }

        /// <summary>
        /// Text to show on the start mission dialog
        /// </summary>
        /// <returns>message to show</returns>
        public abstract string MakeStartMissionText();

        /// <summary>
        /// Create the Battlescape's terrain
        /// </summary>
        /// <returns>the created terrain</returns>
        public Terrain CreateTerrain()
        {
            // ToDo: put any steps specific to all missions here
            Terrain terrain = new Terrain(this);
            CreateTerrainCore(terrain);
            return terrain;
        }

        /// <summary>
        /// Called if we're not going to start the battlescape at this point in time.
        /// </summary>
        public virtual void DontStart()
        {
        }

        /// <summary>
        /// Battlescape terrain creation steps that are specific to this mission type
        /// </summary>
        /// <param name="terrain">The terrain we're creating</param>
        protected virtual void CreateTerrainCore(Terrain terrain)
        {
            // for moment, use "random city" terrain
            //Terrain.TerrainBuilder builder = new Terrain.RandomCityTerrainBuilder();
            // for moment, use "random dungeon" terrain
            Terrain.TerrainBuilder builder = new Terrain.MazeTerrainBuilder();
            builder.BuildCells(terrain);
        }

        /// <summary>Handle mission ending</summary>
        /// <param name="battlescape">Details of battle</param>
        /// <param name="finishType">Who won the battle</param>
        /// <remarks>Method Template pattern</remarks>
        public void OnFinish(Battle battlescape, BattleFinish finishType)
        {
            CalcLosses(battlescape, finishType);

            // ToDo: put any steps specific to all missions here
            OnFinishCore(finishType);
        }

        /// <summary>
        /// Any mission ending handling that's specific to this type of mission goes here
        /// </summary>
        /// <param name="finishType">Who won the battle</param>
        protected virtual void OnFinishCore(BattleFinish finishType)
        {
        }

        /// <summary>
        /// Create the Alien force for the battlescape
        /// </summary>
        /// <returns>The alien force</returns>
        public virtual Team CreateAlienTeam()
        {
            //ToDo: replace this sub with proper implemention
            Team team = new Team();
            team.Combatants.Add(Xenocide.StaticTables.CombatantFactory.MakeAlien(Race.Morlock, AlienRank.Soldier));
            return team;
        }

        /// <summary>Create the XCorp side for the battlescape</summary>
        /// <returns>The XCorp force</returns>
        public virtual Team CreateXCorpTeam()
        {
            // X-Corp soldiers carried by aircraft
            Team team = new Team();
            foreach (Person person in aircraft.Soldiers.Keys)
            {
                team.Combatants.Add(person.Combatant);
            }
            return team;
        }

        /// <summary>
        /// Create the Civilian side for the battlescape
        /// </summary>
        /// <returns>The civilian targets</returns>
        public virtual Team CreateCivilianTeam()
        {
            // only terror missions have civilians
            return new Team();
        }

        /// <summary>Add item(s) to the list of salvaged items</summary>
        /// <param name="itemId">index to item in static items table.</param>
        /// <param name="quantity">number of instances of item to add.</param>
        public void AddToSalvage(string itemId, int quantity)
        {
            foreach (ItemLine itemLine in salvage)
            {
                if (itemLine.ItemId == itemId)
                {
                    itemLine.Quantity += quantity;
                    return;
                }
            }
            // not found, add to collection
            salvage.Add(new ItemLine(itemId, quantity));
        }

        /// <summary>Add a set of items to the salvaged list</summary>
        /// <param name="items">items to add</param>
        public void AddToSalvage(IList<ItemLine> items)
        {
            foreach (ItemLine item in items)
            {
                AddToSalvage(item.ItemId, item.Quantity);
            }
        }

        /// <summary>Figure out number of each type of alien killed</summary>
        /// <param name="battlescape">Details of battle</param>
        protected virtual void ScoreKilledAliens(Battle battlescape)
        {
            foreach (Combatant combatant in battlescape.Teams[Team.Aliens].Combatants)
            {
                if (combatant.IsDead)
                {
                    RecordAlien(kills, true, combatant.CombatantInfo);
                }
            }
        }

        /// <summary>Figure out number of X-Corp soliders and civilians killed if mission aborted</summary>
        /// <param name="battlescape">Details of battle</param>
        protected virtual void CalcXCorpLossesOnAbort(Battle battlescape)
        {
            // any soldier not on an exit square is dead
            for (int i = aircraft.Soldiers.Keys.Count - 1; 0 <= i; --i)
            {
                Person soldier = aircraft.Soldiers.Keys[i];
                Combatant combatant = soldier.Combatant;
                bool onExit = combatant.IsOnExitTile();
                if (combatant.IsDead || !onExit)
                {
                    soldier.DiedOnMission(onExit);
                    ScoreXCorpKia(soldier);
                }
            }
        }

        /// <summary>Figure out number of Aliens killed if mission aborted</summary>
        /// <param name="battlescape">Details of battle</param>
        protected virtual void CalcAlienLossesOnAbort(Battle battlescape)
        {
            foreach (Combatant combatant in battlescape.Teams[Team.Aliens].Combatants)
            {
                // will gain any aliens in craft on exit
                if (combatant.IsOnExitTile())
                {
                    if (!combatant.IsDead)
                    {
                        RecordAlien(captures, false, combatant.CombatantInfo);
                    }
                    RecoverAlien(combatant);
                }
            }
        }

        /// <summary>Record killing or capturing an alien</summary>
        /// <param name="dict">Where to record the action</param>
        /// <param name="dead">Dead or Captured?</param>
        /// <param name="info">Info on this type of alien</param>
        private static void RecordAlien(CasualtyList dict, bool dead, CombatantInfo info)
        {
            // for reporting purposes, aggregate by type
            string name = Xenocide.StaticTables.ItemList[info.StunItemId].Name;
            int score = info.VictoryPoints * (dead ? 1 : 2);
            if (dict.ContainsKey(name))
            {
                KillAndPoints pair = dict[name];
                ++pair.First;
                pair.Second += score;
                dict[name] = pair;
            }
            else
            {
                dict.Add(name, new KillAndPoints(1, score));
            }
        }

        /// <summary>Add alien (and items carried) to mission's salvage</summary>
        /// <param name="combatant">the alien</param>
        private void RecoverAlien(Combatant combatant)
        {
            // alien
            CombatantInfo info = combatant.CombatantInfo;
            int index = combatant.IsDead ? info.DeadItemId : info.StunItemId;
            AddToSalvage(Xenocide.StaticTables.ItemList[index].Id, 1);

            // items being carried
            foreach (CombatantInventory.Slot slot in combatant.Inventory.SnapshotLoadout())
            {
                Debug.Assert(!slot.Item.ItemInfo.IsUnique);
                AddToSalvage(slot.Item.ItemInfo.Id, 1);
            }
        }

        /// <summary>Figure out number of X-Corp soliders and civilians killed if aliens win</summary>
        /// <param name="battlescape">Details of battle</param>
        protected virtual void CalcXCorpLossesOnAlienVictory(Battle battlescape)
        {
            // all X-Corp soldiers are dead
            for (int i = aircraft.Soldiers.Keys.Count - 1; 0 <= i; --i)
            {
                Person soldier = aircraft.Soldiers.Keys[i];
                soldier.DiedOnMission(false);
                ScoreXCorpKia(soldier);
            }
        }

        /// <summary>Figure out number of Aliens killed if aliens win</summary>
        /// <param name="battlescape">Details of battle</param>
        protected virtual void CalcAlienLossesOnAlienVictory(Battle battlescape)
        {
            // nothing to do
        }

        /// <summary>Figure out number of X-Corp soliders and civilians killed if X-Corp win</summary>
        /// <param name="battlescape">Details of battle</param>
        protected virtual void CalcXCorpLossesOnXCorpVictory(Battle battlescape)
        {
            for (int i = aircraft.Soldiers.Keys.Count - 1; 0 <= i; --i)
            {
                Person soldier = aircraft.Soldiers.Keys[i];
                if (soldier.Combatant.IsDead)
                {
                    soldier.DiedOnMission(true);
                    ScoreXCorpKia(soldier);
                }
            }
        }

        /// <summary>Figure out number of Aliens killed if X-Corp win</summary>
        /// <param name="battlescape">Details of battle</param>
        protected virtual void CalcAlienLossesOnXCorpVictory(Battle battlescape)
        {
            // any surviving aliens are captured
            foreach (Combatant combatant in battlescape.Teams[Team.Aliens].Combatants)
            {
                if (!combatant.IsDead)
                {
                    RecordAlien(captures, false, combatant.CombatantInfo);
                }
                RecoverAlien(combatant);
            }
        }

        /// <summary>Add score entry for X-Corp soldier KIA</summary>
        /// <param name="soldier">The dead soldier</param>
        protected void ScoreXCorpKia(Person soldier)
        {
            String description = Util.StringFormat(Strings.SCREEN_BATTLESCAPE_REPORT_ROW_XCORP_KIA, soldier.Name);
            scores.Add(new ScoreEntry(description, -soldier.Combatant.Stats[Statistic.VictoryPoints]));
        }

        /// <summary>Build up report entires for aliens killed and captured</summary>
        /// <param name="list">The set of aliens killed or captured</param>
        /// <param name="dead">Killed or Captured?</param>
        private void AddAlienCasualitiesToScore(CasualtyList list, bool dead)
        {
            foreach (KeyValuePair<string, KillAndPoints> entry in list)
            {
                // get info on type of alien
                string description = Util.StringFormat(Strings.SCREEN_BATTLESCAPE_REPORT_ROW_ALIEN_CASUALTY,
                    dead ? Strings.SCREEN_BATTLESCAPE_REPORT_KILLED : Strings.SCREEN_BATTLESCAPE_REPORT_CAPTURED,
                    Util.ToString(entry.Value.First),
                    entry.Key
                );
                scores.Add(new ScoreEntry(description, entry.Value.Second));
            }
        }

        /// <summary>Add loss of aircraft to other X-Corp mission losses</summary>
        private void CalcLossOfAircraft()
        {
            if (null != aircraft)
            {
                string description = Util.StringFormat(Strings.SCREEN_BATTLESCAPE_REPORT_AIRCRAFT_LOST, aircraft.Name);
                scores.Add(new ScoreEntry(description, (int)-aircraft.ItemInfo.Score));
            }
        }

        /// <summary>Figure out number of X-Corp soliders, Aliens and civilians killed</summary>
        /// <param name="battlescape">Details of battle</param>
        /// <param name="finishType">Who won the battle</param>
        private void CalcLosses(Battle battlescape, BattleFinish finishType)
        {
            ScoreKilledAliens(battlescape);
            switch (finishType)
            {
                case BattleFinish.Aborted:
                    CalcXCorpLossesOnAbort(battlescape);
                    CalcAlienLossesOnAbort(battlescape);
                    break;

                case BattleFinish.AlienVictory:
                    CalcLossOfAircraft();
                    CalcXCorpLossesOnAlienVictory(battlescape);
                    CalcAlienLossesOnAlienVictory(battlescape);
                    break;

                case BattleFinish.XCorpVictory:
                    CalcXCorpLossesOnXCorpVictory(battlescape);
                    CalcAlienLossesOnXCorpVictory(battlescape);
                    break;

                default:
                    Debug.Assert(false);
                    break;
            }
            AddAlienCasualitiesToScore(kills, true);
            AddAlienCasualitiesToScore(captures, false);
        }

        #region Fields

        /// <summary>
        /// Where the battle happened
        /// </summary>
        public GeoPosition Location { get { return location; } protected set { location = value; } }

        /// <summary>
        /// Where to put any recovered items
        /// </summary>
        public Outpost Outpost { get { return outpost; } protected set { outpost = value; } }

        /// <summary>
        /// The items that were recovered
        /// </summary>
        public IList<ItemLine> Salvage { get { return salvage; } protected set { salvage = value; } }

        /// <summary>X-Corp Craft that is landing troops</summary>
        protected Aircraft Aircraft { get { return aircraft; } }

        /// <summary>Things that happend on battlescape that earned/cost X-Corp points</summary>
        /// <remarks>Format is "Description to show user", "points lost/gained"</remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures",
            Justification = "We are not VB programmers")]
        public IList<ScoreEntry> Scores { get { return scores; } }

        /// <summary>
        /// Where the battle happened
        /// </summary>
        private GeoPosition location;

        /// <summary>
        /// Where to put any recovered items
        /// </summary>
        private Outpost outpost;

        /// <summary>
        /// The items that were recovered
        /// </summary>
        private IList<ItemLine> salvage = new List<ItemLine>();

        /// <summary>Things that happend on battlescape that earned/cost X-Corp points</summary>
        /// <remarks>Format is "Description to show user", "points lost/gained"</remarks>
        private List<ScoreEntry> scores = new List<ScoreEntry>();

        /// <summary>Aliens killed</summary>
        /// <remarks>Format is ItemID for alien type, count</remarks>
        private CasualtyList kills = new CasualtyList();

        /// <summary>Aliens captured</summary>
        /// <remarks>Format is ItemID for alien type, count</remarks>
        private CasualtyList captures = new CasualtyList();

        /// <summary>X-Corp Craft that is landing troops</summary>
        private Aircraft aircraft;

        #endregion Fields
    }
}
