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
* @file ActionInfo.cs
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
using ProjectXenocide.Model.Battlescape.Combatants;
using ProjectXenocide.Model.Battlescape;
using Xenocide.Resources;
//using Xenocide.Resources;

#endregion

namespace ProjectXenocide.Model.StaticData.Items
{
    /// <summary>Reasons why an Action may not be possible</summary>
    public enum ActionError
    {
        /// <summary>No error, action can start</summary>
        None,

        /// <summary>Combatant doesn't have enough time</summary>
        InsuffientTUs,

        /// <summary>Device doesn't have enough ammo</summary>
        InsufficientAmmo,

        /// <summary>Distance is to far</summary>
        OutOfRange,

        /// <summary>Target is not in sight of combatant</summary>
        NoLineOfSight,

        /// <summary>Soldier is not facing target</summary>
        WrongDirection,
    }

    /// <summary>
    /// The base class, describing an action a battlescape item can perform
    /// </summary>
    [Serializable]
    public abstract class ActionInfo
    {
        /// <summary>
        /// Construct ActionInfo from information in an XML element
        /// </summary>
        /// <param name="actionElement">XML element holding data to construct ActionInfo</param>
        /// <param name="needsLocation">Is a location on the battlefield is needed to perform action?</param>
        protected ActionInfo(XPathNavigator actionElement, bool needsLocation)
        {
            string timeName    = Util.AttributePresent(actionElement, "percentage") ? "percentage" : "time";
            this.duration      = Util.GetFloatAttribute(actionElement, timeName);
            this.startSound    = Util.GetStringAttribute(actionElement, "startSound");
            this.needsLocation = needsLocation;
        }

        /// <summary>Number of TUs needed to perform action</summary>
        /// <param name="maxTus">Number of TUs combatant gets each turn</param>
        public int TusNeeded(int maxTus)
        {
            return (int)((duration < 1.0f) ? (maxTus * duration) : duration);
        }

