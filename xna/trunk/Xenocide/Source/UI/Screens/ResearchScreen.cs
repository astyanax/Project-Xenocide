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
* @file ResearchScreen.cs
* @date Created: 2007/09/30
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
using ProjectXenocide.Model.StaticData.Items;
using ProjectXenocide.Model.StaticData.Research;
using ProjectXenocide.Model;
using Xenocide.Resources;


#endregion

namespace ProjectXenocide.UI.Screens
{
    /// <summary>
    /// This is the screen that shows Topics being researched and available for research
    /// </summary>
    public class ResearchScreen : Screen
    {
        /// <summary>
        /// Constructor (obviously)
        /// </summary>
        public ResearchScreen()
            : base("Research")
        {
        }

        #region Create the CeGui widgets

        /// <summary>
        /// add the buttons to the screen
        /// </summary>
        protected override void CreateCeguiWidgets()
        {
            // Get projects to bring their progress up to date
            ProjectMgr.Update();

            FindIdleScientists();

            // Text giving number of idle scientists
            availableText = AddStaticText(0.01f, 0.01f, 0.7f, 0.08f);
            availableText.Text = MakeIdleScientistsString();
            availableText.HorizontalFormat = HorizontalTextFormat.WordWrapLeft;

            // The gird of research projects
            InitializeGrid();
            PopulateGrid();

            // buttons
            addIdleScientistsButton = AddButton("BUTTON_ADD_IDLE_SCIENTISTS", 0.7475f, 0.75f, 0.2275f, 0.04125f, "PlanetView\\zoomin.ogg");
            moreScientistsButton = AddButton("BUTTON_MORE_SCIENTISTS", 0.7475f, 0.80f, 0.2275f, 0.04125f, "PlanetView\\zoomin.ogg");
            lessScientistsButton = AddButton("BUTTON_LESS_SCIENTISTS", 0.7475f, 0.85f, 0.2275f, 0.04125f, "PlanetView\\zoomout.ogg");
            removeAllScientistsButton = AddButton("BUTTON_REMOVE_ALL_SCIENTISTS", 0.7475f, 0.90f, 0.2275f, 0.04125f, "PlanetView\\zoomout.ogg");
            closeButton = AddButton("BUTTON_CLOSE", 0.7475f, 0.95f, 0.2275f, 0.04125f);

            moreScientistsButton.Clicked += new CeGui.GuiEventHandler(OnMoreButton);
            lessScientistsButton.Clicked += new CeGui.GuiEventHandler(OnLessButton);
            addIdleScientistsButton.Clicked += new CeGui.GuiEventHandler(OnAddIdleButton);
            removeAllScientistsButton.Clicked += new CeGui.GuiEventHandler(OnRemoveAllButton);
            closeButton.Clicked += new CeGui.GuiEventHandler(OnCloseButton);
        }

        private CeGui.Widgets.StaticText availableText;
        private CeGui.Widgets.MultiColumnList grid;
        private CeGui.Widgets.PushButton moreScientistsButton;
        private CeGui.Widgets.PushButton lessScientistsButton;
        private CeGui.Widgets.PushButton removeAllScientistsButton;
        private CeGui.Widgets.PushButton addIdleScientistsButton;
        private CeGui.Widgets.PushButton closeButton;

        /// <summary>
        /// Create MultiColumnListBox which holds items being shiped
        /// </summary>
        private void InitializeGrid()
        {
            grid = GuiBuilder.CreateGrid("researchGrid");
            AddWidget(grid, 0.01f, 0.13f, 0.70f, 0.86f);
            grid.AddColumn(Strings.SCREEN_RESEARCH_COLUMN_PROJECT, grid.ColumnCount, 0.50f);
            grid.AddColumn(Strings.SCREEN_RESEARCH_COLUMN_SCIENTISTS, grid.ColumnCount, 0.25f);
            grid.AddColumn(Strings.SCREEN_RESEARCH_COLUMN_ETA, grid.ColumnCount, 0.22f);
        }

        /// <summary>
        /// Put the list of items being shiped into the grid
        /// </summary>
        private void PopulateGrid()
        {
            // active projects at top
            foreach (ResearchProject project in ProjectMgr)
            {
                AddRowToGrid(new ProjectLineItem(project));
            }

            // followed by projects that could be started
            ResearchGraph graph = Xenocide.StaticTables.ResearchGraph;
            foreach (ResearchTopic topic in graph.StartableTopics(TechMgr, Outposts))
            {
                if (!ProjectMgr.IsInProgress(topic.Id))
                {
                    AddRowToGrid(new TopicLineItem(topic));
                }
            }
        }

