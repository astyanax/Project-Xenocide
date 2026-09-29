using System;
using System.Collections.Specialized;
using System.Xml.XPath;

using ProjectXenocide.Utils;

using Xenocide.Resources;

namespace ProjectXenocide.Model.StaticData.Items
{
    /// <summary>
    /// Details of shooting a weapon (metadata only — tactical execution removed
    /// with the battlescape).  Used for the X-Net display.
    /// </summary>
    [Serializable]
    public class ShootActionInfo : ActionInfo
    {
        /// <summary>
        /// Construct ShootActionInfo from information in an XML element
        /// </summary>
        /// <param name="actionElement">XML element holding data to construct ShootActionInfo</param>
        public ShootActionInfo(XPathNavigator actionElement)
            : base(actionElement)
        {
            this.name = Util.LoadString(Util.GetStringAttribute(actionElement, "name"));
            this.accuracy = Util.GetFloatAttribute(actionElement, "accuracy");
        }

        /// <summary>Add stats specific to this item type to string collection for display on X-Net</summary>
        /// <param name="stats">string collection to append strings to</param>
        public override void XNetStatistics(StringCollection stats)
        {
            stats.Add(Util.StringFormat(Strings.ITEM_STATS_SHOOT_ACTION, name, (int)(accuracy * 100), (int)(Duration * 100)));
        }

        #region Fields

        /// <summary>Name of shot type</summary>
        private string name;

        /// <summary>Base "To Hit" probability (as a fraction of the combatant's accuracy)</summary>
        private float accuracy;

        #endregion Fields
    }
}
