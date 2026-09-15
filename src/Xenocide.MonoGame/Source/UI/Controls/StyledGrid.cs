using Gum.DataTypes;
using Gum.Forms.Controls;

namespace ProjectXenocide.UI.Controls
{
    /// <summary>
    /// GridPanel subclass with built-in theming:
    /// - Header row: dark band with Strong text (from <see cref="GridPanel"/>)
    /// - Alternating row colours: PrimaryLight tint for even rows
    /// - Selection highlight: Primary colour
    /// - Row height: 25px
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
        public StyledGrid()
        {
            RowButtonFactory = CreateThemedRowButton;
        }

        /// <summary>
        /// Creates a flat row button. Alternating colours are applied by
        /// <see cref="GetRowColorState"/> so selection highlighting can override
        /// and restore them; the textured XenocideButton cannot be tinted this way.
        /// </summary>
        private static Button CreateThemedRowButton()
        {
            var button = ThemedButton.CreateFlat("");
            button.Visual.Height = DefaultRowHeight;
            button.Visual.HeightUnits = DimensionUnitType.Absolute;
            button.Visual.Width = 0;
            button.Visual.WidthUnits = DimensionUnitType.RelativeToParent;
            return button;
        }

        protected override string GetRowColorState(int index) =>
            index % 2 == 0 ? "PrimaryLight" : "Primary";
    }
}
