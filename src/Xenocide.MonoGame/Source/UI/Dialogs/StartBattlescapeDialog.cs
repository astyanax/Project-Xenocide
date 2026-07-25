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

            var okBtn = new Button();
            okBtn.Text = "OK";
            okBtn.Click += OnOkClicked;
            ContentArea.AddChild(okBtn);

            var cancelBtn = new Button();
            cancelBtn.Text = "Cancel";
            cancelBtn.Click += OnCancelClicked;
            ContentArea.AddChild(cancelBtn);

            if (Xenocide.StaticTables.StartSettings.Cheats.AllowAutoWinBattlescape)
            {
                var autoBtn = new Button();
                autoBtn.Text = "Auto Complete";
                autoBtn.Click += OnAutoCompleteClicked;
                ContentArea.AddChild(autoBtn);
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
