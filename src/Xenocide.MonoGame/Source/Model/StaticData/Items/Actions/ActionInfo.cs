using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Xml.XPath;

using ProjectXenocide.Utils;

namespace ProjectXenocide.Model.StaticData.Items
{
    /// <summary>
    /// Metadata describing an action an item can perform.
    /// </summary>
    /// <remarks>
    /// The tactical execution (availability/location checks and spawning battle
    /// orders) was removed together with the battlescape.  This class now only
    /// carries the information the X-Net displays.
    /// </remarks>
    [Serializable]
    public abstract class ActionInfo
    {
        /// <summary>
        /// Construct ActionInfo from information in an XML element
        /// </summary>
        /// <param name="actionElement">XML element holding data to construct ActionInfo</param>
        /// <param name="needsLocation">Is a location on the battlefield needed to perform the action?</param>
        protected ActionInfo(XPathNavigator actionElement, bool needsLocation)
        {
            string timeName = Util.AttributePresent(actionElement, "percentage") ? "percentage" : "time";
            this.duration = Util.GetFloatAttribute(actionElement, timeName);
            this.needsLocation = needsLocation;
        }

        /// <summary>Add stats specific to this item type to string collection for display on X-Net</summary>
        /// <param name="stats">string collection to append strings to</param>
        public virtual void XNetStatistics(StringCollection stats) { }

        /// <summary>Create ActionInfo (or derived class) from information in XML file</summary>
        /// <param name="actionElement">XML element holding data to construct ActionInfo</param>
        /// <returns>The constructed ActionInfo, or null if unsupported type</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
            Justification = "Will throw if actionElement is null")]
        public static ActionInfo Factory(XPathNavigator actionElement)
        {
            string type = actionElement.Name;
            Debug.Assert(!String.IsNullOrEmpty(type));
            if ("shoot" == type)
            {
                return new ShootActionInfo(actionElement);
            }

            // ToDo: throw, hit, prime, guide and scan/psi actions are not implemented.
            return null;
        }

        #region Fields

        /// <summary>Time taken to perform action</summary>
        /// <remarks>if less than 1.0, then is % of combatant's max TUs, if greater than 1.0, then is TUs</remarks>
        public float Duration { get { return duration; } }

        /// <summary>Is a location on the battlefield needed to perform the action?</summary>
        public bool NeedsLocation { get { return needsLocation; } }

        private float duration;
        private bool needsLocation;

        #endregion Fields
    }
}
