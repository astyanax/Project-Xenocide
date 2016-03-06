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
* @file FmodGameComponent.cs
* @date Created: 2007/09/08
* @author File creator: David Teviotdale
* @author Credits: Martin Nordholts
*/
#endregion

#region Using Statements
using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Storage;
#endregion

namespace AudioSystem
{
    /// <summary>
    /// Wrap FMOD as a game component and game service
    /// </summary>
    public class FmodGameComponent : GameComponent, IAudioSystem
    {
        #region GameComponent

        /// <summary>
        /// Standard Constructor
        /// </summary>
        /// <param name="game"></param>
        public FmodGameComponent(Game game)
            : base(game)
        {
            // register self as a GameService
            game.Services.AddService(typeof(IAudioSystem), this);
        }


        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            // load FMOD
            Verify(FMOD.Factory.System_Create(ref fmodSystem));

            // check it's right version
            uint version = 0;
            Verify(fmodSystem.getVersion(ref version));
            if (version < FMOD.VERSION.number)
            {
                throw new ApplicationException("Invalid FMOD version");
            }

            Verify(fmodSystem.init(numSoundChannels, FMOD.INITFLAG.NORMAL, (IntPtr)null));

            sounds = new Dictionary<string, FMOD.Sound>();

            base.Initialize();
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            // if shuffle play is on, change song when current one finishes
            if (null != lastRandomDirectory)
            {
                PlayRandomMusic(lastRandomDirectory);
            }

            fmodSystem.update();
            base.Update(gameTime);
        }

        /// <summary>
        /// Implement Disposable pattern
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose (bool disposing)
        {
            foreach (FMOD.Sound sound in sounds.Values)
            {
                Verify(sound.release());
            }
            sounds.Clear();
            StopMusic();
            if (null != fmodSystem)
            {
                fmodSystem.release();
                fmodSystem = null;
            }
        }

        #endregion GameComponent

        #region IAudioEngine

        /// <summary>
        /// Load a sound off the disk, to play later.
        /// (to reduce load time)
        /// </summary>
        /// <param name="filename">Name of the file holding the sound</param>
        public void LoadSound(string filename)
        {
            FMOD.Sound sound = null;
            string pathname = SoundsDirectory + filename;
            if (VerifyFileExists(pathname))
            {
                Verify(fmodSystem.createSound(pathname, FMOD.MODE.DEFAULT, ref sound));
                sounds.Add(filename, sound);
            }
        }

        /// <summary>
        /// Play a sound
        /// </summary>
        /// <param name="filename">Name of the previously loaded file holding the music</param>
        public void PlaySound(string filename)
        {
            FMOD.Sound sound;
            if (sounds.TryGetValue(filename, out sound))
            {
                FMOD.Channel dummyChannel = new FMOD.Channel();
                Verify(fmodSystem.playSound(FMOD.CHANNELINDEX.FREE, sound, true, ref dummyChannel));
                Verify(dummyChannel.setVolume(soundVolume));
                Verify(dummyChannel.setPaused(false));
            }
            else if (!ignoreMissingAudioFiles)
            {
                throw new ApplicationException("Sound " + filename + " not loaded");
            }
        }

        /// <summary>
        /// Set a piece of music to be played
        /// </summary>
        /// <param name="filename">Name of the file holding the music</param>
        public void PlayMusic(string filename)
        {
            // if we're already playing this, nothing to do
            if (musicFilename != filename)
            {
                // if we're already playing music, stop it
                StopMusic();

                // play the new music (if the file exists)
                String path = MusicDirectory + filename;
                if (VerifyFileExists(path))
                {
                    FMOD.MODE mode = FMOD.MODE.SOFTWARE | FMOD.MODE.LOOP_OFF;
                    Verify(fmodSystem.createStream(path, mode, ref music));
                    Verify(fmodSystem.playSound(FMOD.CHANNELINDEX.FREE, music, true, ref musicChannel));
                    Verify(musicChannel.setVolume(musicVolume));
                    Verify(musicChannel.setPaused(false));
                    musicFilename = filename;
                    lastRandomDirectory = null;
                }
            }
        }

        /// <summary>
        /// Play a song, chosen at random from the directory
        /// </summary>
        /// <param name="directory">Directory to search for songs</param>
        public void PlayRandomMusic(string directory)
        {
            // if we've switched screens, stop music
            if (lastRandomDirectory != directory)
            {
                StopMusic();
            }

            if (!IsMusicPlaying())
            {
                // zero out, in case we can't start anything
                lastRandomDirectory = null;
                musicFilename       = String.Empty;

                // pick a song at random from directory and play it
                string path = MusicDirectory + directory;
                int rootDirLength = MusicDirectory.Length;
                DirectoryInfo dirInfo = new DirectoryInfo(path);
                if (dirInfo.Exists)
                {
                    FileInfo[] files = dirInfo.GetFiles();
                    List<string> names = new List<string>();
                    foreach (FileInfo file in files)
                    {
                        // note that we trim off the MusicDirectory part of name
                        // it will be added back later in PlayMusic
                        names.Add(file.FullName.Substring(rootDirLength));
                    }
                    if (0 < names.Count)
                    {
                        PlayRandomMusic(names);
                        lastRandomDirectory = directory;
                    }
                }
                else if (!ignoreMissingAudioFiles)
                {
                    throw new System.IO.DirectoryNotFoundException("Can't find directory: " + path);
                }
            }
        }

