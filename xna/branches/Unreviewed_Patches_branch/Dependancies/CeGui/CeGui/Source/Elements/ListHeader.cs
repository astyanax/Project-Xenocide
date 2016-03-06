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

/// <summary>
/// Base class for list header widget.
/// </summary>
[ AlternateWidgetName("ListHeader") ]
public abstract class ListHeader : Window {
  #region Constants
  /// <summary>
  /// Speed to scroll at when dragging outside header.
  /// </summary>
  const float ScrollSpeed = 8.0f;

  /// <summary>
  /// Minimum width of a segment in pixels.
  /// </summary>
  public const float MinimumSegmentPixelWidth = 20.0f;
  #endregion

  #region Fields
  /// <summary>
  /// Attached segment windows in header order.
  /// </summary>
  HeaderSegmentList segments;

  /// <summary>
  /// Pointer to the segment that is currently set as the sort-key,
  /// </summary>
  ListHeaderSegment sortSegment;

  /// <summary>
  /// true if segments can be sized by the user.
  /// </summary>
  bool sizingEnabled;

  /// <summary>
  /// true if the sort criteria modifications by user are enabled (no sorting is actuall done)
  /// </summary>
  bool sortingEnabled;

  /// <summary>
  /// true if drag & drop moving of columns / segments is enabled.
  /// </summary>
  bool movingEnabled;

  /// <summary>
  /// Base offset used to layout the segments (allows scrolling within the window area)
  /// </summary>
  float segmentOffset;

  /// <summary>
  /// Brief copy of the current sort direction.
  /// </summary>
  SortDirection sortingDirection;

  #endregion

  #region Constructor
  /// <summary>
  ///		Constructor.
  /// </summary>
  /// <param name="type"></param>
  /// <param name="name"></param>
  public ListHeader(string type, string name)
    : base(type, name) {
    segments = new HeaderSegmentList();
    sortSegment = null;
    sizingEnabled = true;
    sortingEnabled = true;
    movingEnabled = true;
    segmentOffset = 0.0f;
    sortingDirection = SortDirection.None;
  }

  #endregion

  #region Properties

  /// <summary>
  /// Gets the number of columns (segments) in the header.
  /// </summary>
  public int ColumnCount {
    get {
      return segments.Count;
    }
  }

  /// <summary>
  /// Gets/Sets the current sort segment
  /// </summary>
  public ListHeaderSegment SortSegment {
    get {
      if(sortSegment == null) {
        throw new InvalidRequestException("Sort segment is null.  Maybe there are no segments attached to the header?");
      } else {
        return sortSegment;
      }
    }

    set {
      SortColumnIndex = GetColumnIndexFromSegment(value);
    }
  }

  /// <summary>
  /// Gets/Sets the current sort column index
  /// </summary>
  public int SortColumnIndex {
    get {
      return GetColumnIndexFromSegment(SortSegment);
    }

    set {
      if(value >= ColumnCount) {
        throw new InvalidRequestException("The specified index ({0}) is out of range for this ListHeader.", value);
      } else {
        // if column is different to current segment
        if(sortSegment != segments[value]) {
          // set sort direction on current segment to none
          if(sortSegment != null) {
            sortSegment.SortDirection = SortDirection.None;
          }

          // set up new sort segment
          sortSegment = segments[value];
          sortSegment.SortDirection = sortingDirection;

          // fire event
          OnSortColumnChanged(new WindowEventArgs(this));
        }
      }
    }
  }

  /// <summary>
  /// Get/Set current sort column via it's ID.
  /// </summary>
  [WidgetProperty("SortColumnID")]
  public int SortColumnID {
    get {
      return SortSegment.ID;
    }

    set {
      SetSortColumnFromID(value);
    }
  }

  /// <summary>
  /// Get the pixel extent of all segments
  /// </summary>
  public float TotalPixelExtent {
    get {
      float extent = 0.0f;

      for(int i = 0; i < ColumnCount; ++i) {
        extent += segments[i].AbsoluteWidth;
      }

      return extent;
    }
  }

