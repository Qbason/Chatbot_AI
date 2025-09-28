using MediatR;
using Microsoft.EntityFrameworkCore;
using ChatbotAIService.Models;
using ChatbotAIService.Features.Messages.Commands;

namespace ChatbotAIService.Features.Messages.Handlers
{
    public class UpdateMessageHandler : IRequestHandler<UpdateMessageCommand, bool>
    {
        private readonly ConversationContext _context;

        public UpdateMessageHandler(ConversationContext context)
        {
            _context = context;
        }

        public async Task<bool> Handle(UpdateMessageCommand request, CancellationToken cancellationToken)
        {
            var message = await _context.Messages.FirstOrDefaultAsync(m => m.Id == request.MessageId, cancellationToken);
            
            if (message == null)
                return false;

            message.Content = request.Content;
            await _context.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}