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
* @file GeoscapeScreen.cs
* @date Created: 2007/01/21
* @author File creator: David Teviotdale
* @author Credits: none
*/
#endregion

#region Using Statements

using System;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Threading;

using Gum.Forms;
using Gum.Forms.Controls;
using Gum.Wireframe;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using ProjectXenocide.Assets;
using ProjectXenocide.Model;
using ProjectXenocide.Model.Geoscape;
using ProjectXenocide.Model.Geoscape.AI;
using ProjectXenocide.Model.Geoscape.Geography;
using ProjectXenocide.Model.Geoscape.Outposts;
using ProjectXenocide.Model.Geoscape.Vehicles;
using ProjectXenocide.UI.Dialogs;
using ProjectXenocide.UI.Scenes.Geoscape;
using ProjectXenocide.Utils;

using Xenocide.Resources;


#endregion

namespace ProjectXenocide.UI.Screens
{
    /// <summary>
    /// The Geoscape screen
    /// </summary>
    public partial class GeoscapeScreen : PolarScreen
    {
        /// <summary>
        /// constructor (obviously)
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:DisposeObjectsBeforeLosingScope",
           Justification = "FxCop False Positive")]
        public GeoscapeScreen()
            : base("GeoscapeScreen")
        {
            Scene = new GeoscapeScene(oldCameraPosition);
            State = new ViewGeoscapeScreenState(this);
            if (Xenocide.AudioSystem != null)
                Xenocide.AudioSystem.PlayRandomMusic("PlanetView");
        }

        /// <summary>
        /// Implement Dispose
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    if (geoscapeScene != null)
                    {
                        geoscapeScene.Dispose();
                        Scene = null;
                    }
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        #region Create the CeGui widgets

        /// <summary>
        /// add the buttons to the screen
        /// </summary>
        protected override void CreateGumControls()
        {
            SetView(0.00f, 0.00f, 0.745f, 1f);

            if (GumRoot != null)
            {
                WireButton("timeStopButton", OnTimeRateButtonClicked);
                WireButton("timeNormalButton", OnTimeRateButtonClicked);
                WireButton("timeHourButton", OnTimeRateButtonClicked);
                WireButton("timeDayButton", OnTimeRateButtonClicked);
                WireButton("interceptButton", OnInterceptButtonClicked);
                WireButton("basesButton", ShowBasesScreen);
                WireButton("researchButton", ShowResearchDialog);
                WireButton("fundingButton", OnFundingButtonClicked);
                WireButton("statisticsButton", ShowStatisticsScreen);
                WireButton("xnetButton", ShowXNetScreen);
                WireButton("optionsButton", ShowOptionsDialog);
                WireButton("cameraUpButton", OnMoveCameraButtonClicked);
                WireButton("cameraDownButton", OnMoveCameraButtonClicked);
                WireButton("cameraLeftButton", OnMoveCameraButtonClicked);
                WireButton("cameraRightButton", OnMoveCameraButtonClicked);
                WireButton("cameraInButton", OnMoveCameraButtonClicked);
                WireButton("cameraOutButton", OnMoveCameraButtonClicked);

                var labelPanel = new StackPanel();
                labelPanel.Visual.X = 20;
                labelPanel.Visual.Y = 10;
                GumRoot.Children.Add(labelPanel.Visual);

                gameTimeTop = new Label();
                gameTimeHour = new Label();
                gameTimeSec = new Label();
                fundsText = new Label();
                fundsAmount = new Label();
                sceneToolTip = new Label();
                timeText = new Label();

                labelPanel.AddChild(gameTimeTop);
                labelPanel.AddChild(gameTimeHour);
                labelPanel.AddChild(gameTimeSec);
                labelPanel.AddChild(fundsText);
                labelPanel.AddChild(fundsAmount);
                labelPanel.AddChild(sceneToolTip);
                labelPanel.AddChild(timeText);

                fundsText.Text = Strings.SCREEN_GEOSCAPE_FUNDS;
                fundsAmount.Text = Xenocide.GameState.GeoData.XCorp.Bank.DisplayCurrentBalance;
                timeText.Text = Strings.SCREEN_GEOSCAPE_GMT;

                state.CreateGumControls();
                return;
            }

            // add text giving the time
            gameTimeTop = new Label();
            gameTimeHour = new Label();
            gameTimeSec = new Label();
            fundsText = new Label();
            fundsAmount = new Label();
            sceneToolTip = new Label();
            timeText = new Label();

            RootContainer.AddChild(gameTimeTop);
            RootContainer.AddChild(gameTimeHour);
            RootContainer.AddChild(gameTimeSec);
            RootContainer.AddChild(fundsText);
            RootContainer.AddChild(fundsAmount);
            RootContainer.AddChild(sceneToolTip);
            RootContainer.AddChild(timeText);

            // Set Font and color for text
            SetTimeFont(gameTimeTop, "GeoTime");
            SetTimeFont(gameTimeHour, "GeoTimeBig");
            SetTimeFont(gameTimeSec, "GeoTime");
            SetTimeFont(timeText, "GeoTime");
            SetFont(fundsAmount, "XenoBig");

            // change time buttons
            timeStopButton = new Button() { Text = XenocideResourceManager.Get("BUTTON_TIME_STOP") };
            timeNormalButton = new Button() { Text = XenocideResourceManager.Get("BUTTON_TIME_X60") };
            timeHourButton = new Button() { Text = XenocideResourceManager.Get("BUTTON_TIME_X3600") };
            timeDayButton = new Button() { Text = XenocideResourceManager.Get("BUTTON_TIME_X86400") };

            interceptButton = new Button() { Text = XenocideResourceManager.Get("BUTTON_INTERCEPT") };
            basesButton = new Button() { Text = XenocideResourceManager.Get("BUTTON_BASES") };
            researchButton = new Button() { Text = XenocideResourceManager.Get("BUTTON_RESEARCH") };
            fundingButton = new Button() { Text = XenocideResourceManager.Get("BUTTON_FUNDING") };
            statisticsButton = new Button() { Text = XenocideResourceManager.Get("BUTTON_STATISTICS") };
            xnetButton = new Button() { Text = XenocideResourceManager.Get("BUTTON_XNET") };
            optionsButton = new Button() { Text = XenocideResourceManager.Get("BUTTON_OPTIONS") };

            RootContainer.AddChild(timeStopButton);
            RootContainer.AddChild(timeNormalButton);
            RootContainer.AddChild(timeHourButton);
            RootContainer.AddChild(timeDayButton);
            RootContainer.AddChild(interceptButton);
            RootContainer.AddChild(basesButton);
            RootContainer.AddChild(researchButton);
            RootContainer.AddChild(fundingButton);
            RootContainer.AddChild(statisticsButton);
            RootContainer.AddChild(xnetButton);
            RootContainer.AddChild(optionsButton);

            // move camera buttons
            cameraUpButton = new Button() { Text = "BUTTON_UP" };
            cameraDownButton = new Button() { Text = "BUTTON_DOWN" };
            cameraLeftButton = new Button() { Text = "BUTTON_LEFT" };
            cameraRightButton = new Button() { Text = "BUTTON_RIGHT" };
            cameraInButton = new Button() { Text = "BUTTON_ZOOM_IN" };
            cameraOutButton = new Button() { Text = "BUTTON_ZOOM_OUT" };

            RootContainer.AddChild(cameraUpButton);
            RootContainer.AddChild(cameraDownButton);
            RootContainer.AddChild(cameraLeftButton);
            RootContainer.AddChild(cameraRightButton);
            RootContainer.AddChild(cameraInButton);
            RootContainer.AddChild(cameraOutButton);

            // change rate of time
            timeStopButton.Click += OnTimeRateButtonClicked;
            timeNormalButton.Click += OnTimeRateButtonClicked;
            timeHourButton.Click += OnTimeRateButtonClicked;
            timeDayButton.Click += OnTimeRateButtonClicked;

            interceptButton.Click += OnInterceptButtonClicked;
            basesButton.Click += ShowBasesScreen;
            researchButton.Click += ShowResearchDialog;
            fundingButton.Click += OnFundingButtonClicked;
            statisticsButton.Click += ShowStatisticsScreen;
            xnetButton.Click += ShowXNetScreen;
            optionsButton.Click += ShowOptionsDialog;

            cameraUpButton.Click += OnMoveCameraButtonClicked;
            cameraDownButton.Click += OnMoveCameraButtonClicked;
            cameraLeftButton.Click += OnMoveCameraButtonClicked;
            cameraRightButton.Click += OnMoveCameraButtonClicked;
            cameraInButton.Click += OnMoveCameraButtonClicked;
            cameraOutButton.Click += OnMoveCameraButtonClicked;

            //Change button size
            SetFont(timeStopButton, "XenoSmall");
            SetFont(timeHourButton, "XenoSmall");
            SetFont(timeDayButton, "XenoSmall");
            SetFont(timeNormalButton, "XenoSmall");
            SetFont(cameraUpButton, "XenoSmall");
            SetFont(cameraDownButton, "XenoSmall");
            SetFont(cameraLeftButton, "XenoSmall");
            SetFont(cameraRightButton, "XenoSmall");

            //Adds text to top of screen.
            fundsText.Text = Strings.SCREEN_GEOSCAPE_FUNDS;
            fundsAmount.Text = Xenocide.GameState.GeoData.XCorp.Bank.DisplayCurrentBalance;
            timeText.Text = Strings.SCREEN_GEOSCAPE_GMT;

            // special widgets based on state, added directly to the scene window
            state.CreateGumControls();
        }

        private Button timeStopButton;
        private Button timeNormalButton;
        private Button timeHourButton;
        private Button timeDayButton;
        private Button interceptButton;
        private Button basesButton;
        private Button researchButton;
        private Button fundingButton;
        private Button statisticsButton;
        private Button xnetButton;
        private Button optionsButton;

        private Button cameraUpButton;
        private Button cameraDownButton;
        private Button cameraLeftButton;
        private Button cameraRightButton;
        private Button cameraInButton;
        private Button cameraOutButton;

        private Label gameTimeTop;
        private Label gameTimeHour;
        private Label gameTimeSec;
        private Label fundsText;
        private Label fundsAmount;
        private Label sceneToolTip;
        private Label timeText;

        //Used to keep track of time and avoid updating if needed.
        private string gameTimeText;

        /// <summary>
        /// Adjust static text widget for displaying the time
        /// </summary>
        /// <param name="widget">static text widget to adjust</param>
        /// <param name="fontName">Name of font widget should be using</param>
        internal void AddControl(FrameworkElement control)
        {
            if (GumRoot != null)
                GumRoot.Children.Add(control.Visual);
            else if (RootContainer != null)
                RootContainer.AddChild(control);
        }

        private static void SetTimeFont(FrameworkElement widget, string fontName)
        {
            ApplyFont(widget, fontName);
        }

        /// <summary>
        /// Set a widget's font using its legacy CeGui font name.
        /// Resolves via FontRegistry.FontNameMap, defaulting to Arial.
        /// </summary>
        private static void SetFont(FrameworkElement widget, string fontName)
        {
            ApplyFont(widget, fontName);
        }

        private static void ApplyFont(FrameworkElement widget, string fontName)
        {
            var fontId = FontRegistry.FontNameMap.TryGetValue(fontName, out var id) ? id : FontId.Arial;
            string fontFamily = FontRegistry.GetFontFamily(fontId);

            switch (widget)
            {
                case Button btn:
                {
                    var textInstance = btn.Visual.GetChildByNameRecursively("TextInstance") as GraphicalUiElement;
                    textInstance?.SetProperty("Font", fontFamily);
                    break;
                }
                case Label lbl:
                    lbl.Visual.SetProperty("Font", fontFamily);
                    break;
            }
        }

        #endregion Create the CeGui widgets

        #region 3D scene management

        /// <summary>
        /// Update any model data
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            state.Update(gameTime);

            // update time shown on screen
            // Note, if display hasn't changed, don't write new value to text window
            // as this will make CeGui recompute all render quads.
            DateTime time = Xenocide.GameState.GeoData.GeoTime.Time;
            String newTime = time.ToString();

            if (gameTimeText != newTime)
            {
                CultureInfo culture = Thread.CurrentThread.CurrentCulture;

                //First we create the day, add on to it, add the month then the year.
                StringBuilder gametime = new StringBuilder(time.ToString("%d", culture));

                if (gametime.ToString() == "1")
                    gametime.Append("st");
                else if (gametime.ToString() == "2")
                    gametime.Append("nd");
                else if (gametime.ToString() == "3")
                    gametime.Append("rd");
                else
                    gametime.Append("th");

                //Change month to uppercase
                string upper = time.ToString("MMMM", culture);
                gametime.Append(" ");
                gametime.Append(upper.ToUpper(culture));
                gametime.Append(" ");
                gametime.Append(time.ToString("yyyy", culture));

                gameTimeTop.Text = gametime.ToString();
                gameTimeHour.Text = time.ToString("HH:mm", culture);
                gameTimeSec.Text = time.ToString(":ss", culture);

                //Set time
                gameTimeText = newTime;
            }
        }

        /// <summary>
        /// Called to tell the Window prior to destruction to tell it to save any state information
        /// needed to reconstruct it's state.
        /// Basically, used to get the Geoscape to store the camera's position 
        /// when we go to a different screen.
        /// </summary>
        public override void SaveState()
        {
            oldCameraPosition = Scene.CameraPosition;
        }

        #endregion

        #region event handlers

        /// <summary>Replace this screen on display with the Bases Screen</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void ShowBasesScreen(object sender, EventArgs e)
        {
            state.OnBasesButton();
        }

        /// <summary>
        /// React to "Research" button being pressed
        /// </summary>
        private void ShowResearchDialog(object sender, EventArgs e)
        {
            state.OnResearchButton();
        }

        /// <summary>Put the options dialog on the display</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void ShowOptionsDialog(object sender, EventArgs e)
        {
            state.OnOptionsButton();
        }

        /// <summary>Respond to user clicking the "Funding" button</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void OnFundingButtonClicked(object sender, EventArgs e)
        {
            state.OnFundingButton();
        }

        /// <summary>Replace this screen on display with the Statistics Screen</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void ShowStatisticsScreen(object sender, EventArgs e)
        {
            state.OnStatisticsButton();
        }

        /// <summary>Replace this screen on display with the X-Net Screen</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void ShowXNetScreen(object sender, EventArgs e)
        {
            state.OnXNetButton();
        }

        /// <summary>Launch interceptor from base to attack UFO</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void OnInterceptButtonClicked(object sender, EventArgs e)
        {
            state.OnInterceptButton();
        }

        /// <summary>
        /// Return true if Button is the specified button
        /// </summary>
        /// <param name="button">button to examine</param>
        /// <param name="resourceName">Name of resource string to look for</param>
        /// <returns>True if Button's Name ends with the specified resource string</returns>
        private static bool IsButton(Button button, string resourceName)
        {
            string label = XenocideResourceManager.Get(resourceName);
            Debug.Assert(!String.IsNullOrEmpty(label));
            return button.Text.EndsWith(label);
        }

        /// <summary>React to user clicking on one of the "move camera" buttons</summary>
        /// <param name="sender">Button the user clicked</param>
        /// <param name="e">Not used</param>
        private void OnMoveCameraButtonClicked(object sender, EventArgs e)
        {
            // turn an 1/8th of a revolution
            const float rotation = (float)(Math.PI / 4);

            // Step size to move camera in
            const float zoomStep = 0.5f;

            if (sender == cameraLeftButton)
            {
                Scene.RotateCamera(-rotation, 0.0f);
            }
            else if (sender == cameraRightButton)
            {
                Scene.RotateCamera(rotation, 0.0f);
            }
            else if (sender == cameraUpButton)
            {
                Scene.RotateCamera(0.0f, rotation);
            }
            else if (sender == cameraDownButton)
            {
                Scene.RotateCamera(0.0f, -rotation);
            }
            else if (sender == cameraInButton)
            {
                Scene.ZoomCamera(-zoomStep);
            }
            else if (sender == cameraOutButton)
            {
                Scene.ZoomCamera(zoomStep);
            }
        }

        /// <summary>React to user clicking on one of the "time rate" buttons</summary>
        /// <param name="sender">Button the user clicked</param>
        /// <param name="e">Not used</param>
        private void OnTimeRateButtonClicked(object sender, EventArgs e)
        {
            state.OnTimeRateButtonClicked(sender as Button);
        }

        /// <summary>React to user clicking left mouse button in the 3D geoscape scene</summary>
        /// <param name="relX">X co-ordinate relative to viewport (0-1)</param>
        /// <param name="relY">Y co-ordinate relative to viewport (0-1)</param>
        protected override void OnLeftMouseDownInScene(float relX, float relY)
        {
            GeoPosition pos = geoscapeScene.WindowToGeoPosition(new UiPoint(relX, relY));
            if (pos != null)
            {
                state.OnLeftMouseDownInScene(pos);
            }
        }

        /// <summary>React to user clicking the "Cancel setting new base location" button</summary>
        /// <param name="sender">Button the user clicked</param>
        /// <param name="e">Not used</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:DisposeObjectsBeforeLosingScope",
           Justification = "FxCop False Positive")]
        private void OnCancelNewBase(object sender, EventArgs e)
        {
            ScreenManager.ScheduleScreen(new GeoscapeScreen());
        }

        #endregion event handlers

        /// <summary>
        /// Confirm user wants to build base here
        /// </summary>
        /// <param name="pos">location of new base</param>
        /// <param name="isFirstBase">is this player's first base?</param>
        private static void ConfirmBasePositionDialog(GeoPosition pos, bool isFirstBase)
        {
            if (Xenocide.GameState.GeoData.Planet.IsPositionOverWater(pos))
            {
                Util.ShowMessageBox(Strings.MSGBOX_CANNOT_BUILD_ON_WATER);
            }
            else
            {
                PlanetRegion region = Xenocide.GameState.GeoData.Planet.GetRegionAtLocation(pos);
                int cost = isFirstBase ? 0 : region.OutpostBuildCost;
                string area = region.ToString();

                if (Xenocide.GameState.GeoData.XCorp.Bank.CanAfford(cost))
                {
                    GumYesNoDialog dlg = GumYesNoDialog.OkCancelDialog(
                        Util.StringFormat(Strings.YESNOMSG_BUILD_BASE_HERE, Util.StringFormat("{0:N0}", cost), area)
                    );

                    // if yes is pressed, do the "base name" dialog
                    dlg.YesAction += delegate ()
                    {
                        if (!isFirstBase)
                        {
                            Xenocide.GameState.GeoData.XCorp.Bank.Debit(cost);
                        }
                        else
                        {
                            // Overmind starts sending missions near location of X-Corp base
                            GeoPosition target = pos.RandomLocationDistantBykm(500);
                            target = Xenocide.GameState.GeoData.Planet.GetClosestLand(target);
                            Xenocide.GameState.GeoData.Overmind.BeginFirstMissions(target);
                        }
                        Xenocide.ScreenManager.QueueDialog(new NameNewBaseDialog(pos, isFirstBase));
                    };

                    Xenocide.ScreenManager.ShowDialog(dlg);
                }
            }
        }

        /// <summary>
        /// Return the Craft (UFO or Aircraft) that is closest to position on the Geoscape.
        /// <remarks>
        /// Craft more than 500km away from position will be ignored.</remarks>
        /// </summary>
        /// <param name="pos">center of search area</param>
        /// <returns>closest craft, or null if none found</returns>
        private static Craft FindClosestCraft(GeoPosition pos)
        {
            double distance = GeoPosition.KilometersToRadians(501);
            Craft nearest = pos.FindClosest(Xenocide.GameState.GeoData.Overmind.Ufos, distance);
            if (null != nearest)
            {
                distance = pos.Distance(nearest.Position);
            }
            foreach (Outpost outpost in Xenocide.GameState.GeoData.Outposts)
            {
                foreach (Craft craft in outpost.Fleet)
                {
                    if (!craft.InBase && (craft.Position.Distance(pos) < distance))
                    {
                        nearest = craft;
                        distance = craft.Position.Distance(pos);
                    }
                }
            }
            return nearest;
        }

        #region fields

        /// <summary>
        /// What mode are we in?
        /// </summary>
        public ScreenState State
        {
            get { return state; }
            set { state = value; }
        }

        /// <summary>
        /// Return the scene field as it's real type (a GeoscapeScene)
        /// </summary>
        private GeoscapeScene geoscapeScene { get { return (GeoscapeScene)Scene; } }

        /// <summary>
        /// What mode are we in?
        /// </summary>
        private ScreenState state;

        /// <summary>
        /// Remember where camera has been, to prevent Geoscape jumping around
        /// </summary>
        private static Vector3 oldCameraPosition = new Vector3(0.0f, 0.0f, 3.5f);


        #endregion fields
    }
}
