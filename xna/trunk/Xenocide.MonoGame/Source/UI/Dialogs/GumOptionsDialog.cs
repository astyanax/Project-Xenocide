using System;

using Gum.Forms.Controls;

using ProjectXenocide.UI.Screens;

namespace ProjectXenocide.UI.Dialogs
{
    class GumOptionsDialog : GumDialog
    {
        public GumOptionsDialog()
        {
        }

        protected override void CreateGumWidgets()
        {
            var loadButton = new Button();
            loadButton.Text = "Load Game";
            loadButton.Click += OnLoadClicked;
            RootContainer.AddChild(loadButton);

            var saveButton = new Button();
            saveButton.Text = "Save Game";
            saveButton.Click += OnSaveClicked;
            RootContainer.AddChild(saveButton);

            var soundButton = new Button();
            soundButton.Text = "Sound Options";
            soundButton.Click += OnSoundClicked;
            RootContainer.AddChild(soundButton);

            var abandonButton = new Button();
            abandonButton.Text = "Abandon Game";
            abandonButton.Click += OnAbandonClicked;
            RootContainer.AddChild(abandonButton);

            var cancelButton = new Button();
            cancelButton.Text = "Cancel";
            cancelButton.Click += OnCancelClicked;
            RootContainer.AddChild(cancelButton);
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
