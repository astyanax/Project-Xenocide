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
* @file EquipSoldierScreenBattlescapeItemSource.cs
* @date Created: 2008/03/02
* @author File creator: David Teviotdale
* @author Credits: none
*/
#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;


using ProjectXenocide.Utils;
using ProjectXenocide.Model.StaticData.Items;
//using ProjectXenocide.Model.Geoscape;
using ProjectXenocide.Model.Battlescape;
using ProjectXenocide.Model.Battlescape.Combatants;
using ProjectXenocide.UI.Dialogs;
using ProjectXenocide.UI.Scenes;
#endregion

namespace ProjectXenocide.UI.Screens
{
    /// <summary>
    /// This is the screen that lets player set the items a soldier carries
    /// </summary>
    public partial class EquipSoldierScreen
    {
        /// <summary>
        /// Represents the battlescape where we're getting items to add to a soldier (or where we're dumping the items
        /// removed from the soldier)
        /// </summary>
        private class BattlescapeItemSource : ItemSource
        {
            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="battlescape">The battlescape being used as item source/sink</param>
            /// <param name="position">location on the battlescape to get items from/put item</param>
            public BattlescapeItemSource(Battle battlescape, Vector3 position)
            {
                this.battlescape = battlescape;
                this.position    = position;
                BuildItemList();
            }

            /// <summary>Build list of items to show on "Ground" area of screen</summary>
            private void BuildItemList()
            {
                ItemList.Clear();
                foreach (Item item in battlescape.ListGroundContents(position))
                {
                    ItemList.Add(new ItemEntry(item, 1));
                }
            }

            /// <summary>Remove an item from Source</summary>
            /// <param name="item">item to remove</param>
            /// <returns>removed item</returns>
            protected override Item Remove(Item item)
            {
                Debug.Assert(null != item);
                battlescape.RemoveFromGround(item, position);
                return item;
            }

            /// <summary>Put an item back into the store</summary>
            /// <param name="item">Item being dropped</param>
            public override void ReplaceItem(Item item)
            {
                Debug.Assert(null != item);
                battlescape.AddToGround(item, position);
                BuildItemList();
            }

            #region Fields

            /// <summary>The battlescape being used as item source/sink</summary>
            private Battle battlescape;

            /// <summary>location on the battlescape to get items from/put item</summary>
            public Vector3 position;

            #endregion Fields
        }
    }
}
