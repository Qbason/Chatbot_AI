using MediatR;

namespace ChatbotAIService.Features.Conversations.Commands
{
    public class DeleteConversationCommand : IRequest<bool>
    {
        public int Id { get; set; }
    }
}