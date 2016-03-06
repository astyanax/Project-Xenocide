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

using ProjectXenocide.Utils;
using ProjectXenocide.Model;
using ProjectXenocide.Model.Geoscape;
using ProjectXenocide.Model.Geoscape.Outposts;
using ProjectXenocide.Model.Geoscape.Vehicles;
using ProjectXenocide.Model.Geoscape.AI;
using ProjectXenocide.UI.Scenes.Common;

#endregion

namespace ProjectXenocide.UI.Scenes.Geoscape
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
		GeoHud      geoHud    = new GeoHud();
        Effect      effect;
        String      geoTechnique = String.Empty;

		private GeoPosition centerLocation;

		public GeoPosition CenterLocation
		{
			set { centerLocation = value; }
		}

		private string toolTipText = string.Empty;

		public string ToolTipText
		{
			get { return toolTipText; }
		}

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="cameraPosition">Initial position of camera in scene</param>
        public GeoscapeScene(Vector3 cameraPosition) 
            :
            base(cameraPosition)
        {
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
                if (skybox != null)
                {
                    skybox.Dispose();
                    skybox = null;
                }
            }
        }

        /// <summary>
        /// Load the graphic content of the scene
        /// </summary>
        /// <param name="content">content manager that fetches the content</param>
        /// <param name="device">the display</param>
        
        public override void LoadContent(ContentManager content, GraphicsDevice device)
        {
                InitializeEffect(device);
                earth.LoadContent(device);
                skybox.LoadContent(content, device);
                geomarker.LoadContent(device);
				geoHud.LoadContent(content, device);
                effect = content.Load<Effect>(@"Content\Shaders\GeoscapeShader");

                // figure out which shader we call to render the geoscape
                geoTechnique = (Util.GetShaderVersion(device.GraphicsDeviceCapabilities) < 2) ? "RenderGlobeStandard" : "RenderGlobeWithBump";           
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
        public override void Draw(GameTime gameTime, GraphicsDevice device, CeGui.Rect sceneWindow)
        {
            // only draw in area we've been told to
            Viewport oldview = device.Viewport;
            device.Viewport = CalcViewportForSceneWindow(sceneWindow, device.Viewport);

            // set up camera's position
            Vector3 cartesianCamera = GeoPosition.PolarToCartesian(CameraPosition);      
            Matrix worldMatrix = Matrix.CreateTranslation(cartesianCamera);

            Matrix viewMatrix = Matrix.CreateLookAt(
                cartesianCamera,
                Vector3.Zero,
                Vector3.Up
                );

            // Position skybox in world (it's centered on camera position)
            Matrix skyboxMatrix = Matrix.CreateTranslation(cartesianCamera) * viewMatrix;
            skyboxMatrix *= GetProjectionMatrix(AspectRatio);

            // draw the skybox
            skybox.Draw(device, skyboxMatrix, 0.6f);


            // Set the state for the globe
            device.RenderState.CullMode = CullMode.CullCounterClockwiseFace;
            device.RenderState.DepthBufferEnable = true;
            device.RenderState.DepthBufferWriteEnable = true;

            basicEffect.World = worldMatrix;
            basicEffect.TextureEnabled = true;
            basicEffect.Projection = GetProjectionMatrix(AspectRatio);
            basicEffect.View = viewMatrix;

            // configure basic effect for globle (want lighting on)
            basicEffect.DirectionalLight0.Direction = ComputeSunAngle();
            basicEffect.DirectionalLight0.Enabled = true;
            basicEffect.LightingEnabled = true;

            // need to rotate earth so that 0 longitute is along Z axis
            worldMatrix = Matrix.CreateRotationY((float)(Math.PI) / -2.0f);

            // setup our custom shader
            effect.CurrentTechnique = effect.Techniques[geoTechnique];
            effect.Parameters["World"].SetValue(worldMatrix);
            effect.Parameters["View"].SetValue(viewMatrix);
            effect.Parameters["Projection"].SetValue(basicEffect.Projection);
            effect.Parameters["LightDirection"].SetValue(basicEffect.DirectionalLight0.Direction);

			earth.Draw(device, effect);

            // Draw the X-Corp outposts
            geomarker.setupEffect(device, basicEffect);

            basicEffect.Alpha = 0.2f;
            basicEffect.DiffuseColor = new Vector3(0.0f, 1.0f, 0.0f);

			GeoPosition pos = new GeoPosition(CameraPosition.X , CameraPosition.Y );

			geoHud.Begin();
            foreach (Outpost outpost in Xenocide.GameState.GeoData.Outposts)
            {
				//geomarker.Draw(device, outpost.Position.Polar, basicEffect);
				geoHud.DrawIcon(device, outpost.Position.Polar, basicEffect, gameTime, outpost.Position.Distance(pos),outpost.Name,GeoHud.hudIconTypes.XCorpBase);
            }

            // Draw the X-Corp craft
            basicEffect.DiffuseColor = new Vector3(0.0f, 0.0f, 1.0f);
            foreach (Outpost outpost in Xenocide.GameState.GeoData.Outposts)
            {
                foreach (Aircraft craft in outpost.Fleet)
                {
                    if (!craft.InBase)
                    {
                       // geomarker.Draw(device, craft.Position.Polar, basicEffect);
						geoHud.DrawIcon(device, craft.Position.Polar, basicEffect, gameTime, craft.Position.Distance(pos), craft.Name, GeoHud.hudIconTypes.XCorpCraft);
                    }
                }
            }

            // Draw Visible UFOs
            basicEffect.DiffuseColor = new Vector3(1.0f, 0.0f, 0.0f);
            foreach (Ufo ufo in Xenocide.GameState.GeoData.Overmind.Ufos)
            {
                if (ufo.IsKnownToXCorp)
                {
                    //geomarker.Draw(device, ufo.Position.Polar, basicEffect.
					GeoHud.hudIconTypes iconType;
					if (ufo.IsCrashed)
						iconType = GeoHud.hudIconTypes.UfoCrash;
					else
						iconType = GeoHud.hudIconTypes.UfoFly;

					geoHud.DrawIcon(device, ufo.Position.Polar, basicEffect, gameTime, ufo.Position.Distance(pos), ufo.Name, iconType);
                }
            }

            // Draw invisible UFOs
            if (Xenocide.StaticTables.StartSettings.Cheats.ShowUndetectedUfos)
            {
                basicEffect.DiffuseColor = new Vector3(0.3f, 0.3f, 0.3f);
                foreach (Ufo ufo in Xenocide.GameState.GeoData.Overmind.Ufos)
                {
                    if (!ufo.IsKnownToXCorp)
                    {
                       // geomarker.Draw(device, ufo.Position.Polar, basicEffect);
						GeoHud.hudIconTypes iconType;
						if (ufo.IsCrashed)
							iconType = GeoHud.hudIconTypes.UfoCrash;
						else
							iconType = GeoHud.hudIconTypes.UfoFly;

						geoHud.DrawIcon(device, ufo.Position.Polar, basicEffect, gameTime, ufo.Position.Distance(pos), ufo.Name, iconType);
                    }
                }
            }

            // Draw Alien Bases & Terror sites
            basicEffect.DiffuseColor = new Vector3(0.5f, 0.5f, 0.0f);
            foreach (AlienSite site in Xenocide.GameState.GeoData.Overmind.Sites)
            {
                if (site.IsKnownToXCorp)
                {
					GeoHud.hudIconTypes iconType = GeoHud.hudIconTypes.XCorpBase;

					if (site.GetType() == typeof(OutpostAlienSite))
					{
						iconType = GeoHud.hudIconTypes.AlienBase;
					}

					if (site.GetType() == typeof(TerrorMissionAlienSite))
					{
						iconType = GeoHud.hudIconTypes.TerrorSite  ;
					}

					geoHud.DrawIcon(device, site.Position.Polar, basicEffect, gameTime, site.Position.Distance(pos), site.Name, iconType);
                }
            }

			toolTipText = geoHud.End();

            // Draw Nav Paths

            // Draw Radar

            // restore viewport
            device.Viewport = oldview;
        }

        /// <summary>
        /// Compute the angle the sun will strike the earth at, based on the Geoscape's time
        /// </summary>
        /// <returns></returns>
        private static Vector3 ComputeSunAngle()
        {
            DateTime now = Xenocide.GameState.GeoData.GeoTime.Time;

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
            float distance = origin.Distance(offset);
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
    }
}
