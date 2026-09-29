using System;
using System.Collections.Generic;

using NLog;

using ProjectXenocide.Model;
using ProjectXenocide.Services;
using ProjectXenocide.Utils;

using Xenocide.Resources;
using Xenocide.Utils;

namespace ProjectXenocide.UI.Screens
{
    public partial class LoadSaveGameScreen
    {
        /// <summary>
        /// Handles game logic for save/load operations, delegating file I/O to
        /// <see cref="SavegameService"/>.
        /// </summary>
        /// <remarks>
        /// GAME MECHANICS:
        /// - Save files live in LocalApplicationData/Xenocide/saves/ with a .xsv extension.
        /// - Listings are sorted newest-first; only .xsv files are considered.
        /// - Duplicate names prompt the player to overwrite.
        /// - Load validates file format and version compatibility.
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic",
            Justification = "Stateless facade kept as instance methods so the screen's call sites stay unchanged.")]
        private class SaveFileController
        {
            private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

            /// <summary>Save file names (with extension), newest first.</summary>
            public string[] GetSaveFiles()
            {
                var names = new List<string>();
                foreach (SaveFileInfo info in SavegameService.List())
                {
                    names.Add(info.FileName);
                }
                return names.ToArray();
            }

            public GameStateSerializer.SaveFileHeader ReadSaveHeader(string filename)
                => SavegameService.ReadHeader(filename);

            public bool SaveGameExists(string filename) => SavegameService.Exists(filename);

            /// <summary>Saves the current game state under the given name.</summary>
            public bool TrySaveGame(string saveName)
            {
                try
                {
                    if (SavegameService.Save(Xenocide.GameState, saveName))
                    {
                        return true;
                    }
                }
                catch (Exception e)
                {
                    Logger.Error(e, "Save failed");
                }

                Util.ShowMessageBox(Strings.MSGBOX_UNABLE_TO_SAVE_FILE, "see the log for details");
                return false;
            }

            /// <summary>Loads a game state from the named file.</summary>
            public GameState TryLoadGame(string filename)
            {
                if (string.IsNullOrEmpty(filename))
                {
                    Util.ShowMessageBox("Please select a save to load.");
                    return null;
                }

                GameState game = SavegameService.Load(filename, out string error);
                if (game == null)
                {
                    Util.ShowMessageBox(error ?? Strings.SCREEN_LOADSAVEGAME_VERSION_CONFLICT);
                }
                return game;
            }

            public bool TryDeleteSave(string filename) => SavegameService.Delete(filename);

            public string SavesDirectory => SavegameService.SavesDirectory;

            /// <summary>A sensible, unique default name based on the in-game date.</summary>
            public string GenerateDefaultName() => SavegameService.GenerateDefaultName();

            /// <summary>Remove the save extension for display in the name box.</summary>
            public static string StripExtension(string name) => SavegameService.StripExtension(name);
        }
    }
}
