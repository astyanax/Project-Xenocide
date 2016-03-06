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
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;


using ProjectXenocide.Utils;

using ProjectXenocide.UI.Scenes.Geoscape;
using ProjectXenocide.UI.Dialogs;

using ProjectXenocide.Model;
using ProjectXenocide.Model.Geoscape;
using ProjectXenocide.Model.Geoscape.Vehicles;
using ProjectXenocide.Model.Geoscape.Outposts;
using ProjectXenocide.Model.Geoscape.Geography;
using ProjectXenocide.Model.Geoscape.AI;
using System.Threading;
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
        protected override void CreateCeguiWidgets()
        {
            SetView(0.00f, 0.00f, 0.745f, 1f);

            // add text giving the time
			gameTimeTop = AddStaticText(0.7574f, 0.08f, 0.2275f, 0.05f);
            gameTimeHour = AddStaticText(0.79f, 0.10f, 0.4275f, 0.08f);
            gameTimeSec = AddStaticText(0.888f, 0.115f, 0.4275f, 0.08f);
            fundsText = AddStaticText(0.754f, 0.02f, 0.2275f, 0.04125f);
            fundsAmount = AddStaticText(0.84f, 0.015f, 0.2275f, 0.04125f);
			sceneToolTip = AddStaticText(0.7475f, 0.25f, 0.3f, 0.3f);

			sceneToolTip.VerticalAlignment = CeGui.VerticalAlignment.Top;

            // Set Font and color for text
            SetTimeFont(gameTimeTop, "GeoTime");
            SetTimeFont(gameTimeHour, "GeoTimeBig");
            SetTimeFont(gameTimeSec, "GeoTime");
            SetFont(fundsAmount, "XenoBig");

            // change time buttons
            timeStopButton = AddButton("BUTTON_TIME_STOP", 0.7475f, 0.18f, 0.050f, 0.04125f);
            timeNormalButton = AddButton("BUTTON_TIME_X60", 0.8075f, 0.18f, 0.050f, 0.04125f, "PlanetView\\speedslow.ogg");
            timeHourButton = AddButton("BUTTON_TIME_X3600", 0.8675f, 0.18f, 0.050f, 0.04125f, "PlanetView\\speedfast.ogg");
            timeDayButton = AddButton("BUTTON_TIME_X86400", 0.9275f, 0.18f, 0.050f, 0.04125f, "PlanetView\\speedveryfast.ogg");

			
            interceptButton = AddButton("BUTTON_INTERCEPT", 0.7475f, 0.51f, 0.2275f, 0.04125f);
            basesButton = AddButton("BUTTON_BASES", 0.7475f, 0.56f, 0.2275f, 0.04125f);
            researchButton = AddButton("BUTTON_RESEARCH", 0.7475f, 0.61f, 0.2275f, 0.04125f);
            fundingButton = AddButton("BUTTON_FUNDING", 0.7475f, 0.66f, 0.2275f, 0.04125f);
            statisticsButton = AddButton("BUTTON_STATISTICS", 0.7475f, 0.71f, 0.2275f, 0.04125f);
            xnetButton = AddButton("BUTTON_XNET", 0.7475f, 0.76f, 0.2275f, 0.04125f);
            optionsButton = AddButton("BUTTON_OPTIONS", 0.7475f, 0.81f, 0.2275f, 0.04125f);

			
            // move camera buttons
            cameraUpButton = AddStaticImageButton("BUTTON_UP", 0.7890f, 0.885f, 0.020f, 0.03125f, "PanUpNormal", "PanUpPushed", "Menu\\buttonclick1_ok.ogg");
            cameraDownButton = AddStaticImageButton("BUTTON_DOWN", 0.7890f, 0.965f, 0.020f, 0.03125f, "PanDownNormal", "PanDownPushed", "Menu\\buttonclick1_ok.ogg");
            cameraLeftButton = AddStaticImageButton("BUTTON_LEFT", 0.7575f, 0.925f, 0.030f, 0.03125f, "PanLeftNormal", "PanLeftPushed", "Menu\\buttonclick1_ok.ogg");
            cameraRightButton = AddStaticImageButton("BUTTON_RIGHT", 0.8100f, 0.925f, 0.030f, 0.03125f, "PanRightNormal", "PanRightPushed", "Menu\\buttonclick1_ok.ogg");
            cameraInButton = AddStaticImageButton("BUTTON_ZOOM_IN", 0.8950f, 0.915f, 0.020f, 0.03125f, "ZoomInNormal", "ZoomInPushed", "PlanetView\\zoomin.ogg");
            cameraOutButton = AddStaticImageButton("BUTTON_ZOOM_OUT", 0.8625f, 0.955f, 0.020f, 0.03125f, "ZoomOutNormal", "ZoomOutPushed", "PlanetView\\zoomout.ogg");

            // change rate of time
            timeStopButton.Clicked += new CeGui.GuiEventHandler(OnTimeRateButtonClicked);
            timeNormalButton.Clicked += new CeGui.GuiEventHandler(OnTimeRateButtonClicked);
            timeHourButton.Clicked += new CeGui.GuiEventHandler(OnTimeRateButtonClicked);
            timeDayButton.Clicked += new CeGui.GuiEventHandler(OnTimeRateButtonClicked);

            interceptButton.Clicked += new CeGui.GuiEventHandler(OnInterceptButtonClicked);
            basesButton.Clicked += new CeGui.GuiEventHandler(ShowBasesScreen);
            researchButton.Clicked += new CeGui.GuiEventHandler(ShowResearchDialog);
            fundingButton.Clicked += new CeGui.GuiEventHandler(OnFundingButtonClicked);
            statisticsButton.Clicked += new CeGui.GuiEventHandler(ShowStatisticsScreen);
            xnetButton.Clicked += new CeGui.GuiEventHandler(ShowXNetScreen);
            optionsButton.Clicked += new CeGui.GuiEventHandler(ShowOptionsDialog);

            cameraUpButton.MouseClicked += new CeGui.MouseEventHandler(OnMoveCameraButtonClicked);
            cameraDownButton.MouseClicked += new CeGui.MouseEventHandler(OnMoveCameraButtonClicked);
            cameraLeftButton.MouseClicked += new CeGui.MouseEventHandler(OnMoveCameraButtonClicked);
            cameraRightButton.MouseClicked += new CeGui.MouseEventHandler(OnMoveCameraButtonClicked);
            cameraInButton.MouseClicked += new CeGui.MouseEventHandler(OnMoveCameraButtonClicked);
            cameraOutButton.MouseClicked += new CeGui.MouseEventHandler(OnMoveCameraButtonClicked);

            //Change button size
            SetFont(timeStopButton, "XenoSmall");
            SetFont(timeHourButton, "XenoSmall");
            SetFont(timeDayButton, "XenoSmall");
            SetFont(timeNormalButton, "XenoSmall");
            SetFont(cameraUpButton, "XenoSmall");
            SetFont(cameraDownButton, "XenoSmall");
            SetFont(cameraLeftButton, "XenoSmall");
            SetFont(cameraRightButton, "XenoSmall");

            //Adds funds to top of screen.
            fundsText.Text = Strings.SCREEN_GEOSCAPE_FUNDS;
            fundsAmount.Text = Xenocide.GameState.GeoData.XCorp.Bank.DisplayCurrentBalance;

            // special widgets based on state
            CeGui.Window extraButton = state.CreateCeguiWidgets();

            // nasty bit of hackary, if a button was created, need to make the button
            // a child of the Scene Window, because it's drawn OVER the scene window
            if (null != extraButton)
            {
                RootWidget.RemoveChild(extraButton);
                SceneWindow.AddChild(extraButton);
            }
        }

        private CeGui.Widgets.PushButton timeStopButton;
        private CeGui.Widgets.PushButton timeNormalButton;
        private CeGui.Widgets.PushButton timeHourButton;
        private CeGui.Widgets.PushButton timeDayButton;
        private CeGui.Widgets.PushButton interceptButton;
        private CeGui.Widgets.PushButton basesButton;
        private CeGui.Widgets.PushButton researchButton;
        private CeGui.Widgets.PushButton fundingButton;
        private CeGui.Widgets.PushButton statisticsButton;
        private CeGui.Widgets.PushButton xnetButton;
        private CeGui.Widgets.PushButton optionsButton;

        private CeGui.Widgets.StaticImage cameraUpButton;
        private CeGui.Widgets.StaticImage cameraDownButton;
        private CeGui.Widgets.StaticImage cameraLeftButton;
        private CeGui.Widgets.StaticImage cameraRightButton;
        private CeGui.Widgets.StaticImage cameraInButton;
        private CeGui.Widgets.StaticImage cameraOutButton;

        private CeGui.Widgets.StaticText gameTimeTop;
        private CeGui.Widgets.StaticText gameTimeHour;
        private CeGui.Widgets.StaticText gameTimeSec;
        private CeGui.Widgets.StaticText fundsText;
        private CeGui.Widgets.StaticText fundsAmount;
		private CeGui.Widgets.StaticText sceneToolTip;

        //Used to keep track of time and avoid updating if needed.
        private string gameTimeText;

        /// <summary>
        /// Adjust static text widget for displaying the time
        /// </summary>
        /// <param name="widget">static text widget to adjust</param>
        /// <param name="fontName">Name of font widget should be using</param>
        private static void SetTimeFont(CeGui.Widgets.StaticText widget, string fontName)
        {
            //Create color for fonts 
            widget.SetTextColor(new CeGui.Colour(0.4921875f, 0.703125f, 0.609375f));
            widget.Font = CeGui.FontManager.Instance.GetFont(fontName);
        }

        /// <summary>
        /// Set a widget's font
        /// </summary>
        /// <param name="widget"></param>
        /// <param name="fontName"></param>
        private static void SetFont(CeGui.Window widget, string fontName)
        {
            widget.Font = CeGui.FontManager.Instance.GetFont(fontName);
        }

        #endregion Create the CeGui widgets

        #region 3D scene management

        /// <summary>
        /// Update any model data
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            state.Update(gameTime);

            // update time shown on screen
            // Note, if display hasn't changed, don't write new value to text window
            // as this will make CeGui recompute all render quads.
            DateTime time = Xenocide.GameState.GeoData.GeoTime.Time;
            String newTime = time.ToString();

			// render hover text even if game time is not ticking
			sceneToolTip.Text = geoscapeScene.ToolTipText;
			sceneToolTip.VerticalFormat = CeGui.VerticalTextFormat.Top;

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
        private void ShowBasesScreen(object sender, CeGui.GuiEventArgs e)
        {
            state.OnBasesButton();
        }

        /// <summary>
        /// React to "Research" button being pressed
        /// </summary>
        private void ShowResearchDialog(object sender, CeGui.GuiEventArgs e)
        {
            state.OnResearchButton();
        }

        /// <summary>Put the options dialog on the display</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void ShowOptionsDialog(object sender, CeGui.GuiEventArgs e)
        {
            state.OnOptionsButton();
        }

        /// <summary>Respond to user clicking the "Funding" button</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void OnFundingButtonClicked(object sender, CeGui.GuiEventArgs e)
        {
            state.OnFundingButton();
        }

        /// <summary>Replace this screen on display with the Statistics Screen</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void ShowStatisticsScreen(object sender, CeGui.GuiEventArgs e)
        {
            state.OnStatisticsButton();
        }

        /// <summary>Replace this screen on display with the X-Net Screen</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void ShowXNetScreen(object sender, CeGui.GuiEventArgs e)
        {
            state.OnXNetButton();
        }

        /// <summary>Launch interceptor from base to attack UFO</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void OnInterceptButtonClicked(object sender, CeGui.GuiEventArgs e)
        {
            state.OnInterceptButton();
        }

        /// <summary>
        /// Return true if Button is the specified button
        /// </summary>
        /// <param name="button">button to examine</param>
        /// <param name="resourceName">Name of resource string to look for</param>
        /// <returns>True if Button's Name ends with the specified resource string</returns>
        private static bool IsButton(CeGui.Widgets.PushButton button, string resourceName)
        {
            string label = XenocideResourceManager.Get(resourceName);
            Debug.Assert(!String.IsNullOrEmpty(label));
            return button.Name.EndsWith(label);
        }

        /// <summary>React to user clicking on one of the "move camera" buttons</summary>
        /// <param name="sender">Button the user clicked</param>
        /// <param name="e">Not used</param>
        private void OnMoveCameraButtonClicked(object sender, CeGui.MouseEventArgs e)
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
        private void OnTimeRateButtonClicked(object sender, CeGui.GuiEventArgs e)
        {
            state.OnTimeRateButtonClicked((CeGui.Widgets.PushButton)sender);
        }

        /// <summary>React to user clicking left mouse button in the 3D geoscape scene</summary>
        /// <param name="e">Mouse information</param>
        protected override void OnLeftMouseDownInScene(CeGui.MouseEventArgs e)
        {
            Debug.Assert(e.Button == System.Windows.Forms.MouseButtons.Left);
            GeoPosition pos = WindowPixelToGeoPosition(e.Position.X, e.Position.Y);
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
        private void OnCancelNewBase(object sender, CeGui.GuiEventArgs e)
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
                    YesNoDialog dlg = YesNoDialog.OkCancelDialog(
                        Util.StringFormat(Strings.YESNOMSG_BUILD_BASE_HERE, Util.StringFormat("{0:N0}", cost), area)
                    );

                    // if yes is pressed, do the "base name" dialog
                    dlg.YesAction += delegate()
                    {
                        if (!isFirstBase)
                        {
                            Xenocide.GameState.GeoData.XCorp.Bank.Debit(cost);
                        }
                        else
                        {
                            // Overmind starts sending missions near location of X-Corp base
                            GeoPosition target = pos.RandomLocationDistantBykm(500);
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

        /// <summary>
        /// Convert position of pixel on screen to geoposition
        /// </summary>
        /// <param name="X">pixel's column</param>
        /// <param name="Y">pixel's row</param>
        /// <returns>corresponding Geoposition, or null if not on Geoscape</returns>
        private GeoPosition WindowPixelToGeoPosition(float X, float Y)
        {
            PointF coords2 = SceneWindow.AbsoluteToRelative(new PointF(
                X - SceneWindow.AbsoluteX,
                Y - SceneWindow.AbsoluteY));
            return geoscapeScene.WindowToGeoPosition(coords2);
        }

        /// <summary>
        /// Move camera due to mouse move
        /// </summary>
        /// <param name="e">details of the mouse move</param>
        protected override void RotateSceneUsingMouse(CeGui.MouseEventArgs e)
        {
            // try to track cursor on globe
            GeoPosition final = WindowPixelToGeoPosition(e.Position.X, e.Position.Y);
            GeoPosition start = WindowPixelToGeoPosition(e.Position.X - e.MoveDelta.X, e.Position.Y - e.MoveDelta.Y);
            if ((null != start) && (null != final))
            {
                float latitude = final.Latitude - start.Latitude;
                float longitude = final.Longitude - start.Longitude;
                Scene.RotateCamera(longitude, latitude);
            }
            else
            {
                // can't track cursor on globe, fallback to default handling
                base.RotateSceneUsingMouse(e);
            }
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
