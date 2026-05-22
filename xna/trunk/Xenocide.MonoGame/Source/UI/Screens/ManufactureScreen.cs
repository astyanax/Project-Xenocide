#region Copyright
/*
--------------------------------------------------------------------------------
This source file is part of Xenocide
  by  Project Xenocide Team

For the latest info on Xenocide, see http://www.projectxenocide.com/

This work is licensed under the Creative Commons
Attribution-NonCommercial-ShareAlike 2.5 License.

To view a copy of this license, visit
http://creativecommons.org/licenses/by-nc-sa/2.5/
or send a letter to Creative Commons, 543 Howard Street, 5th Floor,
San Francisco, California, 94105, USA.
--------------------------------------------------------------------------------
*/

/*
* @file ManufactureScreen.cs
* @date Created: 2007/10/07
* @author File creator: David Teviotdale
* @author Credits: none
*/
#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

using Gum.Forms.Controls;

using ProjectXenocide.Model;
using ProjectXenocide.Model.Geoscape;
using ProjectXenocide.Model.Geoscape.Outposts;
using ProjectXenocide.Model.StaticData;
using ProjectXenocide.Model.StaticData.Items;
using ProjectXenocide.Model.StaticData.Research;
using ProjectXenocide.UI.Controls;
using ProjectXenocide.Utils;

using Xenocide.Resources;


#endregion

namespace ProjectXenocide.UI.Screens
{
    /// <summary>
    /// This is the screen where player sets the items an outpost is manufacturing
    /// </summary>
    public class ManufactureScreen : GumScreen
    {
        /// <summary>
        /// Constructor (obviously)
        /// </summary>
        /// <param name="selectedOutpostIndex">Index to outpost screen is to show</param>
        public ManufactureScreen(int selectedOutpostIndex)
            : base("Manufacture")
        {
            this.selectedOutpostIndex = selectedOutpostIndex;
        }

        #region Create the Gum controls

        protected override void CreateGumControls()
        {
            ProjectMgr.Update();

            FindIdleEngineers();

            availableText = new Label() { Text = MakeIdleEngineersString() };
            RootContainer.AddChild(availableText);

            InitializeGrids();
            PopulateProjectGrid();

            buildMoreButton = new Button() { Text = XenocideResourceManager.Get("BUTTON_BUILD_MORE") };
            RootContainer.AddChild(buildMoreButton);
            buildLessButton = new Button() { Text = XenocideResourceManager.Get("BUTTON_BUILD_LESS") };
            RootContainer.AddChild(buildLessButton);
            cancelBuildButton = new Button() { Text = XenocideResourceManager.Get("BUTTON_CANCEL_BUILD") };
            RootContainer.AddChild(cancelBuildButton);
            addIdleEngineersButton = new Button() { Text = XenocideResourceManager.Get("BUTTON_ADD_IDLE_ENGINEERS") };
            RootContainer.AddChild(addIdleEngineersButton);
            moreEngineersButton = new Button() { Text = XenocideResourceManager.Get("BUTTON_MORE_ENGINEERS") };
            RootContainer.AddChild(moreEngineersButton);
            lessEngineersButton = new Button() { Text = XenocideResourceManager.Get("BUTTON_LESS_ENGINEERS") };
            RootContainer.AddChild(lessEngineersButton);
            removeAllEngineersButton = new Button() { Text = XenocideResourceManager.Get("BUTTON_REMOVE_ALL_ENGINEERS") };
            RootContainer.AddChild(removeAllEngineersButton);
            closeButton = new Button() { Text = XenocideResourceManager.Get("BUTTON_CLOSE") };
            RootContainer.AddChild(closeButton);

            buildMoreButton.Click += OnBuildMoreButton;
            buildLessButton.Click += OnBuildLessButton;
            cancelBuildButton.Click += OnCancelBuildButton;
            moreEngineersButton.Click += OnMoreButton;
            lessEngineersButton.Click += OnLessButton;
            addIdleEngineersButton.Click += OnAddIdleButton;
            removeAllEngineersButton.Click += OnRemoveAllButton;
            closeButton.Click += OnCloseButton;
        }

        private Label availableText;
        private GridPanel projectGrid;
        private GridPanel requirementsGrid;

