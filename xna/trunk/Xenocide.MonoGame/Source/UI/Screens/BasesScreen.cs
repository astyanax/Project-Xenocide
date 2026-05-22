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

using Gum.Forms.Controls;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using ProjectXenocide.Assets;
using ProjectXenocide.Model.Geoscape.Outposts;
using ProjectXenocide.Model.StaticData.Facilities;
using ProjectXenocide.UI.Dialogs;
using ProjectXenocide.UI.Scenes.Facility;
using ProjectXenocide.Utils;

using Xenocide.Resources;


#endregion

namespace ProjectXenocide.UI.Screens
{
    /// <summary>
    /// Screen that shows the layout of facilities in a X-Corp Outpost (Base)
    /// </summary>
    public class BasesScreen : GumScreen
    {
        /// <summary>
        /// Constructor (obviously)
        /// </summary>
        /// <param name="selectedBase">Index to X-Corp outpost screen is to show</param>
        public BasesScreen(int selectedBase)
            : base("BasesScreen", @"Content/Textures/UI/BasesScreenBackground.png")
        {
            this.selectedBase = selectedBase;

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
        }

        /// <summary>
        /// Perform processing which updates the screen.
        /// </summary>
        /// <param name="gameTime">snapshot of timing values</param>
        public override void Update(GameTime gameTime)
        {
            // If ESC key pressed while adding a facility (other than lift) cancel the add
            if (Keyboard.GetState().IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Escape) && (BasesScreenState.AddFacility == state))
            {
                CancelFacility();
            }
        }

        /// <summary>
        /// Render the 3D scene
        /// </summary>
        /// <param name="gameTime">time interval since last render</param>
        /// <param name="device">Device to render the globe to</param>
        public override void Draw(GameTime gameTime, GraphicsDevice device)
        {
            // update funds shown on screen
            // Note, if display hasn't changed, don't write new value to text window
            String funds = Util.StringFormat(Strings.SCREEN_BASES_FUNDS,
                Xenocide.GameState.GeoData.XCorp.Bank.CurrentBalance);
            if (fundsText.Text != funds)
            {
                fundsText.Text = funds;
            }

            scene.Draw(device, sceneWindowRect);
        }

