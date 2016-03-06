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
using Xenocide.UI.Screens;

using Xenocide.Model.Geoscape;
using Xenocide.Model.Geoscape.Craft;

#endregion

namespace Xenocide.UI.Dialogs
{
    class OptionsDialog : Dialog
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public OptionsDialog()
            : base(new System.Drawing.SizeF(0.5f, 0.3f))
        {
        }

        #region Create the CeGui widgets

        /// <summary>
        /// add the buttons to the screen
        /// </summary>
        protected override void CreateCeguiWidgets()
        {
            // buttons
            loadButton    = AddButton("BUTTON_LOAD",     0.10f, 0.40f, 0.80f, 0.10f);
            saveButton    = AddButton("BUTTON_SAVE",     0.10f, 0.55f, 0.80f, 0.10f);
            abandonButton = AddButton("BUTTON_ABANDON",  0.10f, 0.70f, 0.80f, 0.10f);
            cancelButton  = AddButton("BUTTON_CANCEL",   0.10f, 0.85f, 0.80f, 0.10f);

            loadButton.Clicked    += new CeGui.GuiEventHandler(OnButtonClicked);
            saveButton.Clicked    += new CeGui.GuiEventHandler(OnButtonClicked);
            abandonButton.Clicked += new CeGui.GuiEventHandler(OnButtonClicked);
            cancelButton.Clicked  += new CeGui.GuiEventHandler(OnButtonClicked);
        }

        private CeGui.Widgets.PushButton loadButton;
        private CeGui.Widgets.PushButton saveButton;
        private CeGui.Widgets.PushButton abandonButton;
        private CeGui.Widgets.PushButton cancelButton;

        #endregion Create the CeGui widgets

        #region event handlers

        /// <summary>Respond to user clicking button</summary>
        /// <param name="sender">Button the user clicked</param>
        /// <param name="e">Not used</param>
        private void OnButtonClicked(object sender, CeGui.GuiEventArgs e)
        {
            // figure out screen to go to
            Screen screen = null;
            if (sender == loadButton)
            {
                screen = new LoadSaveGameScreen(
                    LoadSaveGameScreen.Mode.Load,
                    ScreenManager,
                    new GeoscapeScreen(ScreenManager)
                );
            }
            else if (sender == saveButton)
            {
                screen = new LoadSaveGameScreen(
                    LoadSaveGameScreen.Mode.Save,
                    ScreenManager,
                    new GeoscapeScreen(ScreenManager)
                );
            }
            else if (sender == abandonButton)
            {
                screen = new StartScreen(ScreenManager);
            }
            
            // close this dialog
            ScreenManager.CloseDialog(this);

            // if screen is null, then cancel was pressed, so just go back to screen that's showing
            if (null != screen)
            {
                ScreenManager.ScheduleScreen(screen);
            }
        }

        #endregion event handlers

        #region Fields

        #endregion
    }
}
