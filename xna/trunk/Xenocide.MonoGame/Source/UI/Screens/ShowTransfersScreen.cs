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
    /// This is the screen that shows the shipments currently on their way to an outpost
    /// </summary>
    public class ShowTransfersScreen : GumScreen
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

        #region Create the Gum controls

        /// <summary>
        /// add the buttons to the screen
        /// </summary>
        protected override void CreateGumControls()
        {
            // The grid of items being shiped to this outpost
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
            grid.AddColumn(Strings.SCREEN_SHOW_TRANSFERS_COLUMN_ITEM, (int)(0.69f * 800));
            grid.AddColumn(Strings.SCREEN_SHOW_TRANSFERS_COLUMN_QUANTITY, (int)(0.15f * 800));
            grid.AddColumn(Strings.SCREEN_SHOW_TRANSFERS_COLUMN_ETA, (int)(0.15f * 800));
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
            int rowNum = grid.RowCount;
            grid.AddRow(rowNum, line.Label, line.Quantity.ToString(), hours.ToString());
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

        #endregion Fields
    }
}
