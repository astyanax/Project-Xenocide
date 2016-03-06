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
* @file Cheats.cs
* @date Created: 2007/08/13
* @author File creator: dteviot
* @author Credits: none
*/
#endregion

#region Using Statements

using System;
using System.Collections;
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
using ProjectXenocide.Model.Geoscape.Outposts;
using ProjectXenocide.Model.StaticData.Items;
using ProjectXenocide.Model.Geoscape.Vehicles;

#endregion

namespace ProjectXenocide.Model.StaticData
{
    /// <summary>
    /// Hold the list of the "cheat" settings
    /// </summary>
    public sealed class Cheats
    {
        /// <summary>
        /// Construct from XML node
        /// </summary>
        /// <param name="cheatsNode">XML node holding data</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
            Justification = "will throw if cheatNode == null")]
        public Cheats(XPathNavigator cheatsNode)
        {
            showAllXNetEntries = Util.GetBoolAttribute(cheatsNode, "showAllXNetEntries");
            showUndetectedUfos = Util.GetBoolAttribute(cheatsNode, "showUndetectedUfos");
            controlAlienMissions = Util.GetBoolAttribute(cheatsNode, "controlAlienMissions");
            xcorpCantLooseAtStartOfMonth = Util.GetBoolAttribute(cheatsNode, "xcorpCantLooseAtStartOfMonth");
            showAllAliens        = Util.GetBoolAttribute(cheatsNode, "showAllAliens");
            playerControlsAliens = Util.GetBoolAttribute(cheatsNode, "playerControlsAliens");
        }

        #region Fields

        /// <summary>
        /// Show all entries in X-Net regardless of if they're discovered or not?
        /// </summary>
        public bool ShowAllXNetEntries { get { return showAllXNetEntries; } }

        /// <summary>
        /// Show UFOs that are not on a radar on the Geoscape?
        /// </summary>
        public bool ShowUndetectedUfos { get { return showUndetectedUfos; } }

        /// <summary>
        /// Show alien mission button on Geoscape?
        /// </summary>
        public bool ControlAlienMissions { get { return controlAlienMissions; } }

        /// <summary>
        /// X-Corp won't loose the game if have a low score or big debit at end of month
        /// </summary>
        /// <remarks>Writable because we disable for unit tests</remarks>
        public bool XcorpCantLooseAtStartOfMonth
        {
            get { return xcorpCantLooseAtStartOfMonth; }
            set { xcorpCantLooseAtStartOfMonth = value; }
        }

        /// <summary>Show aliens on battlescape, regardless of being in line of sight of an X-Corp soldier or not.</summary>
        public bool ShowAllAliens { get { return showAllAliens; } }

        /// <summary>Can the player control the battlescape alien forces the same way the player can control the X-Corp forces?</summary>
        public bool PlayerControlsAliens { get { return playerControlsAliens; } }

        /// <summary>
        /// Show all entries in X-Net regardless of if they're discovered or not?
        /// </summary>
        private bool showAllXNetEntries;

        /// <summary>
        /// Show UFOs that are not on a radar on the Geoscape?
        /// </summary>
        private bool showUndetectedUfos;

        /// <summary>
        /// Show alien mission button on Geoscape?
        /// </summary>
        private bool controlAlienMissions;

        /// <summary>
        /// X-Corp won't loose the game if have a low score or big debit at end of month
        /// </summary>
        private bool xcorpCantLooseAtStartOfMonth;

        /// <summary>Show aliens on battlescape, regardless of being in line of sight of an X-Corp soldier or not.</summary>
        private bool showAllAliens;

        /// <summary>Can the player control the battlescape alien forces the same way the player can control the X-Corp forces?</summary>
        private bool playerControlsAliens;

        #endregion Fields
    }
}
