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
* @file StartScreen.cs
* @date Created: 2007/01/20
* @author File creator: David Teviotdale
* @author Credits: none
*/
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;

using ProjectXenocide.Model;
using ProjectXenocide.Utils;
using ProjectXenocide.Model.Geoscape;
using ProjectXenocide.Model.Geoscape.Vehicles;
using ProjectXenocide.Model.Geoscape.Outposts;
using ProjectXenocide.Model.Geoscape.AI;
using ProjectXenocide.Model.Battlescape;
using ProjectXenocide.Model.Battlescape.Combatants;
using ProjectXenocide.Model.StaticData.Items;

#endregion Using Statements

namespace ProjectXenocide.UI.Screens
{
    /// <summary>
    /// This is the first screen shown when Xenocide starts
    /// </summary>
    public class StartScreen : Screen
    {
        /// <summary>
        /// Default constructor (obviously)
        /// </summary>
        public StartScreen()
            : base("StartScreen", @"Content\Textures\UI\StartScreenBackground.png")
        {
            Xenocide.AudioSystem.PlayRandomMusic("");
        }

        #region Create the CeGui widgets

        /// <summary>
        /// add the buttons to the screen
        /// </summary>
        protected override void CreateCeguiWidgets()
        {
            // put Xenocide version in bottom right corner
            AddStaticText(0.90f, 0.940f, 0.10f, 0.04f).Text = Xenocide.CurrentVersion;

            // Only put up these buttons if debug build
            #if DEBUG
                testsButton                = AddButton("BUTTON_RUN_TESTS",         0.3500f, 0.75f, 0.3000f, 0.04f);
                battlescapeButton          = AddButton("BUTTON_DEBUG_BATTLESCAPE", 0.0000f, 0.75f, 0.3000f, 0.04f);
                testsButton.Clicked       += new CeGui.GuiEventHandler(OnRunTestsClicked);
                battlescapeButton.Clicked += new CeGui.GuiEventHandler(OnBattlescapeClicked);
            #endif

            startButton   = AddButton("BUTTON_NEW_GAME",        0.3500f, 0.80f, 0.3000f, 0.04f);
            loadButton    = AddButton("BUTTON_LOAD_SAVED_GAME", 0.3500f, 0.85f, 0.3000f, 0.04f);
            quitButton    = AddButton("BUTTON_QUIT",            0.3500f, 0.90f, 0.3000f, 0.04f, "Menu\\exitgame.ogg");
            creditsButton = AddButton("BUTTON_CREDITS",         0.3500f, 0.95f, 0.3000f, 0.04f);

            startButton.Clicked   += new CeGui.GuiEventHandler(OnNewGameClicked); 
            loadButton.Clicked    += new CeGui.GuiEventHandler(OnShowLoadGameScreen);
            quitButton.Clicked    += new CeGui.GuiEventHandler(OnQuitGameClicked);
            creditsButton.Clicked += new CeGui.GuiEventHandler(OnCreditsClicked);
        }

        private CeGui.Widgets.PushButton testsButton;
        private CeGui.Widgets.PushButton battlescapeButton;
        private CeGui.Widgets.PushButton startButton;
        private CeGui.Widgets.PushButton loadButton;
        private CeGui.Widgets.PushButton quitButton;
        private CeGui.Widgets.PushButton creditsButton;

        #endregion Create the CeGui widgets

        #region event handlers