        /// <summary>
        /// Set the screen into a build facility state, where the location of a new
        /// facility can be selected.
        /// </summary>
        /// <param name="handle">Handle to the new facility to build.</param>
        public void BuildFacility(FacilityHandle handle)
        {
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
            // Window indicating where the 3D scene is
            //... dimensions chosen to make 3D scene 512 x 512 at 600 x 800 resolution.
            sceneWindowRect = new UiRect(0.02f, 0.073f, 0.661f, 0.9264f);

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

        private UiRect sceneWindowRect;
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

        /// <summary>React to user moving the mouse in the 3D scene</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Mouse information</param>
        private void OnMouseMoveInScene(object sender, EventArgs e)
        {
            var mouseState = Microsoft.Xna.Framework.Input.Mouse.GetState();
            switch (state)
            {
                case BasesScreenState.NotAdding:
                    break;

                case BasesScreenState.AddAccessLift:
                case BasesScreenState.AddFacility:
                    UpdateNewFacilityPosition(MouseToCell(mouseState.X, mouseState.Y));
                    break;

                default:
                    Debug.Assert(false);
                    break;
            }
        }

        /// <summary>React to user clicking mouse in the 3D scene</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Mouse information</param>
        private void OnMouseDownInScene(object sender, EventArgs e)
        {
            Xenocide.AudioSystem.PlaySound(SoundId.ButtonClick2);
            var mouseState = Microsoft.Xna.Framework.Input.Mouse.GetState();
            switch (state)
            {
                case BasesScreenState.NotAdding:
                    RemoveFacility(MouseToCell(mouseState.X, mouseState.Y));
                    break;

                case BasesScreenState.AddAccessLift:
                case BasesScreenState.AddFacility:
                    AddFacility(MouseToCell(mouseState.X, mouseState.Y));
                    break;

                default:
                    Debug.Assert(false);
                    break;
            }
        }

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
                // check that we haven't hit maximum number of bases allowed
                if (8 <= Xenocide.GameState.GeoData.Outposts.Count)
                {
                    Util.ShowMessageBox(Strings.MSGBOX_MAX_EIGHT_BASES);
                }
                else
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
            if (BasesScreenState.NotAdding == state)
            {
                // if base is empty, first step is to add an access lift
                if (SelectedBaseFloorplan.IsBaseEmpty())
                {
                    State = BasesScreenState.AddAccessLift;
                }
                else
                {
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
        /// Convert the mouse's position to the cell co-ordinates on the Base's floorplan
        /// </summary>
        /// <param name="mouseX">Mouse X pixel position</param>
        /// <param name="mouseY">Mouse Y pixel position</param>
        /// <returns>Cell in base cursor is over</returns>
        private Microsoft.Xna.Framework.Vector2 MouseToCell(int mouseX, int mouseY)
        {
            var viewport = Xenocide.Instance.GraphicsDevice.Viewport;
            float relX = (mouseX - viewport.Width * sceneWindowRect.Left) / (viewport.Width * sceneWindowRect.Width);
            float relY = (mouseY - viewport.Height * sceneWindowRect.Top) / (viewport.Height * sceneWindowRect.Height);

            return scene.WindowToCell(new UiPoint(relX, relY));
        }

        /// <summary>
        /// Set the position for the new facility we're adding to the base
        /// </summary>
        /// <param name="cellCoords">top left corner of position, in base floorplan "cells"</param>
        private void UpdateNewFacilityPosition(Microsoft.Xna.Framework.Vector2 cellCoords)
        {
            Debug.Assert(null != newFacility);
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
            XenoError error = SelectedBaseFloorplan.IsPositionLegal(newFacility);
            if ((XenoError.None == error) ||
                ((BasesScreenState.AddAccessLift == state) && (XenoError.CellHasNoNeighbours == error)))
            {
                Xenocide.GameState.GeoData.XCorp.Bank.Debit(newFacility.FacilityInfo.BuildCost);
                SelectedBaseFloorplan.AddFacility(newFacility);

                // Redraw scene with changes
                ScreenManager.ScheduleScreen(new BasesScreen(selectedBase));
            }
            else
            {
                Util.ShowMessageBox(Util.GetErrorMessage(error));
            }
        }

        /// <summary>
        /// Cancel building a new facility
        /// </summary>
        private void CancelFacility()
        {
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
            FacilityHandle facility = SelectedBaseFloorplan.GetFacilityAt((int)cellCoords.X, (int)cellCoords.Y);
            if (null != facility)
            {
                XenoError error = SelectedBaseFloorplan.CanRemoveFacility(facility);
                if (XenoError.None == error)
                {
                    GumYesNoDialog dlg = new GumYesNoDialog(
                        Util.StringFormat(Strings.YESNOMSG_DISMANTLE_FACILITY, facility.FacilityInfo.Name)
                    );

                    // if yes is pressed, delete the facility and redraw scene with changes
                    dlg.YesAction += delegate ()
                    {
                        Xenocide.GameState.GeoData.XCorp.Bank.Credit(facility.FacilityInfo.ScrapRevenue);
                        SelectedBaseFloorplan.RemoveFacility(facility);
                        ScreenManager.ScheduleScreen(new BasesScreen(selectedBase));
                    };

                    Xenocide.ScreenManager.ShowDialog(dlg);
                }
                else
                {
                    Util.ShowMessageBox(Util.GetErrorMessage(error));
                }
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
                // if there's only one outpost, we can't transfer
                int numOutposts = Xenocide.GameState.GeoData.Outposts.Count;
                if (numOutposts < 2)
                {
                    Util.ShowMessageBox(Strings.MSGBOX_NEED_2_BASES_TO_TRANSFER);
                }
                else
                {
                    // pick another base for initial destination outpost for items
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
        /// What mode are we in?
        /// </summary>
        public BasesScreenState State
        {
            get { return state; }
            set
            {
                // if adding a facility, buttons are disabled
                EnableButtonSounds = (value == BasesScreenState.NotAdding);

                state = value;
                if (BasesScreenState.AddAccessLift == state)
                {
                    NewFacility = new FacilityHandle("FAC_BASE_ACCESS_FACILITY");
                }
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

        #endregion Fields
    }
}
