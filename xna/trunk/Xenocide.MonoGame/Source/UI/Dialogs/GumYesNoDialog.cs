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
            : base(title)
        {
            _messageText = messageText;
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

        protected override void WireGumControls()
        {
            base.WireGumControls();

            SetText("TitleLabel", Title);
            SetText("MessageLabel", _messageText);

            var yesBtn = GetButton("YesButton");
            if (yesBtn != null)
            {
                yesBtn.Text = _yesButtonText ?? Strings.BUTTON_YES;
                yesBtn.Click += (s, e) => Close();
            }

            var noBtn = GetButton("NoButton");
            if (noBtn != null)
            {
                noBtn.Text = _noButtonText ?? Strings.BUTTON_NO;
                noBtn.Click += (s, e) => Dismiss();
            }

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
