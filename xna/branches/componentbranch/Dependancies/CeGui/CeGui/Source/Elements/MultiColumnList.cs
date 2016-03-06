#region LGPL License
/*************************************************************************
    Crazy Eddie's GUI System (http://crayzedsgui.sourceforge.net)
    Copyright (C)2004 Paul D Turner (crayzed@users.sourceforge.net)

    C# Port developed by Chris McGuirk (leedgitar@latenitegames.com)
    Compatible with the Axiom 3D Engine (http://axiomengine.sf.net)

    This library is free software; you can redistribute it and/or
    modify it under the terms of the GNU Lesser General Public
    License as published by the Free Software Foundation; either
    version 2.1 of the License, or (at your option) any later version.

    This library is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
    Lesser General Public License for more details.

    You should have received a copy of the GNU Lesser General Public
    License along with this library; if not, write to the Free Software
    Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
*************************************************************************/
#endregion LGPL License

using System;
using System.Drawing;

namespace CeGui.Widgets {

#region GridReference Structure

/// <summary>
///		Simple structure used to hold grid references.
/// </summary>
public struct GridReference {

  #region Fields

  /// <summary>
  /// The zero based row index of this grid reference.
  /// </summary>
  public int Row;

  /// <summary>
  /// The zero based column index of this grid reference.
  /// </summary>
  public int Column;

  #endregion

  #region Constructor

  /// <summary>
  /// Ctor
  /// </summary>
  /// <param name="r"></param>
  /// <param name="c"></param>
  public GridReference(int r, int c) {
    Row = r;
    Column = c;
  }

  /// <summary>
  /// copy ctor
  /// </summary>
  /// <param name="obj"></param>
  public GridReference(GridReference obj) {
    Row = obj.Row;
    Column = obj.Column;
  }

  #endregion

  #region Operators

  /// <summary>
  /// 
  /// </summary>
  /// <returns></returns>
  public override int GetHashCode() {
    return Row ^ Column;
  }

  /// <summary>
  /// Test for value equivelence
  /// </summary>
  /// <param name="a"></param>
  /// <param name="b"></param>
  /// <returns></returns>
  public static bool operator ==(GridReference a, GridReference b) {
    return ((a.Row == b.Row) && (a.Column == b.Column));
  }

  /// <summary>
  /// Test for value equivelence
  /// </summary>
  /// <param name="obj"></param>
  /// <returns></returns>
  public override bool Equals(object obj) {
    return ((this.Row == ((GridReference)obj).Row) && (this.Column == ((GridReference)obj).Column));
  }

  /// <summary>
  /// Test for value inequality
  /// </summary>
  /// <param name="a"></param>
  /// <param name="b"></param>
  /// <returns></returns>
  public static bool operator !=(GridReference a, GridReference b) {
    return !(a == b);
  }

  #endregion
}

#endregion

#region GridRow helper / container class

/// <summary>
///		Class used to wrap a grid 'row' and also ease sorting
/// </summary>
public class GridRow {
  /// <summary>
  /// container to hold the items in the row
  /// </summary>
  public ListboxItemList rowItems = new ListboxItemList();
  
  /// <summary>
  /// The column who's contents are used to order the grid
  /// </summary>
  public int sortColumnIndex;

  /// <summary>
  /// The item in the index'th column in the row
  /// </summary>
  /// <param name="index"></param>
  /// <returns></returns>
  public ListboxItem this[int index] {
    get {
      return rowItems[index];
    }

    set {
      rowItems[index] = value;
    }
  }

  /// <summary>
  /// Less than operator
  /// </summary>
  /// <param name="a"></param>
  /// <param name="b"></param>
  /// <returns></returns>
  public static bool operator <(GridRow a, GridRow b) {
    ListboxItem itemA = a.rowItems[a.sortColumnIndex];
    ListboxItem itemB = b.rowItems[b.sortColumnIndex];

    // handle same object case
    if(a == b) {
      return false;
    }

    // handle empty slot cases
    if(itemB == null) {
      return true;
    } else if(itemA == null) {
      return false;
    }
      // both items valid, do the compare
  else {
      return itemA < itemB;
    }
  }

  /// <summary>
  /// Greater than operator
  /// </summary>
  /// <param name="a"></param>
  /// <param name="b"></param>
  /// <returns></returns>
  public static bool operator >(GridRow a, GridRow b) {
    return b < a;
  }
}

#endregion

/// <summary>
///		Base class for the multi-column list / grid widget.
/// </summary>
[ AlternateWidgetName("MultiColumnList") ]
public abstract class MultiColumnList : Window {
  #region Constants
  #endregion

  #region Fields

  /// <summary>
  ///		vertical scroll-bar widget
  /// </summary>
  protected Scrollbar verticalScrollbar;

  /// <summary>
  ///		horizontal scroll-bar widget
  /// </summary>
  protected Scrollbar horizontalScrollbar;

  /// <summary>
  ///		The ListHeader widget.
  /// </summary>
  protected ListHeader listHeader;

  /// <summary>
  ///		true if vertical scrollbar should always be displayed
  /// </summary>
  protected bool forceVerticalScrollbar;

  /// <summary>
  ///		true if horizontal scrollbar should always be displayed
  /// </summary>
  protected bool forceHorizontalScrollbar;

  /// <summary>
  ///		Holds selection mode (represented by settings below).
  /// </summary>
  protected GridSelectionMode selectMode;

  /// <summary>
  ///		Nominated column for single column selection.
  /// </summary>
  protected int nominatedSelectColumn;

  /// <summary>
  ///		Nominated row for single row selection.
  /// </summary>
  protected int nominatedSelectRow;

  /// <summary>
  ///		Allow multiple selections.
  /// </summary>
  protected bool multiSelect;

  /// <summary>
  ///		All items in a row should be selected.
  /// </summary>
  protected bool fullRowSelect;

  /// <summary>
  ///		All items in a column should be selected.
  /// </summary>
  protected bool fullColumnSelect;

  /// <summary>
  ///		true if we use a nominated row to select.
  /// </summary>
  protected bool useNominatedRow;

  /// <summary>
  ///		true if we use a nominated col to select.
  /// </summary>
  protected bool useNominatedColumn;

  /// <summary>
  ///		holds the last selected item (used in range selections)
  /// </summary>
  protected ListboxItem lastSelectedItem;

  /// <summary>
  ///		Holds the grid data.
  /// </summary>
  protected GridRowList gridData;

  #endregion

  #region Constructor
  /// <summary>
  ///		Constructor.
  /// </summary>
  /// <param name="type"></param>
  /// <param name="name"></param>
  public MultiColumnList(string type, string name)
    : base(type, name) {
    gridData = new GridRowList();

    forceVerticalScrollbar = false;
    forceHorizontalScrollbar = false;
    nominatedSelectColumn = 0;
    nominatedSelectRow = 0;
    lastSelectedItem = null;

    // This is a bit of a hack to ensure all the individual settings that are affected
    // by selection mode are updated properly.  If we do not do this, the box does not
    // function properly.
    SelectionMode = GridSelectionMode.CellSingle;
    SelectionMode = GridSelectionMode.RowSingle;
  }
  #endregion

  #region Properties

  /// <summary>
  ///		Gets/Sets whether use control of the sort column and sort direction is enabled.
  /// </summary>
  [WidgetProperty("SortSettingEnabled")]
  public bool UserSortControlEnabled {
    get {
      return listHeader.UserSortControlEnabled;
    }

    set {
      listHeader.UserSortControlEnabled = value;
    }
  }

  /// <summary>
  ///		Gets/Sets whether the user may SizeF column segments.
  /// </summary>		
  [WidgetProperty("ColumnsSizable")]
  public bool UserColumnSizingEnabled {
    get {
      return listHeader.SegmentSizingEnabled;
    }

    set {
      listHeader.SegmentSizingEnabled = value;
    }
  }

  /// <summary>
  ///		Gets/Sets whether the user may modify the order of the columns.
  /// </summary>
  [WidgetProperty("ColumnsMovable")]
  public bool UserColumnDraggingEnabled {
    get {
      return listHeader.SegmentDraggingEnabled;
    }

    set {
      listHeader.SegmentDraggingEnabled = value;
    }
  }

  /// <summary>
  ///		Gets the number of columns in the grid.
  /// </summary>
  public int ColumnCount {
    get {
      return listHeader.ColumnCount;
    }
  }

  /// <summary>
  ///		Gets the number of rows in the grid.
  /// </summary>
  public int RowCount {
    get {
      return gridData.Count;
    }
  }

  /// <summary>
  ///		Gets/Sets the zero based index of the current sort column.
  ///		NB: There must be at least one column to successfully read this property.
  /// </summary>
  public int SortColumnIndex {
    get {
      return listHeader.SortColumnIndex;
    }

    set {
      if(listHeader.SortColumnIndex != value) {
        listHeader.SortColumnIndex = value;
      }
    }
  }

