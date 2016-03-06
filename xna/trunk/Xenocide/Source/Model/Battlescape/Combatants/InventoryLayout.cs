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
* @file InventoryLayout.cs
* @date Created: 2007/10/22
* @author File creator: David Teviotdale
* @author Credits: none
*/
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

using Microsoft.Xna.Framework;

using ProjectXenocide.Utils;
using ProjectXenocide.Model.StaticData.Items;

namespace ProjectXenocide.Model.Battlescape.Combatants
{
    /// <summary>
    /// How a combatant's inventory system is laid out
    /// </summary>
    public sealed class InventoryLayout
    {
        /// <summary>
        /// Available InventoryLayout configurations
        /// </summary>
        public enum Config
        {
            /// <summary>Human soldier's inventory configuration</summary>
            Humanoid,

            /// <summary>Items are fixed, and cant' be viewed or changed. e.g. X-Cap, terrorist</summary>
            Fixed,
        }

        /// <summary>
        /// Class is static, so doesn't need a constructor
        /// </summary>
        private InventoryLayout()
        {
        }

        /// <summary>
        /// Get the Zone that the specified position belongs to
        /// </summary>
        /// <param name="x">column position</param>
        /// <param name="y">row position</param>
        /// <returns>Zone</returns>
        public static Zone GetZone(int x, int y)
        {
            return Zones[y,x];
        }

        /// <summary>
        /// Where each cell of soldier's inventory is drawn on the EquipSoldierScreen
        /// </summary>
        /// <param name="x">column position of inventory cell</param>
        /// <param name="y">row position of inventory cell</param>
        /// <param name="sceneWindow">coords of view on screen</param>
        /// <returns>Co-ords where cell is drawn on Screen</returns>
        public static Rectangle GetScreenPosition(int x, int y, Rectangle sceneWindow)
        {
            float cellLength = sceneWindow.Width / 20.0f;
            return new Rectangle(
                Util.Round(ScreenPosition[y, x, 0] * sceneWindow.Width),
                Util.Round(ScreenPosition[y, x, 1] * sceneWindow.Height),
                (int)(ScreenPosition[y, x, 2] * cellLength),
                (int)(ScreenPosition[y, x, 3] * cellLength)
            );
        }

        #region Fields

        /* Inventory properties are stored in a grid like
        +-+-+---+-----+
        |g|h|f f|b b b|
        +-+-+---+     |
        |a a a a|b b b|
        + +---+ +     |
        |a|c c|a|b b b|
        +-+-+-+-+-+---+
        |d d|e e|i|   |
        +---+---+-+---+
        where
            a = belt
            b = backpack
            c = right shoulder
            d = right leg
            e = left shoulder
            f = left leg
            g = right hand
            h = left hand
            i = armor
        */

        /// <summary>
        /// Width dimension of the layout
        /// </summary>
        public const int CellsWidth = 7;

        /// <summary>
        /// Height dimension of the layout
        /// </summary>
        public const int CellsHeight = 4;