  /// <summary>
  /// Gets/Sets the current sorting direction.
  /// </summary>
  [SortDirectionProperty("SortDirection")]
  public SortDirection SortDirection {
    get {
      return sortingDirection;
    }

    set {
      if(sortingDirection != value) {
        sortingDirection = value;

        // set direction of current sort segment
        if(sortSegment != null) {
          sortSegment.SortDirection = sortingDirection;
        }

        // fire event
        OnSortDirectionChanged(new WindowEventArgs(this));
      }
    }
  }

  /// <summary>
  /// Gets/Sets whether use control of the sort column and direction is enabled.
  /// </summary>
  [WidgetProperty("SortSettingEnabled")]
  public bool UserSortControlEnabled {
    get {
      return sortingEnabled;
    }

    set {
      if(sortingEnabled != value) {
        sortingEnabled = value;

        // make the setting change for all segments
        for(int i = 0; i < ColumnCount; ++i) {
          segments[i].Clickable = sortingEnabled;
        }

        // fire event
        OnSortSettingChanged(new WindowEventArgs(this));
      }
    }
  }

  /// <summary>
  /// Gets/Sets whether the user may re-SizeF the header segments.
  /// </summary>
  [WidgetProperty("ColumnsSizable")]
  public bool SegmentSizingEnabled {
    get {
      return sizingEnabled;
    }

    set {
      if(sizingEnabled != value) {
        sizingEnabled = value;

        // modify setting on all segments
        for(int i = 0; i < ColumnCount; ++i) {
          segments[i].Sizable = sizingEnabled;
        }

        // fire event
        OnDragSizeSettingChanged(new WindowEventArgs(this));
      }
    }
  }

  /// <summary>
  /// Gets/Sets whether the user may drag & drop the header segments to change their sequence.
  /// </summary>
  [WidgetProperty("ColumnsMovable")]
  public bool SegmentDraggingEnabled {
    get {
      return movingEnabled;
    }

    set {
      if(movingEnabled != value) {
        movingEnabled = value;

        // modify setting on all segments
        for(int i = 0; i < ColumnCount; ++i) {
          segments[i].Draggable = movingEnabled;
        }

        // fire event
        OnDragMoveSettingChanged(new WindowEventArgs(this));
      }
    }
  }

  /// <summary>
  /// Gets/Sets the current segment scroll offset value.
  /// </summary>
  public float ScrollOffset {
    get {
      return segmentOffset;
    }

    set {
      if(segmentOffset != value) {
        segmentOffset = value;
        LayoutSegments();
        RequestRedraw();

        OnScrollOffsetChanged(new WindowEventArgs(this));
      }
    }
  }

  #endregion

  #region Indexers
  /// <summary>
  /// Indexer to return the ListHeaderSegment for a given column index.
  /// </summary>
  public ListHeaderSegment this[int index] {
    get {
      return segments[index];
    }
  }
  #endregion

  #region Methods

  /// <summary>
  ///		Return the ListHeaderSegment object with the specified ID.
  /// </summary>
  /// <param name="id">id code of the ListHeaderSegment to be returned.</param>
  /// <returns>
  ///		ListHeaderSegment object with the requested ID.  If more than one segment has
  ///		the same ID, only the first one will ever be returned.
  /// </returns>
  /// <exception cref="InvalidRequestException">thrown if no segment with the requested ID is attached.</exception>
  public ListHeaderSegment GetSegmentFromID(int id) {
    for(int i = 0; i < ColumnCount; ++i) {
      if(segments[i].ID == id) {
        return segments[i];
      }
    }

    // no such segment...
    throw new InvalidRequestException("There is no segment attached with the ID code {0}", id);
  }

  /// <summary>
  ///		Return the zero based column index of the specified segment.
  /// </summary>
  /// <param name="segment">ListHeaderSegment whos zero based index is to be returned.</param>
  /// <returns>Zero based column index of the given ListHeaderSegment</returns>
  /// <exception cref="InvalidRequestException">thrown if the given segment is not attached to this ListHeader.</exception>
  public int GetColumnIndexFromSegment(ListHeaderSegment segment) {
    for(int i = 0; i < ColumnCount; ++i) {
      if(segments[i] == segment) {
        return i;
      }
    }

    // no such segment...
    throw new InvalidRequestException("The specified ListHeaderSegment is not attached to the ListHeader.");
  }

