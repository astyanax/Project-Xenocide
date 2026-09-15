using System;
using System.Collections.Generic;

using Gum.Converters;
using Gum.DataTypes;
using Gum.Forms.Controls;
using Gum.Wireframe;

using Microsoft.Xna.Framework;

using MonoGameGum;
using MonoGameGum.GueDeriving;

using NLog;

using ProjectXenocide.Assets;
using ProjectXenocide.UI.Controls;
using ProjectXenocide.UI.Screens;
using ProjectXenocide.Utils;

namespace ProjectXenocide.UI.Dialogs
{
    /// <summary>
    /// Base class for all modal dialogs.
    ///
    /// <para>
    /// LAYOUT MODEL: The dialog is an absolutely-positioned
    /// <see cref="ContainerRuntime"/> (the "panel") whose chrome is a 9-slice
    /// window frame cut from <c>XenoNew.png</c> (see <see cref="AtlasNineSlice"/>).
    /// Children are positioned explicitly — this is deliberate, because
    /// <see cref="StackPanel"/> stacks vertically and cannot put a title on the
    /// left of a bar with a close button on the right.
    /// </para>
    ///
    /// <para>
    /// STRUCTURE:
    /// <list type="bullet">
    /// <item>9-slice window frame (corners fixed, edges/centre stretch) — fills the panel</item>
    /// <item>title text over the frame's title bar, close "X" top-right</item>
    /// <item>content area (a vertical <see cref="StackPanel"/>) — subclass labels,
    /// lists and controls (add via <see cref="AddButton"/>)</item>
    /// <item>action-button row — a single centred horizontal row at the bottom
    /// (add via <see cref="AddActionButton"/>)</item>
    /// </list>
    /// </para>
    /// </summary>
    public abstract class ModalDialog : Dialog
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        // Window-frame insets, in pixels. These match the border/title-bar
        // thickness of the XenoNew.png window-frame region: the title bar is
        // taller than the side borders, hence the asymmetric values.
        private const int FrameLeft = 44;
        private const int FrameRight = 44;
        private const int FrameTop = 28;
        private const int FrameBottom = 24;

        /// <summary>Padding between the frame border and the content.</summary>
        protected const int ContentPadding = 10;

        /// <summary>Default width of a button added via <see cref="AddActionButton"/>.</summary>
        protected const int DefaultActionButtonWidth = 130;

        /// <summary>Height of the bottom action-button row, in pixels.</summary>
        protected const int ActionRowHeight = 32;

        private ContainerRuntime _panel;
        private AtlasNineSlice _frame;
        private ColoredRectangleRuntime _contentBackdrop;
        private Label _titleLabel;
        private Button _closeButton;
        private string _title;

        private ContainerRuntime _actionRow;
        private readonly List<Button> _actionButtons = new List<Button>();

        private bool _isClosed;

        /// <summary>
        /// The dialog's title, shown in the title bar. Stored in a backing field
        /// (not read back from the label) so that titles passed to the constructor
        /// survive until <see cref="BuildTitleBar"/> creates the label.
        /// </summary>
        public override string Title
        {
            get => _title;
            protected set
            {
                _title = value;
                if (_titleLabel != null)
                    _titleLabel.Text = value;
            }
        }

        /// <summary>Vertical container for the subclass's content.</summary>
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

            // The action row only exists if the subclass called AddActionButton.
            // Size/centre it now that all content has been added.
            FinishLayout();

            _panel.AddToRoot();

