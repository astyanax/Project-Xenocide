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
* @file InvasionTask.cs
* @date Created: 2007/02/11
* @author File creator: dteviot
* @author Credits: none
*/
#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.Text;

using Xenocide.Model.Geoscape.Craft;

#endregion

namespace Xenocide.Model.Geoscape.AI
{
    /// <summary>
    /// Base class for an alien activity in the Geoscape
    /// (Usually involves multiple UFOs)
    /// </summary>
    [Serializable]
    abstract public class InvasionTask
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="overmind">Overmind that owns this task</param>
        protected InvasionTask(Overmind overmind)
        {
            this.overmind = overmind;
        }

        /// <summary>
        /// Update the alien activity, to allow for passage of time
        /// </summary>
        /// <param name="milliseconds">The amount of time that has passed</param>
        /// <remarks>At moment, just pump the Ufo.</remarks>
        virtual public void Update(double milliseconds)
        {
            // we need to do this backwards, because we may add or remove UFOs
            // during an update cycle (and we don't want to update any UFOs we've just
            // added
            for (int i = Ufos.Count - 1; 0 <= i; --i)
            {
                Ufos[i].Update(milliseconds);
            }
        }

        /// <summary>
        /// Called when UFO has finished the mission the task set it
        /// </summary>
        /// <param name="ufo">UFO that has finished the mission</param>
        abstract public void OnMissionFinished(Ufo ufo);

        /// <summary>
        /// Called when a UFO has been destroyed (by enemy action)
        /// </summary>
        /// <param name="ufo">UFO that was destroyed</param>
        virtual public void OnUfoDestroyed(Ufo ufo)
        {
        }

        /// <summary>
        /// Called when a UFO has crash landed (due to enemy action)
        /// </summary>
        /// <param name="ufo">UFO that crash landed</param>
        virtual public void OnUfoCrashed(Ufo ufo)
        {
        }

        /// <summary>
        /// Assign another UFO to this task
        /// </summary>
        /// <param name="ufo">Ufo to assign</param>
        public void AddUfo(Ufo ufo)
        {
            Ufos.Add(ufo);
            Overmind.Ufos.Add(ufo);
        }

        /// <summary>
        /// Remove UFO from this task
        /// </summary>
        /// <param name="ufo">Ufo to remove</param>
        public void RemoveUfo(Ufo ufo)
        {
            Overmind.Ufos.Remove(ufo);
            Ufos.Remove(ufo);
        }

        /// <summary>
        /// The UFOs assigned to this task.
        /// </summary>
        protected IList<Ufo> Ufos { get { return ufos; } }

        /// <summary>
        /// Overmind that owns this task
        /// </summary>
        protected Overmind Overmind { get { return overmind; } }
        
        /// <summary>
        /// The UFOs assigned to this task.
        /// </summary>
        private List<Ufo> ufos = new List<Ufo>();

        /// <summary>
        /// Overmind that owns this task
        /// </summary>
        private Overmind overmind;
    }
}