        private Button buildMoreButton;
        private Button buildLessButton;
        private Button cancelBuildButton;
        private Button moreEngineersButton;
        private Button lessEngineersButton;
        private Button removeAllEngineersButton;
        private Button addIdleEngineersButton;
        private Button closeButton;

        private void InitializeGrids()
        {
            projectGrid = new GridPanel();
            projectGrid.AddColumn(Strings.SCREEN_MANUFACTURE_COLUMN_PROJECT, 350);
            projectGrid.AddColumn(Strings.SCREEN_MANUFACTURE_COLUMN_ENGINEERS, 105);
            projectGrid.AddColumn(Strings.SCREEN_MANUFACTURE_COLUMN_BUILD_QUANTITY, 105);
            projectGrid.AddColumn(Strings.SCREEN_MANUFACTURE_COLUMN_ETA, 105);
            RootContainer.AddChild(projectGrid.Visual);
            projectGrid.SelectionChanged += OnProjectGridSelectionChanged;

            requirementsGrid = new GridPanel();
            requirementsGrid.AddColumn(Strings.SCREEN_MANUFACTURE_COLUMN_RESOURCE, 350);
            requirementsGrid.AddColumn(Strings.SCREEN_MANUFACTURE_COLUMN_QUANTITY_NEEDED, 160);
            requirementsGrid.AddColumn(Strings.SCREEN_MANUFACTURE_COLUMN_QUANTITY_AVAILABLE, 175);
            RootContainer.AddChild(requirementsGrid.Visual);
        }

        private void PopulateProjectGrid()
        {
            foreach (BuildProject project in ProjectMgr)
            {
                AddRowToProjectGrid(new ProjectLineItem(this, project));
            }

            foreach (ItemInfo item in Xenocide.StaticTables.ItemList)
            {
                if ((null != item.BuildInfo) && TechMgr.IsAvailable(item.Id) && !ProjectMgr.IsInProgress(item.Id))
                {
                    AddRowToProjectGrid(new IdleLineItem(this, item));
                }
            }

            if (0 == projectGrid.RowCount)
            {
                Util.ShowMessageBox(Strings.MSGBOX_NO_BUILDABLE_TECHNOLOGIES);
            }
        }

        private int AddRowToProjectGrid(LineItem lineItem)
        {
            return projectGrid.AddRow(lineItem, lineItem.Name, lineItem.DisplayNumWorkers, lineItem.DisplayQuantity, lineItem.Eta);
        }

        private void ShowRequirements(BuildInfo buildInfo)
        {
            requirementsGrid.Clear();
            if (0 < buildInfo.Space)
            {
                string needed = Util.ToString(buildInfo.Space);
                string available = Util.ToString((int)buildInfo.GetCapacityInfo(SelectedOutpost).Available);
                AddRowToRequirementsGrid(Strings.SCREEN_MANUFACTURE_REPORT_ROW_WORKSPACE, needed, available);
            }

            if (0 < buildInfo.Hours)
            {
                string needed = Util.ToString(buildInfo.Hours);
                string available = String.Empty;
                AddRowToRequirementsGrid(Strings.SCREEN_MANUFACTURE_REPORT_ROW_HOURS, needed, available);
            }

            if (0 < buildInfo.Dollars)
            {
                string needed = Util.FormatCurrency(buildInfo.Dollars);
                string available = Xenocide.GameState.GeoData.XCorp.Bank.DisplayCurrentBalance;
                AddRowToRequirementsGrid(Strings.SCREEN_MANUFACTURE_REPORT_ROW_MONEY, needed, available);
            }

            foreach (ItemLine material in buildInfo.Materials)
            {
                string needed = Util.ToString(material.Quantity);
                string available = Util.ToString(SelectedOutpost.Inventory.NumberInInventory(material.ItemInfo));
                AddRowToRequirementsGrid(material.ItemInfo.Name, needed, available);
            }
        }

        private void AddRowToRequirementsGrid(string resourceName, string needed, string available)
        {
            requirementsGrid.AddRow(null, resourceName, needed, available);
        }

        #endregion Create the Gum controls

        #region event handlers

        private void OnBuildMoreButton(object sender, EventArgs e)
        {
            ChangeBuildNumber(1);
        }

        private void OnBuildLessButton(object sender, EventArgs e)
        {
            ChangeBuildNumber(-1);
        }

        private void OnCancelBuildButton(object sender, EventArgs e)
        {
            CancelProject();
        }

