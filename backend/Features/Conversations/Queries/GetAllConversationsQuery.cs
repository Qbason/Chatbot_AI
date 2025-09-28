using MediatR;
using ChatbotAIService.Models;

namespace ChatbotAIService.Features.Conversations.Queries
{
    public class GetAllConversationsQuery : IRequest<IEnumerable<Conversation>>
    {
    }
}