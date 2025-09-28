using MediatR;
using Microsoft.EntityFrameworkCore;
using ChatbotAIService.Models;
using ChatbotAIService.Features.Conversations.Queries;
using ChatbotAIService.Services;

namespace ChatbotAIService.Features.Conversations.Handlers
{
    public class GetConversationByIdHandler : IRequestHandler<GetConversationByIdQuery, Conversation?>
    {
        private readonly ConversationContext _context;
        private readonly ICurrentUserService _currentUserService;

        public GetConversationByIdHandler(ConversationContext context, ICurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        public async Task<Conversation?> Handle(GetConversationByIdQuery request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.GetUserId();

            return await _context.Conversations
                .Where(c => c.UserId == userId && c.Id == request.Id)
                .Include(c => c.Messages)
                .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);
        }
    }
}