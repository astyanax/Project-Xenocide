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
* @author File creator: dteviot
* @author Credits: none
*/
#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Diagnostics;
using System.Threading;


using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Storage;

using CeGui;

using Xenocide.Resources;
using Xenocide.UI.Dialogs;
using Xenocide.Model;
using Xenocide.Model.Geoscape;
using Xenocide.Utils;

#endregion

namespace Xenocide.UI.Screens
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
        /// <param name="screenManager">the screen manager</param>
        /// <param name="cancelScreen">Screen to go to if cancel is pressed</param>
        public LoadSaveGameScreen(Game game, Mode mode, Screen cancelScreen)
            : base(game, "LoadSaveGameScreen")
        {
            this.mode         = mode;
            this.cancelScreen = cancelScreen;
        }

        public override void Initialize()
        {
            gameStateService = (IGameStateService)Game.Services.GetService(typeof(IGameStateService));
            base.Initialize();
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
            savesgrid.SetGridItem(1, rowNum, Util.CreateListboxItem(RealTimeCol));
            savesgrid.SetGridItem(2, rowNum, Util.CreateListboxItem(GameTimeCol));
        }

        private CeGui.Widgets.MultiColumnList savesgrid;
        private CeGui.Widgets.EditBox         filenameEditBox;
        private CeGui.Widgets.PushButton      saveButton;
        private CeGui.Widgets.PushButton      deleteButton;
        private CeGui.Widgets.PushButton      cancelButton;

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
                ScreenManager.ShowDialog(
                    new MessageBoxDialog(Game, Strings.SCREEN_LOADSAVEGAME_DUPLICATE_FILENAME)
                );
            }
            else
            {
                gameStateService.SaveGame(saveName);
                AddSaveGameToGrid(saveName);
            }
        }

        /// <summary>Load the seleted game</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void OnLoadGame(object sender, GuiEventArgs e)
        {
            CeGui.Widgets.ListboxItem item = savesgrid.GetFirstSelectedItem();
            if (item != null)
            {
                // load the file
                gameStateService.LoadGame(item.Text);

                // resume the game (may be either geoscape or battlescape)
                ScreenManager.ScheduleScreen(new GeoscapeScreen(Game));
            }
        }

        /// <summary>Restore screen that was present before Save/Load game screen</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void OnCloseScreen(object sender, GuiEventArgs e)
        {
            ScreenManager.ScheduleScreen(cancelScreen);
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
            using (StorageContainer container = GetContainer())
            {
                ICollection<string> FileList = Directory.GetFiles(container.Path);
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
            SaveGameHeader header = gameStateService.ReadHeader(stream);
            AddRowToGrid(
                filename,
                header.realTime,
                header.gameTime,
                0);
        }

        /// <summary>
        /// Add this save game to the grid of saved games
        /// </summary>
        /// <param name="filename">filename of saved game</param>
        private void AddSaveGameToGrid(string filename)
        {
            using (StorageContainer container = GetContainer())
            {
                using (FileStream stream = File.Open(Path.Combine(container.Path, filename), FileMode.Open))
                {
                    AddSaveGameToGrid(stream, filename);
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
            using (StorageContainer container = GetContainer())
            {
                return File.Exists(Path.Combine(container.Path, filename));
            }
        }

        /// <summary>
        /// Delete the specified file (presumed to be a save game)
        /// </summary>
        /// <param name="filename">Name of file to delete</param>
        private void DeleteSaveGameFile(string filename)
        {
            using (StorageContainer container = GetContainer())
            {
                string path = Path.Combine(container.Path, filename);
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
        }

        /// <summary>
        /// Get the container (directory) holding the saved files
        /// </summary>
        /// <returns>the container</returns>
        private StorageContainer GetContainer()
        {
            // this bit is dummy on windows
            IAsyncResult result = StorageDevice.BeginShowStorageDeviceGuide(PlayerIndex.One, null, null);
            StorageDevice device = StorageDevice.EndShowStorageDeviceGuide(result);

            // Now open container(directory)
            return device.OpenContainer(savesDirectory);
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
        private Screen cancelScreen;

        private string savesDirectory = ".\\XeNAcide\\saves";

        private IGameStateService gameStateService;
    }
}
