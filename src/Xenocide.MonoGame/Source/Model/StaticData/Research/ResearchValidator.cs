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
#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

using ProjectXenocide.Model.StaticData.Facilities;
using ProjectXenocide.Model.StaticData.Items;
using ProjectXenocide.Utils;

using Xenocide.Resources;

#endregion

namespace ProjectXenocide.Model.StaticData.Research
{
    public static class ResearchValidator
    {
        public static List<string> Validate(ResearchGraph graph, XNetEntryCollection xnetEntries,
            FacilityInfoCollection facilities, ItemCollection items)
        {
            List<string> errors = new List<string>();

            ValidateCycles(graph, errors);
            ValidatePrerequisiteReferences(graph, xnetEntries, facilities, items, errors);

            TechnologyManager mgr = SimulateFullResearch(graph, errors);
            if (mgr != null)
            {
                ValidateXNetReachability(mgr, xnetEntries, errors);
                ValidateFacilityReachability(mgr, facilities, errors);
                ValidateItemReachability(mgr, items, errors);
            }

            ValidateGrantReferences(graph, xnetEntries, facilities, items, errors);

            return errors;
        }

        private static void ValidateCycles(ResearchGraph graph, List<string> errors)
        {
            Dictionary<string, NodeColor> colors = new Dictionary<string, NodeColor>();
            Dictionary<string, string> parent = new Dictionary<string, string>();

            foreach (ResearchTopic topic in graph)
            {
                colors[topic.Id] = NodeColor.White;
            }

            foreach (ResearchTopic topic in graph)
            {
                if (colors[topic.Id] == NodeColor.White)
                {
                    DetectCycle(topic.Id, graph, colors, parent, errors);
                }
            }
        }

        private static void DetectCycle(string nodeId, ResearchGraph graph,
            Dictionary<string, NodeColor> colors, Dictionary<string, string> parent,
            List<string> errors)
        {
            colors[nodeId] = NodeColor.Gray;

            ResearchTopic topic = graph[nodeId];
            if (topic == null) return;

            List<string> prereqIds = new List<string>();
            topic.CollectPrerequisiteIds(prereqIds);

            foreach (string prereqId in prereqIds)
            {
                if (!colors.ContainsKey(prereqId))
                {
                    continue;
                }

                if (colors[prereqId] == NodeColor.Gray)
                {
                    Stack<string> cycle = new Stack<string>();
                    cycle.Push(prereqId);
                    string current = nodeId;
                    while (current != prereqId)
                    {
                        cycle.Push(current);
                        if (!parent.ContainsKey(current)) break;
                        current = parent[current];
                    }
                    cycle.Push(prereqId);

                    StringBuilder path = new StringBuilder();
                    bool first = true;
                    foreach (string id in cycle)
                    {
                        if (!first) path.Append(" -> ");
                        path.Append(id);
                        first = false;
                    }

                    errors.Add(Util.StringFormat(Strings.EXCEPTION_RESEARCH_CYCLE_DETECTED, path.ToString()));
                    return;
                }
                else if (colors[prereqId] == NodeColor.White)
                {
                    parent[prereqId] = nodeId;
                    DetectCycle(prereqId, graph, colors, parent, errors);
                }
            }

            colors[nodeId] = NodeColor.Black;
        }

        private static void ValidatePrerequisiteReferences(ResearchGraph graph,
            XNetEntryCollection xnetEntries, FacilityInfoCollection facilities, ItemCollection items,
            List<string> errors)
        {
            foreach (ResearchTopic topic in graph)
            {
                if (topic.Id == "RES_STARTING_TECHNOLOGY") continue;

                List<string> prereqIds = new List<string>();
                topic.CollectPrerequisiteIds(prereqIds);

                foreach (string prereqId in prereqIds)
                {
                    bool exists = prereqId == "RES_STARTING_TECHNOLOGY"
                        || GraphContainsTopic(graph, prereqId)
                        || xnetEntries.FindById(prereqId) != null
                        || ItemCollectionContains(items, prereqId)
                        || FacilityCollectionContains(facilities, prereqId);

                    if (!exists)
                    {
                        errors.Add(Util.StringFormat(Strings.EXCEPTION_PREREQ_NONEXISTENT_TOPIC,
                            topic.Id, prereqId));
                    }
                }
            }
        }

        private static bool GraphContainsTopic(ResearchGraph graph, string id)
        {
            try
            {
                return graph[id] != null;
            }
            catch (KeyNotFoundException)
            {
                return false;
            }
        }

        private static bool ItemCollectionContains(ItemCollection items, string id)
        {
            try
            {
                return items[id] != null;
            }
            catch (KeyNotFoundException)
            {
                return false;
            }
        }

        private static bool FacilityCollectionContains(FacilityInfoCollection facilities, string id)
        {
            try
            {
                return facilities[id] != null;
            }
            catch (KeyNotFoundException)
            {
                return false;
            }
        }

