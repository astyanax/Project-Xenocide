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
using System.IO;
using System.Linq;
using System.Xml.Linq;

using NLog;

using Xenocide.Source.Utils;
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
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
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

            if (!FileUtil.DoesFileExist(gameOptionsPathName))
                return gameOptions;

            try
            {
                var root = XElement.Load(gameOptionsPathName);
                gameOptions.gameVersion = (int?)root.Element("GameVersion") ?? gameOptions.gameVersion;
                gameOptions.windowMode = (int?)root.Element("WindowMode") ?? gameOptions.windowMode;
                gameOptions.soundVolume = (float?)root.Element("SoundVolume") ?? gameOptions.soundVolume;
                gameOptions.musicVolume = (float?)root.Element("MusicVolume") ?? gameOptions.musicVolume;
                gameOptions.toastNotifications = (bool?)root.Element("ToastNotifications") ?? gameOptions.toastNotifications;
                gameOptions.pauseOnAlerts = (bool?)root.Element("PauseOnAlerts") ?? gameOptions.pauseOnAlerts;
                gameOptions.disabledNotifications = root.Elements("DisabledNotification")
                    .Select(e => (string)e)
                    .Where(s => !string.IsNullOrEmpty(s))
                    .ToList();
            }
            catch (Exception ex)
            {
                Logger.Warn(ex, "Unable to read game options; using defaults");
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
                // Ensure the options directory exists (it is not shipped), and
                // overwrite any previous file rather than throwing if it exists.
                var directory = Path.GetDirectoryName(gameOptionsPathName);
                if (!string.IsNullOrEmpty(directory))
                    Directory.CreateDirectory(directory);

                var root = new XElement("settings",
                    new XElement("GameVersion", gameVersion),
                    new XElement("WindowMode", windowMode),
                    new XElement("SoundVolume", soundVolume),
                    new XElement("MusicVolume", musicVolume),
                    new XElement("ToastNotifications", toastNotifications),
                    new XElement("PauseOnAlerts", pauseOnAlerts),
                    disabledNotifications.Select(id => new XElement("DisabledNotification", id)));

                root.Save(gameOptionsPathName);
            }
            catch (Exception ex)
            {
                // User-facing: settings silently not persisting is a real problem,
                // so log at Error (not Warn) to make it visible.
                Logger.Error(ex, "Unable to save game options");
            }
        }

        /// <summary>
        /// Persisted display mode (windowed / borderless / exclusive).
        /// </summary>
        private int windowMode;
        public WindowMode WindowMode
        {
            get { return (WindowMode)windowMode; }
            set { windowMode = (int)value; }
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
        /// Persisted master switch for toast notifications.
        /// </summary>
        private bool toastNotifications = true;
        public bool ToastNotifications
        {
            get { return toastNotifications; }
            set { toastNotifications = value; }
        }

        /// <summary>
        /// Persisted preference to pause geoscape time on alerts.
        /// </summary>
        private bool pauseOnAlerts = true;
        public bool PauseOnAlerts
        {
            get { return pauseOnAlerts; }
            set { pauseOnAlerts = value; }
        }

        /// <summary>
        /// Persisted set of notification event ids the player has switched off.
        /// </summary>
        private List<string> disabledNotifications = new List<string>();
        public IList<string> DisabledNotifications
        {
            get { return disabledNotifications; }
            set { disabledNotifications = value?.ToList() ?? new List<string>(); }
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
        private const string gameOptionsPathName = @"./XeNAcide/Options/GameOptions.xml";

        /// <summary>
        /// Version number of this structure (to allow backwards compatibility)
        /// </summary>
        private const int CurrentVersion = 0;
    }
}
