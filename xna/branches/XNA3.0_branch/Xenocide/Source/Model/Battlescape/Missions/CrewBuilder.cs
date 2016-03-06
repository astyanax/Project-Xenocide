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
* @file CrewBuilder.cs
* @date Created: 2008/01/28
* @author File creator: David Teviotdale
* @author Credits: none
*/
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Xml.XPath;
using System.Xml;

using ProjectXenocide.Utils;

using ProjectXenocide.Model.Battlescape.Combatants;
using Xenocide.Resources;


namespace ProjectXenocide.Model.Battlescape
{
    /// <summary>
    /// Constructs the aliens that will be encountered on a mission
    /// </summary>
    [Serializable]
    public partial class CrewBuilder
    {
        /// <summary>
        /// For internal use only
        /// </summary>
        private CrewBuilder()
        {
        }

        /// <summary>
        /// Construct from UFO node in item.xml
        /// </summary>
        /// <param name="ufoInfo">XML node holding data to construct CrewBuilder</param>
        /// <param name="manager">Namespace used in item.xml</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
           Justification = "will throw if itemNode == null")]
        public CrewBuilder(XPathNavigator ufoInfo, XmlNamespaceManager manager)
        {
            int max = 0;
            foreach (XPathNavigator crewNode in ufoInfo.Select("i:crew", manager))
            {
                CrewEntry entry = new CrewEntry(crewNode);
                crewList.Add(entry);
                max += entry.MaxAliens;
            }
            if (32 < max)
            {
                throw new XPathException(Util.StringFormat(Strings.EXCEPTION_UFO_CREW_TOO_BIG));
            }
        }

        /// <summary>
        /// Create the crew, on the battlescape
        /// </summary>
        /// <param name="race">race for the aliens</param>
        /// <param name="difficulty">game difficulty</param>
        /// <param name="ufoHealth">How shot up the UFO was, 100 == undamaged, 0 = completely destroyed</param>
        /// <returns>The aliens to put on the battlescape</returns>
        public Team CreateCrew(Race race, Difficulty difficulty, int ufoHealth)
        {
            Team team = new Team();
            foreach (CrewEntry entry in crewList)
            {
                int count = entry.Count(difficulty);
                for (int i = 0; i < count; ++i)
                {
                    // At least half the crew will survive, and there's always at least one survivor
                    if ((i == 0) || Xenocide.Rng.RollDice(50 + (ufoHealth / 2)))
                    {
                        team.Combatants.Add(entry.MakeAlien(race));
                    }
                }
            }
            return team;
        }

        /// <summary>
        /// Return Builder suitable fore making a set of aliens matching the staff of an Alien Outpost
        /// </summary>
        public static CrewBuilder StaffOutpost()
        {
            CrewBuilder builder = new CrewBuilder();
            builder.crewList.Add(new CrewEntry(AlienRank.Soldier,   5, 7, 4));
            builder.crewList.Add(new CrewEntry(AlienRank.Navigator, 1, 2, 0));
            builder.crewList.Add(new CrewEntry(AlienRank.Medic,     1, 1, 0));
            builder.crewList.Add(new CrewEntry(AlienRank.Engineer,  1, 2, 0));
            builder.crewList.Add(new CrewEntry(AlienRank.Leader,    2, 4, 0));
            builder.crewList.Add(new CrewEntry(AlienRank.Commander, 1, 1, 0));
            builder.crewList.Add(new CrewEntry(AlienRank.Terrorist, 1, 5, 2));
            return builder;
        }

        /// <summary>
        /// Details on how many aliens of a given rank should be found
        /// </summary>
        [Serializable]
        private struct CrewEntry
        {
            /// <summary>
            /// Construct from a "crew" xml node item.xml
            /// </summary>
            /// <param name="crewNode">XML node holding data to construct</param>
            public CrewEntry(XPathNavigator crewNode)
                : this(
                    Util.ParseEnum<AlienRank>(Util.GetStringAttribute(crewNode, "rank")),
                    Util.GetIntAttribute(crewNode, "minEasy"),
                    Util.GetIntAttribute(crewNode, "minHard"),
                    Util.GetIntAttribute(crewNode, "extra")
                )
            {
            }

            /// <summary>
            /// Ctor
            /// </summary>
            /// <param name="rank">The rank</param>
            /// <param name="minEasy">Minimim number at this rank (on easy level)</param>
            /// <param name="minHard">Minimim number at this rank (on hard level)</param>
            /// <param name="extra">Maximum additional at this rank</param>
            public CrewEntry(AlienRank rank, int minEasy, int minHard, int extra)
            {
                this.Rank    = rank;
                this.MinEasy = minEasy;
                this.MinHard = minHard;
                this.Extra   = extra;
            }

            /// <summary>Number of aliens of this rank to create</summary>
            /// <param name="difficulty">Game difficulty level</param>
            /// <returns>NumberToCreate</returns>
            public int Count(Difficulty difficulty)
            {
                int max = (difficulty < Difficulty.Hard) ? MinEasy : MinHard;
                return max + Xenocide.Rng.Next(Extra + 1);
            }

            /// <summary>
            /// Create an alien (of rank specified by this entry)
            /// </summary>
            /// <param name="race">alien race</param>
            /// <returns>the Combatant</returns>
            public Combatant MakeAlien(Race race)
            {
                return Xenocide.StaticTables.CombatantFactory.MakeAlien(race, Rank);
            }

            #region fields

            /// <summary>The rank</summary>
            public AlienRank Rank;

            /// <summary>Minimim number at this rank (on easy level)</summary>
            public int MinEasy;

            /// <summary>Minimim number at this rank (on hard level)</summary>
            public int MinHard;

            /// <summary>Maximum additional at this rank</summary>
            public int Extra;

            /// <summary>Maximum possible number of aliens of this rank</summary>
            public int MaxAliens { get { return Math.Max(MinEasy, MinHard) + Extra; } }

            #endregion fields
        }

        #region fields

        /// <summary>How many aliens to create (at each rank)</summary>
        private List<CrewEntry> crewList = new List<CrewEntry>();

        #endregion
    }
}
