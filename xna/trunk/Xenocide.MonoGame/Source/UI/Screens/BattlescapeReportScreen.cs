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

using Gum.Forms;
using Gum.Forms.Controls;

using ProjectXenocide.Model;
using ProjectXenocide.Model.Battlescape;
using ProjectXenocide.Model.Geoscape;
using ProjectXenocide.Model.Geoscape.Geography;
using ProjectXenocide.Model.Geoscape.Outposts;
using ProjectXenocide.Model.StaticData;
using ProjectXenocide.Model.StaticData.Items;
using ProjectXenocide.UI.Controls;
using ProjectXenocide.Utils;

using Xenocide.Resources;

using ScoreEntry = ProjectXenocide.Utils.Pair<string, int>;


#endregion

namespace ProjectXenocide.UI.Screens
{
    /// <summary>
    /// This is the screen that shows the results of a Battlescape mission
    /// At moment, as we don't have battlescape, use salvage from UFO
    /// ToDo: results of real battlescape
    /// </summary>
    public class BattlescapeReportScreen : GumScreen
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

        #region Create the Gum controls

        /// <summary>
        /// add the buttons to the screen
        /// </summary>
        protected override void CreateGumControls()
        {
            if (GumRoot != null)
            {
                WireButton("okButton", OnOkButton);

                recoveredLabelText = new Label();
                recoveredLabelText.Visual.X = 20;
                recoveredLabelText.Visual.Y = 20;
                AddChild(recoveredLabelText);
                recoveredLabelText.Text = Strings.SCREEN_BATTLESCAPE_REPORT_RECOVERED_ITEMS_LABEL;

                InitializeScoreGrid();
                scoreGrid.Visual.X = 20;
                scoreGrid.Visual.Y = 50;
                scoreGrid.Visual.Width = 750;

                InitializeRecoveredGrid();
                recoveredGrid.Visual.X = 20;
                recoveredGrid.Visual.Y = 370;
                recoveredGrid.Visual.Width = 750;
                PopulateScoreGrid();
                PopulateRecoveredGrid();
                return;
            }

            // label the recovered items grid
            recoveredLabelText = new Label();
            RootContainer.AddChild(recoveredLabelText);
            recoveredLabelText.Text = Strings.SCREEN_BATTLESCAPE_REPORT_RECOVERED_ITEMS_LABEL;

            // The girds detailing items recovered
            InitializeScoreGrid();
            InitializeRecoveredGrid();
            PopulateScoreGrid();
            PopulateRecoveredGrid();

            // other buttons
            okButton = new Button() { Text = XenocideResourceManager.Get("BUTTON_OK") };
            RootContainer.AddChild(okButton);

            okButton.Click += OnOkButton;
        }

        private Label recoveredLabelText;
        private GridPanel scoreGrid;
        private GridPanel recoveredGrid;
        private Button okButton;

        /// <summary>
        /// Creates a GridPanel which holds score information about
        /// events on the battlescape
        /// </summary>
        private void InitializeScoreGrid()
        {
            scoreGrid = new GridPanel();
            AddChild(scoreGrid.Visual);
            scoreGrid.AddColumn(Strings.SCREEN_BATTLESCAPE_REPORT_COLUMN_ACTION, (int)(0.70f * 800));
            scoreGrid.AddColumn(Strings.SCREEN_BATTLESCAPE_REPORT_COLUMN_SCORE, (int)(0.25f * 800));
        }

        /// <summary>
        /// Creates a GridPanel which holds items recovered from battlescape
        /// </summary>
        private void InitializeRecoveredGrid()
        {
            recoveredGrid = new GridPanel();
            AddChild(recoveredGrid.Visual);
            recoveredGrid.AddColumn(Strings.SCREEN_BATTLESCAPE_REPORT_COLUMN_ITEM, (int)(0.45f * 800));
            recoveredGrid.AddColumn(Strings.SCREEN_BATTLESCAPE_REPORT_COLUMN_QUANTITY, (int)(0.25f * 800));
            recoveredGrid.AddColumn(Strings.SCREEN_BATTLESCAPE_REPORT_COLUMN_SCORE, (int)(0.25f * 800));
        }

        /// <summary>
        /// Populate list of point generating activities
        /// </summary>
        private void PopulateScoreGrid()
        {
            foreach (ScoreEntry entry in mission.Scores)
            {
                int rowNum = scoreGrid.RowCount;
                scoreGrid.AddRow(rowNum, entry.First, entry.Second.ToString());
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
            int rowNum = recoveredGrid.RowCount;
            recoveredGrid.AddRow(rowNum, item, quantity, score.ToString());
        }

        #endregion Create the Gum controls

        #region event handlers

        /// <summary>React to user pressing the OK button</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:DisposeObjectsBeforeLosingScope",
           Justification = "FxCop False Positive")]
        private void OnOkButton(object sender, EventArgs e)
        {
            Shipment shipment = new Shipment(mission.Outpost, Shipment.CalcEta(0));
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
