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
* @file StartScreen.cs
* @date Created: 2007/01/20
* @author File creator: dteviot
* @author Credits: none
*/
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Text;

using CeGui;

using Xenocide.Model;

#endregion Using Statements

namespace Xenocide.UI.Screens
{
    /// <summary>
    /// This is the first screen shown when Xenocide starts
    /// </summary>
    public class StartScreen : Screen
    {
        /// <summary>
        /// Default constructor (obviously)
        /// </summary>
        public StartScreen(ScreenManager screenManager)
            : base("StartScreen", screenManager)
        {
        }

        #region Create the CeGui widgets

        /// <summary>
        /// add the buttons to the screen
        /// </summary>
        protected override void CreateCeguiWidgets()
        {
            startButton = AddButton("BUTTON_NEW_GAME",        0.3500f, 0.6000f, 0.3000f, 0.1333f);
            loadButton  = AddButton("BUTTON_LOAD_SAVED_GAME", 0.3500f, 0.6000f, 0.3000f, 0.1333f);
            quitButton  = AddButton("BUTTON_QUIT",            0.3500f, 0.8000f, 0.3000f, 0.1333f);

            startButton.Clicked += new CeGui.GuiEventHandler(OnNewGameClicked); 
            loadButton.Clicked  += new CeGui.GuiEventHandler(OnShowLoadGameScreen);
            quitButton.Clicked  += new CeGui.GuiEventHandler(OnQuitGameClicked);
        }

        private CeGui.Widgets.PushButton startButton;
        private CeGui.Widgets.PushButton loadButton;
        private CeGui.Widgets.PushButton quitButton;

        #endregion Create the CeGui widgets

        #region event handlers

        /// <summary>Start a new game</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void OnNewGameClicked(object sender, CeGui.GuiEventArgs e)
        {
            Xenocide.GameState.SetToStartGameCondition();
            GeoscapeScreen geoscapeScreen = new GeoscapeScreen(ScreenManager);
            geoscapeScreen.State = GeoscapeScreen.GeoscapeScreenState.AddingFirstBase;
            ScreenManager.ScheduleScreen(geoscapeScreen);
        }

        /// <summary>Replace screen on display with the Load Game Screen</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void OnShowLoadGameScreen(object sender, CeGui.GuiEventArgs e)
        {
            ScreenManager.ScheduleScreen(
                new LoadSaveGameScreen(
                    LoadSaveGameScreen.Mode.Load, 
                    ScreenManager,
                    new StartScreen(ScreenManager)
                )
            );
        }

        /// <summary>Quit the game</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void OnQuitGameClicked(object sender, CeGui.GuiEventArgs e)
        {
            ScreenManager.QuitGame = true;
        }

        #endregion event handlers
    }
}
