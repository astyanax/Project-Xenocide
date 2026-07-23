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

using Gum.Forms;
using Gum.Forms.Controls;

using Microsoft.Xna.Framework;

using MonoGameGum;

using NLog;

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
    /// <summary>
    /// Main menu screen providing access to new game, load game, settings, and credits.
    /// </summary>
    /// <remarks>
    /// ARCHITECTURE: Entry point screen for the application. Initializes new game state
    /// and provides navigation to other screens. Debug-only buttons provide direct access
    /// to subsystem testing (battlescape, aeroscape, xnet).
    /// 
    /// SCREEN FLOW:
    /// - StartScreen → GeoscapeScreen (new game)
    /// - StartScreen → LoadSaveGameScreen (load game)
    /// - StartScreen → SettingsScreen (settings)
    /// - StartScreen → CreditsScreen (credits)
    /// - StartScreen → QuitGame (exit application)
    /// </remarks>
    public class StartScreen : GumScreen
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public StartScreen()
            : base("StartScreen", @"Content/Textures/UI/StartScreenBackground.png")
        {
            if (Xenocide.AudioSystem != null)
                Xenocide.AudioSystem.PlayRandomMusic("MainMenu");
        }

        protected override void CreateGumControls()
        {
            Xenocide.DebugTesting = false;

            Xenocide.GameState = new GameState();
            Xenocide.GameState.SetToStartGameCondition();
            Utils.MessageLog.LoadFromGameState();

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
                WireButton("SettingsButton", OnSettingsClicked);

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

            var settingsButton = new Button();
            settingsButton.Text = "Settings";
            settingsButton.Click += OnSettingsClicked;
            RootContainer.AddChild(settingsButton);

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

        private void OnSettingsClicked(object sender, EventArgs e)
        {
            Logger.Debug("StartScreen: Settings clicked");
            ScreenManager.ScheduleScreen(new SettingsScreen());
        }

        private void OnRunTestsClicked(object sender, EventArgs e)
        {
            Logger.Debug("StartScreen: Run Tests clicked");
            Xenocide.DebugTesting = true;
            Xenocide.GameState.SetToStartGameCondition();
            Xenocide.StaticTables.StartSettings.Cheats.XcorpCantLooseAtStartOfMonth = true;

            var failures = new List<string>();

            // Replace trace listeners so Debug.Assert failures are captured
            // instead of terminating the process
            var savedListeners = new System.Diagnostics.TraceListener[global::System.Diagnostics.Trace.Listeners.Count];
            global::System.Diagnostics.Trace.Listeners.CopyTo(savedListeners, 0);
            global::System.Diagnostics.Trace.Listeners.Clear();
            global::System.Diagnostics.Trace.Listeners.Add(new AssertFailureLogger(failures));

            try
            {
                RunTest("Planet.RunTests", () => ProjectXenocide.Model.Geoscape.Geography.Planet.RunTests(), failures);
                RunTest("Mission.RunTests", () => ProjectXenocide.Model.Battlescape.Mission.RunTests(), failures);
                RunTest("Combatant.RunTests", () => ProjectXenocide.Model.Battlescape.Combatants.Combatant.RunTests(), failures);
                RunTest("Trajectory.RunTests", () => ProjectXenocide.Model.Battlescape.Trajectory.RunTests(), failures);
                RunTest("Terrain.RunTests", () => ProjectXenocide.Model.Battlescape.Terrain.RunTests(), failures);
                RunTest("ShootOrder.RunTests", () => ProjectXenocide.Model.Battlescape.Combatants.ShootOrder.RunTests(), failures);
                RunTest("MoveOrder.RunTests", () => ProjectXenocide.Model.Battlescape.Combatants.MoveOrder.RunTests(), failures);
                RunTest("CrewBuilder.RunTests", () => ProjectXenocide.Model.Battlescape.CrewBuilder.RunTests(), failures);
                RunTest("Pathfinder.RunTests", () => ProjectXenocide.Model.Battlescape.Pathfinder.RunTests(), failures);
                RunTest("CombatantFactory.RunTests", () => ProjectXenocide.Model.StaticData.Battlescape.CombatantFactory.RunTests(), failures);
                RunTest("Armor.RunTests", () => ProjectXenocide.Model.StaticData.Battlescape.Armor.RunTests(), failures);
                RunTest("Item.RunItemTests", () => ProjectXenocide.Model.StaticData.Items.Item.RunItemTests(), failures);
                RunTest("CombatantInventory.RunTests", () => ProjectXenocide.Model.Battlescape.Combatants.CombatantInventory.RunTests(), failures);
                RunTest("ResearchGraph.RunTests", () => ProjectXenocide.Model.StaticData.Research.ResearchGraph.RunTests(), failures);
                RunTest("BuildProjectManager.RunTests", () => ProjectXenocide.Model.Geoscape.BuildProjectManager.RunTests(), failures);
                RunTest("ResearchProjectManager.RunTests", () => ProjectXenocide.Model.Geoscape.ResearchProjectManager.RunTests(), failures);
                RunTest("RetaliationTask.RunTests", () => ProjectXenocide.Model.Geoscape.AI.RetaliationTask.RunTests(), failures);
                RunTest("BuildOutpostTask.RunTests", () => ProjectXenocide.Model.Geoscape.AI.BuildOutpostTask.RunTests(), failures);
                RunTest("SupplyOutpostTask.RunTests", () => ProjectXenocide.Model.Geoscape.AI.SupplyOutpostTask.RunTests(), failures);
                RunTest("TerrorTask.RunTests", () => ProjectXenocide.Model.Geoscape.AI.TerrorTask.RunTests(), failures);
                RunTest("InfiltrationTask.RunTests", () => ProjectXenocide.Model.Geoscape.AI.InfiltrationTask.RunTests(), failures);
                RunTest("GeoBitmap.RunTests", () => ProjectXenocide.Model.Geoscape.Geography.GeoBitmap.RunTests(), failures);
                RunTest("GeoPosition.RunTests", () => ProjectXenocide.Model.Geoscape.GeoPosition.RunTests(), failures);
                RunTest("Scheduler.RunTests", () => ProjectXenocide.Model.Scheduler.RunTests(), failures);
                RunTest("AttackAlienSiteMission.RunTests", () => ProjectXenocide.Model.Geoscape.Vehicles.AttackAlienSiteMission.RunTests(), failures);
                RunTest("OutpostStatistics.RunTests", () => ProjectXenocide.Model.Geoscape.Outposts.OutpostStatistics.RunTests(), failures);
                RunTest("Floorplan.RunTests", () => ProjectXenocide.Model.Geoscape.Outposts.Floorplan.RunTests(), failures);
                RunTest("OutpostInventory.RunTests", () => ProjectXenocide.Model.Geoscape.Outposts.OutpostInventory.RunTests(), failures);
                RunTest("Ufo.RunTests", () => ProjectXenocide.Model.Geoscape.Vehicles.Ufo.RunTests(), failures);
                RunTest("Aircraft.RunTests", () => ProjectXenocide.Model.Geoscape.Vehicles.Aircraft.RunTests(), failures);
                RunTest("ScoreLog.RunTests", () => ProjectXenocide.Model.ScoreLog.RunTests(), failures);
            }
            finally
            {
                global::System.Diagnostics.Trace.Listeners.Clear();
                foreach (var l in savedListeners)
                    global::System.Diagnostics.Trace.Listeners.Add(l);
            }

            if (failures.Count > 0)
            {
                var sb = new StringBuilder();
                sb.Append("Unit tests completed with ");
                sb.Append(failures.Count);
                sb.AppendLine(" failure(s):");
                sb.AppendLine();
                foreach (var f in failures)
                    sb.AppendLine("• " + f);
                Util.ShowMessageBox(sb.ToString().TrimEnd());
            }
            else
            {
                Util.ShowMessageBox("All unit tests passed");
            }
        }

        private static void RunTest(string name, Action test, List<string> failures)
        {
            int before = failures.Count;
            try
            {
                test();
            }
            catch (Exception ex)
            {
                failures.Add($"{name}: {ex.GetType().Name}: {ex.Message}");
            }

            // Prefix any Debug.Assert failures collected during this test
            for (int i = before; i < failures.Count; i++)
            {
                failures[i] = $"{name}: {failures[i]}";
            }
        }

        /// <summary>
        /// TraceListener that collects Debug.Assert failures into a list
        /// instead of terminating the process.
        /// </summary>
        private sealed class AssertFailureLogger : System.Diagnostics.TraceListener
        {
            private readonly List<string> _failures;

            public AssertFailureLogger(List<string> failures)
            {
                _failures = failures;
            }

            public override void Write(string message) { }

            public override void WriteLine(string message) { }

            public override void Fail(string message)
            {
                _failures.Add(message);
            }

            public override void Fail(string message, string detailMessage)
            {
                _failures.Add(message + Environment.NewLine + detailMessage);
            }
        }

        private void OnBattlescapeClicked(object sender, EventArgs e)
        {
            Logger.Debug("StartScreen: Battlescape clicked");
            Xenocide.DebugTesting = true;
            StartDebugBattlescape();
        }

        private void OnNewGameClicked(object sender, EventArgs e)
        {
            Logger.Debug("StartScreen: New Game clicked");
            Xenocide.GameState.SetToStartGameCondition();
            GeoscapeScreen geoscapeScreen = new GeoscapeScreen();
            geoscapeScreen.State = new GeoscapeScreen.AddingFirstBaseScreenState(geoscapeScreen);
            ScreenManager.ScheduleScreen(geoscapeScreen);
        }

        private void OnShowLoadGameScreen(object sender, EventArgs e)
        {
            Logger.Debug("StartScreen: Load Game clicked");
            ScreenManager.ScheduleScreen(
                new LoadSaveGameScreen(
                    LoadSaveGameScreen.Mode.Load,
                    LoadSaveGameScreen.CancelScreen.Start
                )
            );
        }

        private void OnQuitGameClicked(object sender, EventArgs e)
        {
            Logger.Debug("StartScreen: Quit clicked");
            ScreenManager.QuitGame = true;
        }

        private void OnCreditsClicked(object sender, EventArgs e)
        {
            Logger.Debug("StartScreen: Credits clicked");
            ShowCreditsScreen();
        }

#if DEBUG
        private void OnXNetDebugClicked(object sender, EventArgs e)
        {
            Xenocide.DebugTesting = true;
            Logger.Debug("StartScreen: Debug XNet clicked");
            Xenocide.GameState.SetToStartGameCondition();
            ScreenManager.ScheduleScreen(new XNetScreen());
        }

        private void OnAeroscapeDebugClicked(object sender, EventArgs e)
        {
            Xenocide.DebugTesting = true;
            Logger.Debug("StartScreen: Debug Aeroscape clicked");
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

            // Pick a random armed UFO type (any except Probe which has no weapon)
            string[] armedUfoTypes = {
                "ITEM_UFO_RECON", "ITEM_UFO_ESCORT", "ITEM_UFO_REAPER",
                "ITEM_UFO_COLLECTOR", "ITEM_UFO_INTIMIDATOR", "ITEM_UFO_JUGGERNAUT",
                "ITEM_UFO_ALIEN_FREIGHTER"
            };
            string ufoType = armedUfoTypes[Xenocide.Rng.Next(armedUfoTypes.Length)];
            ufo.DebugTransmute(Xenocide.StaticTables.ItemList[ufoType]);
            ufo.DebugRearm();

            Aircraft aircraft = outpost.Fleet[0] as Aircraft;
            ScreenManager.ScheduleScreen(new AeroscapeScreen(aircraft, ufo));
        }
#endif

        #endregion event handlers

        private static void ShowCreditsScreen()
        {
            Logger.Debug("StartScreen: Scheduling CreditsScreen");
            ScreenManager.ScheduleScreen(new CreditsScreen());
        }

        private static void StartDebugBattlescape()
        {
            Logger.Debug("StartScreen: Starting debug battlescape");
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
