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
using System.Text;
using System.Diagnostics;

using CeGui;


using ProjectXenocide.Utils;
using ProjectXenocide.Model.Geoscape;
using ProjectXenocide.Model.Geoscape.Outposts;
using ProjectXenocide.Model.StaticData;
using ProjectXenocide.Model.StaticData.Items;
using ProjectXenocide.Model.StaticData.Research;
using ProjectXenocide.Model;
using Xenocide.Resources;


#endregion

namespace ProjectXenocide.UI.Screens
{
    /// <summary>
    /// This is the screen where player sets the items an outpost is manufacturing
    /// </summary>
    public class ManufactureScreen : Screen
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

        #region Create the CeGui widgets

        /// <summary>
        /// add the buttons to the screen
        /// </summary>
        protected override void CreateCeguiWidgets()
        {
            // Get projects to bring their progress up to date
            ProjectMgr.Update();

            FindIdleEngineers();

            // Text giving number of idle engineers
            availableText = AddStaticText(0.01f, 0.01f, 0.7f, 0.08f);
            availableText.Text = MakeIdleEngineersString();
            availableText.HorizontalFormat = HorizontalTextFormat.WordWrapLeft;

            // The grids
            InitializeGrids();
            PopulateProjectGrid();
            // PopulateRequirementsGrid();

            // buttons
            buildMoreButton = AddButton("BUTTON_BUILD_MORE", 0.7475f, 0.60f, 0.2275f, 0.04125f, "PlanetView\\zoomin.ogg");
            buildLessButton = AddButton("BUTTON_BUILD_LESS", 0.7475f, 0.65f, 0.2275f, 0.04125f, "PlanetView\\zoomout.ogg");
            cancelBuildButton = AddButton("BUTTON_CANCEL_BUILD", 0.7475f, 0.70f, 0.2275f, 0.04125f);
            addIdleEngineersButton = AddButton("BUTTON_ADD_IDLE_ENGINEERS", 0.7475f, 0.75f, 0.2275f, 0.04125f, "PlanetView\\zoomin.ogg");
            moreEngineersButton = AddButton("BUTTON_MORE_ENGINEERS", 0.7475f, 0.80f, 0.2275f, 0.04125f, "PlanetView\\zoomin.ogg");
            lessEngineersButton = AddButton("BUTTON_LESS_ENGINEERS", 0.7475f, 0.85f, 0.2275f, 0.04125f, "PlanetView\\zoomout.ogg");
            removeAllEngineersButton = AddButton("BUTTON_REMOVE_ALL_ENGINEERS", 0.7475f, 0.90f, 0.2275f, 0.04125f, "PlanetView\\zoomout.ogg");
            closeButton = AddButton("BUTTON_CLOSE", 0.7475f, 0.95f, 0.2275f, 0.04125f);

            buildMoreButton.Clicked += new CeGui.GuiEventHandler(OnBuildMoreButton);
            buildLessButton.Clicked += new CeGui.GuiEventHandler(OnBuildLessButton);
            cancelBuildButton.Clicked += new CeGui.GuiEventHandler(OnCancelBuildButton);
            moreEngineersButton.Clicked += new CeGui.GuiEventHandler(OnMoreButton);
            lessEngineersButton.Clicked += new CeGui.GuiEventHandler(OnLessButton);
            addIdleEngineersButton.Clicked += new CeGui.GuiEventHandler(OnAddIdleButton);
            removeAllEngineersButton.Clicked += new CeGui.GuiEventHandler(OnRemoveAllButton);
            closeButton.Clicked += new CeGui.GuiEventHandler(OnCloseButton);
        }

        private CeGui.Widgets.StaticText availableText;
        private CeGui.Widgets.MultiColumnList projectGrid;
        private CeGui.Widgets.MultiColumnList requirementsGrid;

