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

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

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
    /// DogfightController handles all combat logic. Radar viewport is drawn
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
            this.log = new BattleLog();
            this.dogfightController = new DogfightController(aircraft, ufo, log);

            aircraft.OnDogfightStart();
            ufo.OnDogfightStart();
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

            // Initialize default state
            runRealTime = false;
            speedMultiplier = 0;

            // Set initial display
            DrawScreen();
        }

        #region Fields

        private Aircraft aircraft;
        private Ufo ufo;
        private BattleLog log;
        private DogfightController dogfightController;

        // Speed control
        private bool runRealTime;
        private int speedMultiplier; // 0=paused, 1=normal, 2=fast
        private double elapsed;

        // Radar rendering resources
        private SpriteBatch spriteBatch;
        private Texture2D radarBackground;
        private Texture2D craftIcon;
        private Texture2D ufoBlob;

        // HUD control references (populated from GumRoot or fallback)
        private Label statusLabel;
        private Label timeLabel;
        private Label weapon1Label;
        private Label weapon1InfoLabel;
        private Button weapon1ToggleBtn;
        private Label weapon2Label;
        private Label weapon2InfoLabel;
        private Button weapon2ToggleBtn;
        private Label ufoNameLabel;
        private Label ufoHullLabel;
        private Label ufoWeaponLabel;
        private Label craftNameLabel;
        private Label craftHullLabel;
        private Label craftFuelLabel;
        private Label distanceLabel;
        private Label logLabel;

        // Weapon enable states
        private bool weapon1Enabled = true;
        private bool weapon2Enabled = true;

        #endregion

        #region Tactical Mode Handlers

        private void OnStandoffButton(object sender, EventArgs e)
        {
            dogfightController.SetTacticalMode(TacticalMode.Standoff);
            DrawScreen();
        }

        private void OnCautiousButton(object sender, EventArgs e)
        {
            dogfightController.SetTacticalMode(TacticalMode.Cautious);
            DrawScreen();
        }

        private void OnStandardButton(object sender, EventArgs e)
        {
            dogfightController.SetTacticalMode(TacticalMode.Standard);
            DrawScreen();
        }

        private void OnAggressiveButton(object sender, EventArgs e)
        {
            dogfightController.SetTacticalMode(TacticalMode.Aggressive);
            DrawScreen();
        }

        private void OnDisengageButton(object sender, EventArgs e)
        {
            dogfightController.SetTacticalMode(TacticalMode.Disengage);
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
            speedMultiplier = 2;
            runRealTime = true;
            DrawScreen();
        }

        #endregion

        #region Weapon Toggle Handlers

        private void OnWeapon1Toggle(object sender, EventArgs e)
        {
            weapon1Enabled = !weapon1Enabled;
            DrawScreen();
        }

        private void OnWeapon2Toggle(object sender, EventArgs e)
        {
            weapon2Enabled = !weapon2Enabled;
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
            dogfightController.EndDogfight();

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

        #endregion

        #region Game Loop

        /// <summary>
        /// Update game logic each frame.
        /// </summary>
        public override void Update(GameTime gameTime)
        {
            if (runRealTime && !dogfightController.IsDogfightOver)
            {
                elapsed += gameTime.ElapsedGameTime.TotalMilliseconds;
                double tickInterval = 1000.0 / speedMultiplier;
                if (elapsed >= tickInterval)
                {
                    UpdateDogfight();
                }
            }

            // Check if dogfight ended
            if (dogfightController.IsDogfightOver)
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

        #region Radar Rendering

        /// <summary>
        /// Draw the 2D radar viewport showing aircraft, UFO, and weapon fire.
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

            // Draw UFO blob at top (moving down as distance decreases)
            float ufoY = CalculateUfoYPosition(radarY, radarHeight);
            spriteBatch.Draw(ufoBlob,
                new Rectangle(radarX + (radarWidth / 2) - 16, (int)ufoY - 16, 32, 32),
                Color.Red);

            // Draw aircraft icon at bottom (moving up as distance decreases)
            float craftY = CalculateCraftYPosition(radarY, radarHeight);
            spriteBatch.Draw(craftIcon,
                new Rectangle(radarX + (radarWidth / 2) - 12, (int)craftY - 12, 24, 24),
                Color.LimeGreen);

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
        /// </summary>
        private void DrawWeaponRanges(SpriteBatch sb, int x, int y, int w, int h)
        {
            // TODO: Draw range lines for each weapon pod
            // Range is mapped from meters to pixels: 80,000m = full height
        }

        /// <summary>
        /// Calculate Y position of UFO on radar based on current distance.
        /// </summary>
        private float CalculateUfoYPosition(int radarY, int radarHeight)
        {
            double distance = dogfightController.CurrentDistance;
            float normalized = (float)(distance / 80000.0); // 0.0 = point blank, 1.0 = standoff
            return radarY + (radarHeight * normalized * 0.4f); // UFO in top 40%
        }

        /// <summary>
        /// Calculate Y position of aircraft on radar based on current distance.
        /// </summary>
        private float CalculateCraftYPosition(int radarY, int radarHeight)
        {
            double distance = dogfightController.CurrentDistance;
            float normalized = (float)(distance / 80000.0);
            return radarY + radarHeight - (radarHeight * normalized * 0.4f); // Craft in bottom 40%
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
                    // Simple upward-pointing triangle
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
            elapsed = 0.0;
            dogfightController.AdvanceTurn();
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
            if (statusLabel != null)
            {
                statusLabel.Text = dogfightController.CurrentModeName;
            }
        }

        private void UpdateTimeDisplay()
        {
            if (timeLabel != null)
            {
                timeLabel.Text = string.Format("Time: {0:F0}s", log.Now);
            }
        }

        private void UpdateDistanceDisplay()
        {
            if (distanceLabel != null)
            {
                int distanceKm = (int)(dogfightController.CurrentDistance / 1000.0);
                distanceLabel.Text = string.Format("Distance: {0}km", distanceKm);
            }
        }

        private void UpdateAircraftStatus()
        {
            if (craftNameLabel != null)
                craftNameLabel.Text = aircraft.Name;

            if (craftHullLabel != null)
                craftHullLabel.Text = string.Format("Hull: {0}%", aircraft.HullPercent);

            if (craftFuelLabel != null)
                craftFuelLabel.Text = string.Format("Fuel: {0}%", aircraft.FuelPercent);
        }

        private void UpdateWeaponInfo()
        {
            DrawPodInformation(weapon1Label, weapon1InfoLabel, weapon1ToggleBtn, 0, weapon1Enabled);
            DrawPodInformation(weapon2Label, weapon2InfoLabel, weapon2ToggleBtn, 1, weapon2Enabled);
        }

        private void DrawPodInformation(Label headerLabel, Label infoLabel, Button toggleBtn, int podIndex, bool isEnabled)
        {
            if (headerLabel == null || infoLabel == null)
                return;

            if (podIndex < aircraft.WeaponPods.Count && aircraft.WeaponPods[podIndex] != null)
            {
                var pod = aircraft.WeaponPods[podIndex];
                headerLabel.Text = string.Format("WEAPON {0}: {1}", podIndex + 1, pod.Name);

                string ammoText = pod.UsesAmmo
                    ? string.Format("Ammo: {0}/{1}", pod.ShotsLeft, pod.ClipSize)
                    : "Ammo: Unlimited";

                infoLabel.Text = string.Format("{0}  Range: {1}km  Dmg: {2}",
                    ammoText, pod.WeaponRange / 1000, pod.WeaponDamage);

                if (toggleBtn != null)
                {
                    toggleBtn.Text = isEnabled ? "ON" : "OFF";
                }
            }
            else
            {
                headerLabel.Text = string.Format("WEAPON {0}: Empty", podIndex + 1);
                infoLabel.Text = "";
                if (toggleBtn != null)
                {
                    toggleBtn.Text = "N/A";
                }
            }
        }

        private void UpdateUfoInfo()
        {
            if (ufoNameLabel != null)
                ufoNameLabel.Text = string.Format("{0} ({1})", ufo.Name, ufo.UfoItemInfo.UfoSize);

            if (ufoHullLabel != null)
                ufoHullLabel.Text = string.Format("Hull: {0}%", ufo.HullPercent);

            if (ufoWeaponLabel != null)
            {
                if (ufo.WeaponPods.Count > 0 && ufo.WeaponPods[0] != null)
                {
                    ufoWeaponLabel.Text = string.Format("Weapon: {0}", ufo.WeaponPods[0].Name);
                }
                else
                {
                    ufoWeaponLabel.Text = "Weapon: None";
                }
            }
        }

        private void UpdateCombatLog()
        {
            if (logLabel == null)
                return;

            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("Time: {0}", log.Now);

            // Show last few entries
            int startIdx = Math.Max(0, log.Entries.Count - 5);
            for (int i = startIdx; i < log.Entries.Count; i++)
            {
                sb.Append(Util.Linefeed);
                sb.Append(log.Entries[i].Details);
            }

            logLabel.Text = sb.ToString();
        }

        private void UpdateSpeedButtons()
        {
            // Visual feedback for active speed (would need button references to highlight)
        }

        private void UpdateTacticalButtons()
        {
            // Visual feedback for active tactical mode (would need button references to highlight)
        }

        #endregion
    }
}
