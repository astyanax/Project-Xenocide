using System;

using Gum.Forms.Controls;

using ProjectXenocide.Assets;
using ProjectXenocide.UI.Screens;

namespace ProjectXenocide.UI.Dialogs
{
    sealed class GumOptionsDialog : GumDialog
    {
        public GumOptionsDialog() : base("Options") { }
        protected override void WireGumControls()
        {
            base.WireGumControls();

            var loadBtn = GetButton("LoadButton");
            if (loadBtn != null) loadBtn.Click += OnLoadClicked;

            var saveBtn = GetButton("SaveButton");
            if (saveBtn != null) saveBtn.Click += OnSaveClicked;

            var soundBtn = GetButton("SoundButton");
            if (soundBtn != null) soundBtn.Click += OnSoundClicked;

            var abandonBtn = GetButton("AbandonButton");
            if (abandonBtn != null) abandonBtn.Click += OnAbandonClicked;

            var cancelBtn = GetButton("CancelButton");
            if (cancelBtn != null) cancelBtn.Click += OnCancelClicked;
        }

        public void OnCancelClicked(object sender, EventArgs e)
        {
            ScreenManager.CloseDialog(this);
        }

        public void OnAbandonClicked(object sender, EventArgs e)
        {
            Screen screen = new StartScreen();
            ScreenManager.CloseDialog(this);
            ScreenManager.ScheduleScreen(screen);
        }

        public void OnLoadClicked(object sender, EventArgs e)
        {
            Screen screen = new LoadSaveGameScreen(
                    LoadSaveGameScreen.Mode.Load,
                    LoadSaveGameScreen.CancelScreen.Geoscape);
            ScreenManager.CloseDialog(this);
            ScreenManager.ScheduleScreen(screen);
        }

        public void OnSaveClicked(object sender, EventArgs e)
        {
            Screen screen = new LoadSaveGameScreen(
                    LoadSaveGameScreen.Mode.Save,
                    LoadSaveGameScreen.CancelScreen.Geoscape);
            ScreenManager.CloseDialog(this);
            ScreenManager.ScheduleScreen(screen);
        }

        public void OnSoundClicked(object sender, EventArgs e)
        {
            ScreenManager.CloseDialog(this);
            ScreenManager.ShowDialog(new SoundOptionsDialog());
        }
    }
}
