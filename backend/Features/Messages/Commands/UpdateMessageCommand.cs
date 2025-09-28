using MediatR;

namespace ChatbotAIService.Features.Messages.Commands
{
    public class UpdateMessageCommand : IRequest<bool>
    {
        public int MessageId { get; set; }
        public string Content { get; set; } = string.Empty;
    }
}