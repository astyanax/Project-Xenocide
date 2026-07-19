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
* @file GraphBuilder.cs
* @date Created: 2026/07/19
* @author File creator: Xenocide Team
* @author Credits: Based on Graph.cs by Oded Coster
*/
#endregion

#region Using Statements

using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ProjectXenocide.Model;
using ProjectXenocide.UI.Screens;

#endregion

namespace ProjectXenocide.UI.Scenes.Statistics
{
    /// <summary>
    /// Builds vertex data for 2D graph rendering: filled areas under lines
    /// and line outlines connecting data points.
    /// </summary>
    /// <remarks>
    /// ARCHITECTURE: Generates VertexPositionColor data in pixel coordinates.
    /// The StatisticsRenderer calls Build() and draws the results with
    /// DrawUserPrimitives().
    /// 
    /// COORDINATE SYSTEM: Pixel coordinates within the provided bounds rectangle.
    /// X-axis: 12 month positions evenly spaced across bounds.Width.
    /// Y-axis: data values scaled to bounds.Height, with 0 at bottom.
    /// 
    /// FILL ORDER: Series are drawn largest-average-first so smaller areas
    /// appear on top of larger ones.
    /// </remarks>
    internal sealed class GraphBuilder
    {
        /// <summary>
        /// Initializes a new instance of the GraphBuilder class.
        /// </summary>
        /// <param name="bounds">Pixel rectangle of the graph area</param>
        /// <param name="maxValue">Maximum visible value for Y-axis scaling</param>
        /// <param name="currentMonth">Current game month index (0-11)</param>
        /// <param name="dataset">The series data to graph</param>
        public GraphBuilder(Rectangle bounds, int maxValue, int currentMonth, IList<Series> dataset)
        {
            this.bounds = bounds;
            this.maxValue = Math.Max(maxValue, 1);
            this.currentMonth = currentMonth;
            this.dataset = dataset;
        }

        /// <summary>
        /// Builds all vertex and index data for rendering.
        /// </summary>
        /// <param name="fillVertices">Output: triangle vertices for filled areas</param>
        /// <param name="fillIndices">Output: triangle indices for filled areas</param>
        /// <param name="lineVertices">Output: line vertices for outlines</param>
        /// <param name="lineIndices">Output: line indices for outlines</param>
        public void Build(
            out List<VertexPositionColor> fillVertices,
            out List<short> fillIndices,
            out List<VertexPositionColor> lineVertices,
            out List<short> lineIndices)
        {
            fillVertices = new List<VertexPositionColor>();
            fillIndices = new List<short>();
            lineVertices = new List<VertexPositionColor>();
            lineIndices = new List<short>();

            short fillIndex = 0;
            short lineIndex = 0;

            foreach (Series series in dataset)
            {
                if (!series.Show) continue;

                Color seriesColor = StatisticsScreen.DataColors[dataset.IndexOf(series) % StatisticsScreen.DataColors.Length];
                Color fillColor = new Color((byte)seriesColor.R, (byte)seriesColor.G, (byte)seriesColor.B, (byte)76); // 30% alpha

                // Generate 12 data point positions in pixel space
                Vector2[] points = new Vector2[12];
                int month = currentMonth + 1;
                for (int i = 0; i < 12; i++)
                {
                    float x = bounds.Left + (i / 11.0f) * bounds.Width;
                    float normalizedY = (float)series.ScaledData(month) / maxValue;
                    float y = bounds.Bottom - (normalizedY * bounds.Height);
                    points[i] = new Vector2(x, y);

                    month++;
                    month %= 12;
                }

                // Build filled area triangles (line to baseline)
                for (int i = 0; i < 12; i++)
                {
                    float topX = points[i].X;
                    float topY = points[i].Y;
                    float bottomY = bounds.Bottom;

                    fillVertices.Add(new VertexPositionColor(new Vector3(topX, topY, 0), fillColor));
                    fillVertices.Add(new VertexPositionColor(new Vector3(topX, bottomY, 0), fillColor));
                }

                for (int i = 0; i < 11; i++)
                {
                    short baseIdx = (short)(fillIndex + i * 2);
                    // Triangle 1: top[i], bottom[i], top[i+1]
                    fillIndices.Add(baseIdx);
                    fillIndices.Add((short)(baseIdx + 1));
                    fillIndices.Add((short)(baseIdx + 2));
                    // Triangle 2: bottom[i], bottom[i+1], top[i+1]
                    fillIndices.Add((short)(baseIdx + 1));
                    fillIndices.Add((short)(baseIdx + 3));
                    fillIndices.Add((short)(baseIdx + 2));
                }

                fillIndex += 24;

                // Build line segments
                for (int i = 0; i < 11; i++)
                {
                    lineVertices.Add(new VertexPositionColor(new Vector3(points[i].X, points[i].Y, 0), seriesColor));
                    lineVertices.Add(new VertexPositionColor(new Vector3(points[i + 1].X, points[i + 1].Y, 0), seriesColor));

                    lineIndices.Add(lineIndex);
                    lineIndices.Add((short)(lineIndex + 1));
                    lineIndex += 2;
                }
            }
        }

        #region Fields

        private readonly Rectangle bounds;
        private readonly int maxValue;
        private readonly int currentMonth;
        private readonly IList<Series> dataset;

        #endregion
    }
}