        private CeGui.Widgets.PushButton buildMoreButton;
        private CeGui.Widgets.PushButton buildLessButton;
        private CeGui.Widgets.PushButton cancelBuildButton;
        private CeGui.Widgets.PushButton moreEngineersButton;
        private CeGui.Widgets.PushButton lessEngineersButton;
        private CeGui.Widgets.PushButton removeAllEngineersButton;
        private CeGui.Widgets.PushButton addIdleEngineersButton;
        private CeGui.Widgets.PushButton closeButton;

        /// <summary>
        /// Create MultiColumnListBoxs which appear on the screen
        /// </summary>
        private void InitializeGrids()
        {
            projectGrid = AddGrid(0.01f, 0.13f, 0.70f, 0.56f,
                Strings.SCREEN_MANUFACTURE_COLUMN_PROJECT, 0.50f,
                Strings.SCREEN_MANUFACTURE_COLUMN_ENGINEERS, 0.15f,
                Strings.SCREEN_MANUFACTURE_COLUMN_BUILD_QUANTITY, 0.15f,
                Strings.SCREEN_MANUFACTURE_COLUMN_ETA, 0.15f
            );
            projectGrid.SelectionChanged += new WindowEventHandler(OnProjectGridSelectionChanged);

            requirementsGrid = AddGrid(0.01f, 0.72f, 0.70f, 0.27f,
                Strings.SCREEN_MANUFACTURE_COLUMN_RESOURCE, 0.50f,
                Strings.SCREEN_MANUFACTURE_COLUMN_QUANTITY_NEEDED, 0.23f,
                Strings.SCREEN_MANUFACTURE_COLUMN_QUANTITY_AVAILABLE, 0.25f
            );
        }

        /// <summary>
        /// Put the list of list of items being built/available to build into the grid
        /// </summary>
        private void PopulateProjectGrid()
        {
            // put items being built at top of list
            foreach (BuildProject project in ProjectMgr)
            {
                AddRowToProjectGrid(new ProjectLineItem(this, project));
            }

            // now add everything else that can be built
            foreach (ItemInfo item in Xenocide.StaticTables.ItemList)
            {
                if ((null != item.BuildInfo) && TechMgr.IsAvailable(item.Id) && !ProjectMgr.IsInProgress(item.Id))
                {
                    AddRowToProjectGrid(new IdleLineItem(this, item));
                }
            }

            // give message if there's nothing that can be built
            if (0 == projectGrid.RowCount)
            {
                Util.ShowMessageBox(Strings.MSGBOX_NO_BUILDABLE_TECHNOLOGIES);
            }
        }

        /// <summary>
        /// Add a row to the grid
        /// </summary>
        /// <param name="lineItem">data to put on line</param>
        private void AddRowToProjectGrid(LineItem lineItem)
        {
            CeGui.ListboxTextItem listboxItem = Util.CreateListboxItem(lineItem.Name);
            int rowNum = projectGrid.AddRow(listboxItem, 0);
            listboxItem.ID = rowNum;
            Util.AddStringElementToGrid(projectGrid, 1, rowNum, lineItem.DisplayNumWorkers);
            Util.AddStringElementToGrid(projectGrid, 2, rowNum, lineItem.DisplayQuantity);
            Util.AddStringElementToGrid(projectGrid, 3, rowNum, lineItem.Eta);

            // and record details of this item
            lineItems[rowNum] = lineItem;
        }

        /// <summary>
        /// Update the Requirements grid to show what is needed to build the specified item
        /// </summary>
        /// <param name="buildInfo">Requirements to build specified item</param>
        private void ShowRequirements(BuildInfo buildInfo)
        {
            requirementsGrid.ResetList();
            // workspace
            if (0 < buildInfo.Space)
            {
                string needed = Util.ToString(buildInfo.Space);
                string available = Util.ToString((int)buildInfo.GetCapacityInfo(SelectedOutpost).Available);
                AddRowToRequirementsGrid(Strings.SCREEN_MANUFACTURE_REPORT_ROW_WORKSPACE, needed, available);
            }

            // hours
            if (0 < buildInfo.Hours)
            {
                string needed = Util.ToString(buildInfo.Hours);
                string available = String.Empty;
                AddRowToRequirementsGrid(Strings.SCREEN_MANUFACTURE_REPORT_ROW_HOURS, needed, available);
            }

            // dollars
            if (0 < buildInfo.Dollars)
            {
                string needed = Util.FormatCurrency(buildInfo.Dollars);
                string available = Xenocide.GameState.GeoData.XCorp.Bank.DisplayCurrentBalance;
                AddRowToRequirementsGrid(Strings.SCREEN_MANUFACTURE_REPORT_ROW_MONEY, needed, available);
            }

            // materials
            foreach (ItemLine material in buildInfo.Materials)
            {
                string needed = Util.ToString(material.Quantity);
                string available = Util.ToString(SelectedOutpost.Inventory.NumberInInventory(material.ItemInfo));
                AddRowToRequirementsGrid(material.ItemInfo.Name, needed, available);
            }
        }

