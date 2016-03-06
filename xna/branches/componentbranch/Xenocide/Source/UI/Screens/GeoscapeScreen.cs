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
* @author File creator: dteviot
* @author Credits: none
*/
#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Drawing;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using Xenocide.Resources;
using Xenocide.Utils;

using Xenocide.UI.Scenes.Geoscape;
using Xenocide.UI.Dialogs;

using Xenocide.Model;
using Xenocide.Model.Geoscape;
using Xenocide.Model.Geoscape.Craft;
using Xenocide.Model.Geoscape.HumanBases;
using Xenocide.Source.UI.Dialogs;
using Xenocide.Model.Geoscape.Research;

#endregion

namespace Xenocide.UI.Screens
{
    /// <summary>
    /// The Geoscape screen
    /// </summary>
    public class GeoscapeScreen : PolarScreen
    {
        /// <summary>
        /// constructor (obviously)
        /// </summary>
        public GeoscapeScreen(Game game)
            : base(game, "GeoscapeScreen")
        {
            Scene = new GeoscapeScene(game);
        }

        public override void Initialize()
        {
            humanBaseService = (IHumanBaseService)Game.Services.GetService(typeof(IHumanBaseService));
            base.Initialize();
            ((IResearchService)Game.Services.GetService(typeof(IResearchService))).TopicResearched += TopicResearched;
            timeService = (IGeoTimeService)Game.Services.GetService(typeof(IGeoTimeService));
        }

        #region Create the CeGui widgets

        /// <summary>
        /// add the buttons to the screen
        /// </summary>
        protected override void CreateCeguiWidgets()
        {
            // dimensions chosen to make 3D scene 512 x 512 at 600 x 800 resolution.
            SetView(0.02f, 0.12f, 0.641f, 0.8534f);

            // add text giving the time
            gameTimeText = GuiBuilder.CreateText(CeguiId + "_gameTimeText");
            AddWidget(gameTimeText, 0.7475f, 0.31f, 0.2275f, 0.04125f);

            // static text to show the string "set position of first base" message
            setFirstBaseTextWindow = GuiBuilder.CreateText(CeguiId + "_setFirstBase");
            AddWidget(setFirstBaseTextWindow, 0.02f, 0.06f, 0.641f, 0.04125f);
            setFirstBaseTextWindow.Text = Strings.SCREEN_GEOSCAPE_FIRST_BASE;
            setFirstBaseTextWindow.Visible = false;

            // the "Cancel selecting place for new base" button
            cancelNewBaseButton = AddButton("BUTTON_CANCEL_NEW_BASE", 0.02f, 0.06f, 0.641f, 0.04125f);
            cancelNewBaseButton.Visible = false;

            // change time buttons
            timeStopButton   = AddButton("BUTTON_TIME_STOP",   0.7475f, 0.36f, 0.1000f, 0.04125f);
            timeNormalButton = AddButton("BUTTON_TIME_X60",    0.8575f, 0.36f, 0.1000f, 0.04125f);
            timeHourButton   = AddButton("BUTTON_TIME_X3600",  0.7475f, 0.41f, 0.1000f, 0.04125f);
            timeDayButton    = AddButton("BUTTON_TIME_X86400", 0.8575f, 0.41f, 0.1000f, 0.04125f);
            
            interceptButton  = AddButton("BUTTON_INTERCEPT",     0.7475f, 0.46f, 0.2275f, 0.04125f);
            basesButton      = AddButton("BUTTON_BASES",         0.7475f, 0.51f, 0.2275f, 0.04125f);
            researchButton   = AddButton("BUTTON_RESEARCH",      0.7475f, 0.56f, 0.2275f, 0.04125f);
            logisticsButton  = AddButton("BUTTON_TRANSACTIONS",  0.7475f, 0.61f, 0.2275f, 0.04125f);
            productionButton = AddButton("BUTTON_MANUFACTURING", 0.7475f, 0.66f, 0.2275f, 0.04125f);
            statisticsButton = AddButton("BUTTON_STATISTICS",    0.7475f, 0.71f, 0.2275f, 0.04125f);
            xnetButton       = AddButton("BUTTON_XNET",          0.7475f, 0.76f, 0.2275f, 0.04125f);
            optionsButton    = AddButton("BUTTON_OPTIONS",       0.7475f, 0.81f, 0.2275f, 0.04125f);

            // move camera buttons
            cameraUpButton    = AddButton("BUTTON_UP",           0.7475f, 0.86f, 0.1000f, 0.04125f);
            cameraInButton    = AddButton("BUTTON_ZOOM_IN",      0.8575f, 0.86f, 0.1000f, 0.04125f);
            cameraDownButton  = AddButton("BUTTON_DOWN",         0.7475f, 0.91f, 0.1000f, 0.04125f);
            cameraOutButton   = AddButton("BUTTON_ZOOM_OUT",     0.8575f, 0.91f, 0.1000f, 0.04125f);
            cameraLeftButton  = AddButton("BUTTON_LEFT",         0.7475f, 0.96f, 0.1000f, 0.04125f);
            cameraRightButton = AddButton("BUTTON_RIGHT",        0.8575f, 0.96f, 0.1000f, 0.04125f);

            // change rate of time
            timeStopButton.Clicked   += new CeGui.GuiEventHandler(OnTimeRateButtonClicked);
            timeNormalButton.Clicked += new CeGui.GuiEventHandler(OnTimeRateButtonClicked);
            timeHourButton.Clicked   += new CeGui.GuiEventHandler(OnTimeRateButtonClicked);
            timeDayButton.Clicked    += new CeGui.GuiEventHandler(OnTimeRateButtonClicked);

            interceptButton.Clicked  += new CeGui.GuiEventHandler(OnInterceptButtonClicked);
            basesButton.Clicked      += new CeGui.GuiEventHandler(ShowBasesScreen);
            researchButton.Clicked   += new CeGui.GuiEventHandler(ShowResearchDialog);
            logisticsButton.Clicked  += new CeGui.GuiEventHandler(unimplemented);
            productionButton.Clicked += new CeGui.GuiEventHandler(unimplemented);
            statisticsButton.Clicked += new CeGui.GuiEventHandler(ShowStatisticsScreen);
            xnetButton.Clicked       += new CeGui.GuiEventHandler(ShowXNetScreen);
            optionsButton.Clicked    += new CeGui.GuiEventHandler(ShowOptionsDialog);

            cameraUpButton.Clicked    += new CeGui.GuiEventHandler(OnMoveCameraButtonClicked);
            cameraDownButton.Clicked  += new CeGui.GuiEventHandler(OnMoveCameraButtonClicked);
            cameraLeftButton.Clicked  += new CeGui.GuiEventHandler(OnMoveCameraButtonClicked);
            cameraRightButton.Clicked += new CeGui.GuiEventHandler(OnMoveCameraButtonClicked);
            cameraInButton.Clicked    += new CeGui.GuiEventHandler(OnMoveCameraButtonClicked);
            cameraOutButton.Clicked   += new CeGui.GuiEventHandler(OnMoveCameraButtonClicked);

            // handle special windows shown when we're adding a base
            cancelNewBaseButton.Clicked += new CeGui.GuiEventHandler(OnCancelNewBase);
            switch (state)
            {
                case GeoscapeScreenState.AddingBase:
                    cancelNewBaseButton.Visible = true;
                    break;

                case GeoscapeScreenState.AddingFirstBase:
                    setFirstBaseTextWindow.Visible = true;
                    break;

                case GeoscapeScreenState.ViewGeoscape:
                    // nothing to do
                    break;

                default:
                    Debug.Assert(false);
                    break;
            }
        }

