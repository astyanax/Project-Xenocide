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
* @file GeoscapeScreenStates.cs
* @date Created: 2007/01/21
* @author File creator: dteviot
* @author Credits: none
*/
#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;

using Gum.Forms.Controls;

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
    /*
      This file holds the Geoscape's nested ScreenState classes
    */

    public partial class GeoscapeScreen : PolarScreen
    {
        /// <summary>
        /// Control behaviour, based on state screen is in.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")]
        public abstract class ScreenState
        {
            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="geoscapeScreen">The parent geoscape</param>
            protected ScreenState(GeoscapeScreen geoscapeScreen)
            {
                this.geoscapeScreen = geoscapeScreen;
            }

            /// <summary>
            /// Create the widgets shown when in this state
            /// </summary>
            public virtual void CreateGumControls() { }

            /// <summary>
            /// Update any model data
            /// </summary>
            /// <param name="gameTime">Provides a snapshot of timing values.</param>
            public virtual void Update(GameTime gameTime)
            {
                // default behaviour is do nothing
            }

            /// <summary>
            /// React to "Bases" button being pressed
            /// </summary>
            public virtual void OnBasesButton()
            {
                // default behaviour is do nothing
            }

            /// <summary>
            /// React to "Research" button being pressed
            /// </summary>
            public virtual void OnResearchButton()
            {
                // default behaviour is do nothing
            }

            /// <summary>
            /// React to "Options" button being pressed
            /// </summary>
            public virtual void OnOptionsButton()
            {
                // default behaviour is do nothing
            }

            /// <summary>
            /// React to "Funding" button being pressed
            /// </summary>
            public virtual void OnFundingButton()
            {
                // default behaviour is do nothing
            }

            /// <summary>
            /// React to "Statistics" button being pressed
            /// </summary>
            public virtual void OnStatisticsButton()
            {
                // default behaviour is do nothing
            }

            /// <summary>
            /// React to "X-Net" button being pressed
            /// </summary>
            public virtual void OnXNetButton()
            {
                // default behaviour is do nothing
            }

            /// <summary>
            /// React to "Intercept" button being pressed
            /// </summary>
            public virtual void OnInterceptButton()
            {
                // default behaviour is do nothing
            }

            /// <summary>React to user clicking on one of the "time rate" buttons</summary>
            /// <param name="button">Button the user clicked</param>
            public virtual void OnTimeRateButtonClicked(Button button)
            {
                // default behaviour is do nothing
            }

            /// <summary>React to user clicking left mouse button in the 3D geoscape scene</summary>
            /// <param name="pos">Position on earth where mouse was clicked</param>
            public virtual void OnLeftMouseDownInScene(GeoPosition pos)
            {
            }

            #region Fields

            /// <summary>
            /// The parent geoscape
            /// </summary>
            public GeoscapeScreen GeoscapeScreen { get { return geoscapeScreen; } }

            /// <summary>
            /// The parent geoscape
            /// </summary>
            private GeoscapeScreen geoscapeScreen;

            #endregion Fields
        }

        /// <summary>
        /// Screen behaviour, when we're just viewing the Geoscape
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")]
        public class ViewGeoscapeScreenState : ScreenState
        {
            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="geoscapeScreen">The parent geoscape</param>
            public ViewGeoscapeScreenState(GeoscapeScreen geoscapeScreen) : base(geoscapeScreen) { }

            /// <summary>
            /// Create extra debug button for view state
            /// </summary>
            public override void CreateGumControls()
            {
                if (Xenocide.StaticTables.StartSettings.Cheats.ControlAlienMissions)
                {
                    alienMissionButton = new Button() { Text = XenocideResourceManager.Get("BUTTON_ALIEN_MISSION") };
                    alienMissionButton.Click += (s, e) => OnAlienMissionsClicked(s, e);
                    GeoscapeScreen.AddControl(alienMissionButton);
                }
            }

            /// <summary>
            /// Update any model data
            /// </summary>
            /// <param name="gameTime">Provides a snapshot of timing values.</param>
            public override void Update(GameTime gameTime)
            {
                Xenocide.GameState.GeoData.Update(gameTime);
            }

            /// <summary>
            /// React to "Bases" button being pressed
            /// </summary>
            public override void OnBasesButton()
            {
                // bring up the bases screen
                GeoscapeScreen.ScreenManager.ScheduleScreen(new BasesScreen(0));
            }

            /// <summary>
            /// React to "Research" button being pressed
            /// </summary>
            public override void OnResearchButton()
            {
                // bring up the research screen
                GeoscapeScreen.ScreenManager.ScheduleScreen(new ResearchScreen());
            }

            /// <summary>
            /// React to "Options" button being pressed
            /// </summary>
            public override void OnOptionsButton()
            {
                // bring up the options screen
                GeoscapeScreen.ScreenManager.ShowDialog(new GumOptionsDialog());
            }

            /// <summary>
            /// React to "Funding" button being pressed
            /// </summary>
            public override void OnFundingButton()
            {
                // bring up the funding screen
                GeoscapeScreen.ScreenManager.ScheduleScreen(new MonthlyReportScreen(false));
            }

            /// <summary>
            /// React to "Statistics" button being pressed
            /// </summary>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:DisposeObjectsBeforeLosingScope",
               Justification = "FxCop False Positive")]
            public override void OnStatisticsButton()
            {
                // bring up the statistics screen
                GeoscapeScreen.ScreenManager.ScheduleScreen(new StatisticsScreen());
            }

            /// <summary>
            /// React to "X-Net" button being pressed
            /// </summary>
            public override void OnXNetButton()
            {
                // bring up the X-Net screen
                GeoscapeScreen.ScreenManager.ScheduleScreen(new XNetScreen());
            }

            /// <summary>
            /// React to "Intercept" button being pressed
            /// </summary>
            public override void OnInterceptButton()
            {
                // bring up the launch intercept Dialog
                GeoscapeScreen.ScreenManager.ShowDialog(new LaunchInterceptDialog());
            }

            /// <summary>React to user clicking on one of the "time rate" buttons</summary>
            /// <param name="button">Button the user clicked</param>
            public override void OnTimeRateButtonClicked(Button button)
            {
                float ratio = 0.0f;
                if (GeoscapeScreen.IsButton(button, "BUTTON_TIME_STOP"))
                {
                    ratio = 0.0f;
                }
                else if (GeoscapeScreen.IsButton(button, "BUTTON_TIME_X60"))
                {
                    ratio = 60.0f;
                }
                else if (GeoscapeScreen.IsButton(button, "BUTTON_TIME_X3600"))
                {
                    ratio = 3600.0f;
                }
                else if (GeoscapeScreen.IsButton(button, "BUTTON_TIME_X86400"))
                {
                    ratio = 86400.0f;
                }
                Xenocide.GameState.GeoData.GeoTime.TimeRatio = ratio;
            }

            /// <summary>React to user clicking the AlienMission button</summary>
            /// <param name="sender">unused</param>
            /// <param name="e">unused</param>
            private void OnAlienMissionsClicked(object sender, EventArgs e)
            {
                GeoscapeScreen.ScreenManager.ShowDialog(new AlienMissionDialog());
            }

            private Button alienMissionButton;
        }

        /// <summary>
        /// Screen behaviour, when we're adding a base
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")]
        public class AddingBaseScreenState : ScreenState
        {
            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="geoscapeScreen">The parent geoscape</param>
            public AddingBaseScreenState(GeoscapeScreen geoscapeScreen) : base(geoscapeScreen) { }

            /// <summary>
            /// Create the widgets shown when in this state
            /// </summary>
            public override void CreateGumControls()
            {
                GeoscapeScreen.EnableButtonSounds = false;

                cancelNewBaseButton = new Button() { Text = XenocideResourceManager.Get("BUTTON_CANCEL_NEW_BASE") };
                cancelNewBaseButton.Click += (s, e) => GeoscapeScreen.OnCancelNewBase(s, e);
                GeoscapeScreen.AddControl(cancelNewBaseButton);
            }

            /// <summary>React to user clicking left mouse button in the 3D geoscape scene</summary>
            /// <param name="pos">Position on earth where mouse was clicked</param>
            public override void OnLeftMouseDownInScene(GeoPosition pos)
            {
                Xenocide.AudioSystem.PlaySound(SoundId.PlanetViewClickObject);
                // Build base at selected point
                GeoscapeScreen.ConfirmBasePositionDialog(pos, false);
            }

            private Button cancelNewBaseButton;
        }

        /// <summary>
        /// Screen behaviour, when we're adding the first base
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")]
        public class AddingFirstBaseScreenState : ScreenState
        {
            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="geoscapeScreen">The parent geoscape</param>
            public AddingFirstBaseScreenState(GeoscapeScreen geoscapeScreen) : base(geoscapeScreen) { }

            /// <summary>
            /// Create the widgets shown when in this state
            /// </summary>
            public override void CreateGumControls()
            {
                GeoscapeScreen.EnableButtonSounds = false;

                setFirstBaseTextWindow = new Label() { Text = Strings.SCREEN_GEOSCAPE_FIRST_BASE };
                GeoscapeScreen.AddControl(setFirstBaseTextWindow);
            }

            /// <summary>React to user clicking left mouse button in the 3D geoscape scene</summary>
            /// <param name="pos">Position on earth where mouse was clicked</param>
            public override void OnLeftMouseDownInScene(GeoPosition pos)
            {
                Xenocide.AudioSystem.PlaySound(SoundId.PlanetViewClickObject);
                // Build base at selected point
                GeoscapeScreen.ConfirmBasePositionDialog(pos, true);
            }

            private Label setFirstBaseTextWindow;
        }

        /// <summary>
        /// Screen behaviour, when we're setting an intercept target
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")]
        public class TargetingScreenState : ScreenState
        {
            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="geoscapeScreen">The parent geoscape</param>
            /// <param name="craft">The craft we're setting a destination for</param>
            public TargetingScreenState(GeoscapeScreen geoscapeScreen, Craft craft)
                :
                base(geoscapeScreen)
            {
                this.craft = craft;
            }

            /// <summary>
            /// Create the widgets shown when in this state
            /// </summary>
            public override void CreateGumControls()
            {
                GeoscapeScreen.EnableButtonSounds = false;

                cancelTargetingButton = new Button() { Text = XenocideResourceManager.Get("BUTTON_CANCEL_TARGETING") };
                cancelTargetingButton.Click += OnCancelTargeting;
                GeoscapeScreen.AddControl(cancelTargetingButton);
            }

            /// <summary>React to user clicking left mouse button in the 3D geoscape scene</summary>
            /// <param name="pos">Position on earth where mouse was clicked</param>
            public override void OnLeftMouseDownInScene(GeoPosition pos)
            {
                Xenocide.AudioSystem.PlaySound(SoundId.PlanetViewClickObject);
                Craft ufo = FindClosestUfo(pos);
                AlienSite site = FindClosestAlienSite(pos);
                if ((null == ufo) && (null == site))
                {
                    // no ufo or site, so target the position
                    TargetGeoposition(pos);
                }
                else if ((ufo != null) &&
                    ((site == null) || (pos.Distance(ufo.Position) <= pos.Distance(site.Position)))
                )
                {
                    // no site or UFO closer than site, so target the UFO
                    Target(ufo);
                }
                else
                {
                    Target(site);
                }
            }

            /// <summary>
            /// Return the UFO that is closest to position on the Geoscape.
            /// <remarks>
            /// 1. UFOs more than 500km away from position will be ignored.</remarks>
            /// </summary>
            /// <param name="pos">center of search</param>
            /// <returns>closest UFO, or null if nothing found</returns>
            private static Ufo FindClosestUfo(GeoPosition pos)
            {
                return pos.FindClosest(Xenocide.GameState.GeoData.Overmind.Ufos, FindClosestMaxDistance);
            }

            /// <summary>
            /// Return the AlienSite that is closest to position on the Geoscape.
            /// <remarks>
            /// 1. Sites more than 500km away from position will be ignored.</remarks>
            /// </summary>
            /// <param name="pos">center of search</param>
            /// <returns>closest Alien site, or null if nothing found</returns>
            private static AlienSite FindClosestAlienSite(GeoPosition pos)
            {
                return pos.FindClosest(Xenocide.GameState.GeoData.Overmind.Sites, FindClosestMaxDistance);
            }

            /// <summary>
            /// If player approves, set UFO as craft's target
            /// </summary>
            /// <param name="ufo">Ufo to set as target</param>
            private void Target(Craft ufo)
            {
                GumYesNoDialog dlg = new GumYesNoDialog(
                    Util.StringFormat(Strings.YESNOMSG_TARGET_UFO, ufo.Name)
                );

                // if yes is pressed, send aircraft after the ufo and exit target mode
                dlg.YesAction += delegate ()
                {
                    craft.Mission.Abort();
                    craft.Mission = new InterceptMission(craft, ufo);
                    GeoscapeScreen.ScreenManager.ScheduleScreen(new GeoscapeScreen());
                };

                Xenocide.ScreenManager.ShowDialog(dlg);
            }

            /// <summary>
            /// If player approves, set Alien Site as craft's target
            /// </summary>
            /// <param name="site">AlienSite to set as target</param>
            private void Target(AlienSite site)
            {
                // can only target ground sites if craft is carrying soldiers
                if (craft.IsCarryingSoldiers)
                {
                    GumYesNoDialog dlg = new GumYesNoDialog(
                        Util.StringFormat(Strings.YESNOMSG_TARGET_ALIEN_SITE, site.Name)
                    );

                    // if yes is pressed, send aircraft after the site and exit target mode
                    dlg.YesAction += delegate ()
                    {
                        craft.Mission.Abort();
                        craft.Mission = new AttackAlienSiteMission(craft, site);
                        GeoscapeScreen.ScreenManager.ScheduleScreen(new GeoscapeScreen());
                    };

                    Xenocide.ScreenManager.ShowDialog(dlg);
                }
                else
                {
                    Util.ShowMessageBox(Strings.MSGBOX_NOT_CARRYING_SOLDIERS, craft.Name, site.Name);
                }
            }

            /// <summary>
            /// If player approves, set Geoposition as craft's target
            /// </summary>
            /// <param name="pos">GeoPosition to set as target</param>
            private void TargetGeoposition(GeoPosition pos)
            {
                GumYesNoDialog dlg = new GumYesNoDialog(Strings.YESNOMSG_TARGET_GEOPOSITION);

                // if yes is pressed, send aircraft to the location and exit target mode
                dlg.YesAction += delegate ()
                {
                    craft.Mission.Abort();
                    craft.Mission = new PatrolMission(craft, pos);
                    GeoscapeScreen.ScreenManager.ScheduleScreen(new GeoscapeScreen());
                };

                Xenocide.ScreenManager.ShowDialog(dlg);
            }

            /// <summary>User has clicked on the "Cancel Target Selection" button</summary>
            /// <param name="sender">Not used</param>
            /// <param name="e">Not used</param>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:DisposeObjectsBeforeLosingScope",
               Justification = "FxCop False Positive")]
            private void OnCancelTargeting(object sender, EventArgs e)
            {
                // put Geoscape back in "view anything" mode
                GeoscapeScreen.ScreenManager.ScheduleScreen(new GeoscapeScreen());
            }

            /// <summary>
            /// The "Cancel target selection" button
            /// </summary>
            private Button cancelTargetingButton;

            #region Fields

            /// <summary>
            /// Maximum distance to look at when using FindClosestXXXXX() functions.
            /// Currently 500 km.
            /// </summary>
            private static readonly double FindClosestMaxDistance = GeoPosition.KilometersToRadians(501);

            /// <summary>
            /// The craft we're setting a destination for
            /// </summary>
            Craft craft;

            #endregion Fields
        }

        /// <summary>
        /// Screen behaviour, when selecting an alien mission site
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")]
        public class TargetAlienMissionState : ScreenState
        {
            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="geoscapeScreen">The parent geoscape</param>
            /// <param name="missionType">The mission type to create</param>
            public TargetAlienMissionState(GeoscapeScreen geoscapeScreen, AlienMission missionType)
                : base(geoscapeScreen)
            {
                this.missionType = missionType;
            }

            /// <summary>
            /// Create the widgets shown when in this state
            /// </summary>
            public override void CreateGumControls()
            {
                GeoscapeScreen.EnableButtonSounds = false;

                cancelTargetButton = new Button() { Text = XenocideResourceManager.Get("BUTTON_CANCEL_ALIEN_MISSION") };
                cancelTargetButton.Click += OnCancelAlienMission;
                GeoscapeScreen.AddControl(cancelTargetButton);
            }

            /// <summary>React to user clicking left mouse button in the 3D geoscape scene</summary>
            /// <param name="pos">Position on earth where mouse was clicked</param>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:DisposeObjectsBeforeLosingScope",
                Justification = "FxCop false positive")]
            public override void OnLeftMouseDownInScene(GeoPosition pos)
            {
                // Generate mission at selected point
                Xenocide.GameState.GeoData.Overmind.DebugCreateMission(missionType, pos);
                GeoscapeScreen.ScreenManager.ScheduleScreen(new GeoscapeScreen());
            }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:DisposeObjectsBeforeLosingScope",
                Justification = "FxCop false positive")]
            private void OnCancelAlienMission(object sender, EventArgs e)
            {
                GeoscapeScreen.ScreenManager.ScheduleScreen(new GeoscapeScreen());
            }

            private Button cancelTargetButton;
            private AlienMission missionType;
        }
    }
}
