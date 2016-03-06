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
using System.Collections;
using System.Drawing;

namespace CeGui.Widgets {

/// <summary>
/// Base class for standard Listbox widget.
/// </summary>
[ AlternateWidgetName("Listbox") ]
public abstract class Listbox : Window {
  #region Fields

  /// <summary>
  /// true if list items are sorted.
  /// </summary>
  protected bool sorted;
  /// <summary>
  /// true if the multi-select is enabled.
  /// </summary>
  protected bool multiSelect;
  /// <summary>
  /// true if vertical scrollbar should always be shown.
  /// </summary>
  protected bool forceVertScrollbar;
  /// <summary>
  /// true if horizontal scrollbar should always be shown.
  /// </summary>
  protected bool forceHorzScrollbar;

  /// <summary>
  /// Widget used as the vertical scrollbar of the Listbox.
  /// </summary>
  protected Scrollbar vertScrollbar;
  /// <summary>
  /// Widget used as the horizontal scrollbar of the Listbox.
  /// </summary>
  protected Scrollbar horzScrollbar;

  /// <summary>
  /// Reference to the last selected item (null for none)
  /// </summary>
  protected ListboxItem lastSelectedItem;

  /// <summary>
  /// The list of ListboxItems in the Listbox
  /// </summary>
  protected ListboxItemList items = new ListboxItemList();

  #endregion

  #region Constructor
  /// <summary>
  /// Constructor
  /// </summary>
  /// <param name="type"></param>
  /// <param name="name">unique name for the Window</param>
  public Listbox(string type, string name)
    : base(type, name) {
    sorted = false;
    multiSelect = false;
    forceVertScrollbar = false;
    forceHorzScrollbar = false;
    lastSelectedItem = null;
  }

  #endregion

  #region Properties
  /// <summary>
  /// Get the number of items in the Listbox.  (read only).
  /// </summary>
  public int ItemCount {
    get {
      return items.Count;
    }
  }

  /// <summary>
  /// Get the number of selected items in the Listbox.
  /// </summary>
  public int SelectedCount {
    get {
      int count = 0;

      for(int i = 0; i < ItemCount; ++i) {
        if(items[i].Selected) {
          ++count;
        }
      }

      return count;
    }
  }

  /// <summary>
  /// Get/Set whether the items in the list box are sorted.
  /// </summary>
  public bool Sorted {
    get {
      return sorted;
    }

    set {
      // only react if setting is changing
      if(sorted != value) {
        sorted = value;

        // if we are enabling sorting, we need to sort the list
        if(sorted) {
          items.Sort();
        }

        OnSortModeChanged(new WindowEventArgs(this));
      }
    }
  }

  /// <summary>
  /// Get/Set the selection mode for the Listbox.
  /// </summary>
  public bool Multiselect {
    get {
      return multiSelect;
    }

    set {
      // only react if setting is changing
      if(multiSelect != value) {
        multiSelect = value;

        // if we change to single-select, deselect all except the first selected item.
        if((!multiSelect) && (SelectedCount > 1)) {
          ListboxItem item = GetFirstSelectedItem();

          while((item = GetNextSelectedItem(item)) != null) {
            item.Selected = false;
          }

          OnSelectionChanged(new WindowEventArgs(this));
        }

        OnSelectionModeChanged(new WindowEventArgs(this));
      }
    }
  }

  /// <summary>
  /// Get/Set whether the vertical scrollbar will shown even if it is not required.
  /// </summary>
  public bool AlwaysShowVerticalScrollbar {
    get {
      return forceVertScrollbar;
    }

    set {
      if(forceVertScrollbar != value) {
        forceVertScrollbar = value;
        ConfigureScrollbars();

        OnVerticalScrollbarModeChanged(new WindowEventArgs(this));
      }
    }
  }

  /// <summary>
  /// Get/Set whether the horizontal scrollbar will be shown even if it is not required.
  /// </summary>
  public bool AlwaysShowHorizontalScrollbar {
    get {
      return forceHorzScrollbar;
    }

    set {
      if(forceHorzScrollbar != value) {
        forceHorzScrollbar = value;
        ConfigureScrollbars();

        OnHorizontalScrollbarModeChanged(new WindowEventArgs(this));
      }
    }
  }

