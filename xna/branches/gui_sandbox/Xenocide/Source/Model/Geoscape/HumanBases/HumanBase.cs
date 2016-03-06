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
* @file HumanBase.cs
* @date Created: 2007/02/04
* @author File creator: dteviot
* @author Credits: none
*/
#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;

using Xenocide.Model.Geoscape.Craft;

#endregion

namespace Xenocide.Model.Geoscape.HumanBases
{
    /// <summary>
    /// An X-Corp Base
    /// </summary>
    [Serializable]
    public class HumanBase
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public HumanBase(GeoPosition position, String name) 
        {
            this.position = position;
            this.name     = name;

            statistics = new BaseStatistics();
            floorplan  = new Floorplan(statistics);
        }

        /// <summary>
        /// Update the base's state, to allow for passage of time
        /// </summary>
        /// <param name="milliseconds">The amount of time that has passed</param>
        /// <remarks>At moment, just pump the aircraft and facilities owned by the base.</remarks>
        public void Update(double milliseconds)
        {
            // can't use foreach, because aircraft may be removed from collection
            for (int i = Fleet.Count - 1; 0 <= i; --i)
            {
                Fleet[i].Update(milliseconds);
            }

            floorplan.Update(milliseconds);
        }

        /// <summary>
        /// Assign another Aircraft to this base
        /// </summary>
        /// <param name="plane">aircraft to assign</param>
        public void AddAircraft(Aircraft plane)
        {
            Fleet.Add(plane);
        }

        /// <summary>
        /// Remove Aircraft from this base
        /// </summary>
        /// <param name="plane">aircraft to remove</param>
        public void RemoveAircraft(Aircraft plane)
        {
            Fleet.Remove(plane);
        }

        /// <summary>
        /// Configure the first base the player gets
        /// </summary>
        public void SetupPlayersFirstBase()
        {
            // add facilities to base
            floorplan.SetupPlayersFirstBase();

            EquipFirstBaseWithCraft();

            //ToDo: staff & items.
        }

        /// <summary>
        /// Equip base with the craft user initially starts with
        /// </summary>
        private void EquipFirstBaseWithCraft()
        {
            //ToDo: finish. (at moment, it's just a single craft)
            double speed = GeoPosition.KilometersToRadians(5000.0) / 3600.0;   // 5000 kph (approx) in radians/sec
            AddAircraft(new Aircraft(speed, this));
        }

        #region Fields

        /// <summary>
        /// Where the base is on the globe
        /// </summary>
        public GeoPosition Position { get { return position; } set { position = value; } }

        /// <summary>
        /// The aircraft owned by the base
        /// </summary>
        public IList<Aircraft> Fleet { get { return fleet; } }
        
        /// <summary>
        /// Where the base is on the globe
        /// </summary>
        private GeoPosition position;

        /// <summary>
        /// The aircraft owned by the base
        /// </summary>
        private List<Aircraft> fleet = new List<Aircraft>();

        /// <summary>
        /// Layout of facilities in the base
        /// </summary>
        public Floorplan Floorplan { get { return floorplan; } }

        /// <summary>
        /// Layout of facilities in the base
        /// </summary>
        private Floorplan floorplan;

        /// <summary>
        /// The Name this base has been given
        /// </summary>
        public String Name { get { return name; } }

        /// <summary>
        /// The Name this base has been given
        /// </summary>
        private String name;

        /// <summary>
        /// The capabilities of the base
        /// </summary>
        public BaseStatistics Statistics { get { return statistics; } }
        
        /// <summary>
        /// The capabilities of the base
        /// </summary>
        private BaseStatistics statistics;

        #endregion Fields
    }
}
