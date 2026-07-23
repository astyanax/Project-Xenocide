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
* @file StatisticsRenderer.cs
* @date Created: 2026/07/19
* @author File creator: Xenocide Team
* @author Credits: Based on StatisticsScene.cs by Oded Coster, dteviot
*/
#endregion

#region Using Statements

using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using ProjectXenocide.Model;
using ProjectXenocide.UI;
using ProjectXenocide.UI.Screens;
using ProjectXenocide.Utils;

using Xenocide.Resources;

#endregion

namespace ProjectXenocide.UI.Scenes.Statistics
{
    /// <summary>
    /// Renders a 2D graph using SpriteBatch, replacing the old 3D perspective StatisticsScene.
    /// </summary>
    /// <remarks>
    /// ARCHITECTURE: Standalone renderer that draws a flat 2D graph with filled areas,
    /// grid lines, axis labels, data points, and a current month indicator.
    /// Uses orthographic coordinates — no perspective distortion.
    /// 
    /// RENDERING ORDER:
    /// 1. Grid background gradient
    /// 2. Grid lines (horizontal + vertical)
    /// 3. Filled triangle areas (via GraphBuilder)
    /// 4. Line outlines (via GraphBuilder)
    /// 5. Data point dots
    /// 6. Current month vertical indicator
    /// 7. Axis labels (X: months, Y: values)
    /// 
    /// DATA FLOW: Receives Series list from StatisticsScreen, passes to GraphBuilder
    /// for vertex generation. Invalidates cached mesh when DataSet changes.
    /// </remarks>
    public sealed class StatisticsRenderer : IDisposable
    {
        /// <summary>
        /// Initializes a new instance of the StatisticsRenderer class.
        /// </summary>
        /// <param name="currentMonth">Current game month index (0-11)</param>
        /// <param name="dataset">The series data to render</param>
        public StatisticsRenderer(int currentMonth, IList<Series> dataset)
        {
            thisMonth = currentMonth;
            this.dataset = dataset;
        }

        /// <summary>
        /// Implements IDisposable.
        /// </summary>
        public void Dispose()
        {
            if (disposed) return;
            disposed = true;

            spriteBatch?.Dispose();
            spriteBatch = null;
            pixelTexture?.Dispose();
            pixelTexture = null;
            content?.Dispose();
            content = null;
        }

        /// <summary>
        /// Loads graphics content (fonts, textures).
        /// </summary>
        /// <param name="device">The graphics device</param>
        public void LoadContent(GraphicsDevice device)
        {
            spriteBatch = new SpriteBatch(device);
            content = new ContentManager(Xenocide.Instance.Services, "Content");
            font = content.Load<SpriteFont>(@"SpriteFonts\XenoBig");
            numberFont = content.Load<SpriteFont>(@"SpriteFonts\GeoTimeBig");

            // Create a 1x1 white pixel texture for drawing filled rectangles and dots
            pixelTexture = new Texture2D(device, 1, 1);
            pixelTexture.SetData(new[] { Color.White });
        }

        /// <summary>
        /// Renders the graph into the specified bounds.
        /// </summary>
        /// <param name="device">The graphics device</param>
        /// <param name="sceneWindow">Fractional viewport bounds for the graph area</param>
        public void Draw(GraphicsDevice device, UiRect sceneWindow)
        {
            if (font == null || numberFont == null) return;

            Viewport vp = device.Viewport;
            Rectangle bounds = new Rectangle(
                (int)(vp.Width * sceneWindow.Left),
                (int)(vp.Height * sceneWindow.Top),
                (int)(vp.Width * sceneWindow.Width),
                (int)(vp.Height * sceneWindow.Height));

            // Rebuild mesh if data changed
            if (meshDirty)
            {
                int maxValue = StatisticsScreen.GetMaxVisibleValue(dataset);
                if (maxValue > 0)
                {
                    var builder = new GraphBuilder(bounds, maxValue, thisMonth, dataset);
                    builder.Build(out fillVerts, out lineVerts);
                    currentMaxValue = maxValue;
                }
                else
                {
                    fillVerts = new List<VertexPositionColor>();
                    lineVerts = new List<VertexPositionColor>();
                    currentMaxValue = 0;
                }
                meshDirty = false;
                fillVertsArray = null;
                lineVertsArray = null;
            }

            DrawGridBackground(device, bounds);
            DrawGridLines(device, bounds);
            DrawFilledAreas(device, bounds);
            DrawDataLines(device, bounds);
            DrawDataPoints(device, bounds);
            DrawCurrentMonthIndicator(device, bounds);
            DrawAxisLabels(device, bounds);
        }