  #endregion

  #region Indexer
  /// <summary>
  /// Get the ListboxItem at the given index.
  /// </summary>
  /// <exception cref="InvalidRequestException">Thrown if the specified index is out of range.</exception>
  public ListboxItem this[int index] {
    get {
      if(ItemCount <= index) {
        throw new InvalidRequestException("The Listbox contains {0} items, index {1} is invalid.", ItemCount, index);
      }

      return items[index];
    }
  }

  #endregion

  #region Methods

  /// <summary>
  /// Remove all items from the list.
  /// </summary>
  public void ResetList() {
    items.Clear();
    lastSelectedItem = null;

    OnListContentsChanged(new WindowEventArgs(this));
  }

  /// <summary>
  /// Append a number of items to the end of the list
  /// </summary>
  /// <param name="items">the items to add</param>
  public void AddItems(params ListboxItem[] items) {
    foreach(ListboxItem item in items) {
      AddItem(item);
    }
  }
  /// <summary>
  /// Append a number of entries to the end of the list
  /// </summary>
  /// <param name="textItems">the "text" to show for the entries</param>
  public void AddItems(params string[] textItems) {
    foreach(string text in textItems) {
      AddItem(text);
    }
  }
  /// <summary>
  /// Add an item to the list
  /// </summary>
  /// <param name="item">ListboxItem based object to be added to the Listbox.</param>
  public ListboxItem AddItem(ListboxItem item) {
    if(item != null) {
      // establish ownership
      item.OwnerWindow = this;
      items.Add(item);

      // re-sort list if needed
      if(Sorted) {
        items.Sort();
      }

      OnListContentsChanged(new WindowEventArgs(this));
    }
    return item;
  }

  /// <summary>
  /// Creates a ListboxItem, and appends it to the list.
  /// </summary>
  /// <param name="text">text to give the ListboxItem</param>
  /// <returns>the ListboxItem that has been created</returns>
  public ListboxTextItem AddItem(string text) {
    return (ListboxTextItem)AddItem(new ListboxTextItem(text));
  }

  /// <summary>
  /// Creates the specified ListboxItem, and appends it to the list.
  /// </summary>
  /// <param name="text">text to give the ListboxItem</param>
  /// <param name="id">id to give the ListboxItem</param>
  /// <returns>the ListboxItem that has been created</returns>
  public ListboxTextItem AddItem(string text, int id) {
    return AddItem(text, id, null);
  }

  /// <summary>
  /// Creates the specified ListboxItem, and appends it to the list.
  /// </summary>
  /// <param name="text">text to give the ListboxItem</param>
  /// <param name="id">id to give the ListboxItem</param>
  /// <param name="disabled">if the  ListboxItem is disabled</param>
  /// <returns>the ListboxItem that has been created</returns>
  public ListboxTextItem AddItem(string text, int id, bool disabled) {
    return AddItem(text, id, null, disabled);
  }

  /// <summary>
  /// Creates the specified ListboxItem, and appends it to the list.
  /// </summary>
  /// <param name="text">text to give the ListboxItem</param>
  /// <param name="id">id to give the ListboxItem</param>
  /// <param name="data">data object to give the ListboxItem</param>
  /// <returns>the ListboxItem that has been created</returns>
  public ListboxTextItem AddItem(string text, int id, object data) {
    return AddItem(text, id, data, false);
  }

  /// <summary>
  /// Creates the specified ListboxItem, and appends it to the list.
  /// </summary>
  /// <param name="text">text to give the ListboxItem</param>
  /// <param name="id">id to give the ListboxItem</param>
  /// <param name="data">data object to give the ListboxItem</param>
  /// <returns>the ListboxItem that has been created</returns>
  /// <param name="disabled">if the  ListboxItem is disabled</param>
  public ListboxTextItem AddItem(string text, int id, object data, bool disabled) {
    return (ListboxTextItem)AddItem(new ListboxTextItem(text, id, data, disabled));
  }