        /// <summary>
        /// Add a row of data the requirements grid
        /// </summary>
        /// <param name="resourceName">name of the required resource</param>
        /// <param name="needed">quantity needed</param>
        /// <param name="available">quantity available</param>
        private void AddRowToRequirementsGrid(string resourceName, string needed, string available)
        {
            int rowNum = requirementsGrid.AddRow(Util.CreateListboxItem(resourceName), 0);
            Util.AddStringElementToGrid(requirementsGrid, 1, rowNum, needed);
            Util.AddStringElementToGrid(requirementsGrid, 2, rowNum, available);
        }

        #endregion Create the CeGui widgets

        #region event handlers

        /// <summary>React to user pressing the "Build More" button</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void OnBuildMoreButton(object sender, CeGui.GuiEventArgs e)
        {
            ChangeBuildNumber(1);
        }

        /// <summary>React to user pressing the "Build Less" button</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void OnBuildLessButton(object sender, CeGui.GuiEventArgs e)
        {
            ChangeBuildNumber(-1);
        }

        /// <summary>React to user pressing the "Cancel Build" button</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void OnCancelBuildButton(object sender, CeGui.GuiEventArgs e)
        {
            CancelProject();
        }

        /// <summary>React to user pressing the More Engineers</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void OnMoreButton(object sender, CeGui.GuiEventArgs e)
        {
            AddIdleEngineers(1);
        }

        /// <summary>React to user pressing the Add Idle Engineers Button</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void OnAddIdleButton(object sender, CeGui.GuiEventArgs e)
        {
            AddIdleEngineers(idleEngineers.Count);
        }

        /// <summary>React to user pressing the Remove All Engineers Button</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void OnRemoveAllButton(object sender, CeGui.GuiEventArgs e)
        {
            RemoveAllEngineers();
        }

        /// <summary>React to user pressing the Less Engineers button</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void OnLessButton(object sender, CeGui.GuiEventArgs e)
        {
            RemoveScientist();
        }

        /// <summary>React to user pressing the Close button</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void OnCloseButton(object sender, CeGui.GuiEventArgs e)
        {
            ShowBasesScreen();
        }

        /// <summary>Handle user clicking on an item in the projects grid</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void OnProjectGridSelectionChanged(object sender, WindowEventArgs e)
        {
            CeGui.Widgets.ListboxItem item = projectGrid.GetFirstSelectedItem();
            if (item != null)
            {
                ShowRequirements(lineItems[item.ID].BuildInfo);
            }
        }

        #endregion event handlers

        /// <summary>
        /// Add Engineers to the currently selected project
        /// </summary>
        /// <param name="count">number of Engineers to add</param>
        private void AddIdleEngineers(int count)
        {
            CeGui.Widgets.ListboxItem selectedItem = GetSelectedItem();
            if (null != selectedItem)
            {
                // can only add scientist if we have one that's idle
                if (0 < idleEngineers.Count)
                {
                    Debug.Assert((0 < count) && (count <= idleEngineers.Count));
                    ProjectLineItem project = lineItems[selectedItem.ID].GetProject();
                    if (null != project)
                    {
                        // update lineItems in case we've promoted from TopicLine to ProjectLine
                        lineItems[selectedItem.ID] = project;

                        // add specified number of idle Engineers to the project
                        // note, if we've just spawned the project, available workspace has been reduced
                        count = Math.Min(count, idleEngineers.Count);
                        for (int i = 0; i < count; ++i)
                        {
                            project.AddWorker(idleEngineers);
                        }
                        UpdateDetails(selectedItem, project);
                    }
                }
                else
                {
                    Util.ShowMessageBox(Strings.MSGBOX_NO_IDLE_ENGINEERS);
                }
            }
        }

