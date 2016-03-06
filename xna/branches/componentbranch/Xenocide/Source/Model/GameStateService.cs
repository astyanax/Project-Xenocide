using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Storage;
using System.IO;
using Microsoft.Xna.Framework;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Diagnostics;
using Xenocide.Model.Geoscape;

namespace Xenocide.Model
{
    public interface IGameStateService
    {
        void NewGame();
        void LoadGame(string saveName);

        /// <summary>
        /// Write the current game state to file
        /// </summary>
        /// <param name="saveName">Name of file to save game as</param>
        /// <returns>true if successful</returns>
        void SaveGame(string saveName);

        SaveGameHeader ReadHeader(FileStream stream);
    }

    /// <summary>
    /// The "header" information in a save game file
    /// </summary>
    [Serializable]
    public class SaveGameHeader
    {
        // data members
        public string realTime;
        public string gameTime;

        // format a time for display in the load/save dialog
        /// <param name="time">time to format</param>
        /// <returns>the formatted time</returns>
        public static string FormatTime(DateTime time)
        {
            return time.ToString("yyyy-MM-dd HH:mm:ss", Thread.CurrentThread.CurrentCulture);
        }

        /// <summary>
        /// Write the specified string to a file
        /// </summary>
        /// <param name="fs">stream to write to</param>
        /// <param name="value">string to write to the file</param>
        /// <remarks>Used to write header to file</remarks>
        /// <remarks>Had to write this myself because StreamWriter closes stream when it's done</remarks>
        public static void WriteString(Stream fs, String value)
        {
            byte[] info = new UTF8Encoding(true).GetBytes(value);
            Debug.Assert(info.Length < 256);
            fs.WriteByte((byte)info.Length);
            fs.Write(info, 0, info.Length);
        }

        /// <summary>
        /// Read a string from a file at the current position
        /// </summary>
        /// <param name="fs">file to read from</param>
        /// <returns>string read</returns>
        /// <remarks>Used to read header from file</remarks>
        /// <remarks>Had to write this myself because StreamReader closes stream when it's done</remarks>
        public static String ReadString(Stream fs)
        {
            int count = fs.ReadByte();
            byte[] b = new byte[count];
            fs.Read(b, 0, count);
            UTF8Encoding temp = new UTF8Encoding(true);
            return temp.GetString(b);
        }
    }

    public class NoGameStateAvaiableException : Exception
    {
        public NoGameStateAvaiableException(string msg)
            : base(msg)
        {
        }
    }

    public class GameStateService : GameComponent, IGameStateService
    {
        private GameState gameState;
        private const string savesDirectory = ".\\XeNAcide\\saves";


        public GameStateService(Game game)
            : base(game)
        {
            game.Services.AddService(typeof(IGameStateService), this);
        }

        public void NewGame()
        {
            DisposeGameState();
            gameState = new GameState();
            gameState.Game = Game;
        }

        public void LoadGame(string saveName)
        {
            GameState newGameState = ReadFromFile(saveName);
            DisposeGameState();
            gameState = newGameState;
            gameState.Game = Game;

            //always start with stopped time
            ((IGeoTimeService)Game.Services.GetService(typeof(IGeoTimeService))).StopTime();
        }

        /// <summary>
        /// Write the current game state to file
        /// </summary>
        /// <param name="saveName">Name of file to save game as</param>
        /// <returns>true if successful</returns>
        public void SaveGame(string saveName)
        {
            if (gameState == null)
            {
                throw new NoGameStateAvaiableException("No GameState available");
            }

            using (StorageContainer container = GetContainer())
            {
                string filename = Path.Combine(container.Path, saveName);
                using (FileStream stream = File.Create(filename))
                {
                    WriteHeader(stream);
                    WriteGameState(stream);
                }
            }
        }

        /// <summary>
        /// Retrieve a GameState from file
        /// </summary>
        /// <param name="filename">Name of file holding save game</param>
        /// <returns>GameState to set game to</returns>
        private GameState ReadFromFile(string filename)
        {
            using (StorageContainer container = GetContainer())
            {
                using (FileStream stream = File.Open(Path.Combine(container.Path, filename), FileMode.Open))
                {
                    // skip the header
                    ReadHeader(stream);

                    // get the game state
                    BinaryFormatter formatter = new BinaryFormatter();
                    return (GameState)formatter.Deserialize(stream);
                }
            }
        }

        /// <summary>
        /// Read a save game header from a stream
        /// </summary>
        /// <param name="stream">Stream to read the header from</param>
        public SaveGameHeader ReadHeader(FileStream stream)
        {
            SaveGameHeader header = new SaveGameHeader();
            header.realTime = SaveGameHeader.ReadString(stream);
            header.gameTime = SaveGameHeader.ReadString(stream);
            return header;
        }

        private void DisposeGameState()
        {
            if (gameState != null)
                gameState.Dispose();
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

        /// <summary>
        /// Write a save game header to a stream
        /// </summary>
        /// <param name="stream">Stream to write the header to</param>
        private void WriteHeader(FileStream stream)
        {
            SaveGameHeader.WriteString(stream, SaveGameHeader.FormatTime(DateTime.Now));
            DateTime gameTime = ((IGeoTimeService)Game.Services.GetService(typeof(IGeoTimeService))).Time;
            SaveGameHeader.WriteString(stream, SaveGameHeader.FormatTime(gameTime));
        }

        /// <summary>
        /// Write a the game state to a stream
        /// </summary>
        /// <param name="stream">Stream to write the game state to</param>
        private void WriteGameState(FileStream stream)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream, gameState);
        }
    }
}
