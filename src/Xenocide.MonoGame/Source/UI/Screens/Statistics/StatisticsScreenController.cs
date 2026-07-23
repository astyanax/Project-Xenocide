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
* @file StatisticsScreenController.cs
* @date Created: 2026/07/19
* @author File creator: Xenocide Team
* @author Credits: Based on StatisticsScreen.cs by dteviot, Oded Coster
*/
#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.Globalization;

using Microsoft.Xna.Framework;

using ProjectXenocide.Model;
using ProjectXenocide.Model.Geoscape;
using ProjectXenocide.Model.Geoscape.Geography;
using ProjectXenocide.Utils;

using Xenocide.Resources;

#endregion

namespace ProjectXenocide.UI.Screens
{
    partial class StatisticsScreen
    {
        /// <summary>
        /// Handles game logic for the statistics screen: series creation,
        /// summary metrics computation, and color palette.
        /// </summary>
        /// <remarks>
        /// ARCHITECTURE: Controller contains all game state queries and computations.
        /// No Gum/MonoGame GUI references — pure business logic that can be unit-tested.
        /// 
        /// DATA FLOW:
        /// - Reads from XCorp.TotalScores, XCorp.Bank, Country.ScoreLog/Funds, PlanetRegion.ScoreLog
        /// - Produces Series objects (wrapped MonthlyLogs) for graphing
        /// - Computes SummaryData for the info panel
        /// </remarks>
        private class Controller
        {
            /// <summary>
            /// Initializes a new instance of the Controller class.
            /// </summary>
            /// <param name="gameState">The current game state</param>
            public Controller(GameState gameState)
            {
                this.gameState = gameState;
            }

            /// <summary>
            /// Creates all graph series for all 5 graph types.
            /// </summary>
            /// <returns>List of series lists, indexed by GraphId</returns>
            public List<List<Series>> CreateAllGraphSeries()
            {
                var graphSeries = new List<List<Series>>();
                for (GraphId i = GraphId.Funding; i <= GraphId.XCorpByCountry; ++i)
                {
                    graphSeries.Add(new List<Series>());
                }

                // Funding Graph
                List<Series> data = graphSeries[(int)GraphId.Funding];

                ScoreLog totals = gameState.GeoData.XCorp.TotalScores;
                data.Add(new Series(Strings.SCREEN_STATISTICS_SERIES_XCORP_SCORE, totals[Participant.XCorp]));
                data.Add(new Series(Strings.SCREEN_STATISTICS_SERIES_ALIEN_SCORE, totals[Participant.Alien]));

                Bank bank = gameState.GeoData.XCorp.Bank;
                data.Add(new Series(Strings.SCREEN_STATISTICS_SERIES_SALES, bank.Sales, 0.001));
                data.Add(new Series(Strings.SCREEN_STATISTICS_SERIES_FUNDING, bank.Funds, 0.001));
                data.Add(new Series(Strings.SCREEN_STATISTICS_SERIES_PURCHACES, bank.Purchases, 0.001));
                data.Add(new Series(Strings.SCREEN_STATISTICS_SERIES_MAINTENANCE, bank.Maintenance, 0.001));
                data.Add(new Series(Strings.SCREEN_STATISTICS_SERIES_BALANCE, bank.Balances, 0.001));

                foreach (Country country in gameState.GeoData.Planet.AllCountries)
                {
                    data.Add(new Series(country.Name, country.Funds, 0.001));
                }

                // UFO/X-Corp activity by Region Graphs
                foreach (PlanetRegion r in gameState.GeoData.Planet.AllRegions)
                {
                    graphSeries[(int)GraphId.UfoByRegion].Add(new Series(r.Name, r.ScoreLog[Participant.Alien]));
                    graphSeries[(int)GraphId.XCorpByRegion].Add(new Series(r.Name, r.ScoreLog[Participant.XCorp]));
                }

                // UFO/X-Corp activity by Country Graphs
                foreach (Country c in gameState.GeoData.Planet.AllCountries)
                {
                    graphSeries[(int)GraphId.UfoByCountry].Add(new Series(c.Name, c.ScoreLog[Participant.Alien]));
                    graphSeries[(int)GraphId.XCorpByCountry].Add(new Series(c.Name, c.ScoreLog[Participant.XCorp]));
                }

                return graphSeries;
            }

            /// <summary>
            /// Computes summary metrics for the currently displayed graph.
            /// </summary>
            /// <param name="graph">Which graph is active</param>
            /// <param name="data">The visible series data</param>
            /// <returns>Summary data for the info panel</returns>
            public SummaryData GetSummaryMetrics(GraphId graph, IList<Series> data)
            {
                int thisMonth = MonthlyLog.ThisMonth;
                int lastMonth = MonthlyLog.LastMonth;

                switch (graph)
                {
                    case GraphId.Funding:
                        return GetFundingSummary(thisMonth, lastMonth);
                    case GraphId.UfoByRegion:
                    case GraphId.UfoByCountry:
                        return GetActivitySummary(graph, data, Participant.Alien, thisMonth, lastMonth);
                    case GraphId.XCorpByRegion:
                    case GraphId.XCorpByCountry:
                        return GetActivitySummary(graph, data, Participant.XCorp, thisMonth, lastMonth);
                    default:
                        return new SummaryData();
                }
            }

