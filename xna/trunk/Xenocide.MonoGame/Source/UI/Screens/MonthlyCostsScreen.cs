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

using Gum.Forms.Controls;

using ProjectXenocide.Model.Geoscape;
using ProjectXenocide.Model.Geoscape.Outposts;
using ProjectXenocide.Model.Geoscape.Vehicles;
using ProjectXenocide.Model.StaticData.Items;
using ProjectXenocide.UI.Controls;
using ProjectXenocide.Utils;

using Xenocide.Resources;


#endregion

namespace ProjectXenocide.UI.Screens
{
    class MonthlyCostsScreen : GumScreen
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

        #region Create the Gum controls

        /// <summary>
        /// add the buttons to the screen
        /// </summary>
        protected override void CreateGumControls()
        {
            // The grid of montly costs
            InitializeGrid();
            PopulateGrid();

            // buttons
            closeButton = new Button() { Text = XenocideResourceManager.Get("BUTTON_CLOSE") };
            RootContainer.AddChild(closeButton);
            closeButton.Click += OnCloseButton;
        }

        private GridPanel grid;
        private Button closeButton;

        /// <summary>
        /// Create GridPanel which holds items being shiped
        /// </summary>
        private void InitializeGrid()
        {
            grid = new GridPanel();
            RootContainer.AddChild(grid.Visual);
            grid.AddColumn("", (int)(0.40f * 800));
            grid.AddColumn(Strings.SCREEN_MONTHLY_COSTS_COLUMN_PER_UNIT, (int)(0.22f * 800));
            grid.AddColumn(Strings.SCREEN_MONTHLY_COSTS_COLUMN_QUANTITY, (int)(0.15f * 800));
            grid.AddColumn(Strings.SCREEN_MONTHLY_COSTS_COLUMN_TOTAL, (int)(0.22f * 800));
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
            int rowNum = grid.RowCount;
            grid.AddRow(rowNum, Strings.SCREEN_MONTHLY_COSTS_ROW_BASE_MAINTENANCE, "", "",
                Util.FormatCurrency(SelectedOutpost.CalcFacilityMaintenance()));

            totalCost += SelectedOutpost.CalcFacilityMaintenance();

            //Total cost
            rowNum = grid.RowCount;
            grid.AddRow(rowNum, "", "", Strings.SCREEN_MONTHLY_COSTS_ROW_TOTAL,
                Util.FormatCurrency(totalCost));
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

                int rowNum = grid.RowCount;
                grid.AddRow(rowNum, typeName, Util.FormatCurrency(itemCost), count.ToString(),
                    Util.FormatCurrency(count * itemCost));
            }
        }

        #endregion Create the Gum controls

        #region event handlers

        /// <summary>React to user pressing the Close button</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void OnCloseButton(object sender, EventArgs e)
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
