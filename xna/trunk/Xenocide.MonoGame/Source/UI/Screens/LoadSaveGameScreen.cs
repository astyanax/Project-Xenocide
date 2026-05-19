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
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Threading;


using Microsoft.Xna.Framework;



using CeGui;


using ProjectXenocide.UI.Dialogs;
using ProjectXenocide.Model;
using ProjectXenocide.Model.Geoscape;
using ProjectXenocide.Utils;
using Xenocide.Resources;
using Xenocide.Utils;


#endregion

namespace ProjectXenocide.UI.Screens
{
    /// <summary>
    /// Screen for Saving game to file, and loading game from a file
    /// </summary>
    public class LoadSaveGameScreen : Screen
    {
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
        }

        #region Create the CeGui widgets

        /// <summary>
        /// create the widgets we're going to show on the screen
        /// </summary>
        protected override void CreateCeguiWidgets()
        {
            // initializeEditBox
            filenameEditBox = GuiBuilder.CreateEditBox("editBox");
            AddWidget(filenameEditBox, 0.05f, 0.84f, 0.9f, 0.07f);

            // The list of saved games
            InitializeGrid();

            // and the buttons
            deleteButton = AddButton("BUTTON_DELETE", 0.36f, 0.92f, 0.25f, 0.07f);
            cancelButton = AddButton("BUTTON_CANCEL", 0.66f, 0.92f, 0.25f, 0.07f);

            deleteButton.Clicked += new GuiEventHandler(OnDeleteGame);
            cancelButton.Clicked += new GuiEventHandler(OnCloseScreen);

            // save/load button depends on mode
            if (mode == Mode.Save)
            {
                saveButton = AddButton("BUTTON_SAVE", 0.07f, 0.92f, 0.25f, 0.07f);
                saveButton.Clicked += new GuiEventHandler(OnSaveGame);
            }
            else
            {
                saveButton = AddButton("BUTTON_LOAD", 0.07f, 0.92f, 0.25f, 0.07f);
                saveButton.Clicked += new GuiEventHandler(OnLoadGame);
            }
        }

        /// <summary>
        /// Creates a MultiColumnListBox (will hold the name of the saved games)
        /// </summary>
        private void InitializeGrid()
        {
            savesgrid = GuiBuilder.CreateGrid("savesgrid");
            AddWidget(savesgrid, 0.01f, 0.08f, 0.98f, 0.75f);
            AddColumnHeader("Name", 0.4f);
            AddColumnHeader("Real Time", 0.295f);
            AddColumnHeader("Game Time", 0.295f);

            AddSaveGamesToGrid();

            savesgrid.SelectionChanged += new WindowEventHandler(OnGridSelectionChanged);
        }

        /// <summary>
        /// Add a column to the supplied grid of saved games
        /// </summary>
        /// <param name="title">Name to give the column</param>
        /// <param name="width">Width to make the column (relative to grid's width)</param>
        private void AddColumnHeader(String title, float width)
        {
            savesgrid.AddColumn(title, savesgrid.ColumnCount, width);
        }

        private void AddRowToGrid(String NameCol, String RealTimeCol, String GameTimeCol, int rowNum)
        {
            savesgrid.InsertRow(Util.CreateListboxItem(NameCol), 0, rowNum);
            Util.AddStringElementToGrid(savesgrid, 1, rowNum, RealTimeCol);
            Util.AddStringElementToGrid(savesgrid, 2, rowNum, GameTimeCol);
        }

        private CeGui.Widgets.MultiColumnList savesgrid;
        private CeGui.Widgets.EditBox filenameEditBox;
        private CeGui.Widgets.PushButton saveButton;
        private CeGui.Widgets.PushButton deleteButton;
        private CeGui.Widgets.PushButton cancelButton;

        #endregion Create the CeGui widgets

        #region event handlers

        /// <summary>Write the game state to named file</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void OnSaveGame(object sender, GuiEventArgs e)
        {
            String saveName = filenameEditBox.Text;
            if (SaveGameExists(saveName))
            {
                Util.ShowMessageBox(Strings.SCREEN_LOADSAVEGAME_DUPLICATE_FILENAME);
            }
            else
            {
                // if able to save to file, update the grid
                if (WriteToFile(saveName))
                {
                    AddSaveGameToGrid(saveName);
                }
            }
        }

