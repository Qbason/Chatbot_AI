using MediatR;
using ChatbotAIService.Models;

namespace ChatbotAIService.Features.Messages.Commands
{
    public class AddMessageFeedbackCommand : IRequest<bool>
    {
        public int ConversationId { get; set; }
        public int MessageId { get; set; }
        public Rating Rating { get; set; }
    }
}