  /// <summary>
  /// Insert an item into the list box after a specified item already in the list.
  /// </summary>
  /// <remarks>
  /// Note that if the list is sorted, the item may not end up in the requested position.
  /// </remarks>
  /// <param name="item">ListboxItem to be inserted.  If this parameter is null, nothing happens.</param>
  /// <param name="position">ListboxItem that 'item' is to be inserted after.  If this parameter is null, the item is inserted at the start of the list.</param>
  public void InsertItem(ListboxItem item, ListboxItem position) {
    // if the list is sorted, just do a normal add operation
    if(Sorted) {
      AddItem(item);
    } else if(item != null) {
      // establish ownership
      item.OwnerWindow = this;

      int insertPosition = 0;

      // if position is not null, find insertion point
      if(position != null) {
        insertPosition = GetItemIndex(position);
      }

      items.Insert(insertPosition, item);

      OnListContentsChanged(new WindowEventArgs(this));
    }
  }

  /// <summary>
  /// Removes the given item from the list box.
  /// </summary>
  /// <param name="item">ListboxItem that is to be removed.  If the item is not attached to this list box then nothing will happen.</param>
  public void RemoveItem(ListboxItem item) {
    if(item != null) {
      // if item is attached to this list
      if(items.Find(item) != -1) {
        // disown the item
        item.OwnerWindow = null;

        // remove the item
        items.Remove(item);

        // if the item was the last selected item, reset that to null
        if(lastSelectedItem == item) {
          lastSelectedItem = null;
        }

        OnListContentsChanged(new WindowEventArgs(this));
      }
    }
  }

  /// <summary>
  /// Clear the selected state for all items.
  /// </summary>
  public void ClearAllSelections() {
    if(ClearAllSelectionsImpl()) {
      OnSelectionChanged(new WindowEventArgs(this));
    }
  }

  /// <summary>
  /// Set the select state of an attached ListboxItem.
  /// </summary>
  /// <remarks>
  /// This is the recommended way of selecting and deselecting items attached
  /// to a list box as it respects the multi-select mode setting.  It is
  /// possible to modify the setting on ListboxItems directly, but that
  /// approach does not respect the settings of the list box.					
  /// </remarks>
  /// <param name="item">The ListboxItem to be affected.  This item must be attached to the list box.</param>
  /// <param name="selectState">true to select the item, false to de-select the item.</param>
  /// <exception cref="InvalidRequestException">Thrown if the specified item is not attached to the Listbox.</exception>
  public void SetItemSelectState(ListboxItem item, bool selectState) {
    SetItemSelectState(GetItemIndex(item), selectState);
  }

  /// <summary>
  /// Set the select state of an attached ListboxItem.
  /// </summary>
  /// <remarks>
  /// This is the recommended way of selecting and deselecting items attached
  /// to a list box as it respects the multi-select mode setting.  It is
  /// possible to modify the setting on ListboxItems directly, but that
  /// approach does not respect the settings of the list box.					
  /// </remarks>
  /// <param name="itemIndex">The zero based index of the ListboxItem to be affected.  This must be a valid index (0 &lt;= itemIndex &lt; ItemCount)</param>
  /// <param name="selectState">true to select the item, false to de-select the item.</param>
  /// <exception cref="InvalidRequestException">Thrown if the specified item index is out of range.</exception>
  public void SetItemSelectState(int itemIndex, bool selectState) {
    if(itemIndex < ItemCount) {
      // only do this if the setting is changing
      if(items[itemIndex].Selected != selectState) {
        // conditions apply for single-selection mode
        if(selectState && !multiSelect) {
          ClearAllSelectionsImpl();
        }

        items[itemIndex].Selected = selectState;
        OnSelectionChanged(new WindowEventArgs(this));
      }
    } else {
      throw new InvalidRequestException("The Listbox contains {0} items, index {1} is invalid.", ItemCount, itemIndex);
    }
  }

