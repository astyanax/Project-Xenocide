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

using Gum.Forms;
using Gum.Forms.Controls;

using Microsoft.Xna.Framework;

using MonoGameGum;

using ProjectXenocide.Model;
using ProjectXenocide.Model.Battlescape;
using ProjectXenocide.Model.Battlescape.Combatants;
using ProjectXenocide.Model.Geoscape;
using ProjectXenocide.Model.Geoscape.AI;
using ProjectXenocide.Model.Geoscape.Outposts;
using ProjectXenocide.Model.Geoscape.Vehicles;
using ProjectXenocide.Model.StaticData.Items;
using ProjectXenocide.Utils;

#endregion Using Statements

namespace ProjectXenocide.UI.Screens
{
    public class StartScreen : GumScreen
    {
        public StartScreen()
            : base("StartScreen", @"Content/Textures/UI/StartScreenBackground.png")
        {
            if (Xenocide.AudioSystem != null)
                Xenocide.AudioSystem.PlayRandomMusic("MainMenu");
        }

        protected override void CreateGumControls()
        {
            Xenocide.DebugTesting = false;

            if (GumRoot != null)
            {
#if DEBUG
                WireButton("RunTestsButton", OnRunTestsClicked);
                WireButton("BattlescapeButton", OnBattlescapeClicked);
                WireButton("XNetDebugButton", OnXNetDebugClicked);
                WireButton("AeroscapeDebugButton", OnAeroscapeDebugClicked);
#endif
                WireButton("NewGameButton", OnNewGameClicked);
                WireButton("LoadGameButton", OnShowLoadGameScreen);
                WireButton("QuitButton", OnQuitGameClicked);
                WireButton("CreditsButton", OnCreditsClicked);

                return;
            }

            RootContainer.Width = 250;

#if DEBUG
            var testsButton = new Button();
            testsButton.Text = "Run Tests";
            testsButton.Click += OnRunTestsClicked;
            RootContainer.AddChild(testsButton);

            var battlescapeButton = new Button();
            battlescapeButton.Text = "Debug Battlescape";
            battlescapeButton.Click += OnBattlescapeClicked;
            RootContainer.AddChild(battlescapeButton);

            var xnetDebugButton = new Button();
            xnetDebugButton.Text = "Debug XNet";
            xnetDebugButton.Click += OnXNetDebugClicked;
            RootContainer.AddChild(xnetDebugButton);

            var aeroscapeDebugButton = new Button();
            aeroscapeDebugButton.Text = "Debug Aeroscape";
            aeroscapeDebugButton.Click += OnAeroscapeDebugClicked;
            RootContainer.AddChild(aeroscapeDebugButton);
#endif

            var startButton = new Button();
            startButton.Text = "New Game";
            startButton.Click += OnNewGameClicked;
            RootContainer.AddChild(startButton);

            var loadButton = new Button();
            loadButton.Text = "Load Saved Game";
            loadButton.Click += OnShowLoadGameScreen;
            RootContainer.AddChild(loadButton);

            var quitButton = new Button();
            quitButton.Text = "Quit";
            quitButton.Click += OnQuitGameClicked;
            RootContainer.AddChild(quitButton);

            var creditsButton = new Button();
            creditsButton.Text = "Credits";
            creditsButton.Click += OnCreditsClicked;
            RootContainer.AddChild(creditsButton);

            var spacer = new Label();
            spacer.Height = 20;
            RootContainer.AddChild(spacer);

            var versionLabel = new Label();
            versionLabel.Text = Xenocide.CurrentVersion;
            RootContainer.AddChild(versionLabel);
        }

        #region event handlers

        public override bool HandleEscape()
        {
            return true;
        }

