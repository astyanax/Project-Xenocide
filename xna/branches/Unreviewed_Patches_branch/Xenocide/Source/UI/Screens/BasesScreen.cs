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
using System.Text;
using System.Diagnostics;
using System.Drawing;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using CeGui;


using ProjectXenocide.Utils;
using ProjectXenocide.Model.Geoscape.Outposts;
using ProjectXenocide.UI.Dialogs;
using ProjectXenocide.UI.Scenes.Facility;
using ProjectXenocide.Model.StaticData.Facilities;
using Xenocide.Resources;


#endregion

namespace ProjectXenocide.UI.Screens
{
    /// <summary>
    /// Screen that shows the layout of facilities in a X-Corp Outpost (Base)
    /// </summary>
    public class BasesScreen : Screen
    {
        /// <summary>
        /// Constructor (obviously)
        /// </summary>
        /// <param name="selectedBase">Index to X-Corp outpost screen is to show</param>
        public BasesScreen(int selectedBase)
            : base("BasesScreen", @"Content\Textures\UI\BasesScreenBackground.png")
        {
            this.selectedBase = selectedBase;

            // Before showing, bring floorplan up to date 
            scene = new FacilityScene(SelectedBaseFloorplan);
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
            if (Keyboard.GetState().IsKeyDown(Keys.Escape) && (BasesScreenState.AddFacility == state))
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
            // as this will make CeGui recompute all render quads.
            String funds = Util.StringFormat(Strings.SCREEN_BASES_FUNDS,
                Xenocide.GameState.GeoData.XCorp.Bank.CurrentBalance);
            if (fundsText.Text != funds)
            {
                fundsText.Text = funds;
            }

            scene.Draw(device, sceneWindow.Rect);
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

        #region Create the CeGui widgets

        /// <summary>
        /// add the buttons to the screen
        /// </summary>
        protected override void CreateCeguiWidgets()
        {
            // Window indicating where the 3D scene is
            sceneWindow = GuiBuilder.CreateImage(CeguiId + "_viewport");
            //... dimensions chosen to make 3D scene 512 x 512 at 600 x 800 resolution.
            AddWidget(sceneWindow, 0.02f, 0.073f, 0.641f, 0.8534f);

            // combo box to allow user to pick base to work on
            basesListComboBox = GuiBuilder.CreateComboBox("basesListComboBox");
            AddWidget(basesListComboBox, 0.7475f, 0.06f, 0.2275f, 0.40f);
            Misc.PopulateHumanBasesList(basesListComboBox, selectedBase);
            basesListComboBox.ListSelectionAccepted += new WindowEventHandler(OnBaseSelectionChanged);

            // add text giving available funds
            fundsText = GuiBuilder.CreateText(CeguiId + "_fundsText");
            AddWidget(fundsText, 0.7475f, 0.40f, 0.2275f, 0.04125f);

            // other buttons
            newBaseButton = AddButton("BUTTON_BUILD_NEW_BASE", 0.7475f, 0.50f, 0.2275f, 0.04125f);
            baseInfoButton = AddButton("BUTTON_BASE_INFORMATION", 0.7475f, 0.55f, 0.2275f, 0.04125f);
            soldiersButton = AddButton("BUTTON_SOLDIERS", 0.7475f, 0.60f, 0.2275f, 0.04125f);
            equipCraftButton = AddButton("BUTTON_EQUIP_CRAFT", 0.7475f, 0.65f, 0.2275f, 0.04125f);
            buildFacButton = AddButton("BUTTON_BUILD_FACILITIES", 0.7475f, 0.70f, 0.2275f, 0.04125f);
            produceButton = AddButton("BUTTON_MANUFACTURE", 0.7475f, 0.75f, 0.2275f, 0.04125f);
            transferButton = AddButton("BUTTON_TRANSFER", 0.7475f, 0.80f, 0.2275f, 0.04125f);
            buyButton = AddButton("BUTTON_BUY", 0.7475f, 0.85f, 0.2275f, 0.04125f);
            sellButton = AddButton("BUTTON_SELL", 0.7475f, 0.90f, 0.2275f, 0.04125f);
            geoscapeButton = AddButton("BUTTON_GEOSCAPE", 0.7475f, 0.95f, 0.2275f, 0.04125f);

            // mouse activity on the "Scene" window
            sceneWindow.MouseMove += new CeGui.MouseEventHandler(OnMouseMoveInScene);
            sceneWindow.MouseButtonsDown += new CeGui.MouseEventHandler(OnMouseDownInScene);

            // other buttons being pressed
            newBaseButton.Clicked += new CeGui.GuiEventHandler(OnNewBase);
            baseInfoButton.Clicked += new CeGui.GuiEventHandler(ShowBaseInfoScreen);
            soldiersButton.Clicked += new CeGui.GuiEventHandler(OnSoldiersButton);
            equipCraftButton.Clicked += new CeGui.GuiEventHandler(OnEquipCraftButton);
            buildFacButton.Clicked += new CeGui.GuiEventHandler(OnBuildFacilitiesButton);
            produceButton.Clicked += new CeGui.GuiEventHandler(OnManufactureButton);
            transferButton.Clicked += new CeGui.GuiEventHandler(OnTransferButton);
            buyButton.Clicked += new CeGui.GuiEventHandler(OnBuyButton);
            sellButton.Clicked += new CeGui.GuiEventHandler(OnSellButton);
            geoscapeButton.Clicked += new CeGui.GuiEventHandler(OnGeoscapeButton);
        }

        /// <summary>
        /// CeGui widget that indicates where to draw the 3D scene
        /// </summary>
        private CeGui.Widgets.StaticImage sceneWindow;

        private CeGui.Widgets.ComboBox basesListComboBox;
        private CeGui.Widgets.StaticText fundsText;
        private CeGui.Widgets.PushButton newBaseButton;
        private CeGui.Widgets.PushButton baseInfoButton;
        private CeGui.Widgets.PushButton soldiersButton;
        private CeGui.Widgets.PushButton equipCraftButton;
        private CeGui.Widgets.PushButton buildFacButton;
        private CeGui.Widgets.PushButton produceButton;
        private CeGui.Widgets.PushButton transferButton;
        private CeGui.Widgets.PushButton buyButton;
        private CeGui.Widgets.PushButton sellButton;
        private CeGui.Widgets.PushButton geoscapeButton;

        #endregion Create the CeGui widgets

        #region event handlers

        /// <summary>React to user moving the mouse in the 3D scene</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Mouse information</param>
        private void OnMouseMoveInScene(object sender, CeGui.MouseEventArgs e)
        {
            switch (state)
            {
                case BasesScreenState.NotAdding:
                    // nothing to do
                    break;

                case BasesScreenState.AddAccessLift:
                case BasesScreenState.AddFacility:
                    // Update position of facility we're adding
                    UpdateNewFacilityPosition(MouseToCell(e));
                    break;

                default:
                    Debug.Assert(false);
                    break;
            }
        }

        /// <summary>React to user clicking mouse in the 3D scene</summary>
        /// <param name="sender">CeGui widget sending the event</param>
        /// <param name="e">Mouse information</param>
        private void OnMouseDownInScene(object sender, CeGui.MouseEventArgs e)
        {
            Xenocide.AudioSystem.PlaySound("Menu\\buttonclick2_changesetting.ogg");
            switch (state)
            {
                case BasesScreenState.NotAdding:
                    RemoveFacility(MouseToCell(e));
                    break;

                case BasesScreenState.AddAccessLift:
                case BasesScreenState.AddFacility:
                    // Start building facility at this position
                    AddFacility(MouseToCell(e));
                    break;

                default:
                    Debug.Assert(false);
                    break;
            }
        }

        /// <summary>user wants to look at a different base</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void OnBaseSelectionChanged(object sender, WindowEventArgs e)
        {
            if (BasesScreenState.NotAdding == state)
            {
                CeGui.Widgets.ListboxItem item = basesListComboBox.SelectedItem;
                if (item != null)
                {
                    selectedBase = basesListComboBox.GetItemIndex(item);
                    // Need to completely redraw scene.
                    ScreenManager.ScheduleScreen(new BasesScreen(selectedBase));
                }
            }
        }

        /// <summary>Replace this screen with matching BaseInfoScreen</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void ShowBaseInfoScreen(object sender, CeGui.GuiEventArgs e)
        {
            if (BasesScreenState.NotAdding == state)
            {
                ScreenManager.ScheduleScreen(new BaseInfoScreen(selectedBase));
            }
        }

        /// <summary>Replace this screen with soldier list screen.</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void OnSoldiersButton(object sender, CeGui.GuiEventArgs e)
        {
            if (BasesScreenState.NotAdding == state)
            {
                ScreenManager.ScheduleScreen(new SoldiersListScreen(selectedBase));
            }
        }

        /// <summary>user wants to add a new base</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void OnNewBase(object sender, CeGui.GuiEventArgs e)
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
        private void OnEquipCraftButton(object sender, CeGui.GuiEventArgs e)
        {
            if (BasesScreenState.NotAdding == state)
            {
                ScreenManager.ScheduleScreen(new EquipCraftScreen(selectedBase));
            }
        }

        /// <summary>user wants to add a facility to the current base</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void OnBuildFacilitiesButton(object sender, CeGui.GuiEventArgs e)
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
        private void OnManufactureButton(object sender, CeGui.GuiEventArgs e)
        {
            ShowManufactureScreen();
        }

        /// <summary>User has clicked the "Transfer" button</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void OnTransferButton(object sender, CeGui.GuiEventArgs e)
        {
            ShowMakeTransferScreen();
        }

        /// <summary>User has clicked the "Buy" button</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void OnBuyButton(object sender, CeGui.GuiEventArgs e)
        {
            if (BasesScreenState.NotAdding == state)
            {
                ScreenManager.ScheduleScreen(new PurchaseScreen(selectedBase));
            }
        }

        /// <summary>User has clicked the "Sell" button</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void OnSellButton(object sender, CeGui.GuiEventArgs e)
        {
            if (BasesScreenState.NotAdding == state)
            {
                ScreenManager.ScheduleScreen(new SellScreen(selectedBase));
            }
        }

        /// <summary>User has clicked the "go to geoscape" button</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void OnGeoscapeButton(object sender, CeGui.GuiEventArgs e)
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
        /// <param name="e">Mouse information (specifically, location of mouse)</param>
        /// <returns>Cell in base cursor is over</returns>
        private Microsoft.Xna.Framework.Vector2 MouseToCell(CeGui.MouseEventArgs e)
        {
            //convert co-ords from absolute to relative into the scene window
            PointF coords2 = sceneWindow.AbsoluteToRelative(new PointF(
                e.Position.X - sceneWindow.AbsoluteX,
                e.Position.Y - sceneWindow.AbsoluteY));

            // convert to cell position
            return scene.WindowToCell(coords2);
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
                    YesNoDialog dlg = new YesNoDialog(
                        Util.StringFormat(Strings.YESNOMSG_DISMANTLE_FACILITY, facility.FacilityInfo.Name)
                    );

                    // if yes is pressed, delete the facility and redraw scene with changes
                    dlg.YesAction += delegate()
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
