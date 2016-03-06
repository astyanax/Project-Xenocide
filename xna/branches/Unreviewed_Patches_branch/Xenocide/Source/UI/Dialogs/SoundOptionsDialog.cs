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
* @file OptionsDialog.cs
* @date Created: 2007/03/12
* @author File creator: Jasin Windisch
* @author Credits: none
*/
#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.Text;

using CeGui;

using ProjectXenocide.UI.Screens;
using ProjectXenocide.Utils;
using ProjectXenocide.Model;
using ProjectXenocide.Model.Geoscape;
using ProjectXenocide.Model.Geoscape.Vehicles;

#endregion

namespace ProjectXenocide.UI.Dialogs
{
    /// <summary>
    /// Dialog that lets user set the sound and music volume
    /// </summary>
    class SoundOptionsDialog : Dialog
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public SoundOptionsDialog()
            : base("Content/Layouts/SoundOptionsDialog.layout")
        {
        }

        #region Create the CeGui widgets

        /// <summary>
        /// add the buttons to the screen
        /// </summary>
        protected override void CreateCeguiWidgets()
        {
            musicSlider = (CeGui.Widgets.Slider)WindowManager.Instance.GetWindow(sldrMusicName);
            musicCheckbox = (CeGui.Widgets.Checkbox)WindowManager.Instance.GetWindow(chkMusicName);
            soundSlider = (CeGui.Widgets.Slider)WindowManager.Instance.GetWindow(sldrSoundName);
            soundCheckbox = (CeGui.Widgets.Checkbox)WindowManager.Instance.GetWindow(chkSoundName);

            //Update checkbox to on/off
            soundCheckbox.Checked = 0 < Xenocide.AudioSystem.SoundVolume;
            musicCheckbox.Checked = 0 < Xenocide.AudioSystem.MusicVolume;

            //Disable sliders if off
            if (!musicCheckbox.Checked)
            {
                musicSlider.Disable();
            }
            if (!soundCheckbox.Checked)
            {
                soundSlider.Disable();
            }

            //Update slider to current volume
            musicSlider.Value = Xenocide.AudioSystem.MusicVolume;
            soundSlider.Value = Xenocide.AudioSystem.SoundVolume;
        }

        private CeGui.Widgets.Slider musicSlider;
        private CeGui.Widgets.Slider soundSlider;

        private CeGui.Widgets.Checkbox musicCheckbox;
        private CeGui.Widgets.Checkbox soundCheckbox;


        #endregion Create the CeGui widgets

        #region event handlers

        /// <summary>
        /// Respond to user clicking the Sound or Music check box
        /// </summary>
        /// <param name="sender">checkbox user clicked</param>
        /// <param name="e">unused</param>
        [GuiEvent()]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public void OnCheckboxChecked(object sender, WindowEventArgs e)
        {
            CeGui.Widgets.Checkbox checkbox = sender as CeGui.Widgets.Checkbox;
            if (checkbox == musicCheckbox)
            {
                EnableVolumeSlider(musicSlider, checkbox.Checked, Xenocide.AudioSystem.MusicVolume);
            }
            else
            {
                EnableVolumeSlider(soundSlider, checkbox.Checked, Xenocide.AudioSystem.SoundVolume);
            }
        }

        /// <summary>
        /// Disable/Enable sound/music slider and change volume to match
        /// </summary>
        /// <param name="slider">Music or Sound Volume slider</param>
        /// <param name="enable">enable or diable the slider</param>
        /// <param name="enableValue">Value to set slider to, if enabling</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode",
            Justification = "FxCop false positive")]
        private static void EnableVolumeSlider(CeGui.Widgets.Slider slider, bool enable, float enableValue)
        {
            if (enable)
            {
                slider.Enable();
                slider.Value = (enableValue == 0.0f) ? 1.0f : enableValue;
            }
            else
            {
                slider.Disable();
                slider.Value = 0.0f;
            }
        }

        /// <summary>
        /// Change volume on slider change
        /// </summary>
        /// <param name="sender">slider moved</param>
        /// <param name="e">unused</param>
        [GuiEvent()]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public void OnValueChanged(object sender, WindowEventArgs e)
        {
            if (sender == musicSlider)
            {
                Xenocide.AudioSystem.MusicVolume = musicSlider.Value;
            }
            else
            {
                Xenocide.AudioSystem.SoundVolume = soundSlider.Value;
            }
        }

        /// <summary>Respond to user clicking the save button</summary>
        /// <param name="sender">Button the user clicked</param>
        /// <param name="e">Not used</param>
        [GuiEvent()]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public void OnSaveClicked(object sender, CeGui.GuiEventArgs e)
        {
			GameOptions gameOptions;

			if (GameFiles.DoesFileExist(@".\XeNAcide\options", @"SoundData"))
				 gameOptions = GameFiles.Load<GameOptions>(@".\XeNAcide\options", @"SoundData");
			else
				 gameOptions = new GameOptions(Xenocide.CurrentVersion);

			gameOptions.MusicVolume = musicSlider.Value;
			gameOptions.SoundVolume = soundSlider.Value;

			try
			{
				GameFiles.Save(gameOptions, gameOptionsLocation, gameOptionsFile);				
			}
			catch 
			{
				//Util.ShowMessageBox(Strings.MSGBOX_UNABLE_TO_SAVE_FILE, err.Message);
				//TODO - It's not the end of the world if you can't save game options but it would
				//be nice to save to a log file for details of the error.      
			}

            // close this dialog
            ScreenManager.CloseDialog(this);
        }

        /// <summary>Respond to user clicking the cancel button</summary>
        /// <param name="sender">Button the user clicked</param>
        /// <param name="e">Not used</param>
        [GuiEvent()]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public void OnCancelClicked(object sender, CeGui.GuiEventArgs e)
        {
            Xenocide.AudioSystem.MusicVolume = musicLast;
            Xenocide.AudioSystem.SoundVolume = soundLast;



            // close this dialog
            ScreenManager.CloseDialog(this);
        }

        #endregion event handlers

        #region Fields

        /// <summary>
        /// Save music volume, so can restore if user cancels
        /// </summary>
        private float musicLast = Xenocide.AudioSystem.MusicVolume;

        /// <summary>
        /// Save sound volume, so can restore if user cancels
        /// </summary>
        private float soundLast = Xenocide.AudioSystem.SoundVolume;

		/// <summary>
		/// Where to save the options data
		/// </summary>
		private string gameOptionsLocation = @".\XeNAcide\Options";

		/// <summary>
		/// Name of the file for the options data
		/// </summary>
		private string gameOptionsFile = "SoundData";
        #endregion

        #region Constants

        private const string sldrMusicName = "sldrMusic";
        private const string chkMusicName = "chkMusic";

        private const string sldrSoundName = "sldrSound";
        private const string chkSoundName = "chkSound";

        #endregion
    }
}
