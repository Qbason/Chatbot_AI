using MediatR;
using ChatbotAIService.Models;

namespace ChatbotAIService.Features.Conversations.Queries
{
    public class GetLastAIMessageQuery : IRequest<Message>
    {
        public int ConversationId { get; set; }
    }
}