  /// <summary>
  ///		Return the zero based column index of the segment with the specified ID.
  /// </summary>
  /// <param name="id">ID code of the segment whos column index is to be returned.</param>
  /// <returns>Zero based column index of the first ListHeaderSegment whos ID matches 'id'.</returns>
  /// <exception cref="InvalidRequestException">thrown if no attached segment has the requested ID.</exception>
  public int GetColumnIndexFromSegmentID(int id) {
    for(int i = 0; i < ColumnCount; ++i) {
      if(segments[i].ID == id) {
        return i;
      }
    }

    // no such segment
    throw new InvalidRequestException("There is no segment attached with the ID code {0}", id);
  }

  /// <summary>
  ///		Return the zero based column index of the segment with the specified text.
  /// </summary>
  /// <param name="text">string containing the text to be searched for.</param>
  /// <returns>Zero based column index of the segment with the specified text.</returns>
  /// <exception cref="InvalidRequestException">thrown if no attached segments have the requested text.</exception>
  public int GetColumnIndexFromSegmentText(string text) {
    for(int i = 0; i < ColumnCount; ++i) {
      if(segments[i].Text == text) {
        return i;
      }
    }

    // no such segment...
    throw new InvalidRequestException("No segment with the text '{0}' is attached to this ListHeader.", text);
  }

  /// <summary>
  ///		Return the pixel offset to the given ListHeaderSegment.
  /// </summary>
  /// <param name="segment">ListHeaderSegment object that the offset to is to be returned.</param>
  /// <returns>The number of pixels up-to the begining of the ListHeaderSegment described by 'segment'.</returns>
  /// <exception cref="InvalidRequestException">thrown if 'segment' is not attached to the ListHeader.</exception>
  public float GetPixelOffsetToSegment(ListHeaderSegment segment) {
    float offset = 0.0f;

    for(int i = 0; i < ColumnCount; ++i) {
      if(segments[i] == segment) {
        return offset;
      }

      offset += segments[i].AbsoluteWidth;
    }

    // no such segment...
    throw new InvalidRequestException("The specified ListHeaderSegment is not attached to this ListHeader.");
  }

  /// <summary>
  ///		Return the pixel offset to the ListHeaderSegment at the given zero based column index.
  /// </summary>
  /// <param name="column">Zero based column index of the ListHeaderSegment whos pixel offset it to be returned.</param>
  /// <returns>
  ///		The number of pixels up-to the begining of the ListHeaderSegment located at zero based column index 'column'.
  /// </returns>
  /// <exception cref="InvalidRequestException">thrown if 'column' is out of range.</exception>
  public float GetPixelOffsetToColumn(int column) {
    if(column >= ColumnCount) {
      throw new InvalidRequestException("The specified index ({0}) is out of range for this ListHeader.", column);
    } else {
      float offset = 0.0f;

      for(int i = 0; i < column; ++i) {
        offset += segments[i].AbsoluteWidth;
      }

      return offset;
    }
  }

  /// <summary>
  ///		Return the pixel width of the specified column.
  /// </summary>
  /// <param name="column">Zero based column index of the segment whos pixel width is to be returned.</param>
  /// <returns>Pixel width of the ListHeaderSegment at the zero based column index specified by 'column'.</returns>
  /// <exception cref="InvalidRequestException">thrown if 'column' is out of range.</exception>
  public float GetPixelWidthOfColumn(int column) {
    if(column >= ColumnCount) {
      throw new InvalidRequestException("The specified index ({0}) is out of range for this ListHeader.", column);
    } else {
      return segments[column].AbsoluteWidth;
    }
  }

  /// <summary>
  ///		Set the column to to be used for sorting via its ID code.
  /// </summary>
  /// <param name="id">ID code of the column segment that is to be used as the sort column.</param>
  /// <exception cref="InvalidRequestException">thrown if no segment with ID 'id' is attached to the ListHeader.</exception>
  public void SetSortColumnFromID(int id) {
    SortColumnIndex = GetColumnIndexFromSegmentID(id);
  }

  /// <summary>
  ///		Add a new column segment to the end of the header.
  /// </summary>
  /// <param name="text">String object holding the initial text for the new segment</param>
  /// <param name="id">Client specified ID code to be assigned to the new segment.</param>
  /// <param name="width">Initial width of the new segment using the active metrics system</param>
  public void AddColumn(string text, int id, float width) {
    // add just inserts at the end
    InsertColumn(text, id, width, ColumnCount);
  }

