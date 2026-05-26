using System;
using System.Collections.Generic;
using System.Text;

using Gum.Forms.Controls;

using ProjectXenocide.Model;
using ProjectXenocide.Model.Geoscape;
using ProjectXenocide.Model.Geoscape.Vehicles;
using ProjectXenocide.UI.Screens;
using ProjectXenocide.Utils;

namespace ProjectXenocide.UI.Dialogs
{
    class SoundOptionsDialog : ModalDialog
    {
        public SoundOptionsDialog()
        {
        }

        protected override void CreateDialogWidgets()
        {
            musicToggleBtn = new Button();
            UpdateMusicToggleText();
            musicToggleBtn.Click += OnMusicToggleClicked;
            ContentArea.AddChild(musicToggleBtn);

            musicUpBtn = new Button();
            musicUpBtn.Text = "Music +";
            musicUpBtn.Click += (s, e) => { musicLevel = Math.Min(10, musicLevel + 1); UpdateMusicVolume(); };
            ContentArea.AddChild(musicUpBtn);

            musicDownBtn = new Button();
            musicDownBtn.Text = "Music -";
            musicDownBtn.Click += (s, e) => { musicLevel = Math.Max(0, musicLevel - 1); UpdateMusicVolume(); };
            ContentArea.AddChild(musicDownBtn);

            musicLevelLabel = new Label();
            UpdateMusicLabel();
            ContentArea.AddChild(musicLevelLabel);

            soundToggleBtn = new Button();
            UpdateSoundToggleText();
            soundToggleBtn.Click += OnSoundToggleClicked;
            ContentArea.AddChild(soundToggleBtn);

            soundUpBtn = new Button();
            soundUpBtn.Text = "Sound +";
            soundUpBtn.Click += (s, e) => { soundLevel = Math.Min(10, soundLevel + 1); UpdateSoundVolume(); };
            ContentArea.AddChild(soundUpBtn);

            soundDownBtn = new Button();
            soundDownBtn.Text = "Sound -";
            soundDownBtn.Click += (s, e) => { soundLevel = Math.Max(0, soundLevel - 1); UpdateSoundVolume(); };
            ContentArea.AddChild(soundDownBtn);

            soundLevelLabel = new Label();
            UpdateSoundLabel();
            ContentArea.AddChild(soundLevelLabel);

            var saveBtn = new Button();
            saveBtn.Text = "Save";
            saveBtn.Click += OnSaveClicked;
            ContentArea.AddChild(saveBtn);

            var cancelBtn = new Button();
            cancelBtn.Text = "Cancel";
            cancelBtn.Click += OnCancelClicked;
            ContentArea.AddChild(cancelBtn);
        }

        private Button musicToggleBtn;
        private Button musicUpBtn;
        private Button musicDownBtn;
        private Label musicLevelLabel;
        private int musicLevel = (int)(Xenocide.AudioSystem.MusicVolume * 10);
        private bool musicEnabled = Xenocide.AudioSystem.MusicVolume > 0;

        private Button soundToggleBtn;
        private Button soundUpBtn;
        private Button soundDownBtn;
        private Label soundLevelLabel;
        private int soundLevel = (int)(Xenocide.AudioSystem.SoundVolume * 10);
        private bool soundEnabled = Xenocide.AudioSystem.SoundVolume > 0;

        private float musicLast = Xenocide.AudioSystem.MusicVolume;
        private float soundLast = Xenocide.AudioSystem.SoundVolume;

        private void UpdateMusicToggleText()
        {
            musicToggleBtn.Text = musicEnabled ? "Music: ON" : "Music: OFF";
        }

        private void UpdateSoundToggleText()
        {
            soundToggleBtn.Text = soundEnabled ? "Sound: ON" : "Sound: OFF";
        }

        private void UpdateMusicLabel()
        {
            musicLevelLabel.Text = "Music: " + (musicEnabled ? musicLevel.ToString() : "OFF");
        }

        private void UpdateSoundLabel()
        {
            soundLevelLabel.Text = "Sound: " + (soundEnabled ? soundLevel.ToString() : "OFF");
        }

        private void UpdateMusicVolume()
        {
            Xenocide.AudioSystem.MusicVolume = musicEnabled ? (musicLevel / 10.0f) : 0;
            UpdateMusicLabel();
        }

        private void UpdateSoundVolume()
        {
            Xenocide.AudioSystem.SoundVolume = soundEnabled ? (soundLevel / 10.0f) : 0;
            UpdateSoundLabel();
        }

        public void OnMusicToggleClicked(object sender, EventArgs e)
        {
            musicEnabled = !musicEnabled;
            UpdateMusicToggleText();
            UpdateMusicVolume();
        }

        public void OnSoundToggleClicked(object sender, EventArgs e)
        {
            soundEnabled = !soundEnabled;
            UpdateSoundToggleText();
            UpdateSoundVolume();
        }

        public void OnSaveClicked(object sender, EventArgs e)
        {
            var gameOptions = GameOptions.LoadFromFile();
            gameOptions.MusicVolume = musicEnabled ? (musicLevel / 10.0f) : 0;
            gameOptions.SoundVolume = soundEnabled ? (soundLevel / 10.0f) : 0;
            gameOptions.SaveToFile();

            ScreenManager.CloseDialog(this);
        }

        public void OnCancelClicked(object sender, EventArgs e)
        {
            Xenocide.AudioSystem.MusicVolume = musicLast;
            Xenocide.AudioSystem.SoundVolume = soundLast;

            ScreenManager.CloseDialog(this);
        }
    }
}
