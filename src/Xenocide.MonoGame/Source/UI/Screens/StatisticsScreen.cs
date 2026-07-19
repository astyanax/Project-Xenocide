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

using Gum.Forms.Controls;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using ProjectXenocide.Assets;
using ProjectXenocide.Model;
using ProjectXenocide.UI.Scenes.Statistics;

using Xenocide.Resources;

#endregion

namespace ProjectXenocide.UI.Screens
{
    /// <summary>
    /// GUI layer for the statistics screen. Delegates all game logic to the Controller
    /// and all rendering to the StatisticsRenderer.
    /// </summary>
    /// <remarks>
    /// ARCHITECTURE: 3-layer screen partitioning (see AGENTS.md).
    /// - GUI: This file — Gum controls, event wiring, screen lifecycle
    /// - Controller: StatisticsScreenController.cs — series creation, summary metrics, colors
    /// - Renderer: StatisticsRenderer.cs — 2D SpriteBatch graph rendering
    /// 
    /// This screen is the only Statistics file that references Gum/MonoGame GUI types.
    /// </remarks>
    sealed partial class StatisticsScreen : GumScreen
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public StatisticsScreen()
            : base("StatisticsScreen")
        {
            controller = new Controller(Xenocide.GameState);
        }

        /// <summary>
        /// Implement Dispose.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    renderer?.Dispose();
                    renderer = null;
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        /// <summary>
        /// Load the renderer's graphic content.
        /// </summary>
        /// <param name="content">content manager that fetches the content</param>
        /// <param name="device">the display</param>
        public override void LoadContent(ContentManager content, GraphicsDevice device)
        {
            graphSeries = controller.CreateAllGraphSeries();
            selectedGraph = GraphId.Funding;

            renderer = new StatisticsRenderer(MonthlyLog.ThisMonth, DataSet);
            renderer.LoadContent(device);
        }

        /// <summary>
        /// Render the 2D graph.
        /// </summary>
        /// <param name="gameTime">time interval since last render</param>
        /// <param name="device">Device to render to</param>
        public override void Draw(GameTime gameTime, GraphicsDevice device)
        {
            base.Draw(gameTime, device);
            renderer.Draw(device, sceneWindowRect);
        }

        #region Create the Gum controls

        /// <summary>
        /// Add the buttons and list box to the screen.
        /// </summary>
        protected override void CreateGumControls()
        {
            sceneWindowRect = new UiRect(0.08f, 0.073f, 0.681f, 0.8534f);

            if (GumRoot != null)
            {
                WireButton("ufoByRegionButton", OnUfoByRegion);
                WireButton("ufoByCountryButton", OnUfoByCountry);
                WireButton("xcomByRegionButton", OnXCorpByRegion);
                WireButton("xcomByCountryButton", OnXCorpByCountry);
                WireButton("fundsButton", OnFundsGraph);
                WireButton("geoscapeButton", OnGeoscapeButton);

                seriesList = new ListBox();
                seriesList.Visual.X = 20;
                seriesList.Visual.Y = 20;
                seriesList.Visual.Width = 300;
                seriesList.Visual.Height = 400;
                AddChild(seriesList);
                seriesList.SelectionChanged += (s, a) => OnSeriesSelected(s, EventArgs.Empty);

                SetupGraph(GraphId.Funding);
                return;
            }

            seriesList = new ListBox();
            RootContainer.AddChild(seriesList);

            ufoByRegionButton = new Button() { Text = XenocideResourceManager.Get("BUTTON_UFO_BY_REGION") };
            RootContainer.AddChild(ufoByRegionButton);
            ufoByCountryButton = new Button() { Text = XenocideResourceManager.Get("BUTTON_UFO_BY_COUNTRY") };
            RootContainer.AddChild(ufoByCountryButton);
            xcomByRegionButton = new Button() { Text = XenocideResourceManager.Get("BUTTON_XCORP_BY_REGION") };
            RootContainer.AddChild(xcomByRegionButton);
            xcomByCountryButton = new Button() { Text = XenocideResourceManager.Get("BUTTON_XCORP_BY_COUNTRY") };
            RootContainer.AddChild(xcomByCountryButton);
            fundsButton = new Button() { Text = XenocideResourceManager.Get("BUTTON_FUNDS") };
            RootContainer.AddChild(fundsButton);
            geoscapeButton = new Button() { Text = XenocideResourceManager.Get("BUTTON_GEOSCAPE") };
            RootContainer.AddChild(geoscapeButton);

            seriesList.SelectionChanged += (s, a) => OnSeriesSelected(s, EventArgs.Empty);
            ufoByRegionButton.Click += OnUfoByRegion;
            ufoByCountryButton.Click += OnUfoByCountry;
            xcomByRegionButton.Click += OnXCorpByRegion;
            xcomByCountryButton.Click += OnXCorpByCountry;
            fundsButton.Click += OnFundsGraph;
            geoscapeButton.Click += OnGeoscapeButton;

            SetupGraph(GraphId.Funding);
        }

