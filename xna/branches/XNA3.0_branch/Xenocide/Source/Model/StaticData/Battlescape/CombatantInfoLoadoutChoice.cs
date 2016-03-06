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
* @file CombatantInfoLoadoutChoice.cs
* @date Created: 2008/02/11
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
using System.Xml;
using System.Xml.XPath;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Storage;


using ProjectXenocide.Utils;

#endregion

namespace ProjectXenocide.Model.StaticData.Battlescape
{
    /// <summary>
    /// Creates Combatants
    /// </summary>
    public partial class CombatantInfo
    {
        /// <summary>
        /// a possible loadout this type of combatant may come equiped with
        /// </summary>
        [Serializable]
        private class LoadoutChoice : IOdds
        {
            /// <summary>
            /// Ctor
            /// </summary>
            /// <param name="node">XML node holding data to construct LoadoutInfo</param>
            public LoadoutChoice(XPathNavigator node)
            {
                this.loadoutName = Util.GetStringAttribute(node, "loadout");
                this.odds        = Util.GetIntAttribute(   node, "odds");
            }

            /// <summary>Name of the loadout</summary>
            public string LoadoutName { get { return loadoutName; } }

            /// <summary>relative odds of receiving this loadout</summary>
            public int Odds { get { return odds; } }

            /// <summary>Name of the loadout</summary>
            private  string loadoutName;

            /// <summary>relative odds of receiving this loadout</summary>
            private  int odds;
        }
    }
}
