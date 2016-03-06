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
* @file Statistic.cs
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

namespace ProjectXenocide.Model.Battlescape.Combatants
{
    /// <summary>
    /// The various statistics a combatant can have
    /// </summary>
    public enum Statistic
    {
        /// <summary>Number of time units per turn combatant gets</summary>
        TimeUnits,

        /// <summary>Combatant's endurance</summary>
        Stamina,

        /// <summary>Maximum hit points</summary>
        Health,

        /// <summary>Number of points of physical injury combatant is suffering from</summary>
        InjuryDamage,

        /// <summary>Resistance to panic</summary>
        Bravery,

        /// <summary></summary>
        Reactions,

        /// <summary></summary>
        FiringAccuracy,

        /// <summary></summary>
        ThrowingAccuracy,

        /// <summary></summary>
        Strength,

        /// <summary></summary>
        PsiStrength,

        /// <summary></summary>
        PsiSkill,

        /// Statistics past PsiSkill are not shown to player directly
        /// <summary></summary>
        EnergyRecharge,

        /// <summary></summary>
        VictoryPoints,

        /// <summary></summary>
        Aggression,

        /// <summary></summary>
        Melee,

        /// <summary></summary>
        Intelligence,

        /// <summary></summary>
        StandingHeight,

        /// <summary></summary>
        KneelingHeight,

        /// <summary></summary>
        FloatingHeight,

        /// <summary></summary>
        MotionScannerBlipSize,

        // The statistics past MotionScannerBlip size are highly dynamic

        /// <summary>Number of time units combatant has remaining to use this turn</summary>
        TimeUnitsLeft,

        /// <summary>Number of points of stun damage combatant is suffering from</summary>
        StunDamage,

        // Human soldier only stats

        /// <summary>Number of Aliens soldier has killed</summary>
        Kills,

        /// <summary>Number of Battlescape missions</summary>
        Missions,

        /// <summary>Number of days soldier has been emplyed by X-Corp for</summary>
        DaysHired,
    }

    /// <summary>
    /// Wrapper for getting names of statistics
    /// </summary>
    public static class StaticticNames
    {
        /// <summary>
        ///  Convert statistic to string to show to player
        /// </summary>
        /// <param name="statistic">statistic</param>
        /// <returns>string to show to player</returns>
        public static String DisplayString(Statistic statistic)
        {
            Debug.Assert((statistic <= Statistic.PsiSkill) || (Statistic.TimeUnitsLeft < statistic));
            return displayStrings[(int)statistic];
        }

        /// <summary>
        /// String used for each Statistic
        /// </summary>
        private static readonly String[] displayStrings =
        {
            Strings.STATISTIC_TIMEUNITS,
            Strings.STATISTIC_STAMINA,
            Strings.STATISTIC_HEALTH,
            Strings.STATISTIC_INJURY_DAMAGE,
            Strings.STATISTIC_BRAVERY,
            Strings.STATISTIC_REACTIONS,
            Strings.STATISTIC_FIRING_ACCURACY,
            Strings.STATISTIC_THROWING_ACCURCY,
            Strings.STATISTIC_STRENGTH,
            Strings.STATISTIC_PSI_STRENGTH,
            Strings.STATISTIC_PSI_SKILL,
            Strings.STATISTIC_INTERNAL_ONLY,  // EnergyRecharge
            Strings.STATISTIC_INTERNAL_ONLY,  // VictoryPoints,
            Strings.STATISTIC_INTERNAL_ONLY,  // Aggression,
            Strings.STATISTIC_INTERNAL_ONLY,  // Melee,
            Strings.STATISTIC_INTERNAL_ONLY,  // Intelligence,
            Strings.STATISTIC_INTERNAL_ONLY,  // StandingHeight,
            Strings.STATISTIC_INTERNAL_ONLY,  // KneelingHeight,
            Strings.STATISTIC_INTERNAL_ONLY,  // FloatingHeight,
            Strings.STATISTIC_INTERNAL_ONLY,  // MotionScannerBlipSize,
            Strings.STATISTIC_TIME_UNITS_LEFT,
            Strings.STATISTIC_STUN_DAMAGE,
            Strings.STATISTIC_KILLS,
            Strings.STATISTIC_MISSIONS,
            Strings.STATISTIC_DAYS_HIRED
        };
    }
}