        /// <summary>
        /// Add a row to the grid
        /// </summary>
        /// <param name="lineItem">data to put on line</param>
        private void AddRowToGrid(LineItem lineItem)
        {
            CeGui.ListboxTextItem listboxItem = Util.CreateListboxItem(lineItem.Name);
            int rowNum = grid.AddRow(listboxItem, 0);
            listboxItem.ID = rowNum;
            Util.AddStringElementToGrid(grid, 1, rowNum, lineItem.DisplayNumWorkers);
            Util.AddStringElementToGrid(grid, 2, rowNum, lineItem.Eta);

            // and record details of this item
            lineItems[rowNum] = lineItem;
        }

        #endregion Create the CeGui widgets

        #region event handlers

        /// <summary>React to user pressing the More Scientists</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void OnMoreButton(object sender, CeGui.GuiEventArgs e)
        {
            AddIdleScientists(1);
        }

        /// <summary>React to user pressing the Add Idle Scientists Button</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void OnAddIdleButton(object sender, CeGui.GuiEventArgs e)
        {
            AddIdleScientists(idleScientists.Count);
        }

        /// <summary>React to user pressing the Remove All Scientists Button</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void OnRemoveAllButton(object sender, CeGui.GuiEventArgs e)
        {
            RemoveAllScientists();
        }

        /// <summary>React to user pressing the Less Scientists button</summary>
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
            GoToGeoscapeScreen();
        }

        #endregion event handlers

