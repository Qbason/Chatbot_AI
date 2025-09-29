using MediatR;

namespace ChatbotAIService.Features.Messages.Commands
{
    public class StreamChatCommand : IRequest<IAsyncEnumerable<string>>
    {
        public string Message { get; set; } = string.Empty;
        public int ConversationId { get; set; }
    }
}