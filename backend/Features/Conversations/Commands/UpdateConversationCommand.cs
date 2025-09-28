using MediatR;

namespace ChatbotAIService.Features.Conversations.Commands
{
    public class UpdateConversationCommand : IRequest<bool>
    {
        public int Id { get; set; }
        public required string Title { get; set; }
    }
}