  /// <summary>
  /// Causes the list box to update it's internal state after changes have been made to one or more
  /// attached ListboxItem objects.
  /// </summary>
  /// <remarks>
  /// Client code must call this whenever it has made any changes to ListboxItem objects already attached to the
  /// list box.  If you are just adding items, or removed items to update them prior to re-adding them, there is
  /// no need to call this method.
  /// </remarks>
  public void HandleUpdatedItemData() {
    ConfigureScrollbars();
    RequestRedraw();
  }

  /// <summary>
  /// Ensure the item at the specified index is visible within the list box.
  /// </summary>
  /// <remarks>
  /// If the specified index value is out of range, the list is always scrolled to the bottom.
  /// </remarks>
  /// <param name="itemIndex">Zero based index of the item to be made visible in the list box.</param>
  public void EnsureItemIsVisible(int itemIndex) {
    // handle simple "scroll to the bottom" case
    if(itemIndex >= ItemCount) {
      vertScrollbar.ScrollPosition = vertScrollbar.DocumentSize - vertScrollbar.PageSize;
    } else {
      float bottom;
      float listHeight = ListRenderArea.Height;
      float top = 0;

      // get height to top of item
      int i;
      for(i = 0; i < itemIndex; ++i) {
        top += items[i].Size.Height;
      }

      // calculate height to bottom of item
      bottom = top + items[i].Size.Height;

      // account for current scrollbar value
      float currPos = vertScrollbar.ScrollPosition;
      top -= currPos;
      bottom -= currPos;

      // if top is above the view area, or if item is too big to fit
      if((top < 0.0f) || ((bottom - top) > listHeight)) {
        // scroll top of item to top of box.
        vertScrollbar.ScrollPosition = currPos + top;
      }
        // if bottom is below the view area
    else if(bottom >= listHeight) {
        // position bottom of item at the bottom of the list
        vertScrollbar.ScrollPosition = currPos + bottom - listHeight;
      }
      // Item is already fully visible - nothing more to do.
    }
  }

  /// <summary>
  /// Ensure the item at the specified index is visible within the list box.
  /// </summary>
  /// <param name="item">ListboxItem to be made visible in the list box.</param>
  /// <exception cref="InvalidRequestException">thrown if the specified item is not attached to this list box.</exception>
  public void EnsureItemIsVisible(ListboxItem item) {
    EnsureItemIsVisible(GetItemIndex(item));
  }

  /// <summary>
  /// Return the first selected item.
  /// </summary>
  /// <returns>First selected ListboxItem or null if no item is selected.</returns>
  public ListboxItem GetFirstSelectedItem() {
    return GetNextSelectedItem(null);
  }

  /// <summary>
  /// Return the next selected item after item 'startItem'.
  /// </summary>
  /// <param name="startItem">ListboxItem where the search for the next selected item is to begin.  If this parameter is null, the search will begin with the first item in the list box.</param>
  /// <returns>ListboxItem based object that is the next selected item in the list after the item specified by \a start_item.  Will return null if no further items were selected.</returns>
  /// <exception cref="InvalidRequestException">thrown if the specified item is not attached to this list box.</exception>
  public ListboxItem GetNextSelectedItem(ListboxItem startItem) {
    // if startItem is null begin search at begining, else start at item after startItem
    int index = (startItem == null) ? 0 : (GetItemIndex(startItem) + 1);

    while(index < ItemCount) {
      // return item 'index' if it is selected.
      if(items[index].Selected) {
        return items[index];
      }
        // not selected, advance to next
    else {
        ++index;
      }
    }

    // no more selected items.
    return null;
  }

  /// <summary>
  /// Return the index of ListboxItem 'item'.
  /// </summary>
  /// <param name="item">ListboxItem whos zero based index is to be returned.</param>
  /// <returns>Zero based index indicating the position of ListboxItem 'item' in the list box.</returns>
  /// <exception cref="InvalidRequestException">thrown if the specified item is not attached to this list box.</exception>
  public int GetItemIndex(ListboxItem item) {
    int index = items.Find(item);

    if(index == -1) {
      throw new InvalidRequestException("The given ListboxItem is not attached to this Listbox.");
    }

    return index;
  }

