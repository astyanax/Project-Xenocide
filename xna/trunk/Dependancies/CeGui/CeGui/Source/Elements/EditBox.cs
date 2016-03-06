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
using System.Text.RegularExpressions;
using System.Drawing;

namespace CeGui.Widgets {

/// <summary>
/// Summary description for EditBox.
/// </summary>
[ AlternateWidgetName("EditBox") ]
public abstract class EditBox : Window {
  #region Constants

  static readonly Colour DefaultNormalTextColor = new Colour(0x00FFFFFF);
  static readonly Colour DefaultSelectedTextColor = new Colour(0x00000000);
  static readonly Colour DefaultNormalSelectionColor = new Colour(0x006060FF);
  static readonly Colour DefaultInactiveSelectionColor = new Colour(0x00808080);

  #endregion Constants

  #region Fields

  /// <summary>
  ///		True if the editbox is in read-only mode.
  /// </summary>
  protected bool readOnly;
  /// <summary>
  ///		True if the editbox text should be rendered masked.
  /// </summary>
  protected bool maskText;
  /// <summary>
  ///		Code point to use when rendering masked text.
  /// </summary>
  protected char maskCodePoint;
  /// <summary>
  ///		Maximum number of characters for this Editbox.
  /// </summary>
  protected int maxTextLength;
  /// <summary>
  ///		Position of the carat / insert-point.
  /// </summary>
  protected int caratPos;
  /// <summary>
  ///		Start of selection area.
  /// </summary>
  protected int selectionStart;
  /// <summary>
  ///		End of selection area.
  /// </summary>
  protected int selectionEnd;
  /// <summary>
  ///		true when a selection is being dragged.
  /// </summary>
  protected bool dragging;
  /// <summary>
  ///		Selection index for drag selection anchor point.
  /// </summary>
  protected int dragAnchorIdx;
  /// <summary>
  ///		Used to perform validation against text entered as input.
  /// </summary>
  protected Regex validator;
  /// <summary>
  ///		Pattern to use for validating text.
  /// </summary>
  protected string validationPattern;
  /// <summary>
  ///		Text color used normally.
  /// </summary>
  protected Colour normalTextColor;
  /// <summary>
  ///		Text color used when text is highlighted.
  /// </summary>
  protected Colour selectTextColor;
  /// <summary>
  ///		Color to apply to the selection brush.
  /// </summary>
  protected Colour selectBrushColor;
  /// <summary>
  ///		Color to apply to the selection brush when widget is inactive / read-only.
  /// </summary>
  protected Colour inactiveSelectBrushColor;

  #endregion Fields

  #region Constructor

  /// <summary>
  ///		Constructor.
  /// </summary>
  /// <param name="name">Name for this widget.</param>
  protected EditBox(string type, string name)
    : base(type, name) {
    // default to accepting all characters
    this.ValidationString = ".*";

    // create the validator with the default options
    validator = new Regex(validationPattern);

    maskCodePoint = '*';
    maxTextLength = int.MaxValue;

    // default colors
    normalTextColor = DefaultNormalTextColor;
    selectTextColor = DefaultSelectedTextColor;
    selectBrushColor = DefaultNormalSelectionColor;
    inactiveSelectBrushColor = DefaultInactiveSelectionColor;

    text = "";
  }

  #endregion Constructor

  #region Window Members

  /// <summary>
  ///		
  /// </summary>
  /// <param name="z"></param>
  protected override void DrawSelf(float z) {
  }

  #endregion Window Members

  #region Abstract Members

  /// <summary>
  ///		Return the text code point index that is rendered closest to screen position <paramref cref="point"/>.
  /// </summary>
  /// <param name="point">Point object describing a position on the screen in pixels.</param>
  /// <returns>Code point index into the text that is rendered closest to screen position <paramref cref="point"/>.</returns>
  protected abstract int GetTextIndexFromPosition(PointF point);

  #endregion Abstract Members

  #region Base Members

