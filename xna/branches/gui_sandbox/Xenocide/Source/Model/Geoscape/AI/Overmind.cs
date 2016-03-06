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
* @file Overmind.cs
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
    /// This is the "Alien Overmind" which does the highest level strategic planning
    /// At moment, is pretty dumb, will just spawn a ResearchTask (as it's the only one
    /// it knows.)  Later will get more tasks, and will span intelligently
    /// </summary>
    [Serializable]
    public class Overmind
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public Overmind()
        {
        }

        /// <summary>
        /// Set to "start of new game" condition
        /// </summary>
        public void SetToStartGameCondition()
        {
            Ufos.Clear();
            tasks.Clear();
            tasks.Add(new ResearchTask(this));
        }

        /// <summary>
        /// Update the alien activity, to allow for passage of time
        /// </summary>
        /// <param name="milliseconds">The amount of time that has passed</param>
        /// <remarks>At moment, just pump the tasks.</remarks>
        public void Update(double milliseconds)
        {
            // can't use foreach, because tasks may be removed from collection
            for (int i = tasks.Count - 1; 0 <= i; --i)
            {
                tasks[i].Update(milliseconds);
            }
        }

        /// <summary>
        /// The tasks the Overmind currently has running
        /// </summary>
        private List<InvasionTask> tasks = new List<InvasionTask>();

        /// <summary>
        /// For convenience, keep a list of all active UFOs
        /// </summary>
        public IList<Ufo> Ufos { get { return ufos; } }

        /// <summary>
        /// For convenience, keep a list of all active UFOs
        /// </summary>
        private List<Ufo> ufos = new List<Ufo>();

    }
}