  /// <summary>
  /// return whether the string at index position 'index' is selected
  /// </summary>
  /// <param name="index">Index of item to check.</param>
  /// <returns>true if the item is selected, false if the item is not selected.</returns>
  /// <exception cref="InvalidRequestException">Thrown if the specified item index is out of range.</exception>
  public bool IsItemSelected(int index) {
    return this[index].Selected;
  }

  /// <summary>
  /// Search the list for an item with the specified text.
  /// </summary>
  /// <remarks>
  /// The search will not include 'startItem'.  If 'startItem' is NULL, the search will
  /// begin from the first item in the list.
  /// </remarks>
  /// <param name="text">string containing the text to be searched for.</param>
  /// <param name="startItem">ListboxItem where the search is to begin.</param>
  /// <returns>The first ListboxItem in the list after 'startItem' that has text matching 'text'.  If no item matches the criteria NULL is returned.</returns>
  /// <exception cref="InvalidRequestException">thrown if 'startItem' is not attached to this list box.</exception>
  public ListboxItem FindItemWithText(string text, ListboxItem startItem) {
    // if startItem is null begin search at begining, else start at item after startItem
    int index = (startItem == null) ? 0 : (GetItemIndex(startItem) + 1);

    while(index < ItemCount) {
      // return this item if the text matches
      if(items[index].Text == text) {
        return items[index];
      }
        // no matching text, advance to next item
    else {
        ++index;
      }
    }

    // no items matched.
    return null;
  }

  /// <summary>
  /// Returns whether the given ListboxItem is attached to the Listbox.
  /// </summary>
  /// <param name="item">ListboxItem to test for.</param>
  /// <returns>true if the item is attached, false if the item is not attached.</returns>
  public bool IsListboxItemInList(ListboxItem item) {
    return (items.Find(item) != -1);
  }

  #endregion

  #region Abstract Members

  #region Properties
  /// <summary>
  /// Get the Rect that describes, in un-clipped pixels, the window relative area
  /// that is to be used for rendering list items.
  /// </summary>
  protected abstract Rect ListRenderArea {
    get;
  }

  #endregion

  #region Methods

  /// <summary>
  /// Create a Widget to be used as the vertical scrollbar in this Listbox.
  /// </summary>
  /// <returns>A Scrollbar based object.</returns>
  protected abstract Scrollbar CreateVerticalScrollbar();

  /// <summary>
  /// Create a widget to be used as the horizontal scrollbar in this Listbox.
  /// </summary>
  /// <returns>A Scrollbar based object.</returns>
  protected abstract Scrollbar CreateHorizontalScrollbar();

  /// <summary>
  /// Layout the component widgets of this Listbox.
  /// </summary>
  protected abstract void LayoutComponentWidgets();

  /// <summary>
  /// Perform rendering of the widget control frame and other 'static' areas.  This
  /// method should not render the actual items.  Note that the items are typically
  /// rendered to layer 3, other layers can be used for rendering imagery behind and
  /// infront of the items.
  /// </summary>
  /// <param name="z">base z co-ordinate (layer 0)</param>
  protected abstract void RenderListboxBaseImagery(float z);

  #endregion

  #endregion

  #region Window Members
  /// <summary>
  ///		Intialize this Listbox widget.
  /// </summary>
  public override void Initialize() {
    // calling in case anything is ever added to the base method
    base.Initialize();

    vertScrollbar = CreateVerticalScrollbar();
    horzScrollbar = CreateHorizontalScrollbar();

    AddChild(vertScrollbar);
    AddChild(horzScrollbar);

    ConfigureScrollbars();
    LayoutComponentWidgets();
  }

