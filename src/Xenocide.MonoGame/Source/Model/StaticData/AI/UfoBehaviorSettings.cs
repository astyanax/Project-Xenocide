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
#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.XPath;

using ProjectXenocide.Model.Geoscape;
using ProjectXenocide.Model.Geoscape.AI;
using ProjectXenocide.Utils;

using Xenocide.Resources;

#endregion

namespace ProjectXenocide.Model.StaticData.AI
{
    /// <summary>
    /// Loads UFO behavior configuration from ufobehavior.xml.
    ///
    /// Two categories of settings are loaded:
    ///  - Timing constants (Phase 9.4): the hardcoded duration/range values
    ///    scattered across mission and task classes (UfoMission, TerrorMission,
    ///    TerrorMissionAlienSite, RetaliationTask, Aircraft).
    ///  - Mission launch plans (Phase 9.3): the 8 task plan sequences that
    ///    used to be hardcoded in TaskFactory.ConstructPlans().
    ///
    /// Source: docs/legacy/design/UfoBehaviour.html
    ///
    /// Design notes:
    ///  - This is a singleton-style static-loaded service (lives on
    ///    StaticTables.UfoBehavior), matching the project's existing pattern
    ///    for static game data (ResearchGraph, ItemCollection, etc.).
    ///  - All values in the shipped XML match the original game's hardcoded
    ///    values, so behavior is unchanged from the legacy XNA build.
    ///  - LaunchPlan.CalculateLaunchDelay() preserves the original X-COM
    ///    launch behavior (random delay within the [earliest, latest] range).
    /// </summary>
    public sealed class UfoBehaviorSettings
    {
        /// <summary>
        /// Load configuration from the specified XML file.
        /// </summary>
        /// <param name="filename">Full path to ufobehavior.xml</param>
        public void Populate(string filename)
        {
            // XML namespace must match ufobehavior.xsd targetNamespace.
            const string xmlns = "UfoBehaviorConfig";
            XPathNavigator nav = Util.MakeValidatingXPathNavigator(filename, xmlns);
            XmlNamespaceManager manager = new XmlNamespaceManager(nav.NameTable);
            manager.AddNamespace("u", xmlns);

            LoadTimingConstants(nav, manager);
            LoadMissionPlans(nav, manager);
        }

        /// <summary>
        /// Read the <timingConstants> block and populate the corresponding
        /// TimeSpan / float properties.  These replace the magic numbers that
        /// were previously hardcoded in mission/site/task classes.
        /// </summary>
        private void LoadTimingConstants(XPathNavigator nav, XmlNamespaceManager manager)
        {
            XPathNavigator root = nav.SelectSingleNode("/u:ufobehavior/u:timingConstants", manager);

            // crashed UFO remains on ground 12h in the legacy code.
            // Legacy design doc (UfoBehaviour.html:81) says "1 to 4 days";
            // we keep 12h for backward compatibility with the original game.
            crashSiteDuration = TimeSpan.FromHours(
                GetDoubleAttr(root, manager, "u:crashSiteDuration", "hours"));

            // UFO at sampling site stays 2h (CalcSecondsOnGround).
            // Legacy design doc says 4-12h; we keep 2h.
            landedUfoDuration = TimeSpan.FromHours(
                GetDoubleAttr(root, manager, "u:landedUfoDuration", "hours"));

            // terror site remains 12h before aliens score.
            // Legacy design doc says 4-10h; we keep 12h.
            terrorSiteDuration = TimeSpan.FromHours(
                GetDoubleAttr(root, manager, "u:terrorSiteDuration", "hours"));

            // 30 minute delay between UFO leaving city and terror site
            // becoming visible on the geoscape.
            terrorSiteSpawnDelay = TimeSpan.FromMinutes(
                GetDoubleAttr(root, manager, "u:terrorSiteSpawnDelay", "minutes"));

            // Retaliation scouts search this radius around the destroyed
            // UFO for X-Corp outposts.  Original: 3000 km.
            // Note: the LEGACY detection range for the scout itself is
            // 240 nautical miles (~444 km) per UfoBehaviour.html:80, but
            // the C# implementation uses 3000 km for the search area of
            // the task, and we preserve that here.
            retaliationSearchRadiusRadians = (float)GeoPosition.KilometersToRadians(
                GetDoubleAttr(root, manager, "u:retaliationSearchRadius", "kilometers"));

            // All X-Corp aircraft share the same radar range (700 nm).
            // The original X-COM (UFO: Enemy Unknown) had per-craft radar
            // ranges; this value is a simplification that keeps all
            // aircraft equal in detection ability.
            aircraftRadarRangeRadians = (float)GeoPosition.KnotsToRadians(
                GetDoubleAttr(root, manager, "u:aircraftRadarRange", "nauticalMiles"));
        }