            /// <summary>
            /// Finds the maximum value across all visible series.
            /// </summary>
            /// <param name="data">The dataset to scan</param>
            /// <returns>Maximum value found</returns>
            public static int GetMaxVisibleValue(IList<Series> data)
            {
                int maxValue = 0;
                foreach (Series series in data)
                {
                    if (series.Show)
                    {
                        for (int i = 0; i < 12; i++)
                        {
                            if (maxValue < series.ScaledData(i))
                            {
                                maxValue = series.ScaledData(i);
                            }
                        }
                    }
                }
                return maxValue;
            }

            #region Private Helpers

            private SummaryData GetFundingSummary(int thisMonth, int lastMonth)
            {
                Bank bank = gameState.GeoData.XCorp.Bank;
                int balance = bank.CurrentBalance;
                int funding = bank.Funds[thisMonth];
                int maintenance = bank.Maintenance[thisMonth];
                int income = funding - maintenance;
                int prevBalance = bank.Balances[lastMonth];
                int change = balance - prevBalance;
                double changePercent = prevBalance != 0 ? (double)change / Math.Abs(prevBalance) * 100.0 : 0;

                return new SummaryData
                {
                    PrimaryValue = Util.FormatCurrency(balance),
                    PrimaryLabel = Strings.SCREEN_STATISTICS_SERIES_BALANCE,
                    SecondaryValue = (income >= 0 ? "+" : "") + Util.FormatCurrency(income),
                    SecondaryLabel = "This Month",
                    TrendValue = (change >= 0 ? "+" : "") + changePercent.ToString("0.0", CultureInfo.InvariantCulture) + "%",
                    TrendLabel = "vs Last Month",
                    PrimaryColor = balance >= 0 ? DataColors[3] : DataColors[1],
                    SecondaryColor = income >= 0 ? DataColors[3] : DataColors[1],
                    TrendColor = change >= 0 ? DataColors[3] : DataColors[1],
                };
            }

            private static SummaryData GetActivitySummary(GraphId graph, IList<Series> data, Participant participant, int thisMonth, int lastMonth)
            {
                int totalThisMonth = 0;
                int totalLastMonth = 0;
                string topName = "";
                int topValue = 0;

                foreach (Series series in data)
                {
                    if (!series.Show) continue;

                    int thisVal = series.ScaledData(thisMonth);
                    int lastVal = series.ScaledData(lastMonth);
                    totalThisMonth += thisVal;
                    totalLastMonth += lastVal;

                    if (thisVal > topValue)
                    {
                        topValue = thisVal;
                        topName = series.Label;
                    }
                }

                int change = totalThisMonth - totalLastMonth;
                double changePercent = totalLastMonth != 0 ? (double)change / Math.Abs(totalLastMonth) * 100.0 : 0;

                bool isAlien = participant == Participant.Alien;

                return new SummaryData
                {
                    PrimaryValue = Util.ToString(totalThisMonth),
                    PrimaryLabel = isAlien ? "Alien Activity" : "X-Corp Activity",
                    SecondaryValue = topName,
                    SecondaryLabel = "Most Active",
                    TrendValue = (change >= 0 ? "+" : "") + changePercent.ToString("0.0", CultureInfo.InvariantCulture) + "%",
                    TrendLabel = "vs Last Month",
                    PrimaryColor = isAlien ? DataColors[1] : DataColors[0],
                    SecondaryColor = Color.White,
                    TrendColor = change >= 0 ? DataColors[3] : DataColors[1],
                };
            }

            #endregion

            #region Fields

            private readonly GameState gameState;

            #endregion
        }

        /// <summary>
        /// Summary data for the info panel
        /// </summary>
        private class SummaryData
        {
            public string PrimaryValue = "";
            public string PrimaryLabel = "";
            public string SecondaryValue = "";
            public string SecondaryLabel = "";
            public string TrendValue = "";
            public string TrendLabel = "";
            public Color PrimaryColor = Color.White;
            public Color SecondaryColor = Color.White;
            public Color TrendColor = Color.White;
        }

        /// <summary>
        /// The 2D color palette for graph series, optimized for dark backgrounds.
        /// </summary>
        public static readonly Color[] DataColors = {
            new Color(0x4F, 0xC3, 0xF7),   // 0  Electric Blue   - X-Corp score, balance
            new Color(0xEF, 0x53, 0x50),   // 1  Coral Red       - Alien score, expenses
            new Color(0xFF, 0xC1, 0x07),   // 2  Amber           - Funding, sales
            new Color(0x66, 0xBB, 0x6A),   // 3  Emerald         - Income, growth
            new Color(0xB3, 0x9D, 0xDB),   // 4  Lavender        - Maintenance, balance
            new Color(0x26, 0xC6, 0xDA),   // 5  Cyan            - Regions (cool)
            new Color(0xFF, 0x70, 0x43),   // 6  Salmon          - Countries (warm)
            new Color(0x81, 0xD4, 0xFA),   // 7  Light Blue      - Secondary cool
            new Color(0x9C, 0xCC, 0x65),   // 8  Lime            - Secondary positive
            new Color(0xF4, 0x8F, 0xB1),   // 9  Pink            - Secondary warm
            new Color(0x26, 0xA6, 0x9A),   // 10 Teal            - Tertiary cool
            new Color(0xFF, 0xA7, 0x26),   // 11 Orange          - Tertiary warm
        };
    }
}
