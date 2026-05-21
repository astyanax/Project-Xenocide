using System;
using System.Collections.Generic;
using System.Text;

using Gum.Forms.Controls;

using ProjectXenocide.Utils;

using ProjectXenocide.UI.Screens;

using ProjectXenocide.Model.Geoscape;
using ProjectXenocide.Model.Battlescape;

namespace ProjectXenocide.UI.Dialogs
{
    class StartBattlescapeDialog : GumDialog
    {
        public StartBattlescapeDialog(Mission mission)
        {
            this.mission = mission;
        }

        protected override void CreateGumWidgets()
        {
            var details = new Label();
            details.Text = mission.MakeStartMissionText();
            RootContainer.AddChild(details);

            var okBtn = new Button();
            okBtn.Text = "OK";
            okBtn.Click += OnOkClicked;
            RootContainer.AddChild(okBtn);

            var cancelBtn = new Button();
            cancelBtn.Text = "Cancel";
            cancelBtn.Click += OnCancelClicked;
            RootContainer.AddChild(cancelBtn);

            if (Xenocide.StaticTables.StartSettings.Cheats.AllowAutoWinBattlescape)
            {
                var autoBtn = new Button();
                autoBtn.Text = "Auto Complete";
                autoBtn.Click += OnAutoCompleteClicked;
                RootContainer.AddChild(autoBtn);
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
            ScreenManager.CloseDialog(this);
        }

        private void DoBattlescape()
        {
            Xenocide.GameState.Battlescape = new Battle(mission);
            ScreenManager.ScheduleScreen(new BattlescapeScreen());
            ScreenManager.CloseDialog(this);
        }

        private void DoCancel()
        {
            mission.DontStart();
            ScreenManager.CloseDialog(this);
        }

        private Mission mission;
    }
}
