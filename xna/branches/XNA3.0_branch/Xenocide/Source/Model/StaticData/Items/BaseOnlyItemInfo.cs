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
* @file BaseOnlyItemInfo.cs
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


#endregion

namespace ProjectXenocide.Model.StaticData.Items
{
    /// <summary>
    /// An item that only exists within an outpost, or in transit between outposts
    /// </summary>
    [Serializable]
    public class BaseOnlyItemInfo : ItemInfo
    {
        /// <summary>
        /// Construct a BaseOnlyItemInfo from an XML file
        /// <remarks>Really, this class only exists so that Item class can remain abstract</remarks>
        /// </summary>
        /// <param name="itemNode">XML node holding data to construct Item</param>
        /// <param name="manager">Namespace used in item.xml</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
           Justification = "will throw if itemNode == null")]
        public BaseOnlyItemInfo(XPathNavigator itemNode, XmlNamespaceManager manager)
            : base(itemNode, manager)
        {
        }

        /// <summary>
        /// Add stats specific to this facility type to string collection for display on X-Net
        /// </summary>
        /// <param name="stats">string collection to append strings to</param>
        protected override void XNetStatisticsCore(StringCollection stats) {}
    }
}
