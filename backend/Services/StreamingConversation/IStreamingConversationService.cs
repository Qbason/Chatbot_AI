namespace ChatbotAIService.Services
{
    public interface IStreamingConversationService
    {
        void RegisterStreamingConversation(int conversationId, CancellationTokenSource tokenSource);
        bool TryStopStreamingConversation(int conversationId);
        void UnregisterStreamingConversation(int conversationId);
        bool IsConversationStreaming(int conversationId);
    }
}