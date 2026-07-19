using System;
using System.Collections.Generic;

using ProjectXenocide.Model;
using ProjectXenocide.Model.Geoscape;
using ProjectXenocide.Model.Geoscape.Outposts;
using ProjectXenocide.Model.StaticData.Research;
using ProjectXenocide.Utils;

using Xenocide.Resources;

namespace ProjectXenocide.UI.Screens
{
    public partial class ResearchScreen
    {
        /// <summary>
        /// Handles all research game logic: scientist assignment, project management,
        /// and topic validation. Separated from the GUI layer to enable unit testing
        /// of business rules without MonoGame/Gum dependencies.
        /// </summary>
        /// <remarks>
        /// ARCHITECTURE: This controller owns the idle scientists list and all game state
        /// mutations (adding/removing workers from projects). The Screen class delegates
        /// to this controller for all business logic and updates GUI based on results.
        ///
        /// The controller accesses game state through static shortcuts (ProjectMgr, TechMgr,
        /// Outposts) which are defined on the enclosing Screen class via partial class.
        ///
        /// DATA MODEL: LineItem, ProjectLineItem, and TopicLineItem are nested here because
        /// they represent research data for grid display — they query model state (names,
        /// worker counts, ETAs, can-research flags) without any GUI knowledge.
        /// </remarks>
        private class Controller
        {
            /// <summary>
            /// Scientists that currently are not working, but could work in available lab space.
            /// </summary>
            private readonly List<Person> idleScientists = new List<Person>();

            /// <summary>
            /// Finds all scientists across all X-Corp outposts that are not assigned to research
            /// but have available lab space to work in.
            /// </summary>
            /// <remarks>
            /// Game mechanic: Each outpost has a STORAGE_SCIENTIST capacity. Scientists beyond
            /// that capacity are "idle" and can be assigned to research projects. This method
            /// iterates all outposts, counts available lab slots, and collects the first N
            /// scientists (where N = available slots) from each outpost's staff list.
            /// </remarks>
            public void FindIdleScientists()
            {
                idleScientists.Clear();
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
            /// Assigns idle scientists to a research project.
            /// </summary>
            /// <param name="project">The project to assign scientists to</param>
            /// <param name="count">Number of scientists to assign</param>
            /// <remarks>
            /// Game mechanic: Each scientist assigned to a project reduces the remaining
            /// work units. When all work is complete, the research is finished and rewards
            /// are granted. Scientists can be reassigned at any time.
            /// </remarks>
            public void AddWorkersToProject(ProjectLineItem project, int count)
            {
                for (int i = 0; i < count; ++i)
                {
                    project.AddWorker(idleScientists);
                }
            }

            /// <summary>
            /// Removes a single scientist from a research project, returning them to the idle pool.
            /// </summary>
            public void RemoveWorkerFromProject(LineItem lineItem)
            {
                lineItem.RemoveWorker(idleScientists);
            }

            /// <summary>
            /// Removes all scientists from a research project, returning them to the idle pool.
            /// </summary>
            public void RemoveAllWorkersFromProject(LineItem lineItem)
            {
                while (0 < lineItem.NumWorkers)
                {
                    lineItem.RemoveWorker(idleScientists);
                }
            }

            /// <summary>
            /// Formats a display string showing the number of idle scientists.
            /// </summary>
            public string MakeIdleScientistsString()
            {
                return Util.StringFormat(Strings.SCREEN_RESEARCH_IDLE_SCIENTISTS, idleScientists.Count);
            }

            /// <summary>
            /// Gets the current count of idle scientists.
            /// </summary>
            public int IdleScientistCount => idleScientists.Count;

            /// <summary>
            /// Returns the list of startable research topics, filtering out topics already in progress.
            /// </summary>
            public static List<TopicLineItem> GetStartableTopics()
            {
                var result = new List<TopicLineItem>();
                ResearchGraph graph = Xenocide.StaticTables.ResearchGraph;
                foreach (ResearchTopic topic in graph.StartableTopics(TechMgr, Outposts))
                {
                    if (!ProjectMgr.IsInProgress(topic.Id))
                    {
                        result.Add(new TopicLineItem(topic, ProjectMgr, TechMgr, Outposts));
                    }
                }
                return result;
            }

            /// <summary>
            /// Returns LineItems for all active research projects.
            /// </summary>
            public static List<ProjectLineItem> GetActiveProjects()
            {
                var result = new List<ProjectLineItem>();
                foreach (ResearchProject project in ProjectMgr)
                {
                    result.Add(new ProjectLineItem(project));
                }
                return result;
            }

            #region Static shortcuts to game state

            private static ResearchProjectManager ProjectMgr
                => Xenocide.GameState.GeoData.XCorp.ResearchManager;

            private static TechnologyManager TechMgr
                => Xenocide.GameState.GeoData.XCorp.TechManager;

            private static ICollection<Outpost> Outposts
                => Xenocide.GameState.GeoData.Outposts;

            #endregion
        }

