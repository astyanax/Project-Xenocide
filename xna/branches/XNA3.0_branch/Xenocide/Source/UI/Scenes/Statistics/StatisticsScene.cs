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
* @file StatisticsScene.cs - based on FacilityScene.cs
* @date Created: 2007/12/29
* @author File creator: Oded Coster
* @author Credits: dteviot 
*/
#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

using ProjectXenocide.Model.Geoscape.Outposts;
using ProjectXenocide.Utils;
using ProjectXenocide.Model;
using ProjectXenocide.UI.Scenes.Facility;

#endregion

namespace ProjectXenocide.UI.Scenes.Statistics
{
    /// <summary>
    /// Shows the statistics for Xcorps in a graph
    /// </summary>
    public class StatisticsScene : IDisposable
    {

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="currentMonth">Month we are in</param>
        /// <param name="selectedSerieses">The selected series to render</param>
        public StatisticsScene(int currentMonth, IList<Series> selectedSerieses)
        {
            thisMonth = currentMonth;
            dataset = selectedSerieses;
            this.cameraPosition = ComputeCameraPosition();
        }

        /// <summary>
        /// Implement IDisposable
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Implement IDisposable
        /// </summary>
        /// <param name="disposing">false when called from a finalizer</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (basicEffect != null)
                {
                    basicEffect.Dispose();
                    basicEffect = null;
                }
                if (spriteBatch != null)
                {
                    spriteBatch.Dispose();
                    spriteBatch = null;
                }
                if (content != null)
                {
                    content.Dispose();
                    content = null;
                }
                if (grid != null)
                {
                    grid.Dispose();
                    grid = null;
                }
                if (graph != null)
                {
                    graph.Dispose();
                    graph = null;
                }
            }
        }

        /// <summary>
        /// Load the graphic content of the scene
        /// </summary>
        /// <param name="device">the display</param>
        
        public void LoadContent(GraphicsDevice device)
        {
            
            {
                basicEffect = new BasicEffect(device, null);
                grid.LoadContent(device, new Grid(gridCellsWidth, gridCellsHeight));
                spriteBatch = new SpriteBatch(Xenocide.Instance.GraphicsDevice);
                font = content.Load<SpriteFont>(@"Content\SpriteFont1");
            }
        }

        /// <summary>
        /// Render scene
        /// </summary>
        /// <param name="device">Device to use for render</param>
        /// <param name="sceneWindow">Where to draw the scene on the display</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
            Justification = "Will throw exception if device is null")]
        public void Draw(GraphicsDevice device, CeGui.Rect sceneWindow)
        {
            if ((null == graph) && (0 < GetMaxVisibleValue(dataset)))
            {
                graph = new LineMesh();
                graph.LoadContent(device, new Graph(thisMonth, dataset, gridCellsHeight));
            }

            // only draw in area we've been told to
            Viewport oldview = device.Viewport;
            device.Viewport = CalcViewportForSceneWindow(sceneWindow, device.Viewport);
            basicEffect.Projection = GetProjectionMatrix(AspectRatio);

            Matrix viewMatrix = Matrix.CreateLookAt(
                cameraPosition,
                Vector3.Zero,
                Vector3.Forward
                );
            basicEffect.View = viewMatrix;

            // draw the grid
            grid.ConfigureEffect(basicEffect);
            grid.Draw(device, basicEffect);

            // draw the graph (only if there is something to draw)
            if (GetMaxVisibleValue(dataset) > 0)
            {
                graph.Draw(device, basicEffect);
            }

            // restore viewport
            device.Viewport = oldview;

            // write out the axis lables
            spriteBatch.Begin(SpriteBlendMode.AlphaBlend);

            LabelVerticalAxis();
            LabelHorizontalAxis();

            spriteBatch.End();

        }

        /// <summary>
        /// Compute the projection matrix for the scene
        /// </summary>
        /// <param name="aspectRatio">window's aspect ratio</param>
        /// <returns>The calculated projection matrix</returns>
        private static Matrix GetProjectionMatrix(float aspectRatio)
        {
            return Matrix.CreatePerspectiveFieldOfView(
                ViewAngle,
                aspectRatio,
                nearClipPlane, farClipPlane);
        }

        /// <summary>
        /// Deterime where camera is located.
        /// <remarks>Its above center of graph, just high enough to see all the graph</remarks>
        /// </summary>
        /// <returns>Position for the camera</returns>
        private static Vector3 ComputeCameraPosition()
        {
            // assumes aspect ratio is 1.0.
            // problem is, at this point in time, it's not known.
            float oposite = MathHelper.Max(gridCellsHeight, gridCellsWidth) * 0.5f;
            float adjacent = (float)(oposite / Math.Tan(ViewAngle * 0.5));
            return new Vector3(0.0f, adjacent + 1.0f, 0.0f);
        }

        /// <summary>
        /// convert Window's co-ordinates to viewport co-ordinates
        /// </summary>
        /// <param name="windowCoords">Window co-ords to translate</param>
        /// <param name="viewport">The current viewport</param>
        /// <returns>Viewport co-ordinates</returns>
        private Viewport CalcViewportForSceneWindow(CeGui.Rect windowCoords, Viewport viewport)
        {
            int fullHeight = viewport.Height;
            int fullWidth = viewport.Width;
            viewport.X = (int)(fullWidth * windowCoords.Left);
            viewport.Y = (int)(fullHeight * windowCoords.Top);
            viewport.Width = (int)(fullWidth * windowCoords.Width);
            viewport.Height = (int)(fullHeight * windowCoords.Height);

            // compute the aspect ratio while we're about it
            aspectRatio = (float)viewport.Width / (float)viewport.Height;

            return viewport;
        }

        /// <summary>
        /// Write out month names on the "x" axis
        /// </summary>
        private void LabelHorizontalAxis()
        {
            int month = thisMonth + 1;
            int startX = 85;

            for (int i = 0; i < 12; ++i)
            {
                string output = Util.LoadString(monthNames[month]);
                Vector2 pos = font.MeasureString(output);
                pos.Y = 500;
                pos.X = startX + (43 * i);

                spriteBatch.DrawString(font, output, pos, Color.Purple);
                month++;
                month %= 12;
            }
        }

        /// <summary>
        /// Write out the current values on the "y" axis
        /// </summary>
        private void LabelVerticalAxis()
        {
            // draw scale of graph near y axis, using calculated max
            int cellStep = GetMaxVisibleValue(dataset) / 10;

            for (int i = 0; i < 10; ++i)
            {
                string output = Util.ToString(i * cellStep);
                Vector2 pos = font.MeasureString(output);
                pos.X = 90 - pos.X;
                pos.Y = 486 - (i * 43);

                spriteBatch.DrawString(font, output, pos, Color.Purple);
            }
        }

        /// <summary>
        /// Finds the maximum value within all visible serieses
        /// </summary>
        /// <returns>The maximum value</returns>
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

        #region Fields

        /// <summary>
        /// The position of the camera, in polar co-ordinates.
        /// <remarks>at current time, camera is fixed</remarks>
        /// </summary>
        private Vector3 cameraPosition;

        /// <summary>
        /// The viewport's aspect ratio
        /// </summary>
        protected float AspectRatio { get { return aspectRatio; } }

        /// <summary>
        /// The viewport's aspect ratio
        /// </summary>
        private float aspectRatio;

        /// <summary>
        /// The basic effect used for rendering
        /// </summary>
        private BasicEffect basicEffect;

        /// <summary>
        /// Graph that shows the current series
        /// </summary>
        private LineMesh graph = new LineMesh();

        /// <summary>
        /// Grid on which graph is superimposed
        /// </summary>
        private LineMesh grid = new LineMesh();

        /// <summary>
        /// Used to draw the sprites
        /// </summary>
        private SpriteBatch spriteBatch;

        /// <summary>
        /// Font to draw all text in the graph
        /// </summary>
        private SpriteFont font;

        /// <summary>
        /// Needed so that we can load the sprite font
        /// </summary>
        private ContentManager content = new ContentManager(Xenocide.Instance.Services);

        /// <summary>
        /// The names of the months (used as graph identifiers)
        /// </summary>
        private static readonly String[] monthNames =
        {
            "SCREEN_STATISTICS_COLUMN_JANUARY",
            "SCREEN_STATISTICS_COLUMN_FEBRUARY",
            "SCREEN_STATISTICS_COLUMN_MARCH",
            "SCREEN_STATISTICS_COLUMN_APRIL",
            "SCREEN_STATISTICS_COLUMN_MAY",
            "SCREEN_STATISTICS_COLUMN_JUNE",
            "SCREEN_STATISTICS_COLUMN_JULY",
            "SCREEN_STATISTICS_COLUMN_AUGUST",
            "SCREEN_STATISTICS_COLUMN_SEPTEMBER",
            "SCREEN_STATISTICS_COLUMN_OCTOBER",
            "SCREEN_STATISTICS_COLUMN_NOVEMBER",
            "SCREEN_STATISTICS_COLUMN_DECEMBER"
        };

        /// <summary>
        /// The current month
        /// </summary>
        private int thisMonth;

        /// <summary>
        /// The dataset used for rendering the graph
        /// </summary>
        public IList<Series> DataSet { get { return dataset; } set { dataset = value; graph = null; } }

        /// <summary>
        /// The set of data to graph
        /// </summary>
        private IList<Series> dataset;

        /// <summary>
        ///Size of grid (in cells) along width of display
        /// </summary>
        private const int gridCellsWidth = 11;

        /// <summary>
        /// Size of grid (in cells) along height of display
        /// </summary>
        private const int gridCellsHeight = 9;

        #endregion

        #region Constant definitions

        /// <summary>
        /// Used in constructing viewing fustrum
        /// </summary>
        private const float nearClipPlane = 0.1f;

        /// <summary>
        /// Used in constructing viewing fustrum
        /// </summary>
        private const float farClipPlane = 20.0f;

        /// <summary>
        /// Used in constructing viewing fustrum
        /// </summary>
        private const float ViewAngle = (float)Math.PI / 4.0f;    // 45 degres

        #endregion
    }
}
