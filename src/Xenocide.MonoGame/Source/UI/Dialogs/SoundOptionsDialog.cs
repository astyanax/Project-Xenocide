using System;
using System.Globalization;

using Gum.Forms.Controls;

using ProjectXenocide.Model;
using ProjectXenocide.UI.Screens;

namespace ProjectXenocide.UI.Dialogs
{
    sealed class SoundOptionsDialog : GumDialog
    {
        private Button musicToggleBtn;
        private Button musicUpBtn;
        private Button musicDownBtn;
        private Button soundToggleBtn;
        private Button soundUpBtn;
        private Button soundDownBtn;

        private int musicLevel;
        private bool musicEnabled;
        private int soundLevel;
        private bool soundEnabled;

        private float musicLast;
        private float soundLast;

        public SoundOptionsDialog()
            : base("Sound Options")
        {
            musicLevel = (int)(Xenocide.AudioSystem.MusicVolume * 10);
            musicEnabled = Xenocide.AudioSystem.MusicVolume > 0;
            soundLevel = (int)(Xenocide.AudioSystem.SoundVolume * 10);
            soundEnabled = Xenocide.AudioSystem.SoundVolume > 0;
            musicLast = Xenocide.AudioSystem.MusicVolume;
            soundLast = Xenocide.AudioSystem.SoundVolume;
        }

        protected override void WireGumControls()
        {
            base.WireGumControls();

            musicToggleBtn = GetButton("MusicToggleBtn");
            musicUpBtn = GetButton("MusicUpBtn");
            musicDownBtn = GetButton("MusicDownBtn");
            soundToggleBtn = GetButton("SoundToggleBtn");
            soundUpBtn = GetButton("SoundUpBtn");
            soundDownBtn = GetButton("SoundDownBtn");
            var saveBtn = GetButton("SaveBtn");
            var cancelBtn = GetButton("CancelBtn");

            if (musicToggleBtn != null) musicToggleBtn.Click += OnMusicToggleClicked;
            if (musicUpBtn != null) musicUpBtn.Click += (s, e) => { musicLevel = Math.Min(10, musicLevel + 1); UpdateMusicLabel(); };
            if (musicDownBtn != null) musicDownBtn.Click += (s, e) => { musicLevel = Math.Max(0, musicLevel - 1); UpdateMusicLabel(); };
            if (soundToggleBtn != null) soundToggleBtn.Click += OnSoundToggleClicked;
            if (soundUpBtn != null) soundUpBtn.Click += (s, e) => { soundLevel = Math.Min(10, soundLevel + 1); UpdateSoundLabel(); };
            if (soundDownBtn != null) soundDownBtn.Click += (s, e) => { soundLevel = Math.Max(0, soundLevel - 1); UpdateSoundLabel(); };
            if (saveBtn != null) saveBtn.Click += OnSaveClicked;
            if (cancelBtn != null) cancelBtn.Click += OnCancelClicked;

            UpdateMusicLabel();
            UpdateMusicToggleText();
            UpdateSoundLabel();
            UpdateSoundToggleText();
        }

        private void UpdateMusicToggleText()
        {
            if (musicToggleBtn != null) musicToggleBtn.Text = musicEnabled ? "Music: ON" : "Music: OFF";
        }

        private void UpdateSoundToggleText()
        {
            if (soundToggleBtn != null) soundToggleBtn.Text = soundEnabled ? "Sound: ON" : "Sound: OFF";
        }

        private void UpdateMusicLabel()
        {
            SetText("MusicLevelLabel", "Music: " + (musicEnabled ? musicLevel.ToString(CultureInfo.InvariantCulture) : "OFF"));
        }

        private void UpdateSoundLabel()
        {
            SetText("SoundLevelLabel", "Sound: " + (soundEnabled ? soundLevel.ToString(CultureInfo.InvariantCulture) : "OFF"));
        }

        public void OnMusicToggleClicked(object sender, EventArgs e)
        {
            musicEnabled = !musicEnabled;
            UpdateMusicToggleText();
            UpdateMusicLabel();
        }

        public void OnSoundToggleClicked(object sender, EventArgs e)
        {
            soundEnabled = !soundEnabled;
            UpdateSoundToggleText();
            UpdateSoundLabel();
        }

        public void OnSaveClicked(object sender, EventArgs e)
        {
            Xenocide.AudioSystem.MusicVolume = musicEnabled ? (musicLevel / 10.0f) : 0;
            Xenocide.AudioSystem.SoundVolume = soundEnabled ? (soundLevel / 10.0f) : 0;

            var options = GameOptions.LoadFromFile();
            options.MusicVolume = Xenocide.AudioSystem.MusicVolume;
            options.SoundVolume = Xenocide.AudioSystem.SoundVolume;
            options.SaveToFile();

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
