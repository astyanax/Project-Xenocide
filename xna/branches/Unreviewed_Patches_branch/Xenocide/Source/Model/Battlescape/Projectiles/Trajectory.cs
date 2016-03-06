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
* @file Trajectory.cs
* @date Created: 2008/02/24
* @author File creator: David Teviotdale
* @author Credits: none
*/
#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

using Microsoft.Xna.Framework;


using ProjectXenocide.Utils;

#endregion

namespace ProjectXenocide.Model.Battlescape
{
    /// <summary>The path taken by an object that has been fired or thrown.
    /// e.g. a missile, bullet, plasma burst, thrown object, etc.
    /// </summary>
    public partial class Trajectory
    {
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="start">Where projectile started its journey</param>
        /// <param name="end">Where projectile will finish its journey</param>
        public Trajectory(Vector3 start, Vector3 end)
            : this(start, end, 10)
        {
        }

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="start">Where projectile started its journey</param>
        /// <param name="end">Where projectile will finish its journey</param>
        /// <param name="speed">Projectile's speed, in cells/second</param>
        public Trajectory(Vector3 start, Vector3 end, float speed)
        {
            this.end      = end;
            this.current  = start;
            this.speed    = speed;
            this.velocity = Vector3.Normalize(end - start) * speed;
        }

        /// <summary>Update projectile's position</summary>
        /// <param name="seconds">time to update projectile's progress by</param>
        /// <returns>true if projectile still travelling, false if has reached destination</returns>
        public bool Update(float seconds)
        {
            float toGo  = Vector3.DistanceSquared(current, end);
            float delta = seconds * speed;
            current += (velocity * seconds);
            return ((delta * delta) < toGo);
        }

        #region fields

        /// <summary>Where projectile will finish its journey</summary>
        public Vector3 End { get { return end; } }

        /// <summary>Where projectile currently is</summary>
        public Vector3 Current { get { return current; } }

        /// <summary>How many cells projectile will travel each second</summary>
        public Vector3 Velocity { get { return velocity; } }

        /// <summary>Where projectile will finish its journey</summary>
        private Vector3 end;

        /// <summary>Where projectile currently is</summary>
        private Vector3 current;

        /// <summary>How many cells projectile will travel each second</summary>
        private Vector3 velocity;

        /// <summary>How many cells projectile will travel each second</summary>
        private float speed;

        #endregion fields
    }
}
