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
* @file StatisticsScreen.cs
* @date Created: 2007/01/21
* @author File creator: dteviot
* @author Credits: Oded Coster
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
using ProjectXenocide.UI.Scenes.Facility;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using ProjectXenocide.UI.Scenes.Statistics;
using Xenocide.Resources;


#endregion

namespace ProjectXenocide.UI.Screens
{
    /// <summary>
    /// Screen that shows historical statistics
    /// </summary>
    class StatisticsScreen : Screen
    {
        /// <summary>
        /// Default constructor (obviously)
        /// </summary>
        public StatisticsScreen()
            : base("StatisticsScreen")
        {
        }

        /// <summary>
        /// Implement Dispose
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    if (scene != null)
                    {
                        scene.Dispose();
                        scene = null;
                    }
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        /// <summary>
        /// Load the Scene's graphic content
        /// </summary>
        /// <param name="content">content manager that fetches the content</param>
        /// <param name="device">the display</param>
        
        public override void LoadContent(ContentManager content, GraphicsDevice device)
        {
            // and put up default display
            CreateSeries();
            selectedGraph = GraphId.Funding;
            scene = new StatisticsScene(MonthlyLog.ThisMonth, DataSet);
            scene.LoadContent(device);
        }

        /// <summary>
        /// Render the 3D scene
        /// </summary>
        /// <param name="gameTime">time interval since last render</param>
        /// <param name="device">Device to render the globe to</param>
        public override void Draw(GameTime gameTime, GraphicsDevice device)
        {
            scene.Draw(device, sceneWindow.Rect);
        }

        #region Create the CeGui widgets

        /// <summary>
        /// add the buttons to the screen
        /// </summary>
        protected override void CreateCeguiWidgets()
        {
            // Window indicating where the 3D scene is
            sceneWindow = GuiBuilder.CreateImage(CeguiId + "_viewport");
            //... dimensions chosen to make 3D scene 512 x 512 at 600 x 800 resolution.
            AddWidget(sceneWindow, 0.08f, 0.073f, 0.681f, 0.8534f);

            // ListBox provides list of series that can be shown on graph
            seriesList = GuiBuilder.CreateListBox("seriesList");
            AddWidget(seriesList, 0.7475f, 0.0600f, 0.2275f, 0.6000f);

            ufoByRegionButton = AddButton("BUTTON_UFO_BY_REGION", 0.7475f, 0.70f, 0.2275f, 0.04125f, "Menu\\buttonclick2_changesetting.ogg");
            ufoByCountryButton = AddButton("BUTTON_UFO_BY_COUNTRY", 0.7475f, 0.75f, 0.2275f, 0.04125f, "Menu\\buttonclick2_changesetting.ogg");
            xcomByRegionButton = AddButton("BUTTON_XCORP_BY_REGION", 0.7475f, 0.80f, 0.2275f, 0.04125f, "Menu\\buttonclick2_changesetting.ogg");
            xcomByCountryButton = AddButton("BUTTON_XCORP_BY_COUNTRY", 0.7475f, 0.85f, 0.2275f, 0.04125f, "Menu\\buttonclick2_changesetting.ogg");
            fundsButton = AddButton("BUTTON_FUNDS", 0.7475f, 0.90f, 0.2275f, 0.04125f, "Menu\\buttonclick2_changesetting.ogg");
            geoscapeButton = AddButton("BUTTON_GEOSCAPE", 0.7475f, 0.95f, 0.2275f, 0.04125f);

            seriesList.SelectionChanged += new WindowEventHandler(OnSeriesSelected);
            ufoByRegionButton.Clicked += new CeGui.GuiEventHandler(OnUfoByRegion);
            ufoByCountryButton.Clicked += new CeGui.GuiEventHandler(OnUfoByCountry);
            xcomByRegionButton.Clicked += new CeGui.GuiEventHandler(OnXCorpByRegion);
            xcomByCountryButton.Clicked += new CeGui.GuiEventHandler(OnXCorpByCountry);
            fundsButton.Clicked += new CeGui.GuiEventHandler(OnFundsGraph);
            geoscapeButton.Clicked += new CeGui.GuiEventHandler(OnGeoscapeButton);

            SetupGraph(GraphId.Funding);
        }

