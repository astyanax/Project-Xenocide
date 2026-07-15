using System.Reflection;
using System.Runtime.CompilerServices;

using ProjectXenocide.Model;
using ProjectXenocide.Model.StaticData;
using ProjectXenocide.Model.StaticData.Facilities;
using ProjectXenocide.Model.StaticData.Items;
using ProjectXenocide.Model.StaticData.Research;

namespace Xenocide.Test.MonoGame;

public class ResearchValidationTests
{
    private static readonly XNetEntryCollection EmptyXNet = CreateEmptyXNet();
    private static readonly FacilityInfoCollection EmptyFacilities = CreateEmptyFacilities();
    private static readonly ItemCollection EmptyItems = CreateEmptyItems();

    private static XNetEntryCollection CreateEmptyXNet()
    {
        var xnet = (XNetEntryCollection)RuntimeHelpers.GetUninitializedObject(typeof(XNetEntryCollection));
        SetField(xnet, "entries", new List<XNetEntry>());
        return xnet;
    }

    private static FacilityInfoCollection CreateEmptyFacilities()
    {
        var facilities = (FacilityInfoCollection)RuntimeHelpers.GetUninitializedObject(typeof(FacilityInfoCollection));
        SetField(facilities, "facilities", new SortedList<string, FacilityInfo>());
        return facilities;
    }

    private static ItemCollection CreateEmptyItems()
    {
        var items = (ItemCollection)RuntimeHelpers.GetUninitializedObject(typeof(ItemCollection));
        SetField(items, "items", new SortedList<string, ItemInfo>());
        return items;
    }

    [Fact]
    public void ValidTree_NoErrors()
    {
        var graph = BuildValidGraph();
        var errors = ResearchValidator.Validate(graph, EmptyXNet, EmptyFacilities, EmptyItems);
        Assert.Empty(errors);
    }

    [Fact]
    public void CycleDetected_ThreeNodeCycle()
    {
        var graph = BuildCycleGraph();
        var errors = ResearchValidator.Validate(graph, EmptyXNet, EmptyFacilities, EmptyItems);
        Assert.Contains(errors, e => e.Contains("RES_TOPIC_A -> RES_TOPIC_B -> RES_TOPIC_A"));
    }

    [Fact]
    public void UnreachableTopic_Detected()
    {
        var graph = BuildGraphWithUnreachable();
        var errors = ResearchValidator.Validate(graph, EmptyXNet, EmptyFacilities, EmptyItems);
        Assert.Contains(errors, e => e.Contains("RES_UNREACHABLE") && e.Contains("unreachable"));
    }

    [Fact]
    public void PrereqReferencesNonexistent_Detected()
    {
        var graph = BuildGraphWithFakePrereq();
        var errors = ResearchValidator.Validate(graph, EmptyXNet, EmptyFacilities, EmptyItems);
        Assert.Contains(errors, e => e.Contains("RES_NONEXISTENT_PREREQ"));
    }

    [Fact]
    public void GrantReferencesNonexistentItem_Detected()
    {
        var graph = BuildGraphWithFakeGrantItem();
        var errors = ResearchValidator.Validate(graph, EmptyXNet, EmptyFacilities, EmptyItems);
        Assert.Contains(errors, e => e.Contains("NONEXISTENT_ITEM") && e.Contains("grants"));
    }

    [Fact]
    public void GrantReferencesNonexistentFacility_Detected()
    {
        var graph = BuildGraphWithFakeGrantFacility();
        var errors = ResearchValidator.Validate(graph, EmptyXNet, EmptyFacilities, EmptyItems);
        Assert.Contains(errors, e => e.Contains("NONEXISTENT_FACILITY") && e.Contains("grants"));
    }

    [Fact]
    public void EmptyGraph_NoErrors()
    {
        var graph = BuildGraphWithStartingTechOnly();
        var errors = ResearchValidator.Validate(graph, EmptyXNet, EmptyFacilities, EmptyItems);
        Assert.Empty(errors);
    }

    [Fact]
    public void SelfReferencingPrerequisite_Detected()
    {
        var graph = BuildSelfReferenceGraph();
        var errors = ResearchValidator.Validate(graph, EmptyXNet, EmptyFacilities, EmptyItems);
        Assert.Contains(errors, e => e.Contains("RES_SELF_REF") && e.Contains(" -> RES_SELF_REF"));
    }