  /// <summary>
  /// Gets/Gets the current sort column by ID code.
  /// </summary>
  [WidgetProperty("SortColumnID")]
  public int SortColumnID {
    get {
      return listHeader.SortColumnID;
    }

    set {
      if(listHeader.SortColumnID != value) {
        listHeader.SortColumnID = value;
      }
    }
  }

  /// <summary>
  ///		Gets the total width of all column headers using the active metrics system.
  /// </summary>
  public float TotalColumnHeadersWidth {
    get {
      float width = listHeader.TotalPixelExtent;

      if(MetricsMode == MetricsMode.Relative) {
        width = AbsoluteToRelativeX(width);
      }

      return width;
    }
  }

  /// <summary>
  ///		Gets/Sets the current sort direction.
  /// </summary>
  [SortDirectionProperty("SortDirection")]
  public SortDirection SortDirection {
    get {
      return listHeader.SortDirection;
    }

    set {
      if(listHeader.SortDirection != value) {
        listHeader.SortDirection = value;
      }
    }
  }

  /// <summary>
  ///		Gets the number of selected ListboxItems attached to the grid.
  /// </summary>
  public int SelectedItemCount {
    get {
      int count = 0;

      for(int row = 0; row < RowCount; ++row) {
        for(int column = 0; column < ColumnCount; ++column) {
          ListboxItem item = gridData[row][column];

          if((item != null) && item.Selected) {
            ++count;
          }
        }
      }

      return count;
    }
  }


  /// <summary>
  ///		Gets/Sets the ID of the column to be used when one of the 'NominatedColumn' selection modes is used.
  /// </summary>
  [WidgetProperty("NominatedSelectionColumnID")]
  public int NominatedSelectionColumnID {
    get {
      return listHeader[nominatedSelectColumn].ID;
    }

    set {
      NominatedSelectionColumn = GetColumnIndexWithID(value);
    }
  }

  /// <summary>
  ///		Gets/Sets the index of the column to be used when one of the 'NominatedColumn' selection modes is used. 
  /// </summary>
  public int NominatedSelectionColumn {
    get {
      return nominatedSelectColumn;
    }

    set {
      if(nominatedSelectColumn != value) {
        nominatedSelectColumn = value;
        ClearAllSelections();

        OnNominatedSelectColumnChanged(new WindowEventArgs(this));
      }
    }
  }

  /// <summary>
  ///		Gets/Sets the index of the row to be used when one of the 'NominatedRow' selection modes is used.
  /// </summary>
  [WidgetProperty("NominatedSelectionRow")]
  public int NominatedSelectionRow {
    get {
      return nominatedSelectRow;
    }

    set {
      if(nominatedSelectRow != value) {
        nominatedSelectRow = value;
        ClearAllSelections();

        OnNominatedSelectRowChanged(new WindowEventArgs(this));
      }
    }
  }

  /// <summary>
  ///		Gets/Sets the selection mode to be used with the grid.
  /// </summary>
  [GridSelectionModeProperty("SelectionMode")]
  public GridSelectionMode SelectionMode {
    get {
      return selectMode;
    }

    set {
      if(selectMode != value) {
        selectMode = value;

        ClearAllSelections();

        switch(selectMode) {
          case GridSelectionMode.RowSingle:
            multiSelect = false;
            fullRowSelect = true;
            fullColumnSelect = false;
            useNominatedColumn = false;
            useNominatedRow = false;
            break;

          case GridSelectionMode.RowMultiple:
            multiSelect = true;
            fullRowSelect = true;
            fullColumnSelect = false;
            useNominatedColumn = false;
            useNominatedRow = false;
            break;

          case GridSelectionMode.CellSingle:
            multiSelect = false;
            fullRowSelect = false;
            fullColumnSelect = false;
            useNominatedColumn = false;
            useNominatedRow = false;
            break;

          case GridSelectionMode.CellMultiple:
            multiSelect = true;
            fullRowSelect = false;
            fullColumnSelect = false;
            useNominatedColumn = false;
            useNominatedRow = false;
            break;

          case GridSelectionMode.NominatedColumnSingle:
            multiSelect = false;
            fullRowSelect = false;
            fullColumnSelect = false;
            useNominatedColumn = true;
            useNominatedRow = false;
            break;

          case GridSelectionMode.NominatedColumnMultiple:
            multiSelect = true;
            fullRowSelect = false;
            fullColumnSelect = false;
            useNominatedColumn = true;
            useNominatedRow = false;
            break;

          case GridSelectionMode.ColumnSingle:
            multiSelect = false;
            fullRowSelect = false;
            fullColumnSelect = true;
            useNominatedColumn = false;
            useNominatedRow = false;
            break;

          case GridSelectionMode.ColumnMultiple:
            multiSelect = true;
            fullRowSelect = false;
            fullColumnSelect = true;
            useNominatedColumn = false;
            useNominatedRow = false;
            break;

          case GridSelectionMode.NominatedRowSingle:
            multiSelect = false;
            fullRowSelect = false;
            fullColumnSelect = false;
            useNominatedColumn = false;
            useNominatedRow = true;
            break;

          case GridSelectionMode.NominatedRowMultiple:
            multiSelect = true;
            fullRowSelect = false;
            fullColumnSelect = false;
            useNominatedColumn = false;
            useNominatedRow = true;
            break;

          default:
            throw new InvalidRequestException("Invalid or unknown SelectionMode value supplied.");
        }

        // Fire event.
        OnSelectionModeChanged(new WindowEventArgs(this));
      }
    }
  }

  /// <summary>
  ///		Gets/Sets whether the vertical scroll bar will be shown even if it is not required.
  /// </summary>
  [WidgetProperty("ForceVertScrollbar")]
  public bool AlwaysShowVerticalScrollbar {
    get {
      return forceVerticalScrollbar;
    }

    set {
      if(forceVerticalScrollbar != value) {
        forceVerticalScrollbar = value;

        ConfigureScrollbars();

        OnVerticalScrollbarModeChanged(new WindowEventArgs(this));
      }
    }
  }

  /// <summary>
  ///		Gets/Sets whether the horizontal scroll bar will be shown even if it is not required.
  /// </summary>
  [WidgetProperty("ForceHorzScrollbar")]
  public bool AlwaysShowHorizontalScrollbar {
    get {
      return forceHorizontalScrollbar;
    }

    set {
      if(forceHorizontalScrollbar != value) {
        forceHorizontalScrollbar = value;

        ConfigureScrollbars();

        OnHorizontalScrollbarModeChanged(new WindowEventArgs(this));
      }
    }
  }

  #endregion

  #region Methods

  #region Accessor type methods

  /// <summary>
  ///		Return the zero based column index of the column with the specified ID.
  /// </summary>
  /// <param name="id">ID code of the column whos index is to be returned.</param>
  /// <returns>Zero based column index of the first column whos ID matches 'id'.</returns>
  /// <exception cref="InvalidRequestException">thrown if no attached column has the requested ID.</exception>
  public int GetColumnIndexWithID(int id) {
    return listHeader.GetColumnIndexFromSegmentID(id);
  }

  /// <summary>
  ///		Return the zero based index of the column whos header text matches the specified text.
  /// </summary>
  /// <param name="text">string containing the text to be searched for.</param>
  /// <returns>Zero based column index of the column whos header has the specified text.</returns>
  /// <exception cref="InvalidRequestException">thrown if no columns header has the requested text.</exception>"
  public int GetColumnIndexWithHeaderText(string text) {
    return listHeader.GetColumnIndexFromSegmentText(text);
  }

  /// <summary>
  ///		Return the width of the column at the specified zero based index.
  /// </summary>
  /// <param name="columnIndex">Zero based index of the column whos width is to be returned.</param>
  /// <returns>Width of the column at the zero based column index specified by 'columnIndex', in absolute pixels.</returns>
  /// <exception cref="InvalidRequestException">thrown if columnIndex is out of range.</exception>
  public float GetColumnPixelWidth(int columnIndex) {
    return listHeader.GetPixelWidthOfColumn(columnIndex);
  }

  /// <summary>
  ///		Return the ListHeaderSegment object for the column at the specified index.
  /// </summary>
  /// <param name="columnIndex">Zero based index of the column whos ListHeaderSegment is to be returned.</param>
  /// <returns>ListHeaderSegment object for the column at the requested index.</returns>
  /// <exception cref="InvalidRequestException">thrown if columnIndex is out of range.</exception>
  public ListHeaderSegment GetColumnHeaderSegment(int columnIndex) {
    return listHeader[columnIndex];
  }

