using MediatR;
using ChatbotAIService.Models;

namespace ChatbotAIService.Features.Conversations.Commands
{
    public class CreateConversationCommand : IRequest<Conversation>
    {
        public required string Title { get; set; }
    }
}