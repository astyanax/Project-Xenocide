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
* @file Battlescape/UnitTestMission.cs
* @date Created: 2008/03/21
* @author File creator: David Teviotdale
* @author Credits: none
*/
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

using Microsoft.Xna.Framework;


using ProjectXenocide.Utils;
using ProjectXenocide.Model.StaticData;
using ProjectXenocide.Model.StaticData.Items;
using ProjectXenocide.Model.StaticData.Battlescape;
using ProjectXenocide.Model.Geoscape;
using ProjectXenocide.Model.Geoscape.Outposts;
using ProjectXenocide.Model.Geoscape.Vehicles;
using ProjectXenocide.Model.Geoscape.AI;
using ProjectXenocide.Model.Battlescape.Combatants;

namespace ProjectXenocide.Model.Battlescape
{
    /// <summary>
    /// Code to test the Mission class.
    /// </summary>
    public abstract partial class Mission
    {
        #region UnitTests

        /// <summary>
        /// Run set of tests
        /// </summary>
        [Conditional("DEBUG")]
        public static void RunTests()
        {
            TestXCorpVictory();
            TestXCorpDefeat();
        }

        /// <summary>Setup a mission and battlescape</summary>
        [Conditional("DEBUG")]
        public static void Setup()
        {
            Xenocide.GameState.SetToStartGameCondition();

            // set up outpost for mission
            GeoPosition pos = new GeoPosition();
            Outpost outpost = new Outpost(pos, "Dummy");
            outpost.SetupPlayersFirstBase();
            Xenocide.GameState.GeoData.Outposts.Add(outpost);

            // Launch a UFO
            Overmind overmind = Xenocide.GameState.GeoData.Overmind;
            overmind.DiableStartOfMonth();
            overmind.DebugCreateMission(AlienMission.Retaliation, pos);
            RetaliationTask task = overmind.Tasks[0] as RetaliationTask;
            InvasionTask.TestReleaseUfo(task);
            Ufo ufo = overmind.Ufos[0];

            // change UFO into a bigger one (so we've got a couple of aliens)
            ufo.DebugTransmute(Xenocide.StaticTables.ItemList["ITEM_UFO_RECON"]);

            ProjectXenocide.Model.Battlescape.Mission battlescapeMission = new UfoSiteMission(ufo, outpost.Fleet[2]);
            Xenocide.GameState.Battlescape = new Battle(battlescapeMission);
        }

        /// <summary>Kill or stun all members of a team</summary>
        /// <param name="combatants">Team to stun and kill</param>
        [Conditional("DEBUG")]
        public static void DebugKillAndStunTeam(IList<Combatant> combatants)
        {
            for (int i = 0; i < combatants.Count; ++i)
            {
                Combatant combatant = combatants[i];
                if (0 == (i % 2))
                {
                    combatant.Hit(new DamageInfo(5000, DamageType.Acid), new Vector3(1, 0, 0));
                }
                else
                {
                    combatant.Hit(new DamageInfo(5000, DamageType.Stun), new Vector3(1, 0, 0));
                }
            }
        }

        /// <summary>
        /// Test Handling of a mission X-Corp wins()
        /// </summary>
        [Conditional("DEBUG")]
        public static void TestXCorpVictory()
        {
            Setup();
            Battle battlescape = Xenocide.GameState.Battlescape;

            // Kill or stun the aliens
            DebugKillAndStunTeam(battlescape.Teams[Team.Aliens].Combatants);

            Mission mission = battlescape.Mission;
            mission.OnFinish(battlescape, BattleFinish.XCorpVictory);
            battlescape.PostMissionCleanup();
            Xenocide.GameState.Battlescape = null;
        }

        /// <summary>
        /// Test Handling of a mission X-Corp wins()
        /// </summary>
        [Conditional("DEBUG")]
        public static void TestXCorpDefeat()
        {
            Xenocide.GameState.SetToStartGameCondition();

            // set up outpost for mission
            GeoPosition pos = new GeoPosition();
            Outpost outpost = new Outpost(pos, "Dummy");
            outpost.SetupPlayersFirstBase();
            Xenocide.GameState.GeoData.Outposts.Add(outpost);

            // Launch a UFO
            Overmind overmind = Xenocide.GameState.GeoData.Overmind;
            overmind.DiableStartOfMonth();
            overmind.DebugCreateMission(AlienMission.Retaliation, pos);
            RetaliationTask task = overmind.Tasks[0] as RetaliationTask;
            InvasionTask.TestReleaseUfo(task);
            Ufo ufo = overmind.Ufos[0];

            // change UFO into a bigger one (so we've got a couple of aliens)
            ufo.DebugTransmute(Xenocide.StaticTables.ItemList["ITEM_UFO_RECON"]);

            ProjectXenocide.Model.Battlescape.Mission battlescapeMission = new XCorpOutpostMission(ufo, outpost);
            Xenocide.GameState.Battlescape = new Battle(battlescapeMission);

            // Kill or stun the humans
            Battle battlescape = Xenocide.GameState.Battlescape;
            DebugKillAndStunTeam(battlescape.Teams[Team.XCorp].Combatants);

            Mission mission = battlescape.Mission;
            mission.OnFinish(battlescape, BattleFinish.AlienVictory);
            battlescape.PostMissionCleanup();
            Xenocide.GameState.Battlescape = null;
        }

        #endregion UnitTests
    }
}
