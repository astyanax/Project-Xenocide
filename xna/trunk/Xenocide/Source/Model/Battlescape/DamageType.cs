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
* @file DamageType.cs
* @date Created: 2007/12/14
* @author File creator: David Teviotdale
* @author Credits: none
*/
#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

using ProjectXenocide.Utils;

using ProjectXenocide.Model.Geoscape.Outposts;
using ProjectXenocide.Model.StaticData.Items;

#endregion

namespace ProjectXenocide.Model.Battlescape
{
    /// <summary>
    /// The types of damage that can be inflicted on a combatant on the battlescape
    /// </summary>
    public enum DamageType
    {
        /// <summary></summary>
        Piercing,

        /// <summary></summary>
        Fire,

        /// <summary></summary>
        Explosive,

        /// <summary></summary>
        Laser,

        /// <summary></summary>
        Plasma,

        /// <summary></summary>
        Stun,

        /// <summary></summary>
        Melee,

        /// <summary></summary>
        Acid,

        /// <summary></summary>
        Smoke,
    }
}