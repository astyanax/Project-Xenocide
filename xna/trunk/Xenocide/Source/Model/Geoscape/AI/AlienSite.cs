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
* @file AlienSite.cs
* @date Created: 2007/08/20
* @author File creator: David Teviotdale
* @author Credits: none
*/
#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.Text;

using ProjectXenocide.Model.StaticData;
using ProjectXenocide.Model.StaticData.Items;
using ProjectXenocide.Model.Geoscape;
using ProjectXenocide.Model.Geoscape.Vehicles;
using ProjectXenocide.Model.Battlescape;

#endregion

namespace ProjectXenocide.Model.Geoscape.AI
{
    /// <summary>
    /// A site of alien activity on the earth. (Either a terror site, or an alien outpost)
    /// </summary>
    [Serializable]
    public abstract class AlienSite : IGeoPosition
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the site</param>
        /// <param name="position">The position of the site</param>
        /// <param name="race">The race that is present at this site</param>
        protected AlienSite(String name, GeoPosition position, Race race)
        {
            this.name      = name;
            this.position  = position;
            this.race      = race;
        }

        /// <summary>
        /// Cleanup the site
        /// </summary>
        public void OnSiteDestroyed()
        {
            OnSiteDestroyedCore();
            Xenocide.GameState.GeoData.Overmind.RemoveSite(this);
            for (int i = inbound.Count - 1; 0 <= i; --i)
            {
                inbound[i].OnSiteGone(this);
            }
        }

        /// <summary>
        /// Tell site it has an a craft headed for the site
        /// </summary>
        /// <param name="craft">the inbound craft</param>
        public void AddInbound(Craft craft)
        {
            inbound.Add(craft);
        }

        /// <summary>
        /// Tell site it that a craft is no longer heading for the site
        /// </summary>
        /// <param name="craft">the no longer inbound craft</param>
        public void RemoveInbound(Craft craft)
        {
            inbound.Remove(craft);
        }

        /// <summary>
        /// Template method pattern
        /// </summary>
        protected abstract void OnSiteDestroyedCore();

        /// <summary>Create Alien force for a battlescape mission at this site</summary>
        /// <returns>The alien force</returns>
        public abstract Team CreateAlienTeam();

        /// <summary>Generate a list of items recovered from site</summary>
        /// <returns>the items</returns>
        public virtual IList<ItemLine> RecoveredItems()
        {
            return new List<ItemLine>();
        }

        /// <summary>Create Alien force for a battlescape mission at this site</summary>
        /// <param name="ufoType">Aliens will be the crew of a UFO of this type</param>
        /// <returns>The alien force</returns>
        protected Team CreateAlienTeam(string ufoType)
        {
            UfoItemInfo ufoInfo = Xenocide.StaticTables.ItemList[ufoType] as UfoItemInfo;
            return ufoInfo.CreateCrew(Race, Xenocide.StaticTables.StartSettings.Difficulty, 100);
        }

        #region fields

        /// <summary>
        /// The name of the site
        /// </summary>
        public String Name { get { return name; } }

        /// <summary>
        /// The position of the site
        /// </summary>
        public GeoPosition Position { get { return position; } }

        /// <summary>
        /// Is the position known to X-Corp?
        /// </summary>
        public abstract bool IsKnownToXCorp { get; set;}

        /// <summary>
        /// Hostile (i.e. X-Corp) craft that are inbound to this AlienSite
        /// </summary>
        public IList<Craft> Inbound { get { return inbound; } }

        /// <summary>
        /// The race that is present at this site
        /// </summary>
        public Race Race { get { return race; } }

        /// <summary>
        /// Number of points the Overmind gets each day this site remains in existance
        /// </summary>
        public virtual int DailyScore { get { return 0; } }

        /// <summary>
        /// The name of this site
        /// </summary>
        private String name;

        /// <summary>
        /// The position of the site
        /// </summary>
        private GeoPosition position;

        /// <summary>
        /// Hostile (i.e. X-Corp) craft that are inbound to this AlienSite
        /// </summary>
        private List<Craft> inbound = new List<Craft>();

        /// <summary>
        /// The race that is present at this site
        /// </summary>
        private Race race;

        #endregion fields
    }
}
