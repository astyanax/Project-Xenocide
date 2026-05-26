using System;

using Gum.Forms.Controls;

using Xenocide.Resources;

namespace ProjectXenocide.UI.Dialogs
{
    public class GumYesNoDialog : ModalDialog
    {
        public GumYesNoDialog(string messageText)
            : this(messageText, Strings.DLG_YESNO_TITLE, null, null)
        {
        }

        public GumYesNoDialog(string messageText, string title)
            : this(messageText, title, null, null)
        {
        }

        public GumYesNoDialog(string messageText, string yesButtonText, string noButtonText)
            : this(messageText, Strings.DLG_YESNO_TITLE, yesButtonText, noButtonText)
        {
        }

        public GumYesNoDialog(string messageText, string title, string yesButtonText, string noButtonText)
        {
            _messageText = messageText;
            Title = title;
            _yesButtonText = yesButtonText;
            _noButtonText = noButtonText;
        }

        public static GumYesNoDialog OkCancelDialog(string messageText)
        {
            return new GumYesNoDialog(messageText, Strings.BUTTON_OK, Strings.BUTTON_CANCEL);
        }

        public static GumYesNoDialog OkCancelDialog(string messageText, string title)
        {
            return new GumYesNoDialog(messageText, title, Strings.BUTTON_OK, Strings.BUTTON_CANCEL);
        }

        protected override void CreateDialogWidgets()
        {
            var messageLabel = new Label();
            messageLabel.Text = _messageText;
            ContentArea.AddChild(messageLabel);

            var yesButton = new Button();
            yesButton.Text = _yesButtonText ?? Strings.BUTTON_YES;
            yesButton.Click += (s, e) => Close();
            ContentArea.AddChild(yesButton);

            var noButton = new Button();
            noButton.Text = _noButtonText ?? Strings.BUTTON_NO;
            noButton.Click += (s, e) => Dismiss();
            ContentArea.AddChild(noButton);

            CloseAction = _yesAction;
        }

        public Dialog.ButtonAction YesAction
        {
            get => _yesAction;
            set { _yesAction = value; CloseAction = value; }
        }

        public Dialog.ButtonAction NoAction
        {
            get => _noAction;
            set { _noAction = value; DismissAction = value; }
        }

        private Dialog.ButtonAction _yesAction;
        private Dialog.ButtonAction _noAction;
        private string _messageText;
        private string _yesButtonText;
        private string _noButtonText;
    }
}