        #region Drawing Helpers

        private void DrawGridBackground(GraphicsDevice device, Rectangle bounds)
        {
            // Draw gradient background: darker at bottom, slightly lighter at top
            spriteBatch.Begin();

            int steps = 20;
            int stepHeight = bounds.Height / steps;
            for (int i = 0; i < steps; i++)
            {
                float t = (float)i / steps;
                byte r = (byte)(20 + t * 10);
                byte g = (byte)(25 + t * 10);
                byte b = (byte)(35 + t * 10);
                Color bgColor = new Color(r, g, b);
                spriteBatch.Draw(pixelTexture, new Rectangle(bounds.Left, bounds.Top + i * stepHeight, bounds.Width, stepHeight), bgColor);
            }

            spriteBatch.End();
        }

        private void DrawGridLines(GraphicsDevice device, Rectangle bounds)
        {
            Color gridColor = new Color(60, 65, 75);

            spriteBatch.Begin();

            // Horizontal lines (10 evenly spaced)
            for (int i = 0; i <= 10; i++)
            {
                int y = bounds.Top + (int)((float)i / 10 * bounds.Height);
                spriteBatch.Draw(pixelTexture, new Rectangle(bounds.Left, y, bounds.Width, 1), gridColor);
            }

            // Vertical lines (12 month positions)
            for (int i = 0; i < 12; i++)
            {
                int x = bounds.Left + (int)((float)i / 11 * bounds.Width);
                spriteBatch.Draw(pixelTexture, new Rectangle(x, bounds.Top, 1, bounds.Height), gridColor);
            }

            spriteBatch.End();
        }

        private void DrawFilledAreas(GraphicsDevice device, Rectangle bounds)
        {
            if (fillVerts == null || fillVerts.Count == 0) return;

            if (fillVertsArray == null)
            {
                fillVertsArray = fillVerts.ToArray();
            }

            device.DrawUserPrimitives(PrimitiveType.TriangleList, fillVertsArray, 0, fillVerts.Count / 3, VertexPositionColor.VertexDeclaration);
        }

        private void DrawDataLines(GraphicsDevice device, Rectangle bounds)
        {
            if (lineVerts == null || lineVerts.Count == 0) return;

            if (lineVertsArray == null)
            {
                lineVertsArray = lineVerts.ToArray();
            }

            device.DrawUserPrimitives(PrimitiveType.LineList, lineVertsArray, 0, lineVerts.Count / 2, VertexPositionColor.VertexDeclaration);
        }

        private void DrawDataPoints(GraphicsDevice device, Rectangle bounds)
        {
            if (currentMaxValue <= 0) return;

            spriteBatch.Begin();

            foreach (Series series in dataset)
            {
                if (!series.Show) continue;

                Color seriesColor = StatisticsScreen.DataColors[dataset.IndexOf(series) % StatisticsScreen.DataColors.Length];

                int month = thisMonth + 1;
                for (int i = 0; i < 12; i++)
                {
                    float normalizedX = (float)i / 11;
                    float normalizedY = (float)series.ScaledData(month) / currentMaxValue;

                    int x = bounds.Left + (int)(normalizedX * bounds.Width) - DotRadius;
                    int y = bounds.Bottom - (int)(normalizedY * bounds.Height) - DotRadius;

                    spriteBatch.Draw(pixelTexture, new Rectangle(x, y, DotRadius * 2, DotRadius * 2), seriesColor);

                    month++;
                    month %= 12;
                }
            }

            spriteBatch.End();
        }

