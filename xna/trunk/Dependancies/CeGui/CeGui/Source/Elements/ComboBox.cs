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
/// Summary description for ComboBox.
/// </summary>
[ AlternateWidgetName("Combobox") ]
public abstract class ComboBox : Window {
  #region Fields

  /// <summary>
  ///		Editbox widget sub-component.
  /// </summary>
  protected EditBox editBox;
  /// <summary>
  ///		ComboDropList widget sub-component.
  /// </summary>
  protected ComboDropList dropList;
  /// <summary>
  ///		PushButton widget sub-component.
  /// </summary>
  protected PushButton button;

  #endregion Fields

  #region Constructor

  /// <summary>
  ///		Constructor.
  /// </summary>
  /// <param name="name">Name of this widget.</param>
  public ComboBox(string type, string name)
    : base(type, name) {
  }

  #endregion Constructor

  #region Abstract Members

  #region Methods

  /// <summary>
  ///		Create, initialise, and return a reference to an Editbox widget to be used as part
  ///		of this ComboBox.
  /// </summary>
  /// <returns>Reference to a EditBox derived class.</returns>
  public abstract EditBox CreateEditBox();

  /// <summary>
  ///		Create, initialise, and return a reference to an ListBox widget to be used as part
  ///		of this ComboBox.
  /// </summary>
  /// <returns>Reference to a ListBox derived class.</returns>
  public abstract ComboDropList CreateDropList();

  /// <summary>
  ///		Create, initialise, and return a reference to an PushButton widget to be used as part
  ///		of this ComboBox.
  /// </summary>
  /// <returns>Reference to a PushButton derived class.</returns>
  public abstract PushButton CreatePushButton();

  /// <summary>
  /// Layout the component widgets of this ComboBox.
  /// </summary>
  protected abstract void LayoutComponentWidgets();

  #endregion Methods

  #endregion Abstract Members

  #region Base Members

  #region Properties

  #region EditBox Wrappers

  /// <summary>
  ///		Get/Set the current position of the carat.
  /// </summary>
  /// <value>
  ///		Index of the insert carat relative to the start of the text.
  /// </value>
  public int CaratIndex {
    get {
      return editBox.CaratIndex;
    }
    set {
      editBox.CaratIndex = value;
    }
  }

  /// <summary>
  ///		return true if the Editbox has input focus.
  /// </summary>
  /// <value>
  ///		true if the Editbox has keyboard input focus.
  ///		false if the Editbox does not have keyboard input focus.
  /// </value>
  public bool HasInputFocus {
    get {
      return editBox.HasInputFocus;
    }
  }

  /// <summary>
  ///		Return true if the Editbox text is valid given the currently set validation string.
  /// </summary>
  /// <remarks>
  ///		Validation is performed by means of a regular expression.  If the text matches the regex, the text is said to have passed
  ///		validation.  If the text does not match with the regex then the text fails validation.
  /// </remarks>
  /// <value>true if the current Editbox text passes validation, false if the text does not pass validation.</value>
  public bool IsTextValid {
    get {
      return editBox.IsTextValid;
    }
  }

  /// <summary>
  ///		Gets/Sets the maximum text length set for this Editbox.
  /// </summary>
  /// <remarks>
  ///		Depending on the validation string set, the actual length of text that can be entered may be less than the value
  ///		returned here (it will never be more).
  /// </remarks>
  /// <value>
  ///		The maximum number of code points (characters) that can be entered into this Editbox.
  /// </value>
  public int MaxTextLength {
    get {
      return editBox.MaxTextLength;
    }
    set {
      editBox.MaxTextLength = value;
    }
  }

  /// <summary>
  ///		Return the currently set color to be used for rendering the Editbox selection highlight
  ///		when the Editbox is active.
  /// </summary>
  public Colour NormalSelectBrushColor {
    get {
      return editBox.NormalSelectBrushColor;
    }
    set {
      editBox.NormalSelectBrushColor = value;
    }
  }

  /// <summary>
  ///		Return the currently set color to be used for rendering Editbox text in the normal, unselected state.
  /// </summary>
  /// <value>Color object representing the ARGB color that is currently set.</value>
  public Colour NormalTextColor {
    get {
      return editBox.NormalTextColor;
    }
    set {
      editBox.NormalTextColor = value;
    }
  }

