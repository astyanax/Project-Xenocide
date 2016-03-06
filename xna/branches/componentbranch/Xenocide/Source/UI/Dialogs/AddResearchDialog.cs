using System;
using System.Collections.Generic;
using System.Text;
using Xenocide.UI.Dialogs;
using CeGui;
using Xenocide.Resources;
using Xenocide.Model.Geoscape.Research;
using Xenocide.Utils;
using Microsoft.Xna.Framework;

namespace Xenocide.Source.UI.Dialogs
{
    class AddResearchDialog : Dialog
    {
        IResearchService researchService;

        public AddResearchDialog(Game game, ResearchDialog researchDialog)
            : base(game, new System.Drawing.SizeF(0.8f, 0.8f))
        {
            this.researchDialog = researchDialog;
        }

        #region CeGui

        private CeGui.Widgets.MultiColumnList grid;

        private CeGui.Widgets.PushButton okButton;
        private CeGui.Widgets.PushButton cancelButton;
        private CeGui.Widgets.EditBox scientistsBox;

        public override void Initialize()
        {
            researchService = (IResearchService)Game.Services.GetService(typeof(IResearchService));
            base.Initialize();
        }

        protected override void CreateCeguiWidgets()
        {
            CreateGrid();

            AddWidget(GuiBuilder.CreateText(Util.StringFormat(Strings.STR_SCIENTISTS_MAX, 50)), 0.01f, 0.75f, 0.3f, 0.07f);
            scientistsBox = GuiBuilder.CreateEditBox("daysBox");
            AddWidget(scientistsBox, 0.20f, 0.75f, 0.2f, 0.07f);

            okButton = AddButton("BUTTON_OK", 0.10f, 0.92f, 0.25f, 0.07f);
            cancelButton = AddButton("BUTTON_CANCEL", 0.36f, 0.92f, 0.25f, 0.07f);

            okButton.Clicked += new GuiEventHandler(OnOkButton);
            cancelButton.Clicked += new GuiEventHandler(OnCancelButton);
        }

        private void CreateGrid()
        {
            grid = GuiBuilder.CreateGrid("addResearchGrid");
            AddWidget(grid, 0.01f, 0.08f, 0.98f, 0.60f);
            grid.AddColumn(Strings.DLG_RESEARCH_COLUMN_PROJECT, grid.ColumnCount, 0.50f);

            AddResearchOptions();
        }

        private void AddResearchOptions()
        {
            int id = 0;
            foreach (ResearchTopic topic in researchService.ResearchableTopics)
            {
                string name = Util.LoadString(topic.Id, topic.Id);
                CeGui.Widgets.ListboxItem listboxItem = Util.CreateListboxItem(name);
                listboxItem.ID = id;
                listboxItem.UserData = topic;
                grid.AddRow(listboxItem, 0);

                ++id;
            }
        }

        private void OnOkButton(object sender, CeGui.GuiEventArgs e)
        {
            CeGui.Widgets.ListboxItem item = grid.GetFirstSelectedItem();

            if (item == null)
            {
                ScreenManager.ShowDialog(new MessageBoxDialog(Game, Strings.MSGBOX_NO_RESEARCHTOPIC_SELECTED));
            }
            else
            {
                try
                {
                    int scientists = int.Parse(scientistsBox.Text);
                    ResearchTopic topic = (ResearchTopic)item.UserData;
                    topic.Scientists = scientists;

                    ScreenManager.CloseDialog(this);

                    researchDialog.RefreshList();
                }
                catch (FormatException)
                {
                    ScreenManager.ShowDialog(new MessageBoxDialog(Game, Strings.MSGBOX_NO_SCIENTISTS_ADDED));
                }
            }
        }

        private void OnCancelButton(object sender, CeGui.GuiEventArgs e)
        {
            ScreenManager.CloseDialog(this);
        }

        #endregion

        private ResearchDialog researchDialog;
    }
}
