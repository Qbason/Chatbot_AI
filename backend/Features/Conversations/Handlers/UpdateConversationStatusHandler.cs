using MediatR;
using Microsoft.EntityFrameworkCore;
using ChatbotAIService.Models;
using ChatbotAIService.Features.Conversations.Commands;
using ChatbotAIService.Services;

namespace ChatbotAIService.Features.Conversations.Handlers
{
    public class UpdateConversationStatusHandler : IRequestHandler<UpdateConversationStatusCommand, bool>
    {
        private readonly ConversationContext _context;
        private readonly ICurrentUserService _currentUserService;

        public UpdateConversationStatusHandler(ConversationContext context, ICurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        public async Task<bool> Handle(UpdateConversationStatusCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.GetUserId();

            var conversation = await _context.Conversations
                .FirstOrDefaultAsync(c => c.Id == request.ConversationId && c.UserId == userId, cancellationToken);

            if (conversation == null)
                return false;

            conversation.Status = request.Status;
            await _context.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}