  /// <summary>
  ///		Insert a new column segment at the specified position.
  /// </summary>
  /// <param name="text">String object holding the initial text for the new segment</param>
  /// <param name="id">Client specified ID code to be assigned to the new segment.</param>
  /// <param name="width">Initial width of the new segment using the active metrics system</param>
  /// <param name="position">
  ///		Zero based column index indicating the desired position for the new column.
  ///		If this is greater than the current number of columns, the new segment is added to the end if the header.
  /// </param>
  public void InsertColumn(string text, int id, float width, int position) {
    // if position is too big, insert at end
    if(position > ColumnCount) {
      position = ColumnCount;
    }

    ListHeaderSegment newSegment = CreateInitialisedSegment(text, id, width);
    segments.Insert(position, newSegment);

    // add new segment as a child window
    AddChild(newSegment);

    LayoutSegments();

    // fire event
    OnSegmentAdded(new WindowEventArgs(this));

    // if sort segment is invalid, set-up sorting now we have a segment
    if(sortSegment == null) {
      SortColumnIndex = position;
    }
  }

  /// <summary>
  ///		Removes a column segment from the ListHeader.
  /// </summary>
  /// <param name="columnIndex">Zero based column index indicating the segment to be removed.</param>
  /// <exception cref="InvalidRequestException">thrown if 'columnIndex' is out of range.</exception>
  public void RemoveColumn(int columnIndex) {
    if(columnIndex >= ColumnCount) {
      throw new InvalidRequestException("The specified index ({0}) is out of range for this ListHeader.", columnIndex);
    } else {
      ListHeaderSegment seg = segments[columnIndex];

      // remove the segment from the list
      segments.Remove(seg);

      // patch up sort segment if we have removed it
      if(sortSegment == seg) {
        // more columns?
        if(ColumnCount > 0) {
          // make first column the new sort column
          sortingDirection = SortDirection.None;
          SortColumnIndex = 0;
        }
          // no columns
      else {
          sortSegment = null;
        }
      }

      // remove the segment from our child list
      RemoveChild(seg);

      DestroyListSegment(seg);
      LayoutSegments();

      // fire event
      OnSegmentRemoved(new WindowEventArgs(this));
    }
  }

  /// <summary>
  ///		Moves a column segment into a new position.
  /// </summary>
  /// <param name="columnIndex">Zero based column index indicating the column segment to be moved.</param>
  /// <param name="positionIndex">
  ///		Zero based column index indicating the new position for the segment.  If this is greater than the
  ///		current number of segments, the segment is moved to the end of the header.
  /// </param>
  /// <exception cref="InvalidRequestException">thrown if 'columnIndex' is out of range for this ListHeader.</exception>
  public void MoveColumn(int columnIndex, int positionIndex) {
    if(columnIndex >= ColumnCount) {
      throw new InvalidRequestException("The specified index ({0}) is out of range for this ListHeader.", columnIndex);
    } else {
      // if the target position is out of range, move to the end
      if(positionIndex > ColumnCount) {
        positionIndex = ColumnCount - 1;
      }

      ListHeaderSegment seg = segments[columnIndex];

      // remove original copy of segment
      segments.Remove(seg);

      // re-insert segment at its new position
      segments.Insert(positionIndex, seg);

      // fire event
      OnSegmentSequenceChanged(new HeaderSequenceEventArgs(columnIndex, positionIndex));

      LayoutSegments();
    }
  }

  /// <summary>
  ///		Insert a new column segment at the specified position.
  /// </summary>
  /// <param name="text">String object holding the initial text for the new segment</param>
  /// <param name="id">Client specified ID code to be assigned to the new segment.</param>
  /// <param name="width">Initial width of the new segment using the relative metrics system</param>
  /// <param name="position">
  ///		ListHeaderSegment object indicating the insert position for the new segment.
  ///		The new segment will be inserted before the segment indicated by 'position'.
  /// </param>
  /// <exception cref="InvalidRequestException">thrown if ListHeaderSegment 'position' is not attached to the ListHeader.</exception>
  public void InsertColumn(string text, int id, float width, ListHeaderSegment position) {
    InsertColumn(text, id, width, GetColumnIndexFromSegment(position));
  }

