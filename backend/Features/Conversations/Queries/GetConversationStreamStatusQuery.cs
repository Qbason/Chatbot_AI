using MediatR;
using ChatbotAIService.Models;

namespace ChatbotAIService.Features.Conversations.Queries
{
    public class GetConversationStreamStatusQuery : IRequest<ConversationStatus?>
    {
        public int ConversationId { get; set; }

    }
}