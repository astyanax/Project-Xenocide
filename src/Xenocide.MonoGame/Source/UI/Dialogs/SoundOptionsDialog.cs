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
        private Button soundToggleBtn;
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
            PanelHeight = 340;

            // Ensure Escape (which calls Dismiss) restores the previous volumes,
            // matching the Cancel button.
            DismissAction = RestoreVolumes;
        }

        protected override void CreateDialogWidgets()
        {
            // Music section
            musicToggleBtn = AddButton(musicEnabled ? "Music: ON" : "Music: OFF", OnMusicToggleClicked);
            AddButton("Music -", (s, e) => { musicLevel = Math.Max(0, musicLevel - 1); UpdateMusicLabel(); });
            AddButton("Music +", (s, e) => { musicLevel = Math.Min(10, musicLevel + 1); UpdateMusicLabel(); });

            musicLevelLabel = ThemedLabel.CreateBody("Music: " + (musicEnabled ? musicLevel.ToString(CultureInfo.InvariantCulture) : "OFF"));
            ContentArea.AddChild(musicLevelLabel);

            // Sound section
            soundToggleBtn = AddButton(soundEnabled ? "Sound: ON" : "Sound: OFF", OnSoundToggleClicked);
            AddButton("Sound -", (s, e) => { soundLevel = Math.Max(0, soundLevel - 1); UpdateSoundLabel(); });
            AddButton("Sound +", (s, e) => { soundLevel = Math.Min(10, soundLevel + 1); UpdateSoundLabel(); });

            soundLevelLabel = ThemedLabel.CreateBody("Sound: " + (soundEnabled ? soundLevel.ToString(CultureInfo.InvariantCulture) : "OFF"));
            ContentArea.AddChild(soundLevelLabel);

            // Action buttons
            AddActionButton("Save", OnSaveClicked);
            AddActionButton("Cancel", OnCancelClicked);
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
            Dismiss();
        }

        /// <summary>
        /// Restores the volumes captured when the dialog opened. Invoked from the
        /// Cancel button (via <see cref="Dismiss"/>) and from Escape.
        /// </summary>
        private void RestoreVolumes()
        {
            Xenocide.AudioSystem.MusicVolume = musicLast;
            Xenocide.AudioSystem.SoundVolume = soundLast;
        }
    }
}
