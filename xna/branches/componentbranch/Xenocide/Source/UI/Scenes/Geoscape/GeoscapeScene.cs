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
* @file GeoscapeScene.cs
* @date Created: 2007/01/25
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

using Xenocide.Model;
using Xenocide.Model.Geoscape;
using Xenocide.Model.Geoscape.HumanBases;
using Xenocide.Model.Geoscape.Craft;
using Xenocide.UI.Scenes.Common;

#endregion

namespace Xenocide.UI.Scenes.Geoscape
{
    /// <summary>
    /// This is the 3D scene that appears on the Geoscape screen
    /// </summary>
    public class GeoscapeScene : PolarScene, IDisposable
    {
        BasicEffect basicEffect;
        EarthGlobe  earth     = new EarthGlobe();
        SkyBox      skybox    = new SkyBox();
        GeoMarker   geomarker = new GeoMarker();

        IGeoTimeService timeService;

        /// <summary>
        /// Default constructor
        /// </summary>
        public GeoscapeScene(Game game) 
            :
            base(game, 3.5f)
        {
        }

        public override void Initialize()
        {
            base.Initialize();
            humanBaseService = (IHumanBaseService)Game.Services.GetService(typeof(IHumanBaseService));
            timeService = (IGeoTimeService)Game.Services.GetService(typeof(IGeoTimeService));
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
            }
        }

        /// <summary>
        /// Load the graphic content of the scene
        /// </summary>
        /// <param name="content">content manager that fetches the content</param>
        /// <param name="device">the display</param>
        /// <param name="loadAllContent">true if all graphics resources need to be loaded</param>
        protected override void LoadGraphicsContent(bool loadAllContent)
        {
            if (loadAllContent)
            {
                GraphicsDevice device = GraphicsDeviceService.GraphicsDevice;
                InitializeEffect(device);
                earth.LoadGraphicsContent(ContentManager, device);
                skybox.LoadGraphicsContent(ContentManager, device);
                geomarker.LoadGraphicsContent(device);
            }

            base.LoadGraphicsContent(loadAllContent);
        }

        private void InitializeEffect(GraphicsDevice device)
        {
            basicEffect = new BasicEffect(device, null);
            basicEffect.Alpha = 1.0f;
            basicEffect.DiffuseColor = new Vector3(1.0f, 1.0f, 1.0f);
            basicEffect.SpecularColor = new Vector3(0.25f, 0.25f, 0.25f);
            basicEffect.SpecularPower = 5.0f;
            basicEffect.AmbientLightColor = new Vector3(0.40f, 0.40f, 0.40f);

            basicEffect.DirectionalLight0.Enabled = true;
            basicEffect.DirectionalLight0.DiffuseColor = Vector3.One;
            basicEffect.DirectionalLight0.SpecularColor = Vector3.One;

            basicEffect.DirectionalLight1.Enabled = false;

            basicEffect.LightingEnabled = true;
        }

        /// <summary>
        /// Render the scene to the display
        /// </summary>
        /// <param name="gameTime">Time since last render</param>
        /// <param name="device">device to render to</param>
        /// <param name="sceneWindow">child window to render scene to</param>
        public override void Draw(GameTime gameTime)
        {
            
        //public void Draw(GameTime gameTime, GraphicsDevice device, CeGui.Rect sceneWindow)
            // only draw in area we've been told to
            GraphicsDevice device = GraphicsDeviceService.GraphicsDevice;
            Viewport oldview = device.Viewport;
            device.Viewport = CalcViewportForSceneWindow(SceneWindow, device.Viewport);
            basicEffect.Projection = GetProjectionMatrix(AspectRatio);
            
            // set up camera's position
            Vector3 cartesianCamera = GeoPosition.PolarToCartesian(CameraPosition);

            Matrix viewMatrix = Matrix.CreateLookAt(
                cartesianCamera,
                Vector3.Zero,
                Vector3.Up
                );
            basicEffect.View = viewMatrix;
            device.RenderState.CullMode = CullMode.CullClockwiseFace;
            device.RenderState.DepthBufferEnable = true;
            device.RenderState.DepthBufferWriteEnable = true;
            
            // configure effect for skybox (no lighting)
            basicEffect.DirectionalLight0.Enabled = false;
            basicEffect.LightingEnabled = false;
            basicEffect.DiffuseColor = new Vector3(1.0f, 1.0f, 1.0f);
            basicEffect.Alpha = 1.0f;

            // Position skybox in world (it's centered on camera position)
            Matrix worldMatrix = Matrix.CreateTranslation(cartesianCamera);
            
            // draw the skybox
            skybox.Draw(device, worldMatrix, basicEffect);

            // need to rotate earth so that 0 longitute is along Z axis
            worldMatrix = Matrix.CreateRotationY((float)(Math.PI) / -2.0f);

            // configure basic effect for globle (want lighting on)
            basicEffect.DirectionalLight0.Direction = ComputeSunAngle();
            basicEffect.DirectionalLight0.Enabled = true;
            basicEffect.LightingEnabled = true;

            earth.Draw(device, worldMatrix, basicEffect);

            // Draw the human bases
            geomarker.setupEffect(device, basicEffect);
            basicEffect.Alpha = 0.2f;
            basicEffect.DiffuseColor = new Vector3(0.0f, 1.0f, 0.0f);
            foreach (HumanBase humanBase in humanBaseService.HumanBases)
            {
                geomarker.Draw(device, humanBase.Position.Polar, basicEffect);
            }

            // Draw the human craft
            basicEffect.DiffuseColor = new Vector3(0.0f, 0.0f, 1.0f);
            foreach (HumanBase humanBase in humanBaseService.HumanBases)
            {
                foreach (Craft craft in humanBase.Fleet)
                {
                    if (craft.CanDrawOnGeoscape())
                    {
                        geomarker.Draw(device, craft.Position.Polar, basicEffect);
                    }
                }
            }

            // Draw UFOs
            basicEffect.DiffuseColor = new Vector3(1.0f, 0.0f, 0.0f);
            foreach (Ufo ufo in Xenocide.GameState.GeoData.Overmind.Ufos)
            {
                if (ufo.CanDrawOnGeoscape())
                {
                    geomarker.Draw(device, ufo.Position.Polar, basicEffect);
                }
            }

            // Draw Alien Bases

            // Draw Nav Paths

            // Draw Radar

            // restore viewport
            device.Viewport = oldview;
        }