            Logger.Debug("[DIALOG] {0}: \"{1}\"", GetType().Name, Title);
        }

        /// <summary>Closes the dialog, invoking <see cref="CloseAction"/>.</summary>
        public void Close()
        {
            if (_isClosed)
                return;
            _isClosed = true;

            Logger.Debug("[DIALOG] {0} Close", GetType().Name);
            CloseAction?.Invoke();
            ScreenManager.CloseDialog(this);
        }

        /// <summary>Dismisses the dialog, invoking <see cref="DismissAction"/> (cancel/cleanup).</summary>
        public void Dismiss()
        {
            if (_isClosed)
                return;
            _isClosed = true;

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
                RemoveFromScreen();
            base.Dispose(disposing);
        }

        protected abstract void CreateDialogWidgets();

        private void BuildPanel()
        {
            _panel = new ContainerRuntime();

            var vp = Xenocide.Instance.GraphicsDevice.Viewport;
            int x = Math.Max(0, (vp.Width - PanelWidth) / 2);
            int y = Math.Max(0, (vp.Height - PanelHeight) / 2);

            _panel.X = x;
            _panel.Y = y;
            _panel.XUnits = GeneralUnitType.PixelsFromSmall;
            _panel.YUnits = GeneralUnitType.PixelsFromSmall;
            _panel.Width = PanelWidth;
            _panel.WidthUnits = DimensionUnitType.Absolute;
            _panel.Height = PanelHeight;
            _panel.HeightUnits = DimensionUnitType.Absolute;
            _panel.ClipsChildren = true;

            // 9-slice window frame, added first so it renders behind title/content.
            _frame = new AtlasNineSlice(XenoAtlas.Panels.WindowFrame, FrameLeft, FrameRight, FrameTop, FrameBottom);
            _frame.Visual.X = 0;
            _frame.Visual.Y = 0;
            _frame.Visual.XUnits = GeneralUnitType.PixelsFromSmall;
            _frame.Visual.YUnits = GeneralUnitType.PixelsFromSmall;
            _panel.Children.Add(_frame.Visual);

            // Opaque-ish body fill over the frame's translucent centre so the
            // scene behind does not show through the dialog and hurt readability.
            _contentBackdrop = new ColoredRectangleRuntime();
            _contentBackdrop.Color = new Color(22, 26, 44, 232);
            _contentBackdrop.X = FrameLeft;
            _contentBackdrop.XUnits = GeneralUnitType.PixelsFromSmall;
            _contentBackdrop.Y = FrameTop;
            _contentBackdrop.YUnits = GeneralUnitType.PixelsFromSmall;
            _contentBackdrop.Width = -(FrameLeft + FrameRight);
            _contentBackdrop.WidthUnits = DimensionUnitType.RelativeToParent;
            _contentBackdrop.Height = -(FrameTop + FrameBottom);
            _contentBackdrop.HeightUnits = DimensionUnitType.RelativeToParent;
            _panel.Children.Add(_contentBackdrop);
        }

        private void BuildTitleBar()
        {
            _titleLabel = new Label { Text = Title ?? "" };
            _titleLabel.X = FrameLeft + 4;
            _titleLabel.Y = 5;
            _titleLabel.XUnits = GeneralUnitType.PixelsFromSmall;
            _titleLabel.YUnits = GeneralUnitType.PixelsFromSmall;
            _panel.Children.Add(_titleLabel.Visual);

            // Invisible hit area over the window frame's own close glyph (drawn as
            // part of the 9-slice top-right corner). This keeps the chrome faithful
            // without a second, off-theme "X". The flat button's own background and
            // focus indicator are hidden so only the atlas glyph is visible.
            _closeButton = ThemedButton.CreateFlat("", OnCloseClicked);
            _closeButton.Visual.Width = 22;
            _closeButton.Visual.WidthUnits = DimensionUnitType.Absolute;
            _closeButton.Visual.Height = 20;
            _closeButton.Visual.HeightUnits = DimensionUnitType.Absolute;
            _closeButton.Visual.XOrigin = RenderingLibrary.Graphics.HorizontalAlignment.Right;
            _closeButton.Visual.XUnits = GeneralUnitType.PixelsFromLarge;
            _closeButton.Visual.X = 0;
            _closeButton.Visual.Y = 2;
            _closeButton.Visual.YUnits = GeneralUnitType.PixelsFromSmall;
            HideBackground(_closeButton);
            _panel.Children.Add(_closeButton.Visual);
        }

        private void BuildContentArea()
        {
            ContentArea = new StackPanel();
            ContentArea.Visual.X = FrameLeft + ContentPadding;
            ContentArea.Visual.XUnits = GeneralUnitType.PixelsFromSmall;
            ContentArea.Visual.Y = FrameTop + 10;
            ContentArea.Visual.YUnits = GeneralUnitType.PixelsFromSmall;
            ContentArea.Visual.Width = -(FrameLeft + FrameRight + ContentPadding * 2);
            ContentArea.Visual.WidthUnits = DimensionUnitType.RelativeToParent;
            ContentArea.Visual.Height = -(FrameTop + FrameBottom + ContentPadding * 2);
            ContentArea.Visual.HeightUnits = DimensionUnitType.RelativeToParent;
            _panel.Children.Add(ContentArea.Visual);
        }

        /// <summary>
        /// Creates a themed button and adds it to the dialog's content area
        /// (which stacks vertically — use for list/menu items). The ButtonClick1
        /// sound is auto-wired by <see cref="ThemedButton"/>.
        /// </summary>
        protected Button AddButton(string text, EventHandler onClick)
        {
            var button = ThemedButton.Create(text, onClick);
            ContentArea.AddChild(button);
            return button;
        }

        /// <summary>
        /// Adds a body-text label that wraps to the content width.
        /// </summary>
        protected Label AddBodyText(string text)
        {
            var label = ThemedLabel.CreateBody(text);
            label.Visual.Width = 0;
            label.Visual.WidthUnits = DimensionUnitType.RelativeToParent;
            ContentArea.AddChild(label);
            return label;
        }

        /// <summary>
        /// Creates a themed button and adds it to a single horizontal row centred
        /// near the bottom of the content area. Use for action buttons
        /// (OK/Cancel/Yes/No); use <see cref="AddButton"/> for list items.
        /// </summary>
        protected Button AddActionButton(string text, EventHandler onClick, int width = 0)
        {
            EnsureActionRow();

            var button = ThemedButton.Create(text, onClick);
            ThemedButton.SetWidth(button, width > 0 ? width : DefaultActionButtonWidth);
            button.Visual.Height = 25;
            button.Visual.HeightUnits = DimensionUnitType.Absolute;
            _actionRow.Children.Add(button.Visual);
            _actionButtons.Add(button);
            return button;
        }

        private void EnsureActionRow()
        {
            if (_actionRow != null)
                return;

            // Anchored to the bottom of the panel (not inside the vertical content
            // stack) so a long list can never push the action buttons out of view.
            _actionRow = new ContainerRuntime();
            _actionRow.X = FrameLeft + ContentPadding;
            _actionRow.XUnits = GeneralUnitType.PixelsFromSmall;
            _actionRow.Y = -(FrameBottom + ContentPadding);
            _actionRow.YUnits = GeneralUnitType.PixelsFromLarge;
            _actionRow.YOrigin = RenderingLibrary.Graphics.VerticalAlignment.Bottom;
            // Absolute width: ThemedRow.CenterButtons reads Width to centre the
            // buttons, and RelativeToParent would expose the raw offset, not the
            // resolved width.
            _actionRow.Width = PanelWidth - FrameLeft - FrameRight - ContentPadding * 2;
            _actionRow.WidthUnits = DimensionUnitType.Absolute;
            _actionRow.Height = ActionRowHeight;
            _actionRow.HeightUnits = DimensionUnitType.Absolute;
            _panel.Children.Add(_actionRow);
        }

        private void FinishLayout()
        {
            if (_actionRow == null || _actionButtons.Count == 0)
                return;

            // Reserve the bottom strip for the action row so scrolling/stacking
            // content does not overlap it.
            ContentArea.Visual.Height =
                -(FrameTop + FrameBottom + ContentPadding * 2 + ActionRowHeight + 4);
            ContentArea.Visual.HeightUnits = DimensionUnitType.RelativeToParent;

            ThemedRow.CenterButtons(_actionRow, _actionButtons);
        }

        /// <summary>
        /// Hides a Forms button's own background/focus chrome so it can act as a
        /// transparent hit area over atlas-drawn artwork.
        /// </summary>
        private static void HideBackground(Button button)
        {
            var background = button.Visual.GetGraphicalUiElementByName("Background");
            if (background != null)
                background.Visible = false;

            var focus = button.Visual.GetGraphicalUiElementByName("FocusedIndicator");
            if (focus != null)
                focus.Visible = false;
        }

        private void OnCloseClicked(object sender, EventArgs e)
        {
            // The title-bar "X" is a cancel: it must run the dismiss path, not the
            // affirmative CloseAction. Otherwise X on a Yes/No confirmation would
            // act as "Yes" and let the player bypass the choice.
            Xenocide.AudioSystem?.PlaySound(SoundId.ButtonClick1);
            Dismiss();
        }

        private void RemoveFromScreen()
        {
            if (_panel != null)
            {
                _panel.RemoveFromRoot();
                _panel = null;
            }
            _frame = null;
            _contentBackdrop = null;
            _actionRow = null;
            _actionButtons.Clear();
            ContentArea = null;
        }
    }
}
