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
            PanelWidth = 560;
            PanelHeight = 220;
        }

        protected override void CreateDialogWidgets()
        {
            AddBodyText(Util.StringFormat(Strings.DLG_TRACKINGLOST_LOST_TRACKING, hunter.Name));

            AddActionButton(Strings.BUTTON_RETURN_TO_BASE, OnReturnClicked, 150);
            AddActionButton(Strings.BUTTON_PATROL, OnPatrolClicked, 120);
            AddActionButton("Last Position", OnLastKnownClicked, 150);
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
