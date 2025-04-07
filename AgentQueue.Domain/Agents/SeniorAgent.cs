namespace AgentQueue.Domain.Agents;

public class SeniorAgent : Agent
{
    public override decimal Effiency { get => 0.8m; }

    public override int Level => 3;
}
