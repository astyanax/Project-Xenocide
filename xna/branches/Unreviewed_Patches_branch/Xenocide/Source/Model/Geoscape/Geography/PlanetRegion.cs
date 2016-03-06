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
* @file Region.cs
* @date Created: 2007/06/03
* @author File creator: Darkside
* @author Credits: none
*/
#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Xml;
using System.Xml.XPath;
using System.Threading;
using System.IO;


using ProjectXenocide.Utils;
using ProjectXenocide.Model;
using ProjectXenocide.Model.Geoscape;
using ProjectXenocide.Model.Geoscape.AI;

using CeGui;

#endregion

namespace ProjectXenocide.Model.Geoscape.Geography
{

    /// <summary>
    /// The information for a country in the Geoscape
    /// </summary>
    [Serializable]
    public class PlanetRegion : IGeoBitmapProperty
    {
        /// <summary>
        /// Create a new region
        /// </summary>
        /// <param name="regionNode">XML node holding data to construct region</param>
        /// <param name="manager">Namespace used in planets.xml</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
           Justification = "will throw if regionNode is null")]
        public PlanetRegion(XPathNavigator regionNode, XmlNamespaceManager manager)
        {
            this.name                = Util.GetStringAttribute(regionNode, "name");
            this.outpostBuildCost    = Util.GetIntAttribute(regionNode,    "baseBuildCost");
            this.alienAttackPriority = Util.GetIntAttribute(regionNode,    "alienAttackPriority");
            this.colorKey            = Util.GetColorKey(regionNode, manager);

            // parse the missionPriorityNode
            XPathNavigator priority = regionNode.SelectSingleNode("p:missionPriority", manager);
            alienMissionPriority = new int[(int)AlienMission.Outpost + 1];
            alienMissionPriority[(int)AlienMission.Research]     = Util.GetIntAttribute(priority, "research");
            alienMissionPriority[(int)AlienMission.Harvest]      = Util.GetIntAttribute(priority, "harvest");
            alienMissionPriority[(int)AlienMission.Abduction]    = Util.GetIntAttribute(priority, "abduction");
            alienMissionPriority[(int)AlienMission.Infiltration] = Util.GetIntAttribute(priority, "infiltration");
            alienMissionPriority[(int)AlienMission.Outpost]      = Util.GetIntAttribute(priority, "outpost");
        }

        /// <summary>
        /// Override ToString
        /// </summary>
        /// <returns>the region name</returns>
        public override string ToString()
        {
            return name;
        }

        /// <summary>
        /// Do start of month processing
        /// </summary>
        public void StartOfMonth()
        {
            ScoreLog.StartOfMonth();
        }

        /// <summary>
        /// Pick a mission for the Aliens to perform
        /// </summary>
        /// <returns>mission</returns>
        public AlienMission SelectRandomMission()
        {
            // first, find total options.
            int totalPriority = 0;
            foreach (int priority in alienMissionPriority)
            {
                totalPriority += priority;
            }
            Debug.Assert(0 < totalPriority);

            // now pick at random
            totalPriority = Xenocide.Rng.Next(totalPriority) + 1;
            for (AlienMission mission = AlienMission.Research; mission <= AlienMission.Outpost; ++mission)
            {
                totalPriority -= alienMissionPriority[(int)mission];
                if (totalPriority <= 0)
                {
                    return mission;
                }
            }

            // If get here, something went wrong
            Debug.Assert(false);
            return AlienMission.Research;
        }

        /// <summary>
        /// Mark region as no longer supporting a specific mission type
        /// </summary>
        /// <param name="missionType">type of mission</param>
        public void ClearAlienMissionPriority(AlienMission missionType)
        {
            alienMissionPriority[(int)missionType] = 0;
        }

        #region Fields

        /// <summary>
        /// Name of region
        /// </summary>
        public String Name { get { return name; } }

        /// <summary>
        /// Alien and X-Corp scores for this region
        /// </summary>
        public ScoreLog ScoreLog { get { return scoreLog; } }

        /// <summary>
        /// Cost to build an outpost in this region.
        /// </summary>
        public int OutpostBuildCost { get { return outpostBuildCost; } }

        /// <summary>
        /// Relative priority aliens give to region for attack. A zone with 0 will only get terror attacks.
        /// This priority may be dynamic as the game progresses.
        /// </summary>
        public int AlienAttackPriority 
        {
            get { return alienAttackPriority; }
            set { AlienAttackPriority = value; } //uints can't be null, so no need to check data.
        }

        /// <summary>
        /// The Land part of the region
        /// </summary>
        public GeoBitmapProperty LandedProperty { get { return landedProperty; } set { landedProperty = value; } }

        /// <summary>        /// Name of region
        /// </summary>
        private String name;

        /// <summary>
        /// RGB color associated with this region
        /// </summary>
        private uint colorKey;

        /// <summary>
        /// Number of pixels this region takes up in region bitmap
        /// </summary>
        private uint size;

        /// <summary>
        /// Alien and X-Corp scores for this region
        /// </summary>
        private ScoreLog scoreLog = new ScoreLog();
        
        /// <summary>
        /// Cost to build an outpost in this region.
        /// </summary>
        private int outpostBuildCost;

        /// <summary>
        /// Relative priority aliens give to attacking the region.
        /// </summary>
        private int alienAttackPriority;

        /// <summary>
        /// Relative priority aliens give to different mission types
        /// </summary>
        private int[] alienMissionPriority;

        /// <summary>
        /// The Land part of the region
        /// </summary>
        private GeoBitmapProperty landedProperty;

        #endregion Fields

        #region IGeoBitmapMember Members

        /// <summary>
        /// The color which represents this region on the bitmap
        /// </summary>
        public uint ColorKey { get { return colorKey; } }

        /// <summary>
        /// Number of pixels this region takes up in region bitmap
        /// </summary>
        public uint Size { get { return size; } set { size = value; } }

        #endregion
    }
}
