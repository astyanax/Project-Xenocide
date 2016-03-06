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
using Xenocide.UI.Screens;

#endregion

namespace Xenocide.UI.Scenes.Common
{
    /// <summary>
    /// Base class for a scene that shows a 3D model with a camera that moves 
    /// in polar co-ordinates.  I.e. scene has single 3D model, with camera
    /// that "revolves" around the model.  e.g. Geoscape, XNet, Human Base
    /// </summary>
    public abstract class PolarScene : EmbeddedScene
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="cameraDistance">Initial distance of camera from origin</param>
        protected PolarScene(Game game, float cameraDistance)
            : base(game)
        {
            cameraPosition = new Vector3(0.0f, 0.0f, cameraDistance);
        }

        /// <summary>
        /// Rotate the camera around the model
        /// </summary>
        /// <param name="longitude">Number of radians to move the camera in the Y-Z plane</param>
        /// <param name="latitude">Number of radians to move the camera in the X-Z plane</param>
        public void RotateCamera(float longitude, float latitude)
        {
            cameraPosition.X += longitude;
            cameraPosition.Y += latitude;

            // don't rotate more than +/- 180 degrees
            while (Math.PI < cameraPosition.X)
            {
                cameraPosition.X -= (float)(2 * Math.PI);
            }
            while (cameraPosition.X < -Math.PI)
            {
                cameraPosition.X += (float)(2 * Math.PI);
            }

            // don't go up/down more than 85 degrees
            float clip = (float)(Math.PI * (0.5 * 85.0 / 90.0));
            if (clip < cameraPosition.Y)
            {
                cameraPosition.Y = clip;
            }
            if (cameraPosition.Y < -clip)
            {
                cameraPosition.Y = -clip;
            }
        }

        /// <summary>
        /// Move the camera towards (or away) from the model
        /// </summary>
        /// <param name="distance">Distance to move the camera</param>
        public void ZoomCamera(float distance)
        {
            cameraPosition.Z += distance;

            // don't let the camera go inside the earth (earth's radius is 1)
            if (cameraPosition.Z < MinZoom)
            {
                cameraPosition.Z = MinZoom;
            }

            // don't want camera to go too far away either
            if (MaxZoom < cameraPosition.Z)
            {
                cameraPosition.Z = MaxZoom;
            }
        }

        /// <summary>
        /// Compute the projection matrix for the scene
        /// </summary>
        /// <param name="aspectRatio">window's aspect ratio</param>
        /// <returns>The calculated projection matrix</returns>
        protected static Matrix GetProjectionMatrix(float aspectRatio)
        {
            return Matrix.CreatePerspectiveFieldOfView(
                ViewAngle,  
                aspectRatio,
                nearClipPlane, farClipPlane);
        }

        

        #region Fields

        /// <summary>
        /// The position of the camera, in polar co-ordinates.
        /// </summary>
        protected Vector3 CameraPosition { get { return cameraPosition; } }

        /// <summary>
        /// The position of the camera, in polar co-ordinates.
        /// </summary>
        private Vector3 cameraPosition;

        /// <summary>
        /// Returns normalized height of the camera in [0, 1] range.
        /// </summary>
        public float CameraHeight { get { return (cameraPosition.Z - MinZoom) / (MaxZoom - MinZoom); } }

        /// <summary>
        /// Maximum distance camera can be from the origin
        /// </summary>
        protected virtual float MaxZoom { get { return 5.0f; } }

        /// <summary>
        /// Minimum distance camera can be from the origin
        /// </summary>
        protected virtual float MinZoom { get { return 1.0f + nearClipPlane + 0.05f; } }


        #endregion

        #region Constant definitions

        /// <summary>
        /// Used in constructing viewing fustrum
        /// </summary>
        private   const float nearClipPlane = 0.1f;

        /// <summary>
        /// Used in constructing viewing fustrum
        /// </summary>
        private const float farClipPlane = 20.0f;

        /// <summary>
        /// Used in constructing viewing fustrum
        /// </summary>
        protected const float ViewAngle = (float)Math.PI / 4.0f;    // 45 degres

        #endregion
    }
}