        /// <summary>
        /// Remove a scientist from the currently selected project
        /// </summary>
        private void RemoveScientist()
        {
            CeGui.Widgets.ListboxItem selectedItem = GetSelectedItem();
            if (null != selectedItem)
            {
                LineItem lineItem = lineItems[selectedItem.ID];
                lineItem.RemoveWorker(idleEngineers);
                UpdateDetails(selectedItem, lineItem);
            }
        }

        /// <summary>
        /// Remove all Engineers from the currently selected project
        /// </summary>
        private void RemoveAllEngineers()
        {
            CeGui.Widgets.ListboxItem selectedItem = GetSelectedItem();
            if (null != selectedItem)
            {
                LineItem lineItem = lineItems[selectedItem.ID];
                while (0 < lineItem.NumWorkers)
                {
                    lineItem.RemoveWorker(idleEngineers);
                }
                UpdateDetails(selectedItem, lineItem);
            }
        }

        /// <summary>
        /// Change the number of items to build
        /// </summary>
        /// <param name="change">how much to change the number by</param>
        private void ChangeBuildNumber(int change)
        {
            CeGui.Widgets.ListboxItem selectedItem = GetSelectedItem();
            if (null != selectedItem)
            {
                ProjectLineItem project = lineItems[selectedItem.ID] as ProjectLineItem;
                if (null != project)
                {
                    project.BuildCount += change;
                    UpdateDetails(selectedItem, project);
                }
                else
                {
                    // user clicked on an empty project, so either start it or ignore
                    if (0 < change)
                    {
                        AddIdleEngineers(1);
                    }
                }
            }
        }

        /// <summary>
        /// Cancel the selected project
        /// </summary>
        private void CancelProject()
        {
            CeGui.Widgets.ListboxItem selectedItem = GetSelectedItem();
            if (null != selectedItem)
            {
                ProjectLineItem project = lineItems[selectedItem.ID] as ProjectLineItem;
                if (null != project)
                {
                    project.Cancel();
                    FindIdleEngineers();

                    // change row back to just an item row
                    IdleLineItem lineItem = new IdleLineItem(this, project.Item);
                    lineItems[selectedItem.ID] = lineItem;
                    UpdateDetails(selectedItem, lineItem);
                }
            }
        }

        /// <summary>
        /// Update the screen to reflect the latest changes
        /// </summary>
        /// <param name="selectedItem">row in grid that is selected</param>
        /// <param name="lineItem">LineItem associated with this row</param>
        private void UpdateDetails(CeGui.Widgets.ListboxItem selectedItem, LineItem lineItem)
        {
            availableText.Text = MakeIdleEngineersString();

            // update row in grid
            int row = projectGrid.GetRowIndexOfItem(selectedItem);
            CeGui.Widgets.GridReference position = new CeGui.Widgets.GridReference(row, 1);
            projectGrid.GetItemAtGridReference(position).Text = lineItem.DisplayNumWorkers;
            position.Column = 2;
            projectGrid.GetItemAtGridReference(position).Text = lineItem.DisplayQuantity;
            position.Column = 3;
            projectGrid.GetItemAtGridReference(position).Text = lineItem.Eta;

            // Update the requirements as well (in case we've consumed resources)
            ShowRequirements(lineItem.BuildInfo);
        }

        /// <summary>
        /// Close this screen and go back to the Bases screen
        /// </summary>
        private void ShowBasesScreen()
        {
            ScreenManager.ScheduleScreen(new BasesScreen(selectedOutpostIndex));
        }

