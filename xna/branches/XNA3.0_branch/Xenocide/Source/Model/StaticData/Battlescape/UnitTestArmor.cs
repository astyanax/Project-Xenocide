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
* @file UnitTestArmor.cs
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

using ProjectXenocide.Model.Battlescape;
using ProjectXenocide.Model.StaticData.Items;


#endregion

namespace ProjectXenocide.Model.StaticData.Battlescape
{
    /// <summary>
    /// Unit tests for Armor
    /// </summary>
    public partial class Armor
    {
        #region UnitTests

        /// <summary>
        /// Run set of tests
        /// </summary>
        [Conditional("DEBUG")]
        public static void RunTests()
        {
            Armor armor = Xenocide.StaticTables.ArmorList["Silabrate"];

            // should be absorbed by armor
            Vector2 damage = armor.DamageInflicted(new DamageInfo(18, DamageType.Piercing), Side.Front);
            Debug.Assert(0 == damage.X);
            Debug.Assert(0 == damage.Y);

            // partially absorbed
            damage = armor.DamageInflicted(new DamageInfo(18, DamageType.Piercing), Side.Under);
            Debug.Assert(8 == damage.X);
            Debug.Assert(1 == damage.Y);

            // damage type modifier
            damage = armor.DamageInflicted(new DamageInfo(18, DamageType.Fire), Side.Under);
            Debug.Assert(0 == damage.X);
            Debug.Assert(1 == damage.Y);
        }

        #endregion UnitTests
    }
}
