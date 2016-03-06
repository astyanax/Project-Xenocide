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
* @author File creator: David Teviotdale
* @author Credits: none
*/
#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

using ProjectXenocide.Model.Geoscape.Vehicles;
using ProjectXenocide.Utils;
using ProjectXenocide.Model.StaticData;

#endregion

namespace ProjectXenocide.Model.Geoscape.AI
{
    /// <summary>
    /// Base class for an alien activity in the Geoscape
    /// (Usually involves multiple UFOs)
    /// </summary>
    [Serializable]
    abstract public partial class InvasionTask
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="overmind">Overmind that owns this task</param>
        /// <param name="centroid">Position on Geoscape that will be the center of the UFOs' activity</param>
        /// <param name="taskPlan">The Missions this task requires</param>
        protected InvasionTask(Overmind overmind, GeoPosition centroid, TaskPlan taskPlan)
        {
            this.overmind = overmind;
            this.centroid = centroid;
            this.taskPlan = taskPlan;
            MakeLaunchAppointment();
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
        /// Called when UFO has finished and survived the mission the task set it
        /// </summary>
        /// <param name="ufo">UFO that has finished the mission</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
            Justification = "will throw if ufo is null")]
        virtual public void OnMissionFinished(Ufo ufo)
        {
            // If UFO succeeded in it's part, allocate points
            if (ufo.Mission.Success)
            {
                Xenocide.GameState.GeoData.AddScore(Participant.Alien, taskPlan.Score, centroid);
            }

            // we're done with the UFO
            RemoveUfo(ufo);
        }

        /// <summary>
        /// Called when a UFO has been destroyed (by enemy action)
        /// </summary>
        /// <param name="ufo">UFO that was destroyed</param>
        virtual public void OnUfoDestroyed(Ufo ufo)
        {
            // tell the overmind the craft was destroyed, in case it wants to do anything
            overmind.OnUfoDestroyed(ufo);

            // we're done with the UFO
            RemoveUfo(ufo);
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
            Overmind.Add(ufo);
        }

        /// <summary>
        /// Remove UFO from this task
        /// </summary>
        /// <param name="ufo">Ufo to remove</param>
        public void RemoveUfo(Ufo ufo)
        {
            Overmind.Remove(ufo);
            Ufos.Remove(ufo);

            // if there are no more UFOs associated with this task, then its finished
            if ((0 == Ufos.Count) && !StillUfosToLaunch)
            {
                OnTaskFinished(ufo);
            }
        }

        /// <summary>
        /// The last UFO involved in the task has finished or been destroyed
        /// </summary>
        /// <param name="ufo">the last UFO</param>
        public virtual void OnTaskFinished(Ufo ufo)
        {
            overmind.OnTaskFinished(this);
        }

        /// <summary>
        /// Set up an Appointment for time to launch next UFO
        /// </summary>
        public void MakeLaunchAppointment()
        {
            Debug.Assert(nextUfoToLaunch < taskPlan.Launches.Count);
            TimeSpan launchDelay = taskPlan.Launches[nextUfoToLaunch].CalculateLaunchDelay();
            appointment = Xenocide.GameState.GeoData.GeoTime.MakeAppointment(launchDelay, LaunchUfo);
        }

        /// <summary>
        /// Set up an Appointment for time to launch next UFO
        /// </summary>
        public virtual void LaunchUfo()
        {
            // appoinment has been used. Get rid of it
            appointment = null;

            // Pick the target for this UFO
            GeoPosition target = SelectFirstLandingSite();

            // start location is 1/4 world away from start
            GeoPosition start = target.RandomLocation(Math.PI * 0.5);

            // construct the UFO
            Ufo ufo = new Ufo(taskPlan.Launches[nextUfoToLaunch].UfoType, start, this);
            
            // assign UFO appropriate mission
            ++nextUfoToLaunch;
            GiveMission(ufo);

            // track it.  And prepare for next launch
            AddUfo(ufo);
            if (StillUfosToLaunch)
            {
                MakeLaunchAppointment();
            }
        }

        /// <summary>
        /// Start launches again, from start
        /// </summary>
        public void ResetLaunchCounter()
        {
            nextUfoToLaunch = 0;
        }

        /// <summary>
        /// Cancel the task, in whatever state it currently is in
        /// </summary>
        public virtual void Abort()
        {
            if (null != appointment)
            {
                Xenocide.GameState.GeoData.GeoTime.Scheduler.Remove(appointment);
                appointment = null;
            }
            for (int i = Ufos.Count - 1; 0 <= i; --i)
            {
                Overmind.Remove(Ufos[i]);
                Ufos.RemoveAt(i);
            }
        }

        /// <summary>
        /// Choose first landing site for the UFO
        /// </summary>
        /// <returns>landing site's GeoPosition</returns>
        protected virtual GeoPosition SelectFirstLandingSite()
        {
            // default is return the centroid
            return Centroid;
        }

        /// <summary>
        /// Give a UFO a mission
        /// </summary>
        /// <param name="ufo">Ufo to give mission to</param>
        protected abstract void GiveMission(Ufo ufo);

        /// <summary>
        /// Does the task have more UFOs to launch
        /// </summary>
        /// <returns>true if there are still UFOs that need to be launched</returns>
        public bool StillUfosToLaunch { get { return (nextUfoToLaunch < taskPlan.Launches.Count); } }

        #region Fields

        /// <summary>
        /// Position on Geoscape that will be the center of the UFO's activity
        /// </summary>
        public GeoPosition Centroid { get { return centroid; } protected set { centroid = value; } }

        /// <summary>
        /// Name for this Task (called Mission on UI)
        /// </summary>
        public String Name { get { return taskPlan.Name; } }

        /// <summary>
        /// The UFOs assigned to this task.
        /// </summary>
        protected IList<Ufo> Ufos { get { return ufos; } }

        /// <summary>
        /// Overmind that owns this task
        /// </summary>
        protected Overmind Overmind { get { return overmind; } }

        /// <summary>
        /// The Missions this task requires
        /// </summary>
        protected TaskPlan TaskPlan { get { return taskPlan; } }

        /// <summary>
        /// Which UFO in the task plan is the next one we will launch?
        /// </summary>
        protected int NextUfoToLaunch { get { return nextUfoToLaunch; } }

        /// <summary>
        /// Appointment for the next UFO to launch
        /// </summary>
        protected Appointment Appointment { get { return appointment; } }

        /// <summary>
        /// The UFOs assigned to this task.
        /// </summary>
        private List<Ufo> ufos = new List<Ufo>();

        /// <summary>
        /// Overmind that owns this task
        /// </summary>
        private Overmind overmind;

        /// <summary>
        /// Position on Geoscape that will be the center of the UFOs' activity
        /// </summary>
        private GeoPosition centroid;

        /// <summary>
        /// The Missions this task requires
        /// </summary>
        private TaskPlan taskPlan;

        /// <summary>
        /// Which UFO in the task plan is the next one we will launch?
        /// </summary>
        private int nextUfoToLaunch;

        /// <summary>
        /// Appointment for the next UFO to launch
        /// </summary>
        private Appointment appointment;

        #endregion Fields
    }
}
