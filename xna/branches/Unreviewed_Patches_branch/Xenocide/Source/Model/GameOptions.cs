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
#endregion

namespace ProjectXenocide.Model
{
	/// <summary>
	/// This class holds a list of game options that are persisted when saving out of an options
	/// dialog, and are applied on launch of the game.
	/// </summary>
	[Serializable]
	public class GameOptions
	{
		public GameOptions(string GameVersion)
		{
			gameVersion = GameVersion;
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
		private string gameVersion = string.Empty;
		public string GameVersion
		{
			get { return gameVersion; }
			set { gameVersion = value; }
		}
		
	}
}
