using System;
using System.Collections.Generic;
using System.Text;

using Gum.Forms.Controls;

using ProjectXenocide.Model.Battlescape;
using ProjectXenocide.Model.Geoscape;
using ProjectXenocide.UI.Controls;
using ProjectXenocide.UI.Screens;
using ProjectXenocide.Utils;

namespace ProjectXenocide.UI.Dialogs
{
    sealed class StartBattlescapeDialog : ModalDialog
    {
        public StartBattlescapeDialog(Mission mission) : base("Battlescape")
        {
            this.mission = mission;
            PanelWidth = 560;
            PanelHeight = 260;
        }

        protected override void CreateDialogWidgets()
        {
            AddBodyText(mission.MakeStartMissionText());

            AddActionButton("OK", OnOkClicked);
            AddActionButton("Cancel", OnCancelClicked);

            if (Xenocide.StaticTables.StartSettings.Cheats.AllowAutoWinBattlescape)
            {
                AddActionButton("Auto Complete", OnAutoCompleteClicked, 150);
            }
        }

        public void OnOkClicked(object sender, EventArgs e)
        {
            DoBattlescape();
        }

        public void OnCancelClicked(object sender, EventArgs e)
        {
            DoCancel();
        }

        public void OnAutoCompleteClicked(object sender, EventArgs e)
        {
            Xenocide.GameState.Battlescape = new Battle(mission);
            mission.OnFinish(Xenocide.GameState.Battlescape, BattleFinish.XCorpVictory);
            Xenocide.GameState.Battlescape.PostMissionCleanup();
            Xenocide.GameState.Battlescape = null;
            ScreenManager.ScheduleScreen(new BattlescapeReportScreen(mission));
            Close();
        }

        private void DoBattlescape()
        {
            Xenocide.GameState.Battlescape = new Battle(mission);
            ScreenManager.ScheduleScreen(new BattlescapeScreen());
            Close();
        }

        private void DoCancel()
        {
            mission.DontStart();
            Dismiss();
        }

        private Mission mission;
    }
}