        private void DrawCurrentMonthIndicator(GraphicsDevice device, Rectangle bounds)
        {
            spriteBatch.Begin();

            // Find which position (0-11) is the current month in display order
            int monthPos = 11; // current month is at the rightmost position (month index = thisMonth+1 wrapping)
            for (int i = 0; i < 12; i++)
            {
                int m = (thisMonth + 1 + i) % 12;
                if (m == thisMonth)
                {
                    monthPos = i;
                    break;
                }
            }

            int indicatorX = bounds.Left + (int)((float)monthPos / 11 * bounds.Width);
            Color indicatorColor = new Color(255, 255, 255, 128);

            // Draw dashed line
            int dashLength = 6;
            int gapLength = 4;
            for (int y = bounds.Top; y < bounds.Bottom; y += dashLength + gapLength)
            {
                int segmentHeight = Math.Min(dashLength, bounds.Bottom - y);
                spriteBatch.Draw(pixelTexture, new Rectangle(indicatorX - 1, y, 2, segmentHeight), indicatorColor);
            }

            // Draw "NOW" label at top
            spriteBatch.DrawString(font, "NOW", new Vector2(indicatorX - 15, bounds.Top - 18), Color.White);

            spriteBatch.End();
        }

        private void DrawAxisLabels(GraphicsDevice device, Rectangle bounds)
        {
            spriteBatch.Begin();

            Color labelColor = new Color(180, 190, 200);
            Color valueColor = new Color(200, 210, 220);

            // X-axis: month names
            int month = thisMonth + 1;
            for (int i = 0; i < 12; i++)
            {
                string monthName = GetMonthAbbreviation(month);
                Vector2 size = font.MeasureString(monthName);
                int x = bounds.Left + (int)((float)i / 11 * bounds.Width) - (int)(size.X / 2);
                int y = bounds.Bottom + 6;

                spriteBatch.DrawString(font, monthName, new Vector2(x, y), labelColor);

                month++;
                month %= 12;
            }

            // Y-axis: value labels
            if (currentMaxValue > 0)
            {
                for (int i = 0; i <= 10; i++)
                {
                    int value = (int)((float)i / 10 * currentMaxValue);
                    string valueText = Util.ToString(value);
                    Vector2 size = font.MeasureString(valueText);
                    int x = bounds.Left - (int)size.X - 8;
                    int y = bounds.Top + bounds.Height - (int)((float)i / 10 * bounds.Height) - (int)(size.Y / 2);

                    spriteBatch.DrawString(font, valueText, new Vector2(x, y), valueColor);
                }
            }

            spriteBatch.End();
        }

        private static string GetMonthAbbreviation(int monthIndex)
        {
            return MonthAbbreviations[monthIndex % 12];
        }

        private static readonly string[] MonthAbbreviations = { "Jan", "Feb", "Mar", "Apr", "May", "Jun",
                                                                 "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the dataset to render. Invalidates the cached mesh.
        /// </summary>
        public IList<Series> DataSet
        {
            get { return dataset; }
            set { dataset = value; meshDirty = true; }
        }

        #endregion

        #region Fields

        private int thisMonth;
        private IList<Series> dataset;
        private bool meshDirty = true;
        private bool disposed;

        private SpriteBatch spriteBatch;
        private SpriteFont font;
        private SpriteFont numberFont;
        private Texture2D pixelTexture;
        private ContentManager content;

        private List<VertexPositionColor> fillVerts;
        private List<VertexPositionColor> lineVerts;
        private VertexPositionColor[] fillVertsArray;
        private VertexPositionColor[] lineVertsArray;
        private int currentMaxValue;

        private const int DotRadius = 3;

        #endregion
    }
}
