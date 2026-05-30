using System;
using System.Collections.Generic;
using System.Text;

using Gum.Forms.Controls;

using ProjectXenocide.Model.Battlescape.Combatants;
using ProjectXenocide.Model.StaticData;
using ProjectXenocide.Model.StaticData.Items;
using ProjectXenocide.UI.Dialogs;
using ProjectXenocide.UI.Screens;
using ProjectXenocide.Utils;

using Xenocide.Resources;

namespace ProjectXenocide.UI.Screens
{
    public partial class BattlescapeScreen
    {
        private sealed class PickActionDialog : ModalDialog
        {
            public PickActionDialog(BattlescapeScreen battlescapeScreen, Item item, Combatant combatant, bool rightHand)
            {
                this.battlescapeScreen = battlescapeScreen;
                this.item = item;
                this.combatant = combatant;
                this.rightHand = rightHand;
            }

            protected override void CreateDialogWidgets()
            {
                Combatant.ActiveArm activeArm = Combatant.ActiveArm.Both;
                if (combatant.Inventory.ItemAt(rightHand ? 1 : 0, 0) != null)
                {
                    activeArm = rightHand ? Combatant.ActiveArm.Right : Combatant.ActiveArm.Left;
                }

                int index = 0;
                foreach (ActionInfo action in item.ItemInfo.Actions)
                {
                    int idx = index;
                    var btn = new Button();
                    btn.Text = action.MenuEntry(combatant, activeArm);
                    btn.Click += (s, e) => DoSelectedAction(idx);
                    ContentArea.AddChild(btn);
                    ++index;
                }

                var cancelBtn = new Button();
                cancelBtn.Text = Strings.BUTTON_CANCEL;
                cancelBtn.Click += OnCancelClicked;
                ContentArea.AddChild(cancelBtn);
            }

            public void OnCancelClicked(object sender, EventArgs e)
            {
                ScreenManager.CloseDialog(this);
            }

            private void DoSelectedAction(int actionIndex)
            {
                ActionInfo action = item.ItemInfo.Actions[actionIndex];

                ActionError error = action.ActionAvailable(battlescapeScreen.battlescape, item, combatant);
                if (ActionError.None != error)
                {
                    Util.ShowMessageBox(ActionInfo.ErrorText(error));
                }
                else
                {
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

            private BattlescapeScreen battlescapeScreen;
            private Item item;
            private Combatant combatant;
            private bool rightHand;
        }
    }
}
