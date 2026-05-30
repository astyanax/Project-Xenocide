using System;
using System.Collections.Generic;
using System.Text;

using Gum.Forms.Controls;

using ProjectXenocide.Model.Geoscape;
using ProjectXenocide.Model.Geoscape.Vehicles;
using ProjectXenocide.UI.Screens;
using ProjectXenocide.Utils;

using Xenocide.Resources;

namespace ProjectXenocide.UI.Dialogs
{
    sealed class AircraftOrdersDialog : ModalDialog
    {
        public AircraftOrdersDialog(Aircraft craft)
        {
            this.craft = craft;
        }

        protected override void CreateDialogWidgets()
        {
            var details = new Label();
            details.Text = MakeDialogText();
            ContentArea.AddChild(details);

            var returnBtn = new Button();
            returnBtn.Text = Strings.BUTTON_RETURN_TO_BASE;
            returnBtn.Click += OnReturnClicked;
            ContentArea.AddChild(returnBtn);

            var targetBtn = new Button();
            targetBtn.Text = "Target";
            targetBtn.Click += OnTargetClicked;
            ContentArea.AddChild(targetBtn);

            var cancelBtn = new Button();
            cancelBtn.Text = Strings.BUTTON_CANCEL;
            cancelBtn.Click += OnCancelClicked;
            ContentArea.AddChild(cancelBtn);
        }

        public void OnReturnClicked(object sender, EventArgs e)
        {
            SetReturnToBaseMission();
        }

        public void OnTargetClicked(object sender, EventArgs e)
        {
            NewTarget();
        }

        public void OnCancelClicked(object sender, EventArgs e)
        {
            ScreenManager.CloseDialog(this);
        }

        private void SetReturnToBaseMission()
        {
            craft.Mission.Abort();
            craft.Mission = new PatrolMission(craft, craft.HomeBase.Position);
            craft.Mission.SetState(new ReturnToBaseState(craft.Mission));
            ScreenManager.CloseDialog(this);
        }

        private void NewTarget()
        {
            GeoscapeScreen geoscapeScreen = new GeoscapeScreen();
            geoscapeScreen.State = new GeoscapeScreen.TargetingScreenState(geoscapeScreen, craft);
            ScreenManager.ScheduleScreen(geoscapeScreen);
            ScreenManager.CloseDialog(this);
        }

        private String MakeDialogText()
        {
            StringBuilder sb = new StringBuilder(craft.Name);
            sb.Append(Util.Linefeed);
            sb.Append(Util.StringFormat(Strings.MSGBOX_AIRCRAFT_ORDERS_BASE, craft.HomeBase.Name));
            sb.Append(Util.Linefeed);
            sb.Append(Util.StringFormat(Strings.MSGBOX_AIRCRAFT_ORDERS_SPEED, craft.MetersPerSecond));
            sb.Append(Util.Linefeed);
            sb.Append(Util.StringFormat(Strings.MSGBOX_AIRCRAFT_ORDERS_FUEL, craft.FuelPercent));
            sb.Append(Util.Linefeed);
            foreach (WeaponPod pod in craft.WeaponPods)
            {
                if (null != pod)
                {
                    sb.Append(pod.PodInformationString());
                    sb.Append(Util.Linefeed);
                }
            }
            return sb.ToString();
        }

        private Aircraft craft;
    }
}