  /// <summary>
  ///	Perform rendering for this Listbox.
  /// </summary>
  /// <param name="z">float value specifying the base Z co-ordinate that should be used when rendering.</param>
  protected override void DrawSelf(float z) {
    // get sub-class to draw the box itself
    RenderListboxBaseImagery(z);

    //
    // Render List items
    //
    SizeF itemSize = new SizeF();
    Rect itemClipper = new Rect();
    float widest = WidestItemWidth;

    // calculate on-screen position of area we have to render into
    Rect absArea = ListRenderArea;
    absArea.Offset(UnclippedPixelRect.Position);

    // calculate clipper for entire rendering area
    Rect clipper = absArea.GetIntersection(PixelRect);

    // initialise base values for item rendering
    Vector3 itemPosition = new Vector3
        (
        absArea.Left - horzScrollbar.ScrollPosition,
        absArea.Top - vertScrollbar.ScrollPosition,
        GuiSystem.Instance.Renderer.GetZLayer(3)  // TODO: Magic number?
        );

    float alpha = EffectiveAlpha;

    // loop through items
    for(int i = 0; i < ItemCount; ++i) {
      itemSize.Height = items[i].Size.Height;

      // allow item to have full width of box if this is wider than items
      itemSize.Width = Math.Max(absArea.Width, widest);

      // calculate clipper for this item.
      itemClipper.Left = itemPosition.x;
      itemClipper.Top = itemPosition.y;
      itemClipper.Size = itemSize;
      itemClipper = itemClipper.GetIntersection(clipper);

      // draw this item if it is not totally clipped
      if(itemClipper.Width != 0) {
        items[i].Draw(itemPosition, alpha, itemClipper);
      }

      // update position for next item
      itemPosition.y += itemSize.Height;
    }
  }

  #endregion

  #region Implementation Members

  #region Properties
  /// <summary>
  /// Get the sum of all item heights.
  /// </summary>
  protected float TotalItemsHeight {
    get {
      float height = 0;

      for(int i = 0; i < ItemCount; ++i) {
        height += items[i].Size.Height;
      }

      return height;
    }
  }

  /// <summary>
  /// Get the width of the widest item attached to the listbox.
  /// </summary>
  protected float WidestItemWidth {
    get {
      float widest = 0;

      for(int i = 0; i < ItemCount; ++i) {
        float thisWidth = items[i].Size.Width;

        if(thisWidth > widest) {
          widest = thisWidth;
        }
      }

      return widest;
    }
  }

  #endregion

  #region Methods
  /// <summary>
  /// display required integrated scroll bars according to current state of the list box and update their values.
  /// </summary>
  protected void ConfigureScrollbars() {
    float totalHeight = TotalItemsHeight;
    float widestItem = WidestItemWidth;

    //
    // First show or hide the scroll bars as needed (or requested)
    //
    // show or hide vertical scroll bar as required (or as specified by option)
    if((totalHeight > ListRenderArea.Height) || forceVertScrollbar) {
      vertScrollbar.Show();

      // show or hide horizontal scroll bar as required (or as specified by option)
      if((widestItem > ListRenderArea.Width) || forceHorzScrollbar) {
        horzScrollbar.Show();
      } else {
        horzScrollbar.Hide();
      }
    } else {
      // show or hide horizontal scroll bar as required (or as specified by option)
      if((widestItem > ListRenderArea.Width) || forceHorzScrollbar) {
        horzScrollbar.Show();

        // show or hide vertical scroll bar as required (or as specified by option)
        if((totalHeight > ListRenderArea.Height) || forceVertScrollbar) {
          vertScrollbar.Show();
        } else {
          vertScrollbar.Hide();
        }
      } else {
        vertScrollbar.Hide();
        horzScrollbar.Hide();
      }
    }

    //
    // Set up scroll bar values
    //
    Rect renderArea = ListRenderArea;

    vertScrollbar.DocumentSize = totalHeight;
    vertScrollbar.PageSize = renderArea.Height;
    vertScrollbar.StepSize = Math.Max(1.0f, renderArea.Height / 10.0f);
    vertScrollbar.ScrollPosition = vertScrollbar.ScrollPosition;

    horzScrollbar.DocumentSize = widestItem;
    horzScrollbar.PageSize = renderArea.Width;
    horzScrollbar.StepSize = Math.Max(1.0f, renderArea.Width / 10.0f);
    horzScrollbar.ScrollPosition = horzScrollbar.ScrollPosition;
  }

