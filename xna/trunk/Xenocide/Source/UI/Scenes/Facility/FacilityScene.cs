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
* @file FacilityScene.cs
* @date Created: 2007/04/23
* @author File creator: dteviot
* @author Credits: none
*/
#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Drawing;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

using ProjectXenocide.Model.StaticData.Facilities;
using ProjectXenocide.Model.Geoscape.Outposts;

#endregion

namespace ProjectXenocide.UI.Scenes.Facility
{
    /// <summary>
    /// Shows the facilities (and their layout) in a X-Corp outpost.
    /// </summary>
    public class FacilityScene : IDisposable
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="floorplan">Layout of base's facilities to show in scene</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
           Justification = "Will throw exception if floorplan is null")]
        public FacilityScene(Floorplan floorplan)
        {
            this.floorplan      = floorplan;
            this.cameraPosition = ComputeCameraPosition();
            this.buildTimes     = new BuildTimes(this.floorplan);
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
                if (grid != null)
                {
                    grid.Dispose();
                    grid = null;
                }
            }
        }

        /// <summary>
        /// Load the graphic content of the scene
        /// </summary>
        /// <param name="content">content manager that fetches the content</param>
        /// <param name="device">the display</param>
        
        public void LoadContent(ContentManager content, GraphicsDevice device)
        {
            
            {
                InitializeEffect(device);
                grid.LoadContent(device, new Grid(floorplan.CellsWide, floorplan.CellsHigh));
                models.LoadContent(content);
                buildTimes.LoadContent(content, device);
            }
        }

        /// <summary>
        /// Render scene
        /// </summary>
        /// <param name="device">Device to use for render</param>
        /// <param name="sceneWindow">Where to draw the scene on the display</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
            Justification="Will throw exception if device is null")]
        public void Draw(GraphicsDevice device, CeGui.Rect sceneWindow)
        {
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

            device.RenderState.CullMode = CullMode.CullCounterClockwiseFace;
            device.RenderState.DepthBufferEnable = true;
            device.RenderState.DepthBufferWriteEnable = true;
            
            // draw the grid
            grid.ConfigureEffect(basicEffect);
            grid.Draw(device, basicEffect);

            // Draw the facilities.
            foreach (FacilityHandle handle in floorplan.Facilities)
            {
                Draw(handle);
            }

            // Draw the "new facility position" marker (if we're in add facility mode)
            if (null != newFacility)
            {
                Draw(newFacility);
            }

            // Draw the "build time remaining" labels
            // Must draw them last, because the alpha blending of the labels overwrites the z-buffer
            buildTimes.ConfigureEffect(basicEffect);
            buildTimes.Draw(device, basicEffect);

            // restore viewport
            device.Viewport = oldview;
        }

        /// <summary>
        /// Convert a position in the viewport to a cell in the base's layout
        /// </summary>
        /// <param name="coords">The position in the viewport (in relative co-ords)</param>
        /// <returns>The cell in the base's layout or -1, -1 if point isn't inside base</returns>
        public Vector2 WindowToCell(PointF coords)
        {
            // basic trig, compute viewing fustrum dimension at distance base floorplan is
            double opositeSideLength = Math.Tan(ViewAngle / 2.0) * cameraPosition.Y * 2.0f;

            double x = opositeSideLength * AspectRatio * (coords.X - 0.5);
            double z = opositeSideLength * (coords.Y - 0.5);

            // Allow for center of screen being center of base
            x += (floorplan.CellsWide / 2.0f);
            z += (floorplan.CellsHigh / 2.0f);

            // check that result is within the base
            if ( (x < 0.0f) || (floorplan.CellsWide < x) || (z < 0.0f) || (floorplan.CellsHigh < z))
            {
                x = -1.0f;
                z = -1.0f;
            }
            return new Vector2((float)Math.Floor(x), (float)Math.Floor(z));
        }

        /// <summary>
        /// Render facility into scene
        /// </summary>
        /// <param name="handle">The facility (and it's position in the base)</param>
        private void Draw(FacilityHandle handle)
        {
            // Only draw the facility if it has a position inside the base
            if (handle.HasPosition)
            {
                FacilityInfo info = handle.FacilityInfo;

                // calcuate position to draw model at
                float xdisp = handle.X + ((info.XSize - floorplan.CellsWide) / 2.0f);
                float zdisp = handle.Y + ((info.YSize - floorplan.CellsHigh) / 2.0f);
                Matrix displacement = Matrix.CreateTranslation(xdisp, 0.0f, zdisp);
                models.Draw(basicEffect, info.Id, displacement);
            }
        }

        /// <summary>
        /// Set up the basic effect for rendering
        /// </summary>
        /// <param name="device"></param>
        private void InitializeEffect(GraphicsDevice device)
        {
            basicEffect = new BasicEffect(device, null);
            basicEffect.Alpha = 1.0f;
            basicEffect.DiffuseColor = new Vector3(1.0f, 1.0f, 1.0f);
            basicEffect.SpecularColor = new Vector3(0.25f, 0.25f, 0.25f);
            basicEffect.SpecularPower = 5.0f;
            basicEffect.AmbientLightColor = new Vector3(0.40f, 0.40f, 0.40f);

            basicEffect.DirectionalLight0.Enabled = false;
            basicEffect.DirectionalLight0.DiffuseColor = Vector3.One;
            basicEffect.DirectionalLight0.SpecularColor = Vector3.One;

            basicEffect.DirectionalLight1.Enabled = false;

            basicEffect.LightingEnabled = false;
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
        /// <remarks>Its above center of base, just high enough to see all facilities</remarks>
        /// </summary>
        /// <returns>Position for the camera</returns>
        private Vector3 ComputeCameraPosition()
        {
            // assumes aspect ratio is 1.0.
            // problem is, at this point in time, it's not known.
            float oposite = MathHelper.Max(floorplan.CellsHigh, floorplan.CellsWide) * 0.5f;
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

        #region Fields

        /// <summary>
        /// The facility we are adding to the base
        /// </summary>
        public FacilityHandle NewFacility
        {
            get { return newFacility; }
            set { newFacility = value; }
        }

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
        /// Grid that shows the cells holding facilities
        /// </summary>
        private LineMesh grid = new LineMesh();

        /// <summary>
        /// The 3D models of facilities.
        /// </summary>
        private FacilityModels models = new FacilityModels();

        /// <summary>
        /// Layout of base's facilities to show in scene
        /// </summary>
        private Floorplan floorplan;

        /// <summary>
        /// The facility we are adding to the base
        /// </summary>
        private FacilityHandle newFacility;

        /// <summary>
        /// Quads to decorate facilities under construction with their build times
        /// </summary>
        private BuildTimes buildTimes;

        #endregion

        #region Constant definitions

        /// <summary>
        /// Used in constructing viewing fustrum
        /// </summary>
        private   const float nearClipPlane = 0.1f;

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
