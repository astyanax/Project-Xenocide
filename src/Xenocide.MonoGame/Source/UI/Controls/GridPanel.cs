using System;
using System.Collections.Generic;

using Gum.Converters;
using Gum.DataTypes;
using Gum.Forms.Controls;

using Microsoft.Xna.Framework;

using MonoGameGum.GueDeriving;

namespace ProjectXenocide.UI.Controls
{
    /// <summary>
    /// A simple data grid: a fixed header band followed by clickable rows.
    ///
    /// <para>
    /// Columns are laid out horizontally and share their pixel widths between the
    /// header and every row, so cells line up. (An earlier implementation stacked
    /// the header and cells in vertical StackPanels, which rendered every column as
    /// a separate line.) The grid stretches to the width of its parent and is
    /// intended to live inside a <see cref="Gum.Forms.Controls.ScrollViewer"/> so a
    /// long body scrolls.
    /// </para>
    /// </summary>
    public class GridPanel
    {
        /// <summary>Single source of truth for grid row height, in pixels.</summary>
        public const int DefaultRowHeight = 25;

        private const int CellPaddingLeft = 6;

        private readonly StackPanel _container;
        private readonly ContainerRuntime _headerPanel;
        private readonly StackPanel _bodyPanel;
        private readonly List<Column> _columns = new();
        private readonly List<GridRow> _rows = new();
        private int _selectedIndex = -1;

        public event EventHandler SelectionChanged;

        /// <summary>
        /// Optional factory for creating row buttons with custom styling.
        /// When set, each AddRow call uses this factory instead of the default.
        /// </summary>
        public Func<Button> RowButtonFactory { get; set; }

        /// <summary>
        /// Creates a themed row button using the XenocideButton component
        /// (textured 3-slice from XenoNew.png). Falls back to a plain Button
        /// if the template is unavailable.
        /// </summary>
        public static Button CreateStyledRowButton()
        {
            var button = ThemedButton.Create("");
            button.Visual.Height = DefaultRowHeight;
            button.Visual.HeightUnits = DimensionUnitType.Absolute;
            button.Visual.Width = 0;
            button.Visual.WidthUnits = DimensionUnitType.RelativeToParent;
            return button;
        }

        public GridPanel()
        {
            _container = new StackPanel();
            _container.Visual.Width = 0;
            _container.Visual.WidthUnits = DimensionUnitType.RelativeToParent;

            _headerPanel = new ContainerRuntime();
            _headerPanel.Width = 0;
            _headerPanel.WidthUnits = DimensionUnitType.RelativeToParent;
            _headerPanel.Height = DefaultRowHeight;
            _headerPanel.HeightUnits = DimensionUnitType.Absolute;

            _bodyPanel = new StackPanel();
            _bodyPanel.Visual.Width = 0;
            _bodyPanel.Visual.WidthUnits = DimensionUnitType.RelativeToParent;

            _container.Visual.Children.Add(_headerPanel);
            _container.Visual.Children.Add(_bodyPanel.Visual);
        }

        public StackPanel Visual => _container;

        public int RowCount => _rows.Count;

        public int ColumnCount => _columns.Count;

        public virtual void AddColumn(string header, int widthPixels)
        {
            _columns.Add(new Column(header ?? "", widthPixels));
            RebuildHeader();
        }

