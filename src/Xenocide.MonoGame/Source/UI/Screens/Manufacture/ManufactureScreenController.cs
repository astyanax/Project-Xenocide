using System;
using System.Collections.Generic;

using ProjectXenocide.Model;
using ProjectXenocide.Model.Geoscape;
using ProjectXenocide.Model.Geoscape.Outposts;
using ProjectXenocide.Model.StaticData;
using ProjectXenocide.Model.StaticData.Items;
using ProjectXenocide.Model.StaticData.Research;
using ProjectXenocide.Utils;

using Xenocide.Resources;

namespace ProjectXenocide.UI.Screens
{
    public partial class ManufactureScreen
    {
        /// <summary>
        /// Handles all manufacturing game logic: engineer assignment, project management,
        /// build quantity changes, and project cancellation. Separated from the GUI layer
        /// to enable unit testing of business rules without MonoGame/Gum dependencies.
        /// </summary>
        /// <remarks>
        /// ARCHITECTURE: This controller owns the idle engineers list and all game state
        /// mutations (adding/removing engineers, creating/cancelling projects). The Screen
        /// class delegates to this controller for all business logic and updates GUI
        /// based on results.
        ///
        /// DATA MODEL: LineItem, ProjectLineItem, and IdleLineItem are nested here because
        /// they represent manufacturing data for grid display. They reference the controller
        /// (not the screen) to break the circular dependency.
        ///
        /// GAME MECHANICS:
        /// - Engineers work in workshops (STORAGE_ENGINEER capacity per outpost)
        /// - Each project requires workspace, hours, money, and materials
        /// - Engineers reduce remaining hours; when complete, item is produced
        /// - Build count can be adjusted (1-99); cancelling returns engineers to idle pool
        /// </remarks>
        private class Controller
        {
            private readonly Outpost outpost;

            public Controller(Outpost outpost)
            {
                this.outpost = outpost;
            }

            /// <summary>
            /// Engineers that currently are not working, but could work in available workshop space.
            /// </summary>
            private readonly List<Person> idleEngineers = new List<Person>();

            /// <summary>
            /// Finds idle engineers at the selected outpost (those with available workshop space).
            /// </summary>
            public void FindIdleEngineers()
            {
                idleEngineers.Clear();
                uint spaceFree = outpost.Statistics.Capacities["STORAGE_ENGINEER"].Available;
                int count = -1;
                foreach (Person p in outpost.Inventory.ListStaff("ITEM_PERSON_ENGINEER", false))
                {
                    if (++count < spaceFree)
                    {
                        idleEngineers.Add(p);
                    }
                }
            }

            /// <summary>Number of idle engineers available for assignment.</summary>
            public int IdleEngineerCount => idleEngineers.Count;

            /// <summary>
            /// Formats a display string showing the number of idle engineers.
            /// </summary>
            public string MakeIdleEngineersString()
            {
                return Util.StringFormat(Strings.SCREEN_MANUFACTURE_IDLE_ENGINEERS, idleEngineers.Count);
            }

            /// <summary>
            /// Assigns idle engineers to a manufacturing project.
            /// </summary>
            public void AddWorkersToProject(ProjectLineItem project, int count)
            {
                count = Math.Min(count, idleEngineers.Count);
                for (int i = 0; i < count; ++i)
                {
                    project.AddWorker(idleEngineers);
                }
            }

            /// <summary>
            /// Removes a single engineer from a project, returning them to the idle pool.
            /// </summary>
            public void RemoveWorkerFromProject(LineItem lineItem)
            {
                lineItem.RemoveWorker(idleEngineers);
            }

            /// <summary>
            /// Removes all engineers from a project, returning them to the idle pool.
            /// </summary>
            public void RemoveAllWorkersFromProject(LineItem lineItem)
            {
                while (0 < lineItem.NumWorkers)
                {
                    lineItem.RemoveWorker(idleEngineers);
                }
            }

            /// <summary>
            /// Creates a new manufacturing project for the given item.
            /// Returns null if the item cannot be built (validation error shown via message box).
            /// </summary>
            public ProjectLineItem CreateProject(ItemInfo item)
            {
                string error = item.CanStartManufacture(TechMgr, outpost, Bank);
                if (null != error)
                {
                    Util.ShowMessageBox(Strings.MSGBOX_CANT_BUILD_ITEM, item.Name, error);
                    return null;
                }

                var project = ProjectMgr.CreateProject(item.Id, TechMgr, outpost, Bank);
                FindIdleEngineers();
                return new ProjectLineItem(this, project);
            }

            /// <summary>
            /// Cancels a manufacturing project and returns its engineers to the idle pool.
            /// </summary>
            public IdleLineItem CancelProject(ProjectLineItem project)
            {
                project.Cancel();
                FindIdleEngineers();
                return new IdleLineItem(this, project.Item);
            }

