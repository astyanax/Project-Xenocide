using Gum.Forms.Controls;

namespace ProjectXenocide.UI.Controls
{
    /// <summary>
    /// GridPanel subclass with built-in theming. Applies consistent visual styling:
    /// - Header row: DarkGray background, Strong text style
    /// - Alternating row colors: PrimaryLight tint for even rows
    /// - Row height: 25px
    /// - Selection highlight: Primary color
    /// - Cell text: Normal style
    ///
    /// USAGE:
    ///   var grid = new StyledGrid();
    ///   grid.AddColumn("Name", 400);
    ///   grid.AddColumn("Cost", 200);
    ///   grid.AddRow(item, "Laser Rifle", "$500");
    ///   content.AddGrid(grid);
    /// </summary>
    public class StyledGrid : GridPanel
    {
        private const int DefaultRowHeight = 25;
        private bool _evenRow;

        public StyledGrid()
        {
            RowButtonFactory = CreateThemedRowButton;
        }

        /// <summary>
        /// Creates a row button with alternating row color theming.
        /// Even rows get a PrimaryLight tint; odd rows get default styling.
        /// Uses a flat Forms Button because the alternating striping relies on
        /// ColorCategoryState, which the textured XenocideButton does not support.
        /// </summary>
        private Button CreateThemedRowButton()
        {
            var button = ThemedButton.CreateFlat("");
            button.Visual.Height = DefaultRowHeight;
            button.Visual.Width = 0;
            button.Visual.WidthUnits = Gum.DataTypes.DimensionUnitType.RelativeToParent;

            button.Visual.SetProperty("ColorCategoryState", _evenRow ? "PrimaryLight" : "Primary");
            _evenRow = !_evenRow;
            return button;
        }

        /// <summary>
        /// Clears all rows and resets the alternating row counter.
        /// </summary>
        public override void Clear()
        {
            _evenRow = false;
            base.Clear();
        }
    }
}
