using System;

using ProjectXenocide.UI.Screens;
using ProjectXenocide.Utils;

namespace ProjectXenocide.UI.Dialogs
{
    public abstract class Dialog : Frame
    {
        protected Dialog(UiSize size)
            : base(MakeDialogId())
        {
        }

        protected Dialog(UiSize size, string title)
            : this(size)
        {
            this.title = title;
        }

        private static string MakeDialogId()
        {
            return Util.StringFormat("Dialog_{0}", ++dialogIdCounter);
        }

        public delegate void ButtonAction();

        private static int dialogIdCounter;
        private string title;
    }
}
