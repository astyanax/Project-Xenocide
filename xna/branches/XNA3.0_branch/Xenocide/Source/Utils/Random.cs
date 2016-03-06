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
* @file Util.cs
* @date Created: 2008/01/27
* @author File creator: David Teviotdale
* @author Credits: none
*/
#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Xml;
using System.Xml.XPath;
using System.Threading;
using System.IO;
using System.Globalization;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;


using ProjectXenocide.Model;
using ProjectXenocide.Model.Geoscape;
using ProjectXenocide.UI.Screens;
using ProjectXenocide.UI.Dialogs;

using CeGui;

using Vector3 = Microsoft.Xna.Framework.Vector3;

#endregion

namespace ProjectXenocide.Utils
{
    /// <summary>
    /// Random number generator (with extra features for debugging)
    /// </summary>
    public class Rng
    {
        /// <summary>
        /// Fetch next random number
        /// </summary>
        /// <param name="max">random number is in range [0, max - 1]</param>
        /// <returns>random number</returns>
        public int Next(int max)
        {
#if DEBUG
            if (0 < loadedValues.Count)
            {
                int temp = loadedValues[loadedValues.Count - 1];
                loadedValues.RemoveAt(loadedValues.Count - 1);
                Debug.Assert(temp < max);
                return temp;
            }
            else
#endif
            return rng.Next(max);
        }

        /// <summary>
        /// Roll the dice, and see if succeed or fail
        /// </summary>
        /// <param name="successPercentage">Odds of succeeding</param>
        /// <returns>true if succeed</returns>
        public bool RollDice(int successPercentage)
        {
            // range of rand is 0 <= rand <= 99
            int rand = Next(100);
            return (rand < successPercentage);
        }

        /// <summary>
        /// Roll the dice, and see if succeed or fail
        /// </summary>
        /// <param name="successProbablility">probability of success</param>
        /// <returns>true if succeed</returns>
        public bool RollDice(double successProbablility)
        {
            // ideally, successProbablility should be in range (0.0, 1.0),
            // so value greater than 1 probably indicates a misunderstanding.
            // However, there's a couple of cases where probabilities exceed 1.0,
            // so using 1.0 as a threshold is too sensitive
            Debug.Assert((0 <= successProbablility) && (successProbablility <= 2.5));
            return RollDice((int)(successProbablility * 100));
        }

        /// <summary>
        /// Load Rng with set of values it's to produce in order
        /// </summary>
        /// <param name="values">the set of values</param>
        [Conditional("DEBUG")]
        public void RigDice(IList<int> values)
        {
            // add items in reverse order, so we pop them off stack in order
            for (int i = values.Count - 1; 0 <= i; --i)
            {
                loadedValues.Add(values[i]);
            }
        }

        #region Fields

        /// <summary>real random number generator</summary>
        private static Random rng = new Random();

        /// <summary>Table of pre-determined numbers, for rigging the RNG for debugging</summary>
        private List<int> loadedValues = new List<int>();

        #endregion Fields
    }
}
