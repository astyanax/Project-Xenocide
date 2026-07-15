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

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using ProjectXenocide.Model.Geoscape.Outposts;
using ProjectXenocide.Model.StaticData.Facilities;
using ProjectXenocide.UI;

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
            this.floorplan = floorplan;
            this.cameraPosition = ComputeCameraPosition();
            this.buildTimes = new BuildTimes(this.floorplan);
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
                DisposeFloor();
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
                grid.LoadContent(device, new Grid(Floorplan.CellsWide, Floorplan.CellsHigh));
                models.LoadContent(content);
                buildTimes.LoadContent(content, device);
                CreateFloor(device, content);
            }
        }

        /// <summary>
        /// Render scene
        /// </summary>
        /// <param name="device">Device to use for render</param>
        /// <param name="sceneWindow">Where to draw the scene on the display</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
            Justification = "Will throw exception if device is null")]
        public void Draw(GraphicsDevice device, UiRect sceneWindow)
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

            device.RasterizerState = RasterizerState.CullCounterClockwise;
            device.DepthStencilState = DepthStencilState.Default;
            ;

            // draw the underground floor plane (behind the grid lines)
            DrawFloor(device);

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
        public Vector2 WindowToCell(UiPoint coords)
        {
            // basic trig, compute viewing fustrum dimension at distance base floorplan is
            double opositeSideLength = Math.Tan(ViewAngle / 2.0) * cameraPosition.Y * 2.0f;

            double x = opositeSideLength * AspectRatio * (coords.X - 0.5);
            double z = opositeSideLength * (coords.Y - 0.5);

            // Allow for center of screen being center of base
            x += (Floorplan.CellsWide / 2.0f);
            z += (Floorplan.CellsHigh / 2.0f);

            // check that result is within the base
            if ((x < 0.0f) || (Floorplan.CellsWide < x) || (z < 0.0f) || (Floorplan.CellsHigh < z))
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
                float xdisp = handle.X + ((info.XSize - Floorplan.CellsWide) / 2.0f);
                float zdisp = handle.Y + ((info.YSize - Floorplan.CellsHigh) / 2.0f);
                Matrix displacement = Matrix.CreateTranslation(xdisp, 0.0f, zdisp);

                if (handle == newFacility)
                {
                    bool valid;
                    if (floorplan.IsBaseEmpty())
                    {
                        valid = true;
                    }
                    else
                    {
                        valid = floorplan.IsPositionLegal(handle) == XenoError.None;
                    }

                    models.Draw(basicEffect, info.Id, displacement,
                        valid ? Vector3.UnitY : Vector3.UnitX, 0.5f);
                }
                else
                {
                    models.Draw(basicEffect, info.Id, displacement);
                }
            }
        }

        /// <summary>
        /// Set up the basic effect for rendering
        /// </summary>
        /// <param name="device"></param>
        private void InitializeEffect(GraphicsDevice device)
        {
            basicEffect = new BasicEffect(device);
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
        private static Vector3 ComputeCameraPosition()
        {
            // assumes aspect ratio is 1.0.
            // problem is, at this point in time, it's not known.
            float oposite = MathHelper.Max(Floorplan.CellsHigh, Floorplan.CellsWide) * 0.5f;
            float adjacent = (float)(oposite / Math.Tan(ViewAngle * 0.5));
            return new Vector3(0.0f, adjacent + 1.0f, 0.0f);
        }

        /// <summary>
        /// convert Window's co-ordinates to viewport co-ordinates
        /// </summary>
        /// <param name="windowCoords">Window co-ords to translate</param>
        /// <param name="viewport">The current viewport</param>
        /// <returns>Viewport co-ordinates</returns>
        private Viewport CalcViewportForSceneWindow(UiRect windowCoords, Viewport viewport)
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

        #region Underground floor plane

        /// <summary>
        /// Creates a textured quad representing the underground floor beneath the base grid.
        /// The floor uses a tiled dirt/concrete texture loaded from the BaseDirtFloor asset
        /// and sits just below y=0 so the grid lines remain visible on top.
        /// </summary>
        private void CreateFloor(GraphicsDevice device, ContentManager content)
        {
            floorTexture = content.Load<Texture2D>(@"Textures/UI/BaseDirtFloor");

            // The floorplan is 6x6 cells, centered at origin.
            // Grid spans from -3 to +3 on both X and Z.  We extend 0.5 beyond
            // the outer grid lines so the floor fills the entire visible area.
            float halfW = Floorplan.CellsWide * 0.5f + 0.5f;
            float halfH = Floorplan.CellsHigh * 0.5f + 0.5f;
            float y = -0.01f; // just below the grid lines

            // Tile the texture ~3 times per cell so the pattern is finer and
            // better proportioned to the facility model sizes on the grid.
            float tileW = Floorplan.CellsWide * 3;
            float tileH = Floorplan.CellsHigh * 3;

            VertexPositionTexture[] verts = new VertexPositionTexture[4];
            verts[0] = new VertexPositionTexture(new Vector3(-halfW, y, -halfH), new Vector2(0, 0));
            verts[1] = new VertexPositionTexture(new Vector3(halfW, y, -halfH), new Vector2(tileW, 0));
            verts[2] = new VertexPositionTexture(new Vector3(-halfW, y, halfH), new Vector2(0, tileH));
            verts[3] = new VertexPositionTexture(new Vector3(halfW, y, halfH), new Vector2(tileW, tileH));

            floorVertexBuffer = new VertexBuffer(device, VertexPositionTexture.VertexDeclaration, 4, BufferUsage.None);
            floorVertexBuffer.SetData(verts);

            floorIndexBuffer = new IndexBuffer(device, IndexElementSize.SixteenBits, 6, BufferUsage.None);
            floorIndexBuffer.SetData(new short[] { 0, 1, 2, 1, 3, 2 });
        }

        /// <summary>
        /// Draws the underground floor plane as a textured quad.
        /// The effect is configured for unlit textured rendering so the
        /// dirt/concrete pattern shows clearly behind the facility models.
        /// </summary>
        private void DrawFloor(GraphicsDevice device)
        {
            if (floorVertexBuffer == null || floorIndexBuffer == null)
                return;

            device.SetVertexBuffer(floorVertexBuffer);
            device.Indices = floorIndexBuffer;

            // Use wrap addressing so the 6x6 UV range tiles the texture across
            // each grid cell rather than stretching or clamping at the edges.
            device.SamplerStates[0] = SamplerState.LinearWrap;

            basicEffect.Texture = floorTexture;
            basicEffect.TextureEnabled = true;
            basicEffect.LightingEnabled = false;
            basicEffect.VertexColorEnabled = false;
            basicEffect.World = Matrix.Identity;

            foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 2);
            }
        }

        /// <summary>
        /// Releases GPU resources allocated for the floor plane.
        /// </summary>
        private void DisposeFloor()
        {
            floorVertexBuffer?.Dispose();
            floorVertexBuffer = null;
            floorIndexBuffer?.Dispose();
            floorIndexBuffer = null;
            floorTexture = null;
        }

        private VertexBuffer floorVertexBuffer;
        private IndexBuffer floorIndexBuffer;
        private Texture2D floorTexture;

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
