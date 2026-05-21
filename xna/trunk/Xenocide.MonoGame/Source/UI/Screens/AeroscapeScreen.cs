using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

using Microsoft.Xna.Framework;

using Gum.Forms.Controls;

using ProjectXenocide.Utils;
using ProjectXenocide.Model.Geoscape;
using ProjectXenocide.Model.Geoscape.Outposts;
using ProjectXenocide.Model.Geoscape.Vehicles;
using ProjectXenocide.Model.StaticData.Items;
using ProjectXenocide.Model.Battlescape;
using Xenocide.Resources;

namespace ProjectXenocide.UI.Screens
{
    public class AeroscapeScreen : GumScreen
    {
        public AeroscapeScreen(Aircraft aircraft, Ufo ufo)
            : base("AeroscapeScreen")
        {
            this.aircraft = aircraft;
            this.ufo = ufo;

            aircraft.OnDogfightStart();
            ufo.OnDogfightStart();
        }

        protected override void CreateGumControls()
        {
            craftNameLabel = new Label();
            craftNameLabel.Text = aircraft.Name;
            RootContainer.AddChild(craftNameLabel);

            craftDamageLabel = new Label();
            RootContainer.AddChild(craftDamageLabel);

            pod1Label = new Label();
            RootContainer.AddChild(pod1Label);

            pod2Label = new Label();
            RootContainer.AddChild(pod2Label);

            logLabel = new Label();
            RootContainer.AddChild(logLabel);

            realTimeBtn = new Button();
            realTimeBtn.Text = Strings.BUTTON_REAL_TIME;
            realTimeBtn.Click += OnRealTimeButton;
            RootContainer.AddChild(realTimeBtn);

            advanceTimeBtn = new Button();
            advanceTimeBtn.Text = Strings.BUTTON_ADVANCE_TIME;
            advanceTimeBtn.Click += OnAdvanceTimeButton;
            RootContainer.AddChild(advanceTimeBtn);

            closeBtn = new Button();
            closeBtn.Text = Strings.BUTTON_CLOSE;
            closeBtn.Click += OnCloseButton;
            RootContainer.AddChild(closeBtn);

            DrawScreen();
        }

        private Label craftNameLabel;
        private Label craftDamageLabel;
        private Label pod1Label;
        private Label pod2Label;
        private Label logLabel;
        private Button realTimeBtn;
        private Button advanceTimeBtn;
        private Button closeBtn;

        private void OnRealTimeButton(object sender, EventArgs e)
        {
            runRealTime = true;
        }

        private void OnAdvanceTimeButton(object sender, EventArgs e)
        {
            runRealTime = false;
            UpdateDogfight();
        }

        private void OnCloseButton(object sender, EventArgs e)
        {
            if (!ufo.IsDestroyed)
            {
                ufo.OnDogfightFinished();
            }
            if (!aircraft.IsDestroyed)
            {
                aircraft.OnDogfightFinished();
            }

            ScreenManager.ScheduleScreen(new GeoscapeScreen());
        }

        public override void Update(GameTime gameTime)
        {
            if (runRealTime)
            {
                elapsed += gameTime.ElapsedGameTime.TotalMilliseconds;
                if (1000.0 < elapsed)
                {
                    UpdateDogfight();
                }
            }
        }

        private void DrawScreen()
        {
            DrawAircraftDamage();
            DrawPodInformation();
            DrawLog();
        }

        private void DrawAircraftDamage()
        {
            craftDamageLabel.Text = Util.StringFormat(Strings.SCREEN_AEROSCAPE_HULL_LEFT, aircraft.HullPercent);
        }

        private void DrawPodInformation()
        {
            pod1Label.Text = "";
            pod2Label.Text = "";

            if (0 < aircraft.WeaponPods.Count)
            {
                DrawPodInformation(pod1Label, aircraft.WeaponPods[0], 1);
            }

            if (1 < aircraft.WeaponPods.Count)
            {
                DrawPodInformation(pod2Label, aircraft.WeaponPods[1], 2);
            }
        }

        private static void DrawPodInformation(Label textControl, WeaponPod pod, int podId)
        {
            StringBuilder info = new StringBuilder(string.Format("Pod {0}", podId));
            info.Append(Util.Linefeed);
            info.Append(WeaponPod.PodInformationString(pod));
            textControl.Text = info.ToString();
        }

        private void DrawLog()
        {
            StringBuilder sb = new StringBuilder(string.Format("Time: {0}", log.Now));
            foreach (BattleLog.LogEntry entry in log.Entries)
            {
                if (entry.Time == log.Now)
                {
                    sb.Append(Util.Linefeed);
                    sb.Append(entry.Details);
                }
            }
            logLabel.Text = sb.ToString();
        }

        private void UpdateDogfight()
        {
            elapsed = 0.0;

            if (!dogfightOver)
            {
                int logsize = log.Entries.Count;
                do
                {
                    log.UpdateTime(1.0);

                    Attack(aircraft, ufo);
                    if (!dogfightOver)
                    {
                        if (ufo.IsArmed)
                        {
                            Attack(ufo, aircraft);
                        }
                    }
                } while (!dogfightOver && log.Entries.Count == logsize);
                DrawScreen();
            }
        }

        private void Attack(Craft attacker, Craft target)
        {
            AttackResult result = attacker.Attack(target, log);
            switch (result)
            {
                case AttackResult.OpponentCrashed:
                case AttackResult.OpponentDestroyed:
                case AttackResult.OpponentFled:
                case AttackResult.OutOfAmmo:
                    dogfightOver = true;
                    break;

                case AttackResult.Nothing:
                    break;

                default:
                    Debug.Assert(false);
                    break;
            }
        }

        private bool dogfightOver;
        private Ufo ufo;
        private Aircraft aircraft;
        private BattleLog log = new BattleLog();
        bool runRealTime;
        double elapsed;
    }
}
