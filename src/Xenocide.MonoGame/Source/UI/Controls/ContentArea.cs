using Gum.Forms.Controls;

namespace ProjectXenocide.UI.Controls
{
    /// <summary>
    /// Manages dynamic content within a ScreenLayout's content panel.
    /// Provides a fluent API for adding headers, labels, grids, and spacers
    /// that auto-stack vertically in the ScrollViewer.
    ///
    /// USAGE:
    ///   var content = new ContentArea(layout.ContentPanel);
    ///   content.AddHeader("Research Projects");
    ///   content.AddGrid(researchGrid);
    ///   content.AddLabel(statusText);
    /// </summary>
    public class ContentArea
    {
        private readonly StackPanel _panel;

        public ContentArea(StackPanel panel)
        {
            _panel = panel;
        }

        /// <summary>The underlying StackPanel for direct access if needed.</summary>
        public StackPanel Panel => _panel;

        /// <summary>
        /// Adds a section header (H2 style: 18px bold).
        /// </summary>
        /// <param name="text">Header text.</param>
        /// <returns>The created Label for further customization.</returns>
        public Label AddHeader(string text)
        {
            var label = ThemedLabel.Create(text, TextStyle.H2);
            _panel.AddChild(label);
            return label;
        }

        /// <summary>
        /// Adds a sub-header (H3 style: 16px bold).
        /// </summary>
        /// <param name="text">Sub-header text.</param>
        /// <returns>The created Label for further customization.</returns>
        public Label AddSubHeader(string text)
        {
            var label = ThemedLabel.Create(text, TextStyle.H3);
            _panel.AddChild(label);
            return label;
        }

        /// <summary>
        /// Adds a body label (Normal style: 14px).
        /// </summary>
        /// <param name="text">Label text.</param>
        /// <returns>The created Label for further customization.</returns>
        public Label AddLabel(string text)
        {
            var label = ThemedLabel.Create(text, TextStyle.Normal);
            _panel.AddChild(label);
            return label;
        }

        /// <summary>
        /// Adds a pre-existing label to the content area.
        /// Useful for labels that need to be updated dynamically.
        /// </summary>
        /// <param name="label">The label to add.</param>
        public void AddLabel(Label label)
        {
            _panel.AddChild(label);
        }

        /// <summary>
        /// Adds a GridPanel to the content area, stretching to full width.
        /// Note: GridPanel is not a FrameworkElement, so we add its visual directly.
        /// </summary>
        /// <param name="grid">The GridPanel to add.</param>
        public void AddGrid(GridPanel grid)
        {
            grid.Visual.Width = 0;
            grid.Visual.WidthUnits = Gum.DataTypes.DimensionUnitType.RelativeToParent;
            _panel.Visual.Children.Add(grid.Visual.Visual);
        }

        /// <summary>
        /// Adds vertical spacing between content items.
        /// </summary>
        /// <param name="height">Spacer height in pixels (default 10).</param>
        public void AddSpacer(int height = 10)
        {
            var spacer = new Gum.Wireframe.GraphicalUiElement();
            spacer.Height = height;
            spacer.HeightUnits = Gum.DataTypes.DimensionUnitType.Absolute;
            spacer.Width = 0;
            spacer.WidthUnits = Gum.DataTypes.DimensionUnitType.RelativeToParent;
            _panel.Visual.Children.Add(spacer);
        }

        /// <summary>
        /// Removes all children from the content area.
        /// Call this before repopulating (e.g., when switching bases).
        /// </summary>
        public void Clear()
        {
            _panel.Visual.Children.Clear();
        }
    }
}
