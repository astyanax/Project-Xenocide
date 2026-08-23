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
        }

        protected override void CreateDialogWidgets()
        {
            var details = ThemedLabel.CreateBody(mission.MakeStartMissionText());
            ContentArea.AddChild(details);

            AddButton("OK", OnOkClicked);
            AddButton("Cancel", OnCancelClicked);

            if (Xenocide.StaticTables.StartSettings.Cheats.AllowAutoWinBattlescape)
            {
                AddButton("Auto Complete", OnAutoCompleteClicked);
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
