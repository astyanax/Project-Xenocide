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
* @file BasesScreen.cs
* @date Created: 2007/01/21
* @author File creator: David Teviotdale
* @author Credits: none
*/
#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

using Gum.Forms;
using Gum.Forms.Controls;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using NLog;

using ProjectXenocide.Assets;
using ProjectXenocide.Model.Geoscape.Outposts;
using ProjectXenocide.Model.StaticData.Facilities;
using ProjectXenocide.UI;
using ProjectXenocide.UI.Dialogs;
using ProjectXenocide.UI.Scenes.Facility;
using ProjectXenocide.Utils;

using Xenocide.Resources;


#endregion

namespace ProjectXenocide.UI.Screens
{
    /// <summary>
    /// Screen that shows the layout of facilities in a X-Corp Outpost (Base)
    ///
    /// STATE MACHINE (BasesScreenState):
    ///   NotAdding    — normal mode, right-click demolishes
    ///   AddAccessLift — auto-entered when building into an empty base
    ///   AddFacility  — user selected a facility from BuildFacilityDialog
    ///
    /// PLACEMENT FLOW:
    ///   BuildFacilityDialog → BuildFacility(handle) → State = AddFacility
    ///   SceneMouseHandler.MouseMoved → OnSceneMouseMoved → UpdateNewFacilityPosition
    ///   FacilityScene.Draw renders ghost (green=valid, red=invalid)
    ///   Left-click → AddFacility(cell) → validates → debits → adds to Floorplan
    ///
    /// INITIALIZATION ORDER (ScreenManager.SwapScreens):
    ///   1. Constructor (BasesScreen) — creates FacilityScene
    ///   2. LoadContent — creates SceneMouseHandler using sceneWindowRect
    ///   3. Show / CreateGumControls — sets sceneWindowRect
    ///   Because LoadContent runs BEFORE Show (see ScreenManager.cs:147-149),
    ///   sceneWindowRect must be initialized at field declaration so the
    ///   SceneMouseHandler receives a non-zero viewport rect. See field below.
    ///
    /// LEAK PREVENTION:
    ///   sceneMouseHandler.Reset() is called in the State setter so that
    ///   the button-down state from a dialog "Select" click doesn't
    ///   leak through as a spurious LeftClicked event on the first frame.
    /// </summary>
    public partial class BasesScreen : GumScreen
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Constructor (obviously)
        /// </summary>
        /// <param name="selectedBase">Index to X-Corp outpost screen is to show</param>
        public BasesScreen(int selectedBase)
            : base("BasesScreen", @"Content/Textures/UI/BaseDirtFloor.png")
        {
            this.selectedBase = selectedBase;
            this.controller = new Controller(Xenocide.GameState.GeoData.Outposts[selectedBase]);
            Logger.Info("BasesScreen ctor: baseIndex={0}", selectedBase);

            // Before showing, bring floorplan up to date 
            scene = new FacilityScene(SelectedBaseFloorplan);
            if (Xenocide.AudioSystem != null)
                Xenocide.AudioSystem.PlayRandomMusic("BaseView");
        }

        /// <summary>
        /// Load the Scene's graphic content
        /// </summary>
        /// <param name="content">content manager that fetches the content</param>
        /// <param name="device">the display</param>

        public override void LoadContent(ContentManager content, GraphicsDevice device)
        {
            scene.LoadContent(content, device);

            Logger.Info("LoadContent: sceneWindowRect=({0},{1},{2},{3})",
                sceneWindowRect.Left, sceneWindowRect.Top, sceneWindowRect.Right, sceneWindowRect.Bottom);

            // Create the mouse handler here (not in Update) so it exists before
            // the first frame.  This ensures Reset() can be called during state
            // transitions even before the first Update() call.
            // IMPORTANT: sceneWindowRect must be initialized BEFORE this call
            // (it is set as a field initializer at declaration time) because
            // ScreenManager.SwapScreens() calls LoadContent() before Show().
            sceneMouseHandler = new SceneMouseHandler(sceneWindowRect);
            sceneMouseHandler.MouseMoved += OnSceneMouseMoved;
            sceneMouseHandler.LeftClicked += OnSceneLeftClicked;
            sceneMouseHandler.RightClicked += OnSceneRightClicked;
        }

