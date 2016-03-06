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
* @file ScreenManager.cs
* @date Created: 2007/01/20
* @author File creator: dteviot
* @author Credits: none
*/
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using CeGui.Renderers.Xna;

using Xenocide.UI.Dialogs;
using Xenocide.Model;
using Xenocide.Model.Geoscape;
using Xenocide.Utils;


#endregion

namespace Xenocide.UI.Screens
{
    /// <summary>
    /// This class is responsible for keeping track of the Screen currently
    /// being shown to the user.
    /// </summary>
    public class ScreenManager : IDisposable
    {
        /// <summary>
        /// The content manager
        /// </summary>
        private ContentManager content;

        /// <summary>
        /// The screen currently being shown to the user
        /// </summary>
        public Screen CurrentScreen { get { return currentScreen; } }
        
        /// <summary>
        /// The screen currently being shown to the user
        /// </summary>
        private Screen currentScreen;

        /// <summary>
        /// The screen to change to on next update
        /// </summary>
        private Screen nextScreen;

        /// <summary>
        /// The game object
        /// </summary>
        private Xenocide xenogame;

        /// <summary>
        /// Set the screen to show on the next update()
        /// </summary>
        /// <param name="newScreen">The new screen to show</param>
        /// <remarks>Need to delay swapping the screen until the next update.
        /// Due to issues with any dialogs being shown being "owned" by the screen
        /// currently being shown.
        /// </remarks>
        public void ScheduleScreen(Screen newScreen)
        {
            // we can only have one screen pending for a swap
            Debug.Assert(nextScreen == null);
            nextScreen = newScreen;
        }

        /// <summary>
        /// Replace the screen currently being shown with the "nextScreen"
        /// </summary>
        private void SwapScreens()
        {
            Util.GeoTimeDebugWriteLine("Showing Screen {0}", nextScreen.GetType().Name);
            Debug.Assert(null != nextScreen);
            if (null != currentScreen)
            {
                currentScreen.UnloadGraphicsContent(true);
                currentScreen.Dispose();
            }
            currentScreen = nextScreen;
            nextScreen = null;
            currentScreen.LoadGraphicsContent(content, xenogame.GraphicsDevice, true);
            currentScreen.Show();
        }
        
        // set this to exit the game
        private bool quitGame;

        /// <summary>
        /// set this to exit the game
        /// </summary>
        public bool QuitGame
        {
            get { return quitGame; }
            set { if (value) quitGame = true;  }
        }

        /// <summary>
        /// Load the Screen's graphic content
        /// </summary>
        /// <param name="game">link to game object</param>
        /// <param name="device">the display</param>
        /// <param name="loadAllContent">true if all graphics resources need to be loaded</param>
        public void LoadGraphicsContent(Xenocide game, GraphicsDevice device, bool loadAllContent)
        {
            // track the game object, we need it later
            xenogame = game;
            
            // construct the content manager, if it doesn't already exist
            if (content == null)
            {
                content = new ContentManager(game.Services);
            }

            if (currentScreen != null)
            {
                currentScreen.LoadGraphicsContent(content, device, loadAllContent);
            }
        }

        /// <summary>
        /// Unload's the Scene's graphic content
        /// </summary>
        /// <param name="unloadAllContent"></param>
        public void UnloadGraphicsContent(bool unloadAllContent)
        {
            if ((content != null) && (unloadAllContent == true))
            {
                content.Unload();
            }

            if (currentScreen != null)
            {
                currentScreen.UnloadGraphicsContent(unloadAllContent);
            }
        }

        /// <summary>
        /// Update any model data
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public void Update(GameTime gameTime)
        {
            // if there is a dialog queued for display, and we're not showing any dialogs
            // show the dialog
            if ((0 == showingDialogs.Count) && (0 < queuedDialogs.Count))
            {
                ShowDialog(queuedDialogs[0]);
                queuedDialogs.RemoveAt(0);
            }

            // only update the screen when there are no dialogs
            if (0 == showingDialogs.Count) 
            {
                // if we're scheduled to swap the screen, so so now
                if (null != nextScreen)
                {
                    SwapScreens();
                }
                
                // and pump it's update
                if (null != currentScreen)
                {
                    currentScreen.Update(gameTime);
                }
            }
        }

        /// <summary>
        /// Render the 3D scene
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        /// <param name="device">Device to use for render</param>
        public void Draw(GameTime gameTime, GraphicsDevice device)
        {
            if (currentScreen != null)
            {
                currentScreen.Draw(gameTime, device);
                fpsCalcs();
            }
        }

