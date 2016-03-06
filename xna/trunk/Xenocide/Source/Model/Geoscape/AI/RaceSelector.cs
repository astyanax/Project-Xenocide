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
* @file RaceSelector.cs
* @date Created: 2007/12/26
* @author File creator: dteviot
* @author Credits: none
*/
#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.Text;

using ProjectXenocide.Utils;
using ProjectXenocide.Model;

#endregion

namespace ProjectXenocide.Model.Geoscape.AI
{
    /// <summary>
    /// Picks the Race that will crew a UFO
    /// </summary>
    [Serializable]
    public class RaceSelector
    {
        /// <summary>
        /// Ctor
        /// </summary>
        public RaceSelector()
        {
            choices = new List<Choice>();
            choices.Add(new Choice(Race.Grey,     35));
            choices.Add(new Choice(Race.Satyrian, 35));
            choices.Add(new Choice(Race.Viper,    20));
            choices.Add(new Choice(Race.Cloak,     5));
            choices.Add(new Choice(Race.Morlock,   5));
        }

        /// <summary>
        /// Pick a race a random
        /// </summary>
        /// <returns>race chosen</returns>
        public Race PickRace()
        {
            return Util.SelectRandom(choices).Race;
        }

        /// <summary>
        /// Adjust odds this race will be picked again, based on how well mission went
        /// </summary>
        /// <param name="race">race to adjust</param>
        /// <param name="evaluation">Mission evaluation -1 = very bad to +1 = very good</param>
        public void UpdateOdds(Race race, float evaluation)
        {
            // adjust odds (but never below one)
            Choice choice = choices.Find(delegate (Choice c) { return c.Race == race; });
            choice.Odds = (int)Math.Max(1.0f, choice.Odds * (1.0f + (evaluation * 0.1f)));
        }

        /// <summary>
        /// An option to pick from
        /// </summary>
        [Serializable]
        private class Choice : IOdds
        {
            /// <summary>
            /// Ctor
            /// </summary>
            /// <param name="race">Species</param>
            /// <param name="odds">Odds this species will be picked</param>
            public Choice(Race race, int odds)
            {
                this.race = race;
                this.odds = odds;
            }

            #region Fields

            /// <summary>Species</summary>
            public Race Race { get { return race; } }

            /// <summary>Odds this species will be picked</summary>
            public int Odds { get { return odds; } set { Odds = value; } }

            /// <summary>Species</summary>
            private Race race;

            /// <summary>Odds this species will be picked</summary>
            private int odds;

            #endregion Fields
        }

        #region Fields

        /// <summary>Table of races and their relative odds</summary>
        private List<Choice> choices;

        #endregion Fields
    }
}