        /// <summary>
        /// Play a song, chosen at random from a list
        /// </summary>
        /// <param name="directory">the list of songs</param>
        public void PlayRandomMusic(IList<string> names)
        {
            Debug.Assert(0 < names.Count);
            // pick a title at random
            Random rng  = new Random();
            string name = names[rng.Next(names.Count)];

            // if we found a song, play it
            if (String.Empty != name)
            {
                PlayMusic(name);
            }
        }

        /// <summary>
        /// stop playing music
        /// </summary>
        /// <remarks>This function MUST NOT THROW, because it's called by Dispose</remarks>
        public void StopMusic()
        {
            musicFilename = String.Empty;
            if (null != musicChannel)
            {
                musicChannel.stop();
                musicChannel = null;
            }
            if (null != music)
            {
                music.release();
                music = null;
            }
        }

        /// <summary>
        /// Is background music currently playing?
        /// </summary>
        /// <returns>true if it's playing</returns>
        public bool IsMusicPlaying()
        {
            bool isPlaying = false;
            if (null != musicChannel)
            {
                if (FMOD.RESULT.OK != musicChannel.isPlaying(ref isPlaying))
                {
                    isPlaying = false;
                }
            }
            return isPlaying;
        }

        /// <summary>
        /// The volume sound will be played at (0 = off, 1 = max)
        /// </summary>
        public float SoundVolume 
        {
            get { return soundVolume; }
            set { Debug.Assert((0.0f <= value) && (value <= 1.0f)); soundVolume = value; }
        }

        /// <summary>
        /// The volume sound will be played at (0 = off, 1 = max)
        /// </summary>
        public float MusicVolume
        {
            get { return musicVolume; }
            set
            {
                Debug.Assert((0.0f <= value) && (value <= 1.0f));
                musicVolume = value;
                if (null != musicChannel)
                {
                    musicChannel.setVolume(musicVolume);
                }
            }
        }

        #endregion IAudioEngine

        /// <summary>
        /// Check the result from an FMOD function call, and throw if it's in error
        /// </summary>
        /// <param name="result">return value from an FMOD function to test</param>
        private static void Verify(FMOD.RESULT result)
        {
            if (FMOD.RESULT.OK != result)
            {
                throw new ApplicationException("FMOD error:" + FMOD.Error.String(result));
            }
        }

        /// <summary>
        /// Throw error if file doesn't exist (and we've got checking turned on
        /// </summary>
        /// <param name="pathname">full pathname of file to check</param>
        private bool VerifyFileExists(string pathname)
        {
            if (File.Exists(pathname))
            {
                return true;
            }

            if (!ignoreMissingAudioFiles)
            {
                throw new System.IO.FileNotFoundException("Audio file not found:" + pathname);
            }

            return false;
        }

        #region Fields

        /// <summary>
        /// Do we ignore missing sound and music files?
        /// </summary>
        public bool IgnoreMissingAudioFiles 
        {
            get { return ignoreMissingAudioFiles; } set { ignoreMissingAudioFiles = value; }
        }

        /// <summary>
        /// The wrapped FMOD engine
        /// </summary>
        private FMOD.System fmodSystem;

        /// <summary>
        /// Internal cache of sounds (gunshot, etc.)
        /// </summary>
        private Dictionary<string, FMOD.Sound> sounds;

        /// <summary>
        /// Name of file holding the current background music
        /// </summary>
        private String musicFilename = String.Empty;

        /// <summary>
        /// The actual music
        /// </summary>
        private FMOD.Sound music;

        /// <summary>
        /// Channel used to play background music
        /// </summary>
        private FMOD.Channel musicChannel;

        /// <summary>
        /// Number of sound channels we want FMOD to have
        /// </summary>
        const int numSoundChannels = 8;

        /// <summary>
        /// Directory where sound files are stored
        /// </summary>
        private string SoundsDirectory { get { return StorageContainer.TitleLocation + @"\Content\Audio\Sounds\"; } }

        /// <summary>
        /// Directory where music files are stored
        /// </summary>
        private string MusicDirectory { get { return StorageContainer.TitleLocation + @"\Content\Audio\Music\"; } }

        /// <summary>
        /// Do we ignore missing sound and music files?
        /// </summary>
        private bool ignoreMissingAudioFiles = true;

        /// <summary>
        /// This stops us changing music when Geoscape & base layout are constantly re-created
        /// </summary>
        private string lastRandomDirectory;

        /// <summary>
        /// The volume sound will be played at (0 = off, 1 = max)
        /// </summary>
        public float soundVolume = 1.0f;

        /// <summary>
        /// The volume sound will be played at (0 = off, 1 = max)
        /// </summary>
        public float musicVolume = 1.0f;

        #endregion Fields
    }
}


