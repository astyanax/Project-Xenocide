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
* @file ShootOrder.cs
* @date Created: 2008/02/24
* @author File creator: David Teviotdale
* @author Credits: none
*/
#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

using Microsoft.Xna.Framework;
using ProjectXenocide.Model.StaticData.Items;

#endregion

namespace ProjectXenocide.Model.Battlescape.Combatants
{
    /// <summary>
    /// Instruction telling combatant to fire a weapon
    /// </summary>
    public partial class ShootOrder : Order
    {
        /// <summary>Ctor</summary>
        /// <param name="combatant">The combatant performing the order</param>
        /// <param name="battlescape">the combatant's environment</param>
        /// <param name="target">Location on battlescape to shoot at</param>
        /// <param name="weapon">weapon being fired</param>
        /// <param name="actionInfo">Additional information on the shooting action</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
            Justification = "Will throw if actionInfo is null")]
        public ShootOrder(Combatant combatant, Battle battlescape, Vector3 target, Item weapon, ShootActionInfo actionInfo)
            :
            base(combatant, battlescape)
        {
            this.target     = new Vector3((int)target.X, (int)target.Y, (int)target.Z);
            this.weapon     = weapon;
            this.actionInfo = actionInfo;
            this.shotCount  = actionInfo.Shots;
        }

        /// <summary>Spend time performing the order</summary>
        /// <param name="seconds">time to update order's progress by</param>
        public override void Update(double seconds)
        {
            Debug.Assert(FinishCode.Executing == Finished);
            Trajectory bullet = Battlescape.Trajectory;

            // If no bullets on screen, fire one, else track the bullet
            if (null == bullet)
            {
                Fire();
            }
            else
            {
                if (!bullet.Update((float)seconds))
                {
                    ShotReachedDestination();
                }
            }
        }

        /// <summary>Fire Weapon</summary>
        private void Fire()
        {
            actionInfo.PlayStartSound();

            // Figure out if shot is going to hit target or not.
            shotHits = Xenocide.Rng.RollDice(actionInfo.ChanceToHit(Combatant));
            if (shotHits)
            {
                Combatant.RecordAchievement(Experience.Act.ShotHitTarget);
            }

            Battlescape.Trajectory = new Trajectory(CalcTrajectoryStart(), CalcTrajectoryEnd());
        }

        /// <summary>Shot has travelled across the battlescape</summary>
        private void ShotReachedDestination()
        {
            // remove from battlescape
            Battlescape.Trajectory = null;

            // allocate damage to whatever the bullet hit
            Combatant victim = Battlescape.FindCombatantAt(endPoint);
            if (shotHits && (null != victim))
            {
                DamageCombatant(victim);
            }
            else
            {
                DamageLandscape();
            }

            // if no more shots, we're done
            if (--shotCount <= 0)
            {
                Finished = FinishCode.Normal;
            }
        }

        /// <summary>Figure out where the missed shot is going to go</summary>
        /// <returns>cell the missed shot is going to hit</returns>
        private Vector3 MissedShotLocation()
        {
            // ToDo: implement, for moment, it's going same place as a hit
            /* Suggested logic
             * Find all cells that are near (say within 3 cells) of target.
             * Exclude all cells that are (a) off map, or (b) not in the shooter's field of view.
             * Pick a cell at random from the remaining set.
             * Trace ray from shooter to selected cell, see where ray ends.
             * If ray is blocked, then shot hits whatever is blocking it.
             * Otherwise, if end cell is occupied, shot hits occupant.
            */
            return target;
        }

        /// <summary>Figure out where muzzle of weapon is</summary>
        /// <returns>muzzle's position, in cell co-ordinates</returns>
        private Vector3 CalcTrajectoryStart()
        {
            // ToDo: implement properly
            // ideally would put bullet at muzzle of weapon, but for time being
            // put roughly in center of mass of combatant
            return Combatant.Position + new Vector3(0, 0.3f, 0);
        }

        /// <summary>Figure out where bullet will finally end up</summary>
        /// <returns>end position, in cell co-ordinates</returns>
        private Vector3 CalcTrajectoryEnd()
        {
            // If it's going to miss, pick random location, otherwise it will hit the target
            endPoint = target;
            if (!shotHits)
            {
                endPoint = MissedShotLocation();
            }
            endPoint += new Vector3(0.5f, Battlescape.Terrain.GroundHeight(endPoint), 0.5f);

            // ToDo: if there's a combatant in the cell, the combatant will be hit, otherwise we hit ground
            // once we have miss targeting location working, for moment, miss just hits ground.
            // i.e. Take out the "shot hits" term.
            if (shotHits && (0 != Battlescape.Terrain.FindCombatantAt(endPoint)))
            {
                endPoint.Y += 0.3f;
            }
            return endPoint;
        }

        /// <summary>Allocate damage to the terrain cell the bullet hit</summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic",
            Justification = "ToDo: function needs implementation")]
        private void DamageLandscape()
        {
            //ToDo: implement
        }

        /// <summary>Allocate damage to the combatant the bullet hit</summary>
        /// <param name="victim">combatant the bullet hit</param>
        private void DamageCombatant(Combatant victim)
        {
            // can't do any more damage to a corpse
            if (!victim.IsDead)
            {
                // apply damage to combatant
                victim.Hit(weapon.DamageInfo, endPoint - Combatant.Position);

                // victim died?
                if (victim.IsDead)
                {
                    Combatant.RecordAchievement(Experience.Act.KilledTarget);
                }
            }
        }

        #region Fields

        /// <summary>Location on battlescape to shoot at</summary>
        private Vector3 target;

        /// <summary>Where the shot is actually going</summary>
        private Vector3 endPoint;

        /// <summary>weapon being fired</summary>
        private Item weapon;

        /// <summary>Additional information on the shooting action</summary>
        private ShootActionInfo actionInfo;

        /// <summary>Number of shots remining to fire</summary>
        /// <remarks>Used to keep track of shots when firing multi-round bursts</remarks>
        private int shotCount;

        /// <summary>Is shot going to hit the intended target?</summary>
        private bool shotHits;

        #endregion
    }
}
