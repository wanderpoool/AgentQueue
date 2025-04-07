namespace AgentQueue.Domain.Agents;

public class TeamLead : Agent
{
    public override decimal Effiency { get => 0.5m; }

    public override int Level => 4;
}
