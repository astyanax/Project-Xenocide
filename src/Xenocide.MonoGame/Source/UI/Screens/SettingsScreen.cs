using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using Gum.Converters;
using Gum.DataTypes;
using Gum.Forms;
using Gum.Forms.Controls;

using MonoGameGum.GueDeriving;

using ProjectXenocide.Assets;
using ProjectXenocide.Model;
using ProjectXenocide.Model.StaticData;
using ProjectXenocide.UI.Controls;

namespace ProjectXenocide.UI.Screens
{
    /// <summary>
    /// Application settings screen with tabs for display, sound, notifications,
    /// and gameplay options.
    /// </summary>
    /// <remarks>
    /// ARCHITECTURE: This screen intentionally has no .gusx layout, so it is built
    /// programmatically. The root is a vertical StackPanel (title, tab bar,
    /// per-tab content, action row); each horizontal row inside it is a
    /// <see cref="ThemedRow"/> container that positions its children explicitly
    /// (Gum StackPanels only stack vertically).
    ///
    /// SETTINGS MANAGED:
    /// - Display: Resolution, fullscreen, cursor mode
    /// - Sound: Music volume, sound volume
    /// - Notifications: Toast notification toggle
    /// - Gameplay: Difficulty, autosave toggle
    ///
    /// PERSISTENCE: Uses GameOptions.LoadFromFile() / SaveToFile() for persistence.
    /// </remarks>
    public class SettingsScreen : GumScreen
    {
        private const int ContentWidth = 600;

        private int _musicLevel;
        private int _soundLevel;
        private WindowMode _displayMode;
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
            : base("SettingsScreen", @"Content/Textures/UI/StartScreenBackground.png")
        {
            _musicLevel = (int)(Xenocide.AudioSystem.MusicVolume * 10);
            _soundLevel = (int)(Xenocide.AudioSystem.SoundVolume * 10);
            _displayMode = GameOptions.LoadFromFile().WindowMode;
            _notifications = true;
            _autosave = false;
            _cursorMode = Xenocide.Instance.IsMouseVisible ? 1 : 0;
            _resolutionIdx = 0;
            _difficultyIdx = (int)StartSettings.Difficulty;
        }

        protected override bool HasGumxLayout => false;

        protected override void CreateGumControls()
        {
            var viewport = Xenocide.Instance.GraphicsDevice.Viewport;

            _rootPanel = RootContainer;
            _rootPanel.Width = ContentWidth;
            _rootPanel.WidthUnits = DimensionUnitType.Absolute;
            _rootPanel.Visual.X = Math.Max(20, (viewport.Width - ContentWidth) / 2);
            _rootPanel.Visual.XUnits = GeneralUnitType.PixelsFromSmall;
            _rootPanel.Visual.Y = 60;
            _rootPanel.Visual.YUnits = GeneralUnitType.PixelsFromSmall;

            _rootPanel.AddChild(ThemedLabel.CreateTitle("Settings"));

            _rootPanel.Visual.Children.Add(BuildTabBar());
            _rootPanel.Visual.Children.Add(BuildContentPanel().Visual);
            _rootPanel.Visual.Children.Add(BuildSpacer(12));
            _rootPanel.Visual.Children.Add(BuildActionRow());

            ShowDisplayTab();
        }

        private ContainerRuntime BuildTabBar()
        {
            var tabBar = ThemedRow.Create(ContentWidth, 30);

            int x = 0;
            foreach (var (text, action) in new (string, Action)[]
            {
                ("Display", ShowDisplayTab),
                ("Sound", ShowSoundTab),
                ("Notifications", ShowNotificationTab),
                ("Gameplay", ShowGamePlayTab),
            })
            {
                var tab = ThemedButton.Create(text);
                ThemedButton.SetWidth(tab, 145);
                tab.Visual.Height = 28;
                tab.Visual.HeightUnits = DimensionUnitType.Absolute;
                tab.Visual.X = x;
                tab.Visual.XUnits = GeneralUnitType.PixelsFromSmall;
                tab.Click += (s, e) => action();
                tabBar.Children.Add(tab.Visual);
                x += 149;
            }

            return tabBar;
        }

        private StackPanel BuildContentPanel()
        {
            _contentPanel = new StackPanel();
            _contentPanel.Visual.Width = ContentWidth;
            _contentPanel.Visual.WidthUnits = DimensionUnitType.Absolute;
            return _contentPanel;
        }

        private ContainerRuntime BuildActionRow()
        {
            var row = ThemedRow.Create(ContentWidth, 32);

            var saveBtn = ThemedButton.Create("Save", OnSaveClicked);
            var cancelBtn = ThemedButton.Create("Cancel", OnCancelClicked);
            ThemedButton.SetWidth(saveBtn, 160);
            ThemedButton.SetWidth(cancelBtn, 160);
            saveBtn.Visual.Height = 28;
            saveBtn.Visual.HeightUnits = DimensionUnitType.Absolute;
            cancelBtn.Visual.Height = 28;
            cancelBtn.Visual.HeightUnits = DimensionUnitType.Absolute;

            ThemedRow.Place(row, saveBtn, 130);
            ThemedRow.Place(row, cancelBtn, 310);
            return row;
        }

