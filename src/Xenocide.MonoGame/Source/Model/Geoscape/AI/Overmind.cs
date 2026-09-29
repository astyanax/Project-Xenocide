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
* @author File creator: David Teviotdale
* @author Credits: none
*/
#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

using NLog;

using ProjectXenocide.Model.Geoscape.Geography;
using ProjectXenocide.Model.Geoscape.Vehicles;
using ProjectXenocide.Model.StaticData;

#endregion

namespace ProjectXenocide.Model.Geoscape.AI
{
    /// <summary>
    /// This is the "Alien Overmind" which does the highest level strategic planning
    /// At moment, is pretty dumb, will just spawn a ResearchTask (as it's the only one
    /// it knows.)  Later will get more tasks, and will span intelligently
    /// </summary>
    [Serializable]
    public class Overmind
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        /// <summary>How close (km) a clicked position must be to a site/city to target it directly.</summary>
        private const double DebugTargetNearKm = 500.0;

        /// <summary>
        /// Default constructor
        /// </summary>
        public Overmind()
        {
        }

        /// <summary>
        /// Do start of month processing
        /// </summary>
        public void StartOfMonth()
        {
            if (!diableStartOfMonth)
            {
                // select region and mission to focus on, and assign to a Task
                PlanetRegion region = Xenocide.GameState.GeoData.Planet.SelectRandomRegion();
                AlienMission missiontype = region.SelectRandomMission();
                AddTask(taskFactory.Create(missiontype, this, region));
            }
        }

        /// <summary>
        /// Overmind starts by sending research missions to location of X-Corp base
        /// </summary>
        /// <param name="centroid">location of X-Corp base</param>
        public void BeginFirstMissions(GeoPosition centroid)
        {
            // X-Corp base
            AddTask(taskFactory.CreateResearchTask(AlienMission.Research, this, centroid));

            // Start terror missions
            AddTask(taskFactory.CreateTerrorTask(this));
        }

        /// <summary>
        /// Update the alien activity, to allow for passage of time
        /// </summary>
        /// <param name="milliseconds">The amount of time that has passed</param>
        /// <remarks>At moment, just pump the tasks</remarks>
        public void Update(double milliseconds)
        {
            // can't use foreach, because tasks may be removed from collection
            for (int i = tasks.Count - 1; 0 <= i; --i)
            {
                tasks[i].Update(milliseconds);
            }
        }

        /// <summary>
        /// Handle an Invasion Task finishing
        /// </summary>
        /// <param name="task">task that finished</param>
        public void OnTaskFinished(InvasionTask task)
        {
            RemoveTask(task);
        }

        /// <summary>
        /// Add a site to the list of alien outposts and terror sites
        /// </summary>
        /// <param name="site">Site to add</param>
        public void AddSite(AlienSite site)
        {
            sites.Add(site);
        }

        /// <summary>
        /// Remove a site from the list of alien outposts and terror sites
        /// </summary>
        /// <param name="site">Site to remove</param>
        public void RemoveSite(AlienSite site)
        {
            sites.Remove(site);
        }

        /// <summary>
        /// Add an invasion task to the list of tasks
        /// </summary>
        /// <param name="task">InvasionTask to add</param>
        public void AddTask(InvasionTask task)
        {
            tasks.Add(task);
        }

        /// <summary>
        /// Remove an invasion task to the list of tasks
        /// </summary>
        /// <param name="task">InvasionTask to remove</param>
        public void RemoveTask(InvasionTask task)
        {
            tasks.Remove(task);
        }

        /// <summary>
        /// Add a UFO to the list of active UFOs
        /// </summary>
        /// <param name="ufo">UFO to add</param>
        public void Add(Ufo ufo)
        {
            Debug.Assert(!ufos.Contains(ufo), "UFO already in ufos");
            ufos.Add(ufo);
        }

        /// <summary>
        /// Remove a UFO from the list of active UFOs
        /// </summary>
        /// <param name="ufo">UFO to remove</param>
        public void Remove(Ufo ufo)
        {
            ufos.Remove(ufo);
        }

