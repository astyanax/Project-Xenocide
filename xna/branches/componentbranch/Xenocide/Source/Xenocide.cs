
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
* @file Xenocide.cs
* @date Created: 2007/01/20
* @author File creator: dteviot
* @author Credits: none
*/
#endregion

#region Using Statements
using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;

using CeGui.Renderers.Xna;
using CeGui;

using Xenocide.UI.Screens;
using Xenocide.Model;
using Xenocide.Model.Geoscape.Research;
using Xenocide.Model.Geoscape;
using Xenocide.Model.Geoscape.HumanBases;

#endregion

namespace Xenocide 
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Xenocide : Microsoft.Xna.Framework.Game
    {
        private GraphicsDeviceManager graphics;
        private GuiManager gui;
        private ScreenManager screenManager;

        /// <summary>
        /// The random number generator everyone should use
        /// </summary>
        private static Random rng;

        /// <summary>
        /// All the data giving the state of the game
        /// </summary>
        private static GameState gameState;

        /// <summary>
        /// Static Data that is loaded from XML files
        /// </summary>
        private static StaticTables staticTables;

        /// <summary>
        /// Constructor
        /// </summary>
        public Xenocide() 
        {
            rng = new Random();
            graphics = new GraphicsDeviceManager(this);
            gui = new GuiManager(this);
            screenManager = new ScreenManager(this);
            staticTables = new StaticTables();
            gameState = new GameState();
            new ResearchService(this);
            new GameStateService(this);
            new HumanBaseService(this);
            Components.Add(gui);
            Components.Add(screenManager);
            Components.Add(new GeoTimeService(this));
        }

        /// <summary>
        /// The Graphics device used for rendering
        /// </summary>
        public GraphicsDevice GraphicsDevice { get { return graphics.GraphicsDevice; } }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize() 
        {
            // TODO: Add your initialization logic here

            base.Initialize();

            InitializeCegui();
        }

        /// <summary>
        /// Set up CeGui to do the work
        /// </summary>
        private void InitializeCegui()
        {
            // When the gui imagery side of things is set up, we should load in a font.
            // You should always load in at least one font, this is to ensure that there
            // is a default available for any gui element which needs to draw text.
            // The first font you load is automatically set as the initial default font,
            // although you can change the default later on if so desired.  Again, it is
            // possible to list fonts to be automatically loaded as part of a scheme, so
            // this step may not usually be performed explicitly.
            //
            // Fonts are loaded via the FontManager singleton.
            FontManager.Instance.CreateFont("Default", "Arial", 9, FontFlags.None);
            FontManager.Instance.CreateFont("WindowTitle", "Arial", 12, FontFlags.Bold);
            GuiSystem.Instance.SetDefaultFont("Default");

            // The next thing we do is to set a default mouse cursor image.  This is
            // not strictly essential, although it is nice to always have a visible
            // cursor if a window or widget does not explicitly set one of its own.
            //
            // This is a bit hacky since we're assuming the TaharezLook image set, referenced
            // below, will always be available.
            GuiSystem.Instance.SetDefaultMouseCursor(
              ImagesetManager.Instance.GetImageset("TaharezLook").GetImage("MouseArrow")
            );

            // Now that the system is initialised, we can actually create some UI elements,
            // for this first example, a full-screen 'root' window is set as the active GUI
            // sheet, and then a simple frame window will be created and attached to it.
            //
            // All windows and widgets are created via the WindowManager singleton.
            WindowManager winMgr = WindowManager.Instance;

            // Here we create a "DefaultWindow". This is a native type, that is, it does not
            // have to be loaded via a scheme, it is always available. One common use for the
            // DefaultWindow is as a generic container for other windows. Its size defaults
            // to 1.0f x 1.0f using the relative metrics mode, which means when it is set as
            // the root GUI sheet window, it will cover the entire display. The DefaultWindow
            // does not perform any rendering of its own, so is invisible.
            //
            // Create a DefaultWindow called 'Root', and set as the GUI root window 
            // (also known as the GUI "sheet"), so the gui we set up will be visible.
            GuiSystem.Instance.GuiSheet = (GuiSheet)winMgr.CreateWindow("DefaultWindow", "Root");

            // And load the start Screen
            screenManager.ScheduleScreen(new UI.Screens.StartScreen(this));
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime) 
        {
            // Allows the default game to exit on Xbox 360 and Windows
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            if (screenManager.QuitGame)
                this.Exit();

            //screenManager.Update(gameTime);
            base.Update(gameTime);
        }


        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime) 
        {
            graphics.GraphicsDevice.Clear(Color.CornflowerBlue);

            base.Draw(gameTime);
        }

        /// <summary>
        /// Looks after the screens and dialogs
        /// </summary>
        //static public ScreenManager ScreenManager { get { return screenManager; } }

        /// <summary>
        /// The generator that should be used for all random numbers
        /// </summary>
        public static Random Rng { get { return rng; } }

        /// <summary>
        /// Current state of the game
        /// </summary>
        public static GameState GameState { get { return gameState; } set { gameState = value; } }

        /// <summary>
        /// The static data, loaded from XML files
        /// </summary>
        public static StaticTables StaticTables { get { return staticTables; } }
    }
}