using System;
using System.Collections.Generic;
using System.Text;

using Gum.Forms.Controls;

using ProjectXenocide.Model.Geoscape.AI;
using ProjectXenocide.UI.Screens;

namespace ProjectXenocide.UI.Dialogs
{
    sealed class AlienMissionDialog : GumDialog
    {
        public AlienMissionDialog() : base("Alien Mission")
        {
        }

        protected override void WireGumControls()
        {
            base.WireGumControls();
            var content = GetOrCreateContentPanel();

            string[] missionNames = Enum.GetNames<AlienMission>();
            for (int i = 0; i < missionNames.Length; i++)
            {
                int idx = i;
                var btn = new Button();
                btn.Text = missionNames[i];
                btn.Click += (s, e) => LaunchMission(idx);
                content.AddChild(btn);
            }

            var cancelBtn = new Button();
            cancelBtn.Text = "Cancel";
            cancelBtn.Click += (s, e) => ScreenManager.CloseDialog(this);
            content.AddChild(cancelBtn);
        }

        private void LaunchMission(int selectedMission)
        {
            AlienMission missionType = (AlienMission)Enum.Parse<AlienMission>(Enum.GetNames<AlienMission>()[selectedMission]);
            GeoscapeScreen screen = new GeoscapeScreen();
            screen.State = new GeoscapeScreen.TargetAlienMissionState(screen, missionType);
            ScreenManager.ScheduleScreen(screen);
            ScreenManager.CloseDialog(this);
        }
    }
}
