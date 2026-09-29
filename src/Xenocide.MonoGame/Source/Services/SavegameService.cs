using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

using NLog;

using ProjectXenocide.Model;

using Xenocide.Utils;

namespace ProjectXenocide.Services
{
    /// <summary>Name + header metadata for one save file.</summary>
    public sealed class SaveFileInfo
    {
        public string FileName { get; set; }
        public GameStateSerializer.SaveFileHeader Header { get; set; }
    }

    /// <summary>
    /// Central save-file store: naming (extension), listing/sorting, read/write
    /// and autosave slots.  Shared by the save/load screen and the autosave timer.
    /// </summary>
    public static class SavegameService
    {
        /// <summary>Extension used for all Xenocide save files.</summary>
        public const string Extension = ".xsv";

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>Directory save files live in (%LOCALAPPDATA%\Xenocide\saves).</summary>
        public static string SavesDirectory { get; } = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Xenocide", "saves");

        /// <summary>
        /// Normalise a user-supplied name into a safe file name with the save
        /// extension (strips any extension the user typed).
        /// </summary>
        public static string NormalizeName(string name)
        {
            name = (name ?? string.Empty).Trim();
            foreach (char invalid in Path.GetInvalidFileNameChars())
            {
                name = name.Replace(invalid, '_');
            }
            if (name.EndsWith(Extension, StringComparison.OrdinalIgnoreCase))
            {
                name = name.Substring(0, name.Length - Extension.Length);
            }
            if (string.IsNullOrEmpty(name))
            {
                name = "save";
            }
            return name + Extension;
        }

        /// <summary>Name without the save extension (for display/editing).</summary>
        public static string StripExtension(string name)
        {
            name ??= string.Empty;
            return name.EndsWith(Extension, StringComparison.OrdinalIgnoreCase)
                ? name.Substring(0, name.Length - Extension.Length)
                : name;
        }

        public static string GetPath(string name) => Path.Combine(SavesDirectory, NormalizeName(name));

        public static bool Exists(string name) => File.Exists(GetPath(name));

        /// <summary>
        /// All save files, newest first.  Only files with the save extension are
        /// considered; unreadable headers are skipped.
        /// </summary>
        public static IReadOnlyList<SaveFileInfo> List()
        {
            if (!Directory.Exists(SavesDirectory))
            {
                return Array.Empty<SaveFileInfo>();
            }

            var results = new List<SaveFileInfo>();
            foreach (string path in Directory.GetFiles(SavesDirectory, "*" + Extension))
            {
                using FileStream stream = File.Open(path, FileMode.Open, FileAccess.Read);
                GameStateSerializer.SaveFileHeader header = GameStateSerializer.ReadHeader(stream);
                if (header != null)
                {
                    results.Add(new SaveFileInfo { FileName = Path.GetFileName(path), Header = header });
                }
            }

            return results
                .OrderByDescending(info => info.Header.RealTime, StringComparer.Ordinal)
                .ToList();
        }

        public static GameStateSerializer.SaveFileHeader ReadHeader(string fileName)
        {
            string path = Path.Combine(SavesDirectory, fileName);
            if (!File.Exists(path))
            {
                return null;
            }
            using FileStream stream = File.Open(path, FileMode.Open, FileAccess.Read);
            return GameStateSerializer.ReadHeader(stream);
        }

        /// <summary>Write the game state. Returns false (and logs) on failure.</summary>
        public static bool Save(GameState state, string name)
        {
            try
            {
                Directory.CreateDirectory(SavesDirectory);
                using FileStream stream = File.Create(GetPath(name));
                GameStateSerializer.Save(stream, state, Xenocide.CurrentVersion);
                return true;
            }
            catch (Exception e)
            {
                Logger.Error(e, "Save failed for {0}", name);
                return false;
            }
        }

        public static GameState Load(string fileName, out string errorMessage)
        {
            errorMessage = null;
            string path = Path.Combine(SavesDirectory, fileName);
            if (!File.Exists(path))
            {
                errorMessage = $"No save file found named '{fileName}'.";
                return null;
            }

            try
            {
                using FileStream stream = File.Open(path, FileMode.Open, FileAccess.Read);
                return GameStateSerializer.Load(stream, Xenocide.CurrentVersion, out errorMessage);
            }
            catch (Exception e)
            {
                Logger.Error(e, "Load failed for {0}", fileName);
                errorMessage = e.Message;
                return null;
            }
        }

        public static bool Delete(string fileName)
        {
            string path = Path.Combine(SavesDirectory, fileName);
            if (!File.Exists(path))
            {
                return false;
            }
            File.Delete(path);
            return true;
        }

        /// <summary>
        /// A sensible default save name derived from the in-game date/time that
        /// does not collide with an existing file.
        /// </summary>
        public static string GenerateDefaultName()
        {
            DateTime time = Xenocide.GameState?.GeoData?.GeoTime?.Time ?? DateTime.Now;
            string baseName = time.ToString("yyyy-MM-dd HH-mm", CultureInfo.InvariantCulture);

            string candidate = NormalizeName(baseName);
            int suffix = 2;
            while (File.Exists(Path.Combine(SavesDirectory, candidate)))
            {
                candidate = NormalizeName($"{baseName} ({suffix++})");
            }
            return candidate;
        }

        /// <summary>Rotating autosave slot file name (1-based).</summary>
        public static string AutosaveName(int slot) => NormalizeName($"autosave-{slot}");
    }
}
