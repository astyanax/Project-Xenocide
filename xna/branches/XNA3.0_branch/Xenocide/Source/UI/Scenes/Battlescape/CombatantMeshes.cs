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
* @file CombatantMeshes.cs
* @date Created: 2008/01/06
* @author File creator: David Teviotdale
* @author Credits: none
*/
#endregion

#region Using Statements

using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

using ProjectXenocide.Utils;
using ProjectXenocide.Model.Battlescape;
using ProjectXenocide.Model.Battlescape.Combatants;
using ProjectXenocide.Model.StaticData;

using XnaModel = Microsoft.Xna.Framework.Graphics.Model;

#endregion

namespace ProjectXenocide.UI.Scenes.Battlescape
{
    /// <summary>
    /// the set of 3D models for the various combatants
    /// </summary>
    public class CombatantMeshes
    {
        /// <summary>
        /// Load models for all the combatants on the battlescape
        /// </summary>
        /// <param name="content">content manager that fetches the content</param>
        /// <param name="battlescape">the battlescape</param>
        public void LoadContent(ContentManager content, Battle battlescape)
        {
            // Draw combatants
            foreach (Team team in battlescape.Teams)
            {
                foreach (Combatant combatant in team.Combatants)
                {
                    string modelName = combatant.Graphic.Model;
                    if (!models.ContainsKey(modelName))
                    {
                        XnaModel model = content.Load<XnaModel>("Content\\Models\\" + modelName);
                        models.Add(modelName, new ModelInfo(model, combatant.Graphic));
                    }
                }
            }
        }

        /// <summary>
        /// Render the combatant
        /// </summary>
        /// <param name="combatant">combatant to render</param>
        /// <param name="basicEffect">effect to use to draw the combatant</param>
        /// <param name="visible">is X-Corp supposed to be able to see this combatant?</param>
        public void Draw(Combatant combatant, BasicEffect basicEffect, bool visible)
        {
            // fetch model
            ModelInfo modelInfo = models[combatant.Graphic.Model];

            // set up world matrix to position object in world
            Matrix world = modelInfo.scalingMatrix;

            //... if combatant is dead or stunned, plant model face down on ground
            if (combatant.CanTakeOrders)
            {
                world *= Matrix.CreateRotationY(combatant.Heading);
            }
            else
            {
                world *= Matrix.CreateRotationZ(-MathHelper.PiOver2) * Matrix.CreateTranslation(-0.5f, 0, 0);
            }
            world *= Matrix.CreateTranslation(BattlescapeScene.CellToWorld(combatant.Position));

            // draw the model
            //...Copy any parent transforms
            Matrix[] transforms = new Matrix[modelInfo.model.Bones.Count];
            modelInfo.model.CopyAbsoluteBoneTransformsTo(transforms);

            //...Draw the model, a model can have multiple meshes, so loop
            foreach (ModelMesh mesh in modelInfo.model.Meshes)
            {
                //This is where the mesh orientation is set, as well as our camera and projection
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.LightingEnabled = false;
                    effect.World = transforms[mesh.ParentBone.Index] * world;
                    effect.View       = basicEffect.View;
                    effect.Projection = basicEffect.Projection;
                    effect.Alpha      = visible ? 1.0f : 0.5f ;
                }
                //Draw the mesh, will use the effects set above.
                mesh.Draw();
            }
        }

        #region Fields

        /// <summary>
        /// Hold the data on a combatant's 3D model
        /// </summary>
        private struct ModelInfo
        {
            /// <summary>
            /// Ctor
            /// </summary>
            /// <param name="model">The model</param>
            /// <param name="graphic">Scaling and orientation information for model</param>
            public ModelInfo(XnaModel model, Graphic graphic)
            {
                this.model         = model;

                // compute matrix to put model to scale and default orientation
                BoundingSphere sphere = Util.CalcBoundingSphere(model);

                // Scaling factor for model, use value from graphic, or scale to bounding sphere if none supplied
                float scale = graphic.Scale;
                if (0 == scale)
                {
                    scale = (1.0f / sphere.Radius);
                }
                scalingMatrix = Matrix.CreateScale(scale) * graphic.InitialRotation;

                // adjust for origin of model not being centered correctly in XZ plane
                Vector3 displacement = Vector3.Transform(sphere.Center, scalingMatrix);
                scalingMatrix *= Matrix.CreateTranslation(-displacement.X, 0, -displacement.Z);
            }

            public XnaModel model;
            public Matrix   scalingMatrix;
        }

        /// <summary>The models</summary>
        private Dictionary<string, ModelInfo> models = new Dictionary<string, ModelInfo>();

        #endregion Fields
    }
}
