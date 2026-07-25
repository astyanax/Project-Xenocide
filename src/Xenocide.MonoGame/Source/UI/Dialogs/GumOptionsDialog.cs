using System;

using Gum.Forms.Controls;

using ProjectXenocide.Assets;
using ProjectXenocide.UI.Screens;

namespace ProjectXenocide.UI.Dialogs
{
    sealed class GumOptionsDialog : ModalDialog
    {
        public GumOptionsDialog() : base("Options") { }
        protected override void CreateDialogWidgets()
        {
            var loadBtn = new Button();
            loadBtn.Text = "Load";
            loadBtn.Click += OnLoadClicked;
            ContentArea.AddChild(loadBtn);

            var saveBtn = new Button();
            saveBtn.Text = "Save";
            saveBtn.Click += OnSaveClicked;
            ContentArea.AddChild(saveBtn);

            var soundBtn = new Button();
            soundBtn.Text = "Sound";
            soundBtn.Click += OnSoundClicked;
            ContentArea.AddChild(soundBtn);

            var abandonBtn = new Button();
            abandonBtn.Text = "Abandon";
            abandonBtn.Click += OnAbandonClicked;
            ContentArea.AddChild(abandonBtn);

            var cancelBtn = new Button();
            cancelBtn.Text = "Cancel";
            cancelBtn.Click += OnCancelClicked;
            ContentArea.AddChild(cancelBtn);
        }

        public void OnCancelClicked(object sender, EventArgs e)
        {
            Dismiss();
        }

        public void OnAbandonClicked(object sender, EventArgs e)
        {
            Screen screen = new StartScreen();
            Close();
            ScreenManager.ScheduleScreen(screen);
        }

        public void OnLoadClicked(object sender, EventArgs e)
        {
            Screen screen = new LoadSaveGameScreen(
                    LoadSaveGameScreen.Mode.Load,
                    LoadSaveGameScreen.CancelScreen.Geoscape);
            Close();
            ScreenManager.ScheduleScreen(screen);
        }

        public void OnSaveClicked(object sender, EventArgs e)
        {
            Screen screen = new LoadSaveGameScreen(
                    LoadSaveGameScreen.Mode.Save,
                    LoadSaveGameScreen.CancelScreen.Geoscape);
            Close();
            ScreenManager.ScheduleScreen(screen);
        }

        public void OnSoundClicked(object sender, EventArgs e)
        {
            Close();
            ScreenManager.ShowDialog(new SoundOptionsDialog());
        }
    }
}
