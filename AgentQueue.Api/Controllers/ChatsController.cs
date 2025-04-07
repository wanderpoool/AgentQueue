using AgentQueue.Domain.Agents;
using AgentQueue.Domain.Chats;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;

namespace AgentQueue.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatsController : ControllerBase
    {
        private static ScheduleManager _scheduleManager;
        private static int _nextSessionId = 1;

        // 10 second timeout
        private static readonly TimeSpan _inactivityTimeout = TimeSpan.FromSeconds(10);

        // polling 1 sec
        private static readonly TimeSpan _pollingInterval = TimeSpan.FromSeconds(1);

        private static bool _isOfficeHours = true;
        private static bool _backgroundTasksStarted = false;

        private static ConcurrentQueue<ChatSession> _chatQueue = new ConcurrentQueue<ChatSession>();
        private static ConcurrentQueue<ChatSession> _sessionQueue = new ConcurrentQueue<ChatSession>();

        public ChatsController(IConfiguration configuration, ScheduleManager scheduleManager)
        {
            _scheduleManager = scheduleManager;
            _isOfficeHours = configuration["IsOfficeHours"] == "True";
            if (!_backgroundTasksStarted)
            {
                Task.Run(() => MonitorQueueActivityAsync());
                Task.Run(() => AssignChatsToAgentsAsync());
                _backgroundTasksStarted = true;
            }
        }

        [HttpPost("start")]
        public IActionResult StartChat()
        {
            var newSessionId = Interlocked.Increment(ref _nextSessionId);
            var newSession = new ChatSession(newSessionId);

            if (!_isOfficeHours)
            {
                return StatusCode(499, new { Message = $"Chat refused." });
            }

            _chatQueue.Enqueue(newSession);
            Console.WriteLine($"Chat session id : {newSession.SessionId} started...");
            return Ok();
        }

        [HttpPost("poll/{sessionId}")]
        public IActionResult PollSession(int sessionId)
        {
            var chatSession = _sessionQueue.FirstOrDefault(s => s.SessionId == sessionId);

            if (chatSession == null)
            {
                var msg = $"Session Id {sessionId} not found.";
                Console.WriteLine(msg);
                return NotFound(new { Message = msg });
            }

            _scheduleManager.PollSession(chatSession);
            if (chatSession != null && chatSession.IsActive)
            {
                Console.WriteLine($"Chat session poll for {chatSession.SessionId} updated : {chatSession.LastPollTime}.");
                return Ok();
            }

            return NotFound(new { Message = "Invalid or inactive session." });
        }

        [HttpPost("finish/{sessionId}")]
        public IActionResult FinishSession(int sessionId)
        {
            var chatSession = _sessionQueue.FirstOrDefault(s => s.SessionId == sessionId);

            if (chatSession == null)
            {
                var msg = $"Session Id {sessionId} not found.";
                Console.WriteLine(msg);
                return NotFound(new { Message = msg });
            }

            try
            {
                _scheduleManager.EndSession(chatSession);
                if (chatSession != null && chatSession.IsActive)
                {
                    chatSession.EndSession();
                    Console.WriteLine($"Chat session ended for session id : {chatSession.SessionId}.");
                }
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { Message = $"{ex.Message}." });
            }

            return Ok();
        }

        private static async Task MonitorQueueActivityAsync()
        {
            while (true)
            {
                await Task.Delay(_pollingInterval);
                foreach (var session in _sessionQueue.Where(s => s.IsActive))
                {
                    if (DateTime.UtcNow - session.LastPollTime > _inactivityTimeout)
                    {
                        session.IsActive = false;
                        _scheduleManager.EndSession(new ChatSession(session.SessionId));
                        Console.WriteLine($"Chat session {session.SessionId} in queue marked as inactive.");
                    }
                }
            }
        }

        private static async Task AssignChatsToAgentsAsync()
        {
            while (true)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(100));
                while (_chatQueue.TryPeek(out var nextChatQueue))
                {
                    if (_chatQueue.TryDequeue(out var sessionToAssign))
                    {
                        try
                        {
                            _scheduleManager.AssignAgent(sessionToAssign.SessionId, DateTime.UtcNow);
                            _sessionQueue.Enqueue(sessionToAssign);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"{ex.Message}.");
                            break;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }
    }
}