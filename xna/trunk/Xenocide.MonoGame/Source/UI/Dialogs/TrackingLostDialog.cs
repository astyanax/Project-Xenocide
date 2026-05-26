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
    class TrackingLostDialog : ModalDialog
    {
        public TrackingLostDialog(GeoPosition target, Craft hunter)
        {
            this.target = target;
            this.hunter = hunter;
        }

        protected override void CreateDialogWidgets()
        {
            var details = new Label();
            details.Text = Util.StringFormat(Strings.DLG_TRACKINGLOST_LOST_TRACKING, hunter.Name);
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
            ScreenManager.CloseDialog(this);
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
            ScreenManager.CloseDialog(this);
        }

        private GeoPosition target;
        private Craft hunter;
    }
}
