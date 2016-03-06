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
* @file OutpostCapacityInfo.cs
* @date Created: 2007/05/13
* @author File creator: David Teviotdale
* @author Credits: none
*/
#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

using Microsoft.Xna.Framework;

#endregion

namespace ProjectXenocide.Model.Geoscape.Outposts
{
    /// <summary>
    /// Represents how much "storage" an outpost has for a class of items
    /// </summary>
    [Serializable]
    public class OutpostCapacityInfo
    {
        /// <summary>
        /// Record that we've started building storage capacity of this type
        /// </summary>
        /// <param name="quantity">amount being built</param>
        public void StartBuilding(uint quantity)
        {
            building += quantity;
        }

        /// <summary>
        /// Record that we've finished building storage capacity of this type
        /// </summary>
        /// <param name="quantity">amount that has been built</param>
        public void FinishedBuilding(uint quantity)
        {
            Debug.Assert(quantity <= building);
            building  -= quantity;
            total     += quantity;
        }

        /// <summary>
        /// Record that we've destroyed storage capacity of this type
        /// </summary>
        /// <param name="quantity">amount that has been built</param>
        /// <param name="finishedBuilding">was it built, or under construction capacity?</param>
        public void Destroy(uint quantity, bool finishedBuilding)
        {
            if (finishedBuilding)
            {
                Debug.Assert(quantity <= total);
                Debug.Assert(quantity <= Available);
                total -= quantity;
            }
            else
            {
                Debug.Assert(quantity <= building);
                building -= quantity;
            }
        }

        /// <summary>
        /// Use some of the storage capacity
        /// </summary>
        /// <param name="quantity">To use</param>
        public void Use(uint quantity)
        {
            inUse += quantity;
        }

        /// <summary>
        /// Release some storage capacity that we were using
        /// </summary>
        /// <param name="quantity">To release</param>
        public void Release(uint quantity)
        {
            // Can't release more capacity than is in use
            Debug.Assert(quantity <= inUse);
            inUse -= quantity;
        }

        /// <summary>
        /// Owning outpost has been destroyed
        /// </summary>
        public void OnOutpostDestroyed()
        {
            // zero out in use, so we don't get assertions on destruction
            inUse    = 0;
        }

        /// <summary>
        /// Reserve storage capacity to hold items in transit when they arrive
        /// </summary>
        /// <param name="quantity">To reserve</param>
        public void Reserve(uint quantity)
        {
            reserved += quantity;
        }

        /// <summary>
        /// Clear reservation, presumably items have arrived.
        /// </summary>
        /// <param name="quantity">To reserve</param>
        public void ClearReservation(uint quantity)
        {
            // Can't release more capacity than is in use
            Debug.Assert(quantity <= reserved);
            reserved -= quantity;

            // We can't just assign this to inUse, because ammo may 
            // compact down and take less room than was reserved.
        }

        #region Fields

        /// <summary>
        /// Amount of storage space of this type that is currently unused.
        /// </summary>
        public uint Available { get { return (InUse < total) ? total - InUse : 0; } }

        /// <summary>
        /// Amount of storage space of this type that is being built.
        /// </summary>
        public uint Building { get { return building; } }

        /// <summary>
        /// Sum of storage space of this type that is currently unused and unused.
        /// </summary>
        public uint Total { get { return total; } }

        /// <summary>
        /// Amount of storage space of this type that is currently in use.
        /// </summary>
        public uint InUse { get { return inUse + reserved; } }

        /// <summary>
        /// Amount of space that will be used by items currently in transit
        /// </summary>
        public uint Reserved { get { return reserved; } }

        /// <summary>
        /// Amount of storage space of this type that is being built.
        /// </summary>
        private uint building;

        /// <summary>
        /// Quantity of storage space that base has fully built
        /// </summary>
        private uint total;

        /// <summary>
        /// Amount of storage space of this type that is being used.
        /// <remarks>on an overfull base, used can be greater than total</remarks>
        /// </summary>
        private uint inUse;

        /// <summary>
        /// Amount of space that will be used by items currently in transit when they arrive
        /// </summary>
        /// <remarks>We can't just assign this to inUse, because ammo may compact down
        /// and take less room than expected.</remarks>
        private uint reserved;

        #endregion Fields
    }
}
