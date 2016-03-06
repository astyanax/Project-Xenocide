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
* @file AlienMissionDialog.cs
* @date Created: 2007/09/20
* @author File creator: cgoat
* @author Credits: none
*/
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using ProjectXenocide.Model.Geoscape.AI;
using ProjectXenocide.UI.Screens;

namespace ProjectXenocide.UI.Dialogs
{
    class AlienMissionDialog : Dialog
    {
        public AlienMissionDialog()
            : base(new System.Drawing.SizeF(0.5f, 0.4f))
        {
        }

        #region Create the CeGui widgets

        /// <summary>
        /// add the buttons to the screen
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:DoNotPassLiteralsAsLocalizedParameters", 
            MessageId = "CeGui.Window.set_Text(System.String)", Justification="It's debugging code")]
        protected override void CreateCeguiWidgets()
        {
            // buttons
            missionTypeBox = GuiBuilder.CreateComboBox("missionTypeBox");
            missionTypeBox.ReadOnly = true;
            missionTypeBox.Text = "Select Mission Type";
            missionTypeBox.AddItems(Enum.GetNames(typeof(AlienMission)));
            AddWidget(missionTypeBox, 0.10f, 0.18f, 0.80f, 0.68f);
            launchButton = AddButton("BUTTON_OK", 0.10f, 0.87f, 0.35f, 0.10f);
            launchButton.Disable();
            cancelButton = AddButton("BUTTON_CANCEL", 0.50f, 0.87f, 0.35f, 0.10f);

            launchButton.Clicked += new CeGui.GuiEventHandler(OnButtonClicked);
            cancelButton.Clicked += new CeGui.GuiEventHandler(OnButtonClicked);
            missionTypeBox.ListSelectionAccepted += new CeGui.WindowEventHandler(OnMissionTypeChanged);
        }

        private CeGui.Widgets.ComboBox missionTypeBox;
        private CeGui.Widgets.PushButton cancelButton;
        private CeGui.Widgets.PushButton launchButton;

        #endregion Create the CeGui widgets

        #region event handlers

        /// <summary>Respond to user clicking button</summary>
        /// <param name="sender">Button the user clicked</param>
        /// <param name="e">Not used</param>
        private void OnButtonClicked(object sender, CeGui.GuiEventArgs e)
        {
            if (sender == launchButton)
            {
                AlienMission missionType = (AlienMission) Enum.Parse(typeof(AlienMission), missionTypeBox.SelectedItem.Text);
                GeoscapeScreen screen = new GeoscapeScreen();
                screen.State = new GeoscapeScreen.TargetAlienMissionState(screen, missionType);
                ScreenManager.ScheduleScreen(screen);
            }
            ScreenManager.CloseDialog(this);
        }

        /// <summary>
        /// Respond to user selection a mission type in the drop down list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnMissionTypeChanged(object sender, CeGui.WindowEventArgs e)
        {
            launchButton.Enable();
        }

        #endregion event handlers
    }
}
