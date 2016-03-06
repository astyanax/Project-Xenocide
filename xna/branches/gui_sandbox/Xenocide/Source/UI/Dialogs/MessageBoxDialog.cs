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
* @file MessageBoxDialog.cs
* @date Created: 2007/03/04
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

#endregion

namespace Xenocide.UI.Dialogs
{
    /// <summary>
    /// Dialog that gives user a message
    /// </summary>
    public class MessageBoxDialog : Dialog
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="messageText">Text to show on dialog</param>
        public MessageBoxDialog(string messageText)
            : base(new System.Drawing.SizeF(0.5f, 0.3f))
        {
            this.messageText = messageText;
        }

        #region Create the CeGui widgets

        /// <summary>
        /// add the buttons to the screen
        /// </summary>
        protected override void CreateCeguiWidgets()
        {
            // static text to show the message
            textWindow = GuiBuilder.CreateText(CeguiId + "_text");
            AddWidget(textWindow, 0.02f, 0.073f, 0.96f, 0.52f);
            textWindow.Text = messageText;
            textWindow.HorizontalFormat = HorizontalTextFormat.WordWrapLeft;

            // other buttons
            okButton        = AddButton("BUTTON_OK",  0.7475f, 0.80f, 0.2275f, 0.10f);

            okButton.Clicked += new CeGui.GuiEventHandler(OnOkClicked);
        }

        private CeGui.Widgets.StaticText  textWindow;
        private CeGui.Widgets.PushButton  okButton;

        #endregion Create the CeGui widgets

        #region event handlers

        /// <summary>user wants to close dialog</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void OnOkClicked(object sender, CeGui.GuiEventArgs e)
        {
            ScreenManager.CloseDialog(this);
        }

        #endregion event handlers

        /// <summary>
        /// Text shown on dialog
        /// </summary>
        private string messageText;
    }
}
