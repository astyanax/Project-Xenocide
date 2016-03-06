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
* @file ShootActionInfo.cs
* @date Created: 2008/02/18
* @author File creator: David Teviotdale
* @author Credits: none
*/
#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using System.Collections.Specialized;
using System.Diagnostics;

using Microsoft.Xna.Framework;


using ProjectXenocide.Utils;
using ProjectXenocide.Model.Battlescape;
using ProjectXenocide.Model.Battlescape.Combatants;
using Xenocide.Resources;


#endregion

namespace ProjectXenocide.Model.StaticData.Items
{
    /// <summary>
    /// Details of shooting a weapon
    /// </summary>
    [Serializable]
    public class ShootActionInfo : ActionInfo
    {
        /// <summary>
        /// Construct ShootActionInfo from information in an XML element
        /// </summary>
        /// <param name="actionElement">XML element holding data to construct ShootActionInfo</param>
        public ShootActionInfo(XPathNavigator actionElement)
            : base(actionElement, true)
        {
            this.name = Util.LoadString(Util.GetStringAttribute(actionElement, "name"));
            this.accuracy = Util.GetFloatAttribute(actionElement, "accuracy");
            this.range = Util.GetFloatAttribute(actionElement, "range");
            this.shots = Util.GetIntAttribute(actionElement, "shots");
        }

        /// <summary>Can we even show this action on the list of available actions?</summary>
        /// <param name="battlescape">the battlescape</param>
        /// <param name="item">item doing the action</param>
        /// <param name="combatant">combatant holding the item</param>
        /// <returns>Why action isn't possible, (or ActionError.None if action possible)</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
            Justification = "Will throw if item is null")]
        public override ActionError ActionAvailable(Battle battlescape, Item item, Combatant combatant)
        {
            if (!item.HasEnoughAmmo(shots))
            {
                return ActionError.InsufficientAmmo;
            }

            // check combatant has sufficient TUs.
            return base.ActionAvailable(battlescape, item, combatant);
        }

        /// <summary>Can action be performed using given location</summary>
        /// <param name="battlescape">the battlescape</param>
        /// <param name="item">item doing the action</param>
        /// <param name="combatant">combatant holding the item</param>
        /// <param name="target">Location the action will effect</param>
        /// <returns>Why action isn't possible, (or ActionError.None if action possible)</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
            Justification = "Will throw if battlescape or combatant is null")]
        public override ActionError IsLocationOk(Battle battlescape, Item item, Combatant combatant, Vector3 target)
        {
            // check target is within range
            if ((range * range) < Vector3.DistanceSquared(combatant.Position, target))
            {
                return ActionError.OutOfRange;
            }

            // check have line of fire on target
            Vector3 dummy = new Vector3();
            if (!battlescape.Terrain.IsLineOfSight(combatant.Position, target, VisibilityChecks.LineOfFire, ref dummy))
            {
                return ActionError.NoLineOfSight;
            }

            // Check soldier is looking in direction of target.  Ignore Vertical component.
            Vector3 diff = target - combatant.Position;
            diff.Y = 0;
            diff.Normalize();
            if (Vector3.Dot(diff, combatant.HeadingVector) < Combatant.DotFieldOfView)
            {
                return ActionError.WrongDirection;
            }

            // it's all good
            return ActionError.None;
        }

        /// <summary>Start the action</summary>
        /// <param name="battlescape">the battlescape</param>
        /// <param name="item">item doing the action</param>
        /// <param name="combatant">combatant holding the item</param>
        /// <param name="target">Location the action will effect</param>
        /// <param name="activeArm">The arm used for the action</param>
        /// <returns>Order object that will perform the action</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
            Justification = "Will throw if item is null")]
        public override Order Start(Battle battlescape, Item item, Combatant combatant, Vector3 target,
            Combatant.ActiveArm activeArm)
        {
            DebitTus(combatant);
            item.ConsumeAmmo(shots);

            // create shoot order
            return new ShootOrder(combatant, battlescape, target, item, this, activeArm);
        }

        /// <summary>Add stats specific to this item type to string collection for display on X-Net</summary>
        /// <param name="stats">string collection to append strings to</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
            Justification = "Will throw if stats is null")]
        public override void XNetStatistics(StringCollection stats)
        {
            Debug.Assert(Duration < 1.0f);
            stats.Add(Util.StringFormat(Strings.ITEM_STATS_SHOOT_ACTION, name, (int)(accuracy * 100), (int)(Duration * 100)));
        }

        /// <summary>String to put in action selection menu</summary>
        /// <param name="combatant">combatant who will do the action</param>
        /// <param name="activeArm">The arm used for the action</param>
        /// <returns>Text to put on menu entry</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
            Justification = "Will throw if combatant is null")]
        public override string MenuEntry(Combatant combatant, Combatant.ActiveArm activeArm)
        {
            return Util.StringFormat(Strings.ACTION_SHOOT_MENU_ENTRY,
                name,
                ChanceToHit(combatant, activeArm),
                TusNeeded(combatant.Stats[Statistic.TimeUnits]));
        }

        /// <summary>Odds of a specific combatant being able to hit a target</summary>
        /// <param name="combatant">the combatant</param>
        /// <param name="activeArm">The arm used for the action</param>
        /// <returns>percentage chance of success</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
           Justification = "Will throw if combatant is null")]
        public int ChanceToHit(Combatant combatant, Combatant.ActiveArm activeArm)
        {
            return (int)(accuracy * combatant.Accuracy(activeArm));
        }

        #region Fields

        /// <summary>number of times to fire weapon</summary>
        public int Shots { get { return shots; } }

        /// <summary>Name of Shot type</summary>
        private string name;

        /// <summary>Base "To Hit" probability</summary>
        private float accuracy;

        /// <summary>Maximum range, in meters</summary>
        private float range;

        /// <summary>number of times to fire weapon</summary>
        private int shots;

        #endregion Fields
    }

}
