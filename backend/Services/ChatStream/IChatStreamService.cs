namespace ChatbotAIService.Services
{
    public interface IChatStreamService
    {
        Task<IAsyncEnumerable<string>> StartStreamAsync(
            int conversationId, 
            string message, 
            bool isResume = false, 
            CancellationToken cancellationToken = default);
    }
}