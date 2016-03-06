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
* @file Battlescape/XCorpOutpostMission.cs
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

using ProjectXenocide.Model.Geoscape;
using ProjectXenocide.Model.Geoscape.Vehicles;
using ProjectXenocide.Model.Geoscape.AI;
using ProjectXenocide.Model.Geoscape.Outposts;
using ProjectXenocide.Model.Battlescape.Combatants;
using Xenocide.Resources;


namespace ProjectXenocide.Model.Battlescape
{
    /// <summary>
    /// Mission is UFO attacking a X-Corp Outpost
    /// </summary>
    [Serializable]
    public class XCorpOutpostMission : Mission
    {
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="ufo">UFO that is attacking the outpost</param>
        /// <param name="outpost">X-Corp Outpost that is being attacked</param>
        public XCorpOutpostMission(Craft ufo, Outpost outpost)
            :
            base(outpost)
        {
            this.ufo = ufo as Ufo;
        }

        /// <summary>
        /// Text to show on the start mission dialog
        /// </summary>
        /// <returns>message to show</returns>
        public override string MakeStartMissionText()
        {
            return Util.StringFormat(Strings.MSGBOX_START_BATTLESCAPE, Outpost.Name, ufo.Name);
        }

        /// <summary>
        /// Any mission ending handling that's specific to this type of mission goes here
        /// </summary>
        /// <param name="finishType">Who won the battle</param>
        protected override void OnFinishCore(BattleFinish finishType)
        {
            // Note results of mission
            Location = Outpost.Position;
            //... Pretend fight occured, and was complete success
            if (BattleFinish.XCorpVictory == finishType)
            {
                // Tell UFO that it has been killed
                ufo.OnDestroyed();
            }
            else
            {
                Outpost.OnDestroyed();
            }
        }

        /// <summary>
        /// Create the Alien force for the battlescape
        /// </summary>
        public override Team CreateAlienTeam()
        {
            return ufo.CreateCrew(Xenocide.StaticTables.StartSettings.Difficulty);
        }

        /// <summary>Create the XCorp side for the battlescape</summary>
        /// <returns>The XCorp force</returns>
        public override Team CreateXCorpTeam()
        {
            // for moment, just grab up to the first 24 soldiers in the outpost
            Team team = new Team();
            int i = 0;
            foreach (Person person in Outpost.ListStaff("ITEM_PERSON_SOLDIER"))
            {
                if (i < 24)
                {
                    ++i;
                    team.Combatants.Add(person.Combatant);
                }
            }
            return team;
        }

        /// <summary>Figure out number of X-Corp soliders and civilians killed if mission aborted</summary>
        /// <param name="battlescape">Details of battle</param>
        protected override void CalcXCorpLossesOnAbort(Battle battlescape)
        {
            // same results as XCorp loosing
            CalcXCorpLossesOnAlienVictory(battlescape);
        }

        /// <summary>Figure out number of Aliens killed if mission aborted</summary>
        /// <param name="battlescape">Details of battle</param>
        protected override void CalcAlienLossesOnAbort(Battle battlescape)
        {
            // same results as XCorp loosing
            CalcAlienLossesOnAlienVictory(battlescape);
        }

        /// <summary>Figure out number of X-Corp soliders and civilians killed if aliens win</summary>
        /// <param name="battlescape">Details of battle</param>
        protected override void CalcXCorpLossesOnAlienVictory(Battle battlescape)
        {
            // all X-Corp soldiers in base are dead
            CalcXCorpLosses(true);
        }

        /// <summary>Figure out number of X-Corp soliders and civilians killed if X-Corp win</summary>
        /// <param name="battlescape">Details of battle</param>
        protected override void CalcXCorpLossesOnXCorpVictory(Battle battlescape)
        {
            // only casualties are dead soldiers
            CalcXCorpLosses(false);
        }

        /// <summary>Build the casualty list</summary>
        /// <param name="allLost">Were all soldiers in the outpost killed?</param>
        private void CalcXCorpLosses(bool allLost)
        {
            for (int i = Outpost.Inventory.Staff.Count - 1; 0 <= i; --i)
            {
                Person person = Outpost.Inventory.Staff[i];
                Combatant combatant = person.Combatant;
                if ((null != combatant) && (allLost || combatant.IsDead))
                {
                    person.DiedOnMission(!allLost);
                    ScoreXCorpKia(person);
                }
            }
        }

        #region Fields

        /// <summary>
        /// UFO that is attacking the outpost
        /// </summary>
        private Ufo ufo;


        #endregion
    }
}