        /// <summary>
        /// CeGui widget that indicates where to draw the 3D scene
        /// </summary>
        private CeGui.Widgets.StaticImage sceneWindow;

        private CeGui.Widgets.Listbox seriesList;
        private CeGui.Widgets.PushButton ufoByRegionButton;
        private CeGui.Widgets.PushButton ufoByCountryButton;
        private CeGui.Widgets.PushButton xcomByRegionButton;
        private CeGui.Widgets.PushButton xcomByCountryButton;
        private CeGui.Widgets.PushButton fundsButton;
        private CeGui.Widgets.PushButton geoscapeButton;

        #endregion Create the CeGui widgets

        #region event handlers

        /// <summary>User has selected a series to display/not display on the graph</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void OnSeriesSelected(object sender, WindowEventArgs e)
        {
            CeGui.Widgets.ListboxItem item = seriesList.GetFirstSelectedItem();
            if (item != null)
            {
                Xenocide.AudioSystem.PlaySound("Menu\\buttonclick2_changesetting.ogg");
                // toggle series between shown and not shown state
                Series series = DataSet[item.ID];
                series.ToggleShow();

                // update list of shown series
                item.Text = series.DecoratedLabel;
                scene.DataSet = DataSet;
            }
        }

        /// <summary>User has clicked the "UFO by Region" button</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void OnUfoByRegion(object sender, CeGui.GuiEventArgs e)
        {
            SetupGraph(GraphId.UfoByRegion);
        }

        /// <summary>User has clicked the "UFO by Country" button</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void OnUfoByCountry(object sender, CeGui.GuiEventArgs e)
        {
            SetupGraph(GraphId.UfoByCountry);
        }

        /// <summary>User has clicked the "X-Corp by Region" button</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void OnXCorpByRegion(object sender, CeGui.GuiEventArgs e)
        {
            SetupGraph(GraphId.XCorpByRegion);
        }

        /// <summary>User has clicked the "X-Corp by Country" button</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void OnXCorpByCountry(object sender, CeGui.GuiEventArgs e)
        {
            SetupGraph(GraphId.XCorpByCountry);
        }

        /// <summary>User has clicked the "Funding" button</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void OnFundsGraph(object sender, CeGui.GuiEventArgs e)
        {
            SetupGraph(GraphId.Funding);
        }

