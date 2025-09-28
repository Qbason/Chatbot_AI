using MediatR;
using Microsoft.EntityFrameworkCore;
using ChatbotAIService.Models;
using ChatbotAIService.Features.Conversations.Commands;
using ChatbotAIService.Services;

namespace ChatbotAIService.Features.Conversations.Handlers
{
    public class UpdateConversationHandler : IRequestHandler<UpdateConversationCommand, bool>
    {
        private readonly ConversationContext _context;
        private readonly ICurrentUserService _currentUserService;

        public UpdateConversationHandler(ConversationContext context, ICurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        public async Task<bool> Handle(UpdateConversationCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.GetUserId();
            
            var conversation = await _context.Conversations
                .FirstOrDefaultAsync(c => c.Id == request.Id && c.UserId == userId, cancellationToken);

            if (conversation == null)
                return false;

            conversation.Title = request.Title;

            try
            {
                await _context.SaveChangesAsync(cancellationToken);
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await ConversationExists(request.Id, cancellationToken))
                    return false;
                
                throw;
            }
        }

        private async Task<bool> ConversationExists(int id, CancellationToken cancellationToken)
        {
            return await _context.Conversations.AnyAsync(e => e.Id == id, cancellationToken);
        }
    }
}