        private void OnMoreButton(object sender, EventArgs e)
        {
            AddIdleEngineers(1);
        }

        private void OnAddIdleButton(object sender, EventArgs e)
        {
            AddIdleEngineers(idleEngineers.Count);
        }

        private void OnRemoveAllButton(object sender, EventArgs e)
        {
            RemoveAllEngineers();
        }

        private void OnLessButton(object sender, EventArgs e)
        {
            RemoveScientist();
        }

        private void OnCloseButton(object sender, EventArgs e)
        {
            ShowBasesScreen();
        }

        private void OnProjectGridSelectionChanged(object sender, EventArgs e)
        {
            LineItem lineItem = GetSelectedItem();
            if (lineItem != null)
            {
                ShowRequirements(lineItem.BuildInfo);
            }
        }

        #endregion event handlers

        private void AddIdleEngineers(int count)
        {
            LineItem selectedLineItem = GetSelectedItem();
            if (null == selectedLineItem)
                return;

            if (0 < idleEngineers.Count)
            {
                Debug.Assert((0 < count) && (count <= idleEngineers.Count));
                ProjectLineItem project = selectedLineItem.GetProject();
                if (null != project)
                {
                    int oldRow = projectGrid.GetRowIndexByTag(selectedLineItem);
                    projectGrid.RemoveRow(oldRow);
                    AddRowToProjectGrid(project);

                    count = Math.Min(count, idleEngineers.Count);
                    for (int i = 0; i < count; ++i)
                    {
                        project.AddWorker(idleEngineers);
                    }
                    UpdateDetails(project);
                }
            }
            else
            {
                Util.ShowMessageBox(Strings.MSGBOX_NO_IDLE_ENGINEERS);
            }
        }

        private void RemoveScientist()
        {
            LineItem lineItem = GetSelectedItem();
            if (null == lineItem)
                return;

            lineItem.RemoveWorker(idleEngineers);
            UpdateDetails(lineItem);
        }

        private void RemoveAllEngineers()
        {
            LineItem lineItem = GetSelectedItem();
            if (null == lineItem)
                return;

            while (0 < lineItem.NumWorkers)
            {
                lineItem.RemoveWorker(idleEngineers);
            }
            UpdateDetails(lineItem);
        }

        private void ChangeBuildNumber(int change)
        {
            LineItem lineItem = GetSelectedItem();
            if (null == lineItem)
                return;

            ProjectLineItem project = lineItem as ProjectLineItem;
            if (null != project)
            {
                project.BuildCount += change;
                UpdateDetails(project);
            }
            else
            {
                if (0 < change)
                {
                    AddIdleEngineers(1);
                }
            }
        }

        private void CancelProject()
        {
            LineItem selectedLineItem = GetSelectedItem();
            if (null == selectedLineItem)
                return;

            ProjectLineItem project = selectedLineItem as ProjectLineItem;
            if (null != project)
            {
                project.Cancel();
                FindIdleEngineers();

                IdleLineItem newLineItem = new IdleLineItem(this, project.Item);
                int oldRow = projectGrid.GetRowIndexByTag(project);
                projectGrid.RemoveRow(oldRow);
                AddRowToProjectGrid(newLineItem);
                UpdateDetails(newLineItem);
            }
        }

        private void UpdateDetails(LineItem lineItem)
        {
            availableText.Text = MakeIdleEngineersString();

            int row = projectGrid.GetRowIndexByTag(lineItem);
            if (row < 0) return;

            projectGrid.SetCell(row, 1, lineItem.DisplayNumWorkers);
            projectGrid.SetCell(row, 2, lineItem.DisplayQuantity);
            projectGrid.SetCell(row, 3, lineItem.Eta);

            ShowRequirements(lineItem.BuildInfo);
        }

        private void ShowBasesScreen()
        {
            ScreenManager.ScheduleScreen(new BasesScreen(selectedOutpostIndex));
        }

        private LineItem GetSelectedItem()
        {
            LineItem lineItem = projectGrid.GetSelectedTag() as LineItem;
            if (null == lineItem)
            {
                Util.ShowMessageBox(Strings.MSGBOX_NO_PROJECT_SELECTED);
            }
            return lineItem;
        }

        private String MakeIdleEngineersString()
        {
            return Util.StringFormat(Strings.SCREEN_MANUFACTURE_IDLE_ENGINEERS, idleEngineers.Count);
        }

