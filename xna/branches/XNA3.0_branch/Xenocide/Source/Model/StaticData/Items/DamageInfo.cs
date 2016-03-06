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
* @file DamageInfo.cs
* @date Created: 2008/02/25
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

using ProjectXenocide.Model.Battlescape;

#endregion

namespace ProjectXenocide.Model.StaticData.Items
{
    /// <summary>
    /// The damage that can be done by a weapon or ammunition
    /// </summary>
    [Serializable]
    public class DamageInfo
    {
        /// <summary>Construct a DamageInfo from an XML file</summary>
        /// <param name="damageNode">XML node holding data to construct DamageInfo</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
           Justification = "will throw if damageNode == null")]
        public DamageInfo(XPathNavigator damageNode)
        {
            points     = Util.GetIntAttribute(damageNode, "amount");
            damageType = Util.ParseEnum<DamageType>(Util.GetStringAttribute(damageNode, "type"));
        }

        /// <summary>Construct a DamageInfo from an XML file</summary>
        /// <param name="points">damage (in points) inflicted by weapon</param>
        /// <param name="damageType">Type of damage done</param>
        public DamageInfo(int points, DamageType damageType)
        {
            this.points     = points;
            this.damageType = damageType;
        }

        #region Fields

        /// <summary>damage (in points) inflicted by weapon</summary>
        public int Points { get { return points; } }

        /// <summary>Type of damage done</summary>
        public DamageType DamageType { get { return damageType; } }

        /// <summary>damage (in points) inflicted by weapon</summary>
        private int points;

        /// <summary>Type of damage done</summary>
        private DamageType damageType;

        #endregion
    }
}
