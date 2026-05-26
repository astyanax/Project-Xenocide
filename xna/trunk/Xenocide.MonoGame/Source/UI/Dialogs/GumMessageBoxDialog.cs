using System;

using Gum.Forms.Controls;

using Xenocide.Resources;

namespace ProjectXenocide.UI.Dialogs
{
    /// <summary>
    /// A simple modal dialog with a message and an OK button.
    /// The OK button invokes the registered OkAction and closes the dialog.
    /// ESC dismisses without invoking the callback.
    /// </summary>
    public class GumMessageBoxDialog : ModalDialog
    {
        public GumMessageBoxDialog(string messageText)
            : this(messageText, Strings.DLG_MESSAGEBOX_TITLE)
        {
        }

        public GumMessageBoxDialog(string messageText, string title)
        {
            _messageText = messageText;
            Title = title;
        }

        protected override void CreateDialogWidgets()
        {
            var messageLabel = new Label();
            messageLabel.Text = _messageText;
            ContentArea.AddChild(messageLabel);

            var okButton = new Button();
            okButton.Text = Strings.BUTTON_OK;
            okButton.Click += (s, e) => Close();
            ContentArea.AddChild(okButton);

            CloseAction = _okAction;
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
