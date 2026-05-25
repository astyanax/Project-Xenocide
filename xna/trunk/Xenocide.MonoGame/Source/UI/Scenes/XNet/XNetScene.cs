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
* @file XNetScene.cs
* @date Created: 2007/04/01
* @author File creator: dteviot
* @author Credits: none
*/
#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using ProjectXenocide.Model;
using ProjectXenocide.Model.Geoscape;
using ProjectXenocide.UI;
using ProjectXenocide.UI.Scenes.Common;
using ProjectXenocide.Utils;

#endregion

namespace ProjectXenocide.UI.Scenes.XNet
{
    /// <summary>
    /// This is the 3D scene that appears on the X-Net screen
    /// </summary>
    public class XNetScene : PolarScene
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public XNetScene()
            :
            base(defaultCameraDistance)
        {
        }

        /// <summary>
        /// Load the graphic content of the scene
        /// </summary>
        /// <param name="content">content manager that fetches the content</param>
        /// <param name="device">the display</param>

        public override void LoadContent(ContentManager content, GraphicsDevice device)
        {
            using (Profile.Time("XNetScene.LoadContent"))
            {
                contentManger = content;
                LoadModel(Matrix.Identity);
            }
        }

        /// <summary>
        /// Render scene
        /// </summary>
        /// <param name="gameTime">time increment to use for updating scene</param>
        /// <param name="device">Device to use for render</param>
        /// <param name="sceneWindow">Where to draw the scene on the display</param>
        public override void Draw(GameTime gameTime, GraphicsDevice device, UiRect sceneWindow)
        {
            // only draw in area we've been told to
            Viewport oldview = device.Viewport;
            device.Viewport = CalcViewportForSceneWindow(sceneWindow, device.Viewport);
            Matrix projection = GetProjectionMatrix(AspectRatio);

            // set up camera's position
            Matrix viewMatrix = rotationMatrix * Matrix.CreateTranslation(0.0f, 0.0f, -CameraPosition.Z);

            device.DepthStencilState = DepthStencilState.Default;
            ;
            device.RasterizerState = RasterizerState.CullCounterClockwise;

            // draw the model
            //...Copy any parent transforms
            Matrix[] transforms = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(transforms);

            //...Draw the model, a model can have multiple meshes, so loop
            foreach (ModelMesh mesh in model.Meshes)
            {
                //This is where the mesh orientation is set, as well as our camera and projection
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.LightingEnabled = false;
                    effect.World = transforms[mesh.ParentBone.Index] * scalingMatrix;
                    effect.View = viewMatrix;
                    effect.Projection = projection;
                }
                //Draw the mesh, will use the effects set above.
                mesh.Draw();
            }

