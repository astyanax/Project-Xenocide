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
* @file CraftWeaponItemInfo.cs
* @date Created: 2007/06/17
* @author File creator: David Teviotdale
* @author Credits: nil
*/
#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using System.Text;
using System.Diagnostics;
using System.Drawing;
using System.Xml;
using System.Xml.XPath;


using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Storage;

using ProjectXenocide.Utils;

using ProjectXenocide.Model.Geoscape.Vehicles;
using Xenocide.Resources;


#endregion

namespace ProjectXenocide.Model.StaticData.Items
{
    /// <summary>
    /// The non-changing information about a weapon used to arm X-Corp aircraft
    /// </summary>
    [Serializable]
    public class CraftWeaponItemInfo : ItemInfo
    {
        /// <summary>
        /// Construct a CraftWeaponItemInfo from an XML file
        /// </summary>
        /// <param name="itemNode">XML node holding data to construct CraftWeaponItemInfo</param>
        /// <param name="manager">Namespace used in item.xml</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
           Justification = "will throw if itemNode == null")]
        public CraftWeaponItemInfo(XPathNavigator itemNode, XmlNamespaceManager manager)
            : base(itemNode, manager)
        {
            // clip (ammo) element.  N.B. not all weapons use ammo
            if (null != AmmoInfo)
            {
                Debug.Assert(1 == AmmoInfo.Ammos.Count);
                this.clip = Xenocide.StaticTables.ItemList[AmmoInfo.Ammos[0]] as ClipItemInfo;
                this.clipSize = AmmoInfo.Capacity;
                weaponDamage = Clip.WeaponDamage;
            }
            else
            {
                // if weapon doesn't have clips, get the damage element
                XPathNavigator damage = itemNode.SelectSingleNode("i:damage", manager);
                weaponDamage = Util.GetIntAttribute(damage, "amount");
            }

            // "shoot" element
            XPathNavigator shoot = itemNode.SelectSingleNode("i:shoot", manager);
            timeToShoot = Util.GetIntAttribute(shoot, "time");
            accuracy = Util.GetDoubleAttribute(shoot, "accuracy");
            weaponRange = Util.GetIntAttribute(shoot, "range");
        }

        #region Methods

        /// <summary>
        /// Add stats specific to this Weapon to string collection for display on X-Net
        /// </summary>
        /// <param name="stats">string collection to append strings to</param>
        protected override void XNetStatisticsCore(StringCollection stats)
        {
            if (null == AmmoInfo)
            {
                stats.Add(Util.StringFormat(Strings.ITEM_STATS_CRAFT_CLIP_SIZE, ClipSizeString()));
                stats.Add(Util.StringFormat(Strings.ITEM_STATS_CRAFT_CLIP_DAMAGE, WeaponDamage));
            }
            stats.Add(Util.StringFormat(Strings.ITEM_STATS_CRAFT_WEAPON_RANGE, weaponRange / 1000));
            stats.Add(Util.StringFormat(Strings.ITEM_STATS_CRAFT_WEAPON_ACCURACY, Accuracy));
        }

        /// <summary>
        /// Return string (for display to user) giving ammunition carried by weapon
        /// </summary>
        /// <returns>String to display</returns>
        public String ClipSizeString()
        {
            return (Clip == null) ? Strings.CLIP_UNLIMITED : Util.StringFormat("{0}", ClipSize);
        }

        /// <summary>
        /// Construct a WeaponPod for an item of this type, in the state it
        /// would be if just purchased
        /// </summary>
        /// <returns>constructed WeaponPod</returns>
        public override Item Manufacture()
        {
            return new WeaponPod(this);
        }

        #endregion

        #region Fields

        /// <summary>
        /// The type of Ammo used by this weapon
        /// </summary>
        public ClipItemInfo Clip { get { return clip; } }

        /// <summary>
        /// Max rounds weapon can carry internally (if it uses ammo)
        /// </summary>
        public int ClipSize { get { return clipSize; } }

        /// <summary>
        /// Time (in TUs) to take a shot with this weapon
        /// </summary>
        public int TimeToShoot { get { return timeToShoot; } }

        /// <summary>
        /// Probability shot will hit target
        /// </summary>
        public double Accuracy { get { return accuracy; } }

        /// <summary>
        /// range (in meters) of the weapon
        /// </summary>
        public int WeaponRange { get { return weaponRange; } }

        /// <summary>
        /// damage (in hull points) inflicted by weapon
        /// </summary>
        public int WeaponDamage { get { return weaponDamage; } }

        /// <summary>
        /// The ammunition used by this weapon
        /// </summary>
        private ClipItemInfo clip;

        /// <summary>
        /// Max rounds weapon can carry internally (if it uses ammo)
        /// </summary>
        private int clipSize;

        /// <summary>
        /// Time (in TUs) to take a shot with this weapon
        /// </summary>
        private int timeToShoot;

        /// <summary>
        /// Probability shot will hit target
        /// </summary>
        private double accuracy;

        /// <summary>
        /// range (in meters) of the weapon
        /// </summary>
        private int weaponRange;

        /// <summary>
        /// damage (in hull points) inflicted by weapon
        /// </summary>
        private int weaponDamage;

        #endregion
    }
}