  /// <summary>
  ///		Remove the specified segment from the ListHeader.
  /// </summary>
  /// <param name="segment">ListHeaderSegment object that is to be removed from the ListHeader.</param>
  /// <exception cref="InvalidRequestException">thrown if 'segment' is not attached to this ListHeader.</exception>
  public void RemoveSegment(ListHeaderSegment segment) {
    RemoveColumn(GetColumnIndexFromSegment(segment));
  }

  /// <summary>
  ///		Move a column segment to a new position.
  /// </summary>
  /// <param name="columnIndex">Zero based column index indicating the column segment to be moved.</param>
  /// <param name="position">
  ///		ListHeaderSegment object indicating the new position for the segment.  The segment at 'columnIndex'
  ///		will be moved behind segment 'position' (that is, segment 'columnIndex' will appear to the right of
  ///		segment 'position').
  /// </param>
  /// <exception cref="InvalidRequestException">
  ///		thrown if 'columnIndex' is out of range for this ListHeader,
  ///		or if 'position' is not attached to this ListHeader.
  ///		</exception>
  public void MoveColumn(int columnIndex, ListHeaderSegment position) {
    MoveColumn(columnIndex, GetColumnIndexFromSegment(position));
  }

  /// <summary>
  ///		Moves a segment into a new position.
  /// </summary>
  /// <param name="segment">ListHeaderSegment object that is to be moved.</param>
  /// <param name="positionIndex">
  ///		Zero based column index indicating the new position for the segment.
  ///		If this is greater than the current number of segments, the segment is moved to the end of the header.
  /// </param>
  /// <exception cref="InvalidRequestException">thrown if 'segment' is not attached to this ListHeader.</exception>
  public void MoveSegment(ListHeaderSegment segment, int positionIndex) {
    MoveColumn(GetColumnIndexFromSegment(segment), positionIndex);
  }

  /// <summary>
  ///		Move a segment to a new position.
  /// </summary>
  /// <param name="segment">ListHeaderSegment object that is to be moved.</param>
  /// <param name="position">
  ///		ListHeaderSegment object indicating the new position for the segment.  The segment 'segment'
  ///		will be moved behind segment 'position' (that is, segment 'segment' will appear to the right
  ///		of segment 'position').
  /// </param>
  /// <exception cref="InvalidRequestException">thrown if either 'segment' or 'position' are not attached to this ListHeader.</exception>
  public void MoveSegment(ListHeaderSegment segment, ListHeaderSegment position) {
    MoveColumn(GetColumnIndexFromSegment(segment), GetColumnIndexFromSegment(position));
  }

  /// <summary>
  ///		Set the pixel width of the specified column.
  /// </summary>
  /// <param name="columnIndex">Zero based column index of the segment whos pixel width is to be set.</param>
  /// <param name="width">
  ///		float value specifying the new pixel width to set for the ListHeaderSegment at the zero based
  ///		column index specified by 'columnIndex'.
  /// </param>
  /// <exception cref="InvalidRequestException">thrown if 'columnIndex' is out of range.</exception>
  public void SetColumnPixelWidth(int columnIndex, float width) {
    if(columnIndex >= ColumnCount) {
      throw new InvalidRequestException("The specified index ({0}) is out of range for this ListHeader.", columnIndex);
    } else {
      // TODO: Update window to support setting of dimensions in each metrics mode, then 
      // this method can be much simpler.

      ListHeaderSegment seg = segments[columnIndex];

      if(seg.MetricsMode == MetricsMode.Relative) {
        seg.Width = AbsoluteToRelativeX(width);
      } else {
        seg.Width = width;
      }

      LayoutSegments();

      OnSegmentSized(new WindowEventArgs(segments[columnIndex]));
    }
  }

  #endregion

  #region Implementation Members

  #region Properties
  #endregion

  #region Methods

