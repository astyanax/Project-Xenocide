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
* @file UnitTestItemHandle.cs
* @date Created: 2007/11/24
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


using ProjectXenocide.Model.Geoscape.Outposts;

#endregion

namespace ProjectXenocide.Model.StaticData.Items
{
    /// <summary>
    /// Unit tests for Item
    /// </summary>
    public partial class Item
    {
        #region UnitTests

        /// <summary>
        /// Run set of tests
        /// </summary>
        [Conditional("DEBUG")]
        public static void RunItemTests()
        {
            Item pistol  = Xenocide.StaticTables.ItemList["ITEM_PISTOL_CLIP"].Manufacture();
            Item heClip1 = Xenocide.StaticTables.ItemList["ITEM_R_C_HE_CLIP"].Manufacture();
            Item heClip2 = Xenocide.StaticTables.ItemList["ITEM_R_C_HE_CLIP"].Manufacture();
            Item heClip3 = Xenocide.StaticTables.ItemList["ITEM_R_C_HE_CLIP"].Manufacture();
            Item apClip = Xenocide.StaticTables.ItemList["ITEM_R_C_AP_CLIP"].Manufacture();
            Item cannon  = Xenocide.StaticTables.ItemList["ITEM_REPEATER_CANNON"].Manufacture();
            Item laser  = Xenocide.StaticTables.ItemList["ITEM_LASER_PISTOL"].Manufacture();

            // Test IsAmmoValid()
            Debug.Assert(pistol.IsAmmoValid(pistol));
            Debug.Assert(!cannon.IsAmmoValid(pistol));
            Debug.Assert(cannon.IsAmmoValid(heClip1));
            Debug.Assert(cannon.IsAmmoValid(apClip));
            Debug.Assert(!apClip.IsAmmoValid(heClip1));
            Debug.Assert(!cannon.IsAmmoValid(laser));
            Debug.Assert(!laser.IsAmmoValid(cannon));

            // Try loading empty cannon
            Debug.Assert(0 == cannon.ShotsLeft);
            Debug.Assert(null == cannon.Load(apClip));
            Debug.Assert(14 == cannon.ShotsLeft);
            Debug.Assert("ITEM_R_C_AP_CLIP" == cannon.AmmoInfo.Id);
            Debug.Assert(0 == apClip.ShotsLeft);

            // swap ammo
            Item ejectedClip = cannon.Load(heClip1);
            Debug.Assert(14 == ejectedClip.ShotsLeft);
            Debug.Assert("ITEM_R_C_AP_CLIP" == ejectedClip.ItemInfo.Id);
            Debug.Assert(14 == cannon.ShotsLeft);
            Debug.Assert("ITEM_R_C_HE_CLIP" == cannon.AmmoInfo.Id);

            // partial clip
            cannon.ShotsLeft = 7;
            Debug.Assert(heClip2 == cannon.Load(heClip2));
            Debug.Assert("ITEM_R_C_HE_CLIP" == cannon.AmmoInfo.Id);
            Debug.Assert(14 == cannon.ShotsLeft);
            Debug.Assert(7 == heClip2.ShotsLeft);

            // load a clip from a clip
            Debug.Assert(heClip3 == heClip2.Load(heClip3));
            Debug.Assert(14 == heClip2.ShotsLeft);
            Debug.Assert(7 == heClip3.ShotsLeft);
        }

        /// <summary>
        /// Convert item into a different item
        /// </summary>
        /// <param name="newItemInfo">New item type</param>
        /// <remarks>This is intended for Unit Testing, mostly to change the type of a UFO</remarks>
        [Conditional("DEBUG")]
        public void DebugTransmute(ItemInfo newItemInfo)
        {
            itemInfo = newItemInfo;
        }

        #endregion UnitTests
    }
}
