// NOTE(System.Drawing): The `using System.Drawing;` below exists solely to provide
// implicit conversion operators (Point↔PointF, Size↔SizeF) for backward compatibility.
// Once the real GUI library replaces this stub AND all callers use CeGui.Point/CeGui.Size
// directly, remove `using System.Drawing;` and the two operator pairs on lines 8-9.
// At that point this file will have zero System.Drawing dependency.
using System;
using System.Collections.Generic;
using System.Drawing;
using Microsoft.Xna.Framework;

namespace CeGui
{
    public struct Point { public float X; public float Y; public Point(float x, float y) { X = x; Y = y; } public static implicit operator Point(PointF p) { return new Point(p.X, p.Y); } public static implicit operator PointF(Point p) { return new PointF(p.X, p.Y); } }
    public struct Size { public float Width; public float Height; public Size(float w, float h) { Width = w; Height = h; } public static implicit operator Size(SizeF s) { return new Size(s.Width, s.Height); } public static implicit operator SizeF(Size s) { return new SizeF(s.Width, s.Height); } }
    public struct Rect { public float Left; public float Top; public float Right; public float Bottom; public float Width { get { return Right - Left; } } public float Height { get { return Bottom - Top; } } public Rect(float left, float top, float right, float bottom) { Left = left; Top = top; Right = right; Bottom = bottom; } }

    public delegate void WindowEventHandler(object sender, WindowEventArgs e);
    public delegate void MouseEventHandler(object sender, MouseEventArgs e);
    public delegate void GuiEventHandler(object sender, GuiEventArgs e);

    public enum HorizontalTextFormat { LeftAligned, RightAligned, HorzCentred, WordWrapLeft }
    public enum VerticalAlignment { Top, Bottom, Centre, TopAligned, BottomAligned, VertCentred }
    public enum VerticalTextFormat { Top, Bottom, Centre, TopAligned, BottomAligned, VertCentred }
    public enum MetricsMode { Absolute, Relative, Pixels }
    [Flags] public enum FontFlags { None = 0, Bold = 1, Italic = 2, Underline = 4 }

    public class GuiEventArgs : EventArgs { }
    public class WindowEventArgs : GuiEventArgs { public Window Window { get; set; } public WindowEventArgs(Window window) { Window = window; } }
    public class MouseEventArgs : GuiEventArgs { public float X { get; set; } public float Y { get; set; } public System.Windows.Forms.MouseButtons Button { get; set; } public Point Position { get; set; } public Point MoveDelta { get; set; } public float MoveDeltaX { get; set; } public float MoveDeltaY { get; set; } public int WheelDelta { get; set; } }
    public enum MouseButton { Left, Right, Middle, None }
    public class KeyEventArgs : GuiEventArgs { public KeyCodes Key { get; set; } public KeyCodes KeyCode { get; set; } }
    public enum KeyCodes { Escape, Return, Space, Tab, Backspace, Up, Down, Left, Right, A, B, C, D, E, F, G, H, I, J, K, L, M, N, O, P, Q, R, S, T, U, V, W, X, Y, Z, F1, F2, F3, F4, F5, F6, F7, F8, F9, F10, F11, F12, D0, D1, D2, D3, D4, D5, D6, D7, D8, D9 }

    public class GuiManager : GameComponent
    {
        public static GuiManager Instance { get; set; }
        public RootSheet Root { get; set; }
        public bool IsMouseDown { get; set; }
        public MouseCursor Cursor { get; set; }
        public GuiManager(Game game) : base(game) { Instance = this; }
        public void SetCursor(string name) { }
        public WindowSheet GetWindowSheet() { return null; }
        public void SetTooltipText(string text) { }
        public void SetTooltipText(Widget tooltipWidget) { }
        public void HandleMouseMove(MouseEventArgs e) { }
        public void HandleMouseButtonDown(MouseEventArgs e) { }
        public void HandleMouseButtonUp(MouseEventArgs e) { }
        public void HandleKeyDown(KeyEventArgs e) { }
        public void HandleKeyUp(KeyEventArgs e) { }
        public override void Update(GameTime gameTime) { }
        public void Render() { }
    }