        /// <summary>
        /// Put dialog into the queue to be displayed
        /// </summary>
        /// <param name="dialog">The dialog to queue</param>
        public void QueueDialog(Dialog dialog)
        {
            //ToDo: put dialog into queue in priority position
            queuedDialogs.Add(dialog);
        }

        /// <summary>
        /// Put dialog into top of stack of dialogs being shown
        /// </summary>
        /// <param name="dialog">The dialog to show</param>
        public void ShowDialog(Dialog dialog)
        {
            Util.GeoTimeDebugWriteLine("Showing dialog {0}", dialog.GetType().Name);
            
            // disable controls on current topmost dialog/screen
            TopmostFrame.Enable(false);
            
            // and now put this dialog on the screen
            showingDialogs.Push(dialog);
            dialog.Show();
        }

        /// <summary>
        /// Remove the topmost dialog currently being shown
        /// </summary>
        /// <param name="dialog">The dialog making the call (which should ALSO be the topmost dialog)</param>
        public void CloseDialog(Dialog dialog)
        {
            Util.GeoTimeDebugWriteLine("Closing dialog {0}", dialog.GetType().Name);

            Debug.Assert(dialog == showingDialogs.Peek());
            
            // remove dialog
            showingDialogs.Pop().Dispose();

            // re-enable whatever dialog/screen is now topmost
            TopmostFrame.Enable(true);
        }

        /// <summary>
        /// This function is intended for checking data integrity
        /// </summary>
        /// <remarks>asserts if there are dialogs queued or showing</remarks>
        [Conditional("DEBUG")]
        public void AssertNoDialogsQueuedOrShowing()
        {
            Debug.Assert((0 == showingDialogs.Count) && (0 == queuedDialogs.Count));
        }

        /// <summary>
        /// Get the "topmost" dialog or screen on the display
        /// </summary>
        /// <returns>Topmost Frame</returns>
        public Frame TopmostFrame
        {
            get
            {
                if (0 < showingDialogs.Count)
                {
                    return showingDialogs.Peek();
                }
                else
                {
                    return currentScreen;
                }
            }
        }

        /// <summary>
        /// Implement IDisposable
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Implement IDisposable
        /// </summary>
        /// <param name="disposing">false when called from a finalizer</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (content != null)
                {
                    content.Dispose();
                    content = null;
                }
            }
        }

        /// <summary>
        /// Retreive the Gui Sheet that is the root node of the tree of all CeGui#
        /// windows on the display
        /// </summary>
        /// <returns>the Gui Sheet</returns>
        /// RK:  BTW use a property named Sheet instead.
        /// DT: Except the Gui Sheet ISN'T owned by the ScreenManager.
        public CeGui.GuiSheet RootGuiSheet
        {
            get { return (CeGui.GuiSheet)CeGui.WindowManager.Instance.GetWindow("Root"); }
        }

        /// <summary>
        /// The CeGui gui builder used to create widgets (that we later attach to screens/dialogs)
        /// </summary>
        public CeGui.GuiBuilder GuiBuilder { get { return guiBuilder; } }
        
        /// <summary>
        /// The CeGui gui builder used to create widgets (that we later attach to screens/dialogs)
        /// </summary>
        private CeGui.GuiBuilder guiBuilder = new CeGui.WidgetSets.Taharez.TLGuiBuilder();

        /// <summary>
        /// The dialogs that are waiting to be shown to the user
        /// </summary>
        private List<Dialog> queuedDialogs = new List<Dialog>();

        /// <summary>
        /// The dialogs that are being shown to the user
        /// </summary>
        private Stack<Dialog> showingDialogs = new Stack<Dialog>();

        /// <summary>
        /// Used for tracking FPS stats
        /// </summary>
        int frameCount;

        /// <summary>
        /// Used for tracking FPS stats
        /// </summary>
        DateTime lastOutput = DateTime.Now;

        /// <summary>
        /// Assorted calcs to put the FPS rate at top of screen
        /// </summary>
        private void fpsCalcs()
        {
            ++frameCount;
            DateTime rightNow = DateTime.Now;
            double seconds = (rightNow - lastOutput).TotalMilliseconds / 1000.0;
            if (1.0 < seconds)
            {
                xenogame.Window.Title = 
                    Util.StringFormat("Xenocide.exe  fps = {0}", (frameCount / seconds));
                frameCount = 0;
                lastOutput = rightNow;
            }
        }
    }
}
