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

using Gum.Forms;
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
    /// Screen for managing manufacturing projects at an outpost.
    /// </summary>
    /// <remarks>
    /// ARCHITECTURE: This screen follows the 3-layer pattern (GUI / Controller / Scene).
    /// This file contains the GUI layer only. All game logic (engineer assignment,
    /// project management, build quantity changes) is delegated to the nested Controller
    /// class in Manufacture/ManufactureScreenController.cs.
    ///
    /// DATA FLOW:
    ///   User clicks "More Engineers" → OnMoreButton → controller.AddWorkersToProject()
    ///   → UpdateDetails → projectGrid.SetCell() + ShowRequirements()
    ///
    /// GRID LAYOUT:
    ///   Project Grid (top):
    ///     Column 0: Item name (350px)
    ///     Column 1: Assigned engineers (105px)
    ///     Column 2: Build quantity (105px)
    ///     Column 3: Days to completion (105px)
    ///   Requirements Grid (bottom):
    ///     Column 0: Resource name (350px)
    ///     Column 1: Quantity needed (160px)
    ///     Column 2: Quantity available (175px)
    ///
    /// GAME MECHANICS:
    ///   - Engineers work in workshops (STORAGE_ENGINEER capacity per outpost)
    ///   - Each project requires workspace, hours, money, and materials
    ///   - Engineers reduce remaining hours; when complete, item is produced
    ///   - Build count can be adjusted (1-99); cancelling returns engineers to idle pool
    /// </remarks>
    public partial class ManufactureScreen : GumScreen
    {
        /// <summary>
        /// Constructs the manufacture screen for the given outpost.
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
            controller = new Controller(SelectedOutpost);
            ProjectMgr.Update();
            controller.FindIdleEngineers();

            if (GumRoot != null)
            {
                WireButton("buildMoreButton", OnBuildMoreButton);
                WireButton("buildLessButton", OnBuildLessButton);
                WireButton("cancelBuildButton", OnCancelBuildButton);
                WireButton("addIdleEngineersButton", OnAddIdleButton);
                WireButton("moreEngineersButton", OnMoreButton);
                WireButton("lessEngineersButton", OnLessButton);
                WireButton("removeAllEngineersButton", OnRemoveAllButton);
                WireButton("closeButton", OnCloseButton);

                availableText = new Label() { Text = controller.MakeIdleEngineersString() };
                availableText.Visual.X = 20;
                availableText.Visual.Y = 20;
                AddChild(availableText);

                InitializeGrids();
                projectGrid.Visual.X = 20;
                projectGrid.Visual.Y = 60;
                projectGrid.Visual.Width = 750;
                requirementsGrid.Visual.X = 20;
                requirementsGrid.Visual.Y = 370;
                requirementsGrid.Visual.Width = 750;
                PopulateProjectGrid();
                return;
            }

            availableText = new Label() { Text = controller.MakeIdleEngineersString() };
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
            AddChild(projectGrid.Visual);
            projectGrid.SelectionChanged += OnProjectGridSelectionChanged;

            requirementsGrid = new GridPanel();
            requirementsGrid.AddColumn(Strings.SCREEN_MANUFACTURE_COLUMN_RESOURCE, 350);
            requirementsGrid.AddColumn(Strings.SCREEN_MANUFACTURE_COLUMN_QUANTITY_NEEDED, 160);
            requirementsGrid.AddColumn(Strings.SCREEN_MANUFACTURE_COLUMN_QUANTITY_AVAILABLE, 175);
            AddChild(requirementsGrid.Visual);
        }

        private void PopulateProjectGrid()
        {
            foreach (LineItem project in controller.GetActiveProjects())
            {
                AddRowToProjectGrid(project);
            }

            foreach (LineItem item in controller.GetBuildableItems())
            {
                AddRowToProjectGrid(item);
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
                string available = controller.GetWorkspaceAvailable(buildInfo);
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
                string available = Controller.BankBalance;
                AddRowToRequirementsGrid(Strings.SCREEN_MANUFACTURE_REPORT_ROW_MONEY, needed, available);
            }

            foreach (ItemLine material in buildInfo.Materials)
            {
                string needed = Util.ToString(material.Quantity);
                string available = controller.GetMaterialAvailable(material.ItemInfo);
                AddRowToRequirementsGrid(material.ItemInfo.Name, needed, available);
            }
        }

        private void AddRowToRequirementsGrid(string resourceName, string needed, string available)
        {
            requirementsGrid.AddRow(null, resourceName, needed, available);
        }

        #endregion Create the Gum controls

        #region Event handlers

        private void OnBuildMoreButton(object sender, EventArgs e) => ChangeBuildNumber(1);
        private void OnBuildLessButton(object sender, EventArgs e) => ChangeBuildNumber(-1);
        private void OnCancelBuildButton(object sender, EventArgs e) => CancelProject();
        private void OnMoreButton(object sender, EventArgs e) => AddIdleEngineers(1);
        private void OnAddIdleButton(object sender, EventArgs e) => AddIdleEngineers(controller.IdleEngineerCount);
        private void OnRemoveAllButton(object sender, EventArgs e) => RemoveAllEngineers();
        private void OnLessButton(object sender, EventArgs e) => RemoveScientist();
        private void OnCloseButton(object sender, EventArgs e) => ShowBasesScreen();

        private void OnProjectGridSelectionChanged(object sender, EventArgs e)
        {
            LineItem lineItem = GetSelectedItem();
            if (lineItem != null)
            {
                ShowRequirements(lineItem.BuildInfo);
            }
        }

        #endregion Event handlers

        private void AddIdleEngineers(int count)
        {
            LineItem selectedLineItem = GetSelectedItem();
            if (null == selectedLineItem)
                return;

            if (0 < controller.IdleEngineerCount)
            {
                Debug.Assert((0 < count) && (count <= controller.IdleEngineerCount));
                ProjectLineItem project = selectedLineItem.GetProject();
                if (null != project)
                {
                    int oldRow = projectGrid.GetRowIndexByTag(selectedLineItem);
                    projectGrid.RemoveRow(oldRow);
                    AddRowToProjectGrid(project);

                    controller.AddWorkersToProject(project, count);
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

            controller.RemoveWorkerFromProject(lineItem);
            UpdateDetails(lineItem);
        }

        private void RemoveAllEngineers()
        {
            LineItem lineItem = GetSelectedItem();
            if (null == lineItem)
                return;

            controller.RemoveAllWorkersFromProject(lineItem);
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
                IdleLineItem newLineItem = controller.CancelProject(project);
                int oldRow = projectGrid.GetRowIndexByTag(project);
                projectGrid.RemoveRow(oldRow);
                AddRowToProjectGrid(newLineItem);
                UpdateDetails(newLineItem);
            }
        }

        private void UpdateDetails(LineItem lineItem)
        {
            availableText.Text = controller.MakeIdleEngineersString();

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

        #region Fields

        /// <summary>
        /// Controller handling all manufacturing game logic (engineer assignment, project management).
        /// </summary>
        private Controller controller;

        private BuildProjectManager ProjectMgr
            => SelectedOutpost.BuildProjectManager;

        private Outpost SelectedOutpost
            => Xenocide.GameState.GeoData.Outposts[selectedOutpostIndex];

        private int selectedOutpostIndex;

        #endregion Fields
    }
}
