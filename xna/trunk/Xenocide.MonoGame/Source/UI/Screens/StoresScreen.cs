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
* @file StoresScreen.cs
* @date Created: 2007/11/01
* @author File creator: Oded Coster
* @author Credits: none
*/
#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.Text;

using Gum.Forms;
using Gum.Forms.Controls;

using ProjectXenocide.Model.Geoscape;
using ProjectXenocide.Model.Geoscape.Outposts;
using ProjectXenocide.Model.StaticData.Items;
using ProjectXenocide.UI.Controls;
using ProjectXenocide.Utils;

using Xenocide.Resources;


#endregion

namespace ProjectXenocide.UI.Screens
{
    /// <summary>
    /// This is the screen that displays items stored in an outpost
    /// </summary>
    public class StoresScreen : GumScreen
    {
        /// <summary>
        /// Constructor (obviously)
        /// </summary>
        /// <param name="selectedOutpostIndex">Index to outpost items will be taken from</param>
        public StoresScreen(int selectedOutpostIndex)
            : base("StoresScreen")
        {
            this.selectedOutpostIndex = selectedOutpostIndex;
        }

        #region Create the Gum controls

        /// <summary>
        /// add the buttons to the screen
        /// </summary>
        protected override void CreateGumControls()
        {
            if (GumRoot != null)
            {
                WireButton("okButton", OnOKButton);
                InitializeGrid();
                PopulateGrid();
                return;
            }

            InitializeGrid();
            PopulateGrid();

            okButton = new Button() { Text = XenocideResourceManager.Get("BUTTON_OK") };
            RootContainer.AddChild(okButton);
            okButton.Click += OnOKButton;
        }

        private GridPanel grid;
        private Button okButton;

        /// <summary>
        /// Create GridPanel which holds items in inventory
        /// </summary>
        private void InitializeGrid()
        {
            grid = new GridPanel();
            AddChild(grid.Visual);
            grid.AddColumn(Strings.SCREEN_STORES_COLUMN_ITEM, (int)(0.58f * 800));
            grid.AddColumn(Strings.SCREEN_STORES_COLUMN_QUANTITY, (int)(0.18f * 800));
            grid.AddColumn(Strings.SCREEN_STORES_COLUMN_SPACE_USED, (int)(0.19f * 800));
        }

        /// <summary>
        /// Put the list of items available to sell into the grid
        /// </summary>
        private void PopulateGrid()
        {
            foreach (Item item in SelectedOutpost.Inventory.ListContents())
            {
                AddRowToGrid(item);
            }
        }

        /// <summary>
        /// Add a row to the grid
        /// </summary>
        /// <param name="item">details to put on grid</param>
        private void AddRowToGrid(Item item)
        {
            int itemCount = SelectedOutpost.Inventory.NumberInInventory(item.ItemInfo);
            int rowNum = grid.RowCount;
            grid.AddRow(rowNum, item.Name, itemCount.ToString(), (item.ItemInfo.StorageUnits * itemCount).ToString());
        }

        #endregion Create the Gum controls

        #region event handlers

        /// <summary>React to user pressing the OK button</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void OnOKButton(object sender, EventArgs e)
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
        /// The current outpost
        /// </summary>
        private Outpost SelectedOutpost { get { return Xenocide.GameState.GeoData.Outposts[selectedOutpostIndex]; } }

        // index specifying the outpost that items will be taken from
        private int selectedOutpostIndex;

        #endregion Fields
    }
}
