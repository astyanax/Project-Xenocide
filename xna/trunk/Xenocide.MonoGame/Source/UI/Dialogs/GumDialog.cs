using System;
using MonoGameGum;
using Gum.Forms.Controls;

namespace ProjectXenocide.UI.Dialogs
{
    public abstract class GumDialog : Dialog
    {
        protected StackPanel RootContainer { get; private set; }

        protected GumDialog() : base(new UiSize(0.5f, 0.3f))
        {
        }

        public override void Show()
        {
            RootContainer = new StackPanel();
            RootContainer.AddToRoot();
            CreateGumWidgets();
        }

        protected abstract void CreateGumWidgets();

        protected override void CreateCeguiWidgets()
        {
        }

        protected override CeGui.Window ConstructRootWidget()
        {
            return null;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && RootContainer != null)
            {
                GumService.Default.Root.Children.Remove(RootContainer.Visual);
                RootContainer = null;
            }
            base.Dispose(disposing);
        }
    }
}
