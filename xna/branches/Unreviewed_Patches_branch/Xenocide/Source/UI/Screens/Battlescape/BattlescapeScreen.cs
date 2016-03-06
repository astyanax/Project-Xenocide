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
* @file BattlescapeScreen.cs
* @date Created: 2007/12/30
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

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using Vector3 = Microsoft.Xna.Framework.Vector3;

using CeGui;


using ProjectXenocide.Utils;
using ProjectXenocide.UI.Dialogs;
using ProjectXenocide.UI.Scenes.Battlescape;
using ProjectXenocide.Model.Battlescape;
using ProjectXenocide.Model.Battlescape.Combatants;
using Xenocide.Resources;


#endregion

namespace ProjectXenocide.UI.Screens
{
    /// <summary>
    /// Screen that shows the battlescape
    /// </summary>
    public partial class BattlescapeScreen : Screen
    {
        /// <summary>
        /// Constructor (obviously)
        /// </summary>
        public BattlescapeScreen()
            : base("BattlescapeScreen", @"Content\Textures\UI\BasesScreenBackground.png")
        {
            Xenocide.AudioSystem.PlayRandomMusic("BaseView");
            ChangeState(new StartTurnScreenState(this));
        }

        /// <summary>
        /// Load the Scene's graphic content
        /// </summary>
        /// <param name="content">content manager that fetches the content</param>
        /// <param name="device">the display</param>
        
        public override void LoadContent(ContentManager content, GraphicsDevice device)
        {
            scene.LoadContent(device, battlescape);
            topLevel = battlescape.Terrain.Levels - 1;
        }

        /// <summary>
        /// Perform processing which updates the screen.
        /// </summary>
        /// <param name="gameTime">snapshot of timing values</param>
        public override void Update(GameTime gameTime)
        {
            state.Update(gameTime);
            scene.Update(gameTime);
        }

        /// <summary>
        /// Render the 3D scene
        /// </summary>
        /// <param name="gameTime">time interval since last render</param>
        /// <param name="device">Device to render the globe to</param>
        public override void Draw(GameTime gameTime, GraphicsDevice device)
        {
            scene.Draw(device, sceneWindow.Rect, topLevel, cursorPosition);
        }

        #region Create the CeGui widgets

        /// <summary>
        /// add the buttons to the screen
        /// </summary>
        protected override void CreateCeguiWidgets()
        {
            // Window indicating where the 3D scene is
            sceneWindow = GuiBuilder.CreateImage(CeguiId + "_viewport");
            AddWidget(sceneWindow, 0.00f, 0.00f, 0.745f, 1.00f);

            // text giving stats for selected combatant
            combatantStatsTextWindow = AddStaticText(0.0f, 0.0f, 0.745f, 0.065f);
            combatantStatsTextWindow.Hide();

            // other buttons
            equipmentButton = AddButton("BUTTON_EQUIPMENT", 0.7475f, 0.75f, 0.2275f, 0.04125f);
            rightHandButton = AddButton("BUTTON_RIGHT_HAND", 0.7475f, 0.80f, 0.2275f, 0.04125f);
            finishTurnButton = AddButton("BUTTON_FINISH_TURN", 0.7475f, 0.85f, 0.2275f, 0.04125f);
            topLevelButton = AddButton("BUTTON_TOP_LEVEL", 0.7475f, 0.90f, 0.2275f, 0.04125f);
            abortButton = AddButton("BUTTON_ABORT_MISSION", 0.7475f, 0.95f, 0.2275f, 0.04125f);

            // other buttons being pressed
            equipmentButton.Clicked += new CeGui.GuiEventHandler(OnEquipmentButton);
            rightHandButton.Clicked += new CeGui.GuiEventHandler(OnRightHandButton);
            finishTurnButton.Clicked += new CeGui.GuiEventHandler(OnFinishTurnButton);
            topLevelButton.Clicked += new CeGui.GuiEventHandler(OnTopLevelButton);
            abortButton.Clicked += new CeGui.GuiEventHandler(OnAbortButton);

            // mouse activity on screen
            sceneWindow.MouseMove += new CeGui.MouseEventHandler(OnMouseMoveInScene);
            sceneWindow.MouseButtonsDown += new CeGui.MouseEventHandler(OnMouseDownInScene);
        }

        /// <summary>
        /// CeGui widget that indicates where to draw the 3D scene
        /// </summary>
        private CeGui.Widgets.StaticImage sceneWindow;

        private CeGui.Widgets.StaticText combatantStatsTextWindow;

        private CeGui.Widgets.PushButton equipmentButton;
        private CeGui.Widgets.PushButton rightHandButton;
        private CeGui.Widgets.PushButton finishTurnButton;
        private CeGui.Widgets.PushButton topLevelButton;
        private CeGui.Widgets.PushButton abortButton;

        #endregion Create the CeGui widgets

        #region event handlers

        /// <summary>User has clicked the "Equipment" button</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void OnEquipmentButton(object sender, CeGui.GuiEventArgs e)
        {
            state.OnEquipmentButton();
        }

        /// <summary>User has clicked the "Right Hand" button</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void OnRightHandButton(object sender, CeGui.GuiEventArgs e)
        {
            state.OnHandButton(true);
        }

