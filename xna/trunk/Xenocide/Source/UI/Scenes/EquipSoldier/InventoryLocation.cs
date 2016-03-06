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
* @file InventoryLocation.cs
* @date Created: 2007/11/19
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
using ProjectXenocide.Model.Battlescape.Combatants;
using ProjectXenocide.Model.StaticData.Items;

namespace ProjectXenocide.UI.Scenes
{
    /// <summary>
    /// A place that can show inventory on the EquipSolider screen.
    /// Either a position on the soldier, or a position on the ground/Outpost inventory
    /// </summary>
    public class InventoryLocation
    {
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="isOnSoldier">Is this location on the soldier or in the inventory/ground area?</param>
        /// <param name="x">X co-ordinate of the location</param>
        /// <param name="y">Y co-ordinate of the location</param>
        public InventoryLocation(bool isOnSoldier, int x, int y)
        {
            this.isOnSoldier = isOnSoldier;
            this.x           = x;
            this.y           = y;
        }

        /// <summary>
        /// Figure out where in inventory mouse is pointing to
        /// </summary>
        /// <param name="x">mouse position</param>
        /// <param name="y">mouse position</param>
        /// <param name="scene">co-ordinates of window showing the EquipSoldier Scene</param>
        /// <returns>Location in inventory corresponding to the mouse's position.  Or null if no match</returns>
        public static InventoryLocation FromMousePosition(int x, int y, Rectangle scene)
        {
            // easy case is mouse is on the "ground"
            Rectangle groundArea = GroundArea(scene);
            if (groundArea.Contains(x, y))
            {
                int column = (int)((x - groundArea.X) * GroundWidthInCells  / (float)groundArea.Width);
                int row    = (int)((y - groundArea.Y) * GroundHeightInCells / (float)groundArea.Height);
                return new InventoryLocation(false, column, row);
            }

            // check the cells making up the soldier's inventory
            for (int i = 0; i < InventoryLayout.CellsWidth; ++i)
            {
                for (int j = 0; j < InventoryLayout.CellsHeight; ++j)
                {
                    if ((Zone.None != InventoryLayout.GetZone(i, j)) &&
                        InventoryLayout.GetScreenPosition(i, j, scene).Contains(x, y))
                    {
                        return new InventoryLocation(true, i, j);
                    }
                }
            }

            // if get here, then mouse isn't over any inventory
            return null;
        }

        /// <summary>
        /// Compute where to draw an item on the "Ground" area of screen
        /// </summary>
        /// <param name="sceneWindow">total size of the screen</param>
        /// <param name="carryInfo">size info of item to draw</param>
        /// <param name="offset">row of cell holding item's leftmost edge</param>
        /// <returns>Rectangle to render object inside</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
            Justification = "Will throw if carryInfo is null")]
        public static Rectangle GroundPosition(Rectangle sceneWindow, CarryInfo carryInfo, int offset)
        {
            Rectangle groundArea = GroundArea(sceneWindow);

            float cellWidth = groundArea.Width / GroundWidthInCells;
            float cellHeight = groundArea.Height / GroundHeightInCells;
            return new Rectangle(
                Util.Round(groundArea.Left + cellWidth * offset),
                groundArea.Top,
                Util.Round(carryInfo.X * cellWidth),
                Util.Round(carryInfo.Y * cellHeight)
            );
        }

        /// <summary>
        /// Calculate the area of screen that is the "ground"
        /// </summary>
        /// <param name="sceneWindow">Area of window on display</param>
        /// <returns>Area of screen that is "ground"</returns>
        public static Rectangle GroundArea(Rectangle sceneWindow)
        {
            return new Rectangle(
                Util.Round( 10.0f / 800.0f * sceneWindow.Width),
                Util.Round(469.0f / 600.0f * sceneWindow.Height),
                Util.Round(779.0f / 800.0f * sceneWindow.Width),
                Util.Round(120.0f / 600.0f * sceneWindow.Height)
                );
        }

        #region Fields

        /// <summary>The width of the ground area, in "cells"</summary>
        public const float GroundWidthInCells = 19.0f;

        /// <summary>The height of the ground area, in "cells"</summary>
        private const float GroundHeightInCells = 3.0f;

        /// <summary>
        /// Is this location on the soldier or in the inventory/ground area?
        /// </summary>
        public bool IsOnSoldier { get { return isOnSoldier; } }

        /// <summary>
        /// X co-ordinate of the location
        /// either x index to CombatantInventory.cells[y,x]
        /// or column on "ground"
        /// </summary>
        public int X { get { return x; } }

        /// <summary>
        /// Y co-ordinate of the location
        /// either y index to CombatantInventory.cells[y,x]
        /// or row on "ground"
        /// </summary>
        public int Y { get { return y; } }

        /// <summary>
        /// Is this location on the soldier or in the inventory/ground area?
        /// </summary>
        private bool isOnSoldier;

        /// <summary>
        /// X co-ordinate of the location
        /// either x index to CombatantInventory.cells[y,x]
        /// or column on "ground"
        /// </summary>
        private int x;

        /// <summary>
        /// Y co-ordinate of the location
        /// either y index to CombatantInventory.cells[y,x]
        /// or row on "ground"
        /// </summary>
        private int y;

        #endregion Fields
    }
}
