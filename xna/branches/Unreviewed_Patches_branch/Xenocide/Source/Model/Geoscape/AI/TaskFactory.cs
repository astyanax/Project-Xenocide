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
using System.Text;
using System.Diagnostics;

using ProjectXenocide.Model.Geoscape.Vehicles;
using ProjectXenocide.Model.Geoscape.Geography;
using Xenocide.Resources;

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
        public LaunchPlan(String ufoType, float earliestLaunch, float latestLaunch)
        {
            Debug.Assert(earliestLaunch <= latestLaunch);
            this.ufoType = ufoType;
            this.earliestLaunch = earliestLaunch;
            this.latestLaunch = latestLaunch;
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
            this.name     = name;
            this.score    = score;
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
    /// Creates the Overmind's tasks
    /// </summary>
    [Serializable]
    public class TaskFactory
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public TaskFactory()
        {
            ConstructPlans();
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
            Planet      planet   = Xenocide.GameState.GeoData.Planet;
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
        public ResearchTask CreateResearchTask(AlienMission type, Overmind overmind, GeoPosition centroid)
        {
            return new ResearchTask(overmind, centroid, plans[(int)type]);
        }

        /// <summary>
        /// Construct an InfiltrationTask
        /// </summary>
        /// <param name="overmind">Overmind that owns the task</param>
        /// <returns>the task</returns>
        public InfiltrationTask CreateInfiltrationTask(Overmind overmind)
        {
            Planet      planet   = Xenocide.GameState.GeoData.Planet;
            Country     country  = planet.SelectCountryToInfiltrate();
            GeoPosition centroid = planet.GetRandomPositionInCountry(country);
            return new InfiltrationTask(overmind, centroid, plans[(int)AlienMission.Infiltration], country);
        }

        /// <summary>
        /// Construct InfiltrationTask as specified position
        /// </summary>
        /// <param name="overmind">Overmind that owns the task</param>
        /// <param name="position">Position to infiltrate</param>
        /// <returns></returns>
        public InfiltrationTask CreateInfiltrationTask(Overmind overmind, GeoPosition position)
        {
            Planet planet = Xenocide.GameState.GeoData.Planet;
            Country country = planet.GetCountryAtLocation(position);
            return new InfiltrationTask(overmind, position, plans[(int)AlienMission.Infiltration], country);
        }

        /// <summary>
        /// Construct a BuildOutputTask
        /// </summary>
        /// <param name="overmind">Overmind that owns the task</param>
        /// <param name="centroid">Where to build the outpost</param>
        /// <returns></returns>
        public BuildOutpostTask CreateBuildOutpostTask(Overmind overmind, GeoPosition centroid)
        {
            return new BuildOutpostTask(overmind, centroid, plans[(int)AlienMission.Outpost]);
        }

        /// <summary>
        /// Construct a TerrorTask
        /// </summary>
        /// <param name="overmind">Overmind that owns the task</param>
        /// <returns>the task</returns>
        public TerrorTask CreateTerrorTask(Overmind overmind)
        {
            return new TerrorTask(overmind, plans[(int)AlienMission.Terror]);
        }

        /// <summary>
        /// Construct a SupplyOutpostTask
        /// </summary>
        /// <param name="overmind">Overmind that owns the task</param>
        /// <param name="outpost">Outpost that is being supplied</param>
        /// <returns>the task</returns>
        public SupplyOutpostTask CreateSupplyTask(Overmind overmind, OutpostAlienSite outpost)
        {
            return new SupplyOutpostTask(overmind, outpost, plans[(int)AlienMission.Supply]);
        }

        /// <summary>
        /// Construct a RetaliationTask
        /// </summary>
        /// <param name="overmind">Overmind that owns the task</param>
        /// <param name="searchStart">Where Overmind will start it's search for X-Corp outposts</param>
        /// <returns>the task</returns>
        public RetaliationTask CreateRetaliationTask(Overmind overmind, GeoPosition searchStart)
        {
            return new RetaliationTask(overmind, searchStart, plans[(int)AlienMission.Retaliation]);
        }

        /// <summary>
        /// Construct the set of missions that make up a Task
        /// </summary>
        private void ConstructPlans()
        {
            plans = new List<TaskPlan>();

            // Research
            List<LaunchPlan> launches = new List<LaunchPlan>();
            launches.Add(new LaunchPlan("ITEM_UFO_PROBE",   20.0f,  30.0f));
            launches.Add(new LaunchPlan("ITEM_UFO_RECON",  132.0f, 204.0f));
            launches.Add(new LaunchPlan("ITEM_UFO_ESCORT", 132.0f, 204.0f));
            launches.Add(new LaunchPlan("ITEM_UFO_ESCORT", 48.0f, 120.0f));
            plans.Add(new TaskPlan(Strings.UFO_MISSION_RESEARCH, 20.0f, launches));

            // Harvest
            launches = new List<LaunchPlan>();
            launches.Add(new LaunchPlan("ITEM_UFO_PROBE",       48.0f, 120.0f));
            launches.Add(new LaunchPlan("ITEM_UFO_PROBE",       48.0f, 120.0f));
            launches.Add(new LaunchPlan("ITEM_UFO_PROBE",      132.0f, 204.0f));
            launches.Add(new LaunchPlan("ITEM_UFO_ESCORT",      48.0f, 120.0f));
            launches.Add(new LaunchPlan("ITEM_UFO_ESCORT",      48.0f, 120.0f));
            launches.Add(new LaunchPlan("ITEM_UFO_REAPER",     132.0f, 204.0f));
            launches.Add(new LaunchPlan("ITEM_UFO_REAPER",      48.0f, 120.0f));
            launches.Add(new LaunchPlan("ITEM_UFO_JUGGERNAUT",  20.0f,  30.0f));
            plans.Add(new TaskPlan(Strings.UFO_MISSION_HARVEST, 30.0f, launches));

            // Abduction
            launches = new List<LaunchPlan>();
            launches.Add(new LaunchPlan("ITEM_UFO_PROBE",      132.0f, 204.0f));
            launches.Add(new LaunchPlan("ITEM_UFO_RECON",      132.0f, 204.0f));
            launches.Add(new LaunchPlan("ITEM_UFO_ESCORT",     240.0f, 432.0f));
            launches.Add(new LaunchPlan("ITEM_UFO_COLLECTOR",   48.0f, 120.0f));
            launches.Add(new LaunchPlan("ITEM_UFO_COLLECTOR",   48.0f, 120.0f));
            launches.Add(new LaunchPlan("ITEM_UFO_JUGGERNAUT",   0.5f,   2.5f));
            plans.Add(new TaskPlan(Strings.UFO_MISSION_ABDUCTION, 50.0f, launches));

            // Infiltration
            launches = new List<LaunchPlan>();
            launches.Add(new LaunchPlan("ITEM_UFO_PROBE",         240.0f, 432.0f));
            launches.Add(new LaunchPlan("ITEM_UFO_RECON",         240.0f, 432.0f));
            launches.Add(new LaunchPlan("ITEM_UFO_RECON",         240.0f, 432.0f));
            launches.Add(new LaunchPlan("ITEM_UFO_ESCORT",        240.0f, 432.0f));
            launches.Add(new LaunchPlan("ITEM_UFO_ESCORT",         48.0f, 120.0f));
            launches.Add(new LaunchPlan("ITEM_UFO_INTIMIDATOR",     0.5f,   2.5f));
            launches.Add(new LaunchPlan("ITEM_UFO_INTIMIDATOR",     0.5f,   2.5f));
            launches.Add(new LaunchPlan("ITEM_UFO_ALIEN_FREIGHTER", 0.5f,   2.5f));
            launches.Add(new LaunchPlan("ITEM_UFO_JUGGERNAUT",      0.5f,   2.5f));
            plans.Add(new TaskPlan(Strings.UFO_MISSION_INFILTRATION, 150.0f, launches));

            // Outpost
            launches = new List<LaunchPlan>();
            launches.Add(new LaunchPlan("ITEM_UFO_PROBE",           132.0f, 204.0f));
            launches.Add(new LaunchPlan("ITEM_UFO_RECON",           132.0f, 204.0f));
            launches.Add(new LaunchPlan("ITEM_UFO_ESCORT",          132.0f, 204.0f));
            launches.Add(new LaunchPlan("ITEM_UFO_ALIEN_FREIGHTER",   0.5f,   2.5f));
            launches.Add(new LaunchPlan("ITEM_UFO_ALIEN_FREIGHTER",   0.5f,   2.5f));
            launches.Add(new LaunchPlan("ITEM_UFO_JUGGERNAUT",        0.5f,   2.5f));
            plans.Add(new TaskPlan(Strings.UFO_MISSION_OUTPOST, 50.0f, launches));

            // Terror (note, no score for the UFO, it's the terror site that earns the points)
            launches = new List<LaunchPlan>();
            launches.Add(new LaunchPlan("ITEM_UFO_RECON",         2.5f,   2.5f));
            launches.Add(new LaunchPlan("ITEM_UFO_ESCORT",      132.0f, 204.0f));
            launches.Add(new LaunchPlan("ITEM_UFO_INTIMIDATOR", 132.0f, 204.0f));
            launches.Add(new LaunchPlan("ITEM_UFO_INTIMIDATOR", 132.0f, 204.0f));
            plans.Add(new TaskPlan(Strings.UFO_MISSION_TERROR, 0.0f, launches));

            // Retaliation
            launches = new List<LaunchPlan>();
            launches.Add(new LaunchPlan("ITEM_UFO_PROBE",      50.0f,  50.0f));
            launches.Add(new LaunchPlan("ITEM_UFO_RECON",      32.0f,  72.0f));
            launches.Add(new LaunchPlan("ITEM_UFO_RECON",      48.0f, 120.0f));
            launches.Add(new LaunchPlan("ITEM_UFO_ESCORT",     48.0f, 120.0f));
            launches.Add(new LaunchPlan("ITEM_UFO_ESCORT",     48.0f, 120.0f));
            launches.Add(new LaunchPlan("ITEM_UFO_ESCORT",     48.0f, 120.0f));
            launches.Add(new LaunchPlan("ITEM_UFO_JUGGERNAUT", 48.0f, 120.0f));
            launches.Add(new LaunchPlan("ITEM_UFO_JUGGERNAUT", 48.0f, 120.0f));
            plans.Add(new TaskPlan(Strings.UFO_MISSION_RETALIATION, 0.0f, launches));

            // Supply
            launches = new List<LaunchPlan>();
            launches.Add(new LaunchPlan("ITEM_UFO_ALIEN_FREIGHTER", 240.0f, 432.0f));
            launches.Add(new LaunchPlan("ITEM_UFO_INTIMIDATOR",     240.0f, 432.0f));
            launches.Add(new LaunchPlan("ITEM_UFO_JUGGERNAUT",      240.0f, 432.0f));
            plans.Add(new TaskPlan(Strings.UFO_MISSION_SUPPLY, 3.0f, launches));
        }

        #region Fields

        private List<TaskPlan> plans;

        #endregion Fields

    }
}