            /// <summary>
            /// Returns LineItems for all active build projects.
            /// </summary>
            public List<LineItem> GetActiveProjects()
            {
                var result = new List<LineItem>();
                foreach (BuildProject project in ProjectMgr)
                {
                    result.Add(new ProjectLineItem(this, project));
                }
                return result;
            }

            /// <summary>
            /// Returns LineItems for all items that can be built (not already in progress).
            /// </summary>
            public List<LineItem> GetBuildableItems()
            {
                var result = new List<LineItem>();
                foreach (ItemInfo item in Xenocide.StaticTables.ItemList)
                {
                    if ((null != item.BuildInfo) && TechMgr.IsAvailable(item.Id) && !ProjectMgr.IsInProgress(item.Id))
                    {
                        result.Add(new IdleLineItem(this, item));
                    }
                }
                return result;
            }

            /// <summary>
            /// Checks whether the outpost has workspace available for a given build info.
            /// </summary>
            public string GetWorkspaceAvailable(BuildInfo buildInfo)
            {
                return Util.ToString((int)BuildInfo.GetCapacityInfo(outpost).Available);
            }

            /// <summary>
            /// Checks whether the outpost has the required materials for a build info.
            /// </summary>
            public string GetMaterialAvailable(ItemInfo materialItem)
            {
                return Util.ToString(outpost.Inventory.NumberInInventory(materialItem));
            }

            /// <summary>
            /// Gets the current bank balance display string.
            /// </summary>
            public static string BankBalance => Bank.DisplayCurrentBalance;

            #region Static shortcuts

            private static BuildProjectManager ProjectMgr
                => Xenocide.GameState.GeoData.Outposts[0].BuildProjectManager; // will be replaced per-outpost

            private static TechnologyManager TechMgr
                => Xenocide.GameState.GeoData.XCorp.TechManager;

            private static Bank Bank
                => Xenocide.GameState.GeoData.XCorp.Bank;

            #endregion
        }

        /// <summary>
        /// Represents a single row in the manufacturing grid. Abstract base provides
        /// display properties and worker management. Subclasses provide project-specific
        /// or idle-item-specific behavior.
        /// </summary>
        private abstract class LineItem
        {
            protected LineItem(Controller controller) { this.controller = controller; }

            public abstract string Name { get; }
            public virtual string DisplayNumWorkers => String.Empty;
            public virtual int NumWorkers => 0;
            public virtual string DisplayQuantity => String.Empty;
            public virtual int Quantity => 0;
            public virtual string Eta => String.Empty;
            public abstract BuildInfo BuildInfo { get; }
            public virtual void RemoveWorker(IList<Person> idle) { }
            public abstract ProjectLineItem GetProject();

            /// <summary>Controller handling game logic for this line item.</summary>
            protected Controller Controller => controller;
            private Controller controller;
        }

        /// <summary>
        /// Line item for an active manufacturing project (has assigned engineers and progress).
        /// </summary>
        private sealed class ProjectLineItem : LineItem
        {
            public ProjectLineItem(Controller controller, BuildProject project)
                : base(controller)
            {
                this.project = project;
            }

            public void AddWorker(List<Person> idle)
            {
                Person worker = idle[idle.Count - 1];
                idle.RemoveAt(idle.Count - 1);
                project.Add(worker);
            }

            public override void RemoveWorker(IList<Person> idle)
            {
                if (0 < project.NumWorkers)
                    idle.Add(project.RemoveWorker());
            }

            public override ProjectLineItem GetProject() => this;

            public void Cancel() => project.Cancel();

            public override string Name => project.Name;
            public override string DisplayNumWorkers => Util.ToString(NumWorkers);
            public override int NumWorkers => project.NumWorkers;
            public override string DisplayQuantity => Util.ToString(project.BuildCount);
            public int BuildCount { get => project.BuildCount; set => project.BuildCount = value; }
            public override string Eta => project.CalcTotalItemsEtaToShow();
            public override BuildInfo BuildInfo => Item.BuildInfo;
            public ItemInfo Item => project.Item;

            private readonly BuildProject project;
        }

        /// <summary>
        /// Line item for an item that can be manufactured (not yet in progress).
        /// </summary>
        private sealed class IdleLineItem : LineItem
        {
            public IdleLineItem(Controller controller, ItemInfo item)
                : base(controller)
            {
                this.item = item;
            }

            public override ProjectLineItem GetProject()
                => Controller.CreateProject(item);

            public override string Name => item.Name;
            public override BuildInfo BuildInfo => item.BuildInfo;

            private readonly ItemInfo item;
        }
    }
}