        /// <summary>
        /// Add scientists to the currently selected project
        /// </summary>
        /// <param name="count">number of scientists to add</param>
        private void AddIdleScientists(int count)
        {
            CeGui.Widgets.ListboxItem selectedItem = GetSelectedItem();
            if (null != selectedItem)
            {
                // can only add scientist if we have one that's idle
                if (0 < idleScientists.Count)
                {
                    Debug.Assert((0 < count) && (count <= idleScientists.Count));
                    ProjectLineItem project = lineItems[selectedItem.ID].GetProject();

                    // update lineItems in case we've promoted from TopicLine to ProjectLine
                    lineItems[selectedItem.ID] = project;

                    // add specified number of idle scientists to the project
                    for (int i = 0; i < count; ++i)
                    {
                        project.AddWorker(idleScientists);
                    }
                    UpdateDetails(selectedItem, project);

                    // we may have started a new project and consumed an artifact needed to
                    // start other research topics
                    RemoveUnavailableTopics();
                }
                else
                {
                    Util.ShowMessageBox(Strings.MSGBOX_NO_IDLE_SCIENTISTS);
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
                lineItem.RemoveWorker(idleScientists);
                UpdateDetails(selectedItem, lineItem);
            }
        }

        /// <summary>
        /// Remove all scientists from the currently selected project
        /// </summary>
        private void RemoveAllScientists()
        {
            CeGui.Widgets.ListboxItem selectedItem = GetSelectedItem();
            if (null != selectedItem)
            {
                LineItem lineItem = lineItems[selectedItem.ID];
                while (0 < lineItem.NumWorkers)
                {
                    lineItem.RemoveWorker(idleScientists);
                }
                UpdateDetails(selectedItem, lineItem);
            }
        }

        /// <summary>
        /// Update the screen to reflect the latest changes
        /// </summary>
        /// <param name="selectedItem">row in gird that is selected</param>
        /// <param name="lineItem">LineItem associated with this row</param>
        private void UpdateDetails(CeGui.Widgets.ListboxItem selectedItem, LineItem lineItem)
        {
            availableText.Text = MakeIdleScientistsString();

            // update row in grid
            int row = grid.GetRowIndexOfItem(selectedItem);
            CeGui.Widgets.GridReference position = new CeGui.Widgets.GridReference(row, 1);
            grid.GetItemAtGridReference(position).Text = lineItem.DisplayNumWorkers;
            position.Column = 2;
            grid.GetItemAtGridReference(position).Text = lineItem.Eta;
        }

        /// <summary>
        /// Remove any rows from the grid that can no longer be researched
        /// </summary>
        private void RemoveUnavailableTopics()
        {
            CeGui.Widgets.GridReference position = new CeGui.Widgets.GridReference(0, 0);
            for (int i = 0; i < grid.RowCount; ++i)
            {
                position.Row = i;
                if (!lineItems[grid.GetItemAtGridReference(position).ID].CanResearch)
                {
                    grid.RemoveRow(i);
                }
            }
        }

        /// <summary>
        /// Close this screen and go back to the Geoscape screen
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:DisposeObjectsBeforeLosingScope",
            Justification = "FxCop False Positive")]
        private void GoToGeoscapeScreen()
        {
            ScreenManager.ScheduleScreen(new GeoscapeScreen());
        }

        // Get currently selected item from Grid.  Give error message if nothing selected
        private CeGui.Widgets.ListboxItem GetSelectedItem()
        {
            CeGui.Widgets.ListboxItem selectedItem = grid.GetFirstSelectedItem();
            if (null == selectedItem)
            {
                Util.ShowMessageBox(Strings.MSGBOX_NO_PROJECT_SELECTED);
            }
            return selectedItem;
        }

        /// <summary>
        /// Make up text to show, giving number of Idle scientists
        /// </summary>
        /// <returns>text to show</returns>
        private String MakeIdleScientistsString()
        {
            return Util.StringFormat(Strings.SCREEN_RESEARCH_IDLE_SCIENTISTS, idleScientists.Count);
        }

        /// <summary>
        /// Find all the scientists (in X-Corp as whole) that are not doing anything
        /// but have available lab space to work in
        /// </summary>
        private void FindIdleScientists()
        {
            foreach (Outpost outpost in Outposts)
            {
                uint spaceFree = outpost.Statistics.Capacities["STORAGE_SCIENTIST"].Available;
                int count = -1;
                foreach (Person p in outpost.Inventory.ListStaff("ITEM_PERSON_SCIENTIST", false))
                {
                    if (++count < spaceFree)
                    {
                        idleScientists.Add(p);
                    }
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
            protected LineItem() { }

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
            /// Value to show in "Assigned Scientists" column
            /// </summary>
            public virtual string DisplayNumWorkers { get { return String.Empty; } }

            /// <summary>
            /// Number of workers assigned to project
            /// </summary>
            public virtual int NumWorkers { get { return 0; } }

            /// <summary>
            /// Value to show in "Days Left" column
            /// </summary>
            public virtual string Eta { get { return String.Empty; } }

            /// <summary>
            /// Can line item still be researched?
            /// </summary>
            public virtual bool CanResearch { get { return true; } }

            #endregion Fields
        }

        private class ProjectLineItem : LineItem
        {
            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="project">Research Project this line is giving details for</param>
            public ProjectLineItem(ResearchProject project)
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

            #region Fields

            /// <summary>
            /// Value to show in "Name" column
            /// </summary>
            public override string Name { get { return project.Name; } }

            /// <summary>
            /// Value to show in "Assigned Scientists" column
            /// </summary>
            public override string DisplayNumWorkers { get { return Util.ToString(NumWorkers); } }

            /// <summary>
            /// Number of workers assigned to project
            /// </summary>
            public override int NumWorkers { get { return project.NumWorkers; } }

            /// <summary>
            /// Value to show in "Days Left" column
            /// </summary>
            public override string Eta { get { return project.CalcEtaToShow(); } }

            /// <summary>
            /// Research Project this line gives details for
            /// </summary>
            private ResearchProject project;

            #endregion Fields
        }

        private class TopicLineItem : LineItem
        {
            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="topic">ResearchTopic  this line is giving details for</param>
            public TopicLineItem(ResearchTopic topic)
            {
                this.topic = topic;
            }

            /// <summary>
            /// Get the project line represented by this line in the Grid, creating a project if necessary
            /// </summary>
            /// <returns>line to put in grid for this topic</returns>
            public override ProjectLineItem GetProject()
            {
                return new ProjectLineItem(ProjectMgr.CreateProject(topic.Id, TechMgr, Outposts));
            }

            #region Fields

            /// <summary>
            /// Value to show in "Name" column
            /// </summary>
            public override string Name { get { return topic.Name; } }

            /// <summary>
            /// Can line item still be researched?
            /// </summary>
            public override bool CanResearch { get { return topic.CanResearch(TechMgr, Outposts); } }

            /// <summary>
            /// Research Topic this line gives details for
            /// </summary>
            private ResearchTopic topic;

            #endregion Fields
        }

        #region Fields

        /// <summary>
        /// Shortcut
        /// </summary>
        private static ResearchProjectManager ProjectMgr
        {
            get { return Xenocide.GameState.GeoData.XCorp.ResearchManager; }
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
        private static ICollection<Outpost> Outposts
        {
            get { return Xenocide.GameState.GeoData.Outposts; }
        }

        /// <summary>
        /// Bind lines in grid to object providing data to show.
        /// format is Dictionary&lt;line id, LineData&gt;
        /// </summary>
        private Dictionary<int, LineItem> lineItems = new Dictionary<int, LineItem>();

        /// <summary>
        /// Scienists that currently are not working, but could work
        /// </summary>
        private List<Person> idleScientists = new List<Person>();

        #endregion Fields
    }
}
