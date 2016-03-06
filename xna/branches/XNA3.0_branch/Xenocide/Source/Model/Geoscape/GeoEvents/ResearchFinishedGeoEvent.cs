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
* @file ResearchFinishedGeoEvent.cs
* @date Created: 2007/05/27
* @author File creator: David Teviotdale
* @author Credits: none
*/
#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.Text;


using ProjectXenocide.Utils;
using ProjectXenocide.Model.Geoscape;
using ProjectXenocide.Model.StaticData.Research;
using Xenocide.Resources;


#endregion

namespace ProjectXenocide.Model.Geoscape.GeoEvents
{
    /// <summary>
    /// A research project has been completed
    /// </summary>
    [Serializable]
    public class ResearchFinishedGeoEvent : GeoEvent
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="topicId">The completed topic's identifier</param>
        public ResearchFinishedGeoEvent(string topicId)
        {
            this.topicId = topicId;
        }
        
        /// <summary>
        /// Called to get the event to do whatever processing is necessary
        /// </summary>
        public override void Process()
        {
            Xenocide.GameState.GeoData.GeoTime.StopTime();
            Util.ShowMessageBox(Strings.MSGBOX_RESEARCH_FINISHED, topic.Name);
        }

#region Fields

        /// <summary>
        /// The completed research topic
        /// </summary>
        private ResearchTopic topic { get { return Xenocide.StaticTables.ResearchGraph[topicId]; } }

        /// <summary>
        /// The completed topic's identifier
        /// </summary>
        private string topicId;

#endregion
    }
}
