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
* @file EquipSoldierScreen.cs
* @date Created: 2007/11/17
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
using ProjectXenocide.Model.Geoscape;
using ProjectXenocide.Model.Geoscape.Outposts;
using ProjectXenocide.Model.StaticData.Items;
using ProjectXenocide.Model.Battlescape;
using ProjectXenocide.Model.Battlescape.Combatants;
using ProjectXenocide.UI.Scenes;
#endregion

// alias Vector2
using Vector2 = Microsoft.Xna.Framework.Vector2;
using Xenocide.Resources;


namespace ProjectXenocide.UI.Screens
{
    /// <summary>
    /// This is the screen that lets player set the items a soldier carries
    /// </summary>
    public partial class EquipSoldierScreen : Screen
    {
        /// <summary>
        /// Constructor used to equip soldiers in an outpost
        /// </summary>
        /// <param name="soldiers">Soldiers player can select from</param>
        /// <param name="soldier">Soldier in the list to initially show</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
            Justification = "Will throw if soldier is null")]
        public EquipSoldierScreen(IEnumerable<Person> soldiers, Person soldier)
            : base("EquipSoldierScreen", null)
        {
            itemSource = new OutpostItemSource(soldier.Outpost.Inventory);
            controller = new InOutpostController(this, soldiers, soldier);
        }

        /// <summary>
        /// Constructor used to adjust combatants on a battlescape
        /// </summary>
        /// <param name="combatant">Combatant who's inventory is being examined/adjusted</param>
        /// <param name="battlescape">the battlescape</param>
        /// <param name="lookOnly">Is screen look only mode? (i.e. combatant is being psi probed)</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
            Justification = "Will throw if soldier is null")]
        public EquipSoldierScreen(Combatant combatant, Battle battlescape, bool lookOnly)
            : base("EquipSoldierScreen", null)
        {
            this.itemSource = new BattlescapeItemSource(battlescape, combatant.Position);
            this.controller = new BattlescapeController(this, battlescape, combatant);
            this.lookOnly = lookOnly;
        }

        //ToDo:
        //A second constructor, used on the battlescape

        /// <summary>
        /// Removes the frame from the display
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    if (scene != null)
                    {
                        scene.Dispose();
                        scene = null;
                    }
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        /// <summary>
        /// Load the Scene's graphic content
        /// </summary>
        /// <param name="content">content manager that fetches the content</param>
        /// <param name="device">the display</param>
        
        public override void LoadContent(ContentManager content, GraphicsDevice device)
        {
            scene.LoadContent();
        }

        /// <summary>
        /// Render the 3D scene
        /// </summary>
        /// <param name="gameTime">time interval since last render</param>
        /// <param name="device">Device to render the screen to</param>
        public override void Draw(GameTime gameTime, GraphicsDevice device)
        {
            Rectangle sceneWindow = GetSceneRectangle(device);
            scene.BeginDraw(sceneWindow);
            scene.DrawSoldiersInventory(sceneWindow, controller.Combatant.Inventory);
            scene.DrawItemBeingMoved(sceneWindow, movingItem, cursorPosition);
            itemSource.Draw(sceneWindow, scene);
            scene.EndDraw();
        }

        #region Create the CeGui widgets

        /// <summary>
        /// add the buttons to the screen
        /// </summary>
        protected override void CreateCeguiWidgets()
        {
            // Text detailing ammo in weapon/clip
            ammoText = AddStaticText(0.7475f, 0.50f, 0.2275f, 0.08f);
            ShowAmmoString();

            // buttons
            closeButton              = AddButton("BUTTON_CLOSE",        0.7025f, 0.08f, 0.2275f, 0.04125f);
            leftButton               = AddButton("BUTTON_SCROLL_LEFT",  0.7025f, 0.13f, 0.2275f, 0.04125f);
            rightButton              = AddButton("BUTTON_SCROLL_RIGHT", 0.7025f, 0.18f, 0.2275f, 0.04125f);

            closeButton.Clicked         += new CeGui.GuiEventHandler(  OnCloseButton);
            leftButton.Clicked          += new CeGui.GuiEventHandler(  OnLeftButton);
            rightButton.Clicked         += new CeGui.GuiEventHandler(  OnRightButton);
            RootWidget.MouseButtonsDown += new CeGui.MouseEventHandler(OnMouseDownInScene);
            RootWidget.MouseMove        += new CeGui.MouseEventHandler(OnMouseMoveInScene);

            AddStaticText("SCREEN_EQUIP_SOLDIER_LEFT_SHOULDER",  0.3054f, 0.130f, 0.2275f, 0.05f);
            AddStaticText("SCREEN_EQUIP_SOLDIER_RIGHT_SHOULDER", 0.0734f, 0.130f, 0.2275f, 0.05f);
            AddStaticText("SCREEN_EQUIP_SOLDIER_LEFT_HAND",      0.3174f, 0.230f, 0.2275f, 0.05f);
            AddStaticText("SCREEN_EQUIP_SOLDIER_RIGHT_HAND",     0.0820f, 0.230f, 0.2275f, 0.05f);
            AddStaticText("SCREEN_EQUIP_SOLDIER_LEFT_LEG",       0.3194f, 0.480f, 0.2275f, 0.05f);
            AddStaticText("SCREEN_EQUIP_SOLDIER_RIGHT_LEG",      0.0825f, 0.480f, 0.2275f, 0.05f);
            AddStaticText("SCREEN_EQUIP_SOLDIER_BACKPACK",       0.4934f, 0.230f, 0.2275f, 0.05f);
            AddStaticText("SCREEN_EQUIP_SOLDIER_BELT",           0.5104f, 0.480f, 0.2275f, 0.05f);

            controller.CreateCeguiWidgets();
        }

        private CeGui.Widgets.StaticText ammoText;
        private CeGui.Widgets.PushButton closeButton;
        private CeGui.Widgets.PushButton leftButton;
        private CeGui.Widgets.PushButton rightButton;

        #endregion Create the CeGui widgets

        #region event handlers

        /// <summary>React to user pressing the Close button</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void OnCloseButton(object sender, CeGui.GuiEventArgs e)
        {
            controller.OnCloseButton();
        }

        /// <summary>React to user pressing the Left button</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void OnLeftButton(object sender, CeGui.GuiEventArgs e)
        {
            itemSource.ScrollLeft();
        }

        /// <summary>React to user pressing the Right button</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void OnRightButton(object sender, CeGui.GuiEventArgs e)
        {
            itemSource.ScrollRight();
        }

        /// <summary>React to user moving the mouse in the 3D scene</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Mouse information</param>
        private void OnMouseMoveInScene(object sender, CeGui.MouseEventArgs e)
        {
            // record the mouse position, we will need it later
            cursorPosition = new Vector2(e.Position.X, e.Position.Y);
        }

        /// <summary>React to user clicking mouse in the 3D scene</summary>
        /// <param name="sender">CeGui widget sending the event</param>
        /// <param name="e">Mouse information</param>
        private void OnMouseDownInScene(object sender, CeGui.MouseEventArgs e)
        {
            // if in look only mode, then can't pick up/drop or move items
            if (lookOnly)
            {
                return;
            }

            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                OnLeftMouseDown((int)e.Position.X, (int)e.Position.Y);
            }
            else if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                OnRightMouseDown((int)e.Position.X, (int)e.Position.Y);
            }
        }

        #endregion event handlers

        /// <summary>
        /// Called when user has left clicked mouse
        /// </summary>
        /// <param name="x">position of mouse when clicked</param>
        /// <param name="y">position of mouse when clicked</param>
        private void OnLeftMouseDown(int x, int y)
        {
            // figure out what mouse is pointing at
            Rectangle         sceneRect = GetSceneRectangle(Xenocide.Instance.GraphicsDevice);
            InventoryLocation location  = InventoryLocation.FromMousePosition(x, y, sceneRect);
            Dump(location);

            // user is either picking up or dropping an item (or we ignore the click)
            if (null != location)
            {
                if (null == movingItem)
                {
                    StartMovingItem(location);
                }
                else
                {
                    PutItemDown(location);
                }
            }
            ShowAmmoString();
        }

        /// <summary>
        /// Called when user has right clicked mouse
        /// </summary>
        /// <param name="x">position of mouse when clicked</param>
        /// <param name="y">position of mouse when clicked</param>
        private void OnRightMouseDown(int x, int y)
        {
            // if there's alread a item being moved, ignore the event
            if (null == movingItem)
            {
                // figure out what mouse is pointing at
                Rectangle         sceneRect = GetSceneRectangle(Xenocide.Instance.GraphicsDevice);
                InventoryLocation location  = InventoryLocation.FromMousePosition(x, y, sceneRect);

                // if it's over an item, then user has asked us to unload it
                if ((null != location) && (location.IsOnSoldier))
                {
                    Item item = controller.Combatant.Inventory.ItemAt(location.X, location.Y);
                    if ((null != item) && (item.HoldsAmmo))
                    {
                        movingItem = item.Unload();
                        ShowAmmoString();
                    }
                }
            }
        }

        /// <summary>
        /// Try to pick up the item under the mouse
        /// </summary>
        /// <param name="location">mouse location</param>
        private void StartMovingItem(InventoryLocation location)
        {
            Debug.Assert(null == movingItem);
            if (location.IsOnSoldier)
            {
                CombatantInventory ci = controller.Combatant.Inventory;
                movingItem = ci.ItemAt(location.X, location.Y);
                if (null != movingItem)
                {
                    ci.Remove(movingItem);
                }
            }
            else
            {
                movingItem = itemSource.FetchItem(location.X, location.Y);
            }
        }

        /// <summary>
        /// Try to put item down at mouse's current location
        /// </summary>
        /// <param name="location">mouse location</param>
        private void PutItemDown(InventoryLocation location)
        {
            Debug.Assert(null != movingItem);
            if (location.IsOnSoldier)
            {
                // put item into this location in the soldier's inventory
                CombatantInventory ci = controller.Combatant.Inventory;
                if (ci.CanFit(movingItem, location.X, location.Y))
                {
                    ci.Insert(movingItem, location.X, location.Y);
                    movingItem = null;
                }
                else
                {
                    // item can't fit.
                    // If location is occupied, perhaps user is trying to load a weapon
                    Item target = ci.ItemAt(location.X, location.Y);
                    if (null != target)
                    {
                        TryLoadingItem(target);
                    }
                }
            }
            else
            {
                ReleaseMovingItem();
            }
        }

        /// <summary>
        /// Get coordinates where scene will be drawn on window
        /// </summary>
        /// <param name="device">Device to render the screen to</param>
        /// <returns>the co-ordinates</returns>
        private static Rectangle GetSceneRectangle(GraphicsDevice device)
        {
            Viewport port = device.Viewport;
            return new Rectangle(port.X, port.Y, port.Width, port.Height);
        }

        /// <summary>
        /// Show ammo state of currently selected item, if it holds ammo
        /// </summary>
        private void ShowAmmoString()
        {
            StringBuilder description = new StringBuilder();

            // if there's no item selected, or item doesn't hold ammo, string is empty
            if ((null != movingItem) && (null != movingItem.ItemInfo.AmmoInfo))
            {
                // if can hold multiple ammo types, give type currently carrying
                if ((1 < movingItem.ItemInfo.AmmoInfo.Ammos.Count) && movingItem.HoldsAmmo)
                {
                    description.Append(Util.StringFormat(Strings.SCREEN_EQUIP_SOLDIER_AMMO_TYPE, movingItem.AmmoInfo.Name));
                    description.Append(Util.Linefeed);
                }
                // number of rounds
                description.Append(Util.StringFormat(Strings.SCREEN_EQUIP_SOLDIER_AMMO_QUANTITY, movingItem.ShotsLeft));
            }
            ammoText.Text = description.ToString();
        }

        /// <summary>
        /// Try loading selected item with item we're moving
        /// </summary>
        /// <param name="target">item we're trying to put clip into</param>
        private void TryLoadingItem(Item target)
        {
            // only works if we're trying to insert a clip, and target can use this clip type
            if (target.IsAmmoValid(movingItem))
            {
                movingItem = target.Load(movingItem);
            }
        }

        /// <summary>
        /// Put the item we moving where we're getting items from
        /// </summary>
        private void ReleaseMovingItem()
        {
            if (null != movingItem)
            {
                itemSource.ReplaceItem(movingItem);
                movingItem = null;
            }
        }

        /// <summary>
        /// Quick and dirty function for testing location
        /// </summary>
        [Conditional("DEBUG")]
        private static void Dump(InventoryLocation location)
        {
            StringBuilder sb = new StringBuilder("Location = ");
            if (null != location)
            {
                if (location.IsOnSoldier)
                {
                    sb.Append(InventoryLayout.GetZone(location.X, location.Y).ToString());
                }
                else
                {
                    sb.Append("on ground");
                }
                sb.Append(". x = ");
                sb.Append(Util.ToString(location.X));
                sb.Append(", y = ");
                sb.Append(Util.ToString(location.Y));
            }
            Debug.WriteLine(sb.ToString());
        }

        #region Fields

        /// <summary>
        /// Item currently being moved
        /// </summary>
        public Item MovingItem
        {
            get { return movingItem; }
            set
            {
                // it's an error if we're already moving an item
                Debug.Assert(null == movingItem);
                movingItem = value;
                ShowAmmoString();
            }
        }

        /// <summary>
        /// The view shown on the screen
        /// </summary>
        private EquipSoldierScene scene = new EquipSoldierScene();

        /// <summary>
        /// Item currently being moved
        /// </summary>
        private Item movingItem;

        /// <summary>
        /// Location of cursor on screen;
        /// </summary>
        private Vector2 cursorPosition = new Vector2();

        /// <summary>
        /// where we're getting the items we're adding to a soldier
        /// </summary>
        private ItemSource itemSource;

        /// <summary>Control screen behaviour that's specific to mode screen is running in</summary>
        private Controller controller;

        /// <summary>Is screen look only mode? (i.e. combatant is being psi probed)</summary>
        private bool lookOnly;

        #endregion Fields
    }
}
