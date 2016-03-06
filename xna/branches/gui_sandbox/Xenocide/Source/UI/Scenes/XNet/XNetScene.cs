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
using System.Text;
using System.Diagnostics;
using System.Drawing;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

using Xenocide.UI.Scenes.Common;
using Xenocide.Model;
using Xenocide.Model.Geoscape;

#endregion

namespace Xenocide.UI.Scenes.XNet
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
            base(3.5f)
        {
        }

        /// <summary>
        /// Load the graphic content of the scene
        /// </summary>
        /// <param name="content">content manager that fetches the content</param>
        /// <param name="device">the display</param>
        /// <param name="loadAllContent">true if all graphics resources need to be loaded</param>
        public override void LoadGraphicsContent(ContentManager content, GraphicsDevice device, bool loadAllContent)
        {
            contentManger = content;
            if (loadAllContent)
            {
                LoadModel();
            }
        }

        /// <summary>
        /// Render scene
        /// </summary>
        /// <param name="gameTime">time increment to use for updating scene</param>
        /// <param name="device">Device to use for render</param>
        /// <param name="sceneWindow">Where to draw the scene on the display</param>
        public override void Draw(GameTime gameTime, GraphicsDevice device, CeGui.Rect sceneWindow)
        {
            // only draw in area we've been told to
            Viewport oldview = device.Viewport;
            device.Viewport = CalcViewportForSceneWindow(sceneWindow, device.Viewport);
            Matrix projection = GetProjectionMatrix(AspectRatio);
            
            // set up camera's position
            Vector3 cartesianCamera = GeoPosition.PolarToCartesian(CameraPosition);

            Matrix viewMatrix = Matrix.CreateLookAt(
                cartesianCamera,
                Vector3.Zero,
                Vector3.Up
                );

            device.RenderState.DepthBufferEnable = true;
            device.RenderState.DepthBufferWriteEnable = true;

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
        /// Compute matrix to scale the model, to fit it inside a bounding sphere of radius 1
        /// </summary>
        /// <returns>The scaling matrix</returns>
        private Matrix CalculateScalingMatrix()
        {
            // first, compute model's current size
            BoundingSphere sphere = model.Meshes[0].BoundingSphere;
            foreach (ModelMesh mesh in model.Meshes)
            {
                sphere = BoundingSphere.CreateMerged(sphere, mesh.BoundingSphere);
            }

            // now figure out how to scale so fits inside sphere of radius 1
            return Matrix.CreateScale((float)(1.0f / sphere.Radius));
        }

        /// <summary>
        /// Set the 3D model to show
        /// </summary>
        /// <param name="newModelName">Name of model to show</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate",
           Justification = "method performs a time-consuming operation")]
        public void SetModel(string newModelName)
        {
            modelName = newModelName;
            LoadModel();
        }
        
        /// <summary>
        /// Load the model from its resource file
        /// </summary>
        private void LoadModel()
        {
            model = contentManger.Load<Microsoft.Xna.Framework.Graphics.Model>("Content\\Models\\" + modelName);
            scalingMatrix = CalculateScalingMatrix();
        }

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
        /// Name of the model to show in the 3D scene
        /// </summary>
        /// <remarks>default is stun launcher, because that's the only model we have</remarks>
        private string modelName = "Stun Launcher";
    }
}
