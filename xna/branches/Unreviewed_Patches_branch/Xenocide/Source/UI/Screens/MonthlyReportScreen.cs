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
* @file MonthlyReportScreen.cs
* @date Created: 2007/06/04
* @author File creator: dteviot
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
using ProjectXenocide.Model.Geoscape;
using ProjectXenocide.Model.Geoscape.Geography;
using Xenocide.Resources;


#endregion

namespace ProjectXenocide.UI.Screens
{
    /// <summary>
    /// This is the screen that shows a breakdown of funding by country
    /// (Will probably also be used for "end of month" progress report)
    /// </summary>
    public class MonthlyReportScreen : Screen
    {
        /// <summary>
        /// Constructor (obviously)
        /// </summary>
        /// <param name="isEndOfMonth">for this month, or previous month</param>
        public MonthlyReportScreen(bool isEndOfMonth)
            : base("MonthlyReportScreen")
        {
            this.isEndOfMonth = isEndOfMonth;
        }

        #region Create the CeGui widgets

        /// <summary>
        /// add the buttons to the screen
        /// </summary>
        protected override void CreateCeguiWidgets()
        {
            // add text giving the month
            monthText = GuiBuilder.CreateText(CeguiId + "_monthText");
            AddWidget(monthText, 0.01f, 0.15f, 0.2275f, 0.04125f);
            monthText.Text = Util.StringFormat(Strings.SCREEN_MONTHLYREPORT_MONTH,
                Xenocide.GameState.GeoData.GeoTime.ToString().Substring(0, 7));

            // add text giving the score
            scoreText = GuiBuilder.CreateText(CeguiId + "_scoreText");
            AddWidget(scoreText, 0.35f, 0.15f, 0.2275f, 0.04125f);
            scoreText.Text = MakeScoreString();

            // The gird detailing per country details
            InitializeGrid();
            PopulateGrid();

            // other buttons
            okButton = AddButton("BUTTON_OK", 0.7475f, 0.95f, 0.2275f, 0.04125f);

            okButton.Clicked += new CeGui.GuiEventHandler(OnOkButton);
        }

        private CeGui.Widgets.StaticText monthText;
        private CeGui.Widgets.StaticText scoreText;
        private CeGui.Widgets.MultiColumnList grid;
        private CeGui.Widgets.PushButton okButton;

        /// <summary>
        /// Creates and populates a MultiColumnListBox which holds funding details for each country
        /// </summary>
        private void InitializeGrid()
        {
            grid = GuiBuilder.CreateGrid("countriesGrid");
            AddWidget(grid, 0.01f, 0.22f, 0.70f, 0.75f);
            grid.AddColumn(Strings.SCREEN_MONTHLYREPORT_COLUMN_COUNTRY, grid.ColumnCount, 0.39f);
            grid.AddColumn(Strings.SCREEN_MONTHLYREPORT_COLUMN_ATTITUDE, grid.ColumnCount, 0.20f);
            grid.AddColumn(Strings.SCREEN_MONTHLYREPORT_COLUMN_FUNDS, grid.ColumnCount, 0.20f);
            grid.AddColumn(Strings.SCREEN_MONTHLYREPORT_COLUMN_CHANGE, grid.ColumnCount, 0.20f);
        }

        /// <summary>
        /// Put the statistics into the grid
        /// </summary>
        private void PopulateGrid()
        {
            int totalFunds = 0;
            int totalChange = 0;

            // for each contry
            int thisMonth = MonthlyLog.ThisMonth;
            int lastMonth = MonthlyLog.LastMonth;
            foreach (Country c in Xenocide.GameState.GeoData.Planet.AllCountries)
            {
                totalFunds += c.Funds[thisMonth];
                totalChange += (c.Funds[thisMonth] - c.Funds[lastMonth]);
                AddRowToGrid(
                    c.Name,
                    c.Attitude,
                    c.Funds[thisMonth],
                    c.Funds[thisMonth] - c.Funds[lastMonth]);
            }

            // totals row
            AddRowToGrid(Strings.SCREEN_MONTHLYREPORT_ROW_TOTAL, "", totalFunds, totalChange);
        }

        /// <summary>
        /// Add a row to the grid
        /// </summary>
        /// <param name="country">name of the country</param>
        /// <param name="attitude">country's attitude to X-Corp</param>
        /// <param name="funding">How much $ country gave X-Corp this month</param>
        /// <param name="change">Difference from previous month</param>
        private void AddRowToGrid(String country, String attitude, int funding, int change)
        {
            int rowNum = grid.AddRow(Util.CreateListboxItem(country), 0);
            Util.AddStringElementToGrid(grid, 1, rowNum, attitude);
            Util.AddNumericElementToGrid(grid, 2, rowNum, funding);
            Util.AddNumericElementToGrid(grid, 3, rowNum, change);
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
            ScreenManager.ScheduleScreen(new GeoscapeScreen());
        }

        #endregion event handlers

        /// <summary>
        /// Get Score to show on dialog
        /// </summary>
        /// <returns>String holding player's net score</returns>
        private String MakeScoreString()
        {
            int month = isEndOfMonth ? MonthlyLog.LastMonth : MonthlyLog.ThisMonth;
            int score = Xenocide.GameState.GeoData.XCorp.TotalScores.NetScore(month);
            return Util.StringFormat(Strings.SCREEN_MONTHLYREPORT_SCORE, score);
        }

        #region Fields

        /// <summary>
        /// Is this the "end of the month" report (shown at start of next month)
        /// or the "progress so far this month" report.
        /// </summary>
        private bool isEndOfMonth;

        #endregion Fields
    }
}