        private void OnRunTestsClicked(object sender, EventArgs e)
        {
            Console.WriteLine("StartScreen: Run Tests clicked");
            Xenocide.DebugTesting = true;
            Xenocide.GameState.SetToStartGameCondition();
            Xenocide.StaticTables.StartSettings.Cheats.XcorpCantLooseAtStartOfMonth = true;
            ProjectXenocide.Model.Geoscape.Geography.Planet.RunTests();
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

        private void OnBattlescapeClicked(object sender, EventArgs e)
        {
            Console.WriteLine("StartScreen: Battlescape clicked");
            Xenocide.DebugTesting = true;
            StartDebugBattlescape();
        }

        private void OnNewGameClicked(object sender, EventArgs e)
        {
            Console.WriteLine("StartScreen: New Game clicked");
            Xenocide.GameState.SetToStartGameCondition();
            GeoscapeScreen geoscapeScreen = new GeoscapeScreen();
            geoscapeScreen.State = new GeoscapeScreen.AddingFirstBaseScreenState(geoscapeScreen);
            ScreenManager.ScheduleScreen(geoscapeScreen);
        }

        private void OnShowLoadGameScreen(object sender, EventArgs e)
        {
            Console.WriteLine("StartScreen: Load Game clicked");
            ScreenManager.ScheduleScreen(
                new LoadSaveGameScreen(
                    LoadSaveGameScreen.Mode.Load,
                    LoadSaveGameScreen.CancelScreen.Start
                )
            );
        }

        private void OnQuitGameClicked(object sender, EventArgs e)
        {
            Console.WriteLine("StartScreen: Quit clicked");
            ScreenManager.QuitGame = true;
        }

        private void OnCreditsClicked(object sender, EventArgs e)
        {
            Console.WriteLine("StartScreen: Credits clicked");
            ShowCreditsScreen();
        }

#if DEBUG
        private void OnXNetDebugClicked(object sender, EventArgs e)
        {
            Xenocide.DebugTesting = true;
            Console.WriteLine("StartScreen: Debug XNet clicked");
            Xenocide.GameState.SetToStartGameCondition();
            ScreenManager.ScheduleScreen(new XNetScreen());
        }

        private void OnAeroscapeDebugClicked(object sender, EventArgs e)
        {
            Xenocide.DebugTesting = true;
            Console.WriteLine("StartScreen: Debug Aeroscape clicked");
            Xenocide.GameState.SetToStartGameCondition();

            GeoPosition pos = new GeoPosition();
            Outpost outpost = new Outpost(pos, "Dummy");
            outpost.SetupPlayersFirstBase();
            Xenocide.GameState.GeoData.Outposts.Add(outpost);

            Overmind overmind = Xenocide.GameState.GeoData.Overmind;
            overmind.DiableStartOfMonth();
            overmind.DebugCreateMission(AlienMission.Retaliation, pos);
            RetaliationTask task = overmind.Tasks[0] as RetaliationTask;
            InvasionTask.TestReleaseUfo(task);
            Ufo ufo = overmind.Ufos[0];
            ufo.DebugTransmute(Xenocide.StaticTables.ItemList["ITEM_UFO_RECON"]);

            Aircraft aircraft = outpost.Fleet[2] as Aircraft;
            ScreenManager.ScheduleScreen(new AeroscapeScreen(aircraft, ufo));
        }
#endif

        #endregion event handlers

        private void ShowCreditsScreen()
        {
            Console.WriteLine("StartScreen: Scheduling CreditsScreen");
            ScreenManager.ScheduleScreen(new CreditsScreen());
        }

        private void StartDebugBattlescape()
        {
            Console.WriteLine("StartScreen: Starting debug battlescape");
            Xenocide.GameState.SetToStartGameCondition();

            GeoPosition pos = new GeoPosition();
            Outpost outpost = new Outpost(pos, "Dummy");
            outpost.SetupPlayersFirstBase();
            Xenocide.GameState.GeoData.Outposts.Add(outpost);

            Overmind overmind = Xenocide.GameState.GeoData.Overmind;
            overmind.DiableStartOfMonth();
            overmind.DebugCreateMission(AlienMission.Retaliation, pos);
            RetaliationTask task = overmind.Tasks[0] as RetaliationTask;
            InvasionTask.TestReleaseUfo(task);
            Ufo ufo = overmind.Ufos[0];

            ufo.DebugTransmute(Xenocide.StaticTables.ItemList["ITEM_UFO_RECON"]);

            ProjectXenocide.Model.Battlescape.Mission battlescapeMission = new UfoSiteMission(ufo, outpost.Fleet[2]);
            Xenocide.GameState.Battlescape = new Battle(battlescapeMission);
            ScreenManager.ScheduleScreen(new BattlescapeScreen());
        }
    }
}
