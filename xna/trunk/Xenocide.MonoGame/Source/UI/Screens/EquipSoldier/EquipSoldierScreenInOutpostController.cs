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
* @file EquipSoldierScreenInOutpostController.cs
* @date Created: 2008/03/02
* @author File creator: David Teviotdale
* @author Credits: none
*/
#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

using Gum.Forms.Controls;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using ProjectXenocide.Model.Battlescape.Combatants;
using ProjectXenocide.Model.Geoscape;
using ProjectXenocide.Model.Geoscape.Outposts;
using ProjectXenocide.Model.StaticData.Items;
using ProjectXenocide.UI.Dialogs;
using ProjectXenocide.UI.Scenes;
using ProjectXenocide.Utils;
#endregion

namespace ProjectXenocide.UI.Screens
{
    /// <summary>This is the screen that lets player set the items a soldier carries</summary>
    public partial class EquipSoldierScreen
    {
        /// <summary>
        /// Specifies behaviour of screen, when Equipping a soldier in an outpost
        /// </summary>
        private sealed class InOutpostController : Controller
        {
            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="equipSoldierScreen">The actual screen</param>
            /// <param name="soldiers">Soldiers player can select from</param>
            /// <param name="soldier">Soldier in the list to initially show</param>
            public InOutpostController(EquipSoldierScreen equipSoldierScreen, IEnumerable<Person> soldiers, Person soldier)
                : base(equipSoldierScreen)
            {
                this.soldiers = new List<Person>(soldiers);
                this.soldierIndex = this.soldiers.IndexOf(soldier);
            }

            #region Create the Gum controls

            /// <summary>add Widgets to the screen</summary>
            public override void CreateGumControls()
            {
                // combo box to allow player to select soldier to work on
                soldiersComboBox = new ComboBox();
                EquipSoldierScreen.RootContainer.AddChild(soldiersComboBox);
                PopulateSoldiersComboBox();
                soldiersComboBox.SelectionChanged += (s, a) => OnSoldierSelectionChanged(s, EventArgs.Empty);
            }

            private ComboBox soldiersComboBox;

            /// <summary>
            /// Fill combo box with a list of soldiers
            /// </summary>
            private void PopulateSoldiersComboBox()
            {
                foreach (Person soldier in soldiers)
                {
                    soldiersComboBox.Items.Add(soldier.Name);
                }

                //... set combo selection to soldier currently being viewed
                soldiersComboBox.SelectedIndex = soldierIndex;
            }

            #endregion Create the Gum controls

            #region event handlers

            /// <summary>React to user pressing the Close button</summary>
            public override void OnCloseButton()
            {
                ReturnToParentScreen();
            }

            /// <summary>Player wants to look at a different soldier</summary>
            /// <param name="sender">Not used</param>
            /// <param name="e">Not used</param>
            private void OnSoldierSelectionChanged(object sender, EventArgs e)
            {
                ChangeSoldier();
            }

            #endregion event handlers

            #region methods

            /// <summary>Player wants to look at a different soldier</summary>
            private void ChangeSoldier()
            {
                int index = soldiersComboBox.SelectedIndex;
                if (index >= 0)
                {
                    EquipSoldierScreen.ReleaseMovingItem();
                    RecordSoldierLoadout();
                    soldierIndex = index;
                }
            }

            /// <summary>
            /// Close this screen and go back to the Screen that created this one 
            /// (either SolidersList or Battlescape)
            /// </summary>
            private void ReturnToParentScreen()
            {
                EquipSoldierScreen.ReleaseMovingItem();
                RecordSoldierLoadout();
                EquipSoldierScreen.ScreenManager.ScheduleScreen(new SoldiersListScreen(SelectedOutpostIndex()));
            }

            /// <summary>index to the outpost that this soldier belongs to</summary>
            /// <returns>index to Outpost</returns>
            private int SelectedOutpostIndex()
            {
                for (int i = 0; i < Xenocide.GameState.GeoData.Outposts.Count; ++i)
                {
                    if (Xenocide.GameState.GeoData.Outposts[i] == Soldier.Outpost)
                    {
                        return i;
                    }
                }
                // should never get here
                Debug.Assert(false);
                return 0;
            }

            /// <summary>
            /// Make a record of currently selected soldier's loadout, so we can
            /// maintain it automatically when soldier returns from misions
            /// </summary>
            private void RecordSoldierLoadout()
            {
                // note, we only record layout when we're in an outpost
                Combatant.Inventory.RecordLoadout();
            }

            #endregion methods

            #region fields

            /// <summary>Combatant this screen is currently showing</summary>
            public override Combatant Combatant { get { return Soldier.Combatant; } }

            /// <summary>
            /// Soldiers player can select between
            /// </summary>
            private List<Person> soldiers;

            /// <summary>
            /// The Soldier this screen is currently showing
            /// </summary>
            private int soldierIndex;

            /// <summary>
            /// The Soldier this screen is currently showing
            /// </summary>
            private Person Soldier { get { return soldiers[soldierIndex]; } }

            #endregion
        }
    }
}
