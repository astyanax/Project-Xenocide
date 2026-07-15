using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

using Gum.Forms.Controls;

using ProjectXenocide.Assets;
using ProjectXenocide.Model.Geoscape;
using ProjectXenocide.Model.Geoscape.Outposts;
using ProjectXenocide.Model.Geoscape.Vehicles;
using ProjectXenocide.UI.Screens;
using ProjectXenocide.Utils;

using Xenocide.Resources;

namespace ProjectXenocide.UI.Dialogs
{
    sealed class LaunchInterceptDialog : GumDialog
    {
        public LaunchInterceptDialog() : base("Select Interceptor")
        {
        }

        protected override void WireGumControls()
        {
            base.WireGumControls();
            var content = GetOrCreateContentPanel();

            int rowNum = 0;
            foreach (Outpost outpost in Xenocide.GameState.GeoData.Outposts)
            {
                foreach (Craft craft in outpost.Fleet)
                {
                    Aircraft aircraft = (Aircraft)craft;
                    int row = rowNum;
                    var label = new Label();
                    label.Text = string.Format(CultureInfo.InvariantCulture, "{0} - {1} (Fuel:{2}% Hull:{3}%)",
                        aircraft.Name, aircraft.HomeBase.Name, aircraft.FuelPercent, aircraft.HullPercent);
                    content.AddChild(label);

                    var selectBtn = new Button();
                    selectBtn.Text = "Select";
                    selectBtn.Click += (s, e) => BringUpGeoscapeInTargetingMode(aircraft);
                    content.AddChild(selectBtn);

                    rowToCraft[rowNum] = aircraft;
                    ++rowNum;
                }
            }

            var cancelBtn = new Button();
            cancelBtn.Text = Strings.BUTTON_CANCEL;
            cancelBtn.Click += OnCancelClicked;
            content.AddChild(cancelBtn);
        }

        private Dictionary<int, Aircraft> rowToCraft = new Dictionary<int, Aircraft>();

        public void OnCancelClicked(object sender, EventArgs e)
        {
            ScreenManager.CloseDialog(this);
        }

        private void BringUpGeoscapeInTargetingMode(Aircraft aircraft)
        {
            Xenocide.AudioSystem.PlaySound(SoundId.ButtonClick2);
            GeoscapeScreen geoscapeScreen = new GeoscapeScreen();
            geoscapeScreen.State = new GeoscapeScreen.TargetingScreenState(geoscapeScreen, aircraft);
            ScreenManager.ScheduleScreen(geoscapeScreen);
            ScreenManager.CloseDialog(this);
        }
    }
}
