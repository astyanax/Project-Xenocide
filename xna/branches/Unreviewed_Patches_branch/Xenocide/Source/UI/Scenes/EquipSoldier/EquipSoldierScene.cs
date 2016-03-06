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
* @file EquipSoldierScene.cs
* @date Created: 2007/11/18
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
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

using ProjectXenocide.Utils;
using ProjectXenocide.Model.Battlescape.Combatants;
using ProjectXenocide.Model.StaticData.Items;
using ProjectXenocide.Model.Geoscape;

#endregion

namespace ProjectXenocide.UI.Scenes
{
    /// <summary>
    /// This is 2D graphics scene, showing the items in inventory/on ground/carried by soldier
    /// </summary>
    public class EquipSoldierScene : IDisposable
    {
        /// <summary>
        /// Implement IDisposable
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Implement IDisposable
        /// </summary>
        /// <param name="disposing">false when called from a finalizer</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (spriteBatch != null)
                {
                    spriteBatch.Dispose();
                    spriteBatch = null;
                }
                if (content != null)
                {
                    content.Dispose();
                    content = null;
                }
            }
        }

        /// <summary>
        /// Load the graphic content of the scene
        /// </summary>
        public void LoadContent()
        {
            spriteBatch = new SpriteBatch(Xenocide.Instance.GraphicsDevice);
            background  = Texture2D.FromFile(Xenocide.Instance.GraphicsDevice, @"Content\Textures\EquipSoldier\EquipScreenBackground.png");
            spriteSheet = Texture2D.FromFile(Xenocide.Instance.GraphicsDevice, @"Content\Textures\EquipSoldier\InventorySprites.png");
            font        = content.Load<SpriteFont>(@"Content\SpriteFont1");
        }

        /// <summary>
        /// Start Rendering scene
        /// </summary>
        /// <param name="sceneWindow">where to draw the scene on the Window</param>
        public void BeginDraw(Rectangle sceneWindow)
        {
            spriteBatch.Begin(SpriteBlendMode.AlphaBlend);
            spriteBatch.Draw(background, sceneWindow, null, Color.White, 0.0f, new Vector2(), SpriteEffects.None, 1.0f);
        }

        /// <summary>
        /// Draw the items being carried by the soldier, at the appropriate locations
        /// </summary>
        /// <param name="sceneWindow">where to draw the scene on the Window</param>
        /// <param name="combatantInventory">the items being carried by the soldier</param>
        public void DrawSoldiersInventory(Rectangle sceneWindow, CombatantInventory combatantInventory)
        {
            foreach (CombatantInventory.Slot slot in combatantInventory.Contents)
            {
                // get where to draw sprite
                Rectangle dest = InventoryLayout.GetScreenPosition(slot.X, slot.Y, sceneWindow);

                // adjust size of output rect to cover correct number of cells.
                CarryInfo carryInfo = slot.Item.ItemInfo.CarryInfo;
                if (InventoryLayout.IsHand(slot.X, slot.Y) || InventoryLayout.IsArmor(slot.X, slot.Y))
                {
                    // hand and armor cells are special case.  Single cell, 2 x 3 units in size.
                    // draw item in center of cell
                    int w = (int)(dest.Width * (carryInfo.X / 2.0));
                    int h = (int)(dest.Height * (carryInfo.Y / 3.0));
                    dest = new Rectangle(dest.X + ((dest.Width - w) / 2), dest.Y + ((dest.Height - h) / 2), w, h);
                }
                else
                {
                    dest = new Rectangle(dest.X, dest.Y, dest.Width * carryInfo.X, dest.Height * carryInfo.Y);
                }

                spriteBatch.Draw(
                    spriteSheet,
                    dest,
                    carryInfo.SpriteRect,
                    Color.White, 0.0f, new Vector2(), SpriteEffects.None, 0.0f);
            }
        }

        /// <summary>
        /// Draw the item the player is currently moving
        /// </summary>
        /// <param name="sceneWindow">where to draw the scene on the Window</param>
        /// <param name="movingItem">Item player is currently moving</param>
        /// <param name="pos">Location of cursor on screen</param>
        public void DrawItemBeingMoved(Rectangle sceneWindow, Item movingItem, Vector2 pos)
        {
            if (null != movingItem)
            {
                // where to draw sprite
                CarryInfo carryInfo = movingItem.ItemInfo.CarryInfo;
                float     cell      = sceneWindow.Width / 20.0f;
                spriteBatch.Draw(
                    spriteSheet,
                    new Rectangle((int)pos.X, (int)pos.Y, (int)(carryInfo.X * cell), (int)(carryInfo.Y * cell)),
                    carryInfo.SpriteRect,
                    Color.White, 0.0f, new Vector2(), SpriteEffects.None, 0.0f);
            }
        }

        /// <summary>
        /// Draw an item on the "ground" area of the screen
        /// </summary>
        /// <param name="sceneWindow">where to draw the scene on the Window</param>
        /// <param name="carryInfo">info on item to draw</param>
        /// <param name="offset">row of cell holding item's leftmost edge</param>
        /// <param name="count">count to draw over item</param>
        public void DrawItemOnGround(Rectangle sceneWindow, CarryInfo carryInfo, int offset, int count)
        {
            // draw item
            Rectangle dest = InventoryLocation.GroundPosition(sceneWindow, carryInfo, offset);
            spriteBatch.Draw(
                spriteSheet,
                dest,
                carryInfo.SpriteRect,
                Color.White, 0.0f, new Vector2(), SpriteEffects.None, 0.0f);

            // add number of items (if more than one)
            if (1 < count)
            {
                string output = Util.ToString(count);

                // figure out where to draw text.
                // align with right edge of item's bitmap
                // if item is 3 units high, put over bottom of bitmap else put count under item.
                Vector2 pos = font.MeasureString(output);
                pos.X = dest.Right - pos.X - 2;
                pos.Y = dest.Bottom - ((carryInfo.Y < 3) ? 0 : pos.Y);
                spriteBatch.DrawString(font, output, pos, Color.Purple);
            }
        }

        /// <summary>
        /// Finish Rendering scene
        /// </summary>
        public void EndDraw()
        {
            spriteBatch.End();
        }

        #region Fields

        /// <summary>
        /// The background image
        /// </summary>
        private Texture2D background;

        /// <summary>
        /// Sprites for all the inventory objects
        /// </summary>
        private Texture2D spriteSheet;

        /// <summary>
        /// Used to draw the sprites
        /// </summary>
        private SpriteBatch spriteBatch;

        /// <summary>
        /// Font to draw number of items on "ground"
        /// </summary>
        private SpriteFont font;

        /// <summary>
        /// Needed so that we can load the sprite font
        /// </summary>
        private ContentManager content = new ContentManager(Xenocide.Instance.Services);

        #endregion Fields
    }
}
