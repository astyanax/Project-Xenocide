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
using System.Diagnostics;
using System.Text;

using Gum.Forms;
using Gum.Forms.Controls;

using ProjectXenocide.Model;
using ProjectXenocide.Model.Geoscape;
using ProjectXenocide.Model.Geoscape.Outposts;
using ProjectXenocide.Model.StaticData.Items;
using ProjectXenocide.Model.StaticData.Research;
using ProjectXenocide.UI.Controls;
using ProjectXenocide.Utils;

using Xenocide.Resources;


#endregion

namespace ProjectXenocide.UI.Screens
{
    /// <summary>
    /// This is the screen that shows Topics being researched and available for research
    /// </summary>
    public class ResearchScreen : GumScreen
    {
        /// <summary>
        /// Constructor (obviously)
        /// </summary>
        public ResearchScreen()
            : base("Research")
        {
        }

        #region Create the Gum controls

        /// <summary>
        /// add the buttons to the screen
        /// </summary>
        protected override void CreateGumControls()
        {
            ProjectMgr.Update();
            FindIdleScientists();

            if (GumRoot != null)
            {
                WireButton("addIdleScientistsButton", OnAddIdleButton);
                WireButton("moreScientistsButton", OnMoreButton);
                WireButton("lessScientistsButton", OnLessButton);
                WireButton("removeAllScientistsButton", OnRemoveAllButton);
                WireButton("closeButton", OnCloseButton);

                availableText = new Label();
                availableText.Visual.X = 20;
                availableText.Visual.Y = 80;
                AddChild(availableText);
                availableText.Text = MakeIdleScientistsString();

                InitializeGrid();
                grid.Visual.X = 20;
                grid.Visual.Y = 110;
                grid.Visual.Width = 800;
                PopulateGrid();
                return;
            }

            availableText = new Label();
            RootContainer.AddChild(availableText);
            availableText.Text = MakeIdleScientistsString();

            InitializeGrid();
            PopulateGrid();

            addIdleScientistsButton = new Button() { Text = XenocideResourceManager.Get("BUTTON_ADD_IDLE_SCIENTISTS") };
            RootContainer.AddChild(addIdleScientistsButton);
            moreScientistsButton = new Button() { Text = XenocideResourceManager.Get("BUTTON_MORE_SCIENTISTS") };
            RootContainer.AddChild(moreScientistsButton);
            lessScientistsButton = new Button() { Text = XenocideResourceManager.Get("BUTTON_LESS_SCIENTISTS") };
            RootContainer.AddChild(lessScientistsButton);
            removeAllScientistsButton = new Button() { Text = XenocideResourceManager.Get("BUTTON_REMOVE_ALL_SCIENTISTS") };
            RootContainer.AddChild(removeAllScientistsButton);
            closeButton = new Button() { Text = XenocideResourceManager.Get("BUTTON_CLOSE") };
            RootContainer.AddChild(closeButton);

            moreScientistsButton.Click += OnMoreButton;
            lessScientistsButton.Click += OnLessButton;
            addIdleScientistsButton.Click += OnAddIdleButton;
            removeAllScientistsButton.Click += OnRemoveAllButton;
            closeButton.Click += OnCloseButton;
        }

        private Label availableText;
        private GridPanel grid;
        private Button moreScientistsButton;
        private Button lessScientistsButton;
        private Button removeAllScientistsButton;
        private Button addIdleScientistsButton;
        private Button closeButton;

        /// <summary>
        /// Create GridPanel which holds items being shiped
        /// </summary>
        private void InitializeGrid()
        {
            grid = new GridPanel();
            AddChild(grid.Visual);
            grid.AddColumn(Strings.SCREEN_RESEARCH_COLUMN_PROJECT, (int)(0.50f * 800));
            grid.AddColumn(Strings.SCREEN_RESEARCH_COLUMN_SCIENTISTS, (int)(0.25f * 800));
            grid.AddColumn(Strings.SCREEN_RESEARCH_COLUMN_ETA, (int)(0.22f * 800));
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
            int rowNum = grid.RowCount;
            grid.AddRow(rowNum, lineItem.Name, lineItem.DisplayNumWorkers, lineItem.Eta);

            // and record details of this item
            lineItems[rowNum] = lineItem;
        }

