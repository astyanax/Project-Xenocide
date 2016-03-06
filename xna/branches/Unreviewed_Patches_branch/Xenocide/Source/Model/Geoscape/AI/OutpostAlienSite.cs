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
* @file OutpostAlienSite.cs
* @date Created: 2007/08/27
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
using ProjectXenocide.Model.Geoscape.Geography;
using ProjectXenocide.Model.StaticData;
using ProjectXenocide.Model.Battlescape;
using ProjectXenocide.Model.StaticData.Items;
using Xenocide.Resources;


#endregion

namespace ProjectXenocide.Model.Geoscape.AI
{
    /// <summary>
    /// An alien outpost on earth
    /// </summary>
    [Serializable]
    public class OutpostAlienSite : AlienSite
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="position">Position of the outpost</param>
        /// <param name="race">The race that is present at this site</param>
        public OutpostAlienSite(GeoPosition position, Race race)
            :
            base(CreateName(), position, race)
        {
            Overmind overmind = Xenocide.GameState.GeoData.Overmind;
            task = overmind.TaskFactory.CreateSupplyTask(overmind, this);
            overmind.AddTask(task);
        }

        /// <summary>
        /// Handle outpost being destroyed
        /// </summary>
        /// <remarks>template method pattern</remarks>
        protected override void OnSiteDestroyedCore()
        {
            task.Abort();
            Xenocide.GameState.GeoData.Overmind.RemoveTask(task);
        }

        /// <summary>Create Alien force for a battlescape mission at this site</summary>
        /// <returns>The alien force</returns>
        public override Team CreateAlienTeam()
        {
            return CreateAlienTeam("ITEM_UFO_DUMMY_ALIEN_OUTPOST");
        }

        /// <summary>Generate a list of items recovered from alien outpost</summary>
        /// <returns>the items</returns>
        public override IList<ItemLine> RecoveredItems()
        {
            return (Xenocide.StaticTables.ItemList["ITEM_UFO_DUMMY_ALIEN_OUTPOST"] as UfoItemInfo).Salvage;
        }

        /// <summary>
        /// Create a unique name for this Alien Outpost
        /// </summary>
        /// <returns>New Name</returns>
        private static String CreateName()
        {
            int counter = Xenocide.GameState.GeoData.Overmind.NextOutpostCounter;
            return Util.StringFormat(Strings.ALIEN_OUTPOST_SITE_NAME, counter);
        }

        #region fields

        /// <summary>
        /// Is the position known to X-Corp?
        /// </summary>
        public override bool IsKnownToXCorp { get { return isKnownToXCorp; } set { isKnownToXCorp = value; } }

        /// <summary>
        /// Number of points the Overmind gets each day this site remains in existance
        /// </summary>
        public override int DailyScore { get { return StartSettings.AlienOutpostDailyScore; } }

        /// <summary>
        /// Is the position known to X-Corp?
        /// </summary>
        private bool isKnownToXCorp;

        /// <summary>
        /// The task that keeps running supply missions to this outpost
        /// </summary>
        private SupplyOutpostTask task;

        #endregion fields
    }
}
