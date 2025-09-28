using MediatR;
using Microsoft.EntityFrameworkCore;
using ChatbotAIService.Models;
using ChatbotAIService.Features.Conversations.Queries;
using ChatbotAIService.Services;

namespace ChatbotAIService.Features.Conversations.Handlers
{
    public class GetConversationStreamStatusHandler : IRequestHandler<GetConversationStreamStatusQuery, ConversationStatus?>
    {
        private readonly ConversationContext _context;
        private readonly ICurrentUserService _currentUserService;

        public GetConversationStreamStatusHandler(ConversationContext context, ICurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        public async Task<ConversationStatus?> Handle(GetConversationStreamStatusQuery request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.GetUserId();

            var conversation = await _context.Conversations
                .FirstOrDefaultAsync(c => c.Id == request.ConversationId && c.UserId == userId, cancellationToken);

            return conversation?.Status;
        }
    }
}