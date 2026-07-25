using System;

using Gum.Forms.Controls;

using ProjectXenocide.Model.Geoscape;
using ProjectXenocide.Model.Geoscape.Outposts;
using ProjectXenocide.UI.Controls;
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
            PanelHeight = 200;
        }

        protected override void CreateDialogWidgets()
        {
            var prompt = ThemedLabel.CreateBody(isFirstBase
                ? "Choose a name for your first base:"
                : "Name your new base:");
            ContentArea.AddChild(prompt);

            baseNameInput = new TextBox();
            baseNameInput.Text = "New Base";
            baseNameInput.Visual.Width = 560;
            baseNameInput.Visual.Height = 30;
            ContentArea.AddChild(baseNameInput);

            var buttonRow = new StackPanel();

            var okBtn = new Button();
            okBtn.Text = Strings.BUTTON_OK;
            okBtn.Visual.Width = 180;
            okBtn.Visual.Height = 30;
            okBtn.Click += OnOkClicked;
            buttonRow.AddChild(okBtn);

            var spacer = ThemedLabel.Create("");
            spacer.Visual.Width = 20;
            buttonRow.AddChild(spacer);

            var cancelBtn = new Button();
            cancelBtn.Text = Strings.BUTTON_CANCEL;
            cancelBtn.Visual.Width = 180;
            cancelBtn.Visual.Height = 30;
            cancelBtn.Click += (s, e) => Dismiss();
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
            Close();
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
                if (outpost.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
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
