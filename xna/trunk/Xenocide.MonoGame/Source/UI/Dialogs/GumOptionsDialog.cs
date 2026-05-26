using System;

using Gum.Forms.Controls;

using ProjectXenocide.UI.Screens;

namespace ProjectXenocide.UI.Dialogs
{
    class GumOptionsDialog : ModalDialog
    {
        public GumOptionsDialog()
        {
        }

        protected override void CreateDialogWidgets()
        {
            var loadButton = new Button();
            loadButton.Text = "Load Game";
            loadButton.Click += OnLoadClicked;
            ContentArea.AddChild(loadButton);

            var saveButton = new Button();
            saveButton.Text = "Save Game";
            saveButton.Click += OnSaveClicked;
            ContentArea.AddChild(saveButton);

            var soundButton = new Button();
            soundButton.Text = "Sound Options";
            soundButton.Click += OnSoundClicked;
            ContentArea.AddChild(soundButton);

            var abandonButton = new Button();
            abandonButton.Text = "Abandon Game";
            abandonButton.Click += OnAbandonClicked;
            ContentArea.AddChild(abandonButton);

            var cancelButton = new Button();
            cancelButton.Text = "Cancel";
            cancelButton.Click += OnCancelClicked;
            ContentArea.AddChild(cancelButton);
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
