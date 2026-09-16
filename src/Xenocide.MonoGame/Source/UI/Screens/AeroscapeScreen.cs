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
* @file AeroscapeScreen.cs
* @date Created: 2007/03/11
* @author File creator: dteviot
* @author Credits: none
*/
#endregion

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

using Gum.Forms;
using Gum.Forms.Controls;
using Gum.Wireframe;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using MonoGameGum.GueDeriving;

using NLog;

using ProjectXenocide.Assets;
using ProjectXenocide.Model.Battlescape;
using ProjectXenocide.Model.Geoscape.Vehicles;
using ProjectXenocide.UI.Controls;
using ProjectXenocide.UI.Dialogs;
using ProjectXenocide.Utils;

using Xenocide.Resources;

namespace ProjectXenocide.UI.Screens
{
    /// <summary>
    /// Dogfight screen for aerial combat between aircraft and UFOs.
    /// Displays a 2D radar viewport with real-time combat simulation,
    /// tactical mode controls, and HUD elements showing craft/UFO status.
    /// </summary>
    /// <remarks>
    /// ARCHITECTURE: Screen manages GUI (Gum controls + radar rendering) while
    /// AeroscapeSimulation handles all combat logic. Radar viewport is drawn
    /// via SpriteBatch in Draw(), overlaid with Gum HUD controls.
    ///
    /// VISUAL: 2D vertical interception track inspired by X-COM. The interceptor
    /// starts at the bottom, the UFO at the top, and they close toward the middle
    /// as the distance shrinks. Weapons fire instantly with a tracer.
    ///
    /// REAL-TIME: Combat runs continuously at adjustable speed (Pause/Normal/Fast).
    /// Weapons auto-fire on their cooldown timers. The player selects a tactical
    /// stance. Shortcuts: Space pause/resume, 1/2/3 speeds, D disengage, Tab next craft.
    /// </remarks>
    public partial class AeroscapeScreen : GumScreen
    {
        private static readonly Logger Logger = LogManager.GetLogger("Aeroscape");

        /// <summary>
        /// Initializes the dogfight screen for aerial combat.
        /// </summary>
        /// <param name="aircraft">The X-Corp aircraft engaging the UFO</param>
        /// <param name="ufo">The alien craft being intercepted</param>
        public AeroscapeScreen(Aircraft aircraft, Ufo ufo)
            : base("AeroscapeScreen")
        {
            this.aircraft = aircraft;
            this.ufo = ufo;

            // Create simulation state and engine
            var aircraftList = new List<Aircraft> { aircraft };
            this.simState = new AeroscapeState(ufo, aircraftList);
            this.simulation = new AeroscapeSimulation(simState);
            this.simulation.SetTacticalMode(0, TacticalMode.Standard);

            aircraft.OnDogfightStart();
            ufo.OnDogfightStart();

            // Log combatants
            Logger.Info("=== AEROSCAPE STARTED ===");
            Logger.Info("Interceptor: {0} (speed={1}m/s, hull={2}/{3}, fuel={4:F0}%)",
                aircraft.Name, aircraft.CraftItemInfo.MaxSpeed,
                aircraft.HullCapacity - aircraft.HullDamage, aircraft.HullCapacity,
                aircraft.FuelPercent);
            Logger.Info("UFO: {0} ({1}, speed={2}m/s, hull={3}/{4})",
                ufo.Name, ufo.UfoItemInfo.UfoSize, ufo.CraftItemInfo.MaxSpeed,
                ufo.HullCapacity - ufo.HullDamage, ufo.HullCapacity);
            Logger.Info("Interceptor weapons:");
            foreach (var pod in aircraft.WeaponPods)
            {
                if (pod != null)
                    Logger.Info("  {0}: acc={1}%, dmg={2}, range={3}m, ammo={4}",
                        pod.Name, pod.Weapon.Accuracy, pod.Weapon.WeaponDamage,
                        pod.WeaponRange, pod.UsesAmmo ? string.Format(CultureInfo.InvariantCulture, "{0}/{1}", pod.ShotsLeft, pod.ClipSize) : "unlimited");
            }
            Logger.Info("UFO weapons:");
            foreach (var pod in ufo.WeaponPods)
            {
                if (pod != null)
                    Logger.Info("  {0}: dmg={1}, range={2}m",
                        pod.Name, pod.Weapon.WeaponDamage, pod.WeaponRange);
            }

            // Initialize display interpolation
            displayDistance = AeroscapeState.MaxDistance;

            // Play aeroscape music
            Xenocide.AudioSystem?.PlayRandomMusic("Aeroscape");
        }

