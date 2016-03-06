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
* @file GameOptions.cs
* @date Created: 2009/10/19
* @author File creator: John Perrin
* @author Credits: none
*/
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Text;

using Xenocide.Source.Utils;
using Microsoft.Xna.Framework.Storage;
using System.IO;
using System.Xml;
#endregion

namespace ProjectXenocide.Model
{
    /// <summary>
    /// This class holds a list of game options that are persisted when saving out of an options
    /// dialog, and are applied on launch of the game.
    /// </summary>
    /// <remarks>
    /// If the gameoptions file exists and is of the correct version then attempt to load it.
    /// At the moment this just stores sound option data, graphics options may be best off in a seperate file.
    /// At the moment version compatability is set to the assembly version, but for options data this might be too
    /// strict and could follow it's own versioning.
    /// </remarks>
    [Serializable]
    public class GameOptions
    {
        /// <summary>
        /// Apply these settings to the game
        /// </summary>
        public void Apply()
        {
            Xenocide.AudioSystem.MusicVolume = MusicVolume;
            Xenocide.AudioSystem.SoundVolume = SoundVolume;
        }

        /// <summary>
        /// Load options from file for this user
        /// </summary>
        /// <returns>the options, or default if file doesn't exist</returns>
        public static GameOptions LoadFromFile()
        {
            var gameOptions = new GameOptions();

            if (FileUtil.DoesFileExist(gameOptionsPathName))
            {
                using (StorageContainer container = FileUtil.GetContainer(gameOptionsPathName))
                using (var f = new FileStream(FileUtil.TruePathName(container, gameOptionsPathName), FileMode.Open))
                using (var r = new XmlTextReader(f))
                {
                    r.ReadStartElement("settings");
                    int oldVersion = 0;
                    ReadIntElement(  r, "GameVersion", ref oldVersion);
                    ReadFloatElement(r, "SoundVolume", ref gameOptions.soundVolume);
                    ReadFloatElement(r, "MusicVolume", ref gameOptions.musicVolume);
                }
            }
            return gameOptions;
        }

        /// <summary>
        /// Write options to this user's options file
        /// </summary>
        public void SaveToFile()
        {
            try
            {
                using (StorageContainer container = FileUtil.GetContainer(gameOptionsPathName))
                using (var f = new FileStream(FileUtil.TruePathName(container, gameOptionsPathName), FileMode.CreateNew))
                using (var w = new XmlTextWriter(f, Encoding.UTF8))
                {
                    w.WriteStartElement("settings");
                    WriteElement(w, "GameVersion", gameVersion);
                    WriteElement(w, "SoundVolume", soundVolume);
                    WriteElement(w, "MusicVolume", musicVolume);
                    w.WriteEndElement();
                }
            }
            catch
            {
                //Util.ShowMessageBox(Strings.MSGBOX_UNABLE_TO_SAVE_FILE, err.Message);
                //TODO - It's not the end of the world if you can't save game options but it would
                //be nice to save to a log file for details of the error.      
            }
        }

        /// <summary>
        /// Write an element to the XML file
        /// </summary>
        /// <typeparam name="T">type of content</typeparam>
        /// <param name="r">the XML writer</param>
        /// <param name="name">name of element</param>
        /// <param name="val">content of element</param>
        private static void WriteElement<T>(XmlTextWriter r, string name, T val)
        {
            r.WriteStartElement(name);
            r.WriteValue(val);
            r.WriteEndElement();
        }

        /// <summary>
        /// Read an integer element from XML source
        /// </summary>
        /// <param name="r">the source</param>
        /// <param name="elementName">Name of element</param>
        /// <param name="val">where to put the value</param>
        private static void ReadIntElement(XmlTextReader r, string elementName, ref int val)
        {
            if (r.IsStartElement(elementName))
            {
                r.ReadStartElement(elementName);
                val = r.ReadContentAsInt();
                r.ReadEndElement();
            }
        }

        /// <summary>
        /// Read a float element from XML source
        /// </summary>
        /// <param name="r">the source</param>
        /// <param name="elementName">Name of element</param>
        /// <param name="val">where to put the value</param>
        private static void ReadFloatElement(XmlTextReader r, string elementName, ref float val)
        {
            if (r.IsStartElement(elementName))
            {
                r.ReadStartElement(elementName);
                val = r.ReadContentAsFloat();
                r.ReadEndElement();
            }
        }

        /// <summary>
        /// Persisted sound volume, defaulted to max
        /// </summary>
        private float soundVolume = 1.0f;
        public float SoundVolume
        {
            get { return soundVolume; }
            set { soundVolume = value; }
        }

        /// <summary>
        /// Persisted Game Volume, defaulted to max
        /// </summary>
        private float musicVolume = 1.0f;
        public float MusicVolume
        {
            get { return musicVolume; }
            set { musicVolume = value; }
        }

        /// <summary>
        /// Persisted Game Volume, defaulted to max
        /// </summary>
        private int gameVersion = CurrentVersion;
        public int GameVersion
        {
            get { return gameVersion; }
            set { gameVersion = value; }
        }

        /// <summary>
        /// Where to save the options data
        /// </summary>
        private const string gameOptionsPathName = @".\XeNAcide\Options\GameOptions.xml";

        /// <summary>
        /// Version number of this structure (to allow backwards compatibility)
        /// </summary>
        private const int CurrentVersion = 0;
    }
}
