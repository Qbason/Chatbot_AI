using System.Collections.Concurrent;

//! THIS IS FINE BUT ONLY FOR SINGLE INSTANCE DEPLOYMENTS.
//! FOR MULTI-INSTANCE, NEED TO MOVE TO SHARED CACHE LIKE REDIS, VALKEY
//! OR HAVE REVERSE PROXY WHICH FORWARD REQUEST TO THE SAME INSTANCE

namespace ChatbotAIService.Services
{
    public class StreamingConversationService : IStreamingConversationService
    {
        private readonly ConcurrentDictionary<int, CancellationTokenSource> _streamingConversations = new();
        private readonly ILogger<StreamingConversationService> _logger;

        public StreamingConversationService(ILogger<StreamingConversationService> logger)
        {
            _logger = logger;
        }

        public void RegisterStreamingConversation(int conversationId, CancellationTokenSource tokenSource)
        {
            if (_streamingConversations.TryAdd(conversationId, tokenSource))
            {
                _logger.LogInformation("Registered streaming conversation {ConversationId}", conversationId);
            }
            else
            {
                if (_streamingConversations.TryGetValue(conversationId, out var existingToken))
                {
                    existingToken.Cancel();
                    existingToken.Dispose();
                }

                _streamingConversations[conversationId] = tokenSource;
                _logger.LogWarning("Replaced existing streaming conversation {ConversationId}", conversationId);
            }
        }

        public bool TryStopStreamingConversation(int conversationId)
        {
            if (_streamingConversations.TryRemove(conversationId, out var tokenSource))
            {
                try
                {
                    tokenSource.Cancel();
                    _logger.LogInformation("Stopped streaming conversation {ConversationId}", conversationId);
                    return true;
                }
                catch (ObjectDisposedException)
                {
                    _logger.LogWarning("Attempted to cancel already disposed token for conversation {ConversationId}", conversationId);
                    return true;
                }
                finally
                {
                    tokenSource.Dispose();
                }
            }

            _logger.LogWarning("Attempted to stop non-streaming conversation {ConversationId}", conversationId);
            return false;
        }

        public void UnregisterStreamingConversation(int conversationId)
        {
            if (_streamingConversations.TryRemove(conversationId, out var tokenSource))
            {
                try
                {
                    tokenSource.Dispose();
                    _logger.LogInformation("Unregistered streaming conversation {ConversationId}", conversationId);
                }
                catch (ObjectDisposedException)
                {
                    _logger.LogDebug("Token for conversation {ConversationId} was already disposed", conversationId);
                }
            }
        }

        public bool IsConversationStreaming(int conversationId)
        {
            return _streamingConversations.ContainsKey(conversationId);
        }
    }
}