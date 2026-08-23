using System;

using Gum.Forms.Controls;

using ProjectXenocide.UI.Controls;

using Xenocide.Resources;

namespace ProjectXenocide.UI.Dialogs
{
    public class GumMessageBoxDialog : ModalDialog
    {
        public GumMessageBoxDialog(string messageText)
            : this(messageText, Strings.DLG_MESSAGEBOX_TITLE)
        {
        }

        public GumMessageBoxDialog(string messageText, string title)
            : base(title)
        {
            _messageText = messageText;
            PanelWidth = 500;
            PanelHeight = 200;
        }

        protected override void CreateDialogWidgets()
        {
            var messageLabel = ThemedLabel.CreateBody(_messageText);
            ContentArea.AddChild(messageLabel);

            AddButton(Strings.BUTTON_OK, (s, e) => Close());
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
