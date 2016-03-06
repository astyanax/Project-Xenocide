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
* @file Order.cs
* @date Created: 2008/02/06
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
    /// The ways an order can finish
    /// </summary>
    public enum FinishCode
    {
        /// <summary>The order is still running</summary>
        Executing,

        /// <summary>Order ran to completion</summary>
        Normal,

        /// <summary>Something happened to prevent the order finishing as expected</summary>
        Interupted,
    }

    /// <summary>
    /// Instruction telling Combatant to perform an action
    /// </summary>
    public abstract class Order
    {
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="combatant">The combatant performing the order</param>
        /// <param name="battlescape">the combatant's environment</param>
        protected Order(Combatant combatant, Battle battlescape)
        {
            this.combatant   = combatant;
            this.battlescape = battlescape;
        }

        /// <summary>
        /// Spend time performing the order
        /// </summary>
        /// <param name="seconds">time to update order's progress by</param>
        public abstract void Update(double seconds);

        #region Fields

        /// <summary>The combatant performing the order</summary>
        public Combatant Combatant { get { return combatant; } }

        /// <summary>the combatant's environment</summary>
        public Battle Battlescape { get { return battlescape; } }

        /// <summary>What is condition of this order?</summary>
        public FinishCode Finished { get { return finished; } protected set { finished = value; } }

        /// <summary>The combatant performing the order</summary>
        private Combatant combatant;

        /// <summary>the combatant's environment</summary>
        private Battle battlescape;

        /// <summary>What is condition of this order?</summary>
        private FinishCode finished;

        #endregion
    }
}
