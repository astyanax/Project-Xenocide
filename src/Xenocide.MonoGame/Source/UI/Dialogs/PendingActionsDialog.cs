using System;
using System.Collections.Generic;
using System.Linq;

using Gum.Forms;
using Gum.Forms.Controls;

using ProjectXenocide.UI.Controls;
using ProjectXenocide.Utils;

namespace ProjectXenocide.UI.Dialogs
{
    /// <summary>
    /// The "situation report" inbox.  Lists messages that require the player's
    /// attention (Required entries), offering a context action ("Go To ...") and
    /// per-entry / blanket dismissal.
    /// </summary>
    /// <remarks>
    /// Opened from the envelope button on the geoscape situation log or with the
    /// global "M" hotkey.  Dismissing an entry clears it from the badge but keeps
    /// it in the log; taking the action resolves it.
    /// </remarks>
    public class PendingActionsDialog : ModalDialog
    {
        private const int ButtonHeight = 22;
        private const int RowHeight = 30;
        private const int ActionButtonWidth = 120;
        private const int DismissButtonWidth = 96;

        public PendingActionsDialog()
            : base("Situation Report")
        {
            PanelWidth = 720;
            PanelHeight = 460;
        }

        protected override void CreateDialogWidgets()
        {
            Rebuild();

            AddActionButton("Dismiss All", (s, e) => { MessageLog.DismissAll(); Rebuild(); }, 150);
            AddActionButton("Close", (s, e) => Close(), 120);
        }

        /// <summary>Rebuild the list of pending entries (called after a dismissal).</summary>
        private void Rebuild()
        {
            ContentArea.Visual.Children.Clear();

            List<MessageEntry> pending = MessageLog.PendingRequired.ToList();
            if (pending.Count == 0)
            {
                AddBodyText("No pending actions. All notifications have been dealt with.");
                return;
            }

            AddBodyText($"{pending.Count} item(s) require your attention:");
            foreach (MessageEntry entry in pending)
            {
                ContentArea.AddChild(BuildRow(entry));
            }
        }

        private Panel BuildRow(MessageEntry entry)
        {
            var row = new Panel();
            row.Visual.Width = 0;
            row.Visual.WidthUnits = Gum.DataTypes.DimensionUnitType.RelativeToParent;
            row.Visual.Height = RowHeight;
            row.Visual.HeightUnits = Gum.DataTypes.DimensionUnitType.Absolute;

            var label = ThemedLabel.CreateBody($"{entry.TimeString}  {entry.DisplayText}");
            label.Visual.X = 0;
            label.Visual.Y = 6;
            label.Visual.XUnits = Gum.Converters.GeneralUnitType.PixelsFromSmall;
            label.Visual.YUnits = Gum.Converters.GeneralUnitType.PixelsFromSmall;
            label.Visual.Width = -(ActionButtonWidth + DismissButtonWidth + 24);
            label.Visual.WidthUnits = Gum.DataTypes.DimensionUnitType.RelativeToParent;
            row.Visual.Children.Add(label.Visual);

            if (NotificationActions.CanInvoke(entry.ActionId))
            {
                var action = ThemedButton.Create(NotificationActions.LabelFor(entry.ActionId), (s, e) => OnAction(entry));
                ThemedButton.SetWidth(action, ActionButtonWidth);
                PositionRight(action, -(DismissButtonWidth + 6));
                row.Visual.Children.Add(action.Visual);
            }

            var dismiss = ThemedButton.Create("Dismiss", (s, e) => { MessageLog.Dismiss(entry.Id); Rebuild(); });
            ThemedButton.SetWidth(dismiss, DismissButtonWidth);
            PositionRight(dismiss, 0);
            row.Visual.Children.Add(dismiss.Visual);

            return row;
        }

        private void OnAction(MessageEntry entry)
        {
            MessageLog.Action(entry.Id);
            Close();
            NotificationActions.Invoke(entry.ActionId, entry.TargetId);
        }

        private static void PositionRight(FrameworkElement control, int xFromRight)
        {
            control.Visual.Height = ButtonHeight;
            control.Visual.HeightUnits = Gum.DataTypes.DimensionUnitType.Absolute;
            control.Visual.Y = 4;
            control.Visual.YUnits = Gum.Converters.GeneralUnitType.PixelsFromSmall;
            control.Visual.XOrigin = RenderingLibrary.Graphics.HorizontalAlignment.Right;
            control.Visual.XUnits = Gum.Converters.GeneralUnitType.PixelsFromLarge;
            control.Visual.X = xFromRight;
        }
    }
}
