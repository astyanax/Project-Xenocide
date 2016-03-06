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
* @file Project.cs
* @date Created: 2007/09/30
* @author File creator: David Teviotdale
* @author Credits: none
*/
#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;


using ProjectXenocide.Utils;
using ProjectXenocide.Model.StaticData.Research;
using Xenocide.Resources;


#endregion

namespace ProjectXenocide.Model.Geoscape
{
    /// <summary>
    /// Base class for Research and Manufacturing projects
    /// Essentially, is responsible for personnel management
    /// </summary>
    [Serializable]
    public abstract class Project
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="workerTypeId">Type of worker needed for this project</param>
        /// <param name="hoursNeeded">Number of person hours needed by project</param>
        /// <param name="projectManager">the owner of this project</param>
        protected Project(string workerTypeId, int hoursNeeded, ProjectManager projectManager)
        {
            Debug.Assert(null != Xenocide.StaticTables.ItemList[workerTypeId]);
            this.workerTypeId   = workerTypeId;
            this.hoursNeeded    = hoursNeeded;
            this.projectManager = projectManager;
        }

        /// <summary>
        /// Add a person to the set of people working on this project
        /// </summary>
        /// <param name="worker">person to add</param>
        public void Add(Person worker)
        {
            Debug.Assert(!workers.Contains(worker), "Person already working on this project");
            Debug.Assert(workerTypeId == worker.ItemInfo.Id);
            Debug.Assert(!IsFinished);
            if (!IsFinished)
            {
                worker.IsWorking = true;
                workers.Add(worker);
                OnNumWorkersChanged();
            }
        }

        /// <summary>
        /// Remove a person from the set of people working on this project
        /// </summary>
        /// <param name="worker">person to remove</param>
        public void Remove(Person worker)
        {
            Debug.Assert(workers.Contains(worker), "Person not working on this project");
            worker.IsWorking = false;
            workers.Remove(worker);
            OnNumWorkersChanged();
        }

        /// <summary>
        /// Remove an arbitary person from the project
        /// </summary>
        /// <returns>person who has been removed</returns>
        public Person RemoveWorker()
        {
            Debug.Assert(0 < workers.Count);
            Person worker = workers[workers.Count - 1];
            Remove(worker);
            return worker;
        }

        /// <summary>
        /// Project is informed that an X-Corp outpost was destroyed
        /// So any people in the outpost are dead and can't be working on this project
        /// </summary>
        /// <param name="people">the people in the destroyed outpost</param>
        public void OnOutpostDestroyed(IEnumerable<Person> people)
        {
            // Need to bring progress up to date before deleting workers
            // so can allow for work they did prior to their deaths
            Update();

            foreach (Person person in people)
            {
                if (workers.Contains(person))
                {
                    Remove(person);
                }
            }
        }

        /// <summary>
        /// Cleanup the project at its conclusion
        /// </summary>
        public void Cleanup()
        {
            CancelAppointment();
            for (int i = workers.Count - 1; 0 <= i; --i)
            {
                Remove(workers[i]);
            }
            projectManager.Remove(this);
        }

        /// <summary>
        /// Called to inform project that time has passed
        /// </summary>
        /// <remarks>Intent is to allow project to update its state for display</remarks>
        public void Update()
        {
            TimeSpan span = Xenocide.GameState.GeoData.GeoTime.Time - lastUpdate;
            lastUpdate    = Xenocide.GameState.GeoData.GeoTime.Time;
            if (0 != span.TotalMilliseconds)
            {
                hoursWorked += (CalcTotalSkill() * span.TotalHours);
                if (IsFinished)
                {
                    OnFinish();
                }
            }
        }

        /// <summary>
        /// Calculate days left value to show to user
        /// </summary>
        /// <returns>Value formatted for display to user</returns>
        public String CalcEtaToShow()
        {
            return CalcEtaToShow(hoursNeeded);
        }

        /// <summary>
        /// Calculate days left value to show to user
        /// </summary>
        /// <returns>Value to show to user</returns>
        protected String CalcEtaToShow(double manHours)
        {
            if (0 == NumWorkers)
            {
                return Strings.ETA_NEVER;
            }
            else
            {
                return Util.ToString((int)CalcEta(manHours).TotalDays);
            }
        }

        /// <summary>
        /// Calculate how long to finish building current item (or research project)
        /// </summary>
        /// <returns>Time required</returns>
        public TimeSpan CalcEta()
        {
            return CalcEta(hoursNeeded);
        }

        /// <summary>
        /// Calculate how long to finish task taking X man hours
        /// </summary>
        /// <param name="manHours">total time needed for job</param>
        /// <returns>Time Neded</returns>
        protected TimeSpan CalcEta(double manHours)
        {
            int skill = CalcTotalSkill();
            Debug.Assert(0 < skill, "Can't calculate an ETA when there are no workers");
            double hours = (manHours - hoursWorked) / skill;
            return new TimeSpan((long)(hours * 36000000000L));
        }

        /// <summary>
        /// Called when the number of workers has changed
        /// </summary>
        protected void OnNumWorkersChanged()
        {
            // reschedule appointment
            CancelAppointment();
            if (!IsFinished && (0 < NumWorkers))
            {
                appointment = Xenocide.GameState.GeoData.GeoTime.MakeAppointment(CalcEta(), Update);
            }
        }

        /// <summary>
        /// Called when project finishes
        /// </summary>
        public abstract void OnFinish();

        /// <summary>
        /// Calculate the total "skill" level of the workers involved in project
        /// </summary>
        /// <returns>total skill level</returns>
        private int CalcTotalSkill()
        {
            int skill = 0;
            foreach (Person worker in workers)
            {
                skill += worker.PersonItemInfo.SkillLevel;
            }
            return skill;
        }

        /// <summary>
        /// If there's an appointment, cancel it
        /// </summary>
        protected void CancelAppointment()
        {
            if (null != appointment)
            {
                Xenocide.GameState.GeoData.GeoTime.Scheduler.Remove(appointment);
                appointment = null;
            }
        }

        #region Fields

        /// <summary>
        /// Internal code used inside Xenocide to refer to this Project
        /// </summary>
        public abstract string Id { get;  }

        /// <summary>
        /// Number of person hours that have been spent working on this project
        /// </summary>
        public double HoursWorked { get { return hoursWorked; } protected set { hoursWorked = value; } }

        /// <summary>
        /// Number of person hours needed by project
        /// </summary>
        public int HoursNeeded { get { return hoursNeeded; } }

        /// <summary>
        /// Number of people working on this project
        /// </summary>
        public int NumWorkers { get { return workers.Count; } }

        /// <summary>
        /// Is the project finished
        /// </summary>
        public bool IsFinished { get { return hoursNeeded <= hoursWorked; } }

        /// <summary>
        /// Type of worker needed for this project
        /// </summary>
        private string workerTypeId;

        /// <summary>
        /// The people working on this project
        /// </summary>
        private List<Person> workers = new List<Person>();

        /// <summary>
        /// GeoTime when Update() was last called on this class
        /// </summary>
        private DateTime lastUpdate = Xenocide.GameState.GeoData.GeoTime.Time;

        /// <summary>
        /// Number of person hours that have been spent working on this project
        /// </summary>
        private double hoursWorked;

        /// <summary>
        /// Number of person hours needed by project
        /// </summary>
        private int hoursNeeded;

        /// <summary>
        /// Appointment for when noticable progress will be made on the project
        /// </summary>
        private Appointment appointment;

        /// <summary>
        /// the owner of this project
        /// </summary>
        private ProjectManager projectManager;

        #endregion Fields
    }
}