        public int SelectedIndex
        {
            get => _selectedIndex;
            set
            {
                if (_selectedIndex != value)
                {
                    int previous = _selectedIndex;
                    _selectedIndex = value;
                    ApplyRowColor(previous);
                    ApplyRowColor(_selectedIndex);
                    SelectionChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public GridRow GetRow(int index) => index >= 0 && index < _rows.Count ? _rows[index] : null;

        public GridRow SelectedRow => GetRow(_selectedIndex);

        public object GetSelectedTag() => SelectedRow?.Tag;

        public string GetSelectedCellText()
        {
            var row = SelectedRow;
            if (row == null || row.CellLabels.Length == 0) return null;
            return row.CellLabels[0].Text;
        }

        public int GetRowIndexByTag(object tag)
        {
            for (int i = 0; i < _rows.Count; i++)
            {
                if (Equals(_rows[i].Tag, tag)) return i;
            }
            return -1;
        }

        public int AddRow(object tag, params string[] cellTexts)
        {
            int rowIndex = _rows.Count;

            var rowButton = RowButtonFactory?.Invoke() ?? CreateStyledRowButton();
            rowButton.Visual.Height = DefaultRowHeight;
            rowButton.Visual.HeightUnits = DimensionUnitType.Absolute;
            rowButton.Visual.Width = 0;
            rowButton.Visual.WidthUnits = DimensionUnitType.RelativeToParent;

            // The cell container and labels must NOT be interactive: Gum click
            // handling is single-target (it does not bubble), so if a child has
            // HasEvents the row Button never receives the click and selection
            // silently fails.
            var rowPanel = new ContainerRuntime();
            rowPanel.HasEvents = false;
            rowPanel.Width = 0;
            rowPanel.WidthUnits = DimensionUnitType.RelativeToParent;
            rowPanel.Height = DefaultRowHeight;
            rowPanel.HeightUnits = DimensionUnitType.Absolute;
            rowButton.Visual.Children.Add(rowPanel);

            var labels = new Label[cellTexts.Length];
            int x = 0;
            for (int i = 0; i < cellTexts.Length; i++)
            {
                var label = ThemedLabel.CreateBody(cellTexts[i] ?? "");
                label.Visual.HasEvents = false;
                label.Visual.X = x + CellPaddingLeft;
                label.Visual.XUnits = GeneralUnitType.PixelsFromSmall;
                label.Visual.Y = 5;
                label.Visual.YUnits = GeneralUnitType.PixelsFromSmall;
                label.Visual.Width = ColumnWidth(i) - CellPaddingLeft;
                label.Visual.WidthUnits = DimensionUnitType.Absolute;
                rowPanel.Children.Add(label.Visual);

                labels[i] = label;
                x += ColumnWidth(i);
            }

            int capturedIndex = rowIndex;
            rowButton.Click += (_, _) => SelectedIndex = capturedIndex;

            _bodyPanel.AddChild(rowButton);
            _rows.Add(new GridRow { Tag = tag, CellLabels = labels, RowButton = rowButton });

            ApplyRowColor(rowIndex);

            return rowIndex;
        }

        public void SetCell(int rowIndex, int colIndex, string text)
        {
            if (rowIndex < 0 || rowIndex >= _rows.Count) return;
            var labels = _rows[rowIndex].CellLabels;
            if (colIndex < 0 || colIndex >= labels.Length) return;
            labels[colIndex].Text = text;
        }

        public string GetCellText(int rowIndex, int colIndex)
        {
            if (rowIndex < 0 || rowIndex >= _rows.Count) return null;
            var labels = _rows[rowIndex].CellLabels;
            if (colIndex < 0 || colIndex >= labels.Length) return null;
            return labels[colIndex].Text;
        }

        public void RemoveRow(int index)
        {
            if (index < 0 || index >= _rows.Count) return;

            var row = _rows[index];
            _bodyPanel.Visual.Children.Remove(row.RowButton.Visual);
            _rows.RemoveAt(index);

            if (_selectedIndex == index)
                _selectedIndex = -1;
            else if (_selectedIndex > index)
                _selectedIndex--;
        }

        public virtual void Clear()
        {
            foreach (var row in _rows)
                _bodyPanel.Visual.Children.Remove(row.RowButton.Visual);
            _rows.Clear();
            _selectedIndex = -1;
        }

        /// <summary>
        /// Base colour state for a row. Subclasses can override to provide
        /// alternating/striped colours; the default is no colour override.
        /// </summary>
        protected virtual string GetRowColorState(int index) => null;

        private void ApplyRowColor(int index)
        {
            if (index < 0 || index >= _rows.Count)
                return;

            string state = index == _selectedIndex ? "Primary" : GetRowColorState(index);
            if (state != null)
                _rows[index].RowButton.Visual.SetProperty("ColorCategoryState", state);
        }

        private int ColumnWidth(int index)
        {
            if (index < _columns.Count)
                return _columns[index].Width;
            return index >= 0 ? 100 : 0;
        }

        private void RebuildHeader()
        {
            _headerPanel.Children.Clear();

            // Header band background.
            var background = new ColoredRectangleRuntime();
            background.Color = new Color(28, 32, 52, 255);
            background.X = 0;
            background.Y = 0;
            background.XUnits = GeneralUnitType.PixelsFromSmall;
            background.YUnits = GeneralUnitType.PixelsFromSmall;
            background.Width = 0;
            background.WidthUnits = DimensionUnitType.RelativeToParent;
            background.Height = DefaultRowHeight;
            background.HeightUnits = DimensionUnitType.Absolute;
            _headerPanel.Children.Add(background);

            int x = 0;
            foreach (var column in _columns)
            {
                var label = ThemedLabel.Create(column.Header, TextStyle.Strong);
                label.Visual.X = x + CellPaddingLeft;
                label.Visual.XUnits = GeneralUnitType.PixelsFromSmall;
                label.Visual.Y = 5;
                label.Visual.YUnits = GeneralUnitType.PixelsFromSmall;
                label.Visual.Width = column.Width - CellPaddingLeft;
                label.Visual.WidthUnits = DimensionUnitType.Absolute;
                _headerPanel.Children.Add(label.Visual);
                x += column.Width;
            }
        }

        private readonly struct Column
        {
            public Column(string header, int width)
            {
                Header = header;
                Width = width;
            }

            public string Header { get; }
            public int Width { get; }
        }
    }

    public class GridRow
    {
        public object Tag { get; set; }
        public Label[] CellLabels { get; set; }
        public Button RowButton { get; set; }
    }
}
