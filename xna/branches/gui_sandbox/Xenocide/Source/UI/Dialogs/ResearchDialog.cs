using System;
using System.Collections.Generic;
using System.Text;
using Xenocide.UI.Dialogs;
using CeGui;
using Xenocide.Resources;
using Xenocide.Utils;
using Xenocide.Model.Geoscape.Research;

namespace Xenocide.Source.UI.Dialogs
{
    class ResearchDialog : Dialog
    {
        public ResearchDialog()
            : base(new System.Drawing.SizeF(0.8f, 0.8f))
        {

        }

        #region CeGui
        
        private CeGui.Widgets.MultiColumnList grid;

        private CeGui.Widgets.PushButton addButton;
        private CeGui.Widgets.PushButton deleteButton;
        private CeGui.Widgets.PushButton closeButton;
        
        protected override void CreateCeguiWidgets()
        {
            CreateGrid();

            addButton = AddButton("BUTTON_ADDPROJECT", 0.10f, 0.92f, 0.25f, 0.07f);
            deleteButton = AddButton("BUTTON_CANCELPROJECT", 0.36f, 0.92f, 0.25f, 0.07f);
            closeButton = AddButton("BUTTON_CLOSE", 0.62f, 0.92f, 0.25f, 0.07f);

            addButton.Clicked += new GuiEventHandler(OnAddButton);
            closeButton.Clicked += new GuiEventHandler(OnCloseButton);
        }

        private void CreateGrid()
        {
            grid = GuiBuilder.CreateGrid("researchGrid");
            AddWidget(grid, 0.01f, 0.08f, 0.98f, 0.80f);
            grid.AddColumn(Strings.DLG_RESEARCH_COLUMN_PROJECT, grid.ColumnCount, 0.50f);
            grid.AddColumn(Strings.DLG_RESEARCH_COLUMN_SCIENTISTS, grid.ColumnCount, 0.25f);
            grid.AddColumn(Strings.DLG_RESEARCH_COLUMN_DAYSLEFT, grid.ColumnCount, 0.22f);

            AddResearchProjects();
        }

        private void AddResearchProjects()
        {
            int id = 0;
            foreach(ResearchTopic topic in Xenocide.GameState.GeoData.ResearchGraph.CurrentResearchProjects)
            {
                string name = Util.LoadString(topic.Id, topic.Id);
                CeGui.Widgets.ListboxItem listboxItem = Util.CreateListboxItem(name);
                listboxItem.ID = id;
                listboxItem.UserData = topic;
                grid.AddRow(listboxItem, 0);
                grid.SetGridItem(1, id, Util.CreateListboxItem(topic.Scientists.ToString()));
                grid.SetGridItem(2, id, Util.CreateListboxItem(topic.DaysLeft.ToString()));

                ++id;
            }
        }

        public void RefreshList()
        {
            while(grid.RowCount > 0)
                grid.RemoveRow(0);

            AddResearchProjects();
        }

        #endregion

        #region EventHandlers

        private void OnAddButton(object sender, CeGui.GuiEventArgs e)
        {
            ScreenManager.ShowDialog(new AddResearchDialog(this));
        }

        private void OnCloseButton(object sender, CeGui.GuiEventArgs e)
        {
            ScreenManager.CloseDialog(this);
        }

        #endregion
    }
}