  /// <summary>
  ///		Return the zero based index of the Row that contains the given ListboxItem.
  /// </summary>
  /// <param name="item">ListboxItem for which the row index is to be returned.</param>
  /// <returns>Zero based index of the row that contains ListboxItem 'item'.</returns>
  /// <exception cref="InvalidRequestException">thrown if 'item' is not attached to the grid.</exception>
  public int GetRowIndexOfItem(ListboxItem item) {
    for(int i = 0; i < RowCount; ++i) {
      if(IsItemInRow(item, i)) {
        return i;
      }
    }

    throw new InvalidRequestException("The specified ListboxItem is not attached to the grid.");
  }

  /// <summary>
  ///		Return the zero based index of the column that contains 'item'.
  /// </summary>
  /// <param name="item">ListboxItem for which the column index is to returned.</param>
  /// <returns>Zero based index of the column that contains ListboxItem 'item'.</returns>
  /// <exception cref="InvalidRequestException">thrown if 'item' is not attached to the grid.</exception>
  public int GetColumnIndexOfItem(ListboxItem item) {
    for(int i = 0; i < ColumnCount; ++i) {
      if(IsItemInColumn(item, i)) {
        return i;
      }
    }

    throw new InvalidRequestException("The specified ListboxItem is not attached to the grid.");
  }

  /// <summary>
  ///		Return the grid reference for 'item'.
  /// </summary>
  /// <param name="item">ListboxItem for which the grid reference is to be returned.</param>
  /// <returns>GridReference describing the current grid reference of ListboxItem 'item'.</returns>
  /// <exception cref="InvalidRequestException">thrown if 'item' is not attached to the grid.</exception>
  public GridReference GetGridReferenceOfItem(ListboxItem item) {
    return new GridReference(GetRowIndexOfItem(item), GetColumnIndexOfItem(item));
  }

  /// <summary>
  ///		Return the ListboxItem at the specified grid reference.
  /// </summary>
  /// <param name="gridRef">GridReference that describes the position of the ListboxItem to be returned.</param>
  /// <returns>ListboxItem at grid reference 'gridRef'</returns>
  /// <exception cref="InvalidRequestException">thrown if 'gridRef' is invalid.</exception>
  public ListboxItem GetItemAtGridReference(GridReference gridRef) {
    // check for invalid grid ref
    if(gridRef.Column >= ColumnCount) {
      throw new InvalidRequestException("The column index {0} is invalid for this grid.", gridRef.Column);
    } else if(gridRef.Row >= RowCount) {
      throw new InvalidRequestException("The row index {0} is invalid for this grid.", gridRef.Row);
    } else {
      return gridData[gridRef.Row][gridRef.Column];
    }
  }

  /// <summary>
  ///		Return whether ListboxItem 'item' is attached to the column at index 'columnIndex'.
  /// </summary>
  /// <param name="item">ListboxItem to look for.</param>
  /// <param name="columnIndex">Zero based index of the column that is to be searched.</param>
  /// <returns>
  ///		- true if 'item' is attached to column 'columnIndex'.
  ///		- false if 'item' is not attached to column 'columnIndex'.
  /// </returns>
  /// <exception cref="InvalidRequestException">thrown if 'columnIndex' is out of range.</exception>
  public bool IsItemInColumn(ListboxItem item, int columnIndex) {
    // check for invalid index
    if(columnIndex >= ColumnCount) {
      throw new InvalidRequestException("Column index {0} is invalid for this grid.", columnIndex);
    } else {
      for(int i = 0; i < RowCount; ++i) {
        if(gridData[i][columnIndex] == item) {
          return true;
        }
      }

      // item was not in search column
      return false;
    }
  }

  /// <summary>
  ///		Return whether ListboxItem 'item' is attached to the row at index 'rowIndex'.
  /// </summary>
  /// <param name="item">ListboxItem to look for.</param>
  /// <param name="rowIndex">Zero based index of the row that is to be searched.</param>
  /// <returns>
  ///		- true if 'item' is attached to row 'rowIndex'.
  ///		- false if 'item' is not attached row 'rowIndex'.
  /// </returns>
  /// <exception cref="InvalidRequestException">thrown if 'rowIndex' is out of range.</exception>
  public bool IsItemInRow(ListboxItem item, int rowIndex) {
    // check for invalid index
    if(rowIndex >= RowCount) {
      throw new InvalidRequestException("Row index {0} is invalid for this grid.", rowIndex);
    } else {
      for(int i = 0; i < ColumnCount; ++i) {
        if(gridData[rowIndex][i] == item) {
          return true;
        }
      }

      // item was not in search row
      return false;
    }
  }

  /// <summary>
  ///		Return whether ListboxItem 'item' is attached to the grid.
  /// </summary>
  /// <param name="item">ListboxItem to look for.</param>
  /// <returns>
  ///		- true if 'item' is attached to the grid.
  ///		- false if 'item' is not attached to the grid.
  /// </returns>
  public bool IsItemInGrid(ListboxItem item) {
    for(int row = 0; row < RowCount; ++row) {
      for(int column = 0; column < ColumnCount; ++column) {
        if(gridData[row][column] == item) {
          return true;
        }
      }
    }

    // item was not in grid.
    return false;
  }

  /// <summary>
  ///		Return the ListboxItem in column 'columnIndex' that has the text string 'text'.
  /// </summary>
  /// <param name="text">string containing the text to be searched for.</param>
  /// <param name="columnIndex">Zero based index of the column to be searched.</param>
  /// <param name="startItem">ListboxItem where the exclusive search is to start, or null to search from the top of the column.</param>
  /// <returns>The first ListboxItem in column 'columnIndex', after 'startItem', that has the string 'text'.</returns>
  /// <exception cref="InvalidRequestException">thrown if 'startItem' is not attached to the grid, or if 'columnIndex' is out of range.</exception>
  public ListboxItem GetColumnItemWithText(string text, int columnIndex, ListboxItem startItem) {
    // check for valid index
    if(columnIndex >= ColumnCount) {
      throw new InvalidRequestException("Column index {0} is invalid for this grid.", columnIndex);
    } else {
      int i = (startItem == null) ? 0 : GetRowIndexOfItem(startItem);

      for(; i < RowCount; ++i) {
        // does item text match?
        if(gridData[i][columnIndex].Text == text) {
          return gridData[i][columnIndex];
        }
      }

      // no matching items
      return null;
    }
  }

  /// <summary>
  ///		Return the ListboxItem in row 'rowIndex' that has the text string 'text'.
  /// </summary>
  /// <param name="text">string containing the text to be searched for.</param>
  /// <param name="rowIndex">Zero based index of the row to be searched.</param>
  /// <param name="startItem">ListboxItem where the exclusive search is to start, or null to search from the start of the row.</param>
  /// <returns>The first ListboxItem in row 'rowIndex', after 'startItem', that has the string 'text'.</returns>
  /// <exception cref="InvalidRequestException">thrown if 'startItem' is not attached to the grid, or if 'rowIndex' is out of range.</exception>
  public ListboxItem GetRowItemWithText(string text, int rowIndex, ListboxItem startItem) {
    // check for valid index
    if(rowIndex >= RowCount) {
      throw new InvalidRequestException("Row index {0} is invalid for this grid.", rowIndex);
    } else {
      int i = (startItem == null) ? 0 : GetColumnIndexOfItem(startItem);

      for(; i < ColumnCount; ++i) {
        // does item text match?
        if(gridData[rowIndex][i].Text == text) {
          return gridData[rowIndex][i];
        }
      }

      // no matching items
      return null;
    }
  }

  /// <summary>
  ///		Return the ListboxItem that has the text string 'text'.
  /// </summary>
  /// <remarks>Searching progresses across the columns in each row.</remarks>
  /// <param name="text">string containing the text to be searched for.</param>
  /// <param name="startItem">ListboxItem where the exclusive search is to start, or null to search the whole grid.</param>
  /// <returns>The first ListboxItem, after 'startItem', that has the string 'text'.</returns>
  /// <exception cref="InvalidRequestException">thrown if 'startItem' is not attached to the grid.</exception>
  public ListboxItem GetItemWithText(string text, ListboxItem startItem) {
    GridReference searchRef;

    // obtain starting location
    if(startItem == null) {
      searchRef = new GridReference(0, 0);
    } else {
      searchRef = GetGridReferenceOfItem(startItem);
    }

    // perform search
    for(; searchRef.Row < RowCount; ++searchRef.Row) {
      for(; searchRef.Column < ColumnCount; ++searchRef.Column) {
        // is this a match?
        if(gridData[searchRef.Row][searchRef.Column].Text == text) {
          return gridData[searchRef.Row][searchRef.Column];
        }
      }
    }

    // no match
    return null;
  }