        private void FindIdleEngineers()
        {
            idleEngineers.Clear();
            uint spaceFree = SelectedOutpost.Statistics.Capacities["STORAGE_ENGINEER"].Available;
            int count = -1;
            foreach (Person p in SelectedOutpost.Inventory.ListStaff("ITEM_PERSON_ENGINEER", false))
            {
                if (++count < spaceFree)
                {
                    idleEngineers.Add(p);
                }
            }
        }

        private abstract class LineItem
        {
            protected LineItem(ManufactureScreen parent)
            {
                this.parent = parent;
            }

            public virtual void RemoveWorker(IList<Person> idle) { }

            public abstract ProjectLineItem GetProject();

            #region Fields

            public abstract string Name { get; }

            public virtual string DisplayNumWorkers { get { return String.Empty; } }

            public virtual int NumWorkers { get { return 0; } }

            public virtual string DisplayQuantity { get { return String.Empty; } }

            public virtual int Quantity { get { return 0; } }

            public virtual string Eta { get { return String.Empty; } }

            public abstract BuildInfo BuildInfo { get; }

            public ManufactureScreen Parent { get { return parent; } }

            private ManufactureScreen parent;

            #endregion Fields
        }

        private class ProjectLineItem : LineItem
        {
            public ProjectLineItem(ManufactureScreen parent, BuildProject project)
                :
                base(parent)
            {
                this.project = project;
            }

            public void AddWorker(IList<Person> idle)
            {
                Person worker = idle[idle.Count - 1];
                idle.RemoveAt(idle.Count - 1);
                project.Add(worker);
            }

            public override void RemoveWorker(IList<Person> idle)
            {
                if (0 < project.NumWorkers)
                {
                    idle.Add(project.RemoveWorker());
                }
            }

            public override ProjectLineItem GetProject()
            {
                return this;
            }

            public void Cancel()
            {
                project.Cancel();
                Parent.FindIdleEngineers();
            }

            #region Fields

            public override string Name { get { return project.Name; } }

            public override string DisplayNumWorkers { get { return Util.ToString(NumWorkers); } }

            public override int NumWorkers { get { return project.NumWorkers; } }

            public override string DisplayQuantity { get { return Util.ToString(project.BuildCount); } }

            public int BuildCount { get { return project.BuildCount; } set { project.BuildCount = value; } }

            public override string Eta { get { return project.CalcTotalItemsEtaToShow(); } }

            public override BuildInfo BuildInfo { get { return Item.BuildInfo; } }

            public ItemInfo Item { get { return project.Item; } }

            private BuildProject project;

            #endregion Fields
        }

        private class IdleLineItem : LineItem
        {
            public IdleLineItem(ManufactureScreen parent, ItemInfo item)
                :
                base(parent)
            {
                this.item = item;
            }

            public override ProjectLineItem GetProject()
            {
                ProjectLineItem project = null;
                string error = item.CanStartManufacture(TechMgr, Parent.SelectedOutpost, Bank);
                if (null != error)
                {
                    Util.ShowMessageBox(Strings.MSGBOX_CANT_BUILD_ITEM, item.Name, error);
                }
                else
                {
                    project = new ProjectLineItem(
                        Parent,
                        Parent.ProjectMgr.CreateProject(item.Id, TechMgr, Parent.SelectedOutpost, Bank)
                    );
                    Parent.FindIdleEngineers();
                }
                return project;
            }

            #region Fields

            public override string Name { get { return item.Name; } }

            public override BuildInfo BuildInfo { get { return item.BuildInfo; } }

            private ItemInfo item;

            #endregion Fields
        }

        #region Fields

        private BuildProjectManager ProjectMgr
        {
            get { return SelectedOutpost.BuildProjectManager; }
        }

        private static TechnologyManager TechMgr
        {
            get { return Xenocide.GameState.GeoData.XCorp.TechManager; }
        }

        private static Bank Bank
        {
            get { return Xenocide.GameState.GeoData.XCorp.Bank; }
        }

        private List<Person> idleEngineers = new List<Person>();

        private Outpost SelectedOutpost { get { return Xenocide.GameState.GeoData.Outposts[selectedOutpostIndex]; } }

        private int selectedOutpostIndex;

        #endregion Fields
    }
}
