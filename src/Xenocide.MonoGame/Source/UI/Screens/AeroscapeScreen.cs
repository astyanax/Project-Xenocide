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
using System.Text;

using Gum.Forms;
using Gum.Forms.Controls;
using Gum.Wireframe;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using NLog;

using ProjectXenocide.Assets;
using ProjectXenocide.Model.Battlescape;
using ProjectXenocide.Model.Geoscape.Vehicles;
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
    /// VISUAL: 2D side-view radar inspired by OpenXCOM. Aircraft at bottom,
    /// UFO at top, distance mapped vertically. Weapons fire as projectiles.
    ///
    /// REAL-TIME: Combat runs continuously at adjustable speed (Pause/1x/2x).
    /// Weapons auto-fire on their cooldown timers. Player selects tactical mode.
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
                        pod.WeaponRange, pod.UsesAmmo ? string.Format("{0}/{1}", pod.ShotsLeft, pod.ClipSize) : "unlimited");
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
            }

            // Start paused — player presses Normal or Fast to begin
            speedMultiplier = 0;
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
        private int speedMultiplier; // 0=paused, 1=normal, 2=fast
        private double elapsed;
        private bool isExiting;

        // Radar rendering resources
        private SpriteBatch spriteBatch;
        private Texture2D radarBackground;
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

        private void OnPauseButton(object sender, EventArgs e)
        {
            speedMultiplier = 0;
            runRealTime = false;
            DrawScreen();
        }

        private void OnNormalButton(object sender, EventArgs e)
        {
            speedMultiplier = 1;
            runRealTime = true;
            DrawScreen();
        }

        private void OnFastButton(object sender, EventArgs e)
        {
            speedMultiplier = 3;
            runRealTime = true;
            DrawScreen();
        }

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

            // Space: toggle pause
            if (keyboard.IsKeyDown(Keys.Space) && prevKeyboardState.IsKeyUp(Keys.Space))
            {
                if (runRealTime)
                {
                    speedMultiplier = 0;
                    runRealTime = false;
                }
                else
                {
                    speedMultiplier = 1;
                    runRealTime = true;
                }
                DrawScreen();
            }

            prevKeyboardState = keyboard;
        }

        #endregion

        #region Radar Rendering

        /// <summary>
        /// Draw the 2D radar viewport showing aircraft, UFO, and weapon fire.
        /// Uses smooth interpolated positions for artifact-free animation.
        /// </summary>
        private void DrawRadarViewport(GraphicsDevice device)
        {
            if (spriteBatch == null || radarBackground == null)
                return;

            // Define radar viewport area (left 70% of screen, below top bar)
            int radarX = 20;
            int radarY = 50;
            int radarWidth = 860;
            int radarHeight = 640;

            spriteBatch.Begin();

            // Draw radar background
            spriteBatch.Draw(radarBackground, new Rectangle(radarX, radarY, radarWidth, radarHeight), Color.White);

            // Draw radar grid lines (horizontal distance markers)
            DrawRadarGrid(spriteBatch, radarX, radarY, radarWidth, radarHeight);

            // Draw weapon range indicators
            DrawWeaponRanges(spriteBatch, radarX, radarY, radarWidth, radarHeight);

            // Use smooth interpolated distance for positions
            double renderDistance = displayDistance;

            // Both icons are centered in the radar, converging toward the middle.
            // As distance decreases, they approach the center line but never cross it.
            float normalized = (float)(renderDistance / AeroscapeState.MaxDistance);
            int centerX = radarX + (radarWidth / 2);

            // Interceptor: starts at BOTTOM (far), ascends toward CENTER as it closes.
            // At n=1 (MaxDistance): Y = radarY + radarHeight (bottom)
            // At n=0 (contact):      Y = radarY + radarHeight/2 (center)
            float craftY = radarY + (radarHeight / 2f) + (radarHeight * normalized / 2f);
            spriteBatch.Draw(craftIcon,
                new Rectangle(centerX - 12, (int)craftY - 12, 24, 24),
                Color.LimeGreen);

            // UFO: starts at TOP (far), descends toward CENTER as interceptor closes.
            // At n=1 (MaxDistance): Y = radarY (top)
            // At n=0 (contact):      Y = radarY + radarHeight/2 (center)
            float ufoY = radarY + (radarHeight / 2f) - (radarHeight * normalized / 2f);
            spriteBatch.Draw(ufoBlob,
                new Rectangle(centerX - 16, (int)ufoY - 16, 32, 32),
                Color.Red);

            spriteBatch.End();
        }

        /// <summary>
        /// Draw horizontal grid lines for distance reference.
        /// </summary>
        private void DrawRadarGrid(SpriteBatch sb, int x, int y, int w, int h)
        {
            // Draw subtle grid lines every 20% of height
            Color gridColor = new Color(20, 60, 20);
            for (int i = 1; i < 5; i++)
            {
                int lineY = y + (h * i / 5);
                sb.Draw(radarBackground, new Rectangle(x, lineY, w, 1), gridColor);
            }
        }

        /// <summary>
        /// Draw weapon range indicator lines on the radar.
        /// Shows maximum reachable firing distance from the interceptor.
        /// </summary>
        private void DrawWeaponRanges(SpriteBatch sb, int x, int y, int w, int h)
        {
            var interceptor = simState.SelectedInterceptor;
            if (interceptor == null)
                return;

            int maxRange = AeroscapeState.GetMaxWeaponRange(interceptor);
            if (maxRange <= 0)
                return;

            // Position the range line at the interceptor's Y when distance = maxRange
            float normalized = (float)((double)maxRange / AeroscapeState.MaxDistance);
            int rangeLineY = y + (int)((h / 2f) + (h * normalized / 2f));

            // Draw range line
            Color rangeColor = new Color(100, 100, 0, 128);
            sb.Draw(radarBackground, new Rectangle(x, rangeLineY, w, 1), rangeColor);
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
            timeLabel?.SetProperty("Text", string.Format("Time: {0:F0}s", simState.ElapsedSeconds));
        }

        private void UpdateDistanceDisplay()
        {
            int distanceKm = (int)(simState.Distance / 1000.0);
            distanceLabel?.SetProperty("Text", string.Format("Distance: {0}km", distanceKm));
        }

        private void UpdateAircraftStatus()
        {
            craftNameLabel?.SetProperty("Text", aircraft.Name);
            craftHullLabel?.SetProperty("Text", string.Format("Hull: {0}%", aircraft.HullPercent));
            craftFuelLabel?.SetProperty("Text", string.Format("Fuel: {0}%", aircraft.FuelPercent));
        }

        private void UpdateWeaponInfo()
        {
            var interceptor = simState.SelectedInterceptor;
            if (interceptor == null)
                return;

            DrawPodInformation(weapon1Label, weapon1InfoLabel, weapon1ToggleBtn, interceptor, 0, interceptor.Weapon1Enabled);
            DrawPodInformation(weapon2Label, weapon2InfoLabel, weapon2ToggleBtn, interceptor, 1, interceptor.Weapon2Enabled);
        }

        private void DrawPodInformation(GraphicalUiElement headerLabel, GraphicalUiElement infoLabel, Button toggleBtn,
            InterceptorState interceptor, int podIndex, bool isEnabled)
        {
            if (headerLabel == null || infoLabel == null)
                return;

            if (podIndex < interceptor.Aircraft.WeaponPods.Count && interceptor.Aircraft.WeaponPods[podIndex] != null)
            {
                var pod = interceptor.Aircraft.WeaponPods[podIndex];
                headerLabel.SetProperty("Text", string.Format("WEAPON {0}: {1}", podIndex + 1, pod.Name));

                string ammoText = pod.UsesAmmo
                    ? string.Format("Ammo: {0}/{1}", pod.ShotsLeft, pod.ClipSize)
                    : "Ammo: Unlimited";

                infoLabel.SetProperty("Text", string.Format("{0}  Range: {1}km  Dmg: {2}",
                    ammoText, pod.WeaponRange / 1000, pod.WeaponDamage));

                if (toggleBtn != null)
                {
                    toggleBtn.Text = isEnabled ? "ON" : "OFF";
                }
            }
            else
            {
                headerLabel.SetProperty("Text", string.Format("WEAPON {0}: Empty", podIndex + 1));
                infoLabel.SetProperty("Text", "");
                if (toggleBtn != null)
                {
                    toggleBtn.Text = "N/A";
                }
            }
        }

        private void UpdateUfoInfo()
        {
            ufoNameLabel?.SetProperty("Text", string.Format("{0} ({1})", ufo.Name, ufo.UfoItemInfo.UfoSize));
            ufoHullLabel?.SetProperty("Text", string.Format("Hull: {0}%", ufo.HullPercent));

            if (ufo.WeaponPods.Count > 0 && ufo.WeaponPods[0] != null)
                ufoWeaponLabel?.SetProperty("Text", string.Format("Weapon: {0}", ufo.WeaponPods[0].Name));
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
            if (GumRoot == null) return;
            var pause = GumRoot.GetFrameworkElementByName<Button>("pauseBtn");
            var normal = GumRoot.GetFrameworkElementByName<Button>("normalBtn");
            var fast = GumRoot.GetFrameworkElementByName<Button>("fastBtn");
            if (pause != null)
                pause.Text = (speedMultiplier == 0) ? "[Pause]" : "Pause";
            if (normal != null)
                normal.Text = (speedMultiplier == 1) ? "[Normal]" : "Normal";
            if (fast != null)
                fast.Text = (speedMultiplier == 3) ? "[Fast]" : "Fast";
        }

        private void UpdateTacticalButtons()
        {
            if (GumRoot == null) return;
            var standoff = GumRoot.GetFrameworkElementByName<Button>("standoffBtn");
            var cautious = GumRoot.GetFrameworkElementByName<Button>("cautiousBtn");
            var standard = GumRoot.GetFrameworkElementByName<Button>("standardBtn");
            var aggressive = GumRoot.GetFrameworkElementByName<Button>("aggressiveBtn");
            if (standoff != null)
                standoff.Text = (currentTacticalMode == TacticalMode.Standoff) ? "[STANDOFF]" : "STANDOFF";
            if (cautious != null)
                cautious.Text = (currentTacticalMode == TacticalMode.Cautious) ? "[CAUTIOUS]" : "CAUTIOUS";
            if (standard != null)
                standard.Text = (currentTacticalMode == TacticalMode.Standard) ? "[STANDARD]" : "STANDARD";
            if (aggressive != null)
                aggressive.Text = (currentTacticalMode == TacticalMode.Aggressive) ? "[AGGRESSIVE]" : "AGGRESSIVE";
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
