using System;
using System.Collections.Generic;

using Gum.DataTypes;
using Gum.Forms.Controls;
using Gum.Wireframe;

namespace ProjectXenocide.UI.Controls
{
    public class GridPanel
    {
        /// <summary>Single source of truth for grid row height, in pixels.</summary>
        public const int DefaultRowHeight = 25;

        private readonly StackPanel _container;
        private readonly StackPanel _headerPanel;
        private readonly StackPanel _bodyPanel;
        private readonly List<GridRow> _rows = new();
        private int _selectedIndex = -1;
        private int _columnCount;

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
            button.Visual.Width = 0;
            button.Visual.WidthUnits = DimensionUnitType.RelativeToParent;
            return button;
        }

        public GridPanel()
        {
            _container = new StackPanel();
            _headerPanel = new StackPanel();
            _bodyPanel = new StackPanel();
            _container.AddChild(_headerPanel);
            _container.AddChild(_bodyPanel);
        }

        public virtual void AddColumn(string header, int widthPixels)
        {
            var label = new Label { Text = header };
            label.Visual.Width = widthPixels;
            _headerPanel.AddChild(label);
            _columnCount++;
        }

        public StackPanel Visual => _container;

        public int RowCount => _rows.Count;

        public int SelectedIndex
        {
            get => _selectedIndex;
            set
            {
                if (_selectedIndex != value)
                {
                    _selectedIndex = value;
                    SelectionChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public GridRow GetRow(int index) => index >= 0 && index < _rows.Count ? _rows[index] : null;

        public GridRow SelectedRow => GetRow(_selectedIndex);

        public object GetSelectedTag()
        {
            var row = SelectedRow;
            return row?.Tag;
        }

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
                if (_rows[i].Tag == tag) return i;
            }
            return -1;
        }

        public int AddRow(object tag, params string[] cellTexts)
        {
            int rowIndex = _rows.Count;

            // Height is owned by the row-button factory (DefaultRowHeight) so it
            // matches the header/row styling; do not override it here.
            var rowButton = RowButtonFactory?.Invoke() ?? CreateStyledRowButton();

            // Fill the button's width; _container.Width is not resolved yet at
            // row-creation time, so an absolute value would be wrong.
            var rowPanel = new StackPanel();
            rowPanel.Visual.Width = 0;
            rowPanel.Visual.WidthUnits = DimensionUnitType.RelativeToParent;
            rowButton.Visual.Children.Add(rowPanel.Visual);

            var labels = new Label[cellTexts.Length];
            for (int i = 0; i < cellTexts.Length; i++)
            {
                labels[i] = new Label { Text = cellTexts[i] ?? "" };
                rowPanel.AddChild(labels[i]);
            }

            int capturedIndex = rowIndex;
            rowButton.Click += (_, _) => SelectedIndex = capturedIndex;

            _bodyPanel.AddChild(rowButton);
            _rows.Add(new GridRow { Tag = tag, CellLabels = labels, RowButton = rowButton });

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
    }

    public class GridRow
    {
        public object Tag { get; set; }
        public Label[] CellLabels { get; set; }
        public Button RowButton { get; set; }
    }
}