        /// <summary>
        /// Creates and wires all Gum controls from the .gusx layout.
        /// </summary>
        protected override void CreateGumControls()
        {
            // Wire tactical mode buttons
            WireButton("standoffBtn", OnStandoffButton);
            WireButton("cautiousBtn", OnCautiousButton);
            WireButton("standardBtn", OnStandardButton);
            WireButton("aggressiveBtn", OnAggressiveButton);
            WireButton("disengageBtn", OnDisengageButton);

            // Wire speed control buttons
            WireButton("pauseBtn", OnPauseButton);
            WireButton("normalBtn", OnNormalButton);
            WireButton("fastBtn", OnFastButton);

            // Wire weapon toggle buttons
            WireButton("weapon1ToggleBtn", OnWeapon1Toggle);
            WireButton("weapon2ToggleBtn", OnWeapon2Toggle);

            // Wire close button
            WireButton("closeBtn", OnCloseButton);

            // Populate HUD labels from Gum tree
            // Labels use BaseType="Text" in .gusx (visual-only), so use GetGraphicalUiElementByName
            if (GumRoot != null)
            {
                statusLabel = GumRoot.GetGraphicalUiElementByName("statusLabel");
                timeLabel = GumRoot.GetGraphicalUiElementByName("timeLabel");
                weapon1Label = GumRoot.GetGraphicalUiElementByName("weapon1Label");
                weapon1InfoLabel = GumRoot.GetGraphicalUiElementByName("weapon1InfoLabel");
                weapon1ToggleBtn = GumRoot.GetFrameworkElementByName<Button>("weapon1ToggleBtn");
                weapon2Label = GumRoot.GetGraphicalUiElementByName("weapon2Label");
                weapon2InfoLabel = GumRoot.GetGraphicalUiElementByName("weapon2InfoLabel");
                weapon2ToggleBtn = GumRoot.GetFrameworkElementByName<Button>("weapon2ToggleBtn");
                ufoNameLabel = GumRoot.GetGraphicalUiElementByName("ufoNameLabel");
                ufoHullLabel = GumRoot.GetGraphicalUiElementByName("ufoHullLabel");
                ufoWeaponLabel = GumRoot.GetGraphicalUiElementByName("ufoWeaponLabel");
                craftNameLabel = GumRoot.GetGraphicalUiElementByName("craftNameLabel");
                craftHullLabel = GumRoot.GetGraphicalUiElementByName("craftHullLabel");
                craftFuelLabel = GumRoot.GetGraphicalUiElementByName("craftFuelLabel");
                distanceLabel = GumRoot.GetGraphicalUiElementByName("distanceLabel");
                logLabel = GumRoot.GetGraphicalUiElementByName("logLabel");

                // Cache button references for per-frame updates
                pauseBtn = GumRoot.GetFrameworkElementByName<Button>("pauseBtn");
                normalBtn = GumRoot.GetFrameworkElementByName<Button>("normalBtn");
                fastBtn = GumRoot.GetFrameworkElementByName<Button>("fastBtn");
                standoffBtn = GumRoot.GetFrameworkElementByName<Button>("standoffBtn");
                cautiousBtn = GumRoot.GetFrameworkElementByName<Button>("cautiousBtn");
                standardBtn = GumRoot.GetFrameworkElementByName<Button>("standardBtn");
                aggressiveBtn = GumRoot.GetFrameworkElementByName<Button>("aggressiveBtn");
            }

            // Keep the fixed-coordinate .gusx HUD correct at other window sizes:
            // the right-hand panels hug the right edge and the status/log strip
            // hugs the bottom. At the 1280x1024 design resolution this is a no-op.
            MakeResponsive("TacticalPanel", 900, 50, 340, 180);
            MakeResponsive("WeaponPanel", 900, 300, 340, 170);
            MakeResponsive("UfoInfoPanel", 900, 500, 340, 110);
            MakeResponsive("StatusBar", 20, 700, 860, 30);
            MakeResponsive("LogPanel", 20, 730, 1240, 290);
            MakeResponsive("TopBar", 20, 10, 1240, 30);
            MakeResponsive("closeBtn", 1180, 10, 80, 30);

            // Themed backdrops behind the right-hand control panels and the
            // bottom status/log strip, so HUD text stays readable over the art.
            AddBackdrop(896, 46, 348, 566, 150);   // right column: tactical, weapons, target
            AddBackdrop(16, 696, 868, 38, 150);    // craft status bar
            AddBackdrop(16, 726, 1248, 298, 150);  // combat log

            // Start paused — player presses Normal or Fast to begin
            speedMultiplier = SpeedPaused;
            runRealTime = false;

            // Set initial display
            DrawScreen();
        }

        #region Fields

        private Aircraft aircraft;
        private Ufo ufo;
        private AeroscapeState simState;
        private AeroscapeSimulation simulation;

        // Speed control
        private bool runRealTime;
        private int speedMultiplier; // 0=paused, 1=normal, 3=fast
        private double elapsed;
        private bool isExiting;
        private bool returnScheduled;