        // Get currently selected item from Grid.  Give error message if nothing selected
        private CeGui.Widgets.ListboxItem GetSelectedItem()
        {
            CeGui.Widgets.ListboxItem selectedItem = projectGrid.GetFirstSelectedItem();
            if (null == selectedItem)
            {
                Util.ShowMessageBox(Strings.MSGBOX_NO_PROJECT_SELECTED);
            }
            return selectedItem;
        }

        /// <summary>
        /// Make up text to show, giving number of Idle engineers
        /// </summary>
        /// <returns>text to show</returns>
        private String MakeIdleEngineersString()
        {
            return Util.StringFormat(Strings.SCREEN_MANUFACTURE_IDLE_ENGINEERS, idleEngineers.Count);
        }

        /// <summary>
        /// Find all the engineers (in X-Corp this outpost) that are not doing anything
        /// but have available space to work in
        /// </summary>
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

        /// <summary>
        /// Holds the data for a line in the grid
        /// </summary>
        private abstract class LineItem
        {
            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="parent">Manufacturing Screen this line has info for </param>
            protected LineItem(ManufactureScreen parent)
            {
                this.parent = parent;
            }

            /// <summary>
            /// Remove a worker from the line item
            /// </summary>
            /// <param name="idle">list of idle workers to add now idle woker to</param>
            public virtual void RemoveWorker(IList<Person> idle) { }

            /// <summary>
            /// Get the project line represented by this line in the Grid, creating a project if necessary
            /// </summary>
            /// <returns>line to put in grid for this topic</returns>
            public abstract ProjectLineItem GetProject();

            #region Fields

            /// <summary>
            /// Value to show in "Name" column
            /// </summary>
            public abstract string Name { get; }

            /// <summary>
            /// Value to show in "Engineers" column
            /// </summary>
            public virtual string DisplayNumWorkers { get { return String.Empty; } }

            /// <summary>
            /// Number of workers assigned to project
            /// </summary>
            public virtual int NumWorkers { get { return 0; } }

            /// <summary>
            /// Value to show in "Quantity" column
            /// </summary>
            public virtual string DisplayQuantity { get { return String.Empty; } }

            /// <summary>
            /// Number of items to build
            /// </summary>
            public virtual int Quantity { get { return 0; } }

            /// <summary>
            /// Value to show in "Days Left" column
            /// </summary>
            public virtual string Eta { get { return String.Empty; } }

            /// <summary>
            /// The requirements to manufacture an instance of this item
            /// </summary>
            public abstract BuildInfo BuildInfo { get; }

            /// <summary>
            ///  Manufacturing Screen this line has info for
            /// </summary>
            public ManufactureScreen Parent { get { return parent; } }

            /// <summary>
            ///  Manufacturing Screen this line has info for
            /// </summary>
            private ManufactureScreen parent;

            #endregion Fields
        }

        /// <summary>
        /// A row on the grid for an item we're building
        /// </summary>
        private class ProjectLineItem : LineItem
        {
            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="project">Project this line is giving details for</param>
            /// <param name="parent">Manufacturing Screen this line has info for</param>
            public ProjectLineItem(ManufactureScreen parent, BuildProject project)
                :
                base(parent)
            {
                this.project = project;
            }

            /// <summary>
            /// Add a worker to the line item
            /// </summary>
            /// <param name="idle">list of idle workers to get worker from</param>
            public void AddWorker(IList<Person> idle)
            {
                Person worker = idle[idle.Count - 1];
                idle.RemoveAt(idle.Count - 1);
                project.Add(worker);
            }

            /// <summary>
            /// Remove a worker from the line item
            /// </summary>
            /// <param name="idle">list of idle workers to add now idle woker to</param>
            public override void RemoveWorker(IList<Person> idle)
            {
                if (0 < project.NumWorkers)
                {
                    idle.Add(project.RemoveWorker());
                }
            }

            /// <summary>
            /// Get the project line represented by this line in the Grid, creating a project if necessary
            /// </summary>
            /// <returns>line to put in grid for this topic</returns>
            public override ProjectLineItem GetProject()
            {
                return this;
            }

