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
* @file LoadSaveGameScreen.cs
* @date Created: 2007/01/21
* @author File creator: David Teviotdale
* @author Credits: none
*/
#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;

using Gum.Forms;
using Gum.Forms.Controls;

using Microsoft.Xna.Framework;

using NLog;

using ProjectXenocide.Assets;
using ProjectXenocide.Model;
using ProjectXenocide.Model.Geoscape;
using ProjectXenocide.UI.Controls;
using ProjectXenocide.UI.Dialogs;
using ProjectXenocide.Utils;

using Xenocide.Resources;
using Xenocide.Utils;


#endregion

namespace ProjectXenocide.UI.Screens
{
    /// <summary>
    /// Screen for Saving game to file, and loading game from a file
    /// </summary>
    public partial class LoadSaveGameScreen : GumScreen
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="mode">Run in Load or Save mode</param>
        /// <param name="cancelScreen">Screen to go to if cancel is pressed</param>
        public LoadSaveGameScreen(Mode mode, CancelScreen cancelScreen)
            : base("LoadSaveGameScreen")
        {
            this.mode = mode;
            this.cancelScreen = cancelScreen;
            this.saveFileController = new SaveFileController();
        }

        #region Create the Gum controls

        /// <summary>
        /// create the widgets we're going to show on the screen
        /// </summary>
        protected override void CreateGumControls()
        {
            if (GumRoot != null)
            {
                WireButton("deleteButton", OnDeleteGame);
                WireButton("cancelButton", OnCloseScreen);

                if (mode == Mode.Save)
                {
                    var btn = WireButton("saveButton", OnSaveGame);
                    btn.Text = XenocideResourceManager.Get("BUTTON_SAVE");
                }
                else
                {
                    var btn = WireButton("saveButton", OnLoadGame);
                    btn.Text = XenocideResourceManager.Get("BUTTON_LOAD");
                }

                filenameEditBox = new TextBox();
                filenameEditBox.Visual.X = 20;
                filenameEditBox.Visual.Y = 20;
                filenameEditBox.Visual.Width = 300;
                AddChild(filenameEditBox);

                InitializeGrid();
                savesgrid.Visual.X = 20;
                savesgrid.Visual.Y = 60;
                savesgrid.Visual.Width = 800;
                return;
            }

            // initializeEditBox
            filenameEditBox = new TextBox();
            RootContainer.AddChild(filenameEditBox);

            // The list of saved games
            InitializeGrid();

            // and the buttons
            deleteButton = new Button() { Text = XenocideResourceManager.Get("BUTTON_DELETE") };
            RootContainer.AddChild(deleteButton);
            cancelButton = new Button() { Text = XenocideResourceManager.Get("BUTTON_CANCEL") };
            RootContainer.AddChild(cancelButton);

            deleteButton.Click += OnDeleteGame;
            cancelButton.Click += OnCloseScreen;

            // save/load button depends on mode
            if (mode == Mode.Save)
            {
                saveButton = new Button() { Text = XenocideResourceManager.Get("BUTTON_SAVE") };
                RootContainer.AddChild(saveButton);
                saveButton.Click += OnSaveGame;
            }
            else
            {
                saveButton = new Button() { Text = XenocideResourceManager.Get("BUTTON_LOAD") };
                RootContainer.AddChild(saveButton);
                saveButton.Click += OnLoadGame;
            }
        }

        /// <summary>
        /// Creates a GridPanel (will hold the name of the saved games)
        /// </summary>
        private void InitializeGrid()
        {
            savesgrid = new GridPanel();
            AddChild(savesgrid.Visual);
            savesgrid.AddColumn("Name", (int)(0.4f * 800));
            savesgrid.AddColumn("Real Time", (int)(0.295f * 800));
            savesgrid.AddColumn("Game Time", (int)(0.295f * 800));

            AddSaveGamesToGrid();

            savesgrid.SelectionChanged += OnGridSelectionChanged;
        }

        private void AddRowToGrid(String NameCol, String RealTimeCol, String GameTimeCol)
        {
            savesgrid.AddRow(NameCol, NameCol, RealTimeCol, GameTimeCol);
        }

        private GridPanel savesgrid;
        private TextBox filenameEditBox;
        private Button saveButton;
        private Button deleteButton;
        private Button cancelButton;

        #endregion Create the Gum controls

        #region event handlers