        /// <summary>
        /// Called when a UFO has been destroyed (by enemy action)
        /// </summary>
        /// <param name="ufo">UFO that was destroyed</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
            Justification = "will throw if ufo is null")]
        public void OnUfoDestroyed(Craft ufo)
        {
            // if we're running unit tests, we don't want extra tasks spawned
            if (!diableStartOfMonth)
            {
                // see if we spawn a retaliation task
                if (Xenocide.Rng.RollDice(StartSettings.RetaliationPercentage))
                {
                    AddTask(TaskFactory.CreateRetaliationTask(this, ufo.Position));
                }
            }
        }

        /// <summary>
        /// For debugging usage, create a mission of the requested type targeting
        /// (or near) the specified position.  If the clicked position is not a
        /// valid target, a random valid target is chosen; if none exists the
        /// mission is refused.
        /// </summary>
        /// <param name="missionType">Type of mission to create</param>
        /// <param name="position">Position clicked on the geoscape</param>
        /// <returns>true if a task was created</returns>
        public bool DebugCreateMission(AlienMission missionType, GeoPosition position)
        {
            Planet planet = Xenocide.GameState.GeoData.Planet;
            InvasionTask task = null;
            string refusal = null;

            switch (missionType)
            {
                case AlienMission.Research:
                case AlienMission.Harvest:
                case AlienMission.Abduction:
                    // These all behave like a research mission over land.
                    task = taskFactory.CreateResearchTask(missionType, this, planet.GetClosestLand(position));
                    break;

                case AlienMission.Infiltration:
                    {
                        Country country = planet.GetCountryAtLocation(position);
                        if (country != null)
                        {
                            // clicked inside a country: infiltrate that one
                            task = taskFactory.CreateInfiltrationTask(this, position);
                        }
                        else
                        {
                            // fall back to a random country
                            country = planet.SelectCountryToInfiltrate();
                            if (country == null)
                            {
                                refusal = "no countries to infiltrate";
                                break;
                            }
                            task = taskFactory.CreateInfiltrationTask(this, planet.GetRandomPositionInCountry(country));
                        }
                        break;
                    }

                case AlienMission.Outpost:
                    task = taskFactory.CreateBuildOutpostTask(this, planet.GetClosestLand(position));
                    break;

                case AlienMission.Retaliation:
                    if ((Xenocide.GameState.GeoData.Outposts == null) || (Xenocide.GameState.GeoData.Outposts.Count == 0))
                    {
                        refusal = "X-Corp has no outposts to retaliate against";
                        break;
                    }
                    task = taskFactory.CreateRetaliationTask(this, position);
                    break;

                case AlienMission.Supply:
                    {
                        OutpostAlienSite outpost = PickTarget(
                            Sites.OfType<OutpostAlienSite>(), position, site => site.Position);
                        if (outpost == null)
                        {
                            refusal = "no alien outposts to resupply";
                            break;
                        }
                        task = taskFactory.CreateSupplyTask(this, outpost);
                        break;
                    }

                case AlienMission.Terror:
                    {
                        City city = PickTarget(planet.AllCities, position, c => c.Position);
                        if (city == null)
                        {
                            refusal = "no cities to terrorise";
                            break;
                        }
                        task = taskFactory.CreateTerrorTask(this, city);
                        break;
                    }

                default:
                    refusal = "unsupported mission type";
                    break;
            }

            if (task == null)
            {
                Log.Info("DebugCreateMission({0}) refused: {1}", missionType, refusal ?? "no valid target");
                return false;
            }

            AddTask(task);
            Log.Info("DebugCreateMission({0}) -> {1}", missionType, task.GetType().Name);
            return true;
        }

        /// <summary>
        /// Pick the valid target nearest the clicked position, or a random one if
        /// none is within range (so a stray click still produces a mission).
        /// </summary>
        private static T PickTarget<T>(IEnumerable<T> candidates, GeoPosition position, Func<T, GeoPosition> location)
        {
            List<T> list = candidates.ToList();
            if (list.Count == 0)
            {
                return default;
            }

            T nearest = list[0];
            double nearestDistance = double.MaxValue;
            foreach (T candidate in list)
            {
                double distance = position.Distance(location(candidate));
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearest = candidate;
                }
            }

            if (nearestDistance <= GeoPosition.KilometersToRadians(DebugTargetNearKm))
            {
                return nearest;
            }
            return list[Xenocide.Rng.Next(list.Count)];
        }

        /// <summary>
        /// Debugging flag.  Turn of launching missions at start of month.
        /// </summary>
        public void DiableStartOfMonth() { diableStartOfMonth = true; }

        #region Fields

        /// <summary>
        /// Locations on the globe where the aliens are active (bases and terror sites)
        /// </summary>
        public IList<AlienSite> Sites { get { return sites; } }

        /// <summary>
        /// The tasks the Overmind currently has running
        /// </summary>
        /// <remarks>Treat this as READ ONLY</remarks>
        public IList<InvasionTask> Tasks { get { return tasks; } }

        /// <summary>
        /// Creates the tasks
        /// </summary>
        public TaskFactory TaskFactory { get { return taskFactory; } }

        /// <summary>
        /// Used to assign each UFO a unique name of form UFO-{x}, where x is number for UFO
        /// </summary>
        public int NextUfoCounter { get { return ++nextUfoCounter; } }

        /// <summary>
        /// Used to pick the race that will crew a UFO
        /// </summary>
        public RaceSelector RaceSelector { get { return raceSelector; } }

        /// <summary>
        /// Used to assign each outpost a unique name of form Alien outpost-{x}, 
        /// where x is number for Outpost
        /// </summary>
        public int NextOutpostCounter { get { return ++nextOutpostCounter; } }

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

        /// <summary>
        /// Locations on the globe where the aliens are active (outposts and terror sites)
        /// </summary>
        private List<AlienSite> sites = new List<AlienSite>();

        /// <summary>
        /// Creates the tasks
        /// </summary>
        private TaskFactory taskFactory = new TaskFactory();

        /// <summary>
        /// Debugging flag.  Turn of launching missions at start of month.
        /// </summary>
        private bool diableStartOfMonth;

        /// <summary>
        /// Used to assign each UFO a unique name of form UFO-{x}, where x is number for UFO
        /// </summary>
        private int nextUfoCounter;

        /// <summary>
        /// Used to pick the race that will crew a UFO
        /// </summary>
        private RaceSelector raceSelector = new RaceSelector();

        /// <summary>
        /// Used to assign each Alien Outpost a unique name of form Alien Outpost-{x},
        /// where x is number for outpost
        /// </summary>
        private int nextOutpostCounter;

        #endregion Fields
    }
}
