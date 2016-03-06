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
* @file MonthlyLog.cs
* @date Created: 2007/06/03
* @author File creator: dteviot
* @author Credits: none
*/
#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Xml;
using System.Xml.XPath;
using System.Threading;
using System.IO;

using CeGui;


using ProjectXenocide.Utils;
using ProjectXenocide.Model;
using ProjectXenocide.Model.StaticData;
using ProjectXenocide.Model.StaticData.Items;
using ProjectXenocide.Model.Geoscape;
using ProjectXenocide.Model.Geoscape.Outposts;
using ProjectXenocide.Model.Geoscape.Geography;
using ProjectXenocide.Model.Geoscape.GeoEvents;
using Xenocide.Resources;


#endregion

namespace ProjectXenocide.Model.Geoscape
{
    /// <summary>
    /// X-Corp on the Geoscape
    /// </summary>
    [Serializable]
    public class XCorp
    {
        /// <summary>
        /// Do start of month processing
        /// </summary>
        public void StartOfMonth()
        {
            bank.StartOfMonth(LastMonthsMaintenance(), FundingForThisMonth());
            totalScores.StartOfMonth();

            PsiTrainingThisMonth();

            CheckIfGameLost();
        }

        /// <summary>
        /// Adjust score
        /// </summary>
        /// <param name="side">which side is being changed</param>
        /// <param name="points">size of the change</param>
        public void AddScore(Participant side, float points)
        {
            totalScores.AddScore(side, points);
        }

        /// <summary>
        /// Create a unique name for this item
        /// </summary>
        /// <param name="itemType">type of item (from StaticTables.ItemList)</param>
        /// <returns>the created name</returns>
        public String CreateItemName(ItemInfo itemType)
        {
            // Figure out how many of this item there been
            // and record for next time
            int serialNumber = 1;
            if (nextItemId.ContainsKey(itemType.Id))
            {
                serialNumber = nextItemId[itemType.Id] + 1;
            }
            nextItemId[itemType.Id] = serialNumber;

            // build up the name string
            return Util.StringFormat(Strings.ITEM_UNIQUE_NAME_FORMAT, itemType.Name, serialNumber);
        }

        /// <summary>
        /// Check if player has lost the game
        /// </summary>
        private void CheckIfGameLost()
        {
            // if cheat mode on, skip tests
            if (Xenocide.StaticTables.StartSettings.Cheats.XcorpCantLooseAtStartOfMonth)
            {
                return;
            }

            int lastMonth = MonthlyLog.LastMonth;
            int twoMonthsAgo = MonthlyLog.LastMonth + 11;

            string gameOver = String.Empty;
            // Game lost if in debt of 1,000,000 or more for this and last month
            if ((Bank.Balances[lastMonth] < StartSettings.LooseGameMonthlyDebit) &&
                (Bank.Balances[twoMonthsAgo] < StartSettings.LooseGameMonthlyDebit))
            {
                gameOver = Util.StringFormat(Strings.MSGBOX_GAME_OVER_BAD_DEBT, StartSettings.LooseGameMonthlyDebit);
            }

            // Game lost if score is too low for this month and last month
            if ((totalScores.NetScore(lastMonth) < StartSettings.LooseGameMonthlyScore) &&
                (totalScores.NetScore(twoMonthsAgo) < StartSettings.LooseGameMonthlyScore))
            {
                gameOver = Util.StringFormat(Strings.MSGBOX_GAME_OVER_LOW_SCORE, StartSettings.LooseGameMonthlyScore);
            }

            // if Game lost, tell user and go to start screen
            if (!String.IsNullOrEmpty(gameOver))
            {
                Xenocide.GameState.GeoData.QueueEvent(new GameOverGeoEvent(gameOver));
            }
        }

        /// <summary>
        /// Calcuate how much funding X-Corp is getting from the countries this month
        /// </summary>
        /// <returns>the value of the funding</returns>
        private static int FundingForThisMonth()
        {
            int ThisMonthIndex = MonthlyLog.ThisMonth;
            int funds = 0;
            foreach (Country c in Xenocide.GameState.GeoData.Planet.AllCountries)
            {
                funds += c.Funds[ThisMonthIndex];
            }
            return funds;
        }

        /// <summary>
        /// Calculate and update psi skills statistics for all soldiers in psi training
        /// </summary>
        private static void PsiTrainingThisMonth()
        {
            foreach (Outpost o in Xenocide.GameState.GeoData.Outposts)
            {
                foreach (Person p in o.ListStaff("ITEM_PERSON_SOLDIER"))
                {
                    if (p.PsiTraining)
                    {
                        //ToDo: Calculate PSI Skill improvements for this soldier
                        //as per http://www.ufopaedia.org/index.php?title=Psi_Skill#Improvement
                        int psiSkill = p.Combatant.Stats[Battlescape.Combatants.Statistic.PsiSkill];
                        int minGain = 1, maxGain = 3;
                        if (psiSkill <= 16)
                        {
                            minGain = 16;
                            maxGain = 24;
                        }
                        else if (psiSkill <= 50)
                        {
                            minGain = 5;
                            maxGain = 12;
                        }
                        p.Combatant.Stats[Battlescape.Combatants.Statistic.PsiSkill] += minGain + Xenocide.Rng.Next(maxGain - minGain + 1);
                    }
                }
            }
        }

        /// <summary>
        /// Calcuate how much X-Corp had to spend on maintenance last month
        /// </summary>
        /// <returns>the value of the funding</returns>
        private static int LastMonthsMaintenance()
        {
            int cost = 0;
            foreach (Outpost o in Xenocide.GameState.GeoData.Outposts)
            {
                cost += o.CalcFacilityMaintenance();
                cost += o.CalcCraftMaintenance();
                cost += o.CalcStaffSalaries();
                cost += o.CalcInTransitMonthlyCharge();
            }
            return cost;
        }

        #region Fields

        /// <summary>
        /// The money X-Corp has
        /// </summary>
        public Bank Bank { get { return bank; } }

        /// <summary>
        /// Total Alien and X-Corp scores
        /// </summary>
        public ScoreLog TotalScores { get { return totalScores; } }

        /// <summary>
        /// The technologies X-Corp has access to
        /// </summary>
        public TechnologyManager TechManager { get { return techManager; } }

        /// <summary>
        /// Research projects in progress by X-Corp
        /// </summary>
        public ResearchProjectManager ResearchManager { get { return researchManager; } }

        /// <summary>
        /// The money X-Corp has
        /// </summary>
        private Bank bank = new Bank();

        /// <summary>
        /// Alien and X-Corp scores for this country
        /// </summary>
        private ScoreLog totalScores = new ScoreLog();

        /// <summary>
        /// Keep track of the IDs we've used to generate names for unique items
        /// </summary>
        private Dictionary<String, int> nextItemId = new Dictionary<String, int>();

        /// <summary>
        /// The technologies X-Corp has access to
        /// </summary>
        private TechnologyManager techManager = new TechnologyManager(Xenocide.StaticTables.ResearchGraph);

        /// <summary>
        /// Research projects in progress by X-Corp
        /// </summary>
        private ResearchProjectManager researchManager = new ResearchProjectManager();

        #endregion Fields
    }
}
