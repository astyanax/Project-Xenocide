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
* @file Graph.cs
* @date Created: 2008/01/01
* @author File creator: Oded Coster
* @author Credits: none
*/
#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using ProjectXenocide.Utils;
using ProjectXenocide.Model;
using ProjectXenocide.UI.Screens;

#endregion

namespace ProjectXenocide.UI.Scenes.Statistics
{
    /// <summary>
    /// A graph that displays game statistics
    /// </summary>
    internal class Graph : LineMeshBuilder
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="currentMonth">Month we are in</param>
        /// <param name="selectedSerieses">Data to graph</param>
        /// <param name="cellsHeight">Size of grid (in cells) along height of display that graph is rendered on</param>
        public Graph(int currentMonth, IList<Series> selectedSerieses, int cellsHeight)
        {
            thisMonth = currentMonth;
            dataset = selectedSerieses;
            gridCellsHeight = cellsHeight;
        }

        /// <summary>
        /// Fill the lists that define the graph lines
        /// </summary>
        /// <param name="meshVertices">the endpoints of the lines</param>
        /// <param name="meshIndices">order to draw lines</param>
        public override void Build(IList<VertexPositionColor> meshVertices, IList<short> meshIndices)
        {
            float maxCellHeight = gridCellsHeight / 2.0f;

            Color lineColor;
            short index = -1;

            foreach (Series series in dataset)
            {
                //Only add visible serieses
                if (series.Show)
                {
                    lineColor = StatisticsScreen.dataColors[dataset.IndexOf(series)];
                    int month = thisMonth + 1;

                    float zScaler = (float)StatisticsScene.GetMaxVisibleValue(dataset) / 2.0f;

                    for (int i = 0; i < 11; ++i)
                    {
                        meshVertices.Add(new VertexPositionColor(new Vector3((float)i - 5.5f, 0.0f, ((series.ScaledData(month) / zScaler) - 1.0f) * -maxCellHeight), lineColor));
                        meshIndices.Add(++index);

                        month++;
                        month %= 12;

                        meshVertices.Add(new VertexPositionColor(new Vector3((float)i - 4.5f, 0.0f, ((series.ScaledData(month) / zScaler) - 1.0f) * -maxCellHeight), lineColor));
                        meshIndices.Add(++index);
                    }
                }
            }
        }

        #region Fields

        /// <summary>
        ///Size of grid (in cells) along height of display
        /// </summary>
        int gridCellsHeight;

        /// <summary>
        /// Month we are in
        /// </summary>
        private int thisMonth;

        /// <summary>
        /// The data to be rendered as a graph
        /// </summary>
        private IList<Series> dataset;

        #endregion Fields

    }
}
