
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
using System.IO;
using System.Linq;
using System.Resources;

using AudioSystem;

using Gum.DataTypes;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using MonoGameGum;

using NLog;

using ProjectXenocide.Model;
using ProjectXenocide.UI.Controls;
using ProjectXenocide.UI.Screens;
using ProjectXenocide.Utils;

using Xenocide.Resources;

#endregion

[assembly: NeutralResourcesLanguage("en")]

namespace ProjectXenocide
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Xenocide : Microsoft.Xna.Framework.Game
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private GraphicsDeviceManager graphics;
        private static ScreenManager screenManager;
        private static Xenocide instance;
        private KeyboardState _prevKeyState;

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

        private static GumProjectSave gumProject;

        /// <summary>
        /// Constructor
        /// </summary>
        public Xenocide()
        {
            instance = this;
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferHeight = 1024;
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            screenManager = new ScreenManager();
            staticTables = new StaticTables();
            staticTables.Populate();
            gameBalance = new GameBalanceClass(Difficulty.Easy);
            gameState = new GameState();
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1300:SpecifyMessageBoxOptions",
            Justification = "FxCop false positive")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:DisposeObjectsBeforeLosingScope",
            Justification = "FxCop false positive")]
        protected override void Initialize()
        {
            // check shader version
            if (Util.GetShaderVersion(GraphicsDevice) == 1)
            {
                Logger.Error(Strings.EXCEPTION_USING_V1_1_SHADER);
            }

            // Register audio system
            var audioComponent = new AudioSystem.GameAudioComponent(this);
            Components.Add(audioComponent);
            Services.AddService<AudioSystem.IAudioSystem>(audioComponent);

            var softwareCursor = new UI.SoftwareCursor(this);
            Components.Add(softwareCursor);
            Components.Add(new ToastNotification(this));

            base.Initialize();

            InitializeGameOptions();
            InitializeAudioSystem();

            gumProject = GumService.Default.Initialize(this, "Gum/Xenocide.gumx");
            ValidateGumx();

            screenManager.ScheduleScreen(new UI.Screens.StartScreen());

            var profileMax = GraphicsDevice.GraphicsProfile == GraphicsProfile.HiDef ? 4096 : 2048;
            Logger.Info("MaxTextureSize (profile): {0} ({1})", profileMax, GraphicsDevice.GraphicsProfile);
        }

        private void ValidateGumx()
        {
            if (gumProject == null) return;
            var screenDir = Path.Combine(Content.RootDirectory, "Gum", "Screens");
            foreach (var screen in gumProject.Screens)
            {
                var path = Path.Combine(screenDir, screen.Name + ".gusx");
                if (!File.Exists(path))
                    Logger.Warn("GumX validation: MISSING {0}", path);
            }
        }

        /// <summary>
        /// Load in the sound effects we're going to play
        /// </summary>
        public static void InitializeAudioSystem()
        {
            IAudioSystem audioSystem = Xenocide.AudioSystem;
            audioSystem.LoadSound(Assets.SoundId.PlanetViewSpeedFast);
            audioSystem.LoadSound(Assets.SoundId.PlanetViewSpeedSlow);
            audioSystem.LoadSound(Assets.SoundId.PlanetViewSpeedVeryFast);
            audioSystem.LoadSound(Assets.SoundId.PlanetViewZoomIn);
            audioSystem.LoadSound(Assets.SoundId.PlanetViewZoomOut);
            audioSystem.LoadSound(Assets.SoundId.PlanetViewClickObject);
            audioSystem.LoadSound(Assets.SoundId.ButtonClick1);
            audioSystem.LoadSound(Assets.SoundId.ButtonClick2);
            audioSystem.LoadSound(Assets.SoundId.ButtonOver);
            audioSystem.LoadSound(Assets.SoundId.ExitGame);
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
            ContentCache.PreloadGeoscapeContent(graphics.GraphicsDevice);

            var modelNames = Xenocide.StaticTables.XNetEntryList
                .Select(e => e.Graphic.Model).Distinct().ToList();
            ContentCache.PreloadXNetModels(screenManager.Content, modelNames);
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

            var keyState = Keyboard.GetState();
            if (keyState.IsKeyDown(Keys.LeftAlt) && keyState.IsKeyDown(Keys.Enter) &&
                (_prevKeyState.IsKeyUp(Keys.Enter) || _prevKeyState.IsKeyUp(Keys.LeftAlt)))
            {
                graphics.ToggleFullScreen();
            }
            _prevKeyState = keyState;

            GumService.Default.Update(gameTime);
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

            screenManager.Draw(gameTime, graphics.GraphicsDevice);
            GumService.Default.Draw();
            base.Draw(gameTime);
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
        public static GumProjectSave GumProject
        {
            get { return gumProject; }
        }

        public static IAudioSystem AudioSystem
        {
            get { return Instance.Services.GetService<IAudioSystem>(); }
        }

        public const string GameVersion = "0.4";

        public static bool DebugTesting { get; set; }

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
