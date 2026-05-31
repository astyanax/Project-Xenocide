using System;

using Gum.Forms;
using Gum.Forms.Controls;
using Gum.Wireframe;

using MonoGameGum;

using ProjectXenocide.Assets;
using ProjectXenocide.UI.Screens;

namespace ProjectXenocide.UI.Dialogs
{
    public abstract class GumDialog : Dialog
    {
        protected GraphicalUiElement GumRoot { get; private set; }

        public Dialog.ButtonAction CloseAction { get; set; }
        public Dialog.ButtonAction DismissAction { get; set; }
        public bool IsRequired { get; set; }

        protected StackPanel ContentPanel { get; private set; }

        protected GumDialog(string title) : base(new UiSize(0.5f, 0.3f))
        {
            Title = title;
        }

        protected GumDialog() : this("") { }

        public override void Show()
        {
            var display = LoadGumxLayout(GetGumxDialogName());
            if (display != null)
            {
                display.AddToRoot();
                GumRoot = display;
                WireGumControls();
            }
            else
            {
                ShowFallback();
            }
        }

        private static GraphicalUiElement LoadGumxLayout(string dialogName)
        {
            var project = Xenocide.GumProject;
            if (project == null)
                return null;

            var screenSave = project.Screens.Find(s => s.Name == dialogName);
            if (screenSave == null)
                return null;

            return screenSave.ToGraphicalUiElement();
        }

        protected virtual void ShowFallback()
        {
            var panel = new StackPanel();
            var vp = Xenocide.Instance.GraphicsDevice.Viewport;
            int x = (vp.Width - 500) / 2;
            int y = (vp.Height - 200) / 2;
            panel.Visual.X = x;
            panel.Visual.Y = y;
            panel.Visual.Width = 500;
            panel.Visual.Height = 200;

            panel.AddToRoot();
            GumRoot = panel.Visual;

            WireGumControls();
        }

        protected virtual string GetGumxDialogName() => GetType().Name;

        protected virtual void WireGumControls()
        {
            var closeBtn = GetButton("CloseButton");
            if (closeBtn != null)
                closeBtn.Click += (s, e) => Dismiss();

            WireClickSounds(GumRoot);
        }

        /// <summary>
        /// Creates or returns a ContentPanel StackPanel for dialogs that add
        /// dynamic widgets at runtime (e.g. aircraft lists, facility lists).
        /// Call this only if the dialog adds children programmatically.
        /// Dialogs with all content defined in the .gusx layout should NOT
        /// call this — the Forms wrapper would overlay the .gusx buttons.
        /// </summary>
        protected StackPanel GetOrCreateContentPanel()
        {
            if (ContentPanel != null) return ContentPanel;

            ContentPanel = new StackPanel();
            ContentPanel.Visual.Width = 0;
            ContentPanel.Visual.WidthUnits = Gum.DataTypes.DimensionUnitType.RelativeToParent;
            var contentElement = GetElement("ContentPanel");
            if (contentElement != null)
                contentElement.Children.Add(ContentPanel.Visual);

            return ContentPanel;
        }

        protected Button GetButton(string name)
        {
            return GumRoot?.GetFrameworkElementByName<Button>(name);
        }

        protected GraphicalUiElement GetElement(string name)
        {
            return GumRoot?.GetGraphicalUiElementByName(name);
        }

        protected void SetText(string name, string text)
        {
            GetElement(name)?.SetProperty("Text", text);
        }

        protected void Close()
        {
            CloseAction?.Invoke();
            ScreenManager.CloseDialog(this);
        }

        protected void Dismiss()
        {
            DismissAction?.Invoke();
            ScreenManager.CloseDialog(this);
        }

        public override bool HandleEscape()
        {
            Dismiss();
            return true;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (GumRoot != null)
                {
                    GumRoot.RemoveFromRoot();
                    GumRoot = null;
                }
            }
            base.Dispose(disposing);
        }

        private static void WireClickSounds(GraphicalUiElement root)
        {
            foreach (var child in root.Children)
            {
                if (child.Tag is Button button)
                    button.Click += OnAnyButtonClicked;
                WireClickSounds(child);
            }
        }

        private static void OnAnyButtonClicked(object sender, EventArgs e)
        {
            Xenocide.AudioSystem?.PlaySound(SoundId.ButtonClick1);
        }
    }
}
