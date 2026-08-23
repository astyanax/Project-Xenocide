using System;
using System.Collections.Generic;
using System.Text;

using Gum.Forms.Controls;

using ProjectXenocide.Model.Geoscape.AI;
using ProjectXenocide.UI.Screens;

namespace ProjectXenocide.UI.Dialogs
{
    sealed class AlienMissionDialog : ModalDialog
    {
        public AlienMissionDialog() : base("Alien Mission")
        {
        }

        protected override void CreateDialogWidgets()
        {
            string[] missionNames = Enum.GetNames<AlienMission>();
            for (int i = 0; i < missionNames.Length; i++)
            {
                int idx = i;
                AddButton(missionNames[i], (s, e) => LaunchMission(idx));
            }

            AddButton("Cancel", (s, e) => Dismiss());
        }

        private void LaunchMission(int selectedMission)
        {
            AlienMission missionType = (AlienMission)Enum.Parse<AlienMission>(Enum.GetNames<AlienMission>()[selectedMission]);
            GeoscapeScreen screen = new GeoscapeScreen();
            screen.State = new GeoscapeScreen.TargetAlienMissionState(screen, missionType);
            ScreenManager.ScheduleScreen(screen);
            Close();
        }
    }
}
