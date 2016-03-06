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
* @file BaseCapacityInfo.cs
* @date Created: 2007/05/13
* @author File creator: dteviot
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

namespace Xenocide.Model.Geoscape.HumanBases
{
    /// <summary>
    /// Represents how much "storage" a human base has for a class of items
    /// </summary>
    [Serializable]
    public class BaseCapacityInfo
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

        #region Fields

        /// <summary>
        /// Amount of storage space of this type that is currently unused.
        /// </summary>
        public uint Available { get { return (inUse < total) ? total - inUse : 0; } }

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
        public uint InUse { get { return inUse; } }

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

        #endregion Fields
    }
}
