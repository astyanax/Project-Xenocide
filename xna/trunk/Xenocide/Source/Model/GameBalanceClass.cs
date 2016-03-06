using System;
using System.Collections.Generic;
using System.Text;

using ProjectXenocide.Utils;
using ProjectXenocide.Model.Battlescape;
using ProjectXenocide.Model.StaticData.Items;
using Xenocide.Resources;

namespace ProjectXenocide.Model
{
    /// <summary>
    /// Class responsible for handling things that is subject to change due to game balance.
    /// </summary>
    /// <remarks>
    /// Weapon dealing damage function, fatal wounds etc. should be placed in this class.
    /// </remarks>
    public class GameBalanceClass
    {
        private ProjectXenocide.Model.Difficulty difficulty;

        /// <summary>
        /// Constructor
        /// </summary>
        public GameBalanceClass(ProjectXenocide.Model.Difficulty difficulty)
        {
            this.difficulty = difficulty;
            // TODO: Loading of coefficients from an xml-file should be added.
        }

        /// <summary>
        /// Generates random weapon damage in the range [1 2*baseWeaponDamage].
        /// </summary>
        /// <param name="baseWeaponDamage">The base weapon damage.</param>
        /// <returns>The generated damage.</returns>
        public int RandomWeaponDamage(int baseWeaponDamage)
        {
            // TODO: Add difference between the difficulties.
            // Create random damage in the range 1-2*baseWeaponDamage
            return Xenocide.Rng.Next(2 * baseWeaponDamage) + 1;
        }

        /// <summary>
        /// Generates the number of fatal wounds that was inflicted by the damage.
        /// </summary>
        /// <param name="inflictedDamage">The damage inflicted to the target.</param>
        /// <returns>The number of fatal wounds.</returns>
        public int GenerateFatalWounds(int inflictedDamage)
        {
            if (inflictedDamage > 10)
            {
                // Damage is more than 10, return 1-3 fatal wounds.
                return Xenocide.Rng.Next(3) + 1;
            }
            else
            {
                // Damage is less than 11, fatal wounds is inflicted with an probability of (11-inflictedDamage)/11
                if (Xenocide.Rng.Next(11) + 1 > 11 - inflictedDamage)
                {
                    // Return 1-3 fatal wounds.
                    return Xenocide.Rng.Next(3) + 1;
                }
                return 0;
            }
        }

        /// <summary>
        /// Function that generates the number of fatal wounds healed at a time by a med kit.
        /// </summary>
        /// <returns>Number of fatal wounds healed.</returns>
        public int HealFatalWounds()
        {
            // Heal one wound each time
            return 1;
        }

        /// <summary>
        /// Function that generates the number of injury damage points that is healed per fatal wound with a
        /// med kit.
        /// </summary>
        /// <returns>The number of injury points healed.</returns>
        public int HealInjuryDamage()
        {
            // Heal three injury damage each time
            return 3;
        }

        /// <summary>
        /// Function that generates the number of days it takes for a soldier to recover from the lost health 
        /// points.
        /// </summary>
        /// <param name="lostHealthPoints">The number of lost health points.</param>
        /// <returns>The number of days it takes to recover the lost health.</returns>
        public int WoundRecoveryDays(int lostHealthPoints)
        {
            return Xenocide.Rng.Next(lostHealthPoints) + (int)Math.Truncate((double)lostHealthPoints / 2);
        }

        /// <summary>
        /// Returns a factor that reduces the maximum number of time units due to fatal wounds to the legs.
        /// </summary>
        /// <param name="fatalWoundsToLegs">The number of fatal wounds to the legs.</param>
        /// <returns>The time unit reduce factor.</returns>
        public double TimeUnitDecreaseDueToFatalWounds(int fatalWoundsToLegs)
        {
            if (fatalWoundsToLegs < 9 && fatalWoundsToLegs > 0)
            {
                return (double)fatalWoundsToLegs / 10;
            }
            else if (fatalWoundsToLegs == 0)
            {
                return 1;
            }
            else
            {
                return 0.1;
            }
        }

        /// <summary>
        /// Returns a factor that reduces the energy recharge rate due to fatal wounds to the body.
        /// </summary>
        /// <param name="fatalWoundsToBody">The number of fatal wounds to the body.</param>
        /// <returns>The energy recharge rate reduce factor.</returns>
        public double EnergyRechargeDecreaseDueToFatalWounds(int fatalWoundsToBody)
        {
            if (fatalWoundsToBody < 9 && fatalWoundsToBody > 0)
            {
                return (double)fatalWoundsToBody / 10;
            }
            else if (fatalWoundsToBody == 0)
            {
                return 1;
            }
            else
            {
                return 0.1;
            }
        }

        /// <summary>
        /// Returns a factor that reduces the accuracy due to fatal wounds.
        /// </summary>
        /// <param name="fatalWounds">The number of fata wounds on affecting body parts</param>
        /// <returns>The accuracy reduction factor.</returns>
        public double AccuracyDecreaseDueToFatalWounds(int fatalWounds)
        {
            if (fatalWounds < 9 && fatalWounds > 0)
            {
                return (double)fatalWounds / 10;
            }
            else if (fatalWounds == 0)
            {
                return 1;
            }
            else
            {
                return 0.1;
            }
        }

        public double AccuracyDecreaseDueToInjury(int injury, int maximumHealth)
        {
            return (double)(maximumHealth - injury) / maximumHealth;
        }
    }
}
