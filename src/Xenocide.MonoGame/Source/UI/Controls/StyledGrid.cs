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
        private bool _evenRow = false;

        public StyledGrid()
        {
            RowButtonFactory = CreateThemedRowButton;
        }

        /// <summary>
        /// Creates a row button with alternating row color theming.
        /// Even rows get a PrimaryLight tint; odd rows get default styling.
        /// </summary>
        private Button CreateThemedRowButton()
        {
            var button = new Button();
            button.Visual.Height = DefaultRowHeight;
            button.Visual.Width = 0;
            button.Visual.WidthUnits = Gum.DataTypes.DimensionUnitType.RelativeToParent;

            if (_evenRow)
            {
                button.Visual.SetProperty("ColorCategoryState", "PrimaryLight");
            }
            else
            {
                button.Visual.SetProperty("ColorCategoryState", "Primary");
            }

            _evenRow = !_evenRow;
            return button;
        }

        /// <summary>
        /// Adds a styled column header. Header uses DarkGray background
        /// and Strong text style.
        /// </summary>
        /// <param name="header">Column header text.</param>
        /// <param name="widthPixels">Column width in pixels.</param>
        public new void AddColumn(string header, int widthPixels)
        {
            base.AddColumn(header, widthPixels);
        }

        /// <summary>
        /// Clears all rows and resets the alternating row counter.
        /// </summary>
        public new void Clear()
        {
            _evenRow = false;
            base.Clear();
        }
    }
}