        /// <summary>
        /// Represents a single row in the research grid. Abstract base provides
        /// display properties (Name, Workers, ETA) and mutation methods (AddWorker,
        /// RemoveWorker). Subclasses provide project-specific or topic-specific behavior.
        /// </summary>
        /// <remarks>
        /// Game mechanic: The research grid shows active projects at the top, followed by
        /// available topics. Each row tracks either an active project (with assigned workers)
        /// or a startable topic (with no workers yet). When a worker is added to a topic,
        /// it becomes a project via ProjectMgr.CreateProject().
        /// </remarks>
        private abstract class LineItem
        {
            /// <summary>Display name for the Name column.</summary>
            public abstract string Name { get; }

            /// <summary>Display value for the Assigned Scientists column.</summary>
            public virtual string DisplayNumWorkers => String.Empty;

            /// <summary>Number of workers currently assigned.</summary>
            public virtual int NumWorkers => 0;

            /// <summary>Display value for the Days Left column.</summary>
            public virtual string Eta => String.Empty;

            /// <summary>Whether this topic can still be researched (prerequisites met).</summary>
            public virtual bool CanResearch => true;

            /// <summary>Removes a worker from this item, returning them to the idle pool.</summary>
            public virtual void RemoveWorker(IList<Person> idle) { }

            /// <summary>
            /// Gets the project line item, creating a new project if this is a topic.
            /// </summary>
            public abstract ProjectLineItem GetProject();
        }

        /// <summary>
        /// Line item for an active research project (has assigned workers and progress).
        /// </summary>
        private sealed class ProjectLineItem : LineItem
        {
            public ProjectLineItem(ResearchProject project) { this.project = project; }

            /// <summary>Adds a scientist from the idle pool to this project.</summary>
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

            public override string Name => project.Name;
            public override string DisplayNumWorkers => Util.ToString(NumWorkers);
            public override int NumWorkers => project.NumWorkers;
            public override string Eta => project.CalcEtaToShow();

            private readonly ResearchProject project;
        }

        /// <summary>
        /// Line item for a startable research topic (no workers, waiting to be started).
        /// </summary>
        private sealed class TopicLineItem : LineItem
        {
            public TopicLineItem(ResearchTopic topic, ResearchProjectManager projectMgr,
                TechnologyManager techMgr, ICollection<Outpost> outposts)
            {
                this.topic = topic;
                this.projectMgr = projectMgr;
                this.techMgr = techMgr;
                this.outposts = outposts;
            }

            public override ProjectLineItem GetProject()
            {
                return new ProjectLineItem(projectMgr.CreateProject(topic.Id, techMgr, outposts));
            }

            public override string Name => topic.Name;
            public override bool CanResearch => topic.CanResearch(techMgr, outposts);

            private readonly ResearchTopic topic;
            private readonly ResearchProjectManager projectMgr;
            private readonly TechnologyManager techMgr;
            private readonly ICollection<Outpost> outposts;
        }
    }
}
