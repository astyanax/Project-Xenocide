using System;
using System.Collections.Generic;
using System.Text;

using Gum.Forms.Controls;

using ProjectXenocide.Model;
using ProjectXenocide.Model.Geoscape;
using ProjectXenocide.Model.Geoscape.Geography;
using ProjectXenocide.Model.Geoscape.Outposts;
using ProjectXenocide.Model.Geoscape.Vehicles;
using ProjectXenocide.UI.Screens;
using ProjectXenocide.Utils;

using Xenocide.Resources;

namespace ProjectXenocide.UI.Dialogs
{
    class UfoInfoDialog : GumDialog
    {
        public UfoInfoDialog(Ufo ufo)
        {
            this.ufo = ufo;
        }

        protected override void CreateGumWidgets()
        {
            var details = new Label();
            details.Text = MakeDialogText();
            RootContainer.AddChild(details);

            var cancelBtn = new Button();
            cancelBtn.Text = Strings.BUTTON_CLOSE;
            cancelBtn.Click += OnCancelClicked;
            RootContainer.AddChild(cancelBtn);
        }

        public void OnCancelClicked(object sender, EventArgs e)
        {
            ScreenManager.CloseDialog(this);
        }

        private String MakeDialogText()
        {
            StringBuilder sb = new StringBuilder(ufo.Name);
            sb.Append(Util.Linefeed);
            sb.Append(Util.StringFormat(Strings.MSGBOX_UFOINFO_SIZE, ufo.UfoItemInfo.UfoSize));
            sb.Append(Util.Linefeed);
            sb.Append(Util.StringFormat(Strings.MSGBOX_UFOINFO_SPEED, ufo.MetersPerSecond));

            if (UfoWithinDecodeTransmissionsRange())
            {
                sb.Append(Util.Linefeed);
                sb.Append(Util.StringFormat(Strings.MSGBOX_UFOINFO_CRAFT_TYPE, ufo.UfoItemInfo.Name));
                sb.Append(Util.Linefeed);
                sb.Append(Util.StringFormat(Strings.MSGBOX_UFOINFO_RACE, Races.DisplayString(ufo.Race)));
                sb.Append(Util.Linefeed);
                sb.Append(Util.StringFormat(Strings.MSGBOX_UFOINFO_MISSION, ufo.Task.Name));
                sb.Append(Util.Linefeed);

                GeoPosition target = ufo.Task.Centroid;
                PlanetRegion region = Xenocide.GameState.GeoData.Planet.GetRegionAtLocation(target);
                if (region != null)
                {
                    sb.Append(Util.StringFormat(Strings.MSGBOX_UFOINFO_ZONE, region.Name));
                }
            }
            return sb.ToString();
        }

        private bool UfoWithinDecodeTransmissionsRange()
        {
            bool decoding = false;
            foreach (Outpost outpost in Xenocide.GameState.GeoData.Outposts)
            {
                decoding |= (outpost.IsOnRadar(ufo.Position, true) & outpost.Statistics.CanDecodeTransmissions());
            }
            return decoding;
        }

        private Ufo ufo;
    }
}
