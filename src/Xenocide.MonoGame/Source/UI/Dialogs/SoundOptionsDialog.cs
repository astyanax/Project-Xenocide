using System;
using System.Globalization;

using Gum.DataTypes;
using Gum.Forms.Controls;

using ProjectXenocide.Assets;
using ProjectXenocide.Model;
using ProjectXenocide.UI.Controls;
using ProjectXenocide.UI.Screens;

namespace ProjectXenocide.UI.Dialogs
{
    /// <summary>
    /// Music / sound-effect volume options.
    ///
    /// <para>
    /// Each channel has an on/off toggle, a slider (0–10) and a readout. Volume
    /// changes are applied live so the player can hear the result; the sound
    /// slider also plays a test effect. Cancel/Escape restores the values that
    /// were in effect when the dialog opened.
    /// </para>
    /// </summary>
    sealed class SoundOptionsDialog : ModalDialog
    {
        private const int MaxLevel = 10;

        private Button musicToggleBtn;
        private Button soundToggleBtn;
        private Slider musicSlider;
        private Slider soundSlider;
        private Label musicLevelLabel;
        private Label soundLevelLabel;

        private int musicLevel;
        private bool musicEnabled;
        private int soundLevel;
        private bool soundEnabled;

        private float musicLast;
        private float soundLast;

        private bool _initializing;

        public SoundOptionsDialog()
            : base("Sound Options")
        {
            musicLevel = (int)Math.Round(Xenocide.AudioSystem.MusicVolume * MaxLevel);
            musicEnabled = Xenocide.AudioSystem.MusicVolume > 0;
            soundLevel = (int)Math.Round(Xenocide.AudioSystem.SoundVolume * MaxLevel);
            soundEnabled = Xenocide.AudioSystem.SoundVolume > 0;
            musicLast = Xenocide.AudioSystem.MusicVolume;
            soundLast = Xenocide.AudioSystem.SoundVolume;
            PanelWidth = 520;
            PanelHeight = 380;

            // Ensure Escape (which calls Dismiss) restores the previous volumes,
            // matching the Cancel button.
            DismissAction = RestoreVolumes;
        }

        protected override void CreateDialogWidgets()
        {
            _initializing = true;

            // Music section
            musicToggleBtn = AddButton(musicEnabled ? "Music: ON" : "Music: OFF", OnMusicToggleClicked);
            musicSlider = AddSlider(musicLevel, OnMusicSliderChanged);
            musicLevelLabel = AddValueLabel();

            // Sound section
            soundToggleBtn = AddButton(soundEnabled ? "Sound: ON" : "Sound: OFF", OnSoundToggleClicked);
            soundSlider = AddSlider(soundLevel, OnSoundSliderChanged);
            soundLevelLabel = AddValueLabel();

            UpdateLabels();

            _initializing = false;

            // Action buttons
            AddActionButton("Save", OnSaveClicked);
            AddActionButton("Cancel", OnCancelClicked);
        }

        private Slider AddSlider(int value, EventHandler onChanged)
        {
            var slider = new Slider();
            slider.Minimum = 0;
            slider.Maximum = MaxLevel;
            slider.Value = value;
            slider.Visual.Width = 0;
            slider.Visual.WidthUnits = DimensionUnitType.RelativeToParent;
            slider.Visual.Height = 24;
            slider.Visual.HeightUnits = DimensionUnitType.Absolute;
            slider.ValueChanged += onChanged;
            ContentArea.AddChild(slider);
            return slider;
        }

        private Label AddValueLabel()
        {
            var label = ThemedLabel.CreateBody("");
            ContentArea.AddChild(label);
            return label;
        }

        private void UpdateLabels()
        {
            musicLevelLabel.Text = "Music: " + (musicEnabled
                ? musicLevel.ToString(CultureInfo.InvariantCulture) : "OFF");
            soundLevelLabel.Text = "Sound: " + (soundEnabled
                ? soundLevel.ToString(CultureInfo.InvariantCulture) : "OFF");
        }

        private void ApplyMusicVolume()
        {
            Xenocide.AudioSystem.MusicVolume = musicEnabled ? musicLevel / (float)MaxLevel : 0;
        }

        private void ApplySoundVolume()
        {
            Xenocide.AudioSystem.SoundVolume = soundEnabled ? soundLevel / (float)MaxLevel : 0;
        }

        public void OnMusicToggleClicked(object sender, EventArgs e)
        {
            musicEnabled = !musicEnabled;
            musicToggleBtn.Text = musicEnabled ? "Music: ON" : "Music: OFF";
            ApplyMusicVolume();
            UpdateLabels();
        }

        public void OnSoundToggleClicked(object sender, EventArgs e)
        {
            soundEnabled = !soundEnabled;
            soundToggleBtn.Text = soundEnabled ? "Sound: ON" : "Sound: OFF";
            ApplySoundVolume();
            UpdateLabels();
        }

        private void OnMusicSliderChanged(object sender, EventArgs e)
        {
            if (_initializing)
                return;

            musicLevel = (int)Math.Round(musicSlider.Value);
            if (musicEnabled)
                ApplyMusicVolume();
            UpdateLabels();
        }

        private void OnSoundSliderChanged(object sender, EventArgs e)
        {
            if (_initializing)
                return;

            soundLevel = (int)Math.Round(soundSlider.Value);
            if (soundEnabled)
                ApplySoundVolume();
            UpdateLabels();

            // Audible feedback for the new effect volume.
            Xenocide.AudioSystem?.PlaySound(SoundId.ButtonClick2);
        }

        public void OnSaveClicked(object sender, EventArgs e)
        {
            ApplyMusicVolume();
            ApplySoundVolume();

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