        /// <summary>
        /// The zone each position in the inventory belongs to
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1814:PreferJaggedArraysOverMultidimensional", 
            MessageId = "Member", Justification="There is no wasted space")]
        private static  Zone[,] Zones =
        {
            {Zone.RightHand, Zone.LeftHand,      Zone.LeftLeg,       Zone.LeftLeg,      Zone.BackPack, Zone.BackPack, Zone.BackPack},
            {Zone.Belt,      Zone.Belt,          Zone.Belt,          Zone.Belt,         Zone.BackPack, Zone.BackPack, Zone.BackPack},
            {Zone.Belt,      Zone.RightShoulder, Zone.RightShoulder, Zone.Belt,         Zone.BackPack, Zone.BackPack, Zone.BackPack},
            {Zone.RightLeg,  Zone.RightLeg,      Zone.LeftShoulder,  Zone.LeftShoulder, Zone.Armor,    Zone.None,     Zone.None},
        };

        /// <summary>
        /// Position of each cell of the inventory on the EquipSoldier Screen
        /// </summary>
        /// <remarks>
        /// format is ScreenPosition[cell row][cell column][left/top/width/height]
        /// In an ideal world, this should probably be in the EquipSoldierScreen class, but it's here because 
        /// data is in same order as the Zones table above
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1814:PreferJaggedArraysOverMultidimensional",
            MessageId = "Member", Justification = "There is no wasted space")]
        private static float[, ,] ScreenPosition =
        {
            // top row
            {
                { 58.0f/800.0f, 170.0f/600.0f, 2.0f, 3.0f},     // right hand
                {248.0f/800.0f, 170.0f/600.0f, 2.0f, 3.0f},     // left hand
                {248.0f/800.0f, 320.0f/600.0f, 1.0f, 1.0f},     // left leg
                {289.0f/800.0f, 320.0f/600.0f, 1.0f, 1.0f},     // left leg
                {363.0f/800.0f, 170.0f/600.0f, 1.0f, 1.0f},     // backpack
                {404.0f/800.0f, 170.0f/600.0f, 1.0f, 1.0f},     // backpack
                {445.0f/800.0f, 170.0f/600.0f, 1.0f, 1.0f}      // backpack
            },

            // 2nd row
            {
                {341.0f/800.0f, 320.0f/600.0f, 1.0f, 1.0f},     // belt
                {382.0f/800.0f, 320.0f/600.0f, 1.0f, 1.0f},     // belt
                {423.0f/800.0f, 320.0f/600.0f, 1.0f, 1.0f},     // belt
                {464.0f/800.0f, 320.0f/600.0f, 1.0f, 1.0f},     // belt
                {363.0f/800.0f, 211.0f/600.0f, 1.0f, 1.0f},     // backpack
                {404.0f/800.0f, 211.0f/600.0f, 1.0f, 1.0f},     // backpack
                {445.0f/800.0f, 211.0f/600.0f, 1.0f, 1.0f}      // backpack
            },

            // 3rd row
            {
                {341.0f/800.0f, 361.0f/600.0f, 1.0f, 1.0f},     // belt
                { 58.0f/800.0f,  49.0f/600.0f, 1.0f, 1.0f},     // right shoulder
                { 99.0f/800.0f,  49.0f/600.0f, 1.0f, 1.0f},     // right shoulder
                {464.0f/800.0f, 361.0f/600.0f, 1.0f, 1.0f},     // belt
                {363.0f/800.0f, 252.0f/600.0f, 1.0f, 1.0f},     // backpack
                {404.0f/800.0f, 252.0f/600.0f, 1.0f, 1.0f},     // backpack
                {445.0f/800.0f, 252.0f/600.0f, 1.0f, 1.0f}      // backpack
            },

            // Bottom row
            {
                { 58.0f/800.0f, 320.0f/600.0f, 1.0f, 1.0f},     // right leg
                { 99.0f/800.0f, 320.0f/600.0f, 1.0f, 1.0f},     // right leg
                {248.0f/800.0f,  49.0f/600.0f, 1.0f, 1.0f},     // left shoulder
                {289.0f/800.0f,  49.0f/600.0f, 1.0f, 1.0f},     // left shoulder
                {162.0f/800.0f, 112.0f/600.0f, 2.0f, 3.0f},     // armor
                {0, 0, 0, 0},                                   // unused
                {0, 0, 0, 0},                                   // unused
            }
        };

        /// <summary>
        /// Does the specified position correspond to a hand?
        /// </summary>
        /// <param name="x">column position</param>
        /// <param name="y">row position</param>
        /// <returns>true if position is a hand</returns>
        public static bool IsHand(int x, int y)
        {
            Zone zone = Zones[y, x];
            return ((Zone.RightHand == zone) || (Zone.LeftHand == zone));
        }

        /// <summary>
        /// Does the specified position correspond to the armor position
        /// </summary>
        /// <param name="x">column position</param>
        /// <param name="y">row position</param>
        /// <returns>true if position is armor slot</returns>
        public static bool IsArmor(int x, int y)
        {
            return (Zone.Armor == Zones[y, x]);
        }

        /// <summary>
        /// Can an item fit in a hand?
        /// </summary>
        /// <param name="item">item to check</param>
        /// <returns>true if it will fit</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
            Justification="Will throw if item is null")]
        public static bool CanFitInHand(Item item)
        {
            CarryInfo carryInfo = item.ItemInfo.CarryInfo;
            return ((carryInfo.X <= 2) && (carryInfo.Y <= 3));
        }

        #endregion Fields
    }
}