        /// <summary>User has clicked the "Finish Turn" button</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void OnFinishTurnButton(object sender, CeGui.GuiEventArgs e)
        {
            state.OnFinishTurnButton();
        }

        /// <summary>User has clicked the "Top Level" button</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void OnTopLevelButton(object sender, CeGui.GuiEventArgs e)
        {
            ToggleTopLevel();
        }

        /// <summary>User has clicked the "Abort Mission" button</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void OnAbortButton(object sender, CeGui.GuiEventArgs e)
        {
            state.OnAbortButton();
        }

        /// <summary>React to user moving the mouse in the 3D scene</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Mouse information</param>
        private void OnMouseMoveInScene(object sender, CeGui.MouseEventArgs e)
        {
            Vector3 newPosition = WindowPixelToBattlescapeCell(e.Position.X, e.Position.Y, topLevel);
            if (cursorPosition != newPosition)
            {
                cursorPosition = newPosition;
                state.OnMouseMoveInScene(cursorPosition);
            }
        }

        /// <summary>React to user clicking mouse in the 3D scene</summary>
        /// <param name="sender">CeGui widget sending the event</param>
        /// <param name="e">Mouse information</param>
        private void OnMouseDownInScene(object sender, CeGui.MouseEventArgs e)
        {
            // ignore clicks that are not on the terrain
            Vector3 newPosition = WindowPixelToBattlescapeCell(e.Position.X, e.Position.Y, topLevel);
            if (battlescape.Terrain.IsOnTerrain(newPosition))
            {
                if (e.Button == System.Windows.Forms.MouseButtons.Left)
                {
                    state.OnLeftMouseDownInScene(newPosition);
                }
                else
                {
                    state.OnRightMouseDownInScene(newPosition);
                }
            }
        }

        #endregion event handlers

        /// <summary>Change the State</summary>
        /// <param name="newState">new state</param>
        private void ChangeState(ScreenState newState)
        {
            if (null != state)
            {
                state.OnExitState();
            }
            state = newState;
            if (null != state)
            {
                Debug.WriteLine("Entering Battlescape state " + state.GetType().ToString());
                state.OnEnterState();
            }
        }

        /// <summary>Change the number of levels of the battlescape being shown</summary>
        private void ToggleTopLevel()
        {
            ++topLevel;
            if (battlescape.Terrain.Levels <= topLevel)
            {
                topLevel = 0;
            }
        }

        /// <summary>Battlescape is over</summary>
        /// <param name="finishType">Who won the battle</param>
        private void FinishMission(BattleFinish finishType)
        {
            Mission mission = battlescape.Mission;
            mission.OnFinish(battlescape, finishType);
            battlescape.PostMissionCleanup();
            Xenocide.GameState.Battlescape = null;
            ScreenManager.ScheduleScreen(new BattlescapeReportScreen(mission));
        }

        /// <summary>
        /// Convert position of pixel on screen to cell location on battlescape
        /// </summary>
        /// <param name="X">pixel's column</param>
        /// <param name="Y">pixel's row</param>
        /// <param name="level">level of the battlescape to find intersection at</param>
        /// <returns>corresponding position, or undefined if not on Battlescape</returns>
        private Vector3 WindowPixelToBattlescapeCell(float X, float Y, int level)
        {
            PointF coords2 = sceneWindow.AbsoluteToRelative(new PointF(
                X - sceneWindow.AbsoluteX,
                Y - sceneWindow.AbsoluteY));
            return scene.WindowToBattlescapeCell(coords2, level);
        }

        /// <summary>
        /// Update the combatant stats window with the stats for the currently selected combatant
        /// </summary>
        /// <param name="combatant">the selected combatant (null if there isn't one)</param>
        private void ShowCombatantStats(Combatant combatant)
        {
            if (null == combatant)
            {
                combatantStatsTextWindow.Hide();
            }
            else
            {
                combatantStatsTextWindow.Text = Util.StringFormat(Strings.SCREEN_BATTLESCAPE_COMBATANT_STATS,
                    combatant.Stats[Statistic.TimeUnitsLeft],
                    combatant.Stats[Statistic.TimeUnits],
                    combatant.Stats[Statistic.Health],
                    combatant.Stats[Statistic.InjuryDamage],
                    combatant.Stats[Statistic.StunDamage],
                    combatant.Stats[Statistic.Health] - combatant.Stats[Statistic.InjuryDamage] - combatant.Stats[Statistic.StunDamage]
                );
                combatantStatsTextWindow.Show();
            }
        }

        #region Fields

        /// <summary>The 3D view shown on the screen</summary>
        private BattlescapeScene scene = new BattlescapeScene();

        /// <summary>Topmost level of terrain to draw</summary>
        private int topLevel;

        /// <summary>The cell the mouse cursor is currently over</summary>
        private Vector3 cursorPosition = new Vector3();

        /// <summary>The battlescape</summary>
        private Battle battlescape = Xenocide.GameState.Battlescape;

        /// <summary>State the screen is in</summary>
        private ScreenState state;

        #endregion Fields
    }
}