        // Radar rendering resources
        private SpriteBatch spriteBatch;
        private Texture2D radarBackground;
        private Texture2D whiteTexture;
        private Texture2D craftIcon;
        private Texture2D ufoBlob;

        // HUD label references (populated from GumRoot)
        // NOTE: .gusx labels use BaseType="Text" (visual-only), not Forms Label.
        // Must use GetGraphicalUiElementByName() + SetProperty("Text",...) instead of .Text.
        private GraphicalUiElement statusLabel;
        private GraphicalUiElement timeLabel;
        private GraphicalUiElement weapon1Label;
        private GraphicalUiElement weapon1InfoLabel;
        private Button weapon1ToggleBtn;
        private GraphicalUiElement weapon2Label;
        private GraphicalUiElement weapon2InfoLabel;
        private Button weapon2ToggleBtn;
        private GraphicalUiElement ufoNameLabel;
        private GraphicalUiElement ufoHullLabel;
        private GraphicalUiElement ufoWeaponLabel;
        private GraphicalUiElement craftNameLabel;
        private GraphicalUiElement craftHullLabel;
        private GraphicalUiElement craftFuelLabel;
        private GraphicalUiElement distanceLabel;
        private GraphicalUiElement logLabel;

        // Cached button references (populated once in CreateGumControls)
        private Button pauseBtn;
        private Button normalBtn;
        private Button fastBtn;
        private Button standoffBtn;
        private Button cautiousBtn;
        private Button standardBtn;
        private Button aggressiveBtn;

        // Current tactical mode (for display)
        private TacticalMode currentTacticalMode = TacticalMode.Standard;

        // Keyboard state for Tab cycling
        private KeyboardState prevKeyboardState;

        // Smooth display interpolation
        private double displayDistance;          // smoothly interpolated distance for rendering
        private double displayInterpolation;     // fractional progress between prev and current tick

        #endregion

        #region Tactical Mode Handlers

        private void OnStandoffButton(object sender, EventArgs e)
        {
            currentTacticalMode = TacticalMode.Standoff;
            simulation.SetTacticalMode(simState.SelectedInterceptorIndex, TacticalMode.Standoff);
            DrawScreen();
        }

        private void OnCautiousButton(object sender, EventArgs e)
        {
            currentTacticalMode = TacticalMode.Cautious;
            simulation.SetTacticalMode(simState.SelectedInterceptorIndex, TacticalMode.Cautious);
            DrawScreen();
        }

        private void OnStandardButton(object sender, EventArgs e)
        {
            currentTacticalMode = TacticalMode.Standard;
            simulation.SetTacticalMode(simState.SelectedInterceptorIndex, TacticalMode.Standard);
            DrawScreen();
        }

        private void OnAggressiveButton(object sender, EventArgs e)
        {
            currentTacticalMode = TacticalMode.Aggressive;
            simulation.SetTacticalMode(simState.SelectedInterceptorIndex, TacticalMode.Aggressive);
            DrawScreen();
        }

        private void OnDisengageButton(object sender, EventArgs e)
        {
            simulation.DisengageInterceptor(simState.SelectedInterceptorIndex);
            DrawScreen();
        }

        #endregion

        #region Speed Control Handlers

        /// <summary>Speed multipliers: 0 = paused, 1 = normal, 3 = fast.</summary>
        private const int SpeedPaused = 0;
        private const int SpeedNormal = 1;
        private const int SpeedFast = 3;

        /// <summary>Sets the simulation speed and refreshes the speed buttons.</summary>
        private void SetSpeed(int multiplier)
        {
            speedMultiplier = multiplier;
            runRealTime = multiplier > 0;
            DrawScreen();
        }

        private void OnPauseButton(object sender, EventArgs e) => SetSpeed(SpeedPaused);

        private void OnNormalButton(object sender, EventArgs e) => SetSpeed(SpeedNormal);

        private void OnFastButton(object sender, EventArgs e) => SetSpeed(SpeedFast);

        #endregion

        #region Weapon Toggle Handlers

        private void OnWeapon1Toggle(object sender, EventArgs e)
        {
            simulation.ToggleWeapon(simState.SelectedInterceptorIndex, 0);
            DrawScreen();
        }

        private void OnWeapon2Toggle(object sender, EventArgs e)
        {
            simulation.ToggleWeapon(simState.SelectedInterceptorIndex, 1);
            DrawScreen();
        }

        #endregion

        #region Close / Escape

        private void OnCloseButton(object sender, EventArgs e)
        {
            GoToGeoscape();
        }

        /// <summary>
        /// Handle Escape key press - return to geoscape.
        /// </summary>
        public override bool HandleEscape()
        {
            GoToGeoscape();
            return true;
        }

