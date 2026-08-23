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
            AddButton("Load", OnLoadClicked);
            AddButton("Save", OnSaveClicked);
            AddButton("Sound", OnSoundClicked);
            AddButton("Abandon", OnAbandonClicked);
            AddButton("Cancel", OnCancelClicked);
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
