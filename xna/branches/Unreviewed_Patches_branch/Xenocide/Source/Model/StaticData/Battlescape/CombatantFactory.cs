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
* @file CombatantFactory.cs
* @date Created: 2007/12/26
* @author File creator: David Teviotdale
* @author Credits: nil
*/
#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using System.Text;
using System.Diagnostics;
using System.Xml;
using System.Xml.XPath;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Storage;


using ProjectXenocide.Utils;
using ProjectXenocide.Model;
using ProjectXenocide.Model.Geoscape;
using ProjectXenocide.Model.StaticData.Items;
using ProjectXenocide.Model.Battlescape;
using ProjectXenocide.Model.Battlescape.Combatants;

#endregion

namespace ProjectXenocide.Model.StaticData.Battlescape
{
    /// <summary>
    /// Creates Combatants
    /// </summary>
    public partial class CombatantFactory
    {
        /// <summary>
        /// Construct Factory from an XML file
        /// </summary>
        /// <param name="filename">Name of the XML file</param>
        public void Populate(string filename)
        {
            // Set up XPathNavigator
            const string xmlns = "CombatantConfig";
            XPathNavigator nav = Util.MakeValidatingXPathNavigator(filename, xmlns);
            XmlNamespaceManager manager = new XmlNamespaceManager(nav.NameTable);
            manager.AddNamespace("c", xmlns);

            // Process the XML file
            //... combatant entries
            foreach (XPathNavigator combatantNode in nav.Select("/c:combatantdata/c:combatant", manager))
            {
                combatantInfos.Add(new CombatantInfo(combatantNode, manager));
            }

            //... loadout entries
            foreach (XPathNavigator loadoutNode in nav.Select("/c:combatantdata/c:loadout", manager))
            {
                loadouts.Add(Util.GetStringAttribute(loadoutNode, "name"), new LoadoutInfo(loadoutNode, manager));
            }

            ValidateList();
        }

        /// <summary>
        /// Build a Combatant for an X-Corp soldier
        /// </summary>
        /// <returns>the constructed Combatant</returns>
        public Combatant MakeXCorpSoldier()
        {
            Combatant combatant = new Combatant(combatantInfos[FindIndex(Race.Human, AlienRank.Soldier)], Team.XCorp);
            GiveInitialLoadout(combatant);
            return combatant;
        }

        /// <summary>
        /// Build a Combatant for an X-Cap
        /// </summary>
        /// <param name="item">X-Cap to build combatant for</param>
        /// <returns>the constructed Combatant</returns>
        public Combatant MakeXCap(Item item)
        {
            Debug.Assert(item.ItemInfo.IsXCap);
            // ToDo: implement
            return null;
        }

        /// <summary>
        /// Create a combatant that most closely matches the request
        /// </summary>
        /// <param name="race">Race of Alien</param>
        /// <param name="rank">Rank of Alien</param>
        /// <returns>the constructed Combatant</returns>
        /// <remarks>If alien race does not have member of specified rank,
        /// substitue next highest rank available
        /// </remarks>
        public Combatant MakeAlien(Race race, AlienRank rank)
        {
            Combatant combatant = new Combatant(combatantInfos[FindIndex(race, rank)], Team.Aliens);
            GiveInitialLoadout(combatant);
            return combatant;
        }

        /// <summary>
        /// Return index to CombatantInfo that most closely matches the request
        /// </summary>
        /// <param name="race">Race of Alien</param>
        /// <param name="rank">Rank of Alien</param>
        /// <returns>the constructed Combatant</returns>
        /// <remarks>If alien race does not have member of specified rank, will try substitue next highest rank available</remarks>
        private int FindIndex(Race race, AlienRank rank)
        {
            // Try to build alien of specified race and rank.
            // Note, there's a couple of special cases.
            // 1, if can't find alien of specified rank, take next highest we can find.
            // 2, if terrorist unit, need to map to ownerRace, and allow multiple terror units in race
            List<int> candidates = new List<int>();
            for (int i = 0; i < combatantInfos.Count; ++i)
            {
                CombatantInfo info = combatantInfos[i];
                if ((info.Race == race) || (info.OwnerRace == race))
                {
                    if (AlienRank.Terrorist == rank)
                    {
                        if (info.Rank == AlienRank.Terrorist)
                        {
                            candidates.Add(i);
                        }
                    }
                    else
                    {
                        if (rank <= info.Rank)
                        {
                            if (0 == candidates.Count)
                            {
                                candidates.Add(i);
                            }
                            else if (info.Rank < combatantInfos[candidates[0]].Rank)
                            {
                                candidates[0] = i;
                            }
                        }
                    }
                }
            }
            // pick a candidate
            if (0 < candidates.Count)
            {
                int index = Xenocide.Rng.Next(candidates.Count);
                return candidates[index];
            }
            else
            {
                // Doesn't exist
                Debug.Assert(false);
                return -1;
            }
        }

        /// <summary>Load a combatant with his/her initial equipement</summary>
        /// <param name="combatant">to equip</param>
        private void GiveInitialLoadout(Combatant combatant)
        {
            LoadoutInfo loadout;
            if (loadouts.TryGetValue(combatant.CombatantInfo.PickInitialLoadout(), out loadout))
            {
                loadout.Equip(combatant.Inventory);
            }
        }

        /// <summary>
        /// Check that the list of combatants we've loaded is good
        /// </summary>
        /// <remarks>Will throw if list isn't valid</remarks>
        private void ValidateList()
        {
            // ToDo: Implement
        }

        #region Fields

        /// <summary>
        /// The diferent types of combatants that can be constructed
        /// </summary>
        public IList<CombatantInfo> CombatantInfos { get { return combatantInfos; } }

        /// <summary>
        /// The diferent types of combatants that can be constructed
        /// </summary>
        private List<CombatantInfo> combatantInfos = new List<CombatantInfo>();

        /// <summary>The lists of things combatants can come equiped with</summary>
        private Dictionary<string, LoadoutInfo> loadouts = new Dictionary<string, LoadoutInfo>();

        #endregion
    }
}
