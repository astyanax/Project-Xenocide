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
* @file AudioEngine.cs
* @date Created: 2007/09/08
* @author File creator: dteviot
* @author Credits: Martin Nordholts
*/
#endregion

using System;
using System.Collections.Generic;
using System.Text;

namespace AudioSystem
{
    /// <summary>
    /// The interface the sound system needs to implement
    /// </summary>
    public interface IAudioSystem
    {
        /// <summary>
        /// Load a sound off the disk, to play later.
        /// (to reduce load time)
        /// </summary>
        void LoadSound(string filename);

        /// <summary>
        /// Play a sound
        /// </summary>
        /// <param name="filename">Name of the previously loaded file holding the music</param>
        void PlaySound(string filename);

        /// <summary>
        /// Set a piece of music to be played
        /// </summary>
        /// <param name="filename">Name of the file holding the music</param>
        void PlayMusic(string filename);

        /// <summary>
        /// Play a song, chosen at random from the directory
        /// </summary>
        /// <param name="directory">Directory to search for songs</param>
        void PlayRandomMusic(string directory);

        /// <summary>
        /// Play a song, chosen at random from a list
        /// </summary>
        /// <param name="directory">the list of songs</param>
        void PlayRandomMusic(IList<string> list);

        // stop playing music
        void StopMusic();

        /// <summary>
        /// The volume sound will be played at (0 = off, 1 = max)
        /// </summary>
        float SoundVolume { get; set; }

        /// <summary>
        /// The volume sound will be played at (0 = off, 1 = max)
        /// </summary>
        float MusicVolume { get; set; }
    }
}
