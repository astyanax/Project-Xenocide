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
* @file ShowTransfersScreen.cs
* @date Created: 2007/09/23
* @author File creator: David Teviotdale
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
    /// This is the screen that shows the shipments currently on their way to an outpost
    /// </summary>
    public class ShowTransfersScreen : Screen
    {
        /// <summary>
        /// Constructor (obviously)
        /// </summary>
        /// <param name="selectedOutpostIndex">index specifying the outpost who's shipments will be shown</param>
        public ShowTransfersScreen(int selectedOutpostIndex)
            : base("ShowTransfers")
        {
            this.selectedOutpostIndex = selectedOutpostIndex;
        }

        #region Create the CeGui widgets

        /// <summary>
        /// add the buttons to the screen
        /// </summary>
        protected override void CreateCeguiWidgets()
        {
            // The grid of items being shiped to this outpost
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
                Strings.SCREEN_SHOW_TRANSFERS_COLUMN_ITEM, 0.69f,
                Strings.SCREEN_SHOW_TRANSFERS_COLUMN_QUANTITY, 0.15f,
                Strings.SCREEN_SHOW_TRANSFERS_COLUMN_ETA, 0.15f
            );
        }

        /// <summary>
        /// Put the list of items being shiped into the grid
        /// </summary>
        private void PopulateGrid()
        {
            foreach (Shipment shipment in SelectedOutpost.Inventory.Shipments)
            {
                foreach (Shipment.ManifestLine line in shipment.Manifest)
                {
                    AddRowToGrid(line, (int)shipment.Eta.TotalHours);
                }
            }
        }

        /// <summary>
        /// Add a row to the grid
        /// </summary>
        /// <param name="line">item in shipment</param>
        /// <param name="hours">hours before shipment arrives</param>
        private void AddRowToGrid(Shipment.ManifestLine line, int hours)
        {
            // add item to grid
            int rowNum = grid.AddRow(Util.CreateListboxItem(line.Label), 0);
            Util.AddNumericElementToGrid(grid, 1, rowNum, line.Quantity);
            Util.AddNumericElementToGrid(grid, 2, rowNum, hours);
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

        #endregion Fields
    }
}