  /// <summary>
  ///		Create initialise and return a ListHeaderSegment object, with all events subscribed and ready to use.
  /// </summary>
  /// <param name="text"></param>
  /// <param name="id"></param>
  /// <param name="width"></param>
  /// <returns></returns>
  protected ListHeaderSegment CreateInitialisedSegment(string text, int id, float width) {
    string uniqueName = string.Format("{0}_segment_{1}", Name, Guid.NewGuid());

    // create segment
    ListHeaderSegment newSegment = CreateNewSegment(uniqueName);

    // set up segment
    newSegment.MetricsMode = MetricsMode.Relative;
    newSegment.Size = new SizeF(width, 1.0f);
    newSegment.MinimumSize = AbsoluteToRelativeImpl(null, new SizeF(MinimumSegmentPixelWidth, 0));
    newSegment.Text = text;
    newSegment.ID = id;

    // subscribe to events we listed to
    newSegment.SegmentSized += new WindowEventHandler(SegmentSized_handler);
    newSegment.SegmentDragStop += new WindowEventHandler(SegmentDragStop_handler);
    newSegment.SegmentClicked += new WindowEventHandler(SegmentClicked_handler);
    newSegment.SplitterDoubleClicked += new WindowEventHandler(SplitterDoubleClicked_handler);
    newSegment.SegmentDragPositionChanged += new WindowEventHandler(SegmentDragPositionChanged_handler);

    return newSegment;
  }

  /// <summary>
  ///		Layout the attached segments
  /// </summary>
  protected void LayoutSegments() {
    PointF position = new PointF(-segmentOffset, 0.0f);

    for(int i = 0; i < ColumnCount; ++i) {
      segments[i].Position = position;
      position.X += segments[i].Width;
    }
  }

  #endregion

  #region Abstract Methods

  /// <summary>
  ///		Create a ListHeaderSegment of an appropriate sub-class type.
  /// </summary>
  /// <param name="name">Unique name for the new segment widget</param>
  /// <returns></returns>
  protected abstract ListHeaderSegment CreateNewSegment(string name);

  /// <summary>
  ///		Destroy the given ListHeaderSegment.
  /// </summary>
  /// <param name="segment">ListHeaderSegment to be destroyed.</param>
  /// <returns></returns>
  protected abstract void DestroyListSegment(ListHeaderSegment segment);

  #endregion

  #endregion

  #region Events

  #region Event Declarations
  /// <summary>
  /// The current sort column changed.
  /// </summary>
  public event WindowEventHandler SortColumnChanged;

  /// <summary>
  /// The sort direction changed.
  /// </summary>
  public event WindowEventHandler SortDirectionChanged;

  /// <summary>
  /// A segment has been sized by the user (e.Window is the segment).
  /// </summary>
  public event WindowEventHandler SegmentSized;

  /// <summary>
  /// A segment has been clicked by the user (e.Window is the segment).
  /// </summary>
  public event WindowEventHandler SegmentClicked;

  /// <summary>
  /// A segment splitter has been double-clicked.  (e.Window is the segment).
  /// </summary>
  public event WindowEventHandler SplitterDoubleClicked;

  /// <summary>
  /// The order of the segments has changed.  ('e' is a HeaderSequenceEventArgs)
  /// </summary>
  public event HeaderSequenceEventHandler SegmentSequenceChanged;

  /// <summary>
  /// A segment has been added to the header.
  /// </summary>
  public event WindowEventHandler SegmentAdded;

  /// <summary>
  /// A segment has been removed from the header.
  /// </summary>
  public event WindowEventHandler SegmentRemoved;

  /// <summary>
  /// The setting that controls user modification to sort configuration changed.
  /// </summary>
  public event WindowEventHandler SortSettingChanged;

  /// <summary>
  /// The setting that controls user drag & drop of segments changed.
  /// </summary>
  public event WindowEventHandler DragMoveSettingChanged;

  /// <summary>
  /// The setting that controls user sizing of segments changed.
  /// </summary>
  public event WindowEventHandler DragSizeSettingChanged;

  /// <summary>
  /// The rendering offset for the segments changed (header has been scrolled).
  /// </summary>
  public event WindowEventHandler ScrollOffsetChanged;

  #endregion

  #region Trigger Methods
  /// <summary>
  ///		Handler invoked internally when the sort column is changed.
  /// </summary>
  /// <param name="e">Events args.</param>
  protected internal void OnSortColumnChanged(WindowEventArgs e) {
    if(SortColumnChanged != null) {
      SortColumnChanged(this, e);
    }
  }

