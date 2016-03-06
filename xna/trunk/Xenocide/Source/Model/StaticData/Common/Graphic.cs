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
* @file Graphic.cs
* @date Created: 2008/01/06
* @author File creator: David Teviotdale
* @author Credits: none
*/
#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using System.Text;
using System.Diagnostics;
using System.Drawing;
using System.Xml;
using System.Xml.XPath;


using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Storage;

using ProjectXenocide.Utils;

#endregion

namespace ProjectXenocide.Model.StaticData
{
    /// <summary>
    /// 3D model shown on X-Net or Battlescape
    /// </summary>
    [Serializable]
    public class Graphic
    {
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="model">Name of 3D model to show</param>
        public Graphic(string model)
        {
            this.model = model;
        }

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="model">Name of 3D model to show</param>
        /// <param name="x">initial orientation when showing 3D model</param>
        /// <param name="y">initial orientation when showing 3D model</param>
        /// <param name="z">initial orientation when showing 3D model</param>
        public Graphic(string model, float x, float y, float z)
        {
            this.model           = model;
            this.initialRotation = new Vector3(x, y, z);
        }

        /// <summary>
        /// Construct from XML element
        /// </summary>
        /// <param name="graphicNode">XML node holding data needed to construct</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
           Justification = "will throw if graphicNode == null")]
        public Graphic(XPathNavigator graphicNode)
        {
            model = graphicNode.GetAttribute("model", String.Empty);

            // any entry with ".mesh" isn't one we want
            if (model.EndsWith(".mesh"))
            {
                model = "XCorps";
            }

            // Initial orientation when showing 3D model on X-Net
            initialRotation.X = Util.GetDegreesAttribute(graphicNode, "x");
            initialRotation.Y = Util.GetDegreesAttribute(graphicNode, "y");
            initialRotation.Z = Util.GetDegreesAttribute(graphicNode, "z");

            scale = Util.GetFloatAttribute(graphicNode, "scale");
        }

        #region Methods

        #endregion

        #region Fields

        /// <summary>
        /// Name of 3D model to show
        /// </summary>
        public string Model { get { return model; } }

        /// <summary>
        /// Rotation to apply to model, to set to inital orientation
        /// </summary>
        public Matrix InitialRotation
        {
            get
            {
                return Matrix.CreateRotationX(initialRotation.X) *
                    Matrix.CreateRotationY(initialRotation.Y) *
                    Matrix.CreateRotationZ(initialRotation.Z);
            }
        }

        /// <summary>Scaling factor to apply to model to make it correct size</summary>
        public float Scale { get { return scale; } }

        /// <summary>
        /// Name of 3D model to show 
        /// </summary>
        private string model;

        /// <summary>
        /// Rotation to apply to model, to set to inital orientation
        /// </summary>
        private Vector3 initialRotation;

        /// <summary>Scaling factor to apply to model to make it correct size</summary>
        private float scale;

        #endregion
    }
}
