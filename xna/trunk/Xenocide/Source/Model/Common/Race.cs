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
* @file Race.cs
* @date Created: 2007/12/26
* @author File creator: David Teviotdale
* @author Credits: none
*/
#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Xml.XPath;


using ProjectXenocide.Utils;
using Xenocide.Resources;


#endregion

namespace ProjectXenocide.Model
{
    /// <summary>
    /// The various races
    /// </summary>
    public enum Race
    {
        /// <summary>Race has not been set</summary>
        None,

        /// <summary>Sectiod</summary>
        Grey,

        /// <summary>Floater</summary>
        Satyrian,

        /// <summary>Snakeman</summary>
        Viper,

        /// <summary>Ethearial</summary>
        Cloak,

        /// <summary>Muton</summary>
        Morlock,

        /// <summary>Sectiod Terrorist</summary>
        TerrorDisc,

        /// <summary>Floater Terrorist</summary>
        Raptor,

        /// <summary>Snakeman Terrorist</summary>
        Spawn,

        /// <summary>Cloak Terrorist</summary>
        Artopod,

        /// <summary>Morlock Terrorist</summary>
        Ventriculant,

        /// <summary>Morlock Terrorist</summary>
        Silabrate,

        /// <summary>Infected Human</summary>
        Zombie,

        /// <summary></summary>
        Human,

        /// <summary>Tracked X-Cap</summary>
        Tank,

        /// <summary>Hover X-Cap</summary>
        HoverTank
    }

    /// <summary>
    /// Assorted functions for working with Races
    /// </summary>
    public static class Races
    {
        /// <summary>
        ///  Convert race to string to show to player
        /// </summary>
        /// <param name="race">race</param>
        /// <returns>string to show to player</returns>
        public static String DisplayString(Race race)
        {
            return displayStrings[(int)race];
        }

        /// <summary>
        /// String used for each race
        /// </summary>
        private static readonly String[] displayStrings = 
        {
            null,
            Strings.RACE_GREY,
            Strings.RACE_SATYRIAN,
            Strings.RACE_VIPER,
            Strings.RACE_CLOAK,
            Strings.RACE_MORLOCK,
            Strings.RACE_TERRORDISC,
            Strings.RACE_RAPTOR,
            Strings.RACE_SPAWN,
            Strings.RACE_ARTOPOD,
            Strings.RACE_VENTRICULANT,
            Strings.RACE_SILABRATE,
            Strings.RACE_ZOMBIE,
            Strings.RACE_HUMAN,
            Strings.RACE_TANK,
            Strings.RACE_HOVERTANK
        };
    }
}