        private void GoToGeoscape()
        {
            if (isExiting)
                return;
            isExiting = true;

            // Log result
            string resultStr = GetOutcomeString(simState.Outcome);
            Logger.Info("=== AEROSCAPE ENDED: {0} ===", resultStr);
            Logger.Info("Elapsed: {0:F0}s, Distance: {1:F0}m", simState.ElapsedSeconds, simState.Distance);
            Logger.Info("UFO: {0} (hull {1:F0}%, health {2:F0}/{3:F0})", ufo.Name, ufo.HullPercent,
                ufo.HullCapacity - ufo.HullDamage, ufo.HullCapacity);
            Logger.Info("Interceptor: {0} (hull {1:F0}%, health {2:F0}/{3:F0}, fuel {4:F0}%)",
                aircraft.Name, aircraft.HullPercent,
                aircraft.HullCapacity - aircraft.HullDamage, aircraft.HullCapacity,
                aircraft.FuelPercent);
            Logger.Info("Outcome: {0}", simState.Outcome);

            EndDogfight();

            // Show a short debrief before returning, then leave the fight.
            var report = new GumMessageBoxDialog(BuildDebriefText(), "Dogfight Report");
            report.OkAction = ScheduleAfterDogfight;
            ScreenManager.ShowDialog(report);
        }

        /// <summary>
        /// Schedules the screen to return to once the debrief is acknowledged.
        /// Guarded so that a single acknowledgement can only ever queue one
        /// transition, even if the click is delivered more than once.
        /// </summary>
        private void ScheduleAfterDogfight()
        {
            if (returnScheduled)
                return;
            returnScheduled = true;

            if (Xenocide.DebugTesting)
            {
                Xenocide.DebugTesting = false;
                ScreenManager.ScheduleScreen(new StartScreen());
            }
            else
            {
                ScreenManager.ScheduleScreen(new GeoscapeScreen());
            }
        }

        /// <summary>Formats the post-battle summary.</summary>
        private string BuildDebriefText()
        {
            var sb = new StringBuilder();
            sb.AppendLine("Result: " + GetOutcomeString(simState.Outcome));
            sb.AppendLine(string.Format(CultureInfo.InvariantCulture, "Time: {0:F0}s    Distance: {1}km",
                simState.ElapsedSeconds, (int)(simState.Distance / 1000.0)));
            sb.AppendLine(string.Format(CultureInfo.InvariantCulture, "{0}: Hull {1}%  Fuel {2}%",
                aircraft.Name, aircraft.HullPercent, aircraft.FuelPercent));
            sb.AppendLine(string.Format(CultureInfo.InvariantCulture, "{0}: Hull {1}%",
                ufo.Name, ufo.HullPercent));
            return sb.ToString().TrimEnd();
        }

        private static string GetOutcomeString(DogfightOutcome outcome)
        {
            switch (outcome)
            {
                case DogfightOutcome.InProgress: return "IN PROGRESS";
                case DogfightOutcome.AircraftVictory: return "AIRCRAFT VICTORY";
                case DogfightOutcome.AircraftDestroyed: return "AIRCRAFT DESTROYED";
                case DogfightOutcome.AircraftRetreated: return "AIRCRAFT RETREATED";
                case DogfightOutcome.UFOEscaped: return "UFO ESCAPED";
                default: return "UNKNOWN";
            }
        }

        /// <summary>
        /// End the dogfight and clean up vehicle states.
        /// </summary>
        private void EndDogfight()
        {
            if (!ufo.IsDestroyed)
                ufo.OnDogfightFinished();
            if (!aircraft.IsDestroyed)
                aircraft.OnDogfightFinished();
        }

        #endregion

        #region Game Loop

        /// <summary>
        /// Update game logic each frame.
        /// </summary>
        public override void Update(GameTime gameTime)
        {
            // Handle keyboard input
            HandleKeyboardInput();

            if (runRealTime && !simulation.IsDogfightOver)
            {
                elapsed += gameTime.ElapsedGameTime.TotalMilliseconds;
                double tickInterval = 1000.0 / speedMultiplier;
                while (elapsed >= tickInterval)
                {
                    elapsed -= tickInterval;
                    UpdateDogfight();
                }

                // Smooth interpolation: blend between previous and current distance
                if (tickInterval > 0)
                {
                    displayInterpolation = elapsed / tickInterval;
                    displayDistance = simState.PrevDistance +
                        (simState.Distance - simState.PrevDistance) * displayInterpolation;
                }
                else
                {
                    displayDistance = simState.Distance;
                }
            }
            else
            {
                // Not running: snap to actual distance
                displayDistance = simState.Distance;
            }

            // Check if dogfight ended
            if (simulation.IsDogfightOver && !isExiting)
            {
                GoToGeoscape();
            }
        }

