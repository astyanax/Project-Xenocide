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
* @file TaskFactory.cs
* @date Created: 2007/08/11
* @author File creator: David Teviotdale
* @author Credits: none
*/
#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

using ProjectXenocide.Model.Geoscape.Geography;
using ProjectXenocide.Model.Geoscape.Vehicles;
using ProjectXenocide.Model.StaticData.AI;

#endregion

namespace ProjectXenocide.Model.Geoscape.AI
{
    /// <summary>
    /// Entry in a MissionPlan
    /// </summary>
    [Serializable]
    public class LaunchPlan
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="ufoType">Type of UFO to launch</param>
        /// <param name="earliestLaunch">Min time, in hours, before UFO can be launched</param>
        /// <param name="latestLaunch">Max time, in hourse, befoure UFO must be launched</param>
        /// <param name="numLandings">Number times the UFO will land</param>
        /// <param name="numSublandings">Number of points the UFO will investigate between landings</param>
        public LaunchPlan(String ufoType, float earliestLaunch, float latestLaunch, int landings, int subLandings)
        {
            Debug.Assert(earliestLaunch <= latestLaunch);
            this.ufoType = ufoType;
            this.earliestLaunch = earliestLaunch;
            this.latestLaunch = latestLaunch;
            this.numLandings = landings;
            this.numSubLandings = subLandings;
        }

        /// <summary>
        /// Compute a random time between the ealiest and latest launch times.
        /// </summary>
        /// <returns>time to wait</returns>
        public TimeSpan CalculateLaunchDelay()
        {
            float minutes = earliestLaunch * 60.0f;
            minutes += ((latestLaunch - earliestLaunch) * 0.6f * Xenocide.Rng.Next(101));
            return new TimeSpan(0, (int)minutes, 0);
        }

        #region Fields

        /// <summary>
        /// Type of UFO to launch
        /// </summary>
        public String UfoType { get { return ufoType; } }

        /// <summary>
        /// Minimum delay (in hours) after the preceding launch before this UFO can launch.
        /// (Original X-COM: random delay between [earliest, latest] hours)
        /// </summary>
        public float EarliestHours { get { return earliestLaunch; } }

        /// <summary>
        /// Maximum delay (in hours) after the preceding launch before this UFO must launch.
        /// (Original X-COM: random delay between [earliest, latest] hours)
        /// </summary>
        public float LatestHours { get { return latestLaunch; } }

        /// <summary>
        /// Type of UFO to launch
        /// </summary>
        private String ufoType;

        /// <summary>
        /// Minimum time, in hours, Overmind must wait after preceeding launch before this
        /// UFO can be launched
        /// </summary>
        private float earliestLaunch;

        /// <summary>
        /// Maximum time, in hours, Overmind must wait after preceeding launch before this
        /// UFO can be launched
        /// </summary>
        private float latestLaunch;

        private int numLandings;
        /// <summary>
        /// How many times the ufo should land before it reaches it's final destination
        /// </summary>
        public int Landings { get { return numLandings; } }

        private int numSubLandings;
        /// <summary>
        /// How many point of interest (slower speed sub locations) should the UFO stop at before
        /// committing to the landing point
        /// </summary>
        public int SubLandings { get { return numSubLandings; } }