    private static ResearchGraph BuildValidGraph()
    {
        var graph = NewGraph();
        var topics = GetTopics(graph);
        var startTechField = typeof(ResearchGraph).GetField("startingTech",
            BindingFlags.NonPublic | BindingFlags.Instance)!;

        var startTech = CreateTopic("RES_STARTING_TECHNOLOGY", 0, null, null);
        var topicA = CreateTopic("RES_TOPIC_A", 10, "RES_STARTING_TECHNOLOGY", null);
        var topicB = CreateTopic("RES_TOPIC_B", 20, "RES_TOPIC_A", null);

        startTechField.SetValue(graph, startTech);
        topics["RES_TOPIC_A"] = topicA;
        topics["RES_TOPIC_B"] = topicB;

        return graph;
    }

    private static ResearchGraph BuildCycleGraph()
    {
        var graph = NewGraph();
        var topics = GetTopics(graph);
        var startTechField = typeof(ResearchGraph).GetField("startingTech",
            BindingFlags.NonPublic | BindingFlags.Instance)!;

        var startTech = CreateTopic("RES_STARTING_TECHNOLOGY", 0, null, null);
        var topicA = CreateTopic("RES_TOPIC_A", 10, "RES_TOPIC_B", null);
        var topicB = CreateTopic("RES_TOPIC_B", 10, "RES_TOPIC_A", null);

        startTechField.SetValue(graph, startTech);
        topics["RES_TOPIC_A"] = topicA;
        topics["RES_TOPIC_B"] = topicB;

        return graph;
    }

    private static ResearchGraph BuildGraphWithUnreachable()
    {
        var graph = NewGraph();
        var topics = GetTopics(graph);
        var startTechField = typeof(ResearchGraph).GetField("startingTech",
            BindingFlags.NonPublic | BindingFlags.Instance)!;

        var startTech = CreateTopic("RES_STARTING_TECHNOLOGY", 0, null, null);
        var unreachable = CreateTopic("RES_UNREACHABLE", 20, "RES_IMPOSSIBLE_PREREQ", null);

        startTechField.SetValue(graph, startTech);
        topics["RES_UNREACHABLE"] = unreachable;

        return graph;
    }

    private static ResearchGraph BuildGraphWithFakePrereq()
    {
        var graph = NewGraph();
        var topics = GetTopics(graph);
        var startTechField = typeof(ResearchGraph).GetField("startingTech",
            BindingFlags.NonPublic | BindingFlags.Instance)!;

        var startTech = CreateTopic("RES_STARTING_TECHNOLOGY", 0, null, null);
        var topic = CreateTopic("RES_NONEXISTENT_PREREQ", 10, "RES_NONEXISTENT", null);

        startTechField.SetValue(graph, startTech);
        topics["RES_NONEXISTENT_PREREQ"] = topic;

        return graph;
    }

    private static ResearchGraph BuildGraphWithFakeGrantItem()
    {
        var graph = NewGraph();
        var topics = GetTopics(graph);
        var startTechField = typeof(ResearchGraph).GetField("startingTech",
            BindingFlags.NonPublic | BindingFlags.Instance)!;

        var rewards = new List<Technology> { new Technology("NONEXISTENT_ITEM", TechnologyType.Item) };
        var startTech = CreateTopic("RES_STARTING_TECHNOLOGY", 0, null, null);
        var topic = CreateTopic("RES_FAKE_GRANT_ITEM", 10, "RES_STARTING_TECHNOLOGY", rewards);

        startTechField.SetValue(graph, startTech);
        topics["RES_FAKE_GRANT_ITEM"] = topic;

        return graph;
    }

    private static ResearchGraph BuildGraphWithFakeGrantFacility()
    {
        var graph = NewGraph();
        var topics = GetTopics(graph);
        var startTechField = typeof(ResearchGraph).GetField("startingTech",
            BindingFlags.NonPublic | BindingFlags.Instance)!;

        var rewards = new List<Technology> { new Technology("NONEXISTENT_FACILITY", TechnologyType.Facility) };
        var startTech = CreateTopic("RES_STARTING_TECHNOLOGY", 0, null, null);
        var topic = CreateTopic("RES_FAKE_GRANT_FACILITY", 10, "RES_STARTING_TECHNOLOGY", rewards);

        startTechField.SetValue(graph, startTech);
        topics["RES_FAKE_GRANT_FACILITY"] = topic;

        return graph;
    }

    private static ResearchGraph BuildGraphWithStartingTechOnly()
    {
        var graph = NewGraph();
        var startTechField = typeof(ResearchGraph).GetField("startingTech",
            BindingFlags.NonPublic | BindingFlags.Instance)!;

        var startTech = CreateTopic("RES_STARTING_TECHNOLOGY", 0, null, null);
        startTechField.SetValue(graph, startTech);

        return graph;
    }

