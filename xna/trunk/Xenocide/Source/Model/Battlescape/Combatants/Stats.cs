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
* @file Stats.cs
* @date Created: 2008/02/03
* @author File creator: David Teviotdale
* @author Credits: none
*/
#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

#endregion

namespace ProjectXenocide.Model.Battlescape.Combatants
{
    /// <summary>
    /// The various numerical values describing a soldier's capabilities
    /// </summary>
    [Serializable]
    public class Stats
    {
        /// <summary>
        /// Ctor
        /// </summary>
        public Stats()
        {
            // record date of hiring, so can track number of days hired for
            int days = Xenocide.GameState.GeoData.GeoTime.DayNumber();
            initialValues[(int)Statistic.DaysHired] = days;
            currentValues[(int)Statistic.DaysHired] = days;
        }

        /// <summary>
        /// Set a statistic to its initial value
        /// </summary>
        /// <param name="index">stat to view or modify</param>
        /// <param name="value">value to give it</param>
        public void SetInitialValue(Statistic index, int value)
        {
            Debug.Assert(index < Statistic.DaysHired);
            initialValues[(int)index] = currentValues[(int)index] = value;
        }

        /// <summary>
        /// View or Modify current value of a stat
        /// </summary>
        /// <param name="index">stat to view or modify</param>
        /// <returns>stats current value</returns>
        public int this[Statistic index]
        {
            get { return currentValues[(int)index]; }
            set { currentValues[(int)index] = value; }
        }

        /// <summary>Update Stats in response to a turn on the battlescape starting</summary>
        public void OnStartTurn()
        {
            int fatalWoundsToLegs = currentValues[(int)Statistic.FatalWoundsLeftLeg] + 
                currentValues[(int)Statistic.FatalWoundsRightLeg];
            currentValues[(int)Statistic.TimeUnitsLeft] = (int)(currentValues[(int)Statistic.TimeUnits] * 
                Xenocide.GameBalance.TimeUnitDecreaseDueToFatalWounds(fatalWoundsToLegs));

            currentValues[(int)Statistic.StaminaLeft] += (int)(currentValues[(int)Statistic.EnergyRecharge] * 
                Xenocide.GameBalance.EnergyRechargeDecreaseDueToFatalWounds(currentValues[(int)Statistic.FatalWoundsBody]));
            if (currentValues[(int)Statistic.Stamina] < currentValues[(int)Statistic.StaminaLeft])
            {
                currentValues[(int)Statistic.StaminaLeft] = currentValues[(int)Statistic.Stamina];
            }

            // TODO: Add update of stun damage, morale etc.
        }

        /// <summary>Adjust accuracy due to injuries</summary>
        /// <param name="activeArm">arm holding weapon</param>
        /// <returns>base probablity of a hit</returns>
        public double Accuracy(Combatant.ActiveArm activeArm)
        {
            int fatalWounds = currentValues[(int)Statistic.FatalWoundsHead];
            switch (activeArm)
            {
                case Combatant.ActiveArm.Left:
                    fatalWounds += currentValues[(int)Statistic.FatalWoundsLeftArm];
                    break;
                case Combatant.ActiveArm.Right:
                    fatalWounds += currentValues[(int)Statistic.FatalWoundsRightArm];
                    break;
                case Combatant.ActiveArm.Both:
                    fatalWounds += currentValues[(int)Statistic.FatalWoundsLeftArm];
                    fatalWounds += currentValues[(int)Statistic.FatalWoundsRightArm];
                    break;
                default:
                    break;
            }

            double injuryAccuracyDecrease = Xenocide.GameBalance.AccuracyDecreaseDueToInjury(
                currentValues[(int)Statistic.InjuryDamage], currentValues[(int)Statistic.Health]);

            return (double)currentValues[(int)Statistic.FiringAccuracy] * injuryAccuracyDecrease * 
                Xenocide.GameBalance.AccuracyDecreaseDueToFatalWounds(fatalWounds);
        }

        /// <summary>Number of days X-Corp staff member has been employed for</summary>
        public int DaysHired()
        {
            return Xenocide.GameState.GeoData.GeoTime.DayNumber() - initialValues[(int)Statistic.DaysHired];
        }

        #region Fields

        /// <summary>The current value of each statistic</summary>
        private int[] currentValues = new int[(int)Statistic.DaysHired + 1];

        /// <summary>The initial value of each statistic</summary>
        private int[] initialValues = new int[(int)Statistic.DaysHired + 1];

        #endregion Fields
    }
}
