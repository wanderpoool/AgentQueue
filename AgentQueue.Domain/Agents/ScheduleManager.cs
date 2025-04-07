using AgentQueue.Domain.Chats;
using AgentQueue.Domain.Commons;

namespace AgentQueue.Domain.Agents;

public class ScheduleManager
{
    private IDictionary<Shift, Queue<IAgent>> _teams = new Dictionary<Shift, Queue<IAgent>>();
    private const decimal CapacityMultiplier = 1.5m;

    public void CreateTeam(List<IAgent> agents, Shift shift)
    {
        _teams.Add(shift, new Queue<IAgent>(agents.OrderBy(c => c.Level)));
    }

    public IEnumerable<ChatSession> GetActiveSession()
    {
        var activeAgent = _teams.Select(c => c.Value)
            .SelectMany(c => c)
            .Where(c => c.GetActiveSessionIds().Any())
            .Select(c => c)
            .ToList();
        var activeSession = activeAgent.SelectMany(c => c.GetActiveSessionIds().Select(c => new ChatSession(c)));
        return activeSession;
    }

    public void PollSession(ChatSession chatSession)
    {
        if (chatSession == null)
        {
            throw new ArgumentNullException(nameof(chatSession), "Chat session cannot be null");
        }

        bool isActive = _teams.Values.Any(queue => queue.Any(agent => agent.IsSessionActive(chatSession.SessionId)));
        if (isActive)
        {
            chatSession.UpdateLastPollTime();
        }
        else
        {
            chatSession.EndSession();
        }
    }

    public IAgent AssignAgent(int sessionId, DateTime assignmentDate)
    {
        Shift? shift = Shift.GetShiftBySchedule(assignmentDate);

        if (shift == null)
        {
            throw new InvalidOperationException("No agents available");
        }

        IAgent? currentAgent = GetAvailableAgent(shift);

        if (currentAgent is null)
        {
            currentAgent = GetAvailableAgent(Shift.Overflow);
        }

        if (currentAgent is null)
        {
            throw new InvalidOperationException("No agents available to assign");
        }

        currentAgent.AssignChat(sessionId);

        return currentAgent;
    }

    public void EndSession(ChatSession chatSession)
    {
        var agent = _teams.Values.SelectMany(queue => queue).FirstOrDefault(a => a.IsSessionActive(chatSession.SessionId));

        if (agent == null)
        {
            throw new InvalidOperationException("Session ID not found");
        }

        agent.FinishChat(chatSession.SessionId);
    }

    private IAgent? GetAvailableAgent(Shift shift)
    {
        if (NoAvailableAgentsByShift(shift))
        {
            return null;
        }

        foreach (var agent in _teams[shift])
        {
            if (agent.CanAcceptNewChat())
            {
                return agent;
            }
        }

        return null;
    }

    private bool NoAvailableAgentsByShift(Shift shift)
    {
        return _teams[shift].Count == 0;
    }
}