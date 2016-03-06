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
* @file AttackResult.cs
* @date Created: 2007/07/23
* @author File creator: dteviot
* @author Credits: none
*/
#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.Text;

#endregion

namespace ProjectXenocide.Model.Geoscape.Vehicles
{
    /// <summary>
    /// What can happen if one craft tries to attack another
    /// </summary>
    public enum AttackResult
    {
        /// <summary>
        /// Nothing of interest happened.
        /// </summary>
        Nothing,

        /// <summary>
        /// Opposing craft was destroyed
        /// </summary>
        OpponentDestroyed,

        /// <summary>
        /// Opposing craft crashed
        /// </summary>
        OpponentCrashed,

        /// <summary>
        /// Opposing craft ran away
        /// </summary>
        OpponentFled,

        /// <summary>
        /// Attacker is out of ammo
        /// </summary>
        OutOfAmmo,
    }
}
