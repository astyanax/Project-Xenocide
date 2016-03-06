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

            // Setup random generator
            List<int> randomNumbers = new List<int>();
            randomNumbers.Add(24); // Damage, absorbed
            randomNumbers.Add(23); // Damage, partially absorbed
            randomNumbers.Add(23); // Damage, type modifier
            Xenocide.Rng.RigDice(randomNumbers);

            // should be absorbed by armor
            Vector2 damage = armor.DamageInflicted(new DamageInfo(18, DamageType.Piercing), Side.Front);
            Debug.Assert(0 == damage.X);
            Debug.Assert(0 == damage.Y);

            // partially absorbed
            damage = armor.DamageInflicted(new DamageInfo(18, DamageType.Piercing), Side.Under);
            Debug.Assert(damage.X == 14);
            Debug.Assert(damage.Y == 1);

            // damage type modifier
            damage = armor.DamageInflicted(new DamageInfo(18, DamageType.Fire), Side.Under);
            Debug.Assert(0 == damage.X);
            Debug.Assert(damage.Y == 1);
        }

        #endregion UnitTests
    }
}