        #endregion Fields
    }

    /// <summary>
    /// The missions that make up a Task
    /// </summary>
    [Serializable]
    public class TaskPlan
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name of the Task</param>
        /// <param name="score">Points awarded for each UFO that survives it's mission</param>
        /// <param name="launches">The UFOs that will be used in the task</param>
        public TaskPlan(String name, float score, IList<LaunchPlan> launches)
        {
            this.name = name;
            this.score = score;
            this.launches = launches;
        }

        #region Fields

        /// <summary>
        /// Name of the Task
        /// </summary>
        public String Name { get { return name; } }

        /// <summary>
        /// Points awarded for each UFO that survives it's mission
        /// </summary>
        public float Score { get { return score; } }

        /// <summary>
        /// The UFOs that will be used in the task
        /// </summary>
        public IList<LaunchPlan> Launches { get { return launches; } }

        /// <summary>
        /// Name of the Task
        /// </summary>
        private String name;

        /// <summary>
        /// Points awarded for each UFO that survives it's mission
        /// </summary>
        private float score;

        /// <summary>
        /// The UFOs that will be used in the task
        /// </summary>
        private IList<LaunchPlan> launches;

        #endregion Fields
    }

    /// <summary>
    /// Creates the Overmind's tasks.
    ///
    /// NOTE: As of Phase 9.3 of the migration, the 8 mission launch
    /// sequences are no longer hardcoded in C#.  They are loaded from
    /// ufobehavior.xml by UfoBehaviorSettings, and this class simply
    /// delegates to that data source via GetPlan().
    /// </summary>
    [Serializable]
    public class TaskFactory
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <remarks>
        /// The original code called ConstructPlans() here to build the
        /// 8 hardcoded plans.  That has been replaced by XML-driven
        /// loading via StaticTables.UfoBehavior (see UfoBehaviorSettings).
        /// </remarks>
        public TaskFactory()
        {
        }

        /// <summary>
        /// Construct an InvasionTask for the specified type of missions
        /// </summary>
        /// <param name="type">Type of missions to engage in</param>
        /// <param name="overmind">Overmind that owns the task</param>
        /// <param name="region">Position on Geoscape that will be the center of the UFOs' activity</param>
        /// <returns>the task</returns>
        public InvasionTask Create(AlienMission type, Overmind overmind, PlanetRegion region)
        {
            Planet planet = Xenocide.GameState.GeoData.Planet;
            GeoPosition centroid = planet.GetRandomLandPositionInRegion(region);
            InvasionTask task = null;
            switch (type)
            {
                case AlienMission.Abduction:
                case AlienMission.Harvest:
                case AlienMission.Research:
                    // these are all same behaviour as research
                    task = CreateResearchTask(type, overmind, centroid);
                    break;

                case AlienMission.Infiltration:
                    task = CreateInfiltrationTask(overmind);
                    break;

                case AlienMission.Outpost:
                    task = CreateBuildOutpostTask(overmind, centroid);
                    break;

                case AlienMission.Retaliation:  // Use CreateRetaliationTask()
                case AlienMission.Terror:       // Use CreateTerrorTask()
                case AlienMission.Supply:       // Use CreateSupplyTask()
                default:
                    Debug.Assert(false);
                    break;
            }
            return task;
        }

        /// <summary>
        /// Construct a ResearchTask
        /// </summary>
        /// <param name="type">Type of missions to engage in</param>
        /// <param name="overmind">Overmind that owns the task</param>
        /// <param name="centroid">Position on Geoscape that will be the center of the UFOs' activity</param>
        /// <returns>the task</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Performance", "CA1822:MarkMembersAsStatic",
            Justification = "Kept as instance method for backward compat with " +
                            "existing callers (Overmind.taskFactory.X); the data is " +
                            "now loaded from XML and accessed via the static GetPlan().")]
        public ResearchTask CreateResearchTask(AlienMission type, Overmind overmind, GeoPosition centroid)
        {
            return new ResearchTask(overmind, centroid, GetPlan(type));
        }

        /// <summary>
        /// Construct an InfiltrationTask
        /// </summary>
        /// <param name="overmind">Overmind that owns the task</param>
        /// <returns>the task</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Performance", "CA1822:MarkMembersAsStatic",
            Justification = "Instance method to match existing public API; data is XML-driven.")]
        public InfiltrationTask CreateInfiltrationTask(Overmind overmind)
        {
            Planet planet = Xenocide.GameState.GeoData.Planet;
            Country country = planet.SelectCountryToInfiltrate();
            GeoPosition centroid = planet.GetRandomPositionInCountry(country);
            return new InfiltrationTask(overmind, centroid, GetPlan(AlienMission.Infiltration), country);
        }

        /// <summary>
        /// Construct InfiltrationTask as specified position
        /// </summary>
        /// <param name="overmind">Overmind that owns the task</param>
        /// <param name="position">Position to infiltrate</param>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Performance", "CA1822:MarkMembersAsStatic",
            Justification = "Instance method to match existing public API; data is XML-driven.")]
        public InfiltrationTask CreateInfiltrationTask(Overmind overmind, GeoPosition position)
        {
            Planet planet = Xenocide.GameState.GeoData.Planet;
            Country country = planet.GetCountryAtLocation(position);
            return new InfiltrationTask(overmind, position, GetPlan(AlienMission.Infiltration), country);
        }

        /// <summary>
        /// Construct a BuildOutputTask
        /// </summary>
        /// <param name="overmind">Overmind that owns the task</param>
        /// <param name="centroid">Where to build the outpost</param>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Performance", "CA1822:MarkMembersAsStatic",
            Justification = "Instance method to match existing public API; data is XML-driven.")]
        public BuildOutpostTask CreateBuildOutpostTask(Overmind overmind, GeoPosition centroid)
        {
            return new BuildOutpostTask(overmind, centroid, GetPlan(AlienMission.Outpost));
        }

        /// <summary>
        /// Construct a TerrorTask
        /// </summary>
        /// <param name="overmind">Overmind that owns the task</param>
        /// <returns>the task</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Performance", "CA1822:MarkMembersAsStatic",
            Justification = "Instance method to match existing public API; data is XML-driven.")]
        public TerrorTask CreateTerrorTask(Overmind overmind)
        {
            return new TerrorTask(overmind, GetPlan(AlienMission.Terror));
        }

        /// <summary>
        /// Construct a SupplyOutpostTask
        /// </summary>
        /// <param name="overmind">Overmind that owns the task</param>
        /// <param name="outpost">Outpost that is being supplied</param>
        /// <returns>the task</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Performance", "CA1822:MarkMembersAsStatic",
            Justification = "Instance method to match existing public API; data is XML-driven.")]
        public SupplyOutpostTask CreateSupplyTask(Overmind overmind, OutpostAlienSite outpost)
        {
            return new SupplyOutpostTask(overmind, outpost, GetPlan(AlienMission.Supply));
        }

        /// <summary>
        /// Construct a RetaliationTask
        /// </summary>
        /// <param name="overmind">Overmind that owns the task</param>
        /// <param name="searchStart">Where Overmind will start it's search for X-Corp outposts</param>
        /// <returns>the task</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Performance", "CA1822:MarkMembersAsStatic",
            Justification = "Instance method to match existing public API; data is XML-driven.")]
        public RetaliationTask CreateRetaliationTask(Overmind overmind, GeoPosition searchStart)
        {
            return new RetaliationTask(overmind, searchStart, GetPlan(AlienMission.Retaliation));
        }

        /// <summary>
        /// Look up the TaskPlan for a given alien mission type.
        /// Source of truth: ufobehavior.xml via UfoBehaviorSettings.
        /// </summary>
        private static TaskPlan GetPlan(AlienMission type)
        {
            return Xenocide.StaticTables.UfoBehavior.GetPlan(type);
        }
    }
}