        /// <summary>
        /// Perform processing which updates the screen.
        /// Called every frame.  Delegates mouse input to the SceneMouseHandler,
        /// which polls Mouse.GetState() and fires events when the cursor interacts
        /// with the 3D viewport region.
        /// </summary>
        /// <param name="gameTime">snapshot of timing values</param>
        public override void Update(GameTime gameTime)
        {
            sceneMouseHandler.Update();

            // Handle keyboard shortcuts with proper key-down detection
            var keyboard = Microsoft.Xna.Framework.Input.Keyboard.GetState();
            bool ctrlZPressed = keyboard.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.LeftControl) &&
                                keyboard.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Z) &&
                                _prevKeyboardState.IsKeyUp(Microsoft.Xna.Framework.Input.Keys.Z);

            if (ctrlZPressed)
            {
                UndoDemolition();
            }
            else if (state == BasesScreenState.NotAdding)
            {
                // B key = open build dialog (same as clicking Build Facilities button)
                if (keyboard.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.B) &&
                    _prevKeyboardState.IsKeyUp(Microsoft.Xna.Framework.Input.Keys.B))
                {
                    OnBuildFacilitiesButton(this, EventArgs.Empty);
                }
            }
            else if (state == BasesScreenState.AddFacility)
            {
                // Backspace = cancel placement (in addition to Escape)
                if (keyboard.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Back) &&
                    _prevKeyboardState.IsKeyUp(Microsoft.Xna.Framework.Input.Keys.Back))
                {
                    CancelFacility();
                }
            }

            _prevKeyboardState = keyboard;

            // Create tooltip lazily on first update if Gum is ready
            if (tooltip == null && (GumRoot != null || RootContainer != null))
            {
                var tooltipRoot = GumRoot ?? RootContainer?.Visual;
                if (tooltipRoot != null)
                {
                    tooltip = new FacilityTooltip(tooltipRoot);
                }
            }

            // Hide tooltip if mouse leaves the scene viewport
            var mouse = Microsoft.Xna.Framework.Input.Mouse.GetState();
            var device = Xenocide.Instance.GraphicsDevice;
            int vpX = (int)(device.Viewport.Width * sceneWindowRect.Left);
            int vpY = (int)(device.Viewport.Height * sceneWindowRect.Top);
            int vpW = (int)(device.Viewport.Width * sceneWindowRect.Width);
            int vpH = (int)(device.Viewport.Height * sceneWindowRect.Height);

            bool inViewport = mouse.X >= vpX && mouse.X < vpX + vpW
                           && mouse.Y >= vpY && mouse.Y < vpY + vpH;

            if (!inViewport)
            {
                tooltip?.Hide();
            }
        }

        public override bool HandleEscape()
        {
            if (state == BasesScreenState.AddFacility)
            {
                CancelFacility();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Render the 3D scene
        /// </summary>
        /// <param name="gameTime">time interval since last render</param>
        /// <param name="device">Device to render the globe to</param>
        public override void Draw(GameTime gameTime, GraphicsDevice device)
        {
            base.Draw(gameTime, device);
            // update funds shown on screen
            String funds = Controller.GetFundsDisplay();
            if (fundsText.Text != funds)
            {
                fundsText.Text = funds;
            }

            scene.Draw(device, sceneWindowRect);

            // Tooltip is rendered automatically by Gum system (no manual Draw needed)
            // Just ensure it's hidden when dialogs are on top
            if (ScreenManager.TopmostFrame != this)
            {
                tooltip?.Hide();
            }
        }

        /// <summary>
        /// Set the screen into a build facility state, where the location of a new
        /// facility can be selected.
        /// </summary>
        /// <param name="handle">Handle to the new facility to build.</param>
        public void BuildFacility(FacilityHandle handle)
        {
            Logger.Info("BuildFacility: {0} (id={1})", handle.FacilityInfo.Name, handle.FacilityInfo.Id);
            NewFacility = handle;
            State = BasesScreen.BasesScreenState.AddFacility;
            // Change build button to cancel button
            buildFacButton.Text = Strings.BUTTON_CANCEL_FACILITY;
        }

        #region Create the Gum controls

        /// <summary>
        /// add the buttons to the screen
        /// </summary>
        protected override void CreateGumControls()
        {
            // sceneWindowRect is initialized at field declaration so it's available
            // during LoadContent() before Show()/CreateGumControls() runs.
            // The field initializer below is kept as a safety net / documentation
            // of the canonical viewport values.
            sceneWindowRect = new UiRect(0.02f, 0.073f, 0.661f, 0.9264f);

            if (GumRoot != null)
            {
                newBaseButton = WireButton("newBaseButton", OnNewBase);
                baseInfoButton = WireButton("baseInfoButton", ShowBaseInfoScreen);
                soldiersButton = WireButton("soldiersButton", OnSoldiersButton);
                equipCraftButton = WireButton("equipCraftButton", OnEquipCraftButton);
                buildFacButton = WireButton("buildFacButton", OnBuildFacilitiesButton);
                produceButton = WireButton("produceButton", OnManufactureButton);
                transferButton = WireButton("transferButton", OnTransferButton);
                buyButton = WireButton("buyButton", OnBuyButton);
                sellButton = WireButton("sellButton", OnSellButton);
                geoscapeButton = WireButton("geoscapeButton", OnGeoscapeButton);

                basesListComboBox = new ComboBox();
                AddChild(basesListComboBox);
                Misc.PopulateHumanBasesList(basesListComboBox, selectedBase);
                basesListComboBox.SelectionChanged += (s, a) => OnBaseSelectionChanged(s, EventArgs.Empty);

                fundsText = new Label();
                AddChild(fundsText);
                return;
            }

            // combo box to allow user to pick base to work on
            basesListComboBox = new ComboBox();
            RootContainer.AddChild(basesListComboBox);
            Misc.PopulateHumanBasesList(basesListComboBox, selectedBase);
            basesListComboBox.SelectionChanged += (s, a) => OnBaseSelectionChanged(s, EventArgs.Empty);

            // add text giving available funds
            fundsText = new Label();
            RootContainer.AddChild(fundsText);

            // other buttons
            newBaseButton = new Button() { Text = XenocideResourceManager.Get("BUTTON_BUILD_NEW_BASE") };
            RootContainer.AddChild(newBaseButton);
            baseInfoButton = new Button() { Text = XenocideResourceManager.Get("BUTTON_BASE_INFORMATION") };
            RootContainer.AddChild(baseInfoButton);
            soldiersButton = new Button() { Text = XenocideResourceManager.Get("BUTTON_SOLDIERS") };
            RootContainer.AddChild(soldiersButton);
            equipCraftButton = new Button() { Text = XenocideResourceManager.Get("BUTTON_EQUIP_CRAFT") };
            RootContainer.AddChild(equipCraftButton);
            buildFacButton = new Button() { Text = XenocideResourceManager.Get("BUTTON_BUILD_FACILITIES") };
            RootContainer.AddChild(buildFacButton);
            produceButton = new Button() { Text = XenocideResourceManager.Get("BUTTON_MANUFACTURE") };
            RootContainer.AddChild(produceButton);
            transferButton = new Button() { Text = XenocideResourceManager.Get("BUTTON_TRANSFER") };
            RootContainer.AddChild(transferButton);
            buyButton = new Button() { Text = XenocideResourceManager.Get("BUTTON_BUY") };
            RootContainer.AddChild(buyButton);
            sellButton = new Button() { Text = XenocideResourceManager.Get("BUTTON_SELL") };
            RootContainer.AddChild(sellButton);
            geoscapeButton = new Button() { Text = XenocideResourceManager.Get("BUTTON_GEOSCAPE") };
            RootContainer.AddChild(geoscapeButton);

            // other buttons being pressed
            newBaseButton.Click += OnNewBase;
            baseInfoButton.Click += ShowBaseInfoScreen;
            soldiersButton.Click += OnSoldiersButton;
            equipCraftButton.Click += OnEquipCraftButton;
            buildFacButton.Click += OnBuildFacilitiesButton;
            produceButton.Click += OnManufactureButton;
            transferButton.Click += OnTransferButton;
            buyButton.Click += OnBuyButton;
            sellButton.Click += OnSellButton;
            geoscapeButton.Click += OnGeoscapeButton;
        }

        private UiRect sceneWindowRect = new UiRect(0.02f, 0.073f, 0.661f, 0.9264f);
        private ComboBox basesListComboBox;
        private Label fundsText;
        private Button newBaseButton;
        private Button baseInfoButton;
        private Button soldiersButton;
        private Button equipCraftButton;
        private Button buildFacButton;
        private Button produceButton;
        private Button transferButton;
        private Button buyButton;
        private Button sellButton;
        private Button geoscapeButton;

        #endregion Create the Gum controls

        #region event handlers

        #region Scene mouse event handlers

        /// <summary>
        /// Called by SceneMouseHandler when the cursor moves over the 3D viewport.
        /// Converts relative viewport coords to floorplan cell coords and updates
        /// the placement ghost (if we are in add-facility mode) so the red/green
        /// preview shadow tracks the cursor in real time.  Also updates the hover
        /// tooltip with facility info under the cursor.
        /// </summary>
        private void OnSceneMouseMoved(float relX, float relY)
        {
            Vector2 cell = RelToCell(relX, relY);
            Logger.Trace("OnSceneMouseMoved: rel=({0:F3},{1:F3}) cell=({2},{3}) state={4}",
                relX, relY, cell.X, cell.Y, state);

            if (cell.X < 0 || cell.Y < 0)
            {
                tooltip?.Hide();
                return; // outside the floorplan
            }

            // Get screen position for tooltip placement
            var mouse = Microsoft.Xna.Framework.Input.Mouse.GetState();
            Vector2 screenPos = new Vector2(mouse.X, mouse.Y);

            switch (state)
            {
                case BasesScreenState.NotAdding:
                    // Show tooltip for facility under cursor, or empty cell hint
                    FacilityHandle facility = SelectedBaseFloorplan.GetFacilityAt((int)cell.X, (int)cell.Y);
                    if (facility != null)
                    {
                        tooltip?.ShowForFacility(facility.FacilityInfo, screenPos);
                    }
                    else
                    {
                        tooltip?.ShowEmpty(screenPos);
                    }
                    break;

                case BasesScreenState.AddAccessLift:
                case BasesScreenState.AddFacility:
                    UpdateNewFacilityPosition(cell);
                    // Show tooltip for the placement ghost
                    if (newFacility != null)
                    {
                        tooltip?.ShowForPlacement(newFacility, screenPos);
                    }
                    break;

                default:
                    Debug.Assert(false);
                    break;
            }
        }

        /// <summary>
        /// Called by SceneMouseHandler when the left mouse button is pressed inside the
        /// 3D viewport.  Places the facility when in add-facility mode; does nothing in
        /// normal mode.  Demolition is triggered exclusively by right-click so there is
        /// no risk of accidentally dismantling a facility while trying to place one.
        /// </summary>
        private void OnSceneLeftClicked(float relX, float relY)
        {
            Vector2 cell = RelToCell(relX, relY);
            Logger.Info("OnSceneLeftClicked: rel=({0:F3},{1:F3}) cell=({2},{3}) state={4}",
                relX, relY, cell.X, cell.Y, state);

            if (cell.X < 0 || cell.Y < 0)
                return;

            Xenocide.AudioSystem.PlaySound(SoundId.ButtonClick2);

            switch (state)
            {
                case BasesScreenState.NotAdding:
                    // Left-click in normal mode does nothing — only right-click demolishes.
                    break;

                case BasesScreenState.AddAccessLift:
                case BasesScreenState.AddFacility:
                    AddFacility(cell);
                    break;

                default:
                    Debug.Assert(false);
                    break;
            }
        }

        /// <summary>
        /// Called by SceneMouseHandler when the right mouse button is pressed inside the
        /// 3D viewport.  Always tries to demolish the facility under the cursor, regardless
        /// of the current placement state, and always shows a confirmation dialog first.
        /// </summary>
        private void OnSceneRightClicked(float relX, float relY)
        {
            Vector2 cell = RelToCell(relX, relY);
            Logger.Info("OnSceneRightClicked: rel=({0:F3},{1:F3}) cell=({2},{3}) state={4}",
                relX, relY, cell.X, cell.Y, state);

            if (cell.X < 0 || cell.Y < 0)
                return;

            Xenocide.AudioSystem.PlaySound(SoundId.ButtonClick2);

            // Right-click always tries to demolish, even during placement mode
            if (state != BasesScreenState.NotAdding)
            {
                CancelFacility();
            }
            RemoveFacility(cell);
        }

        #endregion

        /// <summary>user wants to look at a different base</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void OnBaseSelectionChanged(object sender, EventArgs e)
        {
            if (BasesScreenState.NotAdding == state)
            {
                int index = basesListComboBox.SelectedIndex;
                if (index >= 0)
                {
                    selectedBase = index;
                    ScreenManager.ScheduleScreen(new BasesScreen(selectedBase));
                }
            }
        }

        /// <summary>Replace this screen with matching BaseInfoScreen</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void ShowBaseInfoScreen(object sender, EventArgs e)
        {
            if (BasesScreenState.NotAdding == state)
            {
                ScreenManager.ScheduleScreen(new BaseInfoScreen(selectedBase));
            }
        }

        /// <summary>Replace this screen with soldier list screen.</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void OnSoldiersButton(object sender, EventArgs e)
        {
            if (BasesScreenState.NotAdding == state)
            {
                ScreenManager.ScheduleScreen(new SoldiersListScreen(selectedBase));
            }
        }

        /// <summary>user wants to add a new base</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void OnNewBase(object sender, EventArgs e)
        {
            // can't create a new base if we're adding a facility to this one
            if (BasesScreenState.NotAdding == state)
            {
                if (Controller.CanCreateNewBase())
                {
                    // stop time
                    Xenocide.GameState.GeoData.GeoTime.StopTime();

                    GeoscapeScreen geoscapeScreen = new GeoscapeScreen();
                    geoscapeScreen.State = new GeoscapeScreen.AddingBaseScreenState(geoscapeScreen);
                    ScreenManager.ScheduleScreen(geoscapeScreen);
                }
            }
        }

        /// <summary>user wants to equip the craft assigned to this outpost</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void OnEquipCraftButton(object sender, EventArgs e)
        {
            if (BasesScreenState.NotAdding == state)
            {
                ScreenManager.ScheduleScreen(new EquipCraftScreen(selectedBase));
            }
        }

        /// <summary>user wants to add a facility to the current base</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void OnBuildFacilitiesButton(object sender, EventArgs e)
        {
            Logger.Info("OnBuildFacilitiesButton: state={0} baseEmpty={1}", state, SelectedBaseFloorplan.IsBaseEmpty());

            if (BasesScreenState.NotAdding == state)
            {
                // if base is empty, first step is to add an access lift
                if (SelectedBaseFloorplan.IsBaseEmpty())
                {
                    Logger.Info("Base empty — entering AddAccessLift mode");
                    State = BasesScreenState.AddAccessLift;
                }
                else
                {
                    Logger.Info("Opening BuildFacilityDialog");
                    ScreenManager.ShowDialog(new BuildFacilityDialog(this));
                }
            }
            else if (BasesScreenState.AddFacility == state)
            {
                // have to explictly play this sound, because sound is turned off
                Xenocide.AudioSystem.PlaySound(DefaultButtonClickSound);

                // In this state, it is a cancel button
                CancelFacility();
            }
        }

        /// <summary>User has clicked the "Manufacture" button</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void OnManufactureButton(object sender, EventArgs e)
        {
            ShowManufactureScreen();
        }

        /// <summary>User has clicked the "Transfer" button</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void OnTransferButton(object sender, EventArgs e)
        {
            ShowMakeTransferScreen();
        }

        /// <summary>User has clicked the "Buy" button</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void OnBuyButton(object sender, EventArgs e)
        {
            if (BasesScreenState.NotAdding == state)
            {
                ScreenManager.ScheduleScreen(new PurchaseScreen(selectedBase));
            }
        }

        /// <summary>User has clicked the "Sell" button</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void OnSellButton(object sender, EventArgs e)
        {
            if (BasesScreenState.NotAdding == state)
            {
                ScreenManager.ScheduleScreen(new SellScreen(selectedBase));
            }
        }

        /// <summary>User has clicked the "go to geoscape" button</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void OnGeoscapeButton(object sender, EventArgs e)
        {
            if (BasesScreenState.NotAdding == state)
            {
                ScreenManager.ScheduleScreen(new GeoscapeScreen());
            }
        }

        #endregion event handlers

        #region helper functions for event handlers

        /// <summary>
        /// Convert the scene viewport-relative coords (0..1) to floorplan cell coords.
        /// The SceneMouseHandler fires callbacks with relative coords, which we pass
        /// through the scene's projection ray to find which grid cell the cursor is over.
        /// </summary>
        /// <param name="relX">Relative X in scene viewport (0 = left, 1 = right)</param>
        /// <param name="relY">Relative Y in scene viewport (0 = top, 1 = bottom)</param>
        /// <returns>Cell in base cursor is over, or (-1,-1) if outside the floorplan</returns>
        private Vector2 RelToCell(float relX, float relY)
        {
            return scene.WindowToCell(new UiPoint(relX, relY));
        }

        /// <summary>
        /// Set the position for the new facility we're adding to the base
        /// </summary>
        /// <param name="cellCoords">top left corner of position, in base floorplan "cells"</param>
        private void UpdateNewFacilityPosition(Microsoft.Xna.Framework.Vector2 cellCoords)
        {
            Debug.Assert(null != newFacility);
            Logger.Trace("UpdateNewFacilityPosition: ghost cell ({0},{1}) facility={2}",
                cellCoords.X, cellCoords.Y, newFacility.FacilityInfo.Id);
            newFacility.X = (SByte)cellCoords.X;
            newFacility.Y = (SByte)cellCoords.Y;
        }

        /// <summary>
        /// Add the new facility to the base
        /// </summary>
        /// <param name="cellCoords">top left corner of position, in base floorplan "cells"</param>
        private void AddFacility(Microsoft.Xna.Framework.Vector2 cellCoords)
        {
            UpdateNewFacilityPosition(cellCoords);

            bool isAccessLiftMode = (BasesScreenState.AddAccessLift == state);
            if (controller.TryAddFacility(newFacility, cellCoords, isAccessLiftMode))
            {
                // Redraw scene with changes
                ScreenManager.ScheduleScreen(new BasesScreen(selectedBase));
            }
        }

        /// <summary>
        /// Cancel building a new facility
        /// </summary>
        private void CancelFacility()
        {
            Logger.Debug("CancelFacility: cancelling placement of {0}",
                newFacility?.FacilityInfo?.Id ?? "null");
            State = BasesScreenState.NotAdding;
            NewFacility = null;
            // Set text back to normal
            buildFacButton.Text = Strings.BUTTON_BUILD_FACILITIES;
        }

        /// <summary>
        /// Remove a facility from the base
        /// </summary>
        /// <param name="cellCoords">location in base of facility to remove</param>
        private void RemoveFacility(Microsoft.Xna.Framework.Vector2 cellCoords)
        {
            FacilityHandle facility = controller.GetRemovableFacility(cellCoords);
            if (facility != null)
            {
                Logger.Info("RemoveFacility: {0} at ({1},{2})", facility.FacilityInfo.Id, cellCoords.X, cellCoords.Y);
                GumYesNoDialog dlg = new GumYesNoDialog(
                    Util.StringFormat(Strings.YESNOMSG_DISMANTLE_FACILITY, facility.FacilityInfo.Name)
                );

                // if yes is pressed, delete the facility and redraw scene with changes
                dlg.YesAction += delegate ()
                {
                    Logger.Info("RemoveFacility: CONFIRMED dismantle of {0}", facility.FacilityInfo.Id);
                    controller.DemolishFacility(facility, selectedBase);
                    ScreenManager.ScheduleScreen(new BasesScreen(selectedBase));
                };

                Xenocide.ScreenManager.ShowDialog(dlg);
            }
            else
            {
                Logger.Trace("RemoveFacility: no facility at ({0},{1})", cellCoords.X, cellCoords.Y);
            }
        }

        /// <summary>
        /// Undo the last demolition by restoring the demolished facility.
        /// Called when user presses Ctrl+Z.
        /// </summary>
        private void UndoDemolition()
        {
            if (controller.TryUndoDemolition(selectedBase))
            {
                Logger.Info("UndoDemolition: restoring facility at baseIndex={0}", selectedBase);
                // Redraw scene
                ScreenManager.ScheduleScreen(new BasesScreen(selectedBase));
            }
        }

        /// <summary>Bring up the "Manufacture" Screen</summary>
        private void ShowManufactureScreen()
        {
            if (BasesScreenState.NotAdding == state)
            {
                ScreenManager.ScheduleScreen(new ManufactureScreen(selectedBase));
            }
        }

        /// <summary>Try bringing up the "MakeTransfer" Screen</summary>
        private void ShowMakeTransferScreen()
        {
            if (BasesScreenState.NotAdding == state)
            {
                if (Controller.CanStartTransfer())
                {
                    int numOutposts = Xenocide.GameState.GeoData.Outposts.Count;
                    int destination = (selectedBase + 1) % numOutposts;
                    ScreenManager.ScheduleScreen(new MakeTransferScreen(selectedBase, destination));
                }
            }
        }

        #endregion helper functions for event handlers

        /// <summary>
        /// The state this screen is in
        /// </summary>
        public enum BasesScreenState
        {
            /// <summary>
            /// We're in "not adding anything to base" mode
            /// </summary>
            /// 
            NotAdding,

            /// <summary>
            /// We're in "Add the Access Lift facility to the base" mode
            /// </summary>
            AddAccessLift,

            /// <summary>
            /// We're in "Add a (not access lift) facility to the base" mode
            /// </summary>
            AddFacility,
        }

        #region Fields

        /// <summary>
        /// Controller handling game logic for base management.
        /// </summary>
        private Controller controller;

        /// <summary>
        /// What mode are we in?
        /// </summary>
        public BasesScreenState State
        {
            get { return state; }
            set
            {
                Logger.Debug("State transition: {0} -> {1} (baseIndex={2})", state, value, selectedBase);

                // if adding a facility, buttons are disabled
                EnableButtonSounds = (value == BasesScreenState.NotAdding);

                state = value;
                if (BasesScreenState.AddAccessLift == state)
                {
                    Logger.Info("Auto-creating access lift ghost for empty base");
                    NewFacility = new FacilityHandle("FAC_BASE_ACCESS_FACILITY");
                }

                // Reset the mouse handler's edge-detection so the button-up/-down
                // state from whatever triggered this transition (e.g. clicking
                // "Select" in the BuildFacilityDialog) doesn't leak through as a
                // spurious click on the first frame the scene is active again.
                sceneMouseHandler?.Reset();

                // Hide tooltip on state change
                tooltip?.Hide();
            }
        }

        /// <summary>
        /// The facility we are adding to the base
        /// </summary>
        public FacilityHandle NewFacility
        {
            get { return newFacility; }
            set { newFacility = value; scene.NewFacility = newFacility; }
        }

        /// <summary>
        /// The 3D view shown on the screen
        /// </summary>
        private FacilityScene scene;

        /// <summary>
        /// The floorplan of the currently selected base
        /// </summary>
        public Floorplan SelectedBaseFloorplan
        {
            get { return Xenocide.GameState.GeoData.Outposts[selectedBase].Floorplan; }
        }

        // index specifying the X-Corp outpost that screen is showing
        private int selectedBase;

        /// <summary>
        /// What mode are we in?
        /// </summary>
        private BasesScreenState state;

        /// <summary>
        /// The facility we are adding to the base
        /// </summary>
        private FacilityHandle newFacility;

        /// <summary>
        /// Polls mouse state each frame and fires scene-level events
        /// (move, left-click, right-click) with relative viewport coords.
        /// </summary>
        private SceneMouseHandler sceneMouseHandler;

        /// <summary>
        /// Hover tooltip showing facility info under the cursor.
        /// </summary>
        private FacilityTooltip tooltip;

        /// <summary>
        /// Previous frame's keyboard state for key-down detection.
        /// </summary>
        private Microsoft.Xna.Framework.Input.KeyboardState _prevKeyboardState;

        #endregion Fields

        /// <summary>
        /// Implement IDisposable
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                tooltip?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