  /// <summary>
  ///		Return the currently set color to be used for rendering Editbox text in selected region.
  /// </summary>
  /// <value>Color object representing the ARGB color that is currently set.</value>
  public Colour SelectedTextColor {
    get {
      return editBox.SelectedTextColor;
    }
    set {
      editBox.SelectedTextColor = value;
    }
  }

  public Colour InactiveSelectBrushColor {
    get {
      return editBox.InactiveSelectBrushColor;
    }
    set {
      editBox.InactiveSelectBrushColor = value;
    }
  }

  /// <summary>
  ///		Gets/Sets the read-only state of the editbox.
  /// </summary>
  /// <value>
  ///		true if the Editbox is read only and can't be edited by the user, false if the Editbox is not
  ///		read only and may be edited by the user.
  /// </value>
  public bool ReadOnly {
    get {
      return editBox.ReadOnly;
    }
    set {
      editBox.ReadOnly = value;
    }
  }

  /// <summary>
  ///		Return the current selection start point.
  /// </summary>
  /// <value>
  ///		Index of the selection start point relative to the start of the text.  If no selection is defined this function returns
  ///		the position of the carat.
  ///	</value>
  public int SelectionStartIndex {
    get {
      return editBox.SelectionStartIndex;
    }
  }

  /// <summary>
  ///		Return the current selection end point.
  /// </summary>
  /// <value>
  ///		Index of the selection end point relative to the start of the text.  If no selection is defined this function returns
  ///		the position of the carat.
  ///	</value>
  public int SelectionEndIndex {
    get {
      return editBox.SelectionEndIndex;
    }
  }

  /// <summary>
  ///		Return the length of the current selection (in code points / characters).
  /// </summary>
  /// <value>Number of code points (or characters) contained within the currently defined selection.</value>
  public int SelectionLength {
    get {
      return editBox.SelectionLength;
    }
  }

  /// <summary>
  ///		Get/Set the regular expression used for text validation.
  /// </summary>
  public string ValidationString {
    get {
      return editBox.ValidationString;
    }
    set {
      editBox.ValidationString = value;
    }
  }

  #endregion EditBox Wrappers

  #region ListBox Wrappers

  /// <summary>
  /// Get/Set whether the vertical scrollbar will shown even if it is not required.
  /// </summary>
  public bool AlwaysShowVerticalScrollbar {
    get {
      return dropList.AlwaysShowVerticalScrollbar;
    }

    set {
      dropList.AlwaysShowVerticalScrollbar = value;
    }
  }

  /// <summary>
  /// Get/Set whether the horizontal scrollbar will be shown even if it is not required.
  /// </summary>
  public bool AlwaysShowHorizontalScrollbar {
    get {
      return dropList.AlwaysShowHorizontalScrollbar;
    }

    set {
      dropList.AlwaysShowHorizontalScrollbar = value;
    }
  }

  /// <summary>
  /// Get the number of items in the Listbox.  (read only).
  /// </summary>
  public int ItemCount {
    get {
      return dropList.ItemCount;
    }
  }

  /// <summary>
  ///		Gets the item that was selected in the droplist.
  /// </summary>
  public ListboxItem SelectedItem {
    get {
      return dropList.GetFirstSelectedItem();
    }
  }

  /// <summary>
  /// Get/Set whether the items in the list box are sorted.
  /// </summary>
  public bool Sorted {
    get {
      return dropList.Sorted;
    }

    set {
      dropList.Sorted = value;
    }
  }

  #endregion ListBox Wrappers

  #endregion Properties

