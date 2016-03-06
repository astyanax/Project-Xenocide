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

using CeGui;


using ProjectXenocide.Utils;
using ProjectXenocide.Model.Geoscape;
using ProjectXenocide.Model.Geoscape.Outposts;
using ProjectXenocide.Model.StaticData.Items;
using Xenocide.Resources;


#endregion

namespace ProjectXenocide.UI.Screens
{
    /// <summary>
    /// This is the screen that displays items stored in an outpost
    /// </summary>
    public class StoresScreen : Screen
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

        #region Create the CeGui widgets

        /// <summary>
        /// add the buttons to the screen
        /// </summary>
        protected override void CreateCeguiWidgets()
        {
            // The grid of items in inventory
            InitializeGrid();
            PopulateGrid();

            // buttons
            okButton = AddButton("BUTTON_OK", 0.7475f, 0.95f, 0.2275f, 0.04125f);
            okButton.Clicked += new CeGui.GuiEventHandler(OnOKButton);
        }

        private CeGui.Widgets.MultiColumnList grid;
        private CeGui.Widgets.PushButton okButton;

        /// <summary>
        /// Create MultiColumnListBox which holds items in inventory
        /// </summary>
        private void InitializeGrid()
        {
            grid = AddGrid(0.01f, 0.01f, 0.73f, 0.98f,
                Strings.SCREEN_STORES_COLUMN_ITEM, 0.58f,
                Strings.SCREEN_STORES_COLUMN_QUANTITY, 0.18f,
                Strings.SCREEN_STORES_COLUMN_SPACE_USED, 0.19f
            );
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
            // add item to grid
            CeGui.ListboxTextItem listboxItem = Util.CreateListboxItem(item.Name);
            int rowNum = grid.AddRow(listboxItem, 0);
            listboxItem.ID = rowNum;

            int itemCount = SelectedOutpost.Inventory.NumberInInventory(item.ItemInfo);

            Util.AddNumericElementToGrid(grid, 1, rowNum, itemCount);
            Util.AddNumericElementToGrid(grid, 2, rowNum, item.ItemInfo.StorageUnits * itemCount);
        }

        #endregion Create the CeGui widgets

        #region event handlers

        /// <summary>React to user pressing the OK button</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void OnOKButton(object sender, CeGui.GuiEventArgs e)
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
