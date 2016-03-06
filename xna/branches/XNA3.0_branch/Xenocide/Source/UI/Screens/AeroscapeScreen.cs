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
* @date Created: 2007/07/23
* @author File creator: David Teviotdale
* @author Credits: none
*/
#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

using Microsoft.Xna.Framework;

using CeGui;


using ProjectXenocide.Utils;
using ProjectXenocide.Model.Geoscape;
using ProjectXenocide.Model.Geoscape.Outposts;
using ProjectXenocide.Model.Geoscape.Vehicles;
using ProjectXenocide.Model.StaticData.Items;
using ProjectXenocide.Model.Battlescape;
using Xenocide.Resources;


#endregion

namespace ProjectXenocide.UI.Screens
{
    /// <summary>
    /// This is the screen where Ufos and Aircraft fight
    /// </summary>
    public class AeroscapeScreen : Screen
    {
        /// <summary>
        /// Constructor (obviously)
        /// </summary>
        /// <param name="aircraft">The human aircraft</param>
        /// <param name="ufo">The Ufo</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
            Justification = "Want it to throw if aircraft or ufo is null")]
        public AeroscapeScreen(Aircraft aircraft, Ufo ufo)
            : base("AeroscapeScreen")
        {
            this.aircraft = aircraft;
            this.ufo = ufo;

            aircraft.OnDogfightStart();
            ufo.OnDogfightStart();
        }

        #region Create the CeGui widgets

        /// <summary>
        /// add the buttons to the screen
        /// </summary>
        protected override void CreateCeguiWidgets()
        {
            // add text giving the craft name
            craftNameText = GuiBuilder.CreateText(CeguiId + "_craftNameText");
            AddWidget(craftNameText, 0.01f, 0.06f, 0.2275f, 0.04f);
            craftNameText.Text = aircraft.Name;

            // text giving craft damage
            craftDamageText = GuiBuilder.CreateText(CeguiId + "_craftDamageText");
            AddWidget(craftDamageText, 0.31f, 0.06f, 0.2275f, 0.04f);

            // text describing Pod 1
            pod1Text = GuiBuilder.CreateText(CeguiId + "_pod1Text");
            AddWidget(pod1Text, 0.01f, 0.12f, 0.2275f, 0.16125f);

            // text describing Pod 2
            pod2Text = GuiBuilder.CreateText(CeguiId + "_pod2Text");
            AddWidget(pod2Text, 0.31f, 0.12f, 0.2275f, 0.16125f);

            // events this turn
            logText = GuiBuilder.CreateText(CeguiId + "_logText");
            AddWidget(logText, 0.31f, 0.30f, 0.70f, 0.60f);

            DrawScreen();

            // buttons
            realTimeButton = AddButton("BUTTON_REAL_TIME", 0.7475f, 0.85f, 0.2275f, 0.04125f);
            advanceTimeButton = AddButton("BUTTON_ADVANCE_TIME", 0.7475f, 0.90f, 0.2275f, 0.04125f);
            closeButton = AddButton("BUTTON_CLOSE", 0.7475f, 0.95f, 0.2275f, 0.04125f);

            realTimeButton.Clicked += new CeGui.GuiEventHandler(OnRealTimeButton);
            advanceTimeButton.Clicked += new CeGui.GuiEventHandler(OnAdvanceTimeButton);
            closeButton.Clicked += new CeGui.GuiEventHandler(OnCloseButton);
        }

        private CeGui.Widgets.StaticText craftNameText;
        private CeGui.Widgets.StaticText craftDamageText;
        private CeGui.Widgets.StaticText pod1Text;
        private CeGui.Widgets.StaticText pod2Text;
        private CeGui.Widgets.StaticText logText;
        private CeGui.Widgets.PushButton realTimeButton;
        private CeGui.Widgets.PushButton advanceTimeButton;
        private CeGui.Widgets.PushButton closeButton;

        #endregion Create the CeGui widgets

        #region event handlers

        /// <summary>Handle user clicking on the "Real Time" button</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void OnRealTimeButton(object sender, GuiEventArgs e)
        {
            runRealTime = true;
        }

        /// <summary>Handle user clicking on the "Advance Time" button</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void OnAdvanceTimeButton(object sender, GuiEventArgs e)
        {
            runRealTime = false;
            UpdateDogfight();
        }

        /// <summary>React to user pressing the Close button</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:DisposeObjectsBeforeLosingScope",
           Justification = "FxCop False Positive")]
        private void OnCloseButton(object sender, CeGui.GuiEventArgs e)
        {
            // Tell UFO & aircraft that fight's over
            if (!ufo.IsDestroyed)
            {
                ufo.OnDogfightFinished();
            }
            if (!aircraft.IsDestroyed)
            {
                aircraft.OnDogfightFinished();
            }

            ScreenManager.ScheduleScreen(new GeoscapeScreen());
        }

        #endregion event handlers

        /// <summary>
        /// Hook the main message pump
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
            Justification = "will throw if gameTime is null")]
        public override void Update(GameTime gameTime)
        {
            // if we're running this dialog "real time" and sufficient time has passed
            // then do another "round" of the aeroscape battle
            if (runRealTime)
            {
                elapsed += gameTime.ElapsedGameTime.TotalMilliseconds;
                if (1000.0 < elapsed)
                {
                    UpdateDogfight();
                }
            }
        }

        /// <summary>
        /// Draw everything on the screen
        /// </summary>
        private void DrawScreen()
        {
            DrawAircraftDamage();
            DrawPodInformation();
            DrawLog();
        }

        /// <summary>
        /// Show the state of the the human craft's hull
        /// </summary>
        private void DrawAircraftDamage()
        {
            craftDamageText.Text =
                Util.StringFormat(Strings.SCREEN_AEROSCAPE_HULL_LEFT, aircraft.HullPercent);
        }

        /// <summary>
        /// Show the information for the weapon pods for a craft
        /// </summary>
        private void DrawPodInformation()
        {
            // Start assuming craft has no pods
            pod1Text.Hide();
            pod2Text.Hide();

            // Document Pod 1, if it exists
            if (0 < aircraft.WeaponPods.Count)
            {
                DrawPodInformation(pod1Text, aircraft.WeaponPods[0], 1);
            }

            // Document Pod 2, if it exists
            if (1 < aircraft.WeaponPods.Count)
            {
                DrawPodInformation(pod2Text, aircraft.WeaponPods[1], 2);
            }
        }

        /// <summary>
        /// Update the details for a specific weapon pod
        /// </summary>
        /// <param name="textControl">Control to write details to</param>
        /// <param name="pod">Pod to get information for</param>
        /// <param name="podId">Pod 1 or Pod 2?</param>
        private static void DrawPodInformation(CeGui.Widgets.StaticText textControl, WeaponPod pod, int podId)
        {
            textControl.Show();
            StringBuilder info = new StringBuilder(Util.StringFormat(Strings.SCREEN_EQUIP_CRAFT_POD_NAME, podId));
            info.Append(Util.Linefeed);
            // can't just call pod.PodInformationString(), because pod may be null
            info.Append(WeaponPod.PodInformationString(pod));
            textControl.Text = info.ToString();
        }

        /// <summary>
        /// Show what happened in the last second of the fight
        /// </summary>
        private void DrawLog()
        {
            StringBuilder sb = new StringBuilder(Util.StringFormat(Strings.SCREEN_AEROSCAPE_LOG_TIME, log.Now));
            foreach (BattleLog.LogEntry entry in log.Entries)
            {
                if (entry.Time == log.Now)
                {
                    sb.Append(Util.Linefeed);
                    sb.Append(entry.Details);
                }
            }
            logText.Text = sb.ToString();
        }

        /// <summary>
        /// Update the dogfight's state
        /// </summary>
        private void UpdateDogfight()
        {
            // we're starting a new round
            elapsed = 0.0;

            // if fight's over, nothing to do
            if (!dogfightOver)
            {
                // repeat until something happens
                int logsize = log.Entries.Count;
                do
                {
                    // prepare log for events
                    log.UpdateTime(1.0);

                    // Aircraft and UFO trade blows
                    Attack(aircraft, ufo);
                    if (!dogfightOver)
                    {
                        // unarmed ufos can't attack
                        if (ufo.IsArmed)
                        {
                            Attack(ufo, aircraft);
                        }
                    }
                } while (!dogfightOver && log.Entries.Count == logsize);
                DrawScreen();
            }
        }

        /// <summary>
        /// Have one craft try to attack other
        /// </summary>
        /// <param name="attacker">craft doing the shooting</param>
        /// <param name="target">craft getting hit</param>
        private void Attack(Craft attacker, Craft target)
        {
            AttackResult result = attacker.Attack(target, log);
            switch (result)
            {
                case AttackResult.OpponentCrashed:
                case AttackResult.OpponentDestroyed:
                case AttackResult.OpponentFled:
                case AttackResult.OutOfAmmo:
                    dogfightOver = true;
                    break;

                case AttackResult.Nothing:
                    // Nothing to do
                    break;

                default:
                    // should never get here
                    Debug.Assert(false);
                    break;
            }
        }

        #region Fields

        /// <summary>
        /// Has the fight finished?
        /// </summary>
        private bool dogfightOver;

        /// <summary>
        /// The UFO
        /// </summary>
        private Ufo ufo;

        /// <summary>
        /// The human craft
        /// </summary>
        private Aircraft aircraft;

        /// <summary>
        /// What happens during the battle
        /// </summary>
        private BattleLog log = new BattleLog();

        /// <summary>
        /// Run aeroscape battle in "real time"
        /// </summary>
        bool runRealTime;

        /// <summary>
        /// Number of milliseconds that have elapsesed since last combat "round"
        /// </summary>
        double elapsed;

        #endregion Fields
    }
}
