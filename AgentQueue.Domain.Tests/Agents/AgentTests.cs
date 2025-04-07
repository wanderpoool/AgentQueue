using AgentQueue.Domain.Agents;

namespace AgentQueue.Domain.Tests.Agents;

[TestFixture]
public class AgentTests
{
    public static IEnumerable<Agent> Agents => new List<Agent>
    {
        new JuniorAgent(),
        new MidLevelAgent(),
        new SeniorAgent(),
    };

    private static IDictionary<string, decimal> AgentScenarios => new Dictionary<string, decimal>()
    {
        { nameof(JuniorAgent), 4m },
        { nameof(MidLevelAgent), 6m },
        { nameof(SeniorAgent), 8m },
        { nameof(TeamLead), 5m }
    };

    [TestCaseSource(nameof(Agents))]
    public void GetAgentCapacityAndEffiencyTests(Agent agent)
    {
        const int divisor = 10;
        var agentCapacity = AgentScenarios[agent.GetType().Name];
        var capacity = agent.GetCapacity();
        var effiency = agentCapacity / divisor;

        Assert.That(capacity, Is.EqualTo(agentCapacity));
        Assert.That(agent.Effiency, Is.EqualTo(effiency));
    }

    [Test]
    public void ThrowInvalidOperationException()
    {
        var agent = new FakeAgent();
        Assert.Throws<InvalidOperationException>(() => agent.GetCapacity());
    }
}

internal class FakeAgent : Agent
{
    public override decimal Effiency { get => -0.3m; }

    public override int Level => 6;
}