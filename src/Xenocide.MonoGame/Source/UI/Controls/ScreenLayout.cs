using System;

using Gum.Forms.Controls;
using Gum.Wireframe;

using MonoGameGum;

namespace ProjectXenocide.UI.Controls
{
    /// <summary>
    /// Reusable screen layout component providing a standard structure:
    /// background image, scrollable content area (left 75%), button bar (right 200px),
    /// and optional status bar (bottom 30px).
    ///
    /// USAGE (programmatic):
    ///   var layout = new ScreenLayout();
    ///   layout.AddToRoot();
    ///   layout.AddButton("Research", OnResearch);
    ///   layout.ContentPanel.AddChild(someLabel);
    /// </summary>
    public class ScreenLayout
    {
        private const int ButtonBarWidthPx = 190;
        private const int ContentMarginPx = 10;
        private const int StatusBarHeightPx = 30;

        public ScreenLayout()
        {
            Visual = new GraphicalUiElement();
            Visual.Width = 100;
            Visual.WidthUnits = Gum.DataTypes.DimensionUnitType.PercentageOfParent;
            Visual.Height = 100;
            Visual.HeightUnits = Gum.DataTypes.DimensionUnitType.PercentageOfParent;

            ContentScroll = new ScrollViewer();
            ContentScroll.Width = 75;
            ContentScroll.WidthUnits = Gum.DataTypes.DimensionUnitType.PercentageOfParent;
            ContentScroll.Height = -StatusBarHeightPx - ContentMarginPx;
            ContentScroll.HeightUnits = Gum.DataTypes.DimensionUnitType.RelativeToParent;
            Visual.Children.Add(ContentScroll.Visual);

            ContentPanel = new StackPanel();
            ContentPanel.Visual.Width = 0;
            ContentPanel.Visual.WidthUnits = Gum.DataTypes.DimensionUnitType.RelativeToParent;
            ContentScroll.Visual.Children.Add(ContentPanel.Visual);

            ButtonBar = new StackPanel();
            ButtonBar.Visual.Width = ButtonBarWidthPx;
            ButtonBar.Visual.WidthUnits = Gum.DataTypes.DimensionUnitType.Absolute;
            ButtonBar.Visual.Height = -StatusBarHeightPx - ContentMarginPx;
            ButtonBar.Visual.HeightUnits = Gum.DataTypes.DimensionUnitType.RelativeToParent;
            ButtonBar.Visual.X = 1280 - ButtonBarWidthPx - ContentMarginPx;
            ButtonBar.Visual.Y = ContentMarginPx;
            Visual.Children.Add(ButtonBar.Visual);

            StatusBar = new StackPanel();
            StatusBar.Visual.Height = StatusBarHeightPx;
            StatusBar.Visual.HeightUnits = Gum.DataTypes.DimensionUnitType.Absolute;
            StatusBar.Visual.Width = 0;
            StatusBar.Visual.WidthUnits = Gum.DataTypes.DimensionUnitType.RelativeToParent;
            StatusBar.Visual.Y = 1024 - StatusBarHeightPx - ContentMarginPx;
            Visual.Children.Add(StatusBar.Visual);
        }

        /// <summary>Root visual element to add to Gum root.</summary>
        public GraphicalUiElement Visual { get; }

        /// <summary>Scrollable content area (left 75% of screen).</summary>
        public ScrollViewer ContentScroll { get; }

        /// <summary>StackPanel inside ContentScroll for adding content children.</summary>
        public StackPanel ContentPanel { get; }

        /// <summary>Vertical button bar (right 200px of screen).</summary>
        public StackPanel ButtonBar { get; }

        /// <summary>Horizontal status bar (bottom 30px of screen).</summary>
        public StackPanel StatusBar { get; }

        /// <summary>Adds this layout to the Gum root.</summary>
        public void AddToRoot()
        {
            GumService.Default.Root.Children.Add(Visual);
        }

        /// <summary>Removes this layout from the Gum root.</summary>
        public void RemoveFromRoot()
        {
            GumService.Default.Root.Children.Remove(Visual);
        }

        /// <summary>
        /// Creates and adds a XenocideButton to the button bar.
        /// Buttons stack vertically in the right panel.
        /// </summary>
        /// <param name="text">Button label text.</param>
        /// <param name="onClick">Click event handler.</param>
        /// <returns>The created Button for further customization.</returns>
        public Button AddButton(string text, EventHandler onClick)
        {
            var button = new Button();
            button.Text = text;
            button.Visual.Width = 0;
            button.Visual.WidthUnits = Gum.DataTypes.DimensionUnitType.RelativeToParent;
            button.Click += OnAnyButtonClicked;
            if (onClick != null)
                button.Click += onClick;
            ButtonBar.AddChild(button);
            return button;
        }

        private static void OnAnyButtonClicked(object sender, EventArgs e)
        {
            Xenocide.AudioSystem?.PlaySound(Assets.SoundId.ButtonClick1);
        }
    }
}
