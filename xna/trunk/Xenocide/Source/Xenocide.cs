
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
* @author File creator: David Teviotdale
* @author Credits: none
*/
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Diagnostics;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;

using CeGui.Renderers.Xna;
using CeGui;

using AudioSystem;

using ProjectXenocide.Utils;
using ProjectXenocide.UI.Screens;
using ProjectXenocide.Model;

using Xenocide.Resources;

#endregion

namespace ProjectXenocide 
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Xenocide : Microsoft.Xna.Framework.Game
    {
        private GraphicsDeviceManager graphics;
        private GuiManager gui;
        private static ScreenManager screenManager;
        private static Xenocide      instance;

        /// <summary>
        /// The random number generator everyone should use
        /// </summary>
        private static Rng rng = new Rng();

        /// <summary>
        /// All the data giving the state of the game
        /// </summary>
        private static GameState gameState;

        /// <summary>
        /// Static Data that is loaded from XML files
        /// </summary>
        private static StaticTables staticTables;

        /// <summary>Game balance class</summary>
        private static GameBalanceClass gameBalance;

        /// <summary>
        /// Constructor
        /// </summary>
        public Xenocide() 
        {
            instance      = this; 
            graphics      = new GraphicsDeviceManager(this);
            gui           = new GuiManager(this);
            screenManager = new ScreenManager();
            staticTables  = new StaticTables();
            staticTables.Populate();
            gameBalance = new GameBalanceClass(Difficulty.Easy);
            gameState     = new GameState();
            Components.Add(gui);
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1300:SpecifyMessageBoxOptions",
            Justification="FxCop false positive")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:DisposeObjectsBeforeLosingScope",
            Justification="FxCop false positive")]
        protected override void Initialize() 
        {
            // check shader version
            if (Util.GetShaderVersion(GraphicsDevice.GraphicsDeviceCapabilities) == 1)
            {
                System.Windows.Forms.MessageBox.Show(Strings.EXCEPTION_USING_V1_1_SHADER);
            }

            // Register FMOD sound system
            Components.Add(new AudioSystem.FmodGameComponent(this));

            base.Initialize();

            InitializeCegui();
            InitializeAudioSystem();
            InitializeGameOptions();
        }

        /// <summary>
        /// Set up CeGui to do the work
        /// </summary>
        private static void InitializeCegui()
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
            FontManager.Instance.CreateFont("Default", "Arial", 8, FontFlags.None);
            FontManager.Instance.CreateFont("Xeno", "Gotthard", 8, FontFlags.None);
            FontManager.Instance.CreateFont("XenoBig", "Gotthard", 10, FontFlags.Bold);
            FontManager.Instance.CreateFont("XenoSmall", "Gotthard", 6, FontFlags.None);
            FontManager.Instance.CreateFont("LargeBaseName", "Arial", 24, FontFlags.Bold);
            FontManager.Instance.CreateFont("GeoTime", "OCR A Extended", 10, FontFlags.Bold);
            FontManager.Instance.CreateFont("GeoTimeBig", "OCR A Extended", 18, FontFlags.Bold);
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
            screenManager.ScheduleScreen(new UI.Screens.StartScreen());
        }

        /// <summary>
        /// Load in the sound effects we're going to play
        /// </summary>
        public static void InitializeAudioSystem()
        {
            IAudioSystem audioSystem = Xenocide.AudioSystem;
            audioSystem.LoadSound("PlanetView\\speedfast.ogg");
            audioSystem.LoadSound("PlanetView\\speedslow.ogg");
            audioSystem.LoadSound("PlanetView\\speedveryfast.ogg");
            audioSystem.LoadSound("PlanetView\\zoomin.ogg");
            audioSystem.LoadSound("PlanetView\\zoomout.ogg");
            audioSystem.LoadSound("PlanetView\\clickobjectonplanet.ogg");
            audioSystem.LoadSound("Menu\\buttonclick1_ok.ogg");
            audioSystem.LoadSound("Menu\\buttonclick2_changesetting.ogg");
            audioSystem.LoadSound("Menu\\buttonover.ogg");
            audioSystem.LoadSound("Menu\\exitgame.ogg");
        }

        /// <summary>
        /// Load and apply the persisted game options if an options file exists.
        /// </summary>
        public static void InitializeGameOptions()
        {
            var gameOptions = GameOptions.LoadFromFile();
            gameOptions.Apply();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            screenManager.LoadContent(graphics.GraphicsDevice);
        }


        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            screenManager.UnloadContent();
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

            screenManager.Update(gameTime);
            base.Update(gameTime);
        }


        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime) 
        {
            graphics.GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here

            screenManager.Draw(gameTime, graphics.GraphicsDevice);
            base.Draw(gameTime);

            // WORKAROUND: Updating DynamicVertexBufffer caused InvalidOperationException. Solved by adding this row.
            graphics.GraphicsDevice.Vertices[0].SetSource(null, 0, 0);
        }

        /// <summary>
        /// Looks after the screens and dialogs
        /// </summary>
        static public ScreenManager ScreenManager { get { return screenManager; } }

        /// <summary>
        /// The generator that should be used for all random numbers
        /// </summary>
        public static Rng Rng { get { return rng; } }

        /// <summary>
        /// Current state of the game
        /// </summary>
        public static GameState GameState { get { return gameState; } set { gameState = value; } }

        /// <summary>
        /// The static data, loaded from XML files
        /// </summary>
        public static StaticTables StaticTables { get { return staticTables; } }

        /// <summary>
        /// The instance that is the Root node of the game.
        /// </summary>
        public static Xenocide Instance { get { return instance; } }

        /// <summary>
        /// Get the sound playing system
        /// </summary>
        public static IAudioSystem AudioSystem 
        {
            get { return Instance.Services.GetService(typeof(IAudioSystem)) as IAudioSystem; } 
        }

        /// <summary>
        /// Current version of Xenocide
        /// </summary>
        public static String CurrentVersion
        {
            get { return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString(); }
        }

        /// <summary>Game balance methods</summary>
        public static GameBalanceClass GameBalance
        {
            get { return gameBalance; }
        }
    }
}