        /// <summary>
        /// Read the 8 <missionPlan> entries and build TaskPlan objects.
        /// Each <launch> child becomes a LaunchPlan with its
        /// earliestLaunch/latestLaunch hour range, landings, and subLandings.
        /// The random delay between launches is computed in
        /// LaunchPlan.CalculateLaunchDelay() (preserves legacy X-COM
        /// behavior of random delay within the configured range).
        /// </summary>
        private void LoadMissionPlans(XPathNavigator nav, XmlNamespaceManager manager)
        {
            missionPlans = new Dictionary<AlienMission, TaskPlan>();

            foreach (XPathNavigator planNav in nav.Select(
                "/u:ufobehavior/u:missionPlans/u:missionPlan", manager))
            {
                string typeName = Util.GetStringAttribute(planNav, "type");
                AlienMission type = Enum.Parse<AlienMission>(typeName);
                float score = (float)Util.GetDoubleAttribute(planNav, "score");

                List<LaunchPlan> launches = new List<LaunchPlan>();
                foreach (XPathNavigator launchNav in planNav.Select("u:launch", manager))
                {
                    string ufoType = Util.GetStringAttribute(launchNav, "ufoType");
                    float earliest = (float)Util.GetDoubleAttribute(launchNav, "earliestLaunch");
                    float latest = (float)Util.GetDoubleAttribute(launchNav, "latestLaunch");
                    int landings = Util.GetIntAttribute(launchNav, "landings");
                    int subLandings = Util.GetIntAttribute(launchNav, "subLandings");

                    launches.Add(new LaunchPlan(ufoType, earliest, latest, landings, subLandings));
                }

                missionPlans[type] = new TaskPlan(typeName, score, launches);
            }
        }

        /// <summary>
        /// Get a required double attribute from a child element.
        /// Throws a clear exception if the element or attribute is missing.
        /// </summary>
        private static double GetDoubleAttr(XPathNavigator parent, XmlNamespaceManager manager, string elementName, string attrName)
        {
            XPathNavigator node = parent.SelectSingleNode(elementName, manager);
            if (node == null)
            {
                throw new System.Xml.XmlException(
                    Util.StringFormat(Strings.EXCEPTION_UFOBEHAVIOR_MISSING_ELEMENT, elementName));
            }
            return Util.GetDoubleAttribute(node, attrName);
        }

        /// <summary>
        /// Get the TaskPlan for a specific alien mission type.
        /// </summary>
        public TaskPlan GetPlan(AlienMission type)
        {
            return missionPlans[type];
        }

        #region Timing Constants (Phase 9.4)

        /// <summary>
        /// Time a crashed UFO remains on the ground before "repairing" and
        /// departing Earth.  Original: 12 hours.
        /// </summary>
        public TimeSpan CrashSiteDuration { get { return crashSiteDuration; } }
        private TimeSpan crashSiteDuration;

        /// <summary>
        /// Time a UFO spends landed at a sampling/research site.
        /// Original: 2 hours.
        /// </summary>
        public TimeSpan LandedUfoDuration { get { return landedUfoDuration; } }
        private TimeSpan landedUfoDuration;

        /// <summary>
        /// Time X-Corp has to respond to a terror site before aliens score.
        /// Original: 12 hours.
        /// </summary>
        public TimeSpan TerrorSiteDuration { get { return terrorSiteDuration; } }
        private TimeSpan terrorSiteDuration;

        /// <summary>
        /// Delay between UFO leaving the city and the terror site
        /// appearing on the geoscape.  Original: 30 minutes.
        /// </summary>
        public TimeSpan TerrorSiteSpawnDelay { get { return terrorSiteSpawnDelay; } }
        private TimeSpan terrorSiteSpawnDelay;

        /// <summary>
        /// Retaliation UFO search radius (radians on a unit sphere) around
        /// the site where a UFO was destroyed.  Original: 3000 km.
        /// </summary>
        public float RetaliationSearchRadius { get { return retaliationSearchRadiusRadians; } }
        private float retaliationSearchRadiusRadians;

        /// <summary>
        /// Range (radians) at which X-Corp aircraft can detect UFOs and
        /// alien sites on the geoscape.  Original: 700 nautical miles.
        /// </summary>
        public float AircraftRadarRange { get { return aircraftRadarRangeRadians; } }
        private float aircraftRadarRangeRadians;

        #endregion

        #region Mission Plans (Phase 9.3)

        /// <summary>
        /// The 8 mission launch plans loaded from XML.
        /// Indexed by AlienMission enum value.
        /// </summary>
        private Dictionary<AlienMission, TaskPlan> missionPlans;

        #endregion
    }
}
