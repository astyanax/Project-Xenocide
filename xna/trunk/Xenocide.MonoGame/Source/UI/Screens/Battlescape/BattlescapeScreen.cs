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

using Gum.Forms;
using Gum.Forms.Controls;

using NLog;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using ProjectXenocide.Model.Battlescape;
using ProjectXenocide.Model.Battlescape.Combatants;
using ProjectXenocide.UI.Dialogs;
using ProjectXenocide.UI.Scenes.Battlescape;
using ProjectXenocide.Utils;

using Xenocide.Resources;

using Vector3 = Microsoft.Xna.Framework.Vector3;


#endregion

namespace ProjectXenocide.UI.Screens
{
    public partial class BattlescapeScreen : GumScreen
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public BattlescapeScreen()
            : base("BattlescapeScreen", @"Content/Textures/UI/BasesScreenBackground.png")
        {
            Xenocide.AudioSystem.PlayRandomMusic();
            ChangeState(new StartTurnScreenState(this));
        }

        public override void LoadContent(ContentManager content, GraphicsDevice device)
        {
            scene.LoadContent(device, battlescape);
            topLevel = battlescape.Terrain.Levels - 1;
        }

        public override void Update(GameTime gameTime)
        {
            state.Update(gameTime);
            scene.Update(gameTime);
            HandleMouseInput();
        }

        public override void Draw(GameTime gameTime, GraphicsDevice device)
        {
            base.Draw(gameTime, device);
            scene.Draw(device, _viewportRect, topLevel, cursorPosition);
        }

        private void HandleMouseInput()
        {
            var mouse = Mouse.GetState();
            var device = Xenocide.Instance.GraphicsDevice;

            int vpX = (int)(device.Viewport.Width * _viewportRect.Left);
            int vpY = (int)(device.Viewport.Height * _viewportRect.Top);
            int vpW = (int)(device.Viewport.Width * _viewportRect.Width);
            int vpH = (int)(device.Viewport.Height * _viewportRect.Height);

            bool inViewport = mouse.X >= vpX && mouse.X <= vpX + vpW
                           && mouse.Y >= vpY && mouse.Y <= vpY + vpH;

            if (!inViewport)
                return;

            float relX = (mouse.X - vpX) / (float)vpW;
            float relY = (mouse.Y - vpY) / (float)vpH;

            Vector3 newPosition = scene.WindowToBattlescapeCell(new UiPoint(relX, relY), topLevel);

            if (cursorPosition != newPosition)
            {
                cursorPosition = newPosition;
                state.OnMouseMoveInScene(cursorPosition);
            }

            if (_prevLeftPressed && mouse.LeftButton == ButtonState.Pressed)
                return;

            if (mouse.LeftButton == ButtonState.Pressed && !_prevLeftPressed)
            {
                if (battlescape.Terrain.IsOnTerrain(newPosition))
                    state.OnLeftMouseDownInScene(newPosition);
            }
            else if (mouse.RightButton == ButtonState.Pressed && !_prevRightPressed)
            {
                if (battlescape.Terrain.IsOnTerrain(newPosition))
                    state.OnRightMouseDownInScene(newPosition);
            }

            _prevLeftPressed = mouse.LeftButton == ButtonState.Pressed;
            _prevRightPressed = mouse.RightButton == ButtonState.Pressed;
        }

        #region Create the Gum controls

        protected override void CreateGumControls()
        {
            _viewportRect = new UiRect(0.00f, 0.00f, 0.745f, 1.00f);

            if (GumRoot != null)
            {
                combatantStatsTextWindow = new Label();
                combatantStatsTextWindow.Visual.Visible = false;

                WireButton("equipmentButton", OnEquipmentButton);
                WireButton("rightHandButton", OnRightHandButton);
                WireButton("finishTurnButton", OnFinishTurnButton);
                WireButton("topLevelButton", OnTopLevelButton);
                WireButton("abortButton", OnAbortButton);
                return;
            }

            combatantStatsTextWindow = new Label(); RootContainer.AddChild(combatantStatsTextWindow);
            combatantStatsTextWindow.Visual.Visible = false;

            equipmentButton = new Button() { Text = XenocideResourceManager.Get("BUTTON_EQUIPMENT") }; RootContainer.AddChild(equipmentButton);
            rightHandButton = new Button() { Text = XenocideResourceManager.Get("BUTTON_RIGHT_HAND") }; RootContainer.AddChild(rightHandButton);
            finishTurnButton = new Button() { Text = XenocideResourceManager.Get("BUTTON_FINISH_TURN") }; RootContainer.AddChild(finishTurnButton);
            topLevelButton = new Button() { Text = XenocideResourceManager.Get("BUTTON_TOP_LEVEL") }; RootContainer.AddChild(topLevelButton);
            abortButton = new Button() { Text = XenocideResourceManager.Get("BUTTON_ABORT_MISSION") }; RootContainer.AddChild(abortButton);

            equipmentButton.Click += OnEquipmentButton;
            rightHandButton.Click += OnRightHandButton;
            finishTurnButton.Click += OnFinishTurnButton;
            topLevelButton.Click += OnTopLevelButton;
            abortButton.Click += OnAbortButton;
        }

        private Label combatantStatsTextWindow;

        private Button equipmentButton;
        private Button rightHandButton;
        private Button finishTurnButton;
        private Button topLevelButton;
        private Button abortButton;

        #endregion Create the Gum controls

        #region event handlers

        private void OnEquipmentButton(object sender, EventArgs e)
        {
            state.OnEquipmentButton();
        }

        private void OnRightHandButton(object sender, EventArgs e)
        {
            state.OnHandButton(true);
        }

        private void OnFinishTurnButton(object sender, EventArgs e)
        {
            state.OnFinishTurnButton();
        }

        private void OnTopLevelButton(object sender, EventArgs e)
        {
            ToggleTopLevel();
        }

        private void OnAbortButton(object sender, EventArgs e)
        {
            state.OnAbortButton();
        }

        #endregion event handlers

        private void ChangeState(ScreenState newState)
        {
            if (null != state)
            {
                state.OnExitState();
            }
            state = newState;
            if (null != state)
            {
                Logger.Debug("Entering Battlescape state {0}", state.GetType());
                state.OnEnterState();
            }
        }

        private void ToggleTopLevel()
        {
            ++topLevel;
            if (battlescape.Terrain.Levels <= topLevel)
            {
                topLevel = 0;
            }
        }

        private void FinishMission(BattleFinish finishType)
        {
            Mission mission = battlescape.Mission;
            mission.OnFinish(battlescape, finishType);
            battlescape.PostMissionCleanup();
            Xenocide.GameState.Battlescape = null;
            ScreenManager.ScheduleScreen(new BattlescapeReportScreen(mission));
        }

        private void ShowCombatantStats(Combatant combatant)
        {
            if (null == combatant)
            {
                combatantStatsTextWindow.Visual.Visible = false;
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
                combatantStatsTextWindow.Visual.Visible = true;
            }
        }

        #region Fields

        private BattlescapeScene scene = new BattlescapeScene();
        private int topLevel;
        private Vector3 cursorPosition = new Vector3();
        private Battle battlescape = Xenocide.GameState.Battlescape;
        private ScreenState state;
        private UiRect _viewportRect;
        private bool _prevLeftPressed;
        private bool _prevRightPressed;

        #endregion Fields
    }
}
