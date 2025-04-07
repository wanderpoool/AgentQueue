namespace AgentQueue.Domain.Agents;
public abstract class Agent : IAgent
{
    private const int MaxConcurrency = 10;
    public abstract decimal Effiency { get; }

    public abstract int Level { get; }

    private int CurrentActiveChats { get; set; }
    private List<int> ActiveSessionId { get; set; } = new List<int>();

    protected Agent()
    {
        CurrentActiveChats = 0;
    }

    public void AssignChat(int sessionId)
    {
        if (!CanAcceptNewChat())
        {
            throw new InvalidOperationException("Agent is at full capacity");
        }

        CurrentActiveChats++;
        ActiveSessionId.Add(sessionId);
    }

    public IEnumerable<int> GetActiveSessionIds()
    {
        return ActiveSessionId;
    }

    public bool IsSessionActive(int sessionId)
    {
        return ActiveSessionId.Contains(sessionId);
    }

    public int GetCapacity()
    {
        if (Effiency < 0)
        {
            throw new InvalidOperationException("Invalid Effiency");
        }

        return (int) Math.Floor(Effiency * MaxConcurrency);
    }

    public void FinishChat(int sessionId)
    {
        if (ActiveSessionId.Contains(sessionId))
        {
            CurrentActiveChats--;
            ActiveSessionId.Remove(sessionId);
        }
        else
        {
            throw new InvalidOperationException("Session ID not found");
        }
    }

    public bool CanAcceptNewChat()
    {
        return CurrentActiveChats < GetCapacity();
    }

}