    private static ResearchGraph BuildSelfReferenceGraph()
    {
        var graph = NewGraph();
        var topics = GetTopics(graph);
        var startTechField = typeof(ResearchGraph).GetField("startingTech",
            BindingFlags.NonPublic | BindingFlags.Instance)!;

        var startTech = CreateTopic("RES_STARTING_TECHNOLOGY", 0, null, null);
        var selfRef = CreateTopic("RES_SELF_REF", 10, "RES_SELF_REF", null);

        startTechField.SetValue(graph, startTech);
        topics["RES_SELF_REF"] = selfRef;

        return graph;
    }

    private static ResearchGraph NewGraph()
    {
        var graph = (ResearchGraph)RuntimeHelpers.GetUninitializedObject(typeof(ResearchGraph));
        SetField(graph, "topics", new Dictionary<string, ResearchTopic>());
        return graph;
    }

    private static System.Collections.IDictionary GetTopics(ResearchGraph graph)
    {
        var topicsField = typeof(ResearchGraph).GetField("topics",
            BindingFlags.NonPublic | BindingFlags.Instance)!;
        return (System.Collections.IDictionary)topicsField.GetValue(graph)!;
    }

    private static ResearchTopic CreateTopic(string id, int days,
        string? prereqTopicId, List<Technology>? rewards)
    {
        var topic = (ResearchTopic)RuntimeHelpers.GetUninitializedObject(typeof(ResearchTopic));

        SetField(topic, "id", id);
        SetField(topic, "name", id);
        SetField(topic, "days", days);

        MultiPrerequisite prereq;
        if (prereqTopicId != null)
        {
            prereq = CreateMultiPrerequisiteForTopic(prereqTopicId);
        }
        else
        {
            prereq = CreateEmptyMultiPrerequisite();
        }
        SetField(topic, "prerequisite", prereq);

        ResearchReward reward;
        if (rewards != null)
        {
            reward = CreateResearchReward(rewards);
        }
        else
        {
            reward = CreateEmptyResearchReward();
        }
        SetField(topic, "researchReward", reward);

        return topic;
    }

    private static MultiPrerequisite CreateMultiPrerequisiteForTopic(string topicId)
    {
        var mp = (MultiPrerequisite)RuntimeHelpers.GetUninitializedObject(typeof(MultiPrerequisite));
        SetField(mp, "allOf", true);
        SetField(mp, "preconditions", new List<Prerequisite>());

        var techPrereq = CreateTechnologyPrerequisite(topicId);
        var preconditions = (System.Collections.IList)typeof(MultiPrerequisite)
            .GetField("preconditions", BindingFlags.NonPublic | BindingFlags.Instance)!
            .GetValue(mp)!;
        preconditions.Add(techPrereq);

        return mp;
    }

    private static MultiPrerequisite CreateEmptyMultiPrerequisite()
    {
        var mp = (MultiPrerequisite)RuntimeHelpers.GetUninitializedObject(typeof(MultiPrerequisite));
        SetField(mp, "allOf", true);
        SetField(mp, "preconditions", new List<Prerequisite>());
        return mp;
    }

    private static TechnologyPrerequisite CreateTechnologyPrerequisite(string techId)
    {
        var tp = (TechnologyPrerequisite)RuntimeHelpers.GetUninitializedObject(typeof(TechnologyPrerequisite));
        var tech = new Technology(techId, TechnologyType.Research);
        SetField(tp, "requiredTech", tech);
        return tp;
    }

    private static ResearchReward CreateResearchReward(List<Technology> technologies)
    {
        var reward = (ResearchReward)RuntimeHelpers.GetUninitializedObject(typeof(ResearchReward));
        SetField(reward, "allOf", true);
        SetField(reward, "technologies", new List<Technology>());

        var techs = (System.Collections.IList)typeof(ResearchReward)
            .GetField("technologies", BindingFlags.NonPublic | BindingFlags.Instance)!
            .GetValue(reward)!;
        foreach (var t in technologies)
        {
            techs.Add(t);
        }

        return reward;
    }

    private static ResearchReward CreateEmptyResearchReward()
    {
        var reward = (ResearchReward)RuntimeHelpers.GetUninitializedObject(typeof(ResearchReward));
        SetField(reward, "allOf", true);
        SetField(reward, "technologies", new List<Technology>());
        return reward;
    }

    private static void SetField(object obj, string fieldName, object value)
    {
        var field = obj.GetType().GetField(fieldName,
            BindingFlags.NonPublic | BindingFlags.Instance);
        field?.SetValue(obj, value);
    }
}