  #region Methods

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
    return dropList.FindItemWithText(text, startItem);
  }

  /// <summary>
  /// Return the index of ListboxItem 'item'.
  /// </summary>
  /// <param name="item">ListboxItem whos zero based index is to be returned.</param>
  /// <returns>Zero based index indicating the position of ListboxItem 'item' in the list box.</returns>
  /// <exception cref="InvalidRequestException">thrown if the specified item is not attached to this list box.</exception>
  public int GetItemIndex(ListboxItem item) {
    return dropList.GetItemIndex(item);
  }

  /// <summary>
  /// return whether the string at index position 'index' is selected
  /// </summary>
  /// <param name="index">Index of item to check.</param>
  /// <returns>true if the item is selected, false if the item is not selected.</returns>
  /// <exception cref="InvalidRequestException">Thrown if the specified item index is out of range.</exception>
  public bool IsItemSelected(int index) {
    return dropList.IsItemSelected(index);
  }

  /// <summary>
  /// Returns whether the given ListboxItem is attached to the Listbox.
  /// </summary>
  /// <param name="item">ListboxItem to test for.</param>
  /// <returns>true if the item is attached, false if the item is not attached.</returns>
  public bool IsListboxItemInList(ListboxItem item) {
    return dropList.IsListboxItemInList(item);
  }

  /// <summary>
  ///		Activate the edit box component of the Combobox.
  /// </summary>
  public void ActivateEditBox() {
    if(!editBox.IsActive) {
      editBox.Activate();
    }
  }

  /// <summary>
  ///		Add the given ListboxItem to the list.
  /// </summary>
  /// <param name="item">
  ///		Reference to the ListboxItem to be added to the list.  Note that it is the passed object that is added to the
  ///		list, a copy is not made.  If this parameter is null, nothing happens.
  /// </param>
  public ListboxItem AddItem(ListboxItem item) {
    return dropList.AddItem(item);
  }



  /// <summary>
  ///		Adds the text using a ListboxTextItem to the list.
  /// </summary>
  /// <param name="item">
  ///		Reference to the ListboxItem to be added to the list.  Note that it is the passed object that is added to the
  ///		list, a copy is not made.  If this parameter is null, nothing happens.
  /// </param>
  public ListboxTextItem AddItem(string text) {
    return dropList.AddItem(text);
  }

  /// <summary>
  ///		Add the given ListboxItem's to the list.
  /// </summary>
  /// <param name="item">
  ///		Reference to the ListboxItem to be added to the list.  Note that it is the passed object that is added to the
  ///		list, a copy is not made.  If this parameter is null, nothing happens.
  /// </param>
  public void AddItems(params ListboxItem[] items) {
    dropList.AddItems(items);
  }


  /// <summary>
  ///		Adds each text as a ListboxTextItem to the list.
  /// </summary>
  /// <param name="item">
  ///		Reference to the ListboxItem to be added to the list.  Note that it is the passed object that is added to the
  ///		list, a copy is not made.  If this parameter is null, nothing happens.
  /// </param>
  public void AddItems(params string[] textItems) {
    dropList.AddItems(textItems);
  }

  /// <summary>
  ///		Insert an item into the list box after a specified item already in the list.
  /// </summary>
  /// <remarks>
  ///		Note that if the list is sorted, the item may not end up in the requested position.
  /// </remarks>
  /// <param name="item">
  ///		Reference to the ListboxItem to be inserted.  Note that it is the passed object that is added to the
  ///		list, a copy is not made.  If this parameter is null, nothing happens.
  /// </param>
  /// <param name="position">
  ///		Reference to a ListboxItem that <paramref cref="item"/> is to be inserted after.  
  ///		If this parameter is null, the item is inserted at the start of the list.
  /// </param>
  public void InsertItem(ListboxItem item, ListboxItem position) {
    dropList.InsertItem(item, position);
  }

  /// <summary>
  ///		Removes the given item from the list box.
  /// </summary>
  /// <param name="item">Reference to the item to remove.
  ///		If <paramref cref="item"/> is not attached to this list box then nothing will happen.
  /// </param>
  public void RemoveItem(ListboxItem item) {
    dropList.RemoveItem(item);
  }

  /// <summary>
  ///		Clear the selected state for all items.
  /// </summary>
  public void ClearAllSelections() {
    dropList.ClearAllSelections();
  }

  /// <summary>
  ///		Remove all items from the list.
  /// </summary>
  public void ResetList() {
    dropList.ResetList();
  }

  /// <summary>
  ///		Hides the drop-down list.
  /// </summary>
  public void HideDropList() {
    // the natural order of things when this happens will ensure the list is
    // hidden and events are fired.
    dropList.ReleaseInput();
  }

  /// <summary>
  ///		Show the drop-down list.
  /// </summary>
  public void ShowDropList() {
    // display the box
    dropList.Show();
    dropList.Activate();
    dropList.CaptureInput();

    // trigger event
    OnDropListDisplayed(new WindowEventArgs(this));
  }

  /// <summary>
  ///		Set the select state of an attached ListboxItem.
  /// </summary>
  /// <remarks>
  ///		This is the recommended way of selecting and deselecting items attached to a list box as it respects the
  ///		multi-select mode setting.  It is possible to modify the setting on ListboxItems directly, but that approach
  ///		does not respect the settings of the list box.
  /// </remarks>
  /// <param name="item">The ListboxItem to be affected.  This item must be attached to the list box.</param>
  /// <param name="state">true to select the item, false to de-select the item.</param>
  public void SetItemSelectState(ListboxItem item, bool state) {
    dropList.SetItemSelectState(item, state);
  }

  /// <summary>
  ///		Set the select state of an attached ListboxItem.
  /// </summary>
  /// <remarks>
  ///		This is the recommended way of selecting and deselecting items attached to a list box as it respects the
  ///		multi-select mode setting.  It is possible to modify the setting on ListboxItems directly, but that approach
  ///		does not respect the settings of the list box.
  /// </remarks>
  /// <param name="index">
  ///		The zero based index of the ListboxItem to be affected.  
  ///		This must be a valid index (0 &lt; = index &lt; ItemCount).
  ///	</param>
  /// <param name="state">true to select the item, false to de-select the item.</param>
  public void SetItemSelectState(int index, bool state) {
    dropList.SetItemSelectState(index, state);
  }

  /// <summary>
  ///		Causes the list box to update it's internal state after changes have been made to one or more
  ///		attached ListboxItem objects.
  /// </summary>
  /// <remarks>
  ///		Client code must call this whenever it has made any changes to ListboxItem objects already attached to the
  ///		list box.  If you are just adding items, or removed items to update them prior to re-adding them, there is
  ///		no need to call this method.
  /// </remarks>
  public void HandleUpdatedListItemData() {
    dropList.HandleUpdatedItemData();
  }

  /// <summary>
  ///		Get the ListboxItem at the given index.
  /// </summary>
  /// <exception cref="InvalidRequestException">Thrown if the specified index is out of range.</exception>
  public ListboxItem this[int index] {
    get {
      return dropList[index];
    }
  }

  #endregion Methods

  #endregion Base Members

  #region Window Members

  #region Methods

  /// <summary>
  ///		Initializes the layout and sub-components of this widget.
  /// </summary>
  public override void Initialize() {
    base.Initialize();

    // create the sub-components
    editBox = CreateEditBox();
    dropList = CreateDropList();
    button = CreatePushButton();

    // all components use the same font
    dropList.SetFont(this.Font);
    editBox.SetFont(this.Font);

    // add the sub-components as children of this widget
    AddChild(editBox);
    AddChild(dropList);
    AddChild(button);

    // internal event wiring
    button.Clicked += new GuiEventHandler(button_Clicked);
    dropList.ListSelectionAccepted += new WindowEventHandler(dropList_ListSelectionAccepted);
    dropList.Hidden += new GuiEventHandler(dropList_Hidden);

    // event forwarding setup
    editBox.ReadOnlyChanged += new WindowEventHandler(editBox_ReadOnlyChanged);
    editBox.ValidationStringChanged += new WindowEventHandler(editBox_ValidationStringChanged);
    editBox.MaximumTextLengthChanged += new WindowEventHandler(editBox_MaximumTextLengthChanged);
    editBox.TextInvalidated += new WindowEventHandler(editBox_TextInvalidated);
    editBox.InvalidEntryAttempted += new WindowEventHandler(editBox_InvalidEntryAttempted);
    editBox.CaratMoved += new WindowEventHandler(editBox_CaratMoved);
    editBox.TextSelectionChanged += new WindowEventHandler(editBox_TextSelectionChanged);
    editBox.EditboxFull += new WindowEventHandler(editBox_EditboxFull);
    editBox.TextAccepted += new WindowEventHandler(editBox_TextAccepted);
    editBox.TextChanged += new WindowEventHandler(editBox_TextChanged);
    dropList.ListContentsChanged += new WindowEventHandler(dropList_ListContentsChanged);
    dropList.SelectionChanged += new WindowEventHandler(dropList_SelectionChanged);
    dropList.SortModeChanged += new WindowEventHandler(dropList_SortModeChanged);
    dropList.VerticalScrollbarModeChanged += new WindowEventHandler(dropList_VerticalScrollbarModeChanged);
    dropList.HorizontalScrollbarModeChanged += new WindowEventHandler(dropList_HorizontalScrollbarModeChanged);

    // layout the child components
    LayoutComponentWidgets();
  }

  #endregion Methods

  #endregion Window Members

  #region Events

  #region Event Declarations

  // Editbox events

  /// <summary>
  ///		The read-only mode for the edit box has been changed.
  /// </summary>
  public event WindowEventHandler ReadOnlyChanged;
  /// <summary>
  ///		The validation string has been changed.
  /// </summary>
  public event WindowEventHandler ValidationStringChanged;
  /// <summary>
  ///		The maximum allowable string length has been changed.
  /// </summary>
  public event WindowEventHandler MaximumTextLengthChanged;
  /// <summary>
  ///		Some operation has made the current text invalid with regards to the validation string.
  /// </summary>
  public event WindowEventHandler TextInvalidated;
  /// <summary>
  ///		The user attempted to modify the text in a way that would have made it invalid.
  /// </summary>
  public event WindowEventHandler InvalidEntryAttempted;
  /// <summary>
  ///		The text carat (insert point) has changed.
  /// </summary>
  public event WindowEventHandler CaratMoved;
  /// <summary>
  ///		The current text selection has changed.
  /// </summary>
  public event WindowEventHandler TextSelectionChanged;
  /// <summary>
  ///		The number of characters in the edit box has reached the current maximum.
  /// </summary>
  public event WindowEventHandler EditboxFull;
  /// <summary>
  ///		The user has accepted the current text by pressing Return, Enter, or Tab.
  /// </summary>
  public event WindowEventHandler TextAccepted;

  // ListBox Events

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

  // New Events

  /// <summary>
  ///		Event triggered when the drop-down list is displayed.
  /// </summary>
  public event WindowEventHandler DropListDisplayed;
  /// <summary>
  ///		Event triggered when the drop-down list is removed / hidden.
  /// </summary>
  public event WindowEventHandler DropListRemoved;
  /// <summary>
  ///		Event triggered when the user accepts a selection from the drop-down list.
  /// </summary>
  public event WindowEventHandler ListSelectionAccepted;

  #endregion Event Declarations

  #region Event Handlers

  /// <summary>
  ///		Handle the dropdown button being clicked.
  /// </summary>
  /// <param name="sender"></param>
  /// <param name="e"></param>
  protected void button_Clicked(object sender, GuiEventArgs e) {
    ShowDropList();

    // if there is an item with the same text as the edit box, pre-select it
    ListboxItem item = dropList.FindItemWithText(editBox.Text, null);

    if(item != null) {
      dropList.SetItemSelectState(item, true);
      dropList.EnsureItemIsVisible(item);
    }
  }

  /// <summary>
  ///		Handler for when the droplist hides itself.
  /// </summary>
  /// <param name="sender"></param>
  /// <param name="e"></param>
  protected void dropList_Hidden(object sender, GuiEventArgs e) {
    OnDropListRemoved(new WindowEventArgs(this));
  }

  /// <summary>
  ///		Handler for selections made in the drop-list.
  /// </summary>
  /// <param name="sender"></param>
  /// <param name="e"></param>
  private void dropList_ListSelectionAccepted(object sender, WindowEventArgs e) {
    // copy the text from the selected item into the edit box
    ListboxItem item = ((ComboDropList)e.Window).GetFirstSelectedItem();

    if(item != null) {
      // put the text from the list item into the edit box
      editBox.Text = item.Text;

      // select text if it's editable, and move carat to end
      if(!ReadOnly) {
        editBox.SetSelection(0, item.Text.Length);
        editBox.CaratIndex = item.Text.Length;
      } else {
        // TODO: C++ code doesn't have this in an else block...
        editBox.CaratIndex = 0;
      }

      // trigger event
      OnListSelectionAccepted(new WindowEventArgs(this));

      // finally, activate the editbox
      editBox.Activate();
    }
  }

  protected void editBox_ReadOnlyChanged(object sender, WindowEventArgs e) {
    OnReadOnlyChanged(e);
  }

  protected void editBox_ValidationStringChanged(object sender, WindowEventArgs e) {
    OnValidationStringChanged(e);
  }

  protected void editBox_MaximumTextLengthChanged(object sender, WindowEventArgs e) {
    OnMaximumTextLengthChanged(e);
  }

  protected void editBox_TextInvalidated(object sender, WindowEventArgs e) {
    OnTextInvalidated(e);
  }

  protected void editBox_InvalidEntryAttempted(object sender, WindowEventArgs e) {
    OnInvalidEntryAttempted(e);
  }

  protected void editBox_CaratMoved(object sender, WindowEventArgs e) {
    OnCaratMoved(e);
  }

  protected void editBox_TextSelectionChanged(object sender, WindowEventArgs e) {
    OnTextSelectionChanged(e);
  }

  protected void editBox_EditboxFull(object sender, WindowEventArgs e) {
    OnEditboxFull(e);
  }

  protected void editBox_TextAccepted(object sender, WindowEventArgs e) {
    OnTextAccepted(e);
  }

  protected void editBox_TextChanged(object sender, WindowEventArgs e) {
    // set our text to the text of the editbox
    this.Text = e.Window.Text;
  }

  protected void dropList_ListContentsChanged(object sender, WindowEventArgs e) {
    OnListContentsChanged(e);
  }

  protected void dropList_SelectionChanged(object sender, WindowEventArgs e) {
    OnSelectionChanged(e);
  }

  protected void dropList_SortModeChanged(object sender, WindowEventArgs e) {
    OnSortModeChanged(e);
  }

  protected void dropList_VerticalScrollbarModeChanged(object sender, WindowEventArgs e) {
    OnVerticalScrollbarModeChanged(e);
  }

  protected void dropList_HorizontalScrollbarModeChanged(object sender, WindowEventArgs e) {
    OnHorizontalScrollbarModeChanged(e);
  }

  #endregion Event Handlers

  #region Event Trigger Methods

  /// <summary>
  ///		Event fired internally when the read only state of the Editbox has been changed.
  /// </summary>
  /// <param name="e">Event arguments.</param>
  protected internal virtual void OnReadOnlyChanged(WindowEventArgs e) {
    if(ReadOnlyChanged != null) {
      ReadOnlyChanged(this, e);
    }
  }

  /// <summary>
  ///		Event fired internally when the validation string is changed.
  /// </summary>
  /// <param name="e">Event arguments.</param>
  protected internal virtual void OnValidationStringChanged(WindowEventArgs e) {
    if(ValidationStringChanged != null) {
      ValidationStringChanged(this, e);
    }
  }

  /// <summary>
  ///		Event fired internally when the maximum text length for the edit box is changed.
  /// </summary>
  /// <param name="e">Event arguments.</param>
  protected internal virtual void OnMaximumTextLengthChanged(WindowEventArgs e) {
    if(MaximumTextLengthChanged != null) {
      MaximumTextLengthChanged(this, e);
    }
  }

  /// <summary>
  ///		Event fired internally when something has caused the current text to now fail validation.
  /// </summary>
  /// <remarks>
  ///		This can be caused by changing the validation string or setting a maximum length that causes the
  ///		current text to be truncated.
  /// </remarks>
  /// <param name="e">Event arguments.</param>
  protected internal virtual void OnTextInvalidated(WindowEventArgs e) {
    if(TextInvalidated != null) {
      TextInvalidated(this, e);
    }
  }

  /// <summary>
  ///		Event fired internally when the user attempted to make a change to the edit box that would
  ///		have caused it to fail validation.
  /// </summary>
  /// <param name="e">Event arguments.</param>
  protected internal virtual void OnInvalidEntryAttempted(WindowEventArgs e) {
    if(InvalidEntryAttempted != null) {
      InvalidEntryAttempted(this, e);
    }
  }

  /// <summary>
  ///		Event fired internally when the carat (insert point) position changes.
  /// </summary>
  /// <param name="e">Event arguments.</param>
  protected internal virtual void OnCaratMoved(WindowEventArgs e) {
    if(CaratMoved != null) {
      CaratMoved(this, e);
    }
  }

  /// <summary>
  ///		Event fired internally when the current text selection changes.
  /// </summary>
  /// <param name="e">Event arguments.</param>
  protected internal virtual void OnTextSelectionChanged(WindowEventArgs e) {
    if(TextSelectionChanged != null) {
      TextSelectionChanged(this, e);
    }
  }

  /// <summary>
  ///		Event fired internally when the edit box text has reached the set maximum length.
  /// </summary>
  /// <param name="e">Event arguments.</param>
  protected internal virtual void OnEditboxFull(WindowEventArgs e) {
    if(EditboxFull != null) {
      EditboxFull(this, e);
    }
  }

  /// <summary>
  ///		Event fired internally when the user accepts the edit box text by pressing Return, Enter, or Tab.
  /// </summary>
  /// <param name="e">Event arguments.</param>
  protected internal virtual void OnTextAccepted(WindowEventArgs e) {
    if(TextAccepted != null) {
      TextAccepted(this, e);
    }
  }

  /// <summary>
  /// Triggered when the contents of the Listbox is changed.
  /// </summary>
  /// <param name="e"></param>
  protected internal void OnListContentsChanged(WindowEventArgs e) {
    if(ListContentsChanged != null) {
      ListContentsChanged(this, e);
    }
  }

  /// <summary>
  /// Triggered when the current selection within the Listbox changes.
  /// </summary>
  /// <param name="e"></param>
  protected internal void OnSelectionChanged(WindowEventArgs e) {
    if(SelectionChanged != null) {
      SelectionChanged(this, e);
    }
  }

  /// <summary>
  /// Triggered when the sort mode of the Listbox changes.
  /// </summary>
  /// <param name="e"></param>
  protected internal void OnSortModeChanged(WindowEventArgs e) {
    if(SortModeChanged != null) {
      SortModeChanged(this, e);
    }
  }

  /// <summary>
  /// Triggered when the 'always show' mode of the vertical scrollbar is changed.
  /// </summary>
  /// <param name="e"></param>
  protected internal void OnVerticalScrollbarModeChanged(WindowEventArgs e) {
    if(VerticalScrollbarModeChanged != null) {
      VerticalScrollbarModeChanged(this, e);
    }
  }

  /// <summary>
  /// Triggered when the 'always show' mode of the horizontal scrollbar is changed.
  /// </summary>
  /// <param name="e"></param>
  protected internal void OnHorizontalScrollbarModeChanged(WindowEventArgs e) {
    if(HorizontalScrollbarModeChanged != null) {
      HorizontalScrollbarModeChanged(this, e);
    }
  }

  /// <summary>
  ///		Handler called internally when the Combobox's drop-down list has been displayed.
  /// </summary>
  /// <param name="e"></param>
  protected internal void OnDropListDisplayed(WindowEventArgs e) {
    if(DropListDisplayed != null) {
      DropListDisplayed(this, e);
    }
  }

  /// <summary>
  ///		Handler called internally when the Combobox's drop-down list has been hidden.
  /// </summary>
  /// <param name="e"></param>
  protected internal void OnDropListRemoved(WindowEventArgs e) {
    if(DropListRemoved != null) {
      DropListRemoved(this, e);
    }
  }

  /// <summary>
  ///		Handler called internally when the user has confirmed a selection within the Combobox's drop-down list.
  /// </summary>
  /// <param name="e"></param>
  protected internal void OnListSelectionAccepted(WindowEventArgs e) {
    if(ListSelectionAccepted != null) {
      ListSelectionAccepted(this, e);
    }
  }

  #endregion Event Trigger Methods

  #region Overridden Trigger Methods

  /// <summary>
  ///		Handle widget font changing.
  /// </summary>
  /// <param name="e"></param>
  protected internal override void OnFontChanged(GuiEventArgs e) {
    // propogate to children
    editBox.SetFont(this.Font);
    dropList.SetFont(this.Font);

    // base class processing
    base.OnFontChanged(e);
  }

  /// <summary>
  ///		Handle the combobox being resized.
  /// </summary>
  /// <param name="e"></param>
  protected internal override void OnSized(GuiEventArgs e) {
    base.OnSized(e);

    LayoutComponentWidgets();

    e.Handled = true;
  }

  /// <summary>
  ///		Handle the text changing.
  /// </summary>
  /// <param name="e"></param>
  protected internal override void OnTextChanged(WindowEventArgs e) {
    // update ourselves only if needed (prevents perpetual event loop & stack overflow)
    if(editBox.Text != this.text) {
      // done before doing base class processing so event subscribers see 'updated' version of this
      editBox.Text = this.text;
      e.Handled = true;

      base.OnTextChanged(e);
    }
  }

  /// <summary>
  /// 
  /// </summary>
  /// <param name="e"></param>
  protected internal override void OnActivated(WindowEventArgs e) {
    if(!IsActive) {
      base.OnActivated(e);
      ActivateEditBox();
    }
  }

  #endregion Overridden Trigger Methods

  #endregion Events
}

} // namespace CeGui.Widgets