            // restore viewport
            device.Viewport = oldview;
        }

        /// <summary>
        /// Compute matrix to scale the model, to fit it inside a bounding sphere of radius 1 centered at origin.
        ///
        /// X-Net displays 3D models of varying sizes (soldiers, aircraft, facilities) in a fixed viewport
        /// with a camera at a fixed distance (defaultCameraDistance = 3.5 units). Without scaling, a soldier
        /// model would be tiny and an aircraft model would overflow the viewport. This matrix normalizes them
        /// all to a consistent visible size regardless of their original dimensions.
        ///
        /// How it works:
        /// 1. Computes the model's combined bounding sphere by walking all meshes and merging their
        ///    per-mesh bounding spheres with bone transforms applied (see Util.CalcBoundingSphere).
        /// 2. Translates the model so its center sits at the origin (CreateTranslation(-sphere.Center)).
        /// 3. Scales uniformly so the bounding sphere has radius 1 (CreateScale(1 / sphere.Radius)).
        ///    A sphere of radius 1 at origin is guaranteed to fit within the fixed camera viewport.
        /// 4. The resulting matrix is multiplied with the caller's initialRotation to produce the final
        ///    world transform for the model's root node.
        ///
        /// This matrix is computed once during preloading (see ContentCache.ComputeScalingMatrix) and
        /// reused from cache to avoid recalculating it on every model switch.
        /// </summary>
        /// <returns>The scaling matrix</returns>
        private Matrix CalculateScalingMatrix()
        {
            BoundingSphere sphere = Util.CalcBoundingSphere(model);
            // now figure out scale and displacement so fits inside sphere of radius 1 centered on origin
            return Matrix.CreateTranslation(-sphere.Center) * Matrix.CreateScale((float)(1.0f / sphere.Radius));
        }

        /// <summary>
        /// Set the 3D model to show
        /// </summary>
        /// <param name="newModelName">Name of model to show</param>
        /// <param name="initialRotation">Rotation to apply to X-Net model, to set to inital orientation</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate",
           Justification = "method performs a time-consuming operation")]
        public void SetModel(string newModelName, Matrix initialRotation)
        {
            modelName = newModelName;
            LoadModel(initialRotation);
            ResetCamera();
        }

        /// <summary>
        /// Load the model from its resource file
        /// </summary>
        /// <param name="initialRotation">The initial orientation of the model</param>
        private void LoadModel(Matrix initialRotation)
        {
            var cached = ContentCache.GetCachedModel(modelName);
            if (cached != null)
            {
                model = cached.Value.Model;
                scalingMatrix = cached.Value.ScalingMatrix * initialRotation;
                return;
            }

            try
            {
                model = contentManger.Load<Microsoft.Xna.Framework.Graphics.Model>("Models/" + modelName);
            }
            catch (ContentLoadException)
            {
                model = contentManger.Load<Microsoft.Xna.Framework.Graphics.Model>("Models/" + defaultModel);
                initialRotation = Matrix.Identity;
            }
            scalingMatrix = CalculateScalingMatrix() * initialRotation;
        }

        /// <summary>
        /// Rotate the camera around the model.
        /// </summary>
        /// <param name="longitude">Number of radians to rotate the camera around Y axis</param>
        /// <param name="latitude">Number of radians to rotate the camera around X axis</param>
        public override void RotateCamera(float longitude, float latitude)
        {
            rotationMatrix *= Matrix.CreateFromYawPitchRoll(longitude, latitude, 0.0f);
        }

        /// <summary>
        /// Applies a roll to the camera.
        /// </summary>
        /// <param name="roll">Number of radians to rotate the camera around Z axis</param>
        public void RollCamera(float roll)
        {
            rotationMatrix *= Matrix.CreateRotationZ(roll);
        }

        /// <summary>
        /// Move camera to it's default position
        /// </summary>
        private void ResetCamera()
        {
            CameraPosition = new Vector3(0.0f, 0.0f, defaultCameraDistance);
            rotationMatrix = Matrix.Identity;
        }

        #region Fields

        /// <summary>
        /// Content manager that handles loading the model
        /// </summary>
        private ContentManager contentManger;

        /// <summary>
        /// 3D Model to show 
        /// </summary>
        private Microsoft.Xna.Framework.Graphics.Model model;

        /// <summary>
        /// Factor we need to scale the model by, to get it to fit into a sphere of radius 1.0
        /// </summary>
        private Matrix scalingMatrix;

        /// <summary>
        /// Model to show on X-Net when we don't have a model
        /// </summary>
        private const string defaultModel = "XCorps";

        /// <summary>
        /// Name of the model to show in the 3D scene
        /// </summary>
        /// <remarks>default is the X-Corps logo</remarks>
        private string modelName = defaultModel;

        /// <summary>
        /// Default distance for camera from model
        /// </summary>
        private const float defaultCameraDistance = 3.5f;

        /// <summary>
        /// Matrix storing the rotation of the camera.
        /// </summary>
        private Matrix rotationMatrix = Matrix.Identity;

        #endregion Fields
    }
}