    public class RootSheet { public float Width { get; set; } public float Height { get; set; } }
    public class MouseCursor { public string Name { get; set; } public Point HotSpot { get; set; } public void Load() { } }
    public class Window
    {
        public bool Visible { get; set; }
        public bool Enabled { get; set; }
        public bool Active { get; set; }
        public string Text { get; set; }
        public Size Size { get; set; }
        public string Name { get; set; }
        public Point Position { get; set; }
        public BitmapFont Font { get; set; }
        public MetricsMode MetricsMode { get; set; }
        public Size MinimumSize { get; set; }
        public event WindowEventHandler Closed;
        public event WindowEventHandler Activated;
        public event WindowEventHandler Deactivated;
        public event MouseEventHandler MouseButtonsDown;
        public event MouseEventHandler MouseMove;
        public virtual void Close() { }
        public virtual void Activate() { }
        public virtual void Deactivate() { }
        public virtual void Update() { }
        public Widget GetWidget(string name) { return null; }
        public T GetWidget<T>(string name) where T : Widget { return null; }
        public void AddChild(Window child) { }
        public void RemoveChild(Window child) { }
        public void Disable() { }
        public void Enable() { }
    }
    public class Widget : Window
    {
        public new bool Visible { get; set; }
        public new bool Enabled { get; set; }
        public new bool Active { get; set; }
        public new string Text { get; set; }
        public new string Name { get; set; }
        public string TooltipText { get; set; }
        public Window Parent { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
        public float XPosition { get; set; }
        public float YPosition { get; set; }
        public object UserData { get; set; }
        public event GuiEventHandler Clicked;
        public event WindowEventHandler SelectionChanged;
        public event GuiEventHandler TextChanged;
        public event MouseEventHandler MouseEnters;
        public event MouseEventHandler MouseLeaves;
        public new event MouseEventHandler MouseMove;
        public virtual void Render() { }
        public new virtual void Update() { }
        public virtual void Show() { Visible = true; }
        public virtual void Hide() { Visible = false; }
    }

    public class WindowSheet : Window { public Point MousePosition { get; set; } public GuiManager GuiManager { get; set; } }
    public class Image { public Texture Texture { get; set; } public Rect Area { get; set; } public static Image FromFile(string filename) { return null; } public static Image FromTexture(string name) { return null; } public static Image FromTexture(Texture texture) { return null; } public void LoadFromFile(string filename) { } }
    public class Texture { public float Width { get; set; } public float Height { get; set; } public float TexXOffset { get; set; } public float TexYOffset { get; set; } public float TexXScale { get; set; } public float TexYScale { get; set; } public void LoadFromFile(string filename) { } }
    public class Colors { public ColourValue R { get; set; } public ColourValue G { get; set; } public ColourValue B { get; set; } public ColourValue A { get; set; } public Colors(float r, float g, float b, float a) { } }
    public class ColourValue { public float Value { get; set; } public ColourValue(float value) { Value = value; } public static implicit operator ColourValue(float value) { return new ColourValue(value); } }
    public class Colour { public float R, G, B, A; public Colour() {} public Colour(float r, float g, float b) { R=r; G=g; B=b; A=1f; } public Colour(float r, float g, float b, float a) { R=r; G=g; B=b; A=a; } }