        /// <summary>Can we even show this action on the list of available actions?</summary>
        /// <param name="battlescape">the battlescape</param>
        /// <param name="item">item doing the action</param>
        /// <param name="combatant">combatant holding the item</param>
        /// <returns>Why action isn't possible, (or ActionError.None if action possible)</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
            Justification = "Will throw if combatant is null")]
        public virtual ActionError ActionAvailable(Battle battlescape, Item item, Combatant combatant)
        {
            if (combatant.Stats[Statistic.TimeUnitsLeft] < TusNeeded(combatant.Stats[Statistic.TimeUnits]))
            {
                return ActionError.InsuffientTUs;
            }
            else
            {
                return ActionError.None;
            }
        }

        /// <summary>Can action be performed using given location</summary>
        /// <param name="battlescape">the battlescape</param>
        /// <param name="item">item doing the action</param>
        /// <param name="combatant">combatant holding the item</param>
        /// <param name="target">Location the action will effect</param>
        /// <returns>Why action isn't possible, (or ActionError.None if action possible)</returns>
        public abstract ActionError IsLocationOk(Battle battlescape, Item item, Combatant combatant, Vector3 target);

        /// <summary>Start the action if it doesn't need a location</summary>
        /// <param name="battlescape">the battlescape</param>
        /// <param name="item">item doing the action</param>
        /// <param name="combatant">combatant holding the item</param>
        /// <returns>Order object that will perform the action</returns>
        /// <remarks>A derived class should only implement ONE of the Start() functions</remarks>
        public virtual Order Start(Battle battlescape, Item item, Combatant combatant)
        {
            // if not overriden, derived class's Start() needs a location
            Debug.Assert(false);
            return null;
        }

        /// <summary>Start the action if it needs a location</summary>
        /// <param name="battlescape">the battlescape</param>
        /// <param name="item">item doing the action</param>
        /// <param name="combatant">combatant holding the item</param>
        /// <param name="target">Location the action will effect</param>
        /// <returns>Order object that will perform the action</returns>
        /// <remarks>A derived class should only implement ONE of the Start() functions</remarks>
        public virtual Order Start(Battle battlescape, Item item, Combatant combatant, Vector3 target)
        {
            // if not overriden, derived class's Start() does not need a location
            Debug.Assert(false);
            return null;
        }

        /// <summary>Remove the time needed to perform this action from combatant's avaialble TUs</summary>
        /// <param name="combatant">combatant to debit</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
            Justification = "Will throw if combatant is null")]
        protected void DebitTus(Combatant combatant)
        {
            combatant.Stats[Statistic.TimeUnitsLeft] -= TusNeeded(combatant.Stats[Statistic.TimeUnits]);
            Debug.Assert(0 <= combatant.Stats[Statistic.TimeUnitsLeft]);
        }

        /// <summary>Add stats specific to this item type to string collection for display on X-Net</summary>
        /// <param name="stats">string collection to append strings to</param>
        public virtual void XNetStatistics(StringCollection stats) {}

        /// <summary>String to put in action selection menu</summary>
        /// <param name="combatant">combatant who will do the action</param>
        public abstract string MenuEntry(Combatant combatant);

        /// <summary>Play sound associated with starting this action</summary>
        public void PlayStartSound()
        {
            if (!String.IsNullOrEmpty(startSound))
            {
                Xenocide.AudioSystem.PlaySound(startSound);
            }
        }

        /// <summary>Create ActionInfo (or derived class) from information in XML file</summary>
        /// <param name="actionElement">XML element holding data to construct ActionInfo</param>
        /// <returns>The constructed ActionInfo, or null if unsuported type</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
            Justification="Will throw if actionElement is null")]
        public static ActionInfo Factory(XPathNavigator actionElement)
        {
            string type = actionElement.Name;
            Debug.Assert(!String.IsNullOrEmpty(type));
            if ("shoot" == type)
            {
                return new ShootActionInfo(actionElement);
            }
            else if ("throw" == type)
            {
                //ToDo: implement
                return null;
            }
            else if ("hit" == type)
            {
                //ToDo: implement
                return null;
            }
            else if ("prime" == type)
            {
                //ToDo: implement
                return null;
            }
            else if ("guide" == type)
            {
                //ToDo: implement
                return null;
            }
            else if ("action" == type)
            {
                //ToDo: implement
                // need to check action name, will be one of
                /*
                ACTION_SCAN
                ACTION_USE_PSIONIC_PROBE
                ACTION_PANIC_UNIT
                ACTION_MIND_CONTROL
                */
                return null;
            }
            else
            {
                Debug.Assert(false);
                return null;
            }
        }

        /// <summary>Convert an ActionError into text to show to user</summary>
        /// <param name="errorCode">The ActionError to get text for</param>
        /// <returns>message for the error</returns>
        public static string ErrorText(ActionError errorCode)
        {
            return displayStrings[(int)errorCode];
        }

        /// <summary>String used for each ActionError</summary>
        private static readonly String[] displayStrings = 
        {
            Strings.ACTION_ERROR_NONE,
            Strings.ACTION_ERROR_INSUFFICENT_TUS,
            Strings.ACTION_ERROR_INSUFFICENT_AMMO,
            Strings.ACTION_ERROR_OUT_OF_RANGE,
            Strings.ACTION_ERROR_NO_LINE_OF_SIGHT,
            Strings.ACTION_ERROR_NOT_FACING_TARGET
        };

        #region Fields

        /// <summary>Time taken to perform action</summary>
        /// <remarks>if less than 1.0, then is % of combatant's max TUs, if greater than 1.0, then is TUs</remarks>
        public float Duration { get { return duration; } }

        /// <summary>Is a location on the battlefield is needed to perform action?</summary>
        public bool NeedsLocation { get { return needsLocation; } }

        /// <summary>Time taken to perform action</summary>
        /// <remarks>if less than 1.0, then is % of combatant's max TUs, if greater than 1.0, then is TUs</remarks>
        private float duration;

        /// <summary>Sound (if any) to play when action begins</summary>
        private string startSound;

        /// <summary>Is a location on the battlefield is needed to perform action?</summary>
        private bool needsLocation;

        #endregion Fields
    }

}
