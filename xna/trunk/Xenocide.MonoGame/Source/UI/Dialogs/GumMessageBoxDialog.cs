using System;

using Gum.Forms.Controls;

using Xenocide.Resources;

namespace ProjectXenocide.UI.Dialogs
{
    public class GumMessageBoxDialog : GumDialog
    {
        public GumMessageBoxDialog(string messageText)
            : this(messageText, Strings.DLG_MESSAGEBOX_TITLE)
        {
        }

        public GumMessageBoxDialog(string messageText, string title)
            : base(title)
        {
            _messageText = messageText;
        }

        protected override void WireGumControls()
        {
            base.WireGumControls();

            SetText("TitleLabel", Title);
            SetText("MessageLabel", _messageText);

            var okBtn = GetButton("OkButton");
            if (okBtn != null) okBtn.Click += (s, e) => Close();
        }

        public Dialog.ButtonAction OkAction
        {
            get => _okAction;
            set { _okAction = value; CloseAction = value; }
        }

        private Dialog.ButtonAction _okAction;
        private string _messageText;
    }
}
