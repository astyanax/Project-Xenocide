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
* @file MontlyCostsScreen.cs
* @date Created: 2007/10/29
* @author File creator: Oded Coster
* @author Credits: none
*/
#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.Text;

using CeGui;


using ProjectXenocide.Utils;
using ProjectXenocide.Model.Geoscape;
using ProjectXenocide.Model.Geoscape.Outposts;
using ProjectXenocide.Model.Geoscape.Vehicles;
using ProjectXenocide.Model.StaticData.Items;
using Xenocide.Resources;


#endregion

namespace ProjectXenocide.UI.Screens
{
    class MonthlyCostsScreen : Screen
    {
        /// <summary>
        /// Constructor (obviously)
        /// </summary>
        /// <param name="selectedOutpostIndex">index specifying the outpost who's monthly costs will be shown</param>
        public MonthlyCostsScreen(int selectedOutpostIndex)
            : base("MonthlyCosts")
        {
            this.selectedOutpostIndex = selectedOutpostIndex;
        }

        #region Create the CeGui widgets

        /// <summary>
        /// add the buttons to the screen
        /// </summary>
        protected override void CreateCeguiWidgets()
        {
            // The grid of montly costs
            InitializeGrid();
            PopulateGrid();

            // buttons
            closeButton = AddButton("BUTTON_CLOSE", 0.7475f, 0.95f, 0.2275f, 0.04125f);
            closeButton.Clicked += new CeGui.GuiEventHandler(OnCloseButton);
        }

        private CeGui.Widgets.MultiColumnList grid;
        private CeGui.Widgets.PushButton closeButton;

        /// <summary>
        /// Create MultiColumnListBox which holds items being shiped
        /// </summary>
        private void InitializeGrid()
        {
            grid = AddGrid(0.01f, 0.01f, 0.73f, 0.98f,
                "", 0.40f,
                Strings.SCREEN_MONTHLY_COSTS_COLUMN_PER_UNIT, 0.22f,
                Strings.SCREEN_MONTHLY_COSTS_COLUMN_QUANTITY, 0.15f,
                Strings.SCREEN_MONTHLY_COSTS_COLUMN_TOTAL, 0.22f
            );
        }

        /// <summary>
        /// Put the list of items being charged into the grid
        /// </summary>
        private void PopulateGrid()
        {
            //Craft
            AddItemRowToGrid("ITEM_XC-1_GRYPHON");
            AddItemRowToGrid("ITEM_XC-11_CONDOR");
            AddItemRowToGrid("ITEM_XC-2_STARFIRE");
            AddItemRowToGrid("ITEM_XC-22_ECLIPSE");
            AddItemRowToGrid("ITEM_XC-33_VENGEANCE");

            //Staff
            AddItemRowToGrid("ITEM_PERSON_SOLDIER");
            AddItemRowToGrid("ITEM_PERSON_ENGINEER");
            AddItemRowToGrid("ITEM_PERSON_SCIENTIST");

            //Facilities (this includes facilities being constructed)
            CeGui.Widgets.ListboxItem listboxItem = Util.CreateListboxItem(Strings.SCREEN_MONTHLY_COSTS_ROW_BASE_MAINTENANCE);
            int rowNum = grid.AddRow(listboxItem, 0);
            Util.AddStringElementToGrid(grid, 3, rowNum, Util.FormatCurrency(SelectedOutpost.CalcFacilityMaintenance()));

            totalCost += SelectedOutpost.CalcFacilityMaintenance();

            //Total cost
            CeGui.Widgets.ListboxItem totalItem = Util.CreateListboxItem(Strings.SCREEN_MONTHLY_COSTS_ROW_TOTAL);
            rowNum = grid.AddRow(totalItem, 2);
            Util.AddStringElementToGrid(grid, 3, rowNum, Util.FormatCurrency(totalCost));
        }

        /// <summary>
        /// Add a row of information to the Grid
        /// </summary>
        /// <param name="itemType">Type of item this row is about</param>
        private void AddItemRowToGrid(string itemType)
        {
            // figure out number of items of this type and number in outpost
            string typeName = Xenocide.StaticTables.ItemList[itemType].Name;

            // Craft and person are mutually exclusive types, so this should be OK
            int count = Util.SequenceLength(SelectedOutpost.ListStaff(itemType));
            count += Util.SequenceLength(SelectedOutpost.ListCrafts(itemType));

            //Count items of this type that are in transit (in a shipment)
            count += SelectedOutpost.CountItemsInTransit(itemType);

            int itemCost = Xenocide.StaticTables.ItemList[itemType].MonthlyCharge;

            // create the row
            if ((count * itemCost) != 0)
            {
                totalCost += (count * itemCost);

                CeGui.Widgets.ListboxItem listboxItem = Util.CreateListboxItem(typeName);
                int rowNum = grid.AddRow(listboxItem, 0);
                Util.AddStringElementToGrid(grid, 1, rowNum, Util.FormatCurrency(itemCost));
                Util.AddNumericElementToGrid(grid, 2, rowNum, count);
                Util.AddStringElementToGrid(grid, 3, rowNum, Util.FormatCurrency(count * itemCost));
            }
        }

        #endregion Create the CeGui widgets

        #region event handlers

        /// <summary>React to user pressing the Close button</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void OnCloseButton(object sender, CeGui.GuiEventArgs e)
        {
            GoToBaseInfoScreen();
        }

        #endregion event handlers

        /// <summary>
        /// Close this screen and go back to the BaseInfo Screen for this outpost
        /// </summary>
        private void GoToBaseInfoScreen()
        {
            ScreenManager.ScheduleScreen(new BaseInfoScreen(selectedOutpostIndex));
        }

        #region Fields

        /// <summary>
        /// The outpost who's shipments will be shown
        /// </summary>
        private Outpost SelectedOutpost { get { return Xenocide.GameState.GeoData.Outposts[selectedOutpostIndex]; } }

        // index specifying the outpost who's shipments will be shown
        private int selectedOutpostIndex;
        private int totalCost;

        #endregion Fields
    }
}
