using MediatR;
using Microsoft.EntityFrameworkCore;
using ChatbotAIService.Models;
using ChatbotAIService.Features.Conversations.Queries;
using ChatbotAIService.Services;

namespace ChatbotAIService.Features.Conversations.Handlers
{
    public class GetLastAIMessageHandler : IRequestHandler<GetLastAIMessageQuery, Message?>
    {
        private readonly ConversationContext _context;
        private readonly ICurrentUserService _currentUserService;

        public GetLastAIMessageHandler(ConversationContext context, ICurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        public async Task<Message?> Handle(GetLastAIMessageQuery request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.GetUserId();

            return await _context.Messages
                .Where(m => m.Conversation.UserId == userId
                 && m.ConversationId == request.ConversationId
                 && m.Role == Role.AI)
                .OrderByDescending(m => m.Timestamp)
                .FirstOrDefaultAsync(cancellationToken);
        }
    }
}