        private CeGui.Widgets.StaticText setFirstBaseTextWindow;
        private CeGui.Widgets.PushButton cancelNewBaseButton;

        private CeGui.Widgets.PushButton timeStopButton;
        private CeGui.Widgets.PushButton timeNormalButton;
        private CeGui.Widgets.PushButton timeHourButton;
        private CeGui.Widgets.PushButton timeDayButton;

        private CeGui.Widgets.PushButton interceptButton;
        private CeGui.Widgets.PushButton basesButton;
        private CeGui.Widgets.PushButton researchButton;
        private CeGui.Widgets.PushButton logisticsButton;
        private CeGui.Widgets.PushButton productionButton;
        private CeGui.Widgets.PushButton statisticsButton;
        private CeGui.Widgets.PushButton xnetButton;
        private CeGui.Widgets.PushButton optionsButton;

        private CeGui.Widgets.PushButton cameraUpButton;
        private CeGui.Widgets.PushButton cameraDownButton;
        private CeGui.Widgets.PushButton cameraLeftButton;
        private CeGui.Widgets.PushButton cameraRightButton;
        private CeGui.Widgets.PushButton cameraInButton;
        private CeGui.Widgets.PushButton cameraOutButton;

        private CeGui.Widgets.StaticText gameTimeText;

        #endregion Create the CeGui widgets

        #region 3D scene management