        private static TechnologyManager SimulateFullResearch(ResearchGraph graph, List<string> errors)
        {
            Dictionary<string, bool> solved = new Dictionary<string, bool>();
            TechnologyManager mgr = new TechnologyManager(graph);

            graph.GiveStartingTech(mgr);

            bool anySolved;
            do
            {
                anySolved = false;
                foreach (ResearchTopic topic in graph)
                {
                    if (solved.ContainsKey(topic.Id)) continue;

                    if (topic.IsSatisfied(mgr))
                    {
                        solved[topic.Id] = true;
                        anySolved = true;
                        while (topic.IsRewardLeft(mgr))
                        {
                            topic.GrantReward(mgr);
                        }
                    }
                }
            }
            while (anySolved);

            foreach (ResearchTopic topic in graph)
            {
                if (!solved.ContainsKey(topic.Id))
                {
                    List<string> failedPrereqs = new List<string>();
                    MultiPrerequisite prereq = GetPrerequisite(topic);
                    if (prereq != null)
                    {
                        prereq.CollectFailedIds(mgr, failedPrereqs);
                    }
                    string detail = failedPrereqs.Count > 0
                        ? " (missing: " + string.Join(", ", failedPrereqs) + ")"
                        : "";
                    errors.Add(Util.StringFormat(Strings.EXCEPTION_UNREACHABLE_RESEARCH_TOPIC + detail, topic.Id));
                }
            }

            return mgr;
        }

        private static MultiPrerequisite GetPrerequisite(ResearchTopic topic)
        {
            System.Reflection.FieldInfo field = typeof(ResearchTopic).GetField("prerequisite",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            return field?.GetValue(topic) as MultiPrerequisite;
        }

        private static void ValidateXNetReachability(TechnologyManager mgr,
            XNetEntryCollection xnetEntries, List<string> errors)
        {
            foreach (XNetEntry entry in xnetEntries)
            {
                if (!mgr.IsAvailable(entry.Id))
                {
                    errors.Add(Util.StringFormat(Strings.EXCEPTION_UNREACHABLE_XNET_ENTRY, entry.Name));
                }
            }

            foreach (Technology tech in mgr)
            {
                if (tech.Type == TechnologyType.XNet && xnetEntries.FindById(tech.Id) == null)
                {
                    errors.Add(Util.StringFormat(Strings.EXCEPTION_MISSING_XNET_ENTRY, tech.Id));
                }
            }
        }

        private static void ValidateFacilityReachability(TechnologyManager mgr,
            FacilityInfoCollection facilities, List<string> errors)
        {
            foreach (FacilityInfo facility in facilities)
            {
                if (!mgr.IsAvailable(facility.Id))
                {
                    errors.Add(Util.StringFormat(Strings.EXCEPTION_UNREACHABLE_FACILITY, facility.Name));
                }
            }

            foreach (Technology tech in mgr)
            {
                if (tech.Type == TechnologyType.Facility && !FacilityCollectionContains(facilities, tech.Id))
                {
                    errors.Add(Util.StringFormat(Strings.EXCEPTION_MISSING_FACILITY, tech.Id));
                }
            }
        }

        private static void ValidateItemReachability(TechnologyManager mgr,
            ItemCollection items, List<string> errors)
        {
            foreach (ItemInfo item in items)
            {
                if (!mgr.IsAvailable(item.Id))
                {
                    if (item.CanPurchase || item.BuildInfo != null)
                    {
                        errors.Add(Util.StringFormat(Strings.EXCEPTION_UNREACHABLE_ITEM, item.Name));
                    }
                }
            }

            foreach (Technology tech in mgr)
            {
                if (tech.Type == TechnologyType.Item && !ItemCollectionContains(items, tech.Id))
                {
                    errors.Add(Util.StringFormat(Strings.EXCEPTION_MISSING_ITEM, tech.Id));
                }
            }
        }

        private static void ValidateGrantReferences(ResearchGraph graph,
            XNetEntryCollection xnetEntries, FacilityInfoCollection facilities,
            ItemCollection items, List<string> errors)
        {
            foreach (ResearchTopic topic in graph)
            {
                if (topic.Id == "RES_STARTING_TECHNOLOGY") continue;

                foreach (Technology tech in topic.GetRewardTechnologies())
                {
                    switch (tech.Type)
                    {
                        case TechnologyType.Item:
                            if (!ItemCollectionContains(items, tech.Id))
                            {
                                errors.Add(Util.StringFormat(Strings.EXCEPTION_GRANT_NONEXISTENT_ITEM,
                                    topic.Id, tech.Id));
                            }
                            break;
                        case TechnologyType.Facility:
                            if (!FacilityCollectionContains(facilities, tech.Id))
                            {
                                errors.Add(Util.StringFormat(Strings.EXCEPTION_GRANT_NONEXISTENT_FACILITY,
                                    topic.Id, tech.Id));
                            }
                            break;
                        case TechnologyType.XNet:
                            if (xnetEntries.FindById(tech.Id) == null)
                            {
                                errors.Add(Util.StringFormat(Strings.EXCEPTION_GRANT_NONEXISTENT_XNET,
                                    topic.Id, tech.Id));
                            }
                            break;
                    }
                }
            }
        }

        private enum NodeColor
        {
            White,
            Gray,
            Black
        }
    }
}
