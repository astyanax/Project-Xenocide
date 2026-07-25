using System;
using System.Collections.Generic;
using System.Text;

using Gum.Forms.Controls;

using ProjectXenocide.Model.Geoscape;
using ProjectXenocide.Model.Geoscape.Vehicles;
using ProjectXenocide.UI.Controls;
using ProjectXenocide.UI.Screens;
using ProjectXenocide.Utils;

using Xenocide.Resources;

namespace ProjectXenocide.UI.Dialogs
{
    sealed class TrackingLostDialog : ModalDialog
    {
        public TrackingLostDialog(GeoPosition target, Craft hunter) : base("Tracking Lost")
        {
            this.target = target;
            this.hunter = hunter;
        }

        protected override void CreateDialogWidgets()
        {
            var details = ThemedLabel.CreateBody(Util.StringFormat(Strings.DLG_TRACKINGLOST_LOST_TRACKING, hunter.Name));
            ContentArea.AddChild(details);

            var returnBtn = new Button();
            returnBtn.Text = Strings.BUTTON_RETURN_TO_BASE;
            returnBtn.Click += OnReturnClicked;
            ContentArea.AddChild(returnBtn);

            var patrolBtn = new Button();
            patrolBtn.Text = Strings.BUTTON_PATROL;
            patrolBtn.Click += OnPatrolClicked;
            ContentArea.AddChild(patrolBtn);

            var lastKnownBtn = new Button();
            lastKnownBtn.Text = "Go to Last Known Position";
            lastKnownBtn.Click += OnLastKnownClicked;
            ContentArea.AddChild(lastKnownBtn);
        }

        public void OnReturnClicked(object sender, EventArgs e)
        {
            Dismiss();
        }

        public void OnPatrolClicked(object sender, EventArgs e)
        {
            SetPatrol(hunter.Position);
        }

        public void OnLastKnownClicked(object sender, EventArgs e)
        {
            SetPatrol(target);
        }

        private void SetPatrol(GeoPosition position)
        {
            hunter.Mission.Abort();
            hunter.Mission = new PatrolMission(hunter, position);
            Close();
        }

        private GeoPosition target;
        private Craft hunter;
    }
}
