using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using Gum.Forms;
using Gum.Forms.Controls;

using Microsoft.Xna.Framework;

using ProjectXenocide.Assets;
using ProjectXenocide.Model;
using ProjectXenocide.Model.StaticData;

namespace ProjectXenocide.UI.Screens
{
    public class SettingsScreen : GumScreen
    {
        private int _musicLevel;
        private int _soundLevel;
        private bool _fullscreen;
        private bool _notifications;
        private bool _autosave;
        private int _cursorMode;
        private int _resolutionIdx;
        private int _difficultyIdx;

        private readonly string[] _resolutions = { "1280x1024", "1280x720", "1920x1080", "3840x2160" };
        private readonly string[] _cursorModes = { "Software", "Hardware" };
        private readonly Difficulty[] _difficultyValues = { Difficulty.Easy, Difficulty.Hard, Difficulty.Sadistic };

        private StackPanel _contentPanel;
        private StackPanel _rootPanel;

        public SettingsScreen()
            : base("SettingsScreen")
        {
            var options = GameOptions.LoadFromFile();
            _musicLevel = (int)(Xenocide.AudioSystem.MusicVolume * 10);
            _soundLevel = (int)(Xenocide.AudioSystem.SoundVolume * 10);
            _fullscreen = Xenocide.Instance.GraphicsDevice.PresentationParameters.IsFullScreen;
            _notifications = true;
            _autosave = false;
            _cursorMode = Xenocide.Instance.IsMouseVisible ? 1 : 0;
            _resolutionIdx = 0;
            _difficultyIdx = (int)StartSettings.Difficulty;
        }

        protected override bool HasGumxLayout => false;

        protected override void CreateGumControls()
        {
            _rootPanel = RootContainer;
            _rootPanel.Width = 600;
            _rootPanel.Visual.X = 340;
            _rootPanel.Visual.Y = 50;

            var title = new Label { Text = "Settings" };
            title.Height = 40;
            _rootPanel.AddChild(title);

            var tabBar = new StackPanel();
            tabBar.Visual.Width = 600;
            tabBar.Visual.Height = 35;

            var displayTab = MakeTabButton("Display", () => ShowDisplayTab());
            var soundTab = MakeTabButton("Sound", () => ShowSoundTab());
            var notifTab = MakeTabButton("Notifications", () => ShowNotificationTab());
            var gameTab = MakeTabButton("Gameplay", () => ShowGamePlayTab());

            tabBar.AddChild(displayTab);
            tabBar.AddChild(soundTab);
            tabBar.AddChild(notifTab);
            tabBar.AddChild(gameTab);
            _rootPanel.AddChild(tabBar);

            _contentPanel = new StackPanel();
            _contentPanel.Visual.Width = 600;
            _rootPanel.AddChild(_contentPanel);

            var spacer = new Label { Height = 20 };
            _rootPanel.AddChild(spacer);

            var saveBtn = new Button { Text = "Save" };
            saveBtn.Click += OnSaveClicked;
            _rootPanel.AddChild(saveBtn);

            var cancelBtn = new Button { Text = "Cancel" };
            cancelBtn.Click += OnCancelClicked;
            _rootPanel.AddChild(cancelBtn);

            ShowDisplayTab();
        }

        private static Button MakeTabButton(string text, Action action)
        {
            var btn = new Button { Text = text };
            btn.Visual.Width = 150;
            btn.Click += (s, e) =>
            {
                Xenocide.AudioSystem?.PlaySound(SoundId.ButtonClick1);
                action();
            };
            return btn;
        }

        private void ShowDisplayTab()
        {
            ClearContent();
            AddSettingRow("Resolution:", _resolutions[_resolutionIdx],
                () => { _resolutionIdx = (_resolutionIdx + 1) % _resolutions.Length; return _resolutions[_resolutionIdx]; });
            AddSettingRow("Fullscreen:", _fullscreen ? "ON" : "OFF",
                () => { _fullscreen = !_fullscreen; return _fullscreen ? "ON" : "OFF"; });
            AddSettingRow("Cursor:", _cursorModes[_cursorMode],
                () => { _cursorMode = (_cursorMode + 1) % _cursorModes.Length; return _cursorModes[_cursorMode]; });
        }

        private void ShowSoundTab()
        {
            ClearContent();
            AddLabel("Music Volume:");
            AddVolumeRow(() => _musicLevel, v => _musicLevel = Math.Clamp(v, 0, 10));

            AddLabel("Sound Volume:");
            AddVolumeRow(() => _soundLevel, v => _soundLevel = Math.Clamp(v, 0, 10));
        }