        /// <summary>Run the unit Tests</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void OnRunTestsClicked(object sender, CeGui.GuiEventArgs e)
        {
            Xenocide.GameState.SetToStartGameCondition();
            Xenocide.StaticTables.StartSettings.Cheats.XcorpCantLooseAtStartOfMonth = true;
            ProjectXenocide.Model.Battlescape.Mission.RunTests();
            ProjectXenocide.Model.Battlescape.Combatants.Combatant.RunTests();
            ProjectXenocide.Model.Battlescape.Trajectory.RunTests();
            ProjectXenocide.Model.Battlescape.Terrain.RunTests();
            ProjectXenocide.Model.Battlescape.Combatants.ShootOrder.RunTests();
            ProjectXenocide.Model.Battlescape.Combatants.MoveOrder.RunTests();
            ProjectXenocide.Model.Battlescape.CrewBuilder.RunTests();
            ProjectXenocide.Model.Battlescape.Pathfinder.RunTests();
            ProjectXenocide.Model.StaticData.Battlescape.CombatantFactory.RunTests();
            ProjectXenocide.Model.StaticData.Battlescape.Armor.RunTests();
            ProjectXenocide.Model.StaticData.Items.Item.RunItemTests();
            ProjectXenocide.Model.Battlescape.Combatants.CombatantInventory.RunTests();
            ProjectXenocide.Model.StaticData.Research.ResearchGraph.RunTests();
            ProjectXenocide.Model.Geoscape.BuildProjectManager.RunTests();
            ProjectXenocide.Model.Geoscape.ResearchProjectManager.RunTests();
            ProjectXenocide.Model.Geoscape.AI.RetaliationTask.RunTests();
            ProjectXenocide.Model.Geoscape.AI.BuildOutpostTask.RunTests();
            ProjectXenocide.Model.Geoscape.AI.SupplyOutpostTask.RunTests();
            ProjectXenocide.Model.Geoscape.Geography.Planet.RunTests();
            ProjectXenocide.Model.Geoscape.AI.TerrorTask.RunTests();
            ProjectXenocide.Model.Geoscape.AI.InfiltrationTask.RunTests();
            ProjectXenocide.Model.Geoscape.Geography.GeoBitmap.RunTests();
            ProjectXenocide.Model.Geoscape.GeoPosition.RunTests();
            ProjectXenocide.Model.Scheduler.RunTests();
            ProjectXenocide.Model.Geoscape.Vehicles.AttackAlienSiteMission.RunTests();
            ProjectXenocide.Model.Geoscape.Outposts.OutpostStatistics.RunTests();
            ProjectXenocide.Model.Geoscape.Outposts.Floorplan.RunTests();
            ProjectXenocide.Model.Geoscape.Outposts.OutpostInventory.RunTests();
            ProjectXenocide.Model.Geoscape.Vehicles.Ufo.RunTests();
            ProjectXenocide.Model.Geoscape.Vehicles.Aircraft.RunTests();
            ProjectXenocide.Model.ScoreLog.RunTests();
            Util.ShowMessageBox("All unit tests passed");
        }

        /// <summary>Go straight to a Battlescape</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void OnBattlescapeClicked(object sender, CeGui.GuiEventArgs e)
        {
            StartDebugBattlescape();
        }

        /// <summary>Start a new game</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void OnNewGameClicked(object sender, CeGui.GuiEventArgs e)
        {
            Xenocide.GameState.SetToStartGameCondition();
            GeoscapeScreen geoscapeScreen = new GeoscapeScreen();
            geoscapeScreen.State = new GeoscapeScreen.AddingFirstBaseScreenState(geoscapeScreen);
            ScreenManager.ScheduleScreen(geoscapeScreen);
        }

        /// <summary>Replace screen on display with the Load Game Screen</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void OnShowLoadGameScreen(object sender, CeGui.GuiEventArgs e)
        {
            ScreenManager.ScheduleScreen(
                new LoadSaveGameScreen(
                    LoadSaveGameScreen.Mode.Load,
                    LoadSaveGameScreen.CancelScreen.Start
                )
            );
        }

        /// <summary>Quit the game</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void OnQuitGameClicked(object sender, CeGui.GuiEventArgs e)
        {
            ScreenManager.QuitGame = true;
        }

        /// <summary>Respond to user pressing the "Credits" button</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void OnCreditsClicked(object sender, CeGui.GuiEventArgs e)
        {
            ShowCreditsScreen();
        }

        #endregion event handlers

        /// <summary>Show the credits screen</summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:DisposeObjectsBeforeLosingScope",
            Justification="FxCop false positive")]
        private void ShowCreditsScreen()
        {
            ScreenManager.ScheduleScreen(new CreditsScreen());
        }

        /// <summary>Setup a battlescape and go to it</summary>
        private void StartDebugBattlescape()
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
            ScreenManager.ScheduleScreen(new BattlescapeScreen());

            // Kill or stun the aliens
            //Battle battlescape = Xenocide.GameState.Battlescape;
            //ProjectXenocide.Model.Battlescape.Mission.DebugKillAndStunTeam(battlescape.Teams[Team.Aliens].Combatants);

            // simple 3 level terrain for testing
            /*
            Terrain.TerrainBuilder builder = new Terrain.TestTerrainBuilder(TestTerrain.Barracks);
            ProjectXenocide.Model.Battlescape.Mission battlescapeMission = new MockMission(builder);
            Xenocide.GameState.Battlescape = new Battle(battlescapeMission);
            ScreenManager.ScheduleScreen(new BattlescapeScreen());
            */
        }
    }
}