  /// <summary>
  ///		Handler invoked internally when the dort direction is changed.
  /// </summary>
  /// <param name="e">Events args.</param>
  protected internal void OnSortDirectionChanged(WindowEventArgs e) {
    if(SortDirectionChanged != null) {
      SortDirectionChanged(this, e);
    }
  }
  /// <summary>
  ///		Handler invoked internally when a segment SizeF is changed.
  /// </summary>
  /// <param name="e">Events args.</param>
  protected internal void OnSegmentSized(WindowEventArgs e) {
    if(SegmentSized != null) {
      SegmentSized(this, e);
    }
  }
  /// <summary>
  ///		Handler invoked internally when a segment is clicked.
  /// </summary>
  /// <param name="e">Events args.</param>
  protected internal void OnSegmentClicked(WindowEventArgs e) {
    if(SegmentClicked != null) {
      SegmentClicked(this, e);
    }
  }
  /// <summary>
  ///		Handler invoked internally when a segment sizer is double-clicked.
  /// </summary>
  /// <param name="e">Events args.</param>
  protected internal void OnSplitterDoubleClicked(WindowEventArgs e) {
    if(SplitterDoubleClicked != null) {
      SplitterDoubleClicked(this, e);
    }
  }
  /// <summary>
  ///		Handler invoked internally when the segment order is changed.
  /// </summary>
  /// <param name="e">Events args.</param>
  protected internal void OnSegmentSequenceChanged(HeaderSequenceEventArgs e) {
    if(SegmentSequenceChanged != null) {
      SegmentSequenceChanged(this, e);
    }
  }
  /// <summary>
  ///		Handler invoked internally when a segment is added.
  /// </summary>
  /// <param name="e">Events args.</param>
  protected internal void OnSegmentAdded(WindowEventArgs e) {
    if(SegmentAdded != null) {
      SegmentAdded(this, e);
    }
  }
  /// <summary>
  ///		Handler invoked internally when a segment is removed.
  /// </summary>
  /// <param name="e">Events args.</param>
  protected internal void OnSegmentRemoved(WindowEventArgs e) {
    if(SegmentRemoved != null) {
      SegmentRemoved(this, e);
    }
  }
  /// <summary>
  ///		Handler invoked internally when user control of sorting is enabled or disabled.
  /// </summary>
  /// <param name="e">Events args.</param>
  protected internal void OnSortSettingChanged(WindowEventArgs e) {
    if(SortSettingChanged != null) {
      SortSettingChanged(this, e);
    }
  }
  /// <summary>
  ///		Handler invoked internally when drag & drop for segments is enabled or disabled.
  /// </summary>
  /// <param name="e">Events args.</param>
  protected internal void OnDragMoveSettingChanged(WindowEventArgs e) {
    if(DragMoveSettingChanged != null) {
      DragMoveSettingChanged(this, e);
    }
  }
  /// <summary>
  ///		Handler invoked internally when user sizing of segments is enabled or disabled.
  /// </summary>
  /// <param name="e">Events args.</param>
  protected internal void OnDragSizeSettingChanged(WindowEventArgs e) {
    if(DragSizeSettingChanged != null) {
      DragSizeSettingChanged(this, e);
    }
  }
  /// <summary>
  ///		Handler invoked internally when the segment render offset (scroll position) is changed.
  /// </summary>
  /// <param name="e">Events args.</param>
  protected internal void OnScrollOffsetChanged(WindowEventArgs e) {
    if(ScrollOffsetChanged != null) {
      ScrollOffsetChanged(this, e);
    }
  }
  #endregion

  #endregion

  #region Window Members

  #region Overridden Trigger Methods
  #endregion

  #endregion

  #region Event Handler Methods

  /// <summary>
  /// Handler method called when an attached segment is sized.
  /// </summary>
  /// <param name="sender"></param>
  /// <param name="e"></param>
  private void SegmentSized_handler(object sender, WindowEventArgs e) {
    LayoutSegments();

    // fire event
    OnSegmentSized(e);
  }