    public class Imageset { public string Name { get; set; } public Image GetImage(string name) { return null; } public static Imageset Load(string name, string filename) { return null; } public void DefineImage(string name, Rect rect, Point offset) { } public void DefineImage(string name, Rect rect, float offsetX, float offsetY) { } }
    public class FontManager { public static FontManager Instance { get; } public BitmapFont GetFont(string name) { return null; } public BitmapFont CreateFont(string name, string fontName, int size, FontFlags flags) { return null; } }
    public class BitmapFont { public string Name { get; set; } public float LineHeight { get; set; } public float BaseHeight { get; set; } }
    public class GuiSystem { public static GuiSystem Instance { get; } public GuiSheet GuiSheet { get; set; } public CeGui.Renderers.Renderer Renderer { get; set; } public void SetDefaultFont(string name) { } public void SetDefaultMouseCursor(Image image) { } }
    public class ImagesetManager { public static ImagesetManager Instance { get; } public Imageset GetImageset(string name) { return null; } public Imageset CreateImageset(string name) { return null; } public Imageset CreateImageset(string name, string filename) { return null; } public Imageset CreateImageset(string name, Texture texture) { return null; } public bool IsImagesetPresent(string name) { return false; } }
    public class WindowManager { public static WindowManager Instance { get; } public Window CreateWindow(string type, string name) { return null; } public Window GetWindow(string name) { return null; } public void DestroyWindow(string name) { } public void DestroyWindow(Window window) { } public Window LoadWindowLayout(string filename) { return null; } public Window LoadWindowLayout(string filename, string prefix) { return null; } public Window LoadWindowLayout(string filename, Window prefix) { return null; } public Window LoadWindowLayout(string filename, object prefix) { return null; } }
    public class Layout { public static Window LoadLayout(string filename) { return null; } }
    public class Scheme { public static Scheme Load(string name, string filename) { return null; } }
    public class AnimationManager { public static AnimationManager Instance { get; } public Animation GetAnimation(string name) { return null; } public void LoadAnimation(string name, string filename) { } }
    public class Animation { public string Name { get; set; } }
    public class AnimationInstance { public Animation Animation { get; set; } public bool Running { get; set; } public void Start() { Running = true; } public void Stop() { Running = false; } public void Pause() { } public void SetPosition(float pos) { } }
    public class Tooltip : Widget { }
    public class DragContainer : Widget { }
    public class GuiBuilder { public static GuiSheet BuildGuiSheet(string name) { return null; } public static Widgets.StaticText CreateText(string text) { return null; } public static Widgets.PushButton CreateButton(string id) { return null; } public static Widgets.Checkbox CreateCheckbox(string id) { return null; } public static Widgets.ComboBox CreateComboBox(string id) { return null; } public static Widgets.EditBox CreateEditBox(string id) { return null; } public static Widgets.FrameWindow CreateFrameWindow(string id) { return null; } public static Widgets.MultiColumnList CreateGrid(string id) { return null; } public static Widgets.StaticImage CreateImage(string id) { return null; } public static Widgets.Listbox CreateListBox(string id) { return null; } public static Widgets.Slider CreateSlider(string id) { return null; } }
    public class GuiEventAttribute : Attribute { public GuiEventAttribute() { } public GuiEventAttribute(string name) { } }
    public class GuiEvent : EventArgs { public string Name { get; set; } public GuiEvent(string name) { Name = name; } }
    public class ListboxTextItem : Widgets.ListboxItem, Widgets.IListboxItem { public ListboxTextItem(string text) { Text = text; } public void SetText(string text) { Text = text; } public void SetSelectionBrushImage(string imageset, string image) { } public new int ID { get; set; } public void SetTextColors(Colour colour) { } public void SetTextColors(ColourValue r, ColourValue g, ColourValue b) { } public void SetTextColors(ColourValue r, ColourValue g, ColourValue b, ColourValue a) { } }
    public class GuiSheet : Window { }
}

namespace CeGui.Widgets
{
    public interface IListboxItem { }
    public class ListboxItem : global::CeGui.Widget, IListboxItem { public bool Selected { get; set; } public int ID { get; set; } }
    public class MultiColumnList : global::CeGui.Widget { public int AddColumn(string caption, int width) { return 0; } public int AddColumn(string caption, int index, float width) { return 0; } public int ColumnCount { get; } public void SetColumnHeaderText(int column, string text) { } public int AddRow() { return 0; } public int AddRow(int columnCount) { return 0; } public int AddRow(ListboxItem item) { return 0; } public int AddRow(ListboxItem item, int column) { return 0; } public int AddRow(string text) { return 0; } public void InsertRow(int index, int columnCount) { } public void InsertRow(int index, ListboxItem item, int column) { } public void InsertRow(ListboxItem item, int index, int column) { } public void SetItem(int row, int column, string text) { } public void SetGridItem(int col, int row, ListboxItem item) { } public void SetUserData(int row, object data) { } public void SortByColumn(int column) { } public void ClearList() { } public int GetSelectedCount() { return 0; } public int GetFirstSelectedRow() { return 0; } public object GetUserData(int row) { return null; } public string GetItemText(int row, int column) { return null; } public int GetRowCount() { return 0; } public int RowCount { get; } public void ResetList() { } public ListboxItem GetFirstSelectedItem() { return null; } public int GetRowIndexOfItem(ListboxItem item) { return 0; } public ListboxItem GetItemAtGridReference(GridReference gridRef) { return null; } public void RemoveRow(int row) { } public void RemoveRow(ListboxItem item) { } public event MouseEventHandler MouseDoubleClicked; }
    public class ComboBox : global::CeGui.Widget { public void AddItem(string text) { } public void AddItems(string[] items) { } public int GetItemIndex(ListboxItem item) { return 0; } public ListboxItem SelectedItem { get; set; } public bool ReadOnly { get; set; } public void SetItemSelectState(int index, bool state) { } public void ResetList() { } public ListboxItem this[int index] { get { return null; } set { } } public event WindowEventHandler ListSelectionAccepted; }
    public class PushButton : global::CeGui.Widget { public int BackgroundColor { get; set; } public int TextColor { get; set; } public BitmapFont Font { get; set; } public void Disable() { } public void Enable() { } }
    public class StaticText : global::CeGui.Widget { public HorizontalTextFormat HorizontalFormat { get; set; } public BitmapFont Font { get; set; } public VerticalAlignment VerticalAlignment { get; set; } public VerticalTextFormat VerticalFormat { get; set; } public void SetTextColor(Colour colour) { } public void SetTextColor(ColourValue r, ColourValue g, ColourValue b) { } public void SetTextColor(ColourValue r, ColourValue g, ColourValue b, ColourValue a) { } }
    public class Slider : global::CeGui.Widget { public float CurrentValue { get; set; } public float MaxValue { get; set; } public float ClickStep { get; set; } public float Value { get; set; } public void Disable() { } public void Enable() { } }
    public class StaticImage : global::CeGui.Widget { public float AbsoluteHeight { get; set; } public float AbsoluteWidth { get; set; } public float AbsoluteX { get; set; } public float AbsoluteY { get; set; } public global::CeGui.Image Image { get; set; } public global::CeGui.Colors Colour { get; set; } public global::CeGui.Rect Rect { get; set; } public Rect AbsoluteToRelative(Rect r) { return r; } public Point AbsoluteToRelative(Point p) { return p; } public void AddChild(Window child) { } public event MouseEventHandler MouseButtonsDown; public event MouseEventHandler MouseClicked; public event MouseEventHandler MouseWheel; public void SetImage(global::CeGui.Image img) { } public void SetImage(string imageset, string image) { } }
    public class Checkbox : global::CeGui.Widget { public bool Selected { get; set; } public bool Checked { get; set; } }
    public class EditBox : global::CeGui.Widget { public int MaxTextLength { get; set; } public new string Text { get; set; } public BitmapFont Font { get; set; } public event WindowEventHandler TextAccepted; }
    public class Listbox : global::CeGui.Widget { public void AddItem(ListboxItem item) { } public ListboxItem AddItem(string text) { return null; } public void InsertItem(ListboxItem item, int index) { } public void InsertItem(ListboxItem item, ListboxItem after) { } public void RemoveItem(ListboxItem item) { } public void ResetList() { } public ListboxItem GetFirstSelectedItem() { return null; } public ListboxItem GetItemAt(int index) { return null; } public int GetItemCount() { return 0; } public void SortByText() { } }
    public class FrameWindow : global::CeGui.Widget { public bool MoveEnabled { get; set; } public bool CloseEnabled { get; set; } public bool RollUpEnabled { get; set; } public bool AlwaysOnTop { get; set; } }
    public class GridReference : global::CeGui.Widget { public int Column { get; set; } public int Row { get; set; } public GridReference(int column, int row) { Column = column; Row = row; } }
}

namespace CeGui.WidgetSets
{
    public class Taharez
    {
        public class TLGuiBuilder : global::CeGui.GuiBuilder { }
    }
}

namespace CeGui.Renderers
{
    public class Renderer { public static Renderer Instance { get; } public float Width { get; set; } public float Height { get; set; } public void Render() { } public global::CeGui.Texture CreateTexture() { return null; } public global::CeGui.Texture CreateTexture(string name, string filename, string area) { return null; } }
}

namespace CeGui.Renderers.Xna
{
    public class XnaRenderer { public static XnaRenderer Instance { get; } public void Render() { } }
}
