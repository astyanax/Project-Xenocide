using System;
using System.Collections.Generic;
using System.Text;

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
        {
            this.pos = pos;
            this.isFirstBase = isFirstBase;
        }

        protected override void CreateDialogWidgets()
        {
            var prompt = new Label();
            prompt.Text = "Name your new base:";
            ContentArea.AddChild(prompt);

            baseNameInput = new Label();
            baseNameInput.Text = "New Base";
            ContentArea.AddChild(baseNameInput);

            var okBtn = new Button();
            okBtn.Text = Strings.BUTTON_OK;
            okBtn.Click += OnOkClicked;
            ContentArea.AddChild(okBtn);
        }

        private Label baseNameInput;

        public void OnOkClicked(object sender, EventArgs e)
        {
            string name = baseNameInput.Text;

            if (!IsNameLegal(name))
            {
                return;
            }

            Outpost outpost = new Outpost(pos, name);
            if (isFirstBase)
            {
                outpost.SetupPlayersFirstBase();
            }
            Xenocide.GameState.GeoData.Outposts.Add(outpost);

            BasesScreen basesScreen = new BasesScreen(
                Xenocide.GameState.GeoData.Outposts.Count - 1
            );
            if (!isFirstBase)
            {
                basesScreen.State = BasesScreen.BasesScreenState.AddAccessLift;
            }
            ScreenManager.ScheduleScreen(basesScreen);
            ScreenManager.CloseDialog(this);
        }

        private static bool IsNameLegal(String name)
        {
            if (String.IsNullOrEmpty(name))
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
