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
    /// Screen for managing research projects and assigning scientists to research topics.
    /// </summary>
    /// <remarks>
    /// ARCHITECTURE: This screen follows the 3-layer pattern (GUI / Controller / Scene).
    /// This file contains the GUI layer only. All game logic (scientist assignment,
    /// project management, topic validation) is delegated to the nested Controller class
    /// in Research/ResearchScreenController.cs.
    ///
    /// DATA FLOW:
    ///   User clicks "Add Scientist" → OnAddIdleButton → controller.AddWorkersToProject()
    ///   → RefreshGrid → controller.GetActiveProjects() / controller.GetStartableTopics()
    ///   → grid rows updated
    ///
    /// GRID LAYOUT:
    ///   Column 0: Project/Topic name (50% width)
    ///   Column 1: Assigned scientists count (25% width)
    ///   Column 2: Days to completion / ETA (22% width)
    ///   Rows are keyed by integer tags (lineItems dictionary) for stable identity
    ///   across grid rebuilds.
    /// </remarks>
    public partial class ResearchScreen : GumScreen
    {
        /// <summary>
        /// Constructs the research screen.
        /// </summary>
        public ResearchScreen()
            : base("Research")
        {
        }

        #region Create the Gum controls

        /// <summary>
        /// Initializes the Gum controls, controller, and populates the research grid.
        /// </summary>
        protected override void CreateGumControls()
        {
            controller = new Controller();
            controller.FindIdleScientists();

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
                availableText.Text = controller.MakeIdleScientistsString();

                InitializeGrid();
                grid.Visual.X = 20;
                grid.Visual.Y = 110;
                grid.Visual.Width = 800;
                PopulateGrid();
                return;
            }

            availableText = new Label();
            RootContainer.AddChild(availableText);
            availableText.Text = controller.MakeIdleScientistsString();

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
        /// Creates the grid panel with column headers for the research display.
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
        /// Populates the grid with active projects and startable topics from the controller.
        /// </summary>
        private void PopulateGrid()
        {
            foreach (ProjectLineItem project in Controller.GetActiveProjects())
            {
                AddRowToGrid(project);
            }

            foreach (TopicLineItem topic in Controller.GetStartableTopics())
            {
                AddRowToGrid(topic);
            }
        }

        /// <summary>
        /// Adds a single row to the research grid.
        /// </summary>
        private void AddRowToGrid(LineItem lineItem)
        {
            int rowNum = grid.RowCount;
            grid.AddRow(rowNum, lineItem.Name, lineItem.DisplayNumWorkers, lineItem.Eta);
            lineItems[rowNum] = lineItem;
        }

        #endregion Create the Gum controls

        #region Event handlers

        private void OnMoreButton(object sender, EventArgs e)
        {
            AddIdleScientists(1);
        }

        private void OnAddIdleButton(object sender, EventArgs e)
        {
            AddIdleScientists(controller.IdleScientistCount);
        }

        private void OnRemoveAllButton(object sender, EventArgs e)
        {
            RemoveAllScientists();
        }

        private void OnLessButton(object sender, EventArgs e)
        {
            RemoveScientist();
        }

        private void OnCloseButton(object sender, EventArgs e)
        {
            GoToGeoscapeScreen();
        }

        #endregion Event handlers

        /// <summary>
        /// Adds scientists to the currently selected project via the controller,
        /// then refreshes the GUI.
        /// </summary>
        private void AddIdleScientists(int count)
        {
            int? tag = GetSelectedTag();
            if (tag.HasValue)
            {
                if (0 < controller.IdleScientistCount)
                {
                    Debug.Assert((0 < count) && (count <= controller.IdleScientistCount));
                    int rowNum = tag.Value;
                    ProjectLineItem project = lineItems[rowNum].GetProject();
                    lineItems[rowNum] = project;

                    controller.AddWorkersToProject(project, count);
                    UpdateDetails(rowNum, project);
                    RemoveUnavailableTopics();
                }
                else
                {
                    Util.ShowMessageBox(Strings.MSGBOX_NO_IDLE_SCIENTISTS);
                }
            }
        }

        /// <summary>
        /// Removes a single scientist from the currently selected project.
        /// </summary>
        private void RemoveScientist()
        {
            int? tag = GetSelectedTag();
            if (tag.HasValue)
            {
                int rowNum = tag.Value;
                LineItem lineItem = lineItems[rowNum];
                controller.RemoveWorkerFromProject(lineItem);
                UpdateDetails(rowNum, lineItem);
            }
        }

        /// <summary>
        /// Removes all scientists from the currently selected project.
        /// </summary>
        private void RemoveAllScientists()
        {
            int? tag = GetSelectedTag();
            if (tag.HasValue)
            {
                int rowNum = tag.Value;
                LineItem lineItem = lineItems[rowNum];
                controller.RemoveAllWorkersFromProject(lineItem);
                UpdateDetails(rowNum, lineItem);
            }
        }

        /// <summary>
        /// Updates the grid row and idle scientist display after a change.
        /// </summary>
        private void UpdateDetails(int rowNum, LineItem lineItem)
        {
            availableText.Text = controller.MakeIdleScientistsString();
            int row = grid.GetRowIndexByTag(rowNum);
            grid.SetCell(row, 1, lineItem.DisplayNumWorkers);
            grid.SetCell(row, 2, lineItem.Eta);
        }

        /// <summary>
        /// Removes grid rows for topics that can no longer be researched
        /// (e.g., prerequisites no longer met after using an artifact).
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

        private static void GoToGeoscapeScreen()
        {
            ScreenManager.ScheduleScreen(new GeoscapeScreen());
        }

        private int? GetSelectedTag()
        {
            if (grid.SelectedRow == null)
            {
                Util.ShowMessageBox(Strings.MSGBOX_NO_PROJECT_SELECTED);
                return null;
            }
            return (int)grid.GetSelectedTag();
        }

        #region Fields

        /// <summary>
        /// Controller handling all research game logic (scientist assignment, project management).
        /// </summary>
        private Controller controller;

        /// <summary>
        /// Maps grid row tags to LineItem data objects for stable identity across rebuilds.
        /// </summary>
        private Dictionary<int, LineItem> lineItems = new Dictionary<int, LineItem>();

        #endregion Fields
    }
}
