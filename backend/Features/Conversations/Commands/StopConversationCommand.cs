using MediatR;

namespace ChatbotAIService.Features.Conversations.Commands
{
    public class StopConversationCommand : IRequest<bool>
    {
        public int ConversationId { get; set; }
    }
}