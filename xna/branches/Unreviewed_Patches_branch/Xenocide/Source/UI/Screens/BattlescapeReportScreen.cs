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
* @file BattlescapeReportScreen.cs
* @date Created: 2007/08/13
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
using ProjectXenocide.Model;
using ProjectXenocide.Model.StaticData;
using ProjectXenocide.Model.StaticData.Items;
using ProjectXenocide.Model.Geoscape;
using ProjectXenocide.Model.Geoscape.Geography;
using ProjectXenocide.Model.Geoscape.Outposts;
using ProjectXenocide.Model.Battlescape;

using ScoreEntry = ProjectXenocide.Utils.Pair<string, int>;
using Xenocide.Resources;


#endregion

namespace ProjectXenocide.UI.Screens
{
    /// <summary>
    /// This is the screen that shows the results of a Battlescape mission
    /// At moment, as we don't have battlescape, use salvage from UFO
    /// ToDo: results of real battlescape
    /// </summary>
    public class BattlescapeReportScreen : Screen
    {
        /// <summary>
        /// Constructor (obviously)
        /// </summary>
        /// <param name="mission">Details of the battlescape mission</param>
        public BattlescapeReportScreen(Mission mission)
            : base("BattlescapeReportScreen")
        {
            this.mission = mission;
        }

        #region Create the CeGui widgets

        /// <summary>
        /// add the buttons to the screen
        /// </summary>
        protected override void CreateCeguiWidgets()
        {
            // label the recovered items grid
            recoveredLabelText = AddStaticText(0.01f, 0.42f, 0.2275f, 0.04125f);
            recoveredLabelText.Text = Strings.SCREEN_BATTLESCAPE_REPORT_RECOVERED_ITEMS_LABEL;

            // The gird detailing items recovered
            InitializeScoreGrid();
            InitializeRecoveredGrid();
            PopulateScoreGrid();
            PopulateRecoveredGrid();

            // other buttons
            okButton = AddButton("BUTTON_OK", 0.7475f, 0.95f, 0.2275f, 0.04125f);

            okButton.Clicked += new CeGui.GuiEventHandler(OnOkButton);
        }

        private CeGui.Widgets.StaticText recoveredLabelText;
        private CeGui.Widgets.MultiColumnList scoreGrid;
        private CeGui.Widgets.MultiColumnList recoveredGrid;
        private CeGui.Widgets.PushButton okButton;

        /// <summary>
        /// Creates a MultiColumnListBox which holds score information about
        /// events on the battlescape
        /// </summary>
        private void InitializeScoreGrid()
        {
            scoreGrid = AddGrid(
                0.01f, 0.15f, 0.70f, 0.25f,
                Strings.SCREEN_BATTLESCAPE_REPORT_COLUMN_ACTION, 0.70f,
                Strings.SCREEN_BATTLESCAPE_REPORT_COLUMN_SCORE, 0.25f);
        }

        /// <summary>
        /// Creates a MultiColumnListBox which holds items recovered from battlescape
        /// </summary>
        private void InitializeRecoveredGrid()
        {
            recoveredGrid = AddGrid(
                0.01f, 0.49f, 0.70f, 0.50f,
                Strings.SCREEN_BATTLESCAPE_REPORT_COLUMN_ITEM, 0.45f,
                Strings.SCREEN_BATTLESCAPE_REPORT_COLUMN_QUANTITY, 0.25f,
                Strings.SCREEN_BATTLESCAPE_REPORT_COLUMN_SCORE, 0.25f);
        }

        /// <summary>
        /// Populate list of point generating activities
        /// </summary>
        private void PopulateScoreGrid()
        {
            foreach (ScoreEntry entry in mission.Scores)
            {
                int rowNum = scoreGrid.AddRow(Util.CreateListboxItem(entry.First), 0);
                Util.AddNumericElementToGrid(scoreGrid, 1, rowNum, entry.Second);
                totalScore += entry.Second;
            }
        }

        /// <summary>
        /// Populate list of items recovered from battlescape
        /// </summary>
        private void PopulateRecoveredGrid()
        {
            // for each contry
            foreach (ItemLine line in mission.Salvage)
            {
                ItemInfo item = Xenocide.StaticTables.ItemList[line.ItemId];
                int score = (int)(item.Score * line.Quantity);
                totalScore += score;
                AddRowToRecoveredGrid(
                    item.Name,
                    Util.StringFormat("{0}", line.Quantity),
                    score);
            }

            // totals row
            AddRowToRecoveredGrid(Strings.SCREEN_BATTLESCAPE_REPORT_ROW_TOTAL, "", totalScore);
        }

        /// <summary>
        /// Add a row to the Recovered grid
        /// </summary>
        /// <param name="item">type of item recovered</param>
        /// <param name="quantity">How many of them were recovered</param>
        /// <param name="score">Score for recovering</param>
        private void AddRowToRecoveredGrid(String item, String quantity, int score)
        {
            int rowNum = recoveredGrid.AddRow(Util.CreateListboxItem(item), 0);
            Util.AddStringElementToGrid(recoveredGrid, 1, rowNum, quantity);
            Util.AddNumericElementToGrid(recoveredGrid, 2, rowNum, score);
        }

        #endregion Create the CeGui widgets

        #region event handlers

        /// <summary>React to user pressing the OK button</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:DisposeObjectsBeforeLosingScope",
           Justification = "FxCop False Positive")]
        private void OnOkButton(object sender, CeGui.GuiEventArgs e)
        {
            Shipment shipment = new Shipment(mission.Outpost, Shipment.CalcEta());
            shipment.Add(mission.Salvage);
            shipment.Ship();
            if (0 < totalScore)
            {
                Xenocide.GameState.GeoData.AddScore(Participant.XCorp, totalScore, mission.Location);
            }
            else
            {
                Xenocide.GameState.GeoData.AddScore(Participant.Alien, -totalScore, mission.Location);
            }
            ScreenManager.ScheduleScreen(new GeoscapeScreen());
        }

        #endregion event handlers

        #region Fields

        /// <summary>
        /// Details of the battlescape mission
        /// </summary>
        private Mission mission;

        /// <summary>
        /// Points this mission is worth
        /// </summary>
        private int totalScore;

        #endregion Fields
    }
}
