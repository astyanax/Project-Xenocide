using System;
using System.Globalization;

using Gum.Forms.Controls;

using ProjectXenocide.Model;
using ProjectXenocide.UI.Controls;
using ProjectXenocide.UI.Screens;

namespace ProjectXenocide.UI.Dialogs
{
    sealed class SoundOptionsDialog : ModalDialog
    {
        private Button musicToggleBtn;
        private Button musicUpBtn;
        private Button musicDownBtn;
        private Button soundToggleBtn;
        private Button soundUpBtn;
        private Button soundDownBtn;
        private Label musicLevelLabel;
        private Label soundLevelLabel;

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
            PanelWidth = 500;
        }

        protected override void CreateDialogWidgets()
        {
            // Music section
            musicToggleBtn = new Button();
            musicToggleBtn.Text = musicEnabled ? "Music: ON" : "Music: OFF";
            musicToggleBtn.Click += OnMusicToggleClicked;
            ContentArea.AddChild(musicToggleBtn);

            musicDownBtn = new Button();
            musicDownBtn.Text = "Music -";
            musicDownBtn.Click += (s, e) => { musicLevel = Math.Max(0, musicLevel - 1); UpdateMusicLabel(); };
            ContentArea.AddChild(musicDownBtn);

            musicUpBtn = new Button();
            musicUpBtn.Text = "Music +";
            musicUpBtn.Click += (s, e) => { musicLevel = Math.Min(10, musicLevel + 1); UpdateMusicLabel(); };
            ContentArea.AddChild(musicUpBtn);

            musicLevelLabel = ThemedLabel.CreateBody("Music: " + (musicEnabled ? musicLevel.ToString(CultureInfo.InvariantCulture) : "OFF"));
            ContentArea.AddChild(musicLevelLabel);

            // Sound section
            soundToggleBtn = new Button();
            soundToggleBtn.Text = soundEnabled ? "Sound: ON" : "Sound: OFF";
            soundToggleBtn.Click += OnSoundToggleClicked;
            ContentArea.AddChild(soundToggleBtn);

            soundDownBtn = new Button();
            soundDownBtn.Text = "Sound -";
            soundDownBtn.Click += (s, e) => { soundLevel = Math.Max(0, soundLevel - 1); UpdateSoundLabel(); };
            ContentArea.AddChild(soundDownBtn);

            soundUpBtn = new Button();
            soundUpBtn.Text = "Sound +";
            soundUpBtn.Click += (s, e) => { soundLevel = Math.Min(10, soundLevel + 1); UpdateSoundLabel(); };
            ContentArea.AddChild(soundUpBtn);

            soundLevelLabel = ThemedLabel.CreateBody("Sound: " + (soundEnabled ? soundLevel.ToString(CultureInfo.InvariantCulture) : "OFF"));
            ContentArea.AddChild(soundLevelLabel);

            // Action buttons
            var saveBtn = new Button();
            saveBtn.Text = "Save";
            saveBtn.Click += OnSaveClicked;
            ContentArea.AddChild(saveBtn);

            var cancelBtn = new Button();
            cancelBtn.Text = "Cancel";
            cancelBtn.Click += OnCancelClicked;
            ContentArea.AddChild(cancelBtn);
        }

        private void UpdateMusicLabel()
        {
            musicLevelLabel.Text = "Music: " + (musicEnabled ? musicLevel.ToString(CultureInfo.InvariantCulture) : "OFF");
        }

        private void UpdateSoundLabel()
        {
            soundLevelLabel.Text = "Sound: " + (soundEnabled ? soundLevel.ToString(CultureInfo.InvariantCulture) : "OFF");
        }

        public void OnMusicToggleClicked(object sender, EventArgs e)
        {
            musicEnabled = !musicEnabled;
            musicToggleBtn.Text = musicEnabled ? "Music: ON" : "Music: OFF";
            UpdateMusicLabel();
        }

        public void OnSoundToggleClicked(object sender, EventArgs e)
        {
            soundEnabled = !soundEnabled;
            soundToggleBtn.Text = soundEnabled ? "Sound: ON" : "Sound: OFF";
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

            Close();
        }

        public void OnCancelClicked(object sender, EventArgs e)
        {
            Xenocide.AudioSystem.MusicVolume = musicLast;
            Xenocide.AudioSystem.SoundVolume = soundLast;
            Dismiss();
        }
    }
}
