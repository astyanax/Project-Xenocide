using System;
using System.Collections.Generic;
using System.IO;

using NLog;

using ProjectXenocide.Model;
using ProjectXenocide.Utils;
using Xenocide.Utils;

using Xenocide.Resources;

namespace ProjectXenocide.UI.Screens
{
    public partial class LoadSaveGameScreen
    {
        /// <summary>
        /// Handles all game logic for save/load operations: file I/O, validation,
        /// and save directory management.
        /// </summary>
        /// <remarks>
        /// ARCHITECTURE: This controller owns all file system operations for saving
        /// and loading games. The Screen class delegates to this controller for
        /// business logic and updates GUI elements based on results.
        ///
        /// GAME MECHANICS:
        /// - Save files are stored in LocalApplicationData/Xenocide/saves/
        /// - Save files contain a header with real-time and game-time info
        /// - Duplicate save names are not allowed (overwrite prevention)
        /// - Load validates file format and version compatibility
        /// </remarks>
        private class SaveFileController
        {
            private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

            private readonly string savesDirectory;

            public SaveFileController()
            {
                savesDirectory = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "Xenocide", "saves");
            }

            /// <summary>
            /// Gets the list of save files in the saves directory.
            /// </summary>
            /// <returns>Collection of filenames (not full paths)</returns>
            public ICollection<string> GetSaveFiles()
            {
                if (Directory.Exists(savesDirectory))
                {
                    return Directory.GetFiles(savesDirectory);
                }
                return Array.Empty<string>();
            }

            /// <summary>
            /// Reads the header information from a save file.
            /// </summary>
            /// <param name="filename">Name of the save file</param>
            /// <returns>Header info, or null if file doesn't exist or is invalid</returns>
            public GameStateSerializer.SaveFileHeader ReadSaveHeader(string filename)
            {
                string path = Path.Combine(savesDirectory, filename);
                if (!File.Exists(path))
                    return null;

                using (FileStream stream = File.Open(path, FileMode.Open))
                {
                    stream.Position = 0;
                    return GameStateSerializer.ReadHeader(stream);
                }
            }

            /// <summary>
            /// Checks if a save file with the given name already exists.
            /// </summary>
            public bool SaveGameExists(string filename)
            {
                return File.Exists(Path.Combine(savesDirectory, filename));
            }

            /// <summary>
            /// Saves the current game state to a file.
            /// </summary>
            /// <param name="saveName">Name for the save file</param>
            /// <returns>True if save was successful</returns>
            public bool TrySaveGame(string saveName)
            {
                try
                {
                    if (!Directory.Exists(savesDirectory))
                        Directory.CreateDirectory(savesDirectory);

                    string filename = Path.Combine(savesDirectory, saveName);
                    using (FileStream stream = File.Create(filename))
                    {
                        GameStateSerializer.Save(stream, Xenocide.GameState, Xenocide.CurrentVersion);
                    }
                    return true;
                }
                catch (Exception e)
                {
                    Logger.Error(e, "Save failed");
                    Util.ShowMessageBox(Strings.MSGBOX_UNABLE_TO_SAVE_FILE, e.Message);
                    return false;
                }
            }

            /// <summary>
            /// Loads a game state from a file.
            /// </summary>
            /// <param name="filename">Name of the save file to load</param>
            /// <returns>Loaded GameState, or null on failure</returns>
            public GameState TryLoadGame(string filename)
            {
                if (string.IsNullOrEmpty(filename))
                {
                    Util.ShowMessageBox("Please enter a filename to load.");
                    return null;
                }

                string path = Path.Combine(savesDirectory, filename);
                if (!File.Exists(path))
                {
                    Util.ShowMessageBox($"No save file found named '{filename}'.");
                    return null;
                }

                try
                {
                    using (FileStream stream = File.Open(path, FileMode.Open))
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
                catch (Exception e)
                {
                    Logger.Error(e, "Load failed");
                    Util.ShowMessageBox($"Failed to load save file: {e.Message}");
                    return null;
                }
            }

            /// <summary>
            /// Deletes a save file.
            /// </summary>
            /// <param name="filename">Name of the file to delete</param>
            /// <returns>True if file was deleted</returns>
            public bool TryDeleteSave(string filename)
            {
                string path = Path.Combine(savesDirectory, filename);
                if (File.Exists(path))
                {
                    File.Delete(path);
                    return true;
                }
                return false;
            }

            /// <summary>
            /// Gets the full path to the saves directory.
            /// </summary>
            public string SavesDirectory => savesDirectory;
        }
    }
}
