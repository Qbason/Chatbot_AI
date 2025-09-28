using MediatR;
using ChatbotAIService.Models;

namespace ChatbotAIService.Features.Conversations.Commands
{
    public class UpdateConversationStatusCommand : IRequest<bool>
    {
        public int ConversationId { get; set; }
        public ConversationStatus Status { get; set; }
    }
}