  /// <summary>
  ///		Return a the first selected ListboxItem attached to the grid.
  /// </summary>
  /// <remarks>Searching progresses across the columns in each row.</remarks>
  /// <returns>The first ListboxItem attached to the grid that is selected, or null if no item is selected.</returns>
  public ListboxItem GetFirstSelectedItem() {
    return GetNextSelectedItem(null);
  }

  /// <summary>
  ///		Return the next selected ListboxItem after 'startItem'.
  /// </summary>
  /// <remarks>Searching progresses across the columns in each row.</remarks>
  /// <param name="startItem">ListboxItem where the exclusive search is to start, or null to search the whole grid.</param>
  /// <returns>The first selected ListboxItem attached to the grid, after 'startItem', or null if no such item is selected.</returns>
  /// <exception cref="InvalidRequestException">thrown if 'startItem' is not attached to the grid.</exception>
  public ListboxItem GetNextSelectedItem(ListboxItem startItem) {
    GridReference startRef = new GridReference(0, 0);

    // obtain starting location
    if(startItem != null) {
        startRef = GetGridReferenceOfItem(startItem);
        if (++startRef.Column == ColumnCount)
        {
            startRef.Column = 0;
            ++startRef.Row;
        }
    }

    // perform search
    for(int i = startRef.Row; i < RowCount; ++i) {
      for(int j=  startRef.Column; j < ColumnCount; ++j) {
        // is this a match?
        ListboxItem item = gridData[i][j];
        if((item != null) && item.Selected) {
          return item;
        }
      }
    }

    // no match
    return null;
  }

  /// <summary>
  ///		Return whether the ListboxItem at 'gridRef' is selected.
  /// </summary>
  /// <param name="gridRef">GridReference describing the grid reference that is to be examined.</param>
  /// <returns>
  ///		- true if there is a ListboxItem at 'gridRef' and it is selected.
  ///		- false if there is no ListboxItem at 'gridRef', or if the item is not selected.
  /// </returns>
  /// <exception cref="InvalidRequestException">thrown if 'gridRef' contains an invalid grid position.</exception>
  public bool IsItemSelected(GridReference gridRef) {
    ListboxItem item = GetItemAtGridReference(gridRef);

    if(item != null) {
      return item.Selected;
    } else {
      return false;
    }
  }

  /// <summary>
  ///		Return the ID code assigned to the specified column.
  /// </summary>
  /// <param name="columnIndex">Zero based index of the column whos ID code is to be returned.</param>
  /// <returns>Current ID code assigned to the column at the requested index.</returns>
  /// <exception cref="InvalidRequestException">thrown if 'columnIndex' is out of range</exception>
  public int GetColumnID(int columnIndex) {
    return listHeader[columnIndex].ID;
  }

  #endregion

  #region Manipulator type methods.

  /// <summary>
  ///		Initialise the Window based object ready for use.
  /// </summary>
  public override void Initialize() {
    base.Initialize();

    // create component widgets
    verticalScrollbar = CreateVerticalScrollbar();
    horizontalScrollbar = CreateHorizontalScrollbar();
    listHeader = CreateListHeader();

    // add component widgets
    AddChild(verticalScrollbar);
    AddChild(horizontalScrollbar);
    AddChild(listHeader);

    // subscribe to event notifications
    listHeader.ScrollOffsetChanged += new WindowEventHandler(ScrollOffsetChanged_handler);
    listHeader.SegmentSequenceChanged += new HeaderSequenceEventHandler(SegmentSequenceChanged_handler);
    listHeader.SegmentSized += new WindowEventHandler(SegmentSized_handler);
    listHeader.SortColumnChanged += new WindowEventHandler(SortColumnChanged_handler);
    listHeader.SortDirectionChanged += new WindowEventHandler(SortDirectionChanged_handler);
    listHeader.SplitterDoubleClicked += new WindowEventHandler(SplitterDoubleClicked_handler);
    horizontalScrollbar.ScrollPositionChanged += new WindowEventHandler(ScrollPositionChanged_handler);

    // do some final initialisation now we are complete
    SortDirection = SortDirection.None;

    // do initial layout
    LayoutComponentWidgets();
  }

  /// <summary>
  ///		Remove all items from the grid.
  /// </summary>
  public void ResetList() {
    if(ResetListImpl()) {
      OnContentsChanged(new WindowEventArgs(this));
    }
  }

  /// <summary>
  ///		Add a column to the grid.
  /// </summary>
  /// <param name="heading">string containing the text label for the column header.</param>
  /// <param name="columnID">ID code to be assigned to the column header.</param>
  /// <param name="width">Initial width to be set for the column using the active metrics mode for this window.</param>
  public void AddColumn(string heading, int columnID, float width) {
    InsertColumn(heading, columnID, width, ColumnCount);
  }

  /// <summary>
  ///		Add multiple columns to the grid.
  /// </summary>
  /// <param name="widths">Initial widths to be set for the columns using the active metrics mode for this window.</param>
  /// <param name="headers">the labels for the column header.</param>
  public void AddColumns(float[] widths, params string[] headers) {
    if(widths.Length != headers.Length)
      throw new ArgumentException("The number of column widths must be the same as the number of column headers.");
    for(int i = 0; i < headers.Length; i++) {
      AddColumn(headers[i], i, widths[i]);
    }
  }

  /// <summary>
  /// Insert a new column in the grid.
  /// </summary>
  /// <param name="heading">string containing the text label for the column header.</param>
  /// <param name="columnID">ID code to be assigned to the column header.</param>
  /// <param name="width">Initial width to be set for the column using the active metrics mode for this window.</param>
  /// <param name="positionIndex">Zero based index where the column is to be inserted.  If this is greater than the current number of columns, the new column is inserted at the end.</param>
  public void InsertColumn(string heading, int columnID, float width, int positionIndex) {
    // if position is out of range, insert at end
    if(positionIndex > ColumnCount) {
      positionIndex = ColumnCount;
    }

    // get desired width as pixels
    if(MetricsMode == MetricsMode.Relative) {
      width = RelativeToAbsoluteX(width);
    }

    // insert new column into header
    listHeader.InsertColumn(heading, columnID, listHeader.AbsoluteToRelativeX(width), positionIndex);

    // insert a blank entry for new column in each row
    for(int i = 0; i < RowCount; ++i) {
      gridData[i].rowItems.Insert(positionIndex, null);
    }

    // update nominated column index if that has now changed
    if((nominatedSelectColumn >= positionIndex) && (ColumnCount > 1)) {
      ++nominatedSelectColumn;
    }

    // signal that the grid content has changed
    OnContentsChanged(new WindowEventArgs(this));
  }

  /// <summary>
  ///		Removes a column from the grid.
  /// </summary>
  /// <param name="columnIndex">Zero based index of the column to be removed.</param>
  /// <exception cref="InvalidRequestException">thrown if 'columnIndex' is invalid.</exception>
  public void RemoveColumn(int columnIndex) {
    // check for invalid index
    if(columnIndex >= ColumnCount) {
      throw new InvalidRequestException("Column index {0} is invalid for this grid.", columnIndex);
    } else {
      // if we're removing the nominated selection column, reset that to 0.
      if(nominatedSelectColumn == columnIndex) {
        nominatedSelectColumn = 0;
      }

      // remove the column entry in each row
      for(int i = 0; i < RowCount; ++i) {
        gridData[i].rowItems.RemoveAt(columnIndex);
      }

      // remove the header segment
      listHeader.RemoveColumn(columnIndex);

      // signal that the grid content has changed
      OnContentsChanged(new WindowEventArgs(this));
    }
  }

  /// <summary>
  ///		Removes a column from the grid.
  /// </summary>
  /// <param name="columnID">ID code of the column to be deleted.</param>
  /// <exception cref="InvalidRequestException">thrown if no column with 'columnID' is in the grid.</exception>
  public void RemoveColumnWithID(int columnID) {
    RemoveColumn(GetColumnIndexWithID(columnID));
  }

  /// <summary>
  ///		Move the column at index 'columnIndex' so it is at index 'positionIndex'.
  /// </summary>
  /// <param name="columnIndex">Zero based index of the column to be moved.</param>
  /// <param name="positionIndex">Zero based index of the new position for the column.</param>
  /// <exception cref="InvalidRequestException">thrown if 'columnIndex' is invalid.</exception>
  public void MoveColumn(int columnIndex, int positionIndex) {
    // all we have to do here is move the header segment.  Events will ensure that
    // the data gets moved.
    listHeader.MoveColumn(columnIndex, positionIndex);
  }