  /// <summary>
  /// Handler called when an attached segment is dropped after being dragged
  /// </summary>
  /// <param name="sender"></param>
  /// <param name="e"></param>
  private void SegmentDragStop_handler(object sender, WindowEventArgs e) {
    PointF mousePosition = MouseCursor.Instance.Position;

    // segment must be dropped within the ListHeader area
    if(IsHit(mousePosition)) {
      // convert mouse position to local coordinates
      mousePosition = ScreenToWindow(mousePosition);

      // consider header scroll value
      float currWidth = -ScrollOffset;

      // convert figures to pixels if needed
      if(MetricsMode == MetricsMode.Relative) {
        mousePosition = RelativeToAbsolute(mousePosition);
        currWidth = RelativeToAbsoluteX(currWidth);
      }

      // calculate column where dragged segment was dropped
      int columnIndex = 0;
      for(; columnIndex < ColumnCount; ++columnIndex) {
        currWidth += segments[columnIndex].AbsoluteWidth;

        if(mousePosition.X < currWidth) {
          // this is the column; break from loop
          break;
        }
      }

      // perform move operation
      MoveSegment((ListHeaderSegment)e.Window, columnIndex);
    }
  }

  /// <summary>
  /// Handler called when an attached segment is clicked.
  /// </summary>
  /// <param name="sender"></param>
  /// <param name="e"></param>
  private void SegmentClicked_handler(object sender, WindowEventArgs e) {
    // double-check we are set to allow this
    if(sortingEnabled) {
      ListHeaderSegment seg = (ListHeaderSegment)e.Window;

      // are we changing column?
      if(sortSegment != seg) {
        sortingDirection = SortDirection.Descending;
        SortSegment = seg;
      }
        // not a different column; toggle direction instead
    else {
        SortDirection currDirection = sortSegment.SortDirection;

        // set new direction based on current value
        switch(currDirection) {
          case SortDirection.None:
            SortDirection = SortDirection.Descending;
            break;

          case SortDirection.Ascending:
            SortDirection = SortDirection.Descending;
            break;

          case SortDirection.Descending:
            SortDirection = SortDirection.Ascending;
            break;
        }
      }

      // fire event
      OnSegmentClicked(e);
    }

  }

  /// <summary>
  /// Handler called when the sizer/splitter on an attached segment is double-clicked.
  /// </summary>
  /// <param name="sender"></param>
  /// <param name="e"></param>
  private void SplitterDoubleClicked_handler(object sender, WindowEventArgs e) {
    OnSplitterDoubleClicked(e);
  }

  /// <summary>
  /// Handler called when the drag position of a segment changes.
  /// </summary>
  /// <param name="sender"></param>
  /// <param name="e"></param>
  private void SegmentDragPositionChanged_handler(object sender, WindowEventArgs e) {
    // get mouse position as local coordinates
    PointF localMousePos = ScreenToWindow(MouseCursor.Instance.Position);

    MetricsMode metrics = MetricsMode;

    // convert mouse position back to pixels if needed
    if(metrics == MetricsMode.Relative) {
      localMousePos = RelativeToAbsolute(localMousePos);
    }

    // scroll left?
    if(localMousePos.X < 0.0f) {
      if(segmentOffset > 0.0f) {
        float adjust = ScrollSpeed;

        // convert adjustment to relative offset if needed
        if(metrics == MetricsMode.Relative) {
          adjust = AbsoluteToRelativeX(adjust);
        }

        ScrollOffset = Math.Max(0.0f, segmentOffset - adjust);
      }

    // scroll right?
    } else if(localMousePos.X >= AbsoluteWidth) {
      float adjust = ScrollSpeed;
      float currOffset = segmentOffset;
      float maxOffset = Math.Max(0.0f, TotalPixelExtent - AbsoluteWidth);

      // if needed, convert values to something we can use
      if(metrics == MetricsMode.Relative) {
        maxOffset = AbsoluteToRelativeX(maxOffset);
        adjust = AbsoluteToRelativeX(adjust);
        currOffset = RelativeToAbsoluteX(currOffset);
      }

      // if we have not scrolled to the limit
      if(segmentOffset < maxOffset) {
        // scroll, but never beyond max
        ScrollOffset = Math.Min(maxOffset, segmentOffset + adjust);
      }
    }
  }

  #endregion
}

} // namespace CeGui.Widgets
