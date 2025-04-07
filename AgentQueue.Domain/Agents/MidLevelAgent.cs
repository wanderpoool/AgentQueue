namespace AgentQueue.Domain.Agents;
public class MidLevelAgent : Agent
{
    public override decimal Effiency { get => 0.6m; }

    public override int Level => 2;
}
