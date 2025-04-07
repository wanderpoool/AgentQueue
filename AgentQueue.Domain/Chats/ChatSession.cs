namespace AgentQueue.Domain.Chats;

public class ChatSession
{
    public int SessionId { get; }
    public DateTime EnqueuedTime { get; }
    public DateTime LastPollTime { get; private set; }
    public bool IsActive { get; set; } = true;
    public int? AssignedAgentId { get; set; }

    public ChatSession(int sessionId)
    {
        if (sessionId < 0)
            throw new ArgumentOutOfRangeException(nameof(sessionId), "Session ID must be greater than zero.");

        SessionId = sessionId;
        EnqueuedTime = DateTime.UtcNow;
        LastPollTime = DateTime.UtcNow;
    }

    public void UpdateLastPollTime()
    {
        LastPollTime = DateTime.UtcNow;
        IsActive = true;
    }

    public void EndSession()
    {
        IsActive = false;
    }
}