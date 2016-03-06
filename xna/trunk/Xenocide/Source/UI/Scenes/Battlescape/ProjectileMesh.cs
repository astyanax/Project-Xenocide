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
* @file ProjectileMesh.cs
* @date Created: 2008/02/24
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

using ProjectXenocide.Model.Battlescape;

using XnaModel = Microsoft.Xna.Framework.Graphics.Model;
using ProjectXenocide.Utils;

#endregion

namespace ProjectXenocide.UI.Scenes.Battlescape
{
    /// <summary>Draws projectile currently in flight (when there is one)</summary>
    /// <remarks>ToDo: This is really a hacked stub, needs to be replaced with something better</remarks>
    public class ProjectileMesh
    {
        /// <summary>
        /// Load model we will use for projectile
        /// </summary>
        /// <param name="content">content manager that fetches the content</param>
        public void LoadContent(ContentManager content)
        {
            // We're just going to use a missile mesh to start with
            model = content.Load<XnaModel>(@"Content\Models\Craft\Weapons\Avalanche");
            BoundingSphere sphere = Util.CalcBoundingSphere(model);
            scalingMatrix = Matrix.CreateScale((float)(0.1f / sphere.Radius));
        }

        /// <summary>
        /// Render the combatant
        /// </summary>
        /// <param name="trajectory">the projectile's trajectory</param>
        /// <param name="basicEffect">effect to use to draw the combatant</param>
        public void Draw(Trajectory trajectory, BasicEffect basicEffect)
        {
            // compute angles to rotate projectile onto trajectory.
            Vector3 unit = Vector3.Normalize(trajectory.Velocity);
            float yaw   = (float)Math.Atan2(-unit.Z, unit.X);
            float pitch = (float)Math.Asin(unit.Y);

            // set up world matrix to position object in world
            Matrix world = scalingMatrix *
                Matrix.CreateRotationZ(pitch) *
                Matrix.CreateRotationY(yaw) *
                Matrix.CreateTranslation(BattlescapeScene.CellToWorld(trajectory.Current));

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
                    effect.World = transforms[mesh.ParentBone.Index] * world;
                    effect.View       = basicEffect.View;
                    effect.Projection = basicEffect.Projection;
                }
                //Draw the mesh, will use the effects set above.
                mesh.Draw();
            }
        }

        #region Fields

        /// <summary>3D model we're going to use for the projectile</summary>
        private XnaModel model;

        /// <summary>Adjust model to meet our requirements</summary>
        private Matrix scalingMatrix;

        #endregion Fields
    }
}
