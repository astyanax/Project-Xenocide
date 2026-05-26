using System;

using Gum.Forms.Controls;

using Microsoft.Xna.Framework;

using MonoGameGum;

using NLog;

using ProjectXenocide.Assets;
using ProjectXenocide.UI.Screens;

namespace ProjectXenocide.UI.Dialogs
{
    public abstract class ModalDialog : Dialog
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private StackPanel _panel;
        private StackPanel _titleBar;
        private Label _titleLabel;
        private Button _closeButton;

        public string Title
        {
            get => _titleLabel?.Text;
            set { if (_titleLabel != null) _titleLabel.Text = value; }
        }

        protected StackPanel ContentArea { get; private set; }

        public int PanelWidth { get; set; } = 500;
        public int PanelHeight { get; set; } = 200;

        public Dialog.ButtonAction CloseAction { get; set; }
        public Dialog.ButtonAction DismissAction { get; set; }

        public bool IsRequired { get; set; }

        protected ModalDialog() : base(new UiSize(0.5f, 0.3f))
        {
        }

        protected ModalDialog(string title) : this()
        {
            Title = title;
        }

        public override void Show()
        {
            BuildPanel();
            BuildTitleBar();
            BuildContentArea();

            CreateDialogWidgets();

            _panel.AddToRoot();

            Logger.Debug("[DIALOG] {0}: \"{1}\"", GetType().Name, Title);
        }

        public void Close()
        {
            Logger.Debug("[DIALOG] {0} Close", GetType().Name);
            CloseAction?.Invoke();
            ScreenManager.CloseDialog(this);
        }

        public void Dismiss()
        {
            Logger.Debug("[DIALOG] {0} Dismiss", GetType().Name);
            DismissAction?.Invoke();
            ScreenManager.CloseDialog(this);
        }

        public override bool HandleEscape()
        {
            Dismiss();
            return true;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                RemoveFromScreen();
            }
            base.Dispose(disposing);
        }

        protected abstract void CreateDialogWidgets();

        private void BuildPanel()
        {
            _panel = new StackPanel();

            var vp = Xenocide.Instance.GraphicsDevice.Viewport;
            int x = (vp.Width - PanelWidth) / 2;
            int y = (vp.Height - PanelHeight) / 2;

            _panel.Visual.X = x;
            _panel.Visual.Y = y;
            _panel.Visual.Width = PanelWidth;
            _panel.Visual.Height = PanelHeight;
        }

        private void BuildTitleBar()
        {
            _titleBar = new StackPanel();
            _titleBar.Visual.X = 0;
            _titleBar.Visual.Y = 0;
            _titleBar.Visual.Width = 0;
            _titleBar.Visual.WidthUnits = Gum.DataTypes.DimensionUnitType.RelativeToContainer;
            _titleBar.Visual.Height = 28;
            _titleBar.Visual.SetProperty("ColorCategoryState", "Primary");

            _titleLabel = new Label();
            _titleLabel.Text = Title ?? "";
            _titleLabel.Visual.X = 8;
            _titleLabel.Visual.Y = 4;
            _titleBar.AddChild(_titleLabel);

            _closeButton = new Button();
            _closeButton.Text = "X";
            _closeButton.Visual.X = PanelWidth - 40;
            _closeButton.Visual.Y = 2;
            _closeButton.Visual.Width = 24;
            _closeButton.Visual.Height = 24;
            _closeButton.Click += OnCloseClicked;
            _titleBar.AddChild(_closeButton);

            _panel.AddChild(_titleBar);
        }

        private void BuildContentArea()
        {
            ContentArea = new StackPanel();
            ContentArea.Visual.X = 0;
            ContentArea.Visual.Y = 0;
            ContentArea.Visual.Width = 0;
            ContentArea.Visual.WidthUnits = Gum.DataTypes.DimensionUnitType.RelativeToContainer;

            _panel.AddChild(ContentArea);
        }

        private void OnCloseClicked(object sender, EventArgs e)
        {
            Xenocide.AudioSystem?.PlaySound(SoundId.ButtonClick1);
            Close();
        }

        private void RemoveFromScreen()
        {
            if (_panel != null)
            {
                GumService.Default.Root.Children.Remove(_panel.Visual);
                _panel = null;
            }
        }
    }
}