        /// <summary>
        /// Render the screen: radar viewport + Gum HUD overlay.
        /// </summary>
        public override void Draw(GameTime gameTime, GraphicsDevice device)
        {
            // Draw radar viewport first (behind Gum controls)
            DrawRadarViewport(device);

            // Draw Gum HUD overlay
            base.Draw(gameTime, device);
        }

        /// <summary>
        /// Load content for radar rendering.
        /// </summary>
        public override void LoadContent(ContentManager content, GraphicsDevice device)
        {
            base.LoadContent(content, device);

            spriteBatch = new SpriteBatch(device);

            // Create programmatic textures for radar elements
            radarBackground = CreateSolidTexture(device, new Color(10, 30, 10));
            whiteTexture = CreateSolidTexture(device, Color.White);
            craftIcon = CreateTriangleTexture(device, Color.LimeGreen, 24, 24);
            ufoBlob = CreateCircleTexture(device, Color.Red, 32);
        }

        /// <summary>
        /// Clean up rendering resources.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                spriteBatch?.Dispose();
                radarBackground?.Dispose();
                craftIcon?.Dispose();
                ufoBlob?.Dispose();
                whiteTexture?.Dispose();
            }
            base.Dispose(disposing);
        }

        #endregion

        #region Keyboard Input

        private void HandleKeyboardInput()
        {
            KeyboardState keyboard = Keyboard.GetState();

            // Tab: cycle to next interceptor
            if (keyboard.IsKeyDown(Keys.Tab) && prevKeyboardState.IsKeyUp(Keys.Tab))
            {
                simulation.SelectNextInterceptor();
                // Sync tactical mode display to newly selected interceptor
                var selected = simState.SelectedInterceptor;
                if (selected != null)
                    currentTacticalMode = selected.Mode;
                DrawScreen();
            }

            // Space: toggle pause / resume
            if (keyboard.IsKeyDown(Keys.Space) && prevKeyboardState.IsKeyUp(Keys.Space))
            {
                SetSpeed(runRealTime ? SpeedPaused : SpeedNormal);
            }

            // 1 / 2 / 3: pause / normal / fast
            if (keyboard.IsKeyDown(Keys.D1) && prevKeyboardState.IsKeyUp(Keys.D1))
                SetSpeed(SpeedPaused);
            else if (keyboard.IsKeyDown(Keys.D2) && prevKeyboardState.IsKeyUp(Keys.D2))
                SetSpeed(SpeedNormal);
            else if (keyboard.IsKeyDown(Keys.D3) && prevKeyboardState.IsKeyUp(Keys.D3))
                SetSpeed(SpeedFast);

            // D: order the selected interceptor to disengage
            if (keyboard.IsKeyDown(Keys.D) && prevKeyboardState.IsKeyUp(Keys.D))
            {
                simulation.DisengageInterceptor(simState.SelectedInterceptorIndex);
                DrawScreen();
            }

            prevKeyboardState = keyboard;
        }

        #endregion

        /// <summary>Repositions a named .gusx element for the current window size.</summary>
        private void MakeResponsive(string name, int designX, int designY, int width, int height)
        {
            if (GumRoot != null)
                ResponsiveHud.Position(GumRoot.GetGraphicalUiElementByName(name), designX, designY, width, height);
        }

        /// <summary>
        /// Adds a themed panel (1px border + translucent fill) behind the HUD to
        /// keep the labels readable over the background art.
        /// </summary>
        private void AddBackdrop(int x, int y, int width, int height, int alpha)
        {
            if (GumRoot == null)
                return;

            var border = new ColoredRectangleRuntime();
            border.Color = new Color(60, 90, 60, 210);
            ResponsiveHud.Position(border, x - 1, y - 1, width + 2, height + 2);

            var fill = new ColoredRectangleRuntime();
            fill.Color = new Color(8, 20, 12, alpha);
            ResponsiveHud.Position(fill, x, y, width, height);

            // Insert after the background sprite so the HUD panels stay on top.
            int index = Math.Min(1, GumRoot.Children.Count);
            GumRoot.Children.Insert(index, border);
            GumRoot.Children.Insert(index + 1, fill);
        }

        #region Radar Rendering

        /// <summary>How long (in simulation seconds) a weapon tracer is shown.</summary>
        private const double TracerLifetime = 0.5;

        /// <summary>
        /// Draws the interception track: distance scale, weapon range band, the
        /// craft and UFO (with hull/fuel bars) and weapon tracers. Geometry is
        /// derived from the viewport so it scales with the window resolution.
        /// </summary>
        private void DrawRadarViewport(GraphicsDevice device)
        {
            if (spriteBatch == null || whiteTexture == null)
                return;

            var vp = device.Viewport;
            int radarX = 20;
            int radarY = 50;
            int radarWidth = Math.Max(240, vp.Width - radarX - 380);
            int radarHeight = Math.Max(240, vp.Height - radarY - 330);
            int centerX = radarX + radarWidth / 2;
            int centerY = radarY + radarHeight / 2;

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);

            // Backdrop + border
            spriteBatch.Draw(radarBackground, new Rectangle(radarX, radarY, radarWidth, radarHeight), Color.White);
            DrawBorder(radarX, radarY, radarWidth, radarHeight, new Color(60, 90, 60), 2);

            DrawDistanceScale(radarX, radarY, radarWidth, radarHeight);

            var interceptor = simState.SelectedInterceptor;
            float normalized = (float)Math.Max(0, Math.Min(1, displayDistance / AeroscapeState.MaxDistance));
            int craftY = (int)(centerY + (radarHeight * normalized / 2f));
            int ufoY = (int)(centerY - (radarHeight * normalized / 2f));

            // Engagement band: the two craft are within weapon range while the
            // closing distance keeps them inside this central zone. It is centred
            // on the meeting point because distance maps to the vertical gap.
            if (interceptor != null)
            {
                int maxRange = AeroscapeState.GetMaxWeaponRange(interceptor);
                if (maxRange > 0 && maxRange < AeroscapeState.MaxDistance)
                {
                    float rangeN = (float)((double)maxRange / AeroscapeState.MaxDistance);
                    int bandHalf = (int)(radarHeight * rangeN / 2f);
                    int bandTop = Math.Max(radarY + 1, centerY - bandHalf);
                    int bandBottom = Math.Min(radarY + radarHeight - 1, centerY + bandHalf);
                    var bandColor = new Color(120, 150, 0, 40);

                    spriteBatch.Draw(whiteTexture,
                        new Rectangle(radarX + 1, bandTop, radarWidth - 2, Math.Max(0, bandBottom - bandTop)),
                        bandColor);
                    spriteBatch.Draw(whiteTexture,
                        new Rectangle(radarX + 1, bandTop, radarWidth - 2, 1), new Color(150, 180, 0, 140));
                    spriteBatch.Draw(whiteTexture,
                        new Rectangle(radarX + 1, bandBottom, radarWidth - 2, 1), new Color(150, 180, 0, 140));
                }
            }

            DrawFlashes(centerX, craftY, ufoY);
            PruneFlashes();

            // Craft icons
            spriteBatch.Draw(craftIcon, new Rectangle(centerX - 12, craftY - 12, 24, 24), Color.LimeGreen);
            spriteBatch.Draw(ufoBlob, new Rectangle(centerX - 16, ufoY - 16, 32, 32), Color.Red);

            // Hull / fuel bars
            if (interceptor != null)
            {
                DrawBar(centerX - 42, craftY + 16, 84, 5, interceptor.Aircraft.HullPercent, Color.LimeGreen);
                DrawBar(centerX - 42, craftY + 23, 84, 5, interceptor.Aircraft.FuelPercent, Color.Gold);
            }
            DrawBar(centerX - 42, ufoY - 27, 84, 5, ufo.HullPercent, Color.OrangeRed);

            spriteBatch.End();
        }

        /// <summary>Draws distance tick marks down both edges of the track.</summary>
        private void DrawDistanceScale(int x, int y, int w, int h)
        {
            const double tickMeters = 5000.0;
            Color minor = new Color(30, 70, 30);
            Color major = new Color(60, 120, 60);

            for (double d = tickMeters; d < AeroscapeState.MaxDistance; d += tickMeters)
            {
                float n = (float)(d / AeroscapeState.MaxDistance);
                int lineY = (int)(y + (h * n));
                bool isMajor = ((int)(d / 10000.0)) * 10000 == (int)d;
                int length = isMajor ? w / 12 : w / 20;
                Color color = isMajor ? major : minor;

                spriteBatch.Draw(whiteTexture, new Rectangle(x + 1, lineY, length, 1), color);
                spriteBatch.Draw(whiteTexture, new Rectangle(x + w - length - 1, lineY, length, 1), color);
            }
        }

        private void DrawBar(int x, int y, int width, int height, int percent, Color color)
        {
            percent = Math.Max(0, Math.Min(100, percent));
            spriteBatch.Draw(whiteTexture, new Rectangle(x, y, width, height), new Color(0, 0, 0, 160));
            spriteBatch.Draw(whiteTexture, new Rectangle(x, y, width * percent / 100, height), color);
        }

        private void DrawBorder(int x, int y, int w, int h, Color color, int thickness)
        {
            spriteBatch.Draw(whiteTexture, new Rectangle(x, y, w, thickness), color);
            spriteBatch.Draw(whiteTexture, new Rectangle(x, y + h - thickness, w, thickness), color);
            spriteBatch.Draw(whiteTexture, new Rectangle(x, y, thickness, h), color);
            spriteBatch.Draw(whiteTexture, new Rectangle(x + w - thickness, y, thickness, h), color);
        }

        private void DrawFlashes(int centerX, int craftY, int ufoY)
        {
            foreach (var flash in simState.Flashes)
            {
                if (simState.ElapsedSeconds - flash.Time > TracerLifetime)
                    continue;

                Color color = flash.FromInterceptor
                    ? (flash.Hit ? Color.Orange : new Color(160, 140, 60))
                    : (flash.Hit ? Color.Red : new Color(140, 60, 60));

                DrawLine(new Vector2(centerX, craftY), new Vector2(centerX, ufoY), color, 2f);
            }
        }

        private void DrawLine(Vector2 a, Vector2 b, Color color, float thickness)
        {
            Vector2 delta = b - a;
            float length = delta.Length();
            if (length < 0.01f)
                return;

            float angle = (float)Math.Atan2(delta.Y, delta.X);
            spriteBatch.Draw(whiteTexture, a, null, color, angle, Vector2.Zero,
                new Vector2(length, thickness), SpriteEffects.None, 0f);
        }

        private void PruneFlashes()
        {
            simState.Flashes.RemoveAll(f => simState.ElapsedSeconds - f.Time > TracerLifetime);
        }

        /// <summary>
        /// Create a solid color texture.
        /// </summary>
        private static Texture2D CreateSolidTexture(GraphicsDevice device, Color color)
        {
            var texture = new Texture2D(device, 1, 1);
            texture.SetData(new[] { color });
            return texture;
        }

        /// <summary>
        /// Create a simple triangle texture for aircraft icon.
        /// </summary>
        private static Texture2D CreateTriangleTexture(GraphicsDevice device, Color color, int width, int height)
        {
            var texture = new Texture2D(device, width, height);
            var data = new Color[width * height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float centerX = width / 2f;
                    float progress = y / (float)height;
                    float halfWidth = progress * (width / 2f);

                    if (x >= centerX - halfWidth && x <= centerX + halfWidth)
                        data[y * width + x] = color;
                    else
                        data[y * width + x] = Color.Transparent;
                }
            }

            texture.SetData(data);
            return texture;
        }

        /// <summary>
        /// Create a simple circle texture for UFO blob.
        /// </summary>
        private static Texture2D CreateCircleTexture(GraphicsDevice device, Color color, int size)
        {
            var texture = new Texture2D(device, size, size);
            var data = new Color[size * size];
            float center = size / 2f;
            float radius = size / 2f - 1;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - center;
                    float dy = y - center;
                    float dist = (float)Math.Sqrt(dx * dx + dy * dy);

                    if (dist <= radius)
                        data[y * size + x] = color;
                    else
                        data[y * size + x] = Color.Transparent;
                }
            }

            texture.SetData(data);
            return texture;
        }

        #endregion

        #region HUD Updates

        /// <summary>
        /// Advance the dogfight by one tick and refresh all displays.
        /// </summary>
        private void UpdateDogfight()
        {
            simulation.Tick(1.0);
            DrawScreen();
        }

        /// <summary>
        /// Refresh all HUD elements from current game state.
        /// Called only from button handlers and keyboard shortcuts (not per-frame),
        /// so string allocations here are at user-interaction rate, not frame rate.
        /// </summary>
        private void DrawScreen()
        {
            UpdateStatusLabel();
            UpdateTimeDisplay();
            UpdateDistanceDisplay();
            UpdateAircraftStatus();
            UpdateWeaponInfo();
            UpdateUfoInfo();
            UpdateCombatLog();
            UpdateSpeedButtons();
            UpdateTacticalButtons();
        }

        private void UpdateStatusLabel()
        {
            statusLabel?.SetProperty("Text", GetTacticalModeName(currentTacticalMode));
        }

        private void UpdateTimeDisplay()
        {
            timeLabel?.SetProperty("Text", string.Format(CultureInfo.InvariantCulture, "Time: {0:F0}s", simState.ElapsedSeconds));
        }

        private void UpdateDistanceDisplay()
        {
            int distanceKm = (int)(simState.Distance / 1000.0);
            distanceLabel?.SetProperty("Text", string.Format(CultureInfo.InvariantCulture, "Distance: {0}km", distanceKm));
        }

        private void UpdateAircraftStatus()
        {
            craftNameLabel?.SetProperty("Text", aircraft.Name);
            craftHullLabel?.SetProperty("Text", string.Format(CultureInfo.InvariantCulture, "Hull: {0}%", aircraft.HullPercent));
            craftFuelLabel?.SetProperty("Text", string.Format(CultureInfo.InvariantCulture, "Fuel: {0}%", aircraft.FuelPercent));
        }

        private void UpdateWeaponInfo()
        {
            var interceptor = simState.SelectedInterceptor;
            if (interceptor == null)
                return;

            DrawPodInformation(weapon1Label, weapon1InfoLabel, weapon1ToggleBtn, interceptor, 0, interceptor.Weapon1Enabled);
            DrawPodInformation(weapon2Label, weapon2InfoLabel, weapon2ToggleBtn, interceptor, 1, interceptor.Weapon2Enabled);
        }

        private static void DrawPodInformation(GraphicalUiElement headerLabel, GraphicalUiElement infoLabel, Button toggleBtn,
            InterceptorState interceptor, int podIndex, bool isEnabled)
        {
            if (headerLabel == null || infoLabel == null)
                return;

            if (podIndex < interceptor.Aircraft.WeaponPods.Count && interceptor.Aircraft.WeaponPods[podIndex] != null)
            {
                var pod = interceptor.Aircraft.WeaponPods[podIndex];
                headerLabel.SetProperty("Text", string.Format(CultureInfo.InvariantCulture, "WEAPON {0}: {1}", podIndex + 1, pod.Name));

                string ammoText = pod.UsesAmmo
                    ? string.Format(CultureInfo.InvariantCulture, "Ammo: {0}/{1}", pod.ShotsLeft, pod.ClipSize)
                    : "Ammo: Unlimited";

                infoLabel.SetProperty("Text", string.Format(CultureInfo.InvariantCulture, "{0}  Range: {1}km  Dmg: {2}",
                    ammoText, pod.WeaponRange / 1000, pod.WeaponDamage));

                if (toggleBtn != null)
                {
                    toggleBtn.Text = isEnabled ? "ON" : "OFF";
                }
            }
            else
            {
                headerLabel.SetProperty("Text", string.Format(CultureInfo.InvariantCulture, "WEAPON {0}: Empty", podIndex + 1));
                infoLabel.SetProperty("Text", "");
                if (toggleBtn != null)
                {
                    toggleBtn.Text = "N/A";
                }
            }
        }

        private void UpdateUfoInfo()
        {
            ufoNameLabel?.SetProperty("Text", string.Format(CultureInfo.InvariantCulture, "{0} ({1})", ufo.Name, ufo.UfoItemInfo.UfoSize));
            ufoHullLabel?.SetProperty("Text", string.Format(CultureInfo.InvariantCulture, "Hull: {0}%", ufo.HullPercent));

            if (ufo.WeaponPods.Count > 0 && ufo.WeaponPods[0] != null)
                ufoWeaponLabel?.SetProperty("Text", string.Format(CultureInfo.InvariantCulture, "Weapon: {0}", ufo.WeaponPods[0].Name));
            else
                ufoWeaponLabel?.SetProperty("Text", "Weapon: None");
        }

        private void UpdateCombatLog()
        {
            if (logLabel == null)
                return;

            StringBuilder sb = new StringBuilder();
            int startIdx = Math.Max(0, simState.Log.Entries.Count - 8);
            for (int i = startIdx; i < simState.Log.Entries.Count; i++)
            {
                if (sb.Length > 0)
                    sb.Append(Util.Linefeed);
                sb.Append(simState.Log.Entries[i].Details);
            }

            logLabel.SetProperty("Text", sb.ToString());
        }

        private void UpdateSpeedButtons()
        {
            if (pauseBtn != null)
                pauseBtn.Text = (speedMultiplier == 0) ? "[Pause]" : "Pause";
            if (normalBtn != null)
                normalBtn.Text = (speedMultiplier == 1) ? "[Normal]" : "Normal";
            if (fastBtn != null)
                fastBtn.Text = (speedMultiplier == 3) ? "[Fast]" : "Fast";
        }

        private void UpdateTacticalButtons()
        {
            if (standoffBtn != null)
                standoffBtn.Text = (currentTacticalMode == TacticalMode.Standoff) ? "[STANDOFF]" : "STANDOFF";
            if (cautiousBtn != null)
                cautiousBtn.Text = (currentTacticalMode == TacticalMode.Cautious) ? "[CAUTIOUS]" : "CAUTIOUS";
            if (standardBtn != null)
                standardBtn.Text = (currentTacticalMode == TacticalMode.Standard) ? "[STANDARD]" : "STANDARD";
            if (aggressiveBtn != null)
                aggressiveBtn.Text = (currentTacticalMode == TacticalMode.Aggressive) ? "[AGGRESSIVE]" : "AGGRESSIVE";
        }

        /// <summary>
        /// Get display name for a tactical mode.
        /// </summary>
        private static string GetTacticalModeName(TacticalMode mode)
        {
            switch (mode)
            {
                case TacticalMode.Standoff: return "STANDOFF";
                case TacticalMode.Cautious: return "CAUTIOUS ATTACK";
                case TacticalMode.Standard: return "STANDARD ATTACK";
                case TacticalMode.Aggressive: return "AGGRESSIVE ATTACK";
                case TacticalMode.Disengage: return "DISENGAGING";
                default: return "UNKNOWN";
            }
        }

        #endregion
    }
}
