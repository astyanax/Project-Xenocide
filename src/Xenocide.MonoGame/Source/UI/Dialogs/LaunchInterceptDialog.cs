using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

using Gum.Forms.Controls;

using ProjectXenocide.Assets;
using ProjectXenocide.Model.Geoscape;
using ProjectXenocide.Model.Geoscape.Outposts;
using ProjectXenocide.Model.Geoscape.Vehicles;
using ProjectXenocide.UI.Controls;
using ProjectXenocide.UI.Screens;
using ProjectXenocide.Utils;

using Xenocide.Resources;

namespace ProjectXenocide.UI.Dialogs
{
    sealed class LaunchInterceptDialog : ModalDialog
    {
        public LaunchInterceptDialog() : base("Select Interceptor")
        {
            PanelWidth = 600;
        }

        protected override void CreateDialogWidgets()
        {
            int rowNum = 0;
            foreach (Outpost outpost in Xenocide.GameState.GeoData.Outposts)
            {
                foreach (Craft craft in outpost.Fleet)
                {
                    Aircraft aircraft = (Aircraft)craft;
                    int row = rowNum;
                    var label = ThemedLabel.CreateBody(
                        string.Format(CultureInfo.InvariantCulture, "{0} - {1} (Fuel:{2}% Hull:{3}%)",
                        aircraft.Name, aircraft.HomeBase.Name, aircraft.FuelPercent, aircraft.HullPercent));
                    ContentArea.AddChild(label);

                    // Select plays a distinct ButtonClick2, so suppress the auto ButtonClick1.
                    var selectBtn = ThemedButton.Create("Select",
                        (s, e) => BringUpGeoscapeInTargetingMode(aircraft), playSound: false);
                    ContentArea.AddChild(selectBtn);

                    rowToCraft[rowNum] = aircraft;
                    ++rowNum;
                }
            }

            AddButton(Strings.BUTTON_CANCEL, OnCancelClicked);
        }

        private Dictionary<int, Aircraft> rowToCraft = new Dictionary<int, Aircraft>();

        public void OnCancelClicked(object sender, EventArgs e)
        {
            Dismiss();
        }

        private void BringUpGeoscapeInTargetingMode(Aircraft aircraft)
        {
            Xenocide.AudioSystem.PlaySound(SoundId.ButtonClick2);
            GeoscapeScreen geoscapeScreen = new GeoscapeScreen();
            geoscapeScreen.State = new GeoscapeScreen.TargetingScreenState(geoscapeScreen, aircraft);
            ScreenManager.ScheduleScreen(geoscapeScreen);
            Close();
        }
    }
}
