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
* @file YesNoDialog.cs
* @date Created: 2007/05/07
* @author File creator: dteviot
* @author Credits: none
*/
#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.Text;

using CeGui;

using Xenocide.Resources;
using Xenocide.UI.Screens;
using Microsoft.Xna.Framework;

#endregion

namespace Xenocide.UI.Dialogs
{
    /// <summary>
    /// Dialog that asks user a question with "yes" and "no" buttons for getting the answer
    /// </summary>
    public class YesNoDialog : Dialog
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="messageText">Text to show on dialog</param>
        /// <param name="yesButtonText">Text to show on yes button</param>
        /// <param name="noButtonText">Text to show on no button</param>
        public YesNoDialog(Game game, string messageText, String yesButtonText, String noButtonText)
            : base(game, new System.Drawing.SizeF(0.5f, 0.3f))
        {
            this.messageText   = messageText;
            this.yesButtonText = yesButtonText;
            this.noButtonText  = noButtonText;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="messageText">Text to show on dialog</param>
        public YesNoDialog(Game game, string messageText)
            : this(game, messageText, null, null)
        {
        }

        /// <summary>
        /// Create a YesNoDialog with 'OK' and 'Cancel' buttons
        /// TODO: Why not refactor this into a DialogBuilder-Service?
        /// </summary>
        /// <returns>the YesNoDialog</returns>
        public static YesNoDialog OkCancelDialog(Game game, string messageText)
        {
            return new YesNoDialog(game, messageText, Strings.BUTTON_OK, Strings.BUTTON_CANCEL);
        }

        /// <summary>
        /// delegate that is hooked up to yes or no button
        /// </summary>
        public delegate void ButtonAction();

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
            yesButton   = AddButton("BUTTON_YES",  0.2500f, 0.80f, 0.2275f, 0.10f);
            noButton    = AddButton("BUTTON_NO",   0.7475f, 0.80f, 0.2275f, 0.10f);

            yesButton.Clicked += new CeGui.GuiEventHandler(OnYesClicked);
            noButton.Clicked  += new CeGui.GuiEventHandler(OnNoClicked);

            // Set text on yes and no buttons (if necessary)
            SetButtonText();
        }

        private CeGui.Widgets.StaticText textWindow;
        private CeGui.Widgets.PushButton yesButton;
        private CeGui.Widgets.PushButton noButton;

        #endregion Create the CeGui widgets

        /// <summary>
        /// Set the text on the yes and no butons
        /// </summary>
        private void SetButtonText()
        {
            if (!String.IsNullOrEmpty(yesButtonText))
            {
                yesButton.Text = yesButtonText;
            }
            if (!String.IsNullOrEmpty(noButtonText))
            {
                noButton.Text = noButtonText;
            }
        }

        #region event handlers

        /// <summary>React to user pressing the "yes" button</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void OnYesClicked(object sender, CeGui.GuiEventArgs e)
        {
            if (null != yesAction)
            {
                yesAction();
            }
            ScreenManager.CloseDialog(this);
        }

        /// <summary>React to user pressing the "no" button</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void OnNoClicked(object sender, CeGui.GuiEventArgs e)
        {
            if (null != noAction)
            {
                noAction();
            }
            ScreenManager.CloseDialog(this);
        }

        #endregion event handlers

        #region Fields

        /// <summary>
        /// What to do when Yes button is pressed
        /// </summary>
        public ButtonAction YesAction { get { return yesAction; } set { yesAction = value; } }

        /// <summary>
        /// What to do when No button is pressed
        /// </summary>
        public ButtonAction NoAction { get { return noAction; } set { noAction = value; } }

        /// <summary>
        /// What to do when Yes button is pressed
        /// </summary>
        private ButtonAction yesAction;

        /// <summary>
        /// What to do when No button is pressed
        /// </summary>
        private ButtonAction noAction;

        /// <summary>
        /// Text shown on dialog
        /// </summary>
        private string messageText;

        /// <summary>
        /// Text to show on Yes button
        /// </summary>
        private string yesButtonText;

        /// <summary>
        /// Text to show on no button
        /// </summary>
        private string noButtonText;

        #endregion
    }
}
