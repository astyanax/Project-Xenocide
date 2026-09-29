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

            // Default action (Escape / close): keep chasing the UFO's last known
            // position. The craft auto-patrols there unless the player chooses
            // otherwise.
            DismissAction = () => SetPatrolOrder(target);

            AddActionButton(Strings.BUTTON_LAST_POSITION, OnLastKnownClicked, 160);
            AddActionButton(Strings.BUTTON_RETURN_TO_BASE, OnReturnClicked, 160);
            AddActionButton(Strings.BUTTON_PATROL, OnPatrolClicked, 120);
        }

        /// <summary>The craft is already heading home; keep that order.</summary>
        public void OnReturnClicked(object sender, EventArgs e)
        {
            Close();
        }

        /// <summary>Patrol where the craft currently is.</summary>
        public void OnPatrolClicked(object sender, EventArgs e)
        {
            SetPatrolOrder(hunter.Position);
            Close();
        }

        /// <summary>Patrol the UFO's last known position.</summary>
        public void OnLastKnownClicked(object sender, EventArgs e)
        {
            SetPatrolOrder(target);
            Close();
        }

        private void SetPatrolOrder(GeoPosition position)
        {
            hunter.Mission?.Abort();
            hunter.Mission = new PatrolMission(hunter, position);
        }

        private GeoPosition target;
        private Craft hunter;
    }
}
