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
* @file Ufo.cs
* @date Created: 2007/02/10
* @author File creator: David Teviotdale
* @author Credits: none
*/
#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;
using System.Diagnostics;


using ProjectXenocide.Utils;

using ProjectXenocide.Model.StaticData;
using ProjectXenocide.Model.Geoscape.AI;
using ProjectXenocide.Model.Geoscape.Outposts;
using ProjectXenocide.Model.Geoscape.GeoEvents;
using ProjectXenocide.Model.StaticData.Items;
using ProjectXenocide.Model.StaticData.Facilities;
using ProjectXenocide.Model.Battlescape;
using Xenocide.Resources;


#endregion

namespace ProjectXenocide.Model.Geoscape.Vehicles
{
    /// <summary>
    /// A craft owned by the alien
    /// </summary>
    [Serializable]
    public partial class Ufo : Craft
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="position">Initial location of UFO</param>
        /// <param name="craftType">type of craft (from item.xml)</param>
        /// <param name="task">Task that created (and "owns") the UFO</param>
        public Ufo(String craftType, GeoPosition position, InvasionTask task)
            :
            base(craftType, Util.StringFormat(Strings.UFO_NAME, UfoCounter), position)
        {
            this.task = task;
            this.race = PickRace();

            // equip Ufo with weapon, if it normally has one
            if (1 == NumHardpoints)
            {
                WeaponPods[0] = new WeaponPod(UfoItemInfo.Weapon);
            }
        }

        /// <summary>
        /// Update the craft, due to passage of time
        /// </summary>
        /// <param name="milliseconds">The amount of time that has passed</param>
        public override void Update(double milliseconds)
        {
            base.Update(milliseconds);
            UpdateVisibility();
        }

        /// <summary>
        /// Respond to this craft being destroyed
        /// </summary>
        /// <remarks>tell owner Ufo no longer exists</remarks>
        public override void OnDestroyed()
        {
            base.OnDestroyed();

            // tell owner that we've been destroyed
            Task.OnUfoDestroyed(this);

            // adjust score
            Xenocide.GameState.GeoData.AddScore(Participant.XCorp, ItemInfo.Score, Position);
        }

        /// <summary>
        /// Respond to this craft crashing
        /// </summary>
        /// <remarks>default behaviour is do nothing</remarks>
        public override void OnCrashed()
        {
            base.OnCrashed();
            
            // adjust score
            Xenocide.GameState.GeoData.AddScore(Participant.XCorp, ItemInfo.Score / 2, Position);
        }

        /// <summary>
        /// Respond to this craft finishing it's current mission
        /// </summary>
        public override void OnMissionFinished()
        {
            base.OnMissionFinished();

            // tell owner that we're done
            Task.OnMissionFinished(this);
        }

        /// <summary>
        /// Generate a list of items recovered from UFO
        /// </summary>
        /// <returns></returns>
        public IList<ItemLine> RecoveredItems()
        {
            List<ItemLine> list = new List<ItemLine>();
            
            // Odds of item surviving are proportional to damage done to UFO
            foreach (ItemLine line in UfoItemInfo.Salvage)
            {
                String itemId   = line.ItemId;
                int    quantity = 0;
                for (int i = 0; i < line.Quantity; ++i)
                {
                    if (Xenocide.Rng.RollDice(HullPercent))
                    {
                        ++quantity;
                    }
                }
                if (0 < quantity)
                {
                    list.Add(new ItemLine(itemId, quantity));
                }
            }
            return list;
        }

        /// <summary>
        /// Create the aliens aboard the UFO (for player to fight on battlescape)
        /// </summary>
        /// <param name="difficulty">game difficulty</param>
        /// <returns>The aliens to put on the battlescape</returns>
        public Team CreateCrew(Difficulty difficulty)
        {
            int health = (int)Math.Round(100 * (1 - (HullDamage / MaxDamage)));
            return UfoItemInfo.CreateCrew(Race, difficulty, health);
        }

        /// <summary>
        /// Update UFO's visibility state
        /// </summary>
        private void UpdateVisibility()
        {
            bool oldVisibility = isOnRadar;
            isOnRadar = IsVisible();

            // send message if UFO's visibility changes
            if (oldVisibility != isOnRadar)
            {
                if (isOnRadar)
                {
                    MessageBoxGeoEvent.Queue(Strings.MSGBOX_UFO_DETECTED, Name);
                }
                else
                {
                    // tell any hunters they've lost the prey
                    for (int i = Hunters.Count - 1; 0 <= i; --i)
                    {
                        Hunters[i].OnPreyTrackingLost();
                    }
                }
            }
        }

        /// <summary>
        /// Test to see if UFO is showing on any radars
        /// </summary>
        private bool IsVisible()
        {
            // crashes are always visible
            if (IsCrashed)
            {
                return true;
            }

            // check if any outposts or the craft owned by the outpost see UFO
            foreach (Outpost outpost in Xenocide.GameState.GeoData.Outposts)
            {
                if (outpost.IsOnRadar(Position, isOnRadar))
                {
                    return true;
                }
                else
                {
                    foreach (Craft aircraft in outpost.Fleet)
                    {
                        if (aircraft.IsOnRadar(Position))
                        {
                            return true;
                        }
                    }
                }
            }

            // if get here, wasn't found
            return false;
        }

        /// <summary>
        /// Pick the race that will crew a UFO
        /// </summary>
        private static Race PickRace()
        {
            return Xenocide.GameState.GeoData.Overmind.RaceSelector.PickRace();
        }

        #region Fields

        /// <summary>
        /// Task that created (and "owns") this UFO
        /// </summary>
        public InvasionTask Task { get { return task; } }

        /// <remarks>UFOs don't have a base (yet), so calling this for a UFO is a mistake</remarks>
        public override Outpost HomeBase 
        {
            get { throw new NotImplementedException(Strings.EXCEPTION_UFOS_HAVE_NO_BASE); }
            set { throw new NotImplementedException(Strings.EXCEPTION_UFOS_HAVE_NO_BASE); }
        }

        /// <remarks>UFOs don't have a base (yet), so calling this for a UFO is a mistake</remarks>
        public override bool InBase 
        {
            get { throw new NotImplementedException(Strings.EXCEPTION_UFOS_HAVE_NO_BASE); }
            set { throw new NotImplementedException(Strings.EXCEPTION_UFOS_HAVE_NO_BASE); } 
        }

        /// <summary>
        /// Does X-Corp know about the position of this UFO?
        /// </summary>
        /// <returns>true if this craft is known</returns>
        public override bool IsKnownToXCorp { get { return isOnRadar; } }

        /// <summary>
        /// The race that is crewing this UFO
        /// </summary>
        public Race Race { get { return race; } set { race = value; } }

        /// <summary>
        /// Task that created (and "owns") the UFO
        /// </summary>
        private InvasionTask task;

        /// <summary>
        /// The Item object holding the static properties of this type of UFO
        /// </summary>
        public UfoItemInfo UfoItemInfo { get { return ItemInfo as UfoItemInfo; } }

        /// <summary>
        /// is the UFO showning on someone's radar
        /// </summary>
        private bool isOnRadar;

        /// <summary>
        /// The race that is crewing this UFO
        /// </summary>
        private Race race;

        /// <summary>
        /// Used to assign each UFO a unique name of form UFO-{x}, where x is number for UFO
        /// </summary>
        private static int UfoCounter { get { return Xenocide.GameState.GeoData.Overmind.NextUfoCounter; } }

        #endregion Fields
    }
}