  /// <summary>
  /// Select all strings between positions startIndex and endIndex (inclusive).
  /// </summary>
  /// <param name="startIndex">Zero based index of the first item to be selected.</param>
  /// <param name="endIndex">Zero based index of the last item to be selected.</param>
  protected void SelectRange(int startIndex, int endIndex) {
    // only continue if list has some items
    if(ItemCount > 0) {
      // if start is out of range, start at begining.
      if(startIndex > ItemCount) {
        startIndex = 0;
      }

      // if end is out of range end at the last item.
      if(endIndex >= ItemCount) {
        endIndex = ItemCount - 1;
      }

      // ensure start becomes before the end.
      if(startIndex > endIndex) {
        int tmp = startIndex;
        startIndex = endIndex;
        endIndex = tmp;
      }

      // perform selections
      for(; startIndex <= endIndex; ++startIndex) {
        items[startIndex].Selected = true;
      }
    }
  }

  /// <summary>
  /// Clear the selected state for all items (implementation)
  /// </summary>
  /// <returns>true if some selections were cleared, false nothing was changed.</returns>
  protected bool ClearAllSelectionsImpl() {
    // flag used so we can track if we did anything.
    bool modified = false;

    for(int i = 0; i < ItemCount; ++i) {
      if(items[i].Selected) {
        items[i].Selected = false;
        modified = true;
      }

    }

    return modified;
  }

  /// <summary>
  /// Return the ListboxItem under the given window local pixel co-ordinate.
  /// </summary>
  /// <param name="point"></param>
  /// <returns>ListboxItem that is under window pixel co-ordinate specified in 'point', or null if no item is under that position.</returns>
  protected ListboxItem GetItemAtPoint(PointF point) {
    float y = ListRenderArea.Top - vertScrollbar.ScrollPosition;

    for(int i = 0; i < ItemCount; ++i) {
      y += items[i].Size.Height;

      if(point.Y < y) {
        return items[i];
      }
    }

    return null;
  }

  #endregion

  #endregion

  #region Events

  #region Event Declarations
  /// <summary>
  /// Event fired when an item is added to, or removed from, the Listbox.
  /// </summary>
  public event WindowEventHandler ListContentsChanged;
  /// <summary>
  /// Event fired when the current list selection changes.
  /// </summary>
  public event WindowEventHandler SelectionChanged;
  /// <summary>
  /// Event fired when the sorting mode of the Listbox changes
  /// </summary>
  public event WindowEventHandler SortModeChanged;
  /// <summary>
  /// Event fired when the 'always show' mode for the vertical scrollbar is changed
  /// </summary>
  public event WindowEventHandler VerticalScrollbarModeChanged;
  /// <summary>
  /// Event fired when the 'always show' mode for the horizontal scrollbar is changed
  /// </summary>
  public event WindowEventHandler HorizontalScrollbarModeChanged;
  /// <summary>
  /// Event fired when the selection mode of the Listbox is changed.
  /// </summary>
  public event WindowEventHandler SelectionModeChanged;

  #endregion

  #region Trigger Methods

  /// <summary>
  /// Triggered when the contents of the Listbox is changed.
  /// </summary>
  /// <param name="e"></param>
  protected internal void OnListContentsChanged(WindowEventArgs e) {
    ConfigureScrollbars();
    RequestRedraw();

    if(ListContentsChanged != null) {
      ListContentsChanged(this, e);
    }
  }

  /// <summary>
  /// Triggered when the current selection within the Listbox changes.
  /// </summary>
  /// <param name="e"></param>
  protected internal void OnSelectionChanged(WindowEventArgs e) {
    RequestRedraw();

    if(SelectionChanged != null) {
      SelectionChanged(this, e);
    }
  }

