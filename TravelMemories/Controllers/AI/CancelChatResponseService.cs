using System.Collections.Concurrent;

namespace TravelMemories.Controllers.AI
{
    public class CancelChatResponseService
    {
        public readonly ConcurrentDictionary<Guid, CancellationTokenSource> _sessions = new();

        public CancellationToken Register(Guid conversationId)
        {
            _sessions[conversationId] = new CancellationTokenSource();
            return _sessions[conversationId].Token;
        }

        public void Cancel(Guid conversationId)
        {
            if (_sessions.TryRemove(conversationId, out var cts))
            {
                cts.Cancel();
                cts.Dispose();
            }
        }
    }
}
