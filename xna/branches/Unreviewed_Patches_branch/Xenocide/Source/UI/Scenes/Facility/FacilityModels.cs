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
* @file FacilityModels.cs
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

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

using ProjectXenocide.Model.StaticData;
using ProjectXenocide.Model.StaticData.Facilities;

#endregion

namespace ProjectXenocide.UI.Scenes.Facility
{
    /// <summary>
    /// The 3D models of the different facilities that can be in a X-Corp outpost
    /// </summary>
    internal class FacilityModels
    {
        /// <summary>
        /// Load a copy of each of the facility models
        /// </summary>
        /// <param name="content">content manager that fetches the content</param>
        public void LoadContent(ContentManager content)
        {
            models = new Dictionary<String, Microsoft.Xna.Framework.Graphics.Model>();
            foreach (FacilityInfo facility in Xenocide.StaticTables.FacilityList)
            {
                models.Add(
                    facility.Id,
                    content.Load<Microsoft.Xna.Framework.Graphics.Model>("Content\\Models\\" + facility.ModelName)
               );
            }
        }

        /// <summary>
        /// Render a facility
        /// </summary>
        /// <param name="basicEffect">Holds assorted rendering information</param>
        /// <param name="facilityId">Name of facility type to draw</param>
        /// <param name="displacement">Position of the facility</param>
        public void Draw(BasicEffect basicEffect, String facilityId, Matrix displacement)
        {
            // get the model for this type of facility
            Microsoft.Xna.Framework.Graphics.Model model = models[facilityId];

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
                    effect.World = transforms[mesh.ParentBone.Index] * displacement;
                    effect.View = basicEffect.View;
                    effect.Projection = basicEffect.Projection;
                }
                //Draw the mesh, will use the effects set above.
                mesh.Draw();
            }
        }

        #region Fields

        /// <summary>
        /// The 3D models for the different facilities
        /// </summary>
        private Dictionary<String, Microsoft.Xna.Framework.Graphics.Model> models;

        #endregion Fields
    }
}
