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
* @author File creator: dteviot
* @author Credits: none
*/
#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.Text;

using CeGui;
using ProjectXenocide.UI.Screens;

using ProjectXenocide.Model.Geoscape;
using ProjectXenocide.Model.Geoscape.Vehicles;



#endregion

namespace ProjectXenocide.UI.Dialogs
{
    class OptionsDialog : Dialog
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public OptionsDialog()
            : base("Content/Layouts/OptionsDialog.layout")
        {
        }

        #region Create the CeGui widgets

        /// <summary>
        /// 
        /// </summary>
        protected override void CreateCeguiWidgets()
        {
            AddButtonSound(btnAbandonName, sndAbandonFilename);
        }

        #endregion Create the CeGui widgets

        #region event handlers

        [GuiEvent()]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public void OnCancelClicked(object sender, CeGui.GuiEventArgs e)
        {
            ScreenManager.CloseDialog(this);
        }

        [GuiEvent()]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public void OnAbandonClicked(object sender, CeGui.GuiEventArgs e)
        {
            Screen screen = new StartScreen();
            ScreenManager.CloseDialog(this);
            ScreenManager.ScheduleScreen(screen);
        }

        [GuiEvent()]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public void OnLoadClicked(object sender, CeGui.GuiEventArgs e)
        {
            Screen screen = new LoadSaveGameScreen(
                    LoadSaveGameScreen.Mode.Load,
                    LoadSaveGameScreen.CancelScreen.Geoscape);
            ScreenManager.CloseDialog(this);
            ScreenManager.ScheduleScreen(screen);
        }

        [GuiEvent()]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public void OnSaveClicked(object sender, CeGui.GuiEventArgs e)
        {
            Screen screen = new LoadSaveGameScreen(
                    LoadSaveGameScreen.Mode.Save,
                    LoadSaveGameScreen.CancelScreen.Geoscape);
            ScreenManager.CloseDialog(this);
            ScreenManager.ScheduleScreen(screen);
        }

        [GuiEvent()]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public void OnSoundClicked(object sender, CeGui.GuiEventArgs e)
        {
            ScreenManager.CloseDialog(this);
            ScreenManager.ShowDialog(new SoundOptionsDialog());
        }

        #endregion event handlers

        #region Constants

        private const string btnAbandonName = "btnAbandon";
        private const string sndAbandonFilename = "Menu/exitgame.ogg";

        #endregion
    }
}