        /// <summary>User has clicked the "go to geoscape" button</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:DisposeObjectsBeforeLosingScope",
           Justification = "FxCop False Positive")]
        private void OnGeoscapeButton(object sender, CeGui.GuiEventArgs e)
        {
            ScreenManager.ScheduleScreen(new GeoscapeScreen());
        }

        #endregion event handlers

        /// <summary>
        /// Setup for user to view a graph
        /// </summary>
        /// <param name="graph">Graph user has picked</param>
        private void SetupGraph(GraphId graph)
        {
            selectedGraph = graph;
            PopulateSeriesList();
            scene.DataSet = this.DataSet;
        }

        /// <summary>
        /// Populate the list of series to select with the available series
        /// </summary>
        private void PopulateSeriesList()
        {
            seriesList.ResetList();
            for (int i = 0; i < DataSet.Count; ++i)
            {
                CeGui.ListboxTextItem item = Util.CreateListboxItem(DataSet[i].DecoratedLabel);
                Colour colour = new Colour(dataColors[i].R / 255.0f, dataColors[i].G / 255.0f, dataColors[i].B / 255.0f, dataColors[i].A / 255.0f);
                item.SetTextColors(colour);
                item.ID = i;
                seriesList.AddItem(item);
            }
        }

        /// <summary>
        /// Create the series used in each graph
        /// </summary>
        private void CreateSeries()
        {
            graphSeries = new List<List<Series>>();
            for (GraphId i = GraphId.Funding; i <= GraphId.XCorpByCountry; ++i)
            {
                graphSeries.Add(new List<Series>());
            }

            // Funding Graph
            List<Series> data = graphSeries[(int)GraphId.Funding];

            //... add Total Scores here 
            ScoreLog totals = Xenocide.GameState.GeoData.XCorp.TotalScores;
            data.Add(new Series(Strings.SCREEN_STATISTICS_SERIES_XCORP_SCORE, totals[Participant.XCorp]));
            data.Add(new Series(Strings.SCREEN_STATISTICS_SERIES_ALIEN_SCORE, totals[Participant.Alien]));

            //... breakdown of X-CORP financials
            Bank bank = Xenocide.GameState.GeoData.XCorp.Bank;
            data.Add(new Series(Strings.SCREEN_STATISTICS_SERIES_SALES, bank.Sales, 0.001));
            data.Add(new Series(Strings.SCREEN_STATISTICS_SERIES_FUNDING, bank.Funds, 0.001));
            data.Add(new Series(Strings.SCREEN_STATISTICS_SERIES_PURCHACES, bank.Purchases, 0.001));
            data.Add(new Series(Strings.SCREEN_STATISTICS_SERIES_MAINTENANCE, bank.Maintenance, 0.001));
            data.Add(new Series(Strings.SCREEN_STATISTICS_SERIES_BALANCE, bank.Balances, 0.001));

            //... include Income by Country
            foreach (Country country in Xenocide.GameState.GeoData.Planet.AllCountries)
            {
                data.Add(new Series(country.Name, country.Funds, 0.001));
            }

            // Ufo/X-Corp activity by Region Graphs
            foreach (PlanetRegion r in Xenocide.GameState.GeoData.Planet.AllRegions)
            {
                graphSeries[(int)GraphId.UfoByRegion].Add(new Series(r.Name, r.ScoreLog[Participant.Alien]));
                graphSeries[(int)GraphId.XCorpByRegion].Add(new Series(r.Name, r.ScoreLog[Participant.XCorp]));
            }

            // Ufo/X-Corp activity by Country Graphs
            foreach (Country c in Xenocide.GameState.GeoData.Planet.AllCountries)
            {
                graphSeries[(int)GraphId.UfoByCountry].Add(new Series(c.Name, c.ScoreLog[Participant.Alien]));
                graphSeries[(int)GraphId.XCorpByCountry].Add(new Series(c.Name, c.ScoreLog[Participant.XCorp]));
            }
        }

        /// <summary>
        /// Identifiers for each of the Graphs
        /// </summary>
        private enum GraphId
        {
            /// <summary>
            /// The "Funding" graph
            /// </summary>
            Funding,

            /// <summary>
            /// Graph that shows UFO activity for each region
            /// </summary>
            UfoByRegion,

            /// <summary>
            /// Graph that shows X-Corp activity for each region
            /// </summary>
            XCorpByRegion,

            /// <summary>
            /// Graph that shows UFO activity for each country
            /// </summary>
            UfoByCountry,

            /// <summary>
            /// Graph that shows X-Corp activity for each country
            /// </summary>
            XCorpByCountry,
        }

        /// <summary>
        /// Which graph is user viewing
        /// </summary>
        private GraphId selectedGraph;

        /// <summary>
        /// The dataSets that make up each of the graphs
        /// </summary>
        private List<List<Series>> graphSeries = new List<List<Series>>();

        /// <summary>
        /// The dataSet of the currently selected graph
        /// </summary>
        private IList<Series> DataSet { get { return graphSeries[(int)selectedGraph]; } }

        /// <summary>
        /// The colors of data items in list and on graph
        /// </summary>
        public static readonly Color[] dataColors = {
            Color.AliceBlue,
            Color.Aqua,
            Color.Beige,
            Color.Blue,
            Color.BlueViolet,
            Color.Brown,
            Color.BurlyWood,
            Color.Chocolate,
            Color.Coral,
            Color.Crimson,
            Color.Cyan,
            Color.DarkGoldenrod,
            Color.DarkGray,
            Color.DarkGreen,
            Color.DarkKhaki,
            Color.DarkMagenta,
            Color.DarkOrange,
            Color.DarkRed,
            Color.DarkSeaGreen,
            Color.DarkTurquoise,
            Color.DeepPink,
            Color.Fuchsia,
            Color.Gold,
            Color.Gray
        };

        /// <summary>
        /// The 3D (2D?) view shown on the screen
        /// </summary>
        private StatisticsScene scene;

    }
}
