using System;
using System.Collections.Generic;
using System.Globalization;

using Gum.Forms.Controls;

using Microsoft.Xna.Framework;

using ProjectXenocide.Model.Battlescape;
using ProjectXenocide.UI.Controls;

namespace ProjectXenocide.UI.Screens
{
    /// <summary>
    /// The Strategic Engagement screen: replaces the tactical battlescape.
    /// Shows both forces, the pre-battle odds prediction (Monte-Carlo), and lets
    /// the player Engage or Withdraw.  After engaging it plays back the round log
    /// (with a skip-to-end option) before the mission report.
    /// </summary>
    public class EngagementScreen : GumScreen
    {
        private const double RoundDelay = 0.35;

        private readonly Mission mission;
        private EngagementSession session;
        private EngagementResult result;

        private ScreenLayout layout;
        private ContentArea content;
        private ListBox logList;
        private Button engageButton;
        private Button withdrawButton;
        private Button skipButton;
        private Button continueButton;

        private bool resolving;
        private int revealedRound;
        private double revealTimer;

        public EngagementScreen(Mission mission)
            : base("EngagementScreen")
        {
            this.mission = mission;
        }

        protected override bool HasGumxLayout => false;

        protected override void CreateGumControls()
        {
            session = new EngagementSession(mission);

            layout = new ScreenLayout();
            layout.AddToRoot();
            content = new ContentArea(layout.ContentPanel);

            engageButton = layout.AddButton("Engage", OnEngage);
            withdrawButton = layout.AddButton("Withdraw", OnWithdraw);
            skipButton = layout.AddButton("Skip to End", OnSkipToEnd);
            continueButton = layout.AddButton("Continue", OnContinue);
            skipButton.Visual.Visible = false;
            continueButton.Visual.Visible = false;

            BuildBriefing();
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (resolving && (revealedRound < result.Rounds))
            {
                revealTimer += gameTime.ElapsedGameTime.TotalSeconds;
                while ((revealTimer >= RoundDelay) && (revealedRound < result.Rounds))
                {
                    revealTimer -= RoundDelay;
                    RevealTo(revealedRound + 1);
                }
            }
        }

        private void BuildBriefing()
        {
            content.Clear();
            content.AddHeader(mission.MakeStartMissionText());
            content.AddLabel(ThemedLabel.CreateBody(PredictionText(session.Prediction)));
            content.AddHeader("Your squad");
            content.AddGrid(BuildForceGrid(session.XCorp));
            content.AddHeader("Enemy forces");
            content.AddGrid(BuildForceGrid(session.Aliens));
        }

        private static StyledGrid BuildForceGrid(IReadOnlyList<CombatantProfile> profiles)
        {
            var grid = new StyledGrid();
            grid.AddColumn("Unit", 240);
            grid.AddColumn("Acc", 80);
            grid.AddColumn("HP", 80);
            grid.AddColumn("Dmg", 80);

            int row = 0;
            foreach (CombatantProfile profile in profiles)
            {
                grid.AddRow(row++, profile.Name,
                    profile.Accuracy.ToString(CultureInfo.InvariantCulture),
                    profile.Health.ToString(CultureInfo.InvariantCulture),
                    profile.Damage.ToString(CultureInfo.InvariantCulture));
            }
            return grid;
        }

        private void OnEngage(object sender, EventArgs e)
        {
            result = session.Engage();
            resolving = true;
            revealedRound = 0;
            revealTimer = 0;

            engageButton.Visual.Visible = false;
            withdrawButton.Visual.Visible = false;
            skipButton.Visual.Visible = true;
            continueButton.Visual.Visible = true;

            content.Clear();
            content.AddHeader("Engagement — " + FinishText(result.Finish));
            content.AddLabel(ThemedLabel.CreateBody(ResultText(result)));

            logList = new ListBox();
            logList.Visual.Height = 320;
            logList.Visual.HeightUnits = Gum.DataTypes.DimensionUnitType.Absolute;
            content.Panel.AddChild(logList);

            RevealTo(1);
        }

        private void OnSkipToEnd(object sender, EventArgs e)
        {
            if (result != null)
            {
                RevealTo(result.Rounds);
            }
        }

        private void OnContinue(object sender, EventArgs e)
        {
            ScreenManager.ScheduleScreen(new BattlescapeReportScreen(mission, result));
        }

        private void OnWithdraw(object sender, EventArgs e)
        {
            session.Withdraw();
            ScreenManager.ScheduleScreen(new GeoscapeScreen());
        }

        private void RevealTo(int round)
        {
            revealedRound = Math.Max(revealedRound, round);
            if ((logList == null) || (result == null))
            {
                return;
            }

            logList.Items.Clear();
            foreach (EngagementLogEntry entry in result.Log.Entries)
            {
                if (entry.Round <= revealedRound)
                {
                    logList.Items.Add(string.Format(CultureInfo.InvariantCulture, "[{0}] {1}", entry.Round, entry.Text));
                }
            }
            if (logList.Items.Count > 0)
            {
                logList.SelectedIndex = logList.Items.Count - 1;
            }
        }

        private static string PredictionText(EngagementPrediction prediction)
        {
            string band = prediction.WinProbability >= 0.75
                ? "Favourable"
                : (prediction.WinProbability >= 0.40 ? "Even" : "Poor");

            double pct = prediction.WinProbability * 100;
            return string.Format(CultureInfo.InvariantCulture,
                "Odds: {0} [{1:0}% win].  Expected (KIA {2:0.0}, wounded {3:0.0}, alien kills {4:0.0}).",
                band, pct, prediction.ExpectedXCorpKia, prediction.ExpectedXCorpWounded, prediction.ExpectedAlienKills);
        }

        private static string ResultText(EngagementResult result)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "Rounds: {0}.  Losses: {1} KIA, {2} wounded.  Aliens killed: {3}.",
                result.Rounds, result.XCorpKia, result.XCorpWounded, result.AlienKills);
        }

        private static string FinishText(BattleFinish finish)
        {
            switch (finish)
            {
                case BattleFinish.XCorpVictory: return "X-Corp Victory";
                case BattleFinish.AlienVictory: return "Alien Victory";
                case BattleFinish.Aborted: return "Disengaged";
                default: return "Unknown";
            }
        }
    }
}
