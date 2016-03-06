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
* @file MonthlyLog.cs
* @date Created: 2007/06/03
* @author File creator: dteviot
* @author Credits: none
*/
#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

using CeGui;

#endregion

namespace ProjectXenocide.Model
{
    /// <summary>
    /// Records the value of something, on a per month basis for
    /// this month and the preceeding 11.
    /// </summary>
    [Serializable]
    public class MonthlyLog
    {
        /// <summary>
        /// Accessor for a given index
        /// </summary>
        /// <param name="index">Month to get stats for 0 = January, 11 = December</param>
        /// <returns>value at specified month</returns>
        public int this[int index] 
        { 
            get { return history[index % 12]; } 
            set { history[index % 12] = value; }
        }

        /// <summary>
        /// Get index to history array element that stores value for specified date
        /// </summary>
        /// <param name="date">Date to check</param>
        /// <returns>Index</returns>
        public static int DateToIndex(DateTime date)
        {
            return date.Month - 1;
        }

        /// <summary>
        /// Get index value to use for this month
        /// </summary>
        /// <returns>Index value</returns>
        public static int ThisMonth
        {
            get { return DateToIndex(Xenocide.GameState.GeoData.GeoTime.Time); }
        }

        /// <summary>
        /// Get index value to use for last month
        /// </summary>
        /// <returns>Index value</returns>
        public static int LastMonth { get { return (ThisMonth + 11) % 12; } }

        /// <summary>
        /// The actual value of each month
        /// </summary>
        private int[] history = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
    }
}
