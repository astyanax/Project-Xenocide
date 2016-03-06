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
* @file PickActionDialog.cs
* @date Created: 2008/02/26
* @author File creator: David Teviotdale
* @author Credits: none
*/
#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;

using CeGui;
using ProjectXenocide.UI.Screens;


using ProjectXenocide.Utils;
using ProjectXenocide.Model.StaticData;
using ProjectXenocide.Model.StaticData.Items;
using ProjectXenocide.Model.Battlescape.Combatants;
using ProjectXenocide.UI.Dialogs;
using Xenocide.Resources;


#endregion

namespace ProjectXenocide.UI.Screens
{
    public partial class BattlescapeScreen
    {
        /// <summary>
        /// Dialog where user selects the type of action to perform
        /// </summary>
        private class PickActionDialog : Dialog
        {
            /// <summary>Constructor</summary>
            /// <param name="battlescapeScreen">The screen to send the user's selection to</param>
            /// <param name="item">Item we're listing actions for</param>
            /// <param name="combatant">Combatant holding the item</param>
            /// <param name="rightHand">True if the action is started from the right hand.</param>
            public PickActionDialog(BattlescapeScreen battlescapeScreen, Item item, Combatant combatant, bool rightHand)
                : base("Content/Layouts/PickActionDialog.layout")
            {
                this.battlescapeScreen = battlescapeScreen;
                this.item = item;
                this.combatant = combatant;
                this.rightHand = rightHand;
            }

            #region Create the CeGui widgets

            /// <summary>add the widgets to the screen</summary>
            protected override void CreateCeguiWidgets()
            {
                grid = (CeGui.Widgets.MultiColumnList)WindowManager.Instance.GetWindow(gridActionsName);
                grid.AddColumn(Strings.DLG_PICKACTION_COLUMN_ACTION, grid.ColumnCount, 1.0f);

                Combatant.ActiveArm activeArm = Combatant.ActiveArm.Both;
                // Check if the other hand is occupied
                if (combatant.Inventory.ItemAt(rightHand ? 1 : 0, 0) != null)
                {
                    // Other hand is occupied, set active arm to correct arm.
                    if (rightHand)
                    {
                        activeArm = Combatant.ActiveArm.Right;
                    }
                    else
                    {
                        activeArm = Combatant.ActiveArm.Left;
                    }
                }

                AddActionsToGrid(activeArm);
            }

            private CeGui.Widgets.MultiColumnList grid;

            /// <summary>Add the available actions to the grid</summary>
            /// <param name="item">Arm using the item</param>
            private void AddActionsToGrid(Combatant.ActiveArm activeArm)
            {
                int index = 0;
                foreach (ActionInfo action in item.ItemInfo.Actions)
                {
                    CeGui.Widgets.ListboxItem listboxItem = Util.CreateListboxItem(action.MenuEntry(combatant, activeArm));
                    listboxItem.ID = index;
                    grid.AddRow(listboxItem, 0);
                    ++index;
                }
            }

            #endregion Create the CeGui widgets

            #region event handlers

            /// <summary>user has selected an action</summary>
            /// <param name="sender">Not used</param>
            /// <param name="e">Not used</param>
            [GuiEvent()]
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
            public void OnOkClicked(object sender, CeGui.GuiEventArgs e)
            {
                DoSelectedAction();
            }

            [GuiEvent()]
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
            public void OnGridMouseDoubleClicked(object sender, MouseEventArgs e)
            {
                DoSelectedAction();
            }

            /// <summary>user wants to cancel picking an action</summary>
            /// <param name="sender">Not used</param>
            /// <param name="e">Not used</param>
            [GuiEvent()]
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
            public void OnCancelClicked(object sender, CeGui.GuiEventArgs e)
            {
                ScreenManager.CloseDialog(this);
            }

            /// <summary>Try to do the action user selected</summary>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode",
                Justification = "FxCop false positive")]
            private void DoSelectedAction()
            {
                // Get the action the user selected
                CeGui.Widgets.ListboxItem listboxItem = grid.GetFirstSelectedItem();
                if (null == listboxItem)
                {
                    Util.ShowMessageBox(Strings.MSGBOX_NO_ACTION_SELECTED);
                }
                else
                {
                    ActionInfo action = item.ItemInfo.Actions[listboxItem.ID];

                    // Check that user can perform action
                    ActionError error = action.ActionAvailable(battlescapeScreen.battlescape, item, combatant);
                    if (ActionError.None != error)
                    {
                        Util.ShowMessageBox(ActionInfo.ErrorText(error));
                    }
                    else
                    {
                        // if action doesn't need a location, start the action, else get a location
                        if (!action.NeedsLocation)
                        {
                            combatant.Order = action.Start(battlescapeScreen.battlescape, item, combatant);
                            battlescapeScreen.ChangeState(new CombatantActivityScreenState(battlescapeScreen, combatant));
                        }
                        else
                        {
                            battlescapeScreen.ChangeState(
                               new LocationOrderCombatantScreenState(battlescapeScreen, item, combatant, action));
                        }
                        ScreenManager.CloseDialog(this);
                    }
                }
            }

            #endregion event handlers

            #region Fields

            /// <summary>The screen to send the user's selection to</summary>
            private BattlescapeScreen battlescapeScreen;

            /// <summary>Item we're listing actions for</summary>
            private Item item;

            /// <summary>Combatant holding the item</summary>
            private Combatant combatant;

            /// <summary>True if the action was started from the right hand.</summary>
            private bool rightHand;

            #endregion

            #region Constants

            private const string gridActionsName = "gridActions";

            #endregion
        }
    }
}