        /// <summary>
        /// Update any model data
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            // update time shown on screen
            // Note, if display hasn't changed, don't write new value to text window
            // as this will make CeGui recompute all render quads.
            String newTime = timeService.ToString();
            if (gameTimeText.Text != newTime)
            {
                gameTimeText.Text = newTime;
            }
        }

        #endregion 

        #region event handlers

        /// <summary>Replace this screen on display with the Start Screen</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void ShowBasesScreen(object sender, CeGui.GuiEventArgs e)
        {
            if (GeoscapeScreenState.ViewGeoscape == state)
            {
                ScreenManager.ScheduleScreen(new BasesScreen(Game, 0));
            }
        }

        private void ShowResearchDialog(object sender, CeGui.GuiEventArgs e)
        {
            if (GeoscapeScreenState.ViewGeoscape == state)
            {
                ScreenManager.ShowDialog(new ResearchDialog(Game));
            }
        }

        /// <summary>Put the options dialog on the display</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void ShowOptionsDialog(object sender, CeGui.GuiEventArgs e)
        {
            if (GeoscapeScreenState.ViewGeoscape == state)
            {
                ScreenManager.ShowDialog(new OptionsDialog(Game));
            }
        }

        /// <summary>Replace this screen on display with the Statistics Screen</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void ShowStatisticsScreen(object sender, CeGui.GuiEventArgs e)
        {
            if (GeoscapeScreenState.ViewGeoscape == state)
            {
                ScreenManager.ScheduleScreen(new StatisticsScreen(Game));
            }
        }

        /// <summary>Replace this screen on display with the X-Net Screen</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void ShowXNetScreen(object sender, CeGui.GuiEventArgs e)
        {
            if (GeoscapeScreenState.ViewGeoscape == state)
            {
                ScreenManager.ScheduleScreen(new XNetScreen(Game));
            }
        }

        /// <summary>Launch interceptor from base to attack UFO</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void OnInterceptButtonClicked(object sender, CeGui.GuiEventArgs e)
        {
            if (GeoscapeScreenState.ViewGeoscape == state)
            {
                // currently this is a testing stub. Assumes there's a single UFO, Base and Aircraft
                // And that the aircraft is currently landed at the human base.
                if (0 < Xenocide.GameState.GeoData.Overmind.Ufos.Count)
                {
                    Ufo ufo = Xenocide.GameState.GeoData.Overmind.Ufos[0];
                    Aircraft aircraft = humanBaseService.HumanBases[0].Fleet[0];
                    aircraft.Mission.Abort();
                    aircraft.Mission = new InterceptMission(aircraft, ufo);
                }
            }
        }

        /// <summary>
        /// Return true if Button's is the specified button
        /// </summary>
        /// <param name="button">button to examine</param>
        /// <param name="resourceName">Name of resource string to look for</param>
        /// <returns>True if Button's Name ends with the specified resource string</returns>
        private static bool IsButton(CeGui.Widgets.PushButton button, string resourceName)
        {
            string label = Strings.ResourceManager.GetString(resourceName);
            Debug.Assert(!String.IsNullOrEmpty(label));
            return button.Name.EndsWith(label);
        }
        
        /// <summary>React to user clicking on one of the "move camera" buttons</summary>
        /// <param name="sender">Button the user clicked</param>
        /// <param name="e">Not used</param>
        private void OnMoveCameraButtonClicked(object sender, CeGui.GuiEventArgs e)
        {
            // turn an 1/8th of a revolution
            const float rotation = (float)(Math.PI / 4);
            
            // Step size to move camera in
            const float zoomStep = 0.5f;

            CeGui.Widgets.PushButton button = sender as CeGui.Widgets.PushButton;
            if (IsButton(button, "BUTTON_LEFT"))
            {
                Scene.RotateCamera(-rotation, 0.0f);
            }
            else if (IsButton(button, "BUTTON_RIGHT"))
            {
                Scene.RotateCamera(rotation, 0.0f);
            }
            else if (IsButton(button, "BUTTON_UP"))
            {
                Scene.RotateCamera(0.0f, rotation);
            }
            else if (IsButton(button, "BUTTON_DOWN"))
            {
                Scene.RotateCamera(0.0f, -rotation);
            }
            else if (IsButton(button, "BUTTON_ZOOM_IN"))
            {
                Scene.ZoomCamera(-zoomStep);
            }
            else if (IsButton(button, "BUTTON_ZOOM_OUT"))
            {
                Scene.ZoomCamera(zoomStep);
            }
        }

