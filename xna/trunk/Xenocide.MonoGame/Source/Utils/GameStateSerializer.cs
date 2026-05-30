using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;

using ProjectXenocide.Model;

namespace Xenocide.Utils
{
    public static class GameStateSerializer
    {
        public const int CurrentFormatVersion = 1;

        private static readonly JsonSerializerOptions Options = new()
        {
            IncludeFields = true,
            WriteIndented = false,
            AllowTrailingCommas = false,
            ReadCommentHandling = JsonCommentHandling.Skip,
            Converters =
            {
                new ModelJsonConverter(),
                new Vector3DictionaryConverterFactory()
            }
        };

        public static JsonSerializerOptions SerializerOptions => Options;

        public static void Save(Stream stream, GameState state, string gameVersion)
        {
            var saveFile = new SaveFileWrapper
            {
                FormatVersion = CurrentFormatVersion,
                SavedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", Thread.CurrentThread.CurrentCulture),
                GameTime = FormatGameTime(state),
                GameVersion = gameVersion,
                GameState = state
            };

            JsonSerializer.Serialize(stream, saveFile, Options);
        }

        public static GameState Load(Stream stream, string gameVersion, out string errorMessage)
        {
            errorMessage = null;
            try
            {
                var saveFile = JsonSerializer.Deserialize<SaveFileWrapper>(stream, Options);
                if (saveFile == null)
                {
                    errorMessage = "Save file is empty or corrupt.";
                    return null;
                }

                if (saveFile.FormatVersion > CurrentFormatVersion)
                {
                    errorMessage = "Save file was created by a newer version of the game.";
                    return null;
                }

                return saveFile.GameState;
            }
            catch (JsonException ex)
            {
                errorMessage = $"Failed to parse save file: {ex.Message}";
                return null;
            }
        }

        public static SaveFileHeader ReadHeader(Stream stream)
        {
            try
            {
                var saveFile = JsonSerializer.Deserialize<SaveFileWrapper>(stream, Options);
                if (saveFile == null) return null;

                return new SaveFileHeader
                {
                    RealTime = saveFile.SavedAt,
                    GameTime = saveFile.GameTime,
                    AssemblyVersion = saveFile.GameVersion
                };
            }
            catch
            {
                return null;
            }
        }

        private static string FormatGameTime(GameState state)
        {
            try
            {
                return state.GeoData.GeoTime.Time.ToString("yyyy-MM-dd HH:mm:ss", Thread.CurrentThread.CurrentCulture);
            }
            catch
            {
                return "Unknown";
            }
        }

        public class SaveFileHeader
        {
            public string RealTime { get; set; }
            public string GameTime { get; set; }
            public string AssemblyVersion { get; set; }
        }

        [Serializable]
        private sealed class SaveFileWrapper
        {
            public int FormatVersion { get; set; }
            public string SavedAt { get; set; }
            public string GameTime { get; set; }
            public string GameVersion { get; set; }
            public GameState GameState { get; set; }
        }
    }
}
