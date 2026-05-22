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
        {
            this.messageText = messageText;
        }

        protected override void CreateGumWidgets()
        {
            var messageLabel = new Label();
            messageLabel.Text = messageText;
            RootContainer.AddChild(messageLabel);

            var okButton = new Button();
            okButton.Text = Strings.BUTTON_OK;
            okButton.Click += OnOkClicked;
            RootContainer.AddChild(okButton);
        }

        private void OnOkClicked(object sender, EventArgs e)
        {
            ScreenManager.CloseDialog(this);
            okAction?.Invoke();
        }

        public ButtonAction OkAction { get => okAction; set => okAction = value; }

        private ButtonAction okAction;
        private string messageText;
    }
}