  /// <summary>
  /// Triggered when the sort mode of the Listbox changes.
  /// </summary>
  /// <param name="e"></param>
  protected internal void OnSortModeChanged(WindowEventArgs e) {
    RequestRedraw();

    if(SortModeChanged != null) {
      SortModeChanged(this, e);
    }
  }

  /// <summary>
  /// Triggered when the 'always show' mode of the vertical scrollbar is changed.
  /// </summary>
  /// <param name="e"></param>
  protected internal void OnVerticalScrollbarModeChanged(WindowEventArgs e) {
    RequestRedraw();

    if(VerticalScrollbarModeChanged != null) {
      VerticalScrollbarModeChanged(this, e);
    }
  }

  /// <summary>
  /// Triggered when the 'always show' mode of the horizontal scrollbar is changed.
  /// </summary>
  /// <param name="e"></param>
  protected internal void OnHorizontalScrollbarModeChanged(WindowEventArgs e) {
    RequestRedraw();

    if(HorizontalScrollbarModeChanged != null) {
      HorizontalScrollbarModeChanged(this, e);
    }
  }

  /// <summary>
  /// Triggered when the selection mode of the Listbox changes.
  /// </summary>
  /// <param name="e"></param>
  protected internal void OnSelectionModeChanged(WindowEventArgs e) {
    if(SelectionModeChanged != null) {
      SelectionModeChanged(this, e);
    }
  }

  #endregion

  #region Overridden Trigger Methods
  /// <summary>
  /// Handler for when the mouse button is pressed
  /// </summary>
  /// <param name="e">Event args.</param>
  protected internal override void OnMouseButtonsDown(MouseEventArgs e) {
    // base class processing
    base.OnMouseButtonsDown(e);

    if(e.Button == System.Windows.Forms.MouseButtons.Left) {
      bool modified = false;

      // clear old selections if no control key is pressed or if multi-select is off
      if(((e.SysKeys & ModifierKeys.Control) == 0) || !multiSelect) {
        modified = ClearAllSelectionsImpl();
      }

      PointF localPosition = ScreenToWindow(e.Position);

      if(MetricsMode == MetricsMode.Relative) {
        localPosition = RelativeToAbsolute(localPosition);
      }

      ListboxItem item = GetItemAtPoint(localPosition);

      if(item != null) {
        modified = true;

        // select range or item, depending upon keys and last selected item
        if((((e.SysKeys & ModifierKeys.Shift) != 0) && (lastSelectedItem != null)) && multiSelect) {
          SelectRange(GetItemIndex(item), GetItemIndex(lastSelectedItem));
        } else {
          item.Selected ^= true;
        }

        // update last selected item
        lastSelectedItem = item.Selected ? item : null;
      }

      // fire event if needed
      if(modified) {
        OnSelectionChanged(new WindowEventArgs(this));
      }

      e.Handled = true;
    }
  }

  /// <summary>
  /// Handler for when the window or widget is sized.
  /// </summary>
  /// <param name="e">Event args.</param>
  protected internal override void OnSized(GuiEventArgs e) {
    // base class handling
    base.OnSized(e);

    ConfigureScrollbars();
    LayoutComponentWidgets();

    e.Handled = true;
  }

  /// <summary>
  /// Handler for when the mouse wheel position is changed.
  /// </summary>
  /// <param name="e">Event args.</param>
  protected internal override void OnMouseWheel(MouseEventArgs e) {
    // base class processing.
    base.OnMouseWheel(e);

    if(vertScrollbar.Visible && (vertScrollbar.DocumentSize > vertScrollbar.PageSize)) {
      vertScrollbar.ScrollPosition = vertScrollbar.ScrollPosition + vertScrollbar.StepSize * -e.WheelDelta;
    } else if(horzScrollbar.Visible && (horzScrollbar.DocumentSize > horzScrollbar.PageSize)) {
      horzScrollbar.ScrollPosition = horzScrollbar.ScrollPosition + horzScrollbar.StepSize * -e.WheelDelta;
    }

    e.Handled = true;
  }

  #endregion

  #region Handlers

  #endregion

  #endregion
}

} // namespace CeGui.Widgets
