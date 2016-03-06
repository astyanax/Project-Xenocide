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
* @file Series.cs
* @date Created: 2008/01/01
* @author File creator: Oded Coster
* @author Credits: dteviot
*/
#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.Text;

#endregion

namespace ProjectXenocide.Model
{
    /// <summary>
    /// A line of data on a graph
    /// </summary>
    public class Series
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="label">The name of the series</param>
        /// <param name="data">The data points</param>
        /// <param name="scale">Value to use to scale data point values</param>
        public Series(String label, MonthlyLog data, double scale)
        {
            this.label = label;
            this.data = data;
            this.scale = scale;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="label">The name of the series</param>
        /// <param name="data">The data points</param>
        public Series(String label, MonthlyLog data)
            :
            this(label, data, 1.0)
        {
        }

        /// <summary>
        /// toggle between shown and not shown state
        /// </summary>
        public void ToggleShow()
        {
            show = !show;
        }

        /// <summary>
        /// The value for a given month, adjusted by the scalling factor
        /// </summary>
        /// <param name="month">Month to get data for</param>
        /// <returns>Value</returns>
        public int ScaledData(int month)
        {
            return (int)Math.Round(data[month] * scale);
        }

        #region Fields

        /// <summary>
        /// The name of the series
        /// </summary>
        public String Label { get { return label; } }

        /// <summary>
        /// Label, with decoration to indicate if it's selected or not
        /// </summary>
        public String DecoratedLabel { get { return (show ? "[x] " : "[ ] ") + label; } }

        /// <summary>
        /// Draw this series on the current graph?
        /// </summary>
        public bool Show { get { return show; } }

        /// <summary>
        /// The name of the series
        /// </summary>
        private String label;

        /// <summary>
        /// The data points
        /// </summary>
        private MonthlyLog data;

        /// <summary>
        /// Value to use to scale data point values
        /// </summary>
        private double scale;

        /// <summary>
        /// Draw this series on the current graph?
        /// </summary>
        private bool show = true;

        #endregion Fields
    }
}