            /// <summary>
            /// Cancel this project, loosing all investment
            /// </summary>
            public void Cancel()
            {
                project.Cancel();
                Parent.FindIdleEngineers();
            }

            #region Fields

            /// <summary>
            /// Value to show in "Name" column
            /// </summary>
            public override string Name { get { return project.Name; } }

            /// <summary>
            /// Value to show in "Engineers" column
            /// </summary>
            public override string DisplayNumWorkers { get { return Util.ToString(NumWorkers); } }

            /// <summary>
            /// Number of workers assigned to project
            /// </summary>
            public override int NumWorkers { get { return project.NumWorkers; } }

            /// <summary>
            /// Number of items to build, formatted for display to user
            /// </summary>
            public override string DisplayQuantity { get { return Util.ToString(project.BuildCount); } }

            /// <summary>
            /// Number of items to build
            /// </summary>
            public int BuildCount { get { return project.BuildCount; } set { project.BuildCount = value; } }

            /// <summary>
            /// Value to show in "Days Left" column
            /// </summary>
            public override string Eta { get { return project.CalcTotalItemsEtaToShow(); } }

            /// <summary>
            /// The requirements to manufacture an instance of this item
            /// </summary>
            public override BuildInfo BuildInfo { get { return Item.BuildInfo; } }

            /// <summary>
            /// Type of item project is building
            /// </summary>
            public ItemInfo Item { get { return project.Item; } }

            /// <summary>
            /// Project this line gives details for
            /// </summary>
            private BuildProject project;

            #endregion Fields
        }

        /// <summary>
        /// A row on the grid for an item we're not building
        /// </summary>
        private class IdleLineItem : LineItem
        {
            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="item">Item this line is giving details for</param>
            /// <param name="parent">Manufacturing Screen this line has info for</param>
            public IdleLineItem(ManufactureScreen parent, ItemInfo item)
                :
                base(parent)
            {
                this.item = item;
            }

            /// <summary>
            /// Get the project line represented by this line in the Grid, creating a project if necessary
            /// </summary>
            /// <returns>line to put in grid for this topic</returns>
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
                    // project takes up space, reduces that available for engineers.
                    Parent.FindIdleEngineers();
                }
                return project;
            }

            #region Fields

            /// <summary>
            /// Value to show in "Project" column
            /// </summary>
            public override string Name { get { return item.Name; } }

            /// <summary>
            /// The requirements to manufacture an instance of this item
            /// </summary>
            public override BuildInfo BuildInfo { get { return item.BuildInfo; } }

            /// <summary>
            /// Item this line is giving details for
            /// </summary>
            private ItemInfo item;

            #endregion Fields
        }

        #region Fields

        /// <summary>
        /// Shortcut
        /// </summary>
        private BuildProjectManager ProjectMgr
        {
            get { return SelectedOutpost.BuildProjectManager; }
        }

        /// <summary>
        /// Shortcut
        /// </summary>
        private static TechnologyManager TechMgr
        {
            get { return Xenocide.GameState.GeoData.XCorp.TechManager; }
        }

        /// <summary>
        /// Shortcut
        /// </summary>
        private static Bank Bank
        {
            get { return Xenocide.GameState.GeoData.XCorp.Bank; }
        }

        /// <summary>
        /// Bind lines in grid to object providing data to show.
        /// format is Dictionary&lt;line id, LineData&gt;
        /// </summary>
        private Dictionary<int, LineItem> lineItems = new Dictionary<int, LineItem>();

        /// <summary>
        /// Engineers that currently are not working, but could work
        /// </summary>
        private List<Person> idleEngineers = new List<Person>();

        /// <summary>
        /// The outpost we're showing the details for
        /// </summary>
        private Outpost SelectedOutpost { get { return Xenocide.GameState.GeoData.Outposts[selectedOutpostIndex]; } }

        // index specifying the outpost that screen is showwing
        private int selectedOutpostIndex;

        #endregion Fields
    }
}
