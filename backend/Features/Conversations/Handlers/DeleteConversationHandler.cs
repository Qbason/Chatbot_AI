using MediatR;
using Microsoft.EntityFrameworkCore;
using ChatbotAIService.Models;
using ChatbotAIService.Features.Conversations.Commands;
using ChatbotAIService.Services;

namespace ChatbotAIService.Features.Conversations.Handlers
{
    public class DeleteConversationHandler : IRequestHandler<DeleteConversationCommand, bool>
    {
        private readonly ConversationContext _context;
        private readonly ICurrentUserService _currentUserService;

        public DeleteConversationHandler(ConversationContext context, ICurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        public async Task<bool> Handle(DeleteConversationCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.GetUserId();

            var conversation = await _context.Conversations
                .FirstOrDefaultAsync(c => c.Id == request.Id && c.UserId == userId, cancellationToken);

            if (conversation == null)
                return false;

            _context.Conversations.Remove(conversation);
            await _context.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}