  /// <summary>
  ///		Move the column with ID 'columnID' so it is at index 'positionIndex'.
  /// </summary>
  /// <param name="columnID">ID code of the column to be moved.</param>
  /// <param name="positionIndex">Zero based index of the new position for the column.</param>
  /// <exception cref="InvalidRequestException">thrown if no column with ID 'columnID' is present.</exception>
  public void MoveColumnWithID(int columnID, int positionIndex) {
    // all we have to do here is move the header segment.  Events will ensure that
    // the data gets moved.
    listHeader.MoveColumn(GetColumnIndexWithID(columnID), positionIndex);
  }

  /// <summary>
  ///		Add an empty row to the grid.
  /// </summary>
  /// <remarks>
  ///		If the list is being sorted, the new row will appear at an appropriate position according to the sorting being
  ///		applied.  If no sorting is being done, the new row will appear at the bottom of the list.
  /// </remarks>
  /// <returns>Initial zero based index of the new row.</returns>
  public int AddRow() {
    return AddRow(null, 0);
  }

  /// <summary>
  ///		Add a row to the grid, and set the item in the column with ID 'columnID' to 'item'.
  /// </summary>
  /// <remarks>
  ///		If the list is being sorted, the new row will appear at an appropriate position according to the sorting being
  ///		applied.  If no sorting is being done, the new row will appear at the bottom of the list.
  /// </remarks>
  /// <param name="item">ListboxItem to be used as the initial contents for the column with ID 'columnID'.</param>
  /// <param name="columnID">ID code of the column whos initial item is to be set to 'item'.</param>
  /// <returns>Initial zero based index of the new row.</returns>
  /// <exception cref="InvalidRequestException">thrown if no column with the specified ID is present.</exception>
  public int AddRow(ListboxItem item, int columnID) {
    int columnIndex = 0;

    // build new row
    GridRow row = new GridRow();
    row.sortColumnIndex = SortColumnIndex;
    row.rowItems.Resize(ColumnCount);

    if(item != null) {
      // discover which column to assign item to
      columnIndex = GetColumnIndexWithID(columnID);

      // establish ownership an enter item into new row.
      item.OwnerWindow = this;
      row[columnIndex] = item;
    }

    int positionIndex;
    gridData.Add(row);
    ResortGrid();

    if(SortDirection != SortDirection.None) {
      positionIndex = gridData.Find(row);
    } else {
      positionIndex = RowCount - 1;
    }

    // signal that the grid content has changed
    OnContentsChanged(new WindowEventArgs(this));

    return positionIndex;
  }

  /// <summary>
  ///		Insert an empty row into the grid.
  /// </summary>
  /// <remarks>
  ///		If the list is being sorted, the new row will appear at an appropriate position according to the sorting being
  ///		applied.  If no sorting is being done, the new row will appear at the specified index.
  /// </remarks>
  /// <param name="rowIndex">Zero based index where the row should be inserted.  If this is greater than the current number of rows, the row is appended to the list.</param>
  /// <returns>Zero based index where the row was actually inserted.</returns>
  public int InsertRow(int rowIndex) {
    return InsertRow(null, 0, rowIndex);
  }

  /// <summary>
  ///		Insert a row into the grid, and set the item in the column with ID 'columnID' to 'item'.
  /// </summary>
  /// <remarks>
  ///		If the list is being sorted, the new row will appear at an appropriate position according to the sorting being
  ///		applied.  If no sorting is being done, the new row will appear at the specified index.
  /// </remarks>
  /// <param name="item">ListboxItem to be used as the initial contents for the column with ID 'columnID'.</param>
  /// <param name="columnID">ID code of the column whos initial item is to be set to 'item'.</param>
  /// <param name="rowIndex">Zero based index where the row should be inserted.  If this is greater than the current number of rows, the row is appended to the list.</param>
  /// <returns>Zero based index where the row was actually inserted.</returns>
  /// <exception cref="InvalidRequestException">thrown if no column with the specified ID is present.</exception>
  public int InsertRow(ListboxItem item, int columnID, int rowIndex) {
    // if list is sorted, just add the row instead
    if(SortDirection != SortDirection.None) {
      return AddRow(item, columnID);
    } else {
      // if insert index is too big, insert at end
      if(rowIndex > RowCount) {
        rowIndex = RowCount;
      }

      // build empty row
      GridRow row = new GridRow();
      row.sortColumnIndex = SortColumnIndex;
      row.rowItems.Resize(ColumnCount);

      // insert the row into the grid
      gridData.Insert(rowIndex, row);

      // set initial item in the new row
      SetGridItem(columnID, rowIndex, item);

      // signal that the grid content has changed
      OnContentsChanged(new WindowEventArgs(this));

      return rowIndex;
    }
  }

  /// <summary>
  ///		Remove the grid row at index 'rowIndex'.
  /// </summary>
  /// <param name="rowIndex">Zero based index of the row to be removed.</param>
  /// <exception cref="InvalidRequestException">thrown if 'rowIndex' is invalid.</exception>
  public void RemoveRow(int rowIndex) {
    // check index
    if(rowIndex >= RowCount) {
      throw new InvalidRequestException("The row index {0} is invalid for this grid.", rowIndex);
    } else {
      // remove row from the grid
      gridData.RemoveAt(rowIndex);

      // re-set nominated row if we have just deleted it
      if(nominatedSelectRow == rowIndex) {
        nominatedSelectRow = 0;
      }

      // signal that the grid content has changed
      OnContentsChanged(new WindowEventArgs(this));
    }
  }

  /// <summary>
  ///		Set the ListboxItem for grid reference 'gridRef'.
  /// </summary>
  /// <param name="gridRef">GridReference describing the location of the item to be set.</param>
  /// <param name="item">ListboxItem to be set at 'gridRef'.</param>
  /// <exception cref="InvalidRequestException">thrown if 'gridRef' contains an invalid grid reference.</exception>
  public void SetGridItem(GridReference gridRef, ListboxItem item) {
    // check for invalid grid ref
    if(gridRef.Column >= ColumnCount) {
      throw new InvalidRequestException("The column index {0} is invalid for this grid.", gridRef.Column);
    } else if(gridRef.Row >= RowCount) {
      throw new InvalidRequestException("The row index {0} is invalid for this grid.", gridRef.Row);
    } else {
      // set new item into grid position
      item.OwnerWindow = this;
      gridData[gridRef.Row][gridRef.Column] = item;

      // if item was put into sort column, resort the grid

      // signal that the grid content has changed
      OnContentsChanged(new WindowEventArgs(this));
    }
  }

  /// <summary>
  ///		Set the ListboxItem for the column with ID 'columnID' in row 'rowIndex'.
  /// </summary>
  /// <param name="columnID">ID code of the column to receive 'item'.</param>
  /// <param name="rowIndex">Zero based index of the row to receive 'item'.</param>
  /// <param name="item">ListboxItem to be put into the grid.</param>
  /// <exception cref="InvalidRequestException">thrown if no column with ID 'columnID' exists, or if 'rowIndex' is out of range.</exception>
  public void SetGridItem(int columnID, int rowIndex, ListboxItem item) {
    SetGridItem(new GridReference(rowIndex, GetColumnIndexWithID(columnID)), item);
  }

  /// <summary>
  ///		Remove the selected state from any currently selected ListboxItem attached to the grid.
  /// </summary>
  public void ClearAllSelections() {
    if(ClearAllSelectionsImpl()) {
      OnSelectionChanged(new WindowEventArgs(this));
    }
  }

  /// <summary>
  ///		Sets or clears the selected state of the given ListboxItem (which must be already attached to the grid).
  /// </summary>
  /// <remarks>
  ///		Depending upon the current selection mode, this may cause other items to be selected, other
  ///		items to be deselected, or for nothing to actually happen at all.
  /// </remarks>
  /// <param name="item">ListboxItem to be affected.</param>
  /// <param name="state">
  ///		- true to put the ListboxItem into the selected state.
  ///		- false to put the ListboxItem into the de-selected state.
  /// </param>
  /// <exception cref="InvalidRequestException">thrown if 'item' is not attached to the grid.</exception>
  public void SetItemSelectState(ListboxItem item, bool state) {
    SetItemSelectState(GetGridReferenceOfItem(item), state);
  }

  /// <summary>
  ///		Sets or clears the selected state of the ListboxItem at the given grid reference.
  /// </summary>
  /// <remarks>
  ///		Depending upon the current selection mode, this may cause other items to be selected, other
  ///		items to be deselected, or for nothing to actually happen at all.
  /// </remarks>
  /// <param name="gridRef">GridReference describing the position of the item to be affected.</param>
  /// <param name="state">
  ///		- true to put the ListboxItem into the selected state.
  ///		- false to put the ListboxItem into the de-selected state.
  /// </param>
  /// <exception cref="InvalidRequestException">thrown if 'gridRef' is invalid.</exception>
  public void SetItemSelectState(GridReference gridRef, bool state) {
    if(SetItemSelectStateImpl(gridRef, state)) {
      OnSelectionChanged(new WindowEventArgs(this));
    }
  }

