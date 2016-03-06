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
* @file FacilityMesh.cs
* @date Created: 2008/03/06
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
using ProjectXenocide.Model.Battlescape.Combatants;

using XnaModel = Microsoft.Xna.Framework.Graphics.Model;
using ProjectXenocide.Utils;

#endregion

namespace ProjectXenocide.UI.Scenes.Battlescape
{
    /// <summary>
    /// Hacked up class to draw a facility on the battlescape
    /// </summary>
    public class FacilityMesh
    {
        /// <summary>
        /// Load models for all the combatants on the battlescape
        /// </summary>
        /// <param name="content">content manager that fetches the content</param>
        public void LoadContent(ContentManager content)
        {
            model = content.Load<XnaModel>(@"Content\Models\Facility\Xnet\Barracks");
            BuildScalingMatrix();
        }

        /// <summary>
        /// Render the facility
        /// </summary>
        /// <param name="basicEffect">effect to use to draw the facility</param>
        public void Draw(BasicEffect basicEffect)
        {
            // set up world matrix to position object in world
            Matrix world = scalingMatrix;

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
                    //effect.Alpha      = 0.7f ;
                }
                //Draw the mesh, will use the effects set above.
                mesh.Draw();
            }
        }

        #region Fields

        // construct a matrix to draw facility at correct scale, position and orientation
        private void BuildScalingMatrix()
        {
            // note, this is a quick hack up that aproximately works
            scalingMatrix = Matrix.CreateScale((float)(26.6667f * 0.7071f / 4985.681f)) *
                Matrix.CreateTranslation(12.0947f, 0, 12.0947f);
        }

        private XnaModel model;

        private Matrix scalingMatrix;

        #endregion Fields
    }
}