        /// <summary>Write the game state to named file</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void OnSaveGame(object sender, EventArgs e)
        {
            String saveName = filenameEditBox.Text;
            if (saveFileController.SaveGameExists(saveName))
            {
                Util.ShowMessageBox(Strings.SCREEN_LOADSAVEGAME_DUPLICATE_FILENAME);
            }
            else
            {
                if (saveFileController.TrySaveGame(saveName))
                {
                    AddSaveGameToGrid(saveName);
                    Util.ShowMessageBox("Game saved successfully.");
                    ScreenManager.ScheduleScreen(new GeoscapeScreen());
                }
            }
        }

        /// <summary>Load the seleted game</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void OnLoadGame(object sender, EventArgs e)
        {
            if (savesgrid.SelectedRow != null)
            {
                string filename = savesgrid.GetSelectedCellText();
                GameState game = saveFileController.TryLoadGame(filename);
                if (game != null)
                {
                    Xenocide.GameState = game;
                    Xenocide.GameState.GeoData.GeoTime.StopTime();
                    Util.ShowMessageBox("Game loaded successfully.");
                    ScreenManager.ScheduleScreen(new GeoscapeScreen());
                }
            }
        }

        /// <summary>Restore screen that was present before Save/Load game screen</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void OnCloseScreen(object sender, EventArgs e)
        {
            Screen nextScreen = null;
            switch (cancelScreen)
            {
                case CancelScreen.Geoscape:
                    nextScreen = new GeoscapeScreen();
                    break;

                case CancelScreen.Start:
                    nextScreen = new StartScreen();
                    break;

                case CancelScreen.Battlescape:
                    // ToDo implement
                    break;

                default:
                    // Should never get here
                    Debug.Assert(false);
                    break;
            }
            ScreenManager.ScheduleScreen(nextScreen);
        }

        /// <summary>delete the currently selected save file</summary>
        /// <param name="sender">Button that has been clicked</param>
        /// <param name="e">Not used</param>
        private void OnDeleteGame(object sender, EventArgs e)
        {
            if (savesgrid.SelectedRow != null)
            {
                string filename = savesgrid.GetSelectedCellText();
                saveFileController.TryDeleteSave(filename);

                savesgrid.RemoveRow(savesgrid.GetRowIndexByTag(filename));
                filenameEditBox.Text = String.Empty;
            }
        }

        /// <summary>Handles user clicking on an item in the grid</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void OnGridSelectionChanged(object sender, EventArgs e)
        {
            if (savesgrid.SelectedRow != null)
            {
                Xenocide.AudioSystem.PlaySound(SoundId.ButtonClick2);
                filenameEditBox.Text = savesgrid.GetSelectedCellText();
            }
        }

        #endregion event handlers

        #region File manipulation routines

        /// <summary>
        /// Populate the grid with the existing saved games
        /// </summary>
        private void AddSaveGamesToGrid()
        {
            foreach (string filename in saveFileController.GetSaveFiles())
            {
                string name = Path.GetFileName(filename);
                var header = saveFileController.ReadSaveHeader(name);
                if (header != null)
                {
                    AddRowToGrid(name, header.RealTime, header.GameTime);
                }
            }
        }

        /// <summary>
        /// Add this save game to the grid of saved games
        /// </summary>
        /// <param name="filename">filename of saved game</param>
        private void AddSaveGameToGrid(string filename)
        {
            var header = saveFileController.ReadSaveHeader(filename);
            if (header != null)
            {
                AddRowToGrid(filename, header.RealTime, header.GameTime);
            }
        }

        private SaveFileController saveFileController;

        #endregion File manipulation routines

        /// <summary>
        /// Screen can run in two modes, Load Game or Save Game
        /// </summary>
        public enum Mode
        {
            /// <summary>
            /// Run dialog in Save Game mode
            /// </summary>
            Save,

            /// <summary>
            /// Run dialog in Load Game mode
            /// </summary>
            Load
        }

        /// <summary>
        /// Is screen running as Save or Load?
        /// </summary>
        private Mode mode;

        /// <summary>
        /// Screen to go to if cancel is pressed
        /// </summary>
        public enum CancelScreen
        {
            /// <summary>
            /// Go to Geoscape screen
            /// </summary>
            Geoscape,

            /// <summary>
            /// Go to StartScreen
            /// </summary>
            Start,

            /// <summary>
            /// Go to Battlescape screen
            /// </summary>
            Battlescape
        }

        /// <summary>
        /// Screen to go to if cancel is pressed
        /// </summary>
        private CancelScreen cancelScreen;
    }
}