  /// <summary>
  ///		Inform the grid that one or more attached ListboxItems have been externally modified, and
  ///		the list should re-sync its internal state and refresh the display as needed.
  /// </summary>
  public void HandleUpdatedItemData() {
    ConfigureScrollbars();
    RequestRedraw();
  }

  /// <summary>
  ///		Set the width of the specified column.
  /// </summary>
  /// <param name="columnIndex">Zero based column index of the column whos width is to be set.</param>
  /// <param name="width">float value specifying the new width for the column using the active metrics system.</param>
  /// <exception cref="InvalidRequestException">thrown if 'columnIndex' is out of range.</exception>
  public void SetColumnWidth(int columnIndex, float width) {
    if(MetricsMode == MetricsMode.Relative) {
      width = RelativeToAbsoluteX(width);
    }

    listHeader.SetColumnPixelWidth(columnIndex, width);
  }

  #endregion

  #endregion

  #region Implementation Members

  #region Abstract Properties

  /// <summary>
  ///		Gets the area in un-clipped pixels, the window relative area
  ///		that is to be used for rendering grid items.
  /// </summary>
  protected abstract Rect ListRenderArea {
    get;
  }

  #endregion

  #region Abstract Methods

  /// <summary>
  ///		Create and return a ListHeaer widget for use as the column headers.
  /// </summary>
  /// <returns>ListHeader based object.</returns>
  protected abstract ListHeader CreateListHeader();

  /// <summary>
  ///		Create and return a Scrollbar widget for use as vertical scroll bar.
  /// </summary>
  /// <returns>Scrollbar to be used for scrolling the list vertically.</returns>
  protected abstract Scrollbar CreateVerticalScrollbar();

  /// <summary>
  ///		Create and return a Scrollbar widget for use as horizontal scroll bar.
  /// </summary>
  /// <returns>Scrollbar to be used for scrolling the list horizontally.</returns>
  protected abstract Scrollbar CreateHorizontalScrollbar();

  /// <summary>
  ///		Setup SizeF and position for the component widgets.
  /// </summary>
  protected abstract void LayoutComponentWidgets();

  /// <summary>
  ///		Perform rendering of the widget control frame and other 'static' areas.  This
  ///		method should not render the actual items.  Note that the items are typically
  ///		rendered to layer 3, other layers can be used for rendering imagery behind and
  ///		infront of the items.
  /// </summary>
  /// <param name="z">Z co-ordinate for layer 0.</param>
  protected abstract void RenderBaseImagery(float z);

  #endregion

  #region Properties
  /// <summary>
  ///		Gets the sum of all row heights
  /// </summary>
  protected float TotalRowsHeight {
    get {
      float height = 0.0f;

      for(int i = 0; i < RowCount; ++i) {
        height += GetHighestRowItemHeight(i);
      }

      return height;
    }
  }

  #endregion

  #region Methods

  /// <summary>
  ///		Perform the actual rendering for this Window.
  /// </summary>
  /// <param name="z">float value specifying the base Z co-ordinate that should be used when rendering.</param>
  protected override void DrawSelf(float z) {
    // make sub-class draw box imagery
    RenderBaseImagery(z);

    //
    // Render items
    //
    Vector3 itemPosition = new Vector3();
    SizeF itemSize = new SizeF();
    Rect itemClipper = new Rect();

    // calculate screen area that we are to draw items in to.
    Rect absArea = ListRenderArea;
    absArea.Offset(UnclippedPixelRect.Position);

    // calculate main clipper for rendering area
    Rect clipper = absArea.GetIntersection(PixelRect);

    // initialise item position
    itemPosition.y = absArea.Top - verticalScrollbar.ScrollPosition;
    itemPosition.z = GuiSystem.Instance.Renderer.GetZLayer(3);	// TODO: Magic number?

    float alpha = EffectiveAlpha;

    // loop through items
    for(int row = 0; row < RowCount; ++row) {
      // set initial x position for this row
      itemPosition.x = absArea.Left - horizontalScrollbar.ScrollPosition;

      // calculate height to be used for this row
      itemSize.Height = GetHighestRowItemHeight(row);

      // loop through columns in the row
      for(int column = 0; column < ColumnCount; ++column) {
        // allow item to use full width of column
        itemSize.Width = listHeader.GetPixelWidthOfColumn(column);

        ListboxItem item = gridData[row][column];

        // if there is an item here to be drawn
        if(item != null) {
          // calculate clipper for this item
          itemClipper.Left = itemPosition.x;
          itemClipper.Top = itemPosition.y;
          itemClipper.Size = itemSize;
          itemClipper = itemClipper.GetIntersection(clipper);

          // draw item if not totally clipped
          if(itemClipper.Width != 0) {
            item.Draw(itemPosition, alpha, itemClipper);
          }
        }

        // update position for next column
        itemPosition.x += itemSize.Width;
      }

      // update position for next row
      itemPosition.y += itemSize.Height;
    }
  }

  /// <summary>
  ///		display required integrated scroll bars according to current state of the list box and update their values.
  /// </summary>
  protected void ConfigureScrollbars() {
    float totalHeight = TotalRowsHeight;
    float totalWidth = listHeader.TotalPixelExtent;

    //
    // First show or hide the scroll bars as needed (or requested)
    //
    // show or hide vertical scroll bar as required (or as specified by option)
    if((totalHeight > ListRenderArea.Height) || forceVerticalScrollbar) {
      verticalScrollbar.Show();

      // show or hide horizontal scroll bar as required (or as specified by option)
      if((totalWidth > ListRenderArea.Width) || forceHorizontalScrollbar) {
        horizontalScrollbar.Show();
      } else {
        horizontalScrollbar.Hide();
      }
    } else {
      // show or hide horizontal scroll bar as required (or as specified by option)
      if((totalWidth > ListRenderArea.Width) || forceHorizontalScrollbar) {
        horizontalScrollbar.Show();

        // show or hide vertical scroll bar as required (or as specified by option)
        if((totalHeight > ListRenderArea.Height) || forceVerticalScrollbar) {
          verticalScrollbar.Show();
        } else {
          verticalScrollbar.Hide();
        }
      } else {
        verticalScrollbar.Hide();
        horizontalScrollbar.Hide();
      }
    }

    //
    // Set up scroll bar values
    //
    Rect renderArea = ListRenderArea;

    verticalScrollbar.DocumentSize = totalHeight;
    verticalScrollbar.PageSize = renderArea.Height;
    verticalScrollbar.StepSize = Math.Max(1.0f, renderArea.Height / 10.0f);
    verticalScrollbar.ScrollPosition = verticalScrollbar.ScrollPosition;

    horizontalScrollbar.DocumentSize = totalWidth;
    horizontalScrollbar.PageSize = renderArea.Width;
    horizontalScrollbar.StepSize = Math.Max(1.0f, renderArea.Width / 10.0f);
    horizontalScrollbar.ScrollPosition = horizontalScrollbar.ScrollPosition;
  }

  /// <summary>
  ///		select all items between positions 'start' and 'end' (inclusive).
  /// </summary>
  /// <param name="start"></param>
  /// <param name="end"></param>
  /// <returns>true if something was modified.</returns>
  protected bool SelectRange(GridReference start, GridReference end) {
    GridReference tmpStart = new GridReference(start);
    GridReference tmpEnd = new GridReference(end);

    // ensure start is before end
    if(tmpStart.Row > tmpEnd.Row) {
      tmpStart.Row = tmpEnd.Row;
      tmpEnd.Row = start.Row;
    }

    if(tmpStart.Column > tmpEnd.Column) {
      tmpStart.Column = tmpEnd.Column;
      tmpEnd.Column = start.Column;
    }

    bool modified = false;

    // loop through range, selecting items
    for(int row = tmpStart.Row; row <= tmpEnd.Row; ++row) {
      for(int column = tmpStart.Column; column <= tmpEnd.Column; ++column) {
        ListboxItem item = gridData[row][column];

        if(item != null) {
          modified |= SetItemSelectStateImpl(GetGridReferenceOfItem(item), true);
        }
      }
    }

    return modified;
  }

  /// <summary>
  ///		Return the width of the widest item in the given column
  /// </summary>
  /// <param name="columnIndex"></param>
  /// <returns>width of the widest item in the given column</returns>
  protected float GetWidestColumnItemWidth(int columnIndex) {
    if(columnIndex >= ColumnCount) {
      throw new InvalidRequestException("Column index {0} is invalid for the grid.", columnIndex);
    } else {
      float widest = 0.0f;

      for(int i = 0; i < RowCount; ++i) {
        ListboxItem item = gridData[i][columnIndex];

        // if slot has an item
        if(item != null) {
          SizeF Size = item.Size;

          // is this item widest so far?
          if(Size.Width > widest) {
            widest = Size.Width;
          }
        }
      }

      return widest;
    }
  }

