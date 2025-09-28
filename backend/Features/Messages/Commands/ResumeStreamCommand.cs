using MediatR;

namespace ChatbotAIService.Features.Messages.Commands
{
    public class ResumeStreamCommand : IRequest<IAsyncEnumerable<string>>
    {
        public int ConversationId { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}