  #region Properties

  /// <summary>
  ///		Get/Set the current position of the carat.
  /// </summary>
  /// <value>
  ///		Index of the insert carat relative to the start of the text.
  /// </value>
  public int CaratIndex {
    get {
      return caratPos;
    }
    set {
      // make sure the new position is valid
      if(value > text.Length) {
        value = text.Length;
      }

      // only bother if the new position is different
      if(caratPos != value) {
        caratPos = value;

        OnCaratMoved(new WindowEventArgs(this));
      }
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
      return IsActive;
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
      return IsStringValid(text);
    }
  }

  /// <summary>
  ///		Gets/Sets the code point used when rendering masked text.
  /// </summary>
  /// <value>
  ///		Code point value representing the Unicode code point that will be rendered instead of the Editbox text
  ///		when rendering in masked mode.
  /// </value>
  public char MaskCodePoint {
    get {
      return maskCodePoint;
    }
    set {
      if(value != maskCodePoint) {
        maskCodePoint = value;

        // event trigger
        OnMaskCodePointChanged(new WindowEventArgs(this));
      }
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
      return maxTextLength;
    }
    set {
      if(maxTextLength != value) {
        maxTextLength = value;

        // trigger max length changed event
        WindowEventArgs e = new WindowEventArgs(this);
        OnMaximumTextLengthChanged(e);

        // trim string
        if(text.Length > maxTextLength) {
          text = text.Substring(0, maxTextLength);
          OnTextChanged(e);

          // see if the new text is valid
          if(!IsTextValid) {
            OnTextInvalidated(e);
          }
        }
      }
    }
  }

  /// <summary>
  ///		Return the currently set color to be used for rendering the Editbox selection highlight
  ///		when the Editbox is active.
  /// </summary>
  public Colour NormalSelectBrushColor {
    get {
      return selectBrushColor;
    }
    set {
      selectBrushColor = value;
      RequestRedraw();
    }
  }

  /// <summary>
  ///		Return the currently set color to be used for rendering Editbox text in the normal, unselected state.
  /// </summary>
  /// <value>Color object representing the ARGB color that is currently set.</value>
  public Colour NormalTextColor {
    get {
      return normalTextColor;
    }
    set {
      normalTextColor = value;
      RequestRedraw();
    }
  }

  /// <summary>
  ///		Return the currently set color to be used for rendering Editbox text in selected region.
  /// </summary>
  /// <value>Color object representing the ARGB color that is currently set.</value>
  public Colour SelectedTextColor {
    get {
      return selectTextColor;
    }
    set {
      selectTextColor = value;
      RequestRedraw();
    }
  }

  public Colour InactiveSelectBrushColor {
    get {
      return inactiveSelectBrushColor;
    }
    set {
      inactiveSelectBrushColor = value;
      RequestRedraw();
    }
  }

  /// <summary>
  ///		Gets/Sets whether the text for the Editbox will be rendered masked.
  /// </summary>
  /// <value>
  ///		true if the Editbox text will be rendered masked using the currently set mask code point, false if the Editbox
  ///		text will be rendered as plain text.
  /// </value>
  public bool TextMasked {
    get {
      return maskText;
    }
    set {
      // only change if the setting is changed
      if(maskText != value) {
        maskText = value;
        OnMaskedRenderingModeChanged(new WindowEventArgs(this));
      }
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
      return readOnly;
    }
    set {
      // only change if the setting changed
      if(readOnly != value) {
        readOnly = value;
        OnReadOnlyChanged(new WindowEventArgs(this));
      }
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
      return (selectionStart != selectionEnd) ? selectionStart : caratPos;
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
      return (selectionStart != selectionEnd) ? selectionEnd : caratPos;
    }
  }

  /// <summary>
  ///		Return the length of the current selection (in code points / characters).
  /// </summary>
  /// <value>Number of code points (or characters) contained within the currently defined selection.</value>
  public int SelectionLength {
    get {
      return selectionEnd - selectionStart;
    }
  }

  /// <summary>
  ///		Get/Set the regular expression used for text validation.
  /// </summary>
  public string ValidationString {
    get {
      return validationPattern;
    }
    set {
      try {

        // recreate the regex
        validator = new Regex(value);

        validationPattern = value;
      }
      catch {
        throw new InvalidRequestException("The Editbox named {0} had the following bad validation expression set: {1}", this.Name, value);
      }

      // notification
      WindowEventArgs e = new WindowEventArgs(this);

      OnValidationStringChanged(e);

      if(!IsTextValid) {
        // also notify that the text is now invalid
        OnTextInvalidated(e);
      }
    }
  }

  #endregion Properties

  #region Methods

  /// <summary>
  ///		Clear the current selection setting.
  /// </summary>
  protected void ClearSelection() {
    // perform action only if required
    if(SelectionLength != 0) {
      SetSelection(0, 0);
    }
  }

  /// <summary>
  ///		Erase the currently selected text.
  /// </summary>
  /// <param name="modifyText">
  ///		When true, the actual text will be modified.  
  ///		When false, everything is done except erasing the characters.
  ///	</param>
  protected void EraseSelectedText(bool modifyText) {
    if(SelectionLength != 0) {
      // setup new carat position and remove selection highlight
      CaratIndex = SelectionStartIndex;
      ClearSelection();

      // erase the selected characters (if required)
      if(modifyText) {
        // remove the text
        text.Remove(SelectionStartIndex, SelectionLength);

        // trigger notifications that the text has changed
        OnTextChanged(new WindowEventArgs(this));
      }
    }
  }

  /// <summary>
  ///		Processing for the backspace key.
  /// </summary>
  protected void HandleBackspace() {
    if(!ReadOnly) {
      string tmp = text;

      if(SelectionLength != 0) {
        tmp = tmp.Remove(SelectionStartIndex, SelectionLength);

        if(IsStringValid(tmp)) {
          // erase selection using mode that does not modify 'text'
          EraseSelectedText(false);

          // set text to the newly modified string
          Text = tmp;
        } else {
          // trigger invalid modification attempted event
          OnInvalidEntryAttempted(new WindowEventArgs(this));
        }
      } else if(CaratIndex > 0) {
        tmp = tmp.Remove(caratPos - 1, 1);

        if(IsStringValid(tmp)) {
          CaratIndex = caratPos - 1;

          // set text to the newly modified string
          Text = tmp;
        } else {
          // trigger invalid modification attempted event
          OnInvalidEntryAttempted(new WindowEventArgs(this));
        }
      }
    }
  }

  /// <summary>
  ///		Processing for the delete key.
  /// </summary>
  protected void HandleDelete() {
    if(!ReadOnly) {
      string tmp = text;

      if(SelectionLength != 0) {
        tmp = tmp.Remove(SelectionStartIndex, SelectionLength);

        if(IsStringValid(tmp)) {
          // erase selection using mode that does not modify 'text'
          EraseSelectedText(false);

          // set text to the newly modified string
          Text = tmp;
        } else {
          // trigger invalid modification attempted event
          OnInvalidEntryAttempted(new WindowEventArgs(this));
        }
      } else if(CaratIndex < tmp.Length) {
        tmp = tmp.Remove(caratPos, 1);

        if(IsStringValid(tmp)) {
          // set text to the newly modified string
          Text = tmp;
        } else {
          // trigger invalid modification attempted event
          OnInvalidEntryAttempted(new WindowEventArgs(this));
        }
      }
    }

  }

  /// <summary>
  ///		Processing to move carat one character left.
  /// </summary>
  /// <param name="sysKeys">Current state of the system keys.</param>
  protected void HandleCharLeft(ModifierKeys sysKeys) {
    if(caratPos > 0) {
      CaratIndex = caratPos - 1;
    }

    if((sysKeys & ModifierKeys.Shift) > 0) {
      SetSelection(caratPos, dragAnchorIdx);
    } else {
      ClearSelection();
    }
  }

  /// <summary>
  ///		Processing to move carat one word left.
  /// </summary>
  /// <param name="sysKeys">Current state of the system keys.</param>
  protected void HandleWordLeft(ModifierKeys sysKeys) {
    if(caratPos > 0) {
      CaratIndex = TextUtil.GetWordStartIdx(text, CaratIndex);
    }

    if((sysKeys & ModifierKeys.Shift) > 0) {
      SetSelection(caratPos, dragAnchorIdx);
    } else {
      ClearSelection();
    }
  }

  /// <summary>
  ///		Processing to move carat one character right.
  /// </summary>
  /// <param name="sysKeys">Current state of the system keys.</param>
  protected void HandleCharRight(ModifierKeys sysKeys) {
    if(caratPos < text.Length) {
      CaratIndex = caratPos + 1;
    }

    if((sysKeys & ModifierKeys.Shift) > 0) {
      SetSelection(caratPos, dragAnchorIdx);
    } else {
      ClearSelection();
    }
  }

  /// <summary>
  ///		Processing to move carat one word right.
  /// </summary>
  /// <param name="sysKeys">Current state of the system keys.</param>
  protected void HandleWordRight(ModifierKeys sysKeys) {
    if(caratPos < text.Length) {
      CaratIndex = TextUtil.GetNextWordStartIdx(text, CaratIndex);
    }

    if((sysKeys & ModifierKeys.Shift) > 0) {
      SetSelection(caratPos, dragAnchorIdx);
    } else {
      ClearSelection();
    }
  }

  /// <summary>
  ///		Processing to move carat to the start of the text.
  /// </summary>
  /// <param name="sysKeys">Current state of the system keys.</param>
  protected void HandleHome(ModifierKeys sysKeys) {
    if(caratPos > 0) {
      CaratIndex = 0;
    }

    if((sysKeys & ModifierKeys.Shift) > 0) {
      SetSelection(caratPos, dragAnchorIdx);
    } else {
      ClearSelection();
    }
  }

  /// <summary>
  ///		Processing to move carat to the end of the text.
  /// </summary>
  /// <param name="sysKeys">Current state of the system keys.</param>
  protected void HandleEnd(ModifierKeys sysKeys) {
    if(caratPos < text.Length) {
      CaratIndex = text.Length;
    }

    if((sysKeys & ModifierKeys.Shift) > 0) {
      SetSelection(caratPos, dragAnchorIdx);
    } else {
      ClearSelection();
    }
  }

  /// <summary>
  ///		Using the current regex, the supplied text is validated.
  /// </summary>
  /// <param name="text">Text to validate.</param>
  /// <returns>True if the text is valid according to the validation string, false otherwise.</returns>
  protected bool IsStringValid(string text) {
    Match match = validator.Match(text);

    // only return true if the entire string matches the pattern
    return match.Success && (match.Index == 0) && (match.Length == text.Length);
  }

  /// <summary>
  ///		Define the current selection for the Editbox.
  /// </summary>
  /// <param name="startPos">
  ///		Index of the starting point for the selection.  If this value is greater than the number of characters in the Editbox, the
  ///		selection start will be set to the end of the text.
  /// </param>
  /// <param name="endPos">
  ///		Index of the ending point for the selection.  If this value is greater than the number of characters in the Editbox, the
  ///		selection end will be set to the end of the text.
  /// </param>
  public void SetSelection(int startPos, int endPos) {
    // ensure selection start point is within the valid range
    if(startPos > text.Length) {
      startPos = text.Length;
    }

    // ensure selection end point is within the valid range
    if(endPos > text.Length) {
      endPos = text.Length;
    }

    // swap values if start is after end
    if(startPos > endPos) {
      int tmp = endPos;
      endPos = startPos;
      startPos = tmp;
    }

    // only change state if values are different
    if((startPos != selectionStart) || endPos != selectionEnd) {
      // setup selection
      selectionStart = startPos;
      selectionEnd = endPos;

      // event trigger
      OnTextSelectionChanged(new WindowEventArgs(this));
    }
  }

  #endregion Methods

  #endregion Base Members

  #region Events

  #region Event Declarations

  /// <summary>
  ///		The read-only mode for the edit box has been changed.
  /// </summary>
  public event WindowEventHandler ReadOnlyChanged;
  /// <summary>
  ///		The masked rendering mode (password mode) has been changed.
  /// </summary>
  public event WindowEventHandler MaskedRenderingModeChanged;
  /// <summary>
  ///		The code point (character) to use for masked text has been changed.
  /// </summary>
  public event WindowEventHandler MaskCodePointChanged;
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

  #endregion Event Declarations

  #region Overridden Event Trigger Methods

  protected internal override void OnMouseButtonsDown(MouseEventArgs e) {
    // base class handling
    base.OnMouseButtonsDown(e);

    if(e.Button == System.Windows.Forms.MouseButtons.Left) {
      // grab inputs
      CaptureInput();

      // handle mouse down
      ClearSelection();
      dragging = true;
      dragAnchorIdx = GetTextIndexFromPosition(e.Position);
      CaratIndex = dragAnchorIdx;

      e.Handled = true;
    }
  }

  protected internal override void OnMouseButtonsUp(MouseEventArgs e) {
    // base class processing
    base.OnMouseButtonsUp(e);

    if(e.Button == System.Windows.Forms.MouseButtons.Left) {
      ReleaseInput();

      e.Handled = true;
    }
  }

  protected internal override void OnMouseDoubleClicked(MouseEventArgs e) {
    // base class processing
    base.OnMouseDoubleClicked(e);

    if(e.Button == System.Windows.Forms.MouseButtons.Left) {
      // if masked, set up to select all
      if(TextMasked) {
        dragAnchorIdx = 0;
        CaratIndex = text.Length;
      } else {
        // not masked, so select the word that was double clicked
        dragAnchorIdx = TextUtil.GetWordStartIdx(text, (caratPos == text.Length) ? caratPos : caratPos + 1);
        caratPos = TextUtil.GetNextWordStartIdx(text, caratPos);
      }

      // perform actual selection operation
      SetSelection(dragAnchorIdx, caratPos);

      e.Handled = true;
    }
  }

  protected internal override void OnMouseTripleClicked(MouseEventArgs e) {
    // base class processing
    base.OnMouseTripleClicked(e);

    if(e.Button == System.Windows.Forms.MouseButtons.Left) {
      dragAnchorIdx = 0;
      CaratIndex = text.Length;
      SetSelection(dragAnchorIdx, caratPos);
      e.Handled = true;
    }
  }

  protected internal override void OnMouseMove(MouseEventArgs e) {
    // base class processing
    base.OnMouseMove(e);

    if(dragging) {
      CaratIndex = GetTextIndexFromPosition(e.Position);
      SetSelection(caratPos, dragAnchorIdx);
    }

    e.Handled = true;
  }

  protected internal override void OnCaptureLost(GuiEventArgs e) {
    dragging = false;

    // base class processing
    base.OnCaptureLost(e);

    e.Handled = true;
  }

  protected internal override void OnCharacter(KeyEventArgs e) {
    // base class processing
    base.OnCharacter(e);

    // only need to take notice if we have focus
    if(HasInputFocus && this.Font.IsCharacterAvailable(e.Character) && !ReadOnly) {
      // backup current text
      string tmp = text;

      tmp = tmp.Remove(SelectionStartIndex, SelectionLength);

      // if there is room
      if(tmp.Length < maxTextLength) {
        tmp = tmp.Insert(SelectionStartIndex, e.Character.ToString());

        if(IsStringValid(tmp)) {
          // erase selection using mode that does not modify 'text' (we just want to update state)
          EraseSelectedText(false);

          // set text to the newly modified string
          Text = tmp;

          // advance carat
          caratPos++;
        } else {
          // trigger invalid modification attempted event
          OnInvalidEntryAttempted(new WindowEventArgs(this));
        }
      } else {
        // trigger text box full event
        OnEditboxFull(new WindowEventArgs(this));
      }
    }

    e.Handled = true;
  }

  protected internal override void OnKeyDown(KeyEventArgs e) {
    // base class processing
    base.OnKeyDown(e);

    if(HasInputFocus && !ReadOnly) {
      WindowEventArgs args = new WindowEventArgs(this);

      switch(e.KeyCode) {
        case System.Windows.Forms.Keys.Shift:
          if(SelectionLength == 0) {
            dragAnchorIdx = CaratIndex;
          }
          break;

        case System.Windows.Forms.Keys.Back:
          HandleBackspace();
          break;

        case System.Windows.Forms.Keys.Delete:
          HandleDelete();
          break;

        case System.Windows.Forms.Keys.Tab:
        case System.Windows.Forms.Keys.Return:
          // fire input accepted event
          OnTextAccepted(args);
          break;

        case System.Windows.Forms.Keys.Left:
          if((e.Modifiers & ModifierKeys.Control) > 0) {
            HandleWordLeft(e.Modifiers);
          } else {
            HandleCharLeft(e.Modifiers);
          }
          break;

        case System.Windows.Forms.Keys.Right:
          if((e.Modifiers & ModifierKeys.Control) > 0) {
            HandleWordRight(e.Modifiers);
          } else {
            HandleCharRight(e.Modifiers);
          }
          break;

        case System.Windows.Forms.Keys.Home:
          HandleHome(e.Modifiers);
          break;

        case System.Windows.Forms.Keys.End:
          HandleEnd(e.Modifiers);
          break;
      } // switch

      e.Handled = true;
    }
  }

  protected internal override void OnTextChanged(WindowEventArgs e) {
    // base class processing
    base.OnTextChanged(e);

    // clear selection
    ClearSelection();

    // make sure carat is within the text
    if(CaratIndex > text.Length) {
      CaratIndex = text.Length;
    }

    e.Handled = true;
  }

  #endregion Overridden Event Trigger Methods

  #region Event Trigger Methods

  /// <summary>
  ///		Event fired internally when the read only state of the Editbox has been changed.
  /// </summary>
  /// <param name="e">Event arguments.</param>
  protected internal virtual void OnReadOnlyChanged(WindowEventArgs e) {
    RequestRedraw();

    if(ReadOnlyChanged != null) {
      ReadOnlyChanged(this, e);
    }
  }

  /// <summary>
  ///		Event fired internally when the masked rendering mode (password mode) has been changed
  /// </summary>
  /// <param name="e">Event arguments.</param>
  protected internal virtual void OnMaskedRenderingModeChanged(WindowEventArgs e) {
    RequestRedraw();

    if(MaskedRenderingModeChanged != null) {
      MaskedRenderingModeChanged(this, e);
    }
  }

  /// <summary>
  ///		Event fired internally when the code point to use for masked rendering has been changed.
  /// </summary>
  /// <param name="e">Event arguments.</param>
  protected internal virtual void OnMaskCodePointChanged(WindowEventArgs e) {
    // if we are in masked mode, trigger a GUI redraw
    if(TextMasked) {
      RequestRedraw();
    }

    if(MaskCodePointChanged != null) {
      MaskCodePointChanged(this, e);
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
    RequestRedraw();

    if(CaratMoved != null) {
      CaratMoved(this, e);
    }
  }

  /// <summary>
  ///		Event fired internally when the current text selection changes.
  /// </summary>
  /// <param name="e">Event arguments.</param>
  protected internal virtual void OnTextSelectionChanged(WindowEventArgs e) {
    RequestRedraw();

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

  #endregion Event Trigger Methods

  #endregion Events
}

} // namespace CeGui.Widgets