  /// <summary>
  ///		Return the height of the highest item in the given row.
  /// </summary>
  /// <param name="rowIndex"></param>
  /// <returns>height of the highest item in the given row.</returns>
  protected float GetHighestRowItemHeight(int rowIndex) {
    if(rowIndex >= RowCount) {
      throw new InvalidRequestException("Row index {0} is invalid for the grid.", rowIndex);
    } else {
      float tallest = 0.0f;

      for(int i = 0; i < ColumnCount; ++i) {
        ListboxItem item = gridData[rowIndex][i];

        // if slot has an item
        if(item != null) {
          SizeF Size = item.Size;

          // is this item tallest so far?
          if(Size.Height > tallest) {
            tallest = Size.Height;
          }
        }
      }

      return tallest;
    }
  }

  /// <summary>
  ///		Clear the selected state for all items (implementation)
  /// </summary>
  /// <returns>true if some selections were cleared, false if nothing was changed.</returns>
  protected bool ClearAllSelectionsImpl() {
    bool modified = false;

    for(int row = 0; row < RowCount; ++row) {
      for(int column = 0; column < ColumnCount; ++column) {
        ListboxItem item = gridData[row][column];

        if((item != null) && item.Selected) {
          item.Selected = false;
          modified = true;
        }
      }
    }

    return modified;
  }

  /// <summary>
  ///		Return the ListboxItem under the given window local pixel co-ordinate.
  /// </summary>
  /// <param name="point"></param>
  /// <returns>
  ///		ListboxItem that is under window pixel co-ordinate 'point', or null if no
  ///		item is under that position.
  /// </returns>
  protected ListboxItem GetItemAtPoint(PointF point) {
    Rect listArea = ListRenderArea;

    float y = listArea.Top - verticalScrollbar.ScrollPosition;
    float x = listArea.Left - horizontalScrollbar.ScrollPosition;

    for(int row = 0; row < RowCount; ++row) {
      y += GetHighestRowItemHeight(row);

      // is this the row?
      if(point.Y < y) {
        // scan across to find the column
        for(int column = 0; column < ColumnCount; ++column) {
          x += GetColumnPixelWidth(column);

          // is this the column?
          if(point.X < x) {
            // return item at this grid cell
            return gridData[row][column];
          }
        }
      }
    }

    // no item was clicked
    return null;
  }

  /// <summary>
  ///		Set select state for the given item.
  ///		This appropriately selects other  items depending upon the select mode.
  /// </summary>
  /// <param name="gridRef"></param>
  /// <param name="state"></param>
  /// <returns>true if something is changed, else false.</returns>
  protected bool SetItemSelectStateImpl(GridReference gridRef, bool state) {
    // check for invalid grid ref
    if(gridRef.Column >= ColumnCount) {
      throw new InvalidRequestException("The column index {0} is invalid for this grid.", gridRef.Column);
    } else if(gridRef.Row >= RowCount) {
      throw new InvalidRequestException("The row index {0} is invalid for this grid.", gridRef.Row);
    }

    // only continue if state is changing
    if(gridData[gridRef.Row][gridRef.Column].Selected != state) {
      // if using nominated selection row and/ or column, check that they match.
      if((!useNominatedColumn || (nominatedSelectColumn == gridRef.Column)) &&
          (!useNominatedRow || (nominatedSelectRow == gridRef.Row))) {
        // clear current selection if not multi-select box
        if(state && !multiSelect) {
          ClearAllSelectionsImpl();
        }

        // full row?
        if(fullRowSelect) {
          // set selection on all items in the row
          SetSelectForItemsInRow(gridRef.Row, state);
        }
          // full column?
      else if(fullColumnSelect) {
          // set selection on all items in the column
          SetSelectForItemsInColumn(gridRef.Column, state);
        }
          // single item to be affected
      else {
          gridData[gridRef.Row][gridRef.Column].Selected = state;
        }

        return true;
      }

    }

    return false;
  }

  /// <summary>
  ///		Set select state for all items in the given row
  /// </summary>
  /// <param name="rowIndex"></param>
  /// <param name="state"></param>
  protected void SetSelectForItemsInRow(int rowIndex, bool state) {
    for(int i = 0; i < ColumnCount; ++i) {
      ListboxItem item = gridData[rowIndex][i];

      if(item != null) {
        item.Selected = state;
      }
    }
  }

  /// <summary>
  ///		Set select state for all items in the given column
  /// </summary>
  /// <param name="columnIndex"></param>
  /// <param name="state"></param>
  protected void SetSelectForItemsInColumn(int columnIndex, bool state) {
    for(int i = 0; i < RowCount; ++i) {
      ListboxItem item = gridData[i][columnIndex];

      if(item != null) {
        item.Selected = state;
      }
    }
  }

  /// <summary>
  ///		Move the column at index 'columnIndex' so it is at index 'positionIndex'.
  ///		Implementation version which does not move the header segment (since that may have already happned).
  /// </summary>
  /// <param name="columnIndex"></param>
  /// <param name="positionIndex"></param>
  /// <exception cref="InvalidRequestException">thrown if 'columnIndex' is invalid.</exception>
  protected void MoveColumnImpl(int columnIndex, int positionIndex) {
    // ensure column is valid
    if(columnIndex >= ColumnCount) {
      throw new InvalidRequestException("Column index {0} is out of range for this grid.", columnIndex);
    }

    // if new position is too big, move to the end
    if(positionIndex > ColumnCount) {
      positionIndex = ColumnCount;
    }

    // update select column and index value if needed
    if(nominatedSelectColumn == columnIndex) {
      nominatedSelectColumn = positionIndex;
    } else if((columnIndex < nominatedSelectColumn) && (positionIndex >= nominatedSelectColumn)) {
      --nominatedSelectColumn;
    } else if((columnIndex > nominatedSelectColumn) && (positionIndex <= nominatedSelectColumn)) {
      ++nominatedSelectColumn;
    }

    // move entry for this column in each row.
    for(int i = 0; i < RowCount; ++i) {
      ListboxItem item = gridData[i][columnIndex];

      // remove old entry
      gridData[i].rowItems.RemoveAt(columnIndex);

      // insert item at new position
      gridData[i].rowItems.Insert(positionIndex, item);
    }
  }

  /// <summary>
  ///		Remove all items from the grid.
  /// </summary>
  /// <returns>
  ///		- true if the grid contents were changed.
  ///		- false if the grid contents were not changed (already empty?).
  /// </returns>
  protected bool ResetListImpl() {
    // return false if the list is empty
    if(RowCount == 0) {
      return false;
    } else {
      gridData.Clear();

      nominatedSelectRow = 0;
      lastSelectedItem = null;

      return true;
    }
  }

  /// <summary>
  ///		Causes a re-sort of the grid data in the appropriate direction
  /// </summary>
  protected void ResortGrid() {
    if(SortDirection == SortDirection.Descending) {
      gridData.SortDescending();
    } else if(SortDirection == SortDirection.Ascending) {
      gridData.SortAscending();
    }

    // else no sorting, so do nothing
  }

  #endregion

  #endregion

  #region Events

  #region Event Declarations

  /// <summary>
  ///		Event fired when the selection mode for the grid changes.
  /// </summary>
  public event WindowEventHandler SelectionModeChanged;

  /// <summary>
  ///		Event fired when the nominated select column changes.
  /// </summary>
  public event WindowEventHandler NominatedSelectColumnChanged;

  /// <summary>
  ///		Event fired when the nominated select row changes.
  /// </summary>
  public event WindowEventHandler NominatedSelectRowChanged;

  /// <summary>
  ///		Event fired when the vertical scroll bar 'force' setting changes.
  /// </summary>
  public event WindowEventHandler VerticalScrollbarModeChanged;

  /// <summary>
  ///		Event fired when the horizontal scroll bar 'force' setting changes.
  /// </summary>
  public event WindowEventHandler HorizontalScrollbarModeChanged;

  /// <summary>
  ///		Event fired when the current selection(s) within the grid changes.
  /// </summary>
  public event WindowEventHandler SelectionChanged;

  /// <summary>
  ///		Event fired when the contents of the grid changes.
  /// </summary>
  public event WindowEventHandler ContentsChanged;

  /// <summary>
  ///		Event fired when the sort column changes.
  /// </summary>
  public event WindowEventHandler SortColumnChanged;

  /// <summary>
  ///		Event fired when the sort direction changes.
  /// </summary>
  public event WindowEventHandler SortDirectionChanged;

  /// <summary>
  ///		Event fired when the width of a column in the grid changes.
  /// </summary>
  public event WindowEventHandler ColumnSized;