        private static ContainerRuntime BuildSpacer(int height)
        {
            var spacer = new ContainerRuntime();
            spacer.Width = 1;
            spacer.WidthUnits = DimensionUnitType.Absolute;
            spacer.Height = height;
            spacer.HeightUnits = DimensionUnitType.Absolute;
            return spacer;
        }

        private void ShowDisplayTab()
        {
            ClearContent();
            AddSettingRow("Resolution:", _resolutions[_resolutionIdx],
                () => { _resolutionIdx = (_resolutionIdx + 1) % _resolutions.Length; return _resolutions[_resolutionIdx]; });
            AddSettingRow("Display mode:", DisplayModeManager.Label(_displayMode),
                () => { _displayMode = NextDisplayMode(_displayMode); return DisplayModeManager.Label(_displayMode); });
            AddSettingRow("Cursor:", _cursorModes[_cursorMode],
                () => { _cursorMode = (_cursorMode + 1) % _cursorModes.Length; return _cursorModes[_cursorMode]; });
        }

        private static WindowMode NextDisplayMode(WindowMode mode)
        {
            switch (mode)
            {
                case WindowMode.Windowed: return WindowMode.Borderless;
                case WindowMode.Borderless: return WindowMode.Exclusive;
                default: return WindowMode.Windowed;
            }
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
            options.WindowMode = _displayMode;
            options.SaveToFile();

            Xenocide.SetDisplayMode(_displayMode);

            // 0 = software cursor (hide the OS pointer), 1 = hardware (show it).
            SoftwareCursor.IsSoftwareCursorEnabled = _cursorMode == 0;
            Xenocide.Instance.IsMouseVisible = _cursorMode == 1;
            ScreenManager.ScheduleScreen(new StartScreen());
        }

        private void OnCancelClicked(object sender, EventArgs e)
        {
            ScreenManager.ScheduleScreen(new StartScreen());
        }

        private void ClearContent()
        {
            _contentPanel?.Visual?.Children.Clear();
        }

        private void AddLabel(string text)
        {
            if (_contentPanel == null) return;
            var label = ThemedLabel.CreateBody(text);
            _contentPanel.AddChild(label);
        }

        /// <summary>
        /// Adds a label-plus-control row. The control is right-aligned, the label
        /// left-aligned (they share one horizontal <see cref="ThemedRow"/>).
        /// </summary>
        private void AddSettingRow(string labelText, string initialValue, Func<string> onToggle)
        {
            if (_contentPanel == null) return;

            var row = ThemedRow.Create(ContentWidth, 30);

            var label = ThemedLabel.CreateBody(labelText);
            ThemedRow.Place(row, label, 0, 4);

            var btn = ThemedButton.Create(initialValue);
            ThemedButton.SetWidth(btn, 220);
            btn.Visual.Height = 26;
            btn.Visual.HeightUnits = DimensionUnitType.Absolute;
            btn.Visual.X = -220;
            btn.Visual.XUnits = GeneralUnitType.PixelsFromLarge;
            btn.Visual.Y = 0;
            btn.Click += (s, e) => { btn.Text = onToggle(); };
            row.Children.Add(btn.Visual);

            _contentPanel.Visual.Children.Add(row);
        }

        /// <summary>
        /// Adds a "- value +" volume row, right-aligned within the row.
        /// </summary>
        private void AddVolumeRow(Func<int> getLevel, Action<int> setLevel)
        {
            if (_contentPanel == null) return;

            var row = ThemedRow.Create(ContentWidth, 30);

            // Flat buttons: the XenocideButton 3-slice cannot render legibly in
            // a narrow (40px) control.
            var downBtn = ThemedButton.CreateFlat("<");
            downBtn.Visual.Width = 40;
            downBtn.Visual.WidthUnits = DimensionUnitType.Absolute;
            downBtn.Visual.Height = 26;
            downBtn.Visual.HeightUnits = DimensionUnitType.Absolute;

            var levelLabel = ThemedLabel.CreateBody(getLevel().ToString(CultureInfo.InvariantCulture));

            var upBtn = ThemedButton.CreateFlat(">");
            upBtn.Visual.Width = 40;
            upBtn.Visual.WidthUnits = DimensionUnitType.Absolute;
            upBtn.Visual.Height = 26;
            upBtn.Visual.HeightUnits = DimensionUnitType.Absolute;

            ThemedRow.Place(row, downBtn, 400, 0);
            ThemedRow.Place(row, levelLabel, 452, 6);
            ThemedRow.Place(row, upBtn, 500, 0);

            downBtn.Click += (s, e) =>
            {
                setLevel(getLevel() - 1);
                levelLabel.Text = getLevel().ToString(CultureInfo.InvariantCulture);
            };
            upBtn.Click += (s, e) =>
            {
                setLevel(getLevel() + 1);
                levelLabel.Text = getLevel().ToString(CultureInfo.InvariantCulture);
            };

            _contentPanel.Visual.Children.Add(row);
        }
    }
}