        private void ShowNotificationTab()
        {
            ClearContent();
            AddSettingRow("Show Toast Notifications:", _notifications ? "ON" : "OFF",
                () => { _notifications = !_notifications; return _notifications ? "ON" : "OFF"; });
        }

        private void ShowGamePlayTab()
        {
            ClearContent();
            AddSettingRow("Difficulty:", _difficultyValues[_difficultyIdx].ToString(),
                () => { _difficultyIdx = (_difficultyIdx + 1) % _difficultyValues.Length; return _difficultyValues[_difficultyIdx].ToString(); });
            AddSettingRow("Autosave:", _autosave ? "ON" : "OFF",
                () => { _autosave = !_autosave; return _autosave ? "ON" : "OFF"; });
        }

        private void OnSaveClicked(object sender, EventArgs e)
        {
            Xenocide.AudioSystem.MusicVolume = _musicLevel / 10.0f;
            Xenocide.AudioSystem.SoundVolume = _soundLevel / 10.0f;

            var options = GameOptions.LoadFromFile();
            options.MusicVolume = Xenocide.AudioSystem.MusicVolume;
            options.SoundVolume = Xenocide.AudioSystem.SoundVolume;
            options.SaveToFile();

            if (_fullscreen != Xenocide.Instance.GraphicsDevice.PresentationParameters.IsFullScreen)
            {
                var gdm = Xenocide.Instance.Services.GetService<Microsoft.Xna.Framework.IGraphicsDeviceManager>() as Microsoft.Xna.Framework.GraphicsDeviceManager;
                gdm.ToggleFullScreen();
            }

            Xenocide.Instance.IsMouseVisible = _cursorMode == 1;
            ScreenManager.ScheduleScreen(new StartScreen());
        }

        private void OnCancelClicked(object sender, EventArgs e)
        {
            ScreenManager.ScheduleScreen(new StartScreen());
        }

        private void ClearContent()
        {
            if (_contentPanel?.Visual != null)
            {
                var children = _contentPanel.Visual.Children.ToList();
                foreach (var child in children)
                    _contentPanel.Visual.Children.Remove(child);
            }
        }

        private void AddLabel(string text)
        {
            if (_contentPanel == null) return;
            var label = new Label { Text = text };
            _contentPanel.AddChild(label);
        }

        private void AddSettingRow(string labelText, string initialValue, Func<string> onToggle)
        {
            if (_contentPanel == null) return;
            var row = new StackPanel();
            row.Visual.Width = 600;
            row.Visual.Height = 30;

            var label = new Label { Text = labelText };
            label.Visual.Width = 350;
            row.AddChild(label);

            var btn = new Button { Text = initialValue };
            btn.Visual.Width = 200;
            btn.Click += (s, e) =>
            {
                Xenocide.AudioSystem?.PlaySound(SoundId.ButtonClick1);
                btn.Text = onToggle();
            };
            row.AddChild(btn);

            _contentPanel.AddChild(row);
        }

        private void AddVolumeRow(Func<int> getLevel, Action<int> setLevel)
        {
            if (_contentPanel == null) return;

            var row = new StackPanel();
            row.Visual.Width = 600;
            row.Visual.Height = 30;

            var downBtn = new Button { Text = "<" };
            downBtn.Visual.Width = 40;
            row.AddChild(downBtn);

            var levelLabel = new Label { Text = getLevel().ToString(CultureInfo.InvariantCulture) };
            levelLabel.Visual.Width = 40;
            row.AddChild(levelLabel);

            var upBtn = new Button { Text = ">" };
            upBtn.Visual.Width = 40;
            row.AddChild(upBtn);

            downBtn.Click += (s, e) =>
            {
                Xenocide.AudioSystem?.PlaySound(SoundId.ButtonClick1);
                setLevel(getLevel() - 1);
                levelLabel.Text = getLevel().ToString(CultureInfo.InvariantCulture);
            };
            upBtn.Click += (s, e) =>
            {
                Xenocide.AudioSystem?.PlaySound(SoundId.ButtonClick1);
                setLevel(getLevel() + 1);
                levelLabel.Text = getLevel().ToString(CultureInfo.InvariantCulture);
            };

            _contentPanel.AddChild(row);
        }
    }
}