  /// <summary>
  ///		Event fired when the column order changes.
  /// </summary>
  public event WindowEventHandler ColumnMoved;

  #endregion

  #region Trigger Methods

  /// <summary>
  /// Internal handler triggered when the selection mode changes
  /// </summary>
  /// <param name="e"></param>
  protected internal void OnSelectionModeChanged(WindowEventArgs e) {
    if(SelectionModeChanged != null) {
      SelectionModeChanged(this, e);
    }
  }

  /// <summary>
  /// Internal handler triggered when the nominated selection column is changed.
  /// </summary>
  /// <param name="e"></param>
  protected internal void OnNominatedSelectColumnChanged(WindowEventArgs e) {
    if(NominatedSelectColumnChanged != null) {
      NominatedSelectColumnChanged(this, e);
    }
  }

  /// <summary>
  /// Internal handler triggered when the nominated selection row is changed.
  /// </summary>
  /// <param name="e"></param>
  protected internal void OnNominatedSelectRowChanged(WindowEventArgs e) {
    if(NominatedSelectRowChanged != null) {
      NominatedSelectRowChanged(this, e);
    }
  }

  /// <summary>
  /// Internal handler triggered when the vertical scroll bar 'force' setting is changed.
  /// </summary>
  /// <param name="e"></param>
  protected internal void OnVerticalScrollbarModeChanged(WindowEventArgs e) {
    if(VerticalScrollbarModeChanged != null) {
      VerticalScrollbarModeChanged(this, e);
    }
  }

  /// <summary>
  /// Internal handler triggered when the horizontal scroll bar 'force' setting is changed.
  /// </summary>
  /// <param name="e"></param>
  protected internal void OnHorizontalScrollbarModeChanged(WindowEventArgs e) {
    if(HorizontalScrollbarModeChanged != null) {
      HorizontalScrollbarModeChanged(this, e);
    }
  }

  /// <summary>
  /// Internal handler triggered when the grid selection changes.
  /// </summary>
  /// <param name="e"></param>
  protected internal void OnSelectionChanged(WindowEventArgs e) {
    RequestRedraw();

    if(SelectionChanged != null) {
      SelectionChanged(this, e);
    }
  }

  /// <summary>
  /// Internal handler triggered when the grid content changes.
  /// </summary>
  /// <param name="e"></param>
  protected internal void OnContentsChanged(WindowEventArgs e) {
    ConfigureScrollbars();
    RequestRedraw();

    if(ContentsChanged != null) {
      ContentsChanged(this, e);
    }
  }

  /// <summary>
  /// Internal handler triggered when the sort column changes.
  /// </summary>
  /// <param name="e"></param>
  protected internal void OnSortColumnChanged(WindowEventArgs e) {
    RequestRedraw();

    if(SortColumnChanged != null) {
      SortColumnChanged(this, e);
    }
  }

  /// <summary>
  /// Internal handler triggered when the sort direction changes.
  /// </summary>
  /// <param name="e"></param>
  protected internal void OnSortDirectionChanged(WindowEventArgs e) {
    RequestRedraw();

    if(SortDirectionChanged != null) {
      SortDirectionChanged(this, e);
    }
  }

  /// <summary>
  /// Internal handler triggered when a column SizeF changes.
  /// </summary>
  /// <param name="e"></param>
  protected internal void OnColumnSized(WindowEventArgs e) {
    ConfigureScrollbars();
    RequestRedraw();

    if(ColumnSized != null) {
      ColumnSized(this, e);
    }
  }

  /// <summary>
  /// Internal handler triggered when the column sequence changes.
  /// </summary>
  /// <param name="e"></param>
  protected internal void OnColumnMoved(WindowEventArgs e) {
    RequestRedraw();

    if(ColumnMoved != null) {
      ColumnMoved(this, e);
    }
  }
  #endregion

  #endregion

  #region Window Members

  #region Overridden Event Triggers

  /// <summary>
  /// Called when the window's size is changed
  /// </summary>
  /// <param name="e"></param>
  protected internal override void OnSized(GuiEventArgs e) {
    // base class processing
    base.OnSized(e);

    ConfigureScrollbars();
    LayoutComponentWidgets();

    e.Handled = true;
  }

  /// <summary>
  /// Called when user clicks mouse inside window
  /// </summary>
  /// <param name="e"></param>
  protected internal override void OnMouseButtonsDown(MouseEventArgs e) {
    // base class processing
    base.OnMouseButtonsDown(e);

    if(e.Button == System.Windows.Forms.MouseButtons.Left) {
      bool modified = false;

      // clear old selections if no control key is pressed or if not multi-select
      if(((e.SysKeys & ModifierKeys.Control) == 0) || !multiSelect) {
        modified = ClearAllSelectionsImpl();
      }

      // get mouse position as a local pixel value
      PointF localMousePosition = ScreenToWindow(e.Position);

      if(MetricsMode == MetricsMode.Relative) {
        localMousePosition = RelativeToAbsolute(localMousePosition);
      }

      ListboxItem item = GetItemAtPoint(localMousePosition);

      if(item != null) {
        modified = true;

        // select range or item depending upon system keys and last selected item
        if((((e.SysKeys & ModifierKeys.Shift) != 0) && (lastSelectedItem != null)) && multiSelect) {
          modified |= SelectRange(GetGridReferenceOfItem(item), GetGridReferenceOfItem(lastSelectedItem));
        } else {
          modified |= SetItemSelectStateImpl(GetGridReferenceOfItem(item), item.Selected ^ true);
        }

        // update last selected item
        lastSelectedItem = item.Selected ? item : null;
      }

      // fire event if something was changed.
      if(modified) {
        OnSelectionChanged(new WindowEventArgs(this));
      }

      e.Handled = true;
    }
  }

  /// <summary>
  /// Called when user spins mouse wheel
  /// </summary>
  /// <param name="e"></param>
  protected internal override void OnMouseWheel(MouseEventArgs e) {
    // base class processing
    base.OnMouseWheel(e);

    if(verticalScrollbar.Visible && (verticalScrollbar.DocumentSize > verticalScrollbar.PageSize)) {
      verticalScrollbar.ScrollPosition = verticalScrollbar.ScrollPosition + verticalScrollbar.StepSize * -e.WheelDelta;
    } else if(horizontalScrollbar.Visible && (horizontalScrollbar.DocumentSize > horizontalScrollbar.PageSize)) {
      horizontalScrollbar.ScrollPosition = horizontalScrollbar.ScrollPosition + horizontalScrollbar.StepSize * -e.WheelDelta;
    }

    e.Handled = true;
  }

  #endregion

  #endregion

  #region Event Handling methods
  private void ScrollOffsetChanged_handler(object sender, WindowEventArgs e) {
    // grab the header scroll value, convert to pixels, and set the scroll bar to match.
    horizontalScrollbar.ScrollPosition = listHeader.RelativeToAbsoluteX(listHeader.ScrollOffset);
  }

  private void SegmentSequenceChanged_handler(object sender, HeaderSequenceEventArgs e) {
    MoveColumnImpl(e.OldIndex, e.NewIndex);

    // signal change
    OnColumnMoved(new WindowEventArgs(this));
  }

  private void SegmentSized_handler(object sender, WindowEventArgs e) {
    ConfigureScrollbars();

    // signal change
    OnColumnSized(new WindowEventArgs(this));
  }

  private void SortColumnChanged_handler(object sender, WindowEventArgs e) {
    int sortColumn = SortColumnIndex;

    // set new sort column on all rows
    for(int i = 0; i < RowCount; ++i) {
      gridData[i].sortColumnIndex = sortColumn;
    }

    ResortGrid();

    // signal change
    OnSortColumnChanged(new WindowEventArgs(this));
  }

  private void SortDirectionChanged_handler(object sender, WindowEventArgs e) {
    ResortGrid();

    // signal change
    OnSortDirectionChanged(new WindowEventArgs(this));
  }

  private void SplitterDoubleClicked_handler(object sender, WindowEventArgs e) {
    // get the column index for the segment that was double-clicked
    int column = listHeader.GetColumnIndexFromSegment((ListHeaderSegment)e.Window);

    // get the width of the widest item in the column.
    float width = Math.Max(GetWidestColumnItemWidth(column), ListHeader.MinimumSegmentPixelWidth);

    // perform metrics conversion if needed
    if(MetricsMode == MetricsMode.Relative) {
      width = AbsoluteToRelativeX(width);
    }

    // set new column width
    SetColumnWidth(column, width);
  }

  private void ScrollPositionChanged_handler(object sender, WindowEventArgs e) {
    // set header offset to match scroll position
    listHeader.ScrollOffset = listHeader.AbsoluteToRelativeX(horizontalScrollbar.ScrollPosition);
  }

  #endregion
}

} // namespace CeGui.Widgets
