using System;

using Gum.Forms.Controls;

using ProjectXenocide.Model.Geoscape;
using ProjectXenocide.Model.Geoscape.Outposts;
using ProjectXenocide.UI.Screens;
using ProjectXenocide.Utils;

using Xenocide.Resources;

namespace ProjectXenocide.UI.Dialogs
{
    public class NameNewBaseDialog : ModalDialog
    {
        public NameNewBaseDialog(GeoPosition pos, bool isFirstBase)
            : base("Name New Base")
        {
            this.pos = pos;
            this.isFirstBase = isFirstBase;
            PanelWidth = 600;
            PanelHeight = 280;
        }

        protected override void CreateDialogWidgets()
        {
            var spacerTop = new Label();
            spacerTop.Height = 10;
            ContentArea.AddChild(spacerTop);

            var prompt = new Label();
            prompt.Text = isFirstBase
                ? "Choose a name for your first base:"
                : "Name your new base:";
            prompt.Visual.Width = 560;
            ContentArea.AddChild(prompt);

            var spacer1 = new Label();
            spacer1.Height = 10;
            ContentArea.AddChild(spacer1);

            baseNameInput = new TextBox();
            baseNameInput.Text = "New Base";
            baseNameInput.Visual.Width = 560;
            baseNameInput.Visual.Height = 30;
            ContentArea.AddChild(baseNameInput);

            var spacer2 = new Label();
            spacer2.Height = 20;
            ContentArea.AddChild(spacer2);

            var buttonRow = new StackPanel();
            buttonRow.Visual.Width = 560;
            buttonRow.Visual.Height = 40;

            var okBtn = new Button();
            okBtn.Text = Strings.BUTTON_OK;
            okBtn.Visual.Width = 180;
            okBtn.Click += OnOkClicked;
            buttonRow.AddChild(okBtn);

            var spacerBtn = new Label();
            spacerBtn.Visual.Width = 20;
            buttonRow.AddChild(spacerBtn);

            var cancelBtn = new Button();
            cancelBtn.Text = Strings.BUTTON_CANCEL;
            cancelBtn.Visual.Width = 180;
            cancelBtn.Click += (s, e) => ScreenManager.CloseDialog(this);
            buttonRow.AddChild(cancelBtn);

            ContentArea.AddChild(buttonRow);
        }

        private TextBox baseNameInput;

        public void OnOkClicked(object sender, EventArgs e)
        {
            string name = baseNameInput.Text?.Trim();

            if (string.IsNullOrEmpty(name))
                name = "New Base";

            if (!IsNameLegal(name))
                return;

            Outpost outpost = new Outpost(pos, name);
            if (isFirstBase)
                outpost.SetupPlayersFirstBase();

            Xenocide.GameState.GeoData.Outposts.Add(outpost);

            BasesScreen basesScreen = new BasesScreen(
                Xenocide.GameState.GeoData.Outposts.Count - 1
            );
            if (!isFirstBase)
                basesScreen.State = BasesScreen.BasesScreenState.AddAccessLift;

            ScreenManager.ScheduleScreen(basesScreen);
            ScreenManager.CloseDialog(this);
        }

        private static bool IsNameLegal(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                Util.ShowMessageBox(Strings.MSGBOX_BASE_NEEDS_NAME);
                return false;
            }

            foreach (Outpost outpost in Xenocide.GameState.GeoData.Outposts)
            {
                if (outpost.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase))
                {
                    Util.ShowMessageBox(Strings.MSGBOX_BASE_NAMES_ARE_UNIQUE, name);
                    return false;
                }
            }

            return true;
        }

        private GeoPosition pos;
        private bool isFirstBase;
    }
}