        /// <summary>
        /// Compute the angle the sun will strike the earth at, based on the Geoscape's time
        /// </summary>
        /// <returns></returns>
        private Vector3 ComputeSunAngle()
        {
            DateTime now = timeService.Time;

            // Fraction of a day (note that 0:0:0 UTC is midnight)
            TimeSpan dayFraction = now.TimeOfDay;
            float dayAngle = (float)((Math.PI * -2.0 * dayFraction.TotalSeconds / 86400.0));

            // Fraction of a year
            int dayOfYear = now.DayOfYear - 1;
            double yearAngle = (Math.PI * 2.0 * dayOfYear / 365.0);

            // assume 1st January is midwinter.  (Close enough)
            float latitudeAngle = (float)(Math.Cos(yearAngle) * MathHelper.ToRadians(22.5f));

            return GeoPosition.PolarToCartesian((float)dayAngle, latitudeAngle);
        }

        /// <summary>
        /// Convert a position in the viewport to a geoposition on the globe
        /// </summary>
        /// <param name="coords">The position in the viewport (in relative co-ords)</param>
        /// <returns>The geoposition or null if point isn't on globe</returns>
        /// <remarks>Uses equation from http://wikipedia.org/Ray-sphere_intersection.htm</remarks>
        public GeoPosition WindowToGeoPosition(PointF coords)
        {
            double lx = Math.Tan(ViewAngle / 2.0) * (coords.X - 0.5) * 2.0 * AspectRatio;
            double ly = Math.Tan(ViewAngle / 2.0) * (coords.Y - 0.5) * -2.0;
            double sz = CameraPosition.Z;

            double term = (sz * sz) - ((lx * lx) + (ly * ly) + 1) * ((sz * sz) - 1);

            // if term is negative, then postion isn't on the globe
            // we're also going to ignore the result when we're close to edge of globe
            // because it's difficult to hit a point accurately around there
            if (term < 0.1f)
            {
                return null;
            }

            // we're only interested in the near solution
            double d = (sz - Math.Sqrt(term)) / ((lx * lx) + (ly * ly) + 1);

            // convert back to get cartesian co-ordinates
            double x = lx * d;
            double y = ly * d;
            double z = sz - d;

            TestWindowToGeoPositionCalcs(x, y, z);

            // convert to polar
            GeoPosition origin = new GeoPosition(0.0f, 0.0f);
            GeoPosition offset = new GeoPosition((float)Math.Atan(x / z), (float)Math.Asin(y));

            // add in camera's displacement
            float distance = origin.GetDistance(offset);
            float azimuth  = origin.GetAzimuth(offset);
            GeoPosition camera = new GeoPosition(CameraPosition.X, CameraPosition.Y);
            return camera.GetEndpoint(azimuth, distance);
        }

        /// <summary>
        /// Verify that the point at co-ords x, y and z lies on sphere of radius 1.0
        /// </summary>
        /// <param name="x">x co-ord</param>
        /// <param name="y">y co-ord</param>
        /// <param name="z">z co-ord</param>
        [Conditional("DEBUG")]
        private static void TestWindowToGeoPositionCalcs(double x, double y, double z)
        {
            double hyp = (z * z) + (x * x) + (y * y);
            Debug.Assert((0.99 < hyp) && (hyp < 1.01));
        }

        private IHumanBaseService humanBaseService;
    }
}
