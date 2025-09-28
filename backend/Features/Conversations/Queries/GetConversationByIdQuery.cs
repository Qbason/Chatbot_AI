using MediatR;
using ChatbotAIService.Models;

namespace ChatbotAIService.Features.Conversations.Queries
{
    public class GetConversationByIdQuery : IRequest<Conversation?>
    {
        public int Id { get; set; }

    }
}