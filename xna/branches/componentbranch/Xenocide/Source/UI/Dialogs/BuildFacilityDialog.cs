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
* @file BuildFacilityDialog.cs
* @date Created: 2007/04/10
* @author File creator: dteviot
* @author Credits: none
*/
#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;

using CeGui;
using Xenocide.UI.Screens;

using Xenocide.Resources;
using Xenocide.Model.Geoscape.HumanBases;
using Xenocide.Model.StaticData.Facilities;
using Xenocide.Utils;
using Microsoft.Xna.Framework;

#endregion

namespace Xenocide.UI.Dialogs
{
    /// <summary>
    /// Dialog where user selects the type of facility to be built
    /// </summary>
    class BuildFacilityDialog : Dialog
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="humanBase">The base we're adding a facility to</param>
        /// <param name="basesScreen">The bases screen we need to send the user's selection to</param>
        public BuildFacilityDialog(Game game, HumanBase humanBase, BasesScreen basesScreen)
            : base(game, new System.Drawing.SizeF(0.8f, 0.8f))
        {
            this.humanBase   = humanBase;
            this.basesScreen = basesScreen;
        }
        #region Create the CeGui widgets

        /// <summary>
        /// add the buttons to the screen
        /// </summary>
        protected override void CreateCeguiWidgets()
        {
            // The list of available facilities
            InitializeGrid();

            // and the buttons
            okButton     = AddButton("BUTTON_OK",     0.36f, 0.92f, 0.25f, 0.07f);
            cancelButton = AddButton("BUTTON_CANCEL", 0.66f, 0.92f, 0.25f, 0.07f);

            okButton.Clicked     += new GuiEventHandler(OnOkButton);
            cancelButton.Clicked += new GuiEventHandler(OnCancelButton);
        }

        private CeGui.Widgets.MultiColumnList grid;
        private CeGui.Widgets.PushButton      okButton;
        private CeGui.Widgets.PushButton      cancelButton;

        /// <summary>
        /// Creates a MultiColumnListBox which holds list of available facilities
        /// and populates it
        /// </summary>
        private void InitializeGrid()
        {
            grid = GuiBuilder.CreateGrid("grid");
            AddWidget(grid, 0.01f, 0.08f, 0.98f, 0.80f);
            grid.AddColumn(Strings.DLG_BUILDFACILITY_COLUMN_FACILITY,    grid.ColumnCount, 0.49f);
            grid.AddColumn(Strings.DLG_BUILDFACILITY_COLUMN_COST,        grid.ColumnCount, 0.15f);
            grid.AddColumn(Strings.DLG_BUILDFACILITY_COLUMN_BUILD_TIME,  grid.ColumnCount, 0.15f);
            grid.AddColumn(Strings.DLG_BUILDFACILITY_COLUMN_MAINTENANCE, grid.ColumnCount, 0.20f);

            AddFacilitiesToGrid();

            // facilitiesGrid.SelectionChanged += new WindowEventHandler(OnGridSelectionChanged);
        }

        /// <summary>
        /// Add the available facilities to the grid
        /// </summary>
        private void AddFacilitiesToGrid()
        {
            int index = 0;
            foreach(FacilityInfo facility in Xenocide.StaticTables.FacilityList)
            {
                // skip the Access Facility
                if (facility.Id != "FAC_BASE_ACCESS_FACILITY")
                {
                    // add facility to grid
                    CeGui.Widgets.ListboxItem listboxItem = Util.CreateListboxItem(facility.Name);
                    listboxItem.ID = index;
                    int rowNum = grid.AddRow(listboxItem, 0);
                    grid.SetGridItem(1, rowNum, Util.CreateListboxItem(facility.BuildCost.ToString(CultureInfo.InvariantCulture)));
                    grid.SetGridItem(2, rowNum,
                        Util.CreateListboxItem(
                            Util.StringFormat(Strings.DLG_BUILDFACILITY_BUILD_TIME_FORMAT, facility.BuildDays)
                    ));
                    grid.SetGridItem(3, rowNum, Util.CreateListboxItem(facility.MonthlyMaintenance.ToString(CultureInfo.InvariantCulture)));
                }
                ++index;
            }
        }

        #endregion Create the CeGui widgets

        #region event handlers

        /// <summary>user has selected a facility</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void OnOkButton(object sender, CeGui.GuiEventArgs e)
        {
            // Get the facility the user selected
            CeGui.Widgets.ListboxItem item = grid.GetFirstSelectedItem();
            if (null == item)
            {
                ScreenManager.ShowDialog(new MessageBoxDialog(Game, Strings.MSGBOX_NO_FACILITY_SELECTED));
            }
            else
            {
                FacilityInfo info = Xenocide.StaticTables.FacilityList[item.ID];

                // ToDo: Check that user has sufficient funds to build this facility

                if (info.LimitIsOnePerBase && (null != basesScreen.SelectedBaseFloorplan.FindUniqueFacility(info.Id)))
                {
                    // this is a "only one per base" facility, and there's already one in the base
                    ScreenManager.ShowDialog(new MessageBoxDialog(
                        Game,
                        Util.StringFormat(Strings.MSGBOX_ONLY_ONE_FACILITY_PER_BASE, info.Name)
                    ));
                }
                else
                {
                    // Now set up the BasesScreen to be adding this facility
                    FacilityHandle handle = new FacilityHandle(item.ID);
                    basesScreen.NewFacility = handle;
                    basesScreen.State = BasesScreen.BasesScreenState.AddFacility;

                    ScreenManager.CloseDialog(this);
                }
            }
        }

        /// <summary>user wants to cancel adding a facility to base</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void OnCancelButton(object sender, CeGui.GuiEventArgs e)
        {
            ScreenManager.CloseDialog(this);
        }

        #endregion event handlers

        #region Fields

        /// <summary>
        /// The base we're adding a facility to
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields",
            Justification = "ToDo: code is under construction, will be needed later")]
        private HumanBase humanBase;

        /// <summary>
        /// The bases screen we send the user's selection to
        /// </summary>
        private BasesScreen basesScreen;

        #endregion
    }
}