        /// <summary>React to user clicking on one of the "time rate" buttons</summary>
        /// <param name="sender">Button the user clicked</param>
        /// <param name="e">Not used</param>
        private void OnTimeRateButtonClicked(object sender, CeGui.GuiEventArgs e)
        {
            if (GeoscapeScreenState.ViewGeoscape == state)
            {
                CeGui.Widgets.PushButton button = sender as CeGui.Widgets.PushButton;
                float ratio = 0.0f;
                if (IsButton(button, "BUTTON_TIME_STOP"))
                {
                    ratio = 0.0f;
                }
                else if (IsButton(button, "BUTTON_TIME_X60"))
                {
                    ratio = 60.0f;
                }
                else if (IsButton(button, "BUTTON_TIME_X3600"))
                {
                    ratio = 3600.0f;
                }
                else if (IsButton(button, "BUTTON_TIME_X86400"))
                {
                    ratio = 86400.0f;
                }
                timeService.TimeRatio = ratio;
            }
        }

        /// <summary>React to user clicking left mouse button in the 3D geoscape scene</summary>
        /// <param name="e">Mouse information</param>
        protected override void OnLeftMouseDownInScene(CeGui.MouseEventArgs e)
        {
            Debug.Assert(e.Button == System.Windows.Forms.MouseButtons.Left);

            // convert co-ords from absolute to relative into the scene window
            PointF coords2 = SceneWindow.AbsoluteToRelative(new PointF(
                e.Position.X - SceneWindow.AbsoluteX,
                e.Position.Y - SceneWindow.AbsoluteY));
            GeoPosition pos = geoscapeScene.WindowToGeoPosition(coords2);
            if (pos != null)
            {
                switch (state)
                {
                    case GeoscapeScreenState.ViewGeoscape:
                        // left clicking sends craft on patrol mission to selected point
                        StartPatrolMission(pos);
                        break;

                    case GeoscapeScreenState.AddingFirstBase:
                    case GeoscapeScreenState.AddingBase:
                        // Build base at selected point
                        ConfirmBasePositionDialog(pos);
                        break;

                    default:
                        Debug.Assert(false);
                        break;
                }
            }
        }

        /// <summary>
        /// Testing stub, launch craft from human base 0 on patrol mission
        /// </summary>
        /// <param name="pos">Location to patrol</param>
        private void StartPatrolMission(GeoPosition pos)
        {
                Aircraft aircraft = humanBaseService.HumanBases[0].Fleet[0];
                aircraft.Mission.Abort();
                aircraft.Mission = new PatrolMission(aircraft, pos);
        }

        /// <summary>
        /// Confirm user wants to build base here
        /// </summary>
        /// <param name="pos">location of new base</param>
        private void ConfirmBasePositionDialog(GeoPosition pos)
        {
            // ToDo: get values
            string cost = "ToDo: calculate";
            string area = "ToDo: calculate";

            YesNoDialog dlg = YesNoDialog.OkCancelDialog(
                Game,
                Util.StringFormat(Strings.YESNOMSG_BUILD_BASE_HERE, cost, area)
            );

            // if yes is pressed, do the "base name" dialog
            dlg.YesAction += delegate()
            {
                ScreenManager.QueueDialog(new NameNewBaseDialog(Game, 
                    pos, 
                    (GeoscapeScreenState.AddingFirstBase == state)
                ));
            };

            ScreenManager.ShowDialog(dlg);
        }

        /// <summary>React to user clicking onthe "Cancel setting new base location" buttons</summary>
        /// <param name="sender">Button the user clicked</param>
        /// <param name="e">Not used</param>
        private void OnCancelNewBase(object sender, CeGui.GuiEventArgs e)
        {
            Debug.Assert(GeoscapeScreenState.AddingBase == state);
            cancelNewBaseButton.Visible = false;
            state = GeoscapeScreenState.ViewGeoscape;
        }

        #endregion event handlers

        private void TopicResearched(ResearchTopic topic)
        {
            ScreenManager.ShowDialog(new MessageBoxDialog(Game, Util.StringFormat(Strings.MSGBOX_RESEARCH_FINISHED, topic.Id)));
        }

        /// <summary>
        /// The state this screen is in
        /// </summary>
        public enum GeoscapeScreenState
        {
            /// <summary>
            /// View the geoscape mode (default)
            /// </summary>
            /// 
            ViewGeoscape,

            /// <summary>
            /// We're in "Add a base" mode
            /// </summary>
            AddingBase,

            /// <summary>
            /// We're adding the very first base.
            /// </summary>
            AddingFirstBase,
        }

        #region fields

        private IHumanBaseService humanBaseService;

        /// <summary>
        /// What mode are we in?
        /// </summary>
        public GeoscapeScreenState State
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
        private GeoscapeScreenState state;

        private IGeoTimeService timeService;

        #endregion fields
    }
}
