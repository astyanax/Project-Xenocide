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
* @file Camera.cs
* @date Created: 2008/01/01
* @author File creator: David Teviotdale
* @author Credits: none
*/
#endregion

#region Using Statements

using System;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

#endregion

namespace ProjectXenocide.UI.Scenes.Battlescape
{
    /// <summary>
    /// Camera used to view the Battlescape
    /// </summary>
    public class Camera
    {
        /// <summary>
        /// Update camera's position, in response to user input
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
            Justification = "will throw if gameTime is null")]
        public void Update(GameTime gameTime)
        {
            // scroll at 20 units/second
            float displacement = (float)gameTime.ElapsedRealTime.TotalMilliseconds * 0.02f;

            KeyboardState keyState = Keyboard.GetState();
            if (keyState.IsKeyDown(Keys.Left))
            {
                position += displacement * Left;
            }

            if (keyState.IsKeyDown(Keys.Right))
            {
                position -= displacement * Left;
            }

            if (keyState.IsKeyDown(Keys.PageUp))
            {
                position.Y += displacement;
            }

            if (keyState.IsKeyDown(Keys.PageDown) && (0.5f < position.Y))
            {
                position.Y -= displacement;
            }

            if (keyState.IsKeyDown(Keys.Up))
            {
                position += displacement * Forward;
            }

            if (keyState.IsKeyDown(Keys.Down))
            {
                position -= displacement * Forward;
            }

            // take 2 seconds to do a 360 degree rotation
            float rotation = (float)gameTime.ElapsedRealTime.TotalMilliseconds * MathHelper.Pi * 0.001f;
            float yaw = 0.0f;
            float pitch = 0.0f;
            if (keyState.IsKeyDown(Keys.A))
            {
                yaw = +rotation;
            }

            if (keyState.IsKeyDown(Keys.D))
            {
                yaw = -rotation;
            }

            if (keyState.IsKeyDown(Keys.W) && (lookVector.Y < -0.45f))
            {
                pitch = rotation;
            }

            if (keyState.IsKeyDown(Keys.X) && (-0.9f < lookVector.Y))
            {
                pitch = -rotation;
            }
            // allow for rotation in horizonal plane
            Vector3 xz = Vector3.Transform(lookVector, Matrix.CreateRotationY(yaw));
            float   x  = (float)Math.Sqrt(xz.X * xz.X + xz.Z * xz.Z);

            // rotation in vertical plane
            Vector3 xy = Vector3.Transform(new Vector3(x, lookVector.Y, 0), Matrix.CreateRotationZ(pitch));
            float scale = xy.X / x;
            lookVector = Vector3.Normalize(new Vector3(xz.X / scale, xy.Y, xz.Z / scale));
            CalcViewMatrix();
        }

        private void CalcViewMatrix()
        {
            view = Matrix.CreateLookAt(position, position + lookVector, Vector3.Up);
        }

        /// <summary>Return a unit vector in the X-Z plane in the same direction as the camera</summary>
        private Vector3 Forward { get { return Vector3.Normalize(new Vector3(lookVector.X, 0, lookVector.Z)); } }

        /// <summary>Return a unit vector in the X-Z plane, 90% counterclockwise to the camera's direction</summary>
        private Vector3 Left
        {
            get { return Vector3.Cross(Forward, Vector3.Down); }
        }

        #region Fields

        /// <summary>View matrix, to use with Effect</summary>
        public Matrix View { get { return view; } }

        /// <summary>Position of the camera in the scene</summary>
        public Vector3 Position { get { return position; } set { position = value; CalcViewMatrix(); } }

        /// <summary>Position of the camera in the scene</summary>
        private Vector3 position = new Vector3(1.5f, 6.7f, 9.2f);

        /// <summary>direction camera is looking in</summary>
        private Vector3 lookVector = new Vector3(0, -0.67f, -0.74f);

        /// <summary>view matrix that encapsulates all camera data</summary>
        private Matrix view;

        #endregion Fields
    }
}
