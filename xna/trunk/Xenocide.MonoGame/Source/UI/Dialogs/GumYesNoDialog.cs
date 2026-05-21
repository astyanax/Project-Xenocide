using System;
using Gum.Forms.Controls;
using Xenocide.Resources;

namespace ProjectXenocide.UI.Dialogs
{
    public class GumYesNoDialog : GumDialog
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
            this.messageText = messageText;
            this.yesButtonText = yesButtonText;
            this.noButtonText = noButtonText;
        }

        public static GumYesNoDialog OkCancelDialog(string messageText)
        {
            return new GumYesNoDialog(messageText, Strings.BUTTON_OK, Strings.BUTTON_CANCEL);
        }

        public static GumYesNoDialog OkCancelDialog(string messageText, string title)
        {
            return new GumYesNoDialog(messageText, title, Strings.BUTTON_OK, Strings.BUTTON_CANCEL);
        }

        protected override void CreateGumWidgets()
        {
            var messageLabel = new Label();
            messageLabel.Text = messageText;
            RootContainer.AddChild(messageLabel);

            var yesButton = new Button();
            yesButton.Text = yesButtonText ?? Strings.BUTTON_YES;
            yesButton.Click += OnYesClicked;
            RootContainer.AddChild(yesButton);

            var noButton = new Button();
            noButton.Text = noButtonText ?? Strings.BUTTON_NO;
            noButton.Click += OnNoClicked;
            RootContainer.AddChild(noButton);
        }

        private void OnYesClicked(object sender, EventArgs e)
        {
            yesAction?.Invoke();
            ScreenManager.CloseDialog(this);
        }

        private void OnNoClicked(object sender, EventArgs e)
        {
            noAction?.Invoke();
            ScreenManager.CloseDialog(this);
        }

        public ButtonAction YesAction { get => yesAction; set => yesAction = value; }
        public ButtonAction NoAction { get => noAction; set => noAction = value; }

        private ButtonAction yesAction;
        private ButtonAction noAction;
        private string messageText;
        private string yesButtonText;
        private string noButtonText;
    }
}