        private UiRect sceneWindowRect;
        private ListBox seriesList;
        private Button ufoByRegionButton;
        private Button ufoByCountryButton;
        private Button xcomByRegionButton;
        private Button xcomByCountryButton;
        private Button fundsButton;
        private Button geoscapeButton;

        #endregion Create the Gum controls

        #region Event handlers

        /// <summary>User has selected a series to display/not display on the graph</summary>
        private void OnSeriesSelected(object sender, EventArgs e)
        {
            int index = seriesList.SelectedIndex;
            if (index >= 0)
            {
                Xenocide.AudioSystem.PlaySound(SoundId.ButtonClick2);
                Series series = DataSet[index];
                series.ToggleShow();

                seriesList.Items[index] = series.DecoratedLabel;
                renderer.DataSet = DataSet;
            }
        }

        private void OnUfoByRegion(object sender, EventArgs e)   { SetupGraph(GraphId.UfoByRegion); }
        private void OnUfoByCountry(object sender, EventArgs e)  { SetupGraph(GraphId.UfoByCountry); }
        private void OnXCorpByRegion(object sender, EventArgs e) { SetupGraph(GraphId.XCorpByRegion); }
        private void OnXCorpByCountry(object sender, EventArgs e){ SetupGraph(GraphId.XCorpByCountry); }
        private void OnFundsGraph(object sender, EventArgs e)     { SetupGraph(GraphId.Funding); }

        /// <summary>User has clicked the "go to geoscape" button</summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:DisposeObjectsBeforeLosingScope",
           Justification = "FxCop False Positive")]
        private void OnGeoscapeButton(object sender, EventArgs e)
        {
            ScreenManager.ScheduleScreen(new GeoscapeScreen());
        }

        #endregion Event handlers

        /// <summary>
        /// Switch to a different graph type, refresh the series list and invalidate the renderer.
        /// </summary>
        private void SetupGraph(GraphId graph)
        {
            selectedGraph = graph;
            PopulateSeriesList();
            renderer.DataSet = DataSet;
        }

        /// <summary>
        /// Populate the list of series with the current graph's available series.
        /// </summary>
        private void PopulateSeriesList()
        {
            seriesList.Items.Clear();
            for (int i = 0; i < DataSet.Count; ++i)
            {
                seriesList.Items.Add(DataSet[i].DecoratedLabel);
            }
        }

        #region Fields

        private readonly Controller controller;
        private StatisticsRenderer renderer;
        private GraphId selectedGraph;
        private List<List<Series>> graphSeries;

        private List<Series> DataSet { get { return graphSeries[(int)selectedGraph]; } }

        /// <summary>
        /// Finds the maximum value across all visible series in a dataset.
        /// Used by StatisticsRenderer for Y-axis scaling.
        /// </summary>
        /// <param name="data">The dataset to scan</param>
        /// <returns>The maximum value found</returns>
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

        #endregion Fields

        /// <summary>
        /// Identifiers for each of the Graphs
        /// </summary>
        private enum GraphId
        {
            Funding,
            UfoByRegion,
            XCorpByRegion,
            UfoByCountry,
            XCorpByCountry,
        }
    }
}