        /// <summary>Load the seleted game</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:DisposeObjectsBeforeLosingScope",
           Justification = "FxCop False Positive")]
        private void OnLoadGame(object sender, GuiEventArgs e)
        {
            CeGui.Widgets.ListboxItem item = savesgrid.GetFirstSelectedItem();
            if (item != null)
            {
                // load the file
                GameState game = ReadFromFile(item.Text);
                if (null != game)
                {
                    Xenocide.GameState = game;

                    // We always restart with time suspended
                    Xenocide.GameState.GeoData.GeoTime.StopTime();

                    // resume the game (may be either geoscape or battlescape)
                    ScreenManager.ScheduleScreen(new GeoscapeScreen());
                }
            }
        }

        /// <summary>Restore screen that was present before Save/Load game screen</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void OnCloseScreen(object sender, GuiEventArgs e)
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
        private void OnDeleteGame(object sender, GuiEventArgs e)
        {
            // delete the currently selected save game
            CeGui.Widgets.ListboxItem item = savesgrid.GetFirstSelectedItem();
            if (item != null)
            {
                //ToDo: we should pop up a messagebox to confirm user really does want to delete the savegame

                DeleteSaveGameFile(item.Text);

                // Now remove the savegame from the screen's grid (and edit box)
                savesgrid.RemoveRow(savesgrid.GetRowIndexOfItem(item));
                filenameEditBox.Text = String.Empty;
            }
        }

        /// <summary>Handles user clicking on an item in the grid</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void OnGridSelectionChanged(object sender, WindowEventArgs e)
        {
            CeGui.Widgets.ListboxItem item = savesgrid.GetFirstSelectedItem();
            if (item != null)
            {
                Xenocide.AudioSystem.PlaySound("Menu\\buttonclick2_changesetting.ogg");
                filenameEditBox.Text = item.Text;
            }
        }

        #endregion event handlers

        #region File manipulation routines

        /// <summary>
        /// Populate the grid with the existing saved games
        /// </summary>
        private void AddSaveGamesToGrid()
        {
            string saveDir = GetSaveDirectory();
            if (Directory.Exists(saveDir))
            {
                ICollection<string> FileList = Directory.GetFiles(saveDir);
                foreach (string filename in FileList)
                {
                    using (FileStream stream = File.Open(filename, FileMode.Open))
                    {
                        AddSaveGameToGrid(stream, Path.GetFileName(filename));
                    }
                }
            }
        }

        /// <summary>
        /// Add this save game to the grid of saved games
        /// </summary>
        /// <param name="stream">stream holding save game</param>
        /// <param name="filename">Name of the file</param>
        private void AddSaveGameToGrid(FileStream stream, String filename)
        {
            stream.Position = 0;
            GameStateSerializer.SaveFileHeader header = GameStateSerializer.ReadHeader(stream);
            if (header != null)
            {
                AddRowToGrid(
                    filename,
                    header.RealTime,
                    header.GameTime,
                    0);
            }
        }

        /// <summary>
        /// Add this save game to the grid of saved games
        /// </summary>
        /// <param name="filename">filename of saved game</param>
        private void AddSaveGameToGrid(string filename)
        {
            string saveDir = GetSaveDirectory();
            using (FileStream stream = File.Open(Path.Combine(saveDir, filename), FileMode.Open))
            {
                AddSaveGameToGrid(stream, filename);
            }
        }

        /// <summary>
        /// Write the current game state to file
        /// </summary>
        /// <param name="saveName">Name of file to save game as</param>
        /// <returns>true if successful</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes",
            Justification = "Guideline is wrong in this case")]
        private bool WriteToFile(string saveName)
        {
            try
            {
                string saveDir = GetSaveDirectory();
                string filename = Path.Combine(saveDir, saveName);
                using (FileStream stream = File.Create(filename))
                {
                    GameStateSerializer.Save(stream, Xenocide.GameState, Xenocide.CurrentVersion);
                }
                return true;
            }
            catch (Exception e)
            {
                Util.ShowMessageBox(Strings.MSGBOX_UNABLE_TO_SAVE_FILE, e.Message);
                return false;
            }
        }

        /// <summary>
        /// Retrieve a GameState from file
        /// </summary>
        /// <param name="filename">Name of file holding save game</param>
        /// <returns>GameState to set game to</returns>
        private GameState ReadFromFile(string filename)
        {
            string saveDir = GetSaveDirectory();
            using (FileStream stream = File.Open(Path.Combine(saveDir, filename), FileMode.Open))
            {
                string error;
                GameState gameState = GameStateSerializer.Load(stream, Xenocide.CurrentVersion, out error);
                if (gameState != null)
                {
                    return gameState;
                }
                else
                {
                    Util.ShowMessageBox(Strings.SCREEN_LOADSAVEGAME_VERSION_CONFLICT);
                    return null;
                }
            }
        }

        /// <summary>
        /// Does a SaveGame with this (file)name already exist?
        /// </summary>
        /// <param name="filename">filename to check</param>
        /// <returns>true if it exists</returns>
        private bool SaveGameExists(string filename)
        {
            string saveDir = GetSaveDirectory();
            return File.Exists(Path.Combine(saveDir, filename));
        }

        /// <summary>
        /// Delete the specified file (presumed to be a save game)
        /// </summary>
        /// <param name="filename">Name of file to delete</param>
        private void DeleteSaveGameFile(string filename)
        {
            string saveDir = GetSaveDirectory();
            string path = Path.Combine(saveDir, filename);
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        /// <summary>
        /// Get the container (directory) holding the saved files
        /// </summary>
        /// <returns>the container</returns>
        private string GetSaveDirectory()
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, savesDirectory);
        }

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

        private string savesDirectory = ".\\XeNAcide\\saves";
    }
}
