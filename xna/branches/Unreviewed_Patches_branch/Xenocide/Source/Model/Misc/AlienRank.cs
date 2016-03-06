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
* @file AlienRank.cs
* @date Created: 2007/09/15
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
    /// The various ranks
    /// </summary>
    public enum AlienRank
    {
        /// <summary>
        /// 
        /// </summary>
        Commander,

        /// <summary>
        /// 
        /// </summary>
        Leader,

        /// <summary>
        /// 
        /// </summary>
        Engineer,

        /// <summary>
        /// 
        /// </summary>
        Medic,

        /// <summary>
        /// 
        /// </summary>
        Navigator,

        /// <summary>
        /// 
        /// </summary>
        Soldier,

        /// <summary>
        /// 
        /// </summary>
        Terrorist,

        /// <summary>
        /// A civilian
        /// </summary>
        Civilian,

        /// <summary>
        /// This one's not really a rank, it's used to indicate we don't care about rank
        /// </summary>
        Alive
    }

    /// <summary>
    /// Assorted functions for working with ranks
    /// </summary>
    public static class Ranks
    {
        /// <summary>
        ///  Convert rank to string to show to player
        /// </summary>
        /// <param name="rank">rank en</param>
        /// <returns>display string</returns>
        public static String DisplayString(AlienRank rank)
        {
            return displayStrings[(int)rank];
        }

        /// <summary>
        /// String used for each rank
        /// </summary>
        private static readonly String[] displayStrings = 
        {
            Strings.RANK_CIVILIAN_DISPLAY,
            Strings.RANK_SOLDIER_DISPLAY,
            Strings.RANK_LEADER_DISPLAY,
            Strings.RANK_TERRORIST_DISPLAY,
            Strings.RANK_NAVIGATOR_DISPLAY,
            Strings.RANK_ENGINEER_DISPLAY,
            Strings.RANK_MEDIC_DISPLAY,
            Strings.RANK_COMMANDER_DISPLAY
        };
    }
}
