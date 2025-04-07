namespace AgentQueue.Domain.Agents;

public interface IAgent
{
    int GetCapacity();

    void AssignChat(int sessionId);

    void FinishChat(int sessionId);

    int Level { get; }

    bool CanAcceptNewChat();

    bool IsSessionActive(int sessionId);

    IEnumerable<int> GetActiveSessionIds();
}