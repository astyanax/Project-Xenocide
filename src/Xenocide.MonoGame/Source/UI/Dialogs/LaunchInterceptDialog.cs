using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

using Gum.Forms.Controls;

using ProjectXenocide.Assets;
using ProjectXenocide.Model.Geoscape;
using ProjectXenocide.Model.Geoscape.AI;
using ProjectXenocide.Model.Geoscape.Outposts;
using ProjectXenocide.Model.Geoscape.Vehicles;
using ProjectXenocide.UI.Controls;
using ProjectXenocide.UI.Screens;
using ProjectXenocide.Utils;

using Xenocide.Resources;

namespace ProjectXenocide.UI.Dialogs
{
    /// <summary>
    /// Lets the player pick a craft to send after a target.
    ///
    /// <para>
    /// With no target, it puts the geoscape into targeting mode so the player can
    /// click a destination (the geoscape "Intercept" button). With a target (a UFO
    /// or alien site the player clicked), it offers per-craft "Intercept" and
    /// "Follow" actions and shows which craft are already pursuing.
    /// </para>
    /// </summary>
    sealed class LaunchInterceptDialog : ModalDialog
    {
        private readonly Ufo targetUfo;
        private readonly AlienSite targetSite;
        private readonly Dictionary<int, Aircraft> rowToCraft = new Dictionary<int, Aircraft>();

        public LaunchInterceptDialog() : base("Select Interceptor")
        {
            PanelWidth = 600;
            PanelHeight = 400;
        }

        public LaunchInterceptDialog(Ufo target) : base("Intercept UFO")
        {
            targetUfo = target;
            PanelWidth = 600;
            PanelHeight = 480;
        }

        public LaunchInterceptDialog(AlienSite target) : base("Attack Alien Site")
        {
            targetSite = target;
            PanelWidth = 600;
            PanelHeight = 480;
        }

        protected override void CreateDialogWidgets()
        {
            bool targeted = targetUfo != null || targetSite != null;
            string targetName = targetUfo != null ? targetUfo.Name : targetSite?.Name;
            GeoPosition targetPosition = targetUfo != null ? targetUfo.Position : targetSite?.Position;

            if (targeted)
                AddBodyText(Util.StringFormat("Send a craft after {0}:", targetName));

            int rowNum = 0;
            foreach (Outpost outpost in Xenocide.GameState.GeoData.Outposts)
            {
                foreach (Craft craft in outpost.Fleet)
                {
                    if (!(craft is Aircraft aircraft))
                        continue;

                    if (!targeted)
                    {
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
                        continue;
                    }

                    bool pursuing = targetUfo != null && ReferenceEquals(aircraft.Prey, targetUfo);
                    string info = string.Format(CultureInfo.InvariantCulture,
                        "{0} - {1} (Fuel:{2}% Hull:{3}%){4}",
                        aircraft.Name, aircraft.HomeBase.Name, aircraft.FuelPercent, aircraft.HullPercent,
                        pursuing ? "  [pursuing]" : "");
                    ContentArea.AddChild(ThemedLabel.CreateBody(info));

                    if (!pursuing)
                    {
                        Aircraft captured = aircraft;
                        var intercept = ThemedButton.Create(
                            targetUfo != null ? "Intercept" : "Attack",
                            (s, e) => OnEngageClicked(captured), playSound: false);
                        ContentArea.AddChild(intercept);
                    }

                    Aircraft followCraft = aircraft;
                    var follow = ThemedButton.Create(
                        "Follow (patrol here)",
                        (s, e) => OnFollowClicked(followCraft, targetPosition), playSound: false);
                    ContentArea.AddChild(follow);
                }
            }

            if (!targeted)
                AddBodyText("Select a craft, then click a target on the globe.");

            AddActionButton(Strings.BUTTON_CANCEL, OnCancelClicked);
        }

        public void OnCancelClicked(object sender, EventArgs e)
        {
            Dismiss();
        }

        private void OnEngageClicked(Aircraft aircraft)
        {
            if (targetSite != null && !aircraft.IsCarryingSoldiers)
            {
                Util.ShowMessageBox(Strings.MSGBOX_NOT_CARRYING_SOLDIERS, aircraft.Name, targetSite.Name);
                return;
            }

            Xenocide.AudioSystem?.PlaySound(SoundId.ButtonClick2);
            aircraft.Mission?.Abort();

            if (targetUfo != null)
                aircraft.Mission = new InterceptMission(aircraft, targetUfo);
            else
                aircraft.Mission = new AttackAlienSiteMission(aircraft, targetSite);

            Close();
        }

        private void OnFollowClicked(Aircraft aircraft, GeoPosition position)
        {
            if (position == null)
                return;

            Xenocide.AudioSystem?.PlaySound(SoundId.ButtonClick2);
            aircraft.Mission?.Abort();
            aircraft.Mission = new PatrolMission(aircraft, position);
            Close();
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
