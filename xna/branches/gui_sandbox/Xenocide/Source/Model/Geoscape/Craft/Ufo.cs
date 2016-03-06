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
* @file Ufo.cs
* @date Created: 2007/02/10
* @author File creator: dteviot
* @author Credits: none
*/
#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.Text;

using Xenocide.Model.Geoscape.AI;
using Xenocide.Resources;

using Xenocide.Model.Geoscape.HumanBases;

#endregion

namespace Xenocide.Model.Geoscape.Craft
{
    /// <summary>
    /// A craft owned by the alien
    /// </summary>
    [Serializable]
    public class Ufo : Craft
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="position">Initial location of UFO</param>
        /// <param name="maxSpeed">Maximum speed UFO is capable of</param>
        /// <param name="task">Task that created (and "owns") the UFO</param>
        public Ufo(double maxSpeed, GeoPosition position, InvasionTask task) :
            base(maxSpeed, position)
        {
            this.task = task;
        }

        /// <summary>
        /// Find out if this craft should be drawn on the Geoscape
        /// </summary>
        /// <returns>true if this craft shold be drawn</returns>
        /// <remarks>this is currently a stub</remarks>
        override public bool CanDrawOnGeoscape() { return true; }

        /// <summary>
        /// Respond to this craft being destroyed
        /// </summary>
        /// <remarks>default behaviour is delegate to state</remarks>
        public override void OnDestroyed()
        {
            base.OnDestroyed();

            // tell owner that we've been destroyed
            Task.OnUfoDestroyed(this);
        }

        /// <summary>
        /// Respond to this craft finishing it's current mission
        /// </summary>
        public override void OnMissionFinished()
        {
            base.OnMissionFinished();

            // tell owner that we're done
            Task.OnMissionFinished(this);
        }

        /// <summary>
        /// Task that created (and "owns") this UFO
        /// </summary>
        public InvasionTask Task { get { return task; } }

        /// <remarks>UFOs don't have a base (yet), so calling this for a UFO is a mistake</remarks>
        public override HumanBase HomeBase 
        {
            get { throw new NotImplementedException(Strings.ResourceManager.GetString("EXCEPTION_UFOS_HAVE_NO_BASE")); }
            set { throw new NotImplementedException(Strings.ResourceManager.GetString("EXCEPTION_UFOS_HAVE_NO_BASE")); }
        }

        /// <remarks>UFOs don't have a base (yet), so calling this for a UFO is a mistake</remarks>
        public override bool InBase 
        {
            get { throw new NotImplementedException(Strings.ResourceManager.GetString("EXCEPTION_UFOS_HAVE_NO_BASE")); }
            set { throw new NotImplementedException(Strings.ResourceManager.GetString("EXCEPTION_UFOS_HAVE_NO_BASE")); } 
        }

        /// <summary>
        /// Player readable identifier for this UFO
        /// </summary>
        public override string Name { get { return "<Ufo ToDo>"; } }

        /// <summary>
        /// Task that created (and "owns") the UFO
        /// </summary>
        private InvasionTask task;
    }
}