        #endregion Create the Gum controls

        #region event handlers

        /// <summary>React to user pressing the More Scientists</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void OnMoreButton(object sender, EventArgs e)
        {
            AddIdleScientists(1);
        }

        /// <summary>React to user pressing the Add Idle Scientists Button</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void OnAddIdleButton(object sender, EventArgs e)
        {
            AddIdleScientists(idleScientists.Count);
        }

        /// <summary>React to user pressing the Remove All Scientists Button</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void OnRemoveAllButton(object sender, EventArgs e)
        {
            RemoveAllScientists();
        }

        /// <summary>React to user pressing the Less Scientists button</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void OnLessButton(object sender, EventArgs e)
        {
            RemoveScientist();
        }

        /// <summary>React to user pressing the Close button</summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void OnCloseButton(object sender, EventArgs e)
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
            int? tag = GetSelectedTag();
            if (tag.HasValue)
            {
                // can only add scientist if we have one that's idle
                if (0 < idleScientists.Count)
                {
                    Debug.Assert((0 < count) && (count <= idleScientists.Count));
                    int rowNum = tag.Value;
                    ProjectLineItem project = lineItems[rowNum].GetProject();

                    // update lineItems in case we've promoted from TopicLine to ProjectLine
                    lineItems[rowNum] = project;

                    // add specified number of idle scientists to the project
                    for (int i = 0; i < count; ++i)
                    {
                        project.AddWorker(idleScientists);
                    }
                    UpdateDetails(rowNum, project);

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
            int? tag = GetSelectedTag();
            if (tag.HasValue)
            {
                int rowNum = tag.Value;
                LineItem lineItem = lineItems[rowNum];
                lineItem.RemoveWorker(idleScientists);
                UpdateDetails(rowNum, lineItem);
            }
        }

        /// <summary>
        /// Remove all scientists from the currently selected project
        /// </summary>
        private void RemoveAllScientists()
        {
            int? tag = GetSelectedTag();
            if (tag.HasValue)
            {
                int rowNum = tag.Value;
                LineItem lineItem = lineItems[rowNum];
                while (0 < lineItem.NumWorkers)
                {
                    lineItem.RemoveWorker(idleScientists);
                }
                UpdateDetails(rowNum, lineItem);
            }
        }

        /// <summary>
        /// Update the screen to reflect the latest changes
        /// </summary>
        /// <param name="rowNum">row in grid that is selected</param>
        /// <param name="lineItem">LineItem associated with this row</param>
        private void UpdateDetails(int rowNum, LineItem lineItem)
        {
            availableText.Text = MakeIdleScientistsString();

            // update row in grid
            int row = grid.GetRowIndexByTag(rowNum);
            grid.SetCell(row, 1, lineItem.DisplayNumWorkers);
            grid.SetCell(row, 2, lineItem.Eta);
        }

        /// <summary>
        /// Remove any rows from the grid that can no longer be researched
        /// </summary>
        private void RemoveUnavailableTopics()
        {
            List<int> tagsToRemove = new List<int>();
            foreach (var kvp in lineItems)
            {
                if (!kvp.Value.CanResearch)
                {
                    tagsToRemove.Add(kvp.Key);
                }
            }
            foreach (int tag in tagsToRemove)
            {
                int rowIndex = grid.GetRowIndexByTag(tag);
                if (rowIndex >= 0)
                {
                    grid.RemoveRow(rowIndex);
                }
                lineItems.Remove(tag);
            }
        }

        /// <summary>
        /// Close this screen and go back to the Geoscape screen
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:DisposeObjectsBeforeLosingScope",
            Justification = "FxCop False Positive")]
        private static void GoToGeoscapeScreen()
        {
            ScreenManager.ScheduleScreen(new GeoscapeScreen());
        }

        // Get currently selected item from Grid.  Give error message if nothing selected
        private int? GetSelectedTag()
        {
            if (grid.SelectedRow == null)
            {
                Util.ShowMessageBox(Strings.MSGBOX_NO_PROJECT_SELECTED);
                return null;
            }
            return (int)grid.GetSelectedTag();
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

        private sealed class ProjectLineItem : LineItem
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
            public void AddWorker(List<Person> idle)
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

        private sealed class TopicLineItem : LineItem
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
