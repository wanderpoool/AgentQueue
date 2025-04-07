using AgentQueue.Domain.Agents;
using AgentQueue.Domain.Commons;

namespace AgentQueue.Domain.Tests.Agents;

[TestFixture]
public class ScheduleManagerTests
{
    private ScheduleManager _scheduleManager;

    private static IDictionary<Shift, List<IAgent>> AgentTeams => new Dictionary<Shift, List<IAgent>>()
    {
        { Shift.Morning,  new List<IAgent>{ new TeamLead(), new MidLevelAgent(), new MidLevelAgent(),new JuniorAgent() } },
        { Shift.Afternoon, new List<IAgent> { new SeniorAgent(), new MidLevelAgent(), new JuniorAgent(), new JuniorAgent() } },
        { Shift.Night, new List<IAgent> { new MidLevelAgent(), new MidLevelAgent() } },
        { Shift.Overflow, new List<IAgent> { new JuniorAgent(), new JuniorAgent(), new JuniorAgent(), new JuniorAgent(), new JuniorAgent(), new JuniorAgent() } }
    };

    public ScheduleManagerTests()
    {
        CreateSheduleManager();
    }

    private void CreateSheduleManager()
    {
        _scheduleManager = new ScheduleManager();
        _scheduleManager.CreateTeam(AgentTeams[Shift.Morning], Shift.Morning);
        _scheduleManager.CreateTeam(AgentTeams[Shift.Afternoon], Shift.Afternoon);
        _scheduleManager.CreateTeam(AgentTeams[Shift.Night], Shift.Night);
        _scheduleManager.CreateTeam(AgentTeams[Shift.Overflow], Shift.Overflow);
    }

    private static readonly Shift[] Shifts = { Shift.Morning, Shift.Afternoon, Shift.Night};

    [TestCaseSource(nameof(Shifts))]
    public void CreateTeamTests(Shift shift)
    {
        CreateSheduleManager();
        var team = AgentTeams[shift];
        var maxCapacity = team.Sum(c => c.GetCapacity());
        var scheduleManager = new ScheduleManager();
        scheduleManager.CreateTeam(team, shift);
        for (int i = 0; i < maxCapacity; i++)
        {
            scheduleManager.AssignAgent(i, new DateTime(2025, 04, 05, shift.Time.Start.Hour, 
                shift.Time.Start.Minute, 
                shift.Time.Start.Second));
        }

        var teamActiveCapacity = scheduleManager.GetActiveSession();
        Assert.That(maxCapacity, Is.EqualTo(teamActiveCapacity.Count()));
    }

    [TestCase(21, "2025-04-05T08:00:00", 4, 12, 0, 5)]
    [TestCase(15, "2025-04-05T15:00:00", 8, 6, 1, 0)]
    [TestCase(12, "2025-04-05T02:00:00", 0, 12, 0, 0)]
    public void AssignChatsTests(int numberOfSession, DateTime request,
        int juniorSessionCount, int midLevelSessionCount,
        int seniorSessionCount, int teamLeadSessionCount)
    {
        CreateSheduleManager();
        List<IAgent> agents = new List<IAgent>();
        for (int i = 0; i < numberOfSession; i++)
        {
            agents.Add(_scheduleManager.AssignAgent(i, request));
        }

        var teamLeadSession = agents.Count(c => c is TeamLead);
        var seniorSession = agents.Count(c => c is SeniorAgent);
        var midLevelSession = agents.Count(c => c is MidLevelAgent);
        var juniorSession = agents.Count(c => c is JuniorAgent);

        Assert.That(agents.Count, Is.EqualTo(numberOfSession));
        Assert.That(juniorSession, Is.EqualTo(juniorSessionCount));
        Assert.That(midLevelSession, Is.EqualTo(midLevelSessionCount));
        Assert.That(seniorSession, Is.EqualTo(seniorSessionCount));
        Assert.That(teamLeadSession, Is.EqualTo(teamLeadSessionCount));
    }

    [TestCase(21, "2025-04-05T08:00:00", 4, 12, 0, 5)]
    public void ReAssignChat_For_MideLevelAgent_After_Ending_Session(int numberOfSession, DateTime request,
        int juniorSessionCount, int midLevelSessionCount,
        int seniorSessionCount, int teamLeadSessionCount)
    {
        CreateSheduleManager();
        List<IAgent> agents = new List<IAgent>();
        for (int i = 0; i < numberOfSession; i++)
        {
            agents.Add(_scheduleManager.AssignAgent(i, request));
        }

        _scheduleManager.EndSession(new Chats.ChatSession(12));
        var agent = _scheduleManager.AssignAgent(numberOfSession, request);

        Assert.That(agent, Is.TypeOf<MidLevelAgent>());
    }

    [TestCase(15, "2025-04-05T15:00:00", 8, 6, 1, 0)]
    [TestCase(22, "2025-04-05T15:00:00", 15, 0, 7, 0)]
    public void AssignChats_For_Overflowshift_Tests(int numberOfSession, DateTime request,
        int juniorSessionCount, int midLevelSessionCount,
        int seniorSessionCount, int teamLeadSessionCount)
    {
        List<IAgent> agents = new List<IAgent>();
        for (int i = 0; i < numberOfSession; i++)
        {
            agents.Add(_scheduleManager.AssignAgent(i, request));
        }

        var teamLeadSession = agents.Count(c => c is TeamLead);
        var seniorSession = agents.Count(c => c is SeniorAgent);
        var midLevelSession = agents.Count(c => c is MidLevelAgent);
        var juniorSession = agents.Count(c => c is JuniorAgent);

        Assert.That(agents.Count, Is.EqualTo(numberOfSession));
        Assert.That(juniorSession, Is.EqualTo(juniorSessionCount));
        Assert.That(midLevelSession, Is.EqualTo(midLevelSessionCount));
        Assert.That(seniorSession, Is.EqualTo(seniorSessionCount));
        Assert.That(teamLeadSession, Is.EqualTo(teamLeadSessionCount));
    }

    [TestCase(45, "2025-04-05T08:00:00")]
    [TestCase(46, "2025-04-05T15:00:00")]
    [TestCase(36, "2025-04-05T23:00:00")]
    public void AssignChats_No_Available_Agent_Tests_Per_Shift(int maxCapacity, DateTime shift)
    {
        CreateSheduleManager();
        List<IAgent> agents = new List<IAgent>();
        for (int i = 0; i < maxCapacity; i++)
        {
            agents.Add(_scheduleManager.AssignAgent(i, shift));
        }

        var result = Assert.Throws<InvalidOperationException>(() =>
        {
            agents.Add(_scheduleManager.AssignAgent(23, shift));
        });

        Assert.That(result.Message, Is.EqualTo("No agents available to assign"));
    }
}