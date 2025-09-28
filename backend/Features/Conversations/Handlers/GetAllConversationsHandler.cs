using MediatR;
using Microsoft.EntityFrameworkCore;
using ChatbotAIService.Models;
using ChatbotAIService.Features.Conversations.Queries;
using ChatbotAIService.Services;

namespace ChatbotAIService.Features.Conversations.Handlers
{
    public class GetAllConversationsHandler : IRequestHandler<GetAllConversationsQuery, IEnumerable<Conversation>>
    {
        private readonly ConversationContext _context;
        private readonly ICurrentUserService _currentUserService;

        public GetAllConversationsHandler(ConversationContext context, ICurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        public async Task<IEnumerable<Conversation>> Handle(GetAllConversationsQuery request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.GetUserId();

            return await _context.Conversations
                .Where(c => c.UserId == userId)
                .Select(c => new Conversation
                {
                    Id = c.Id,
                    Title = c.Title,
                    UserId = c.UserId,
                    Timestamp = c.Timestamp,
                    Status = c.Status,
                    Messages = new List<Message>()
                })
                .ToListAsync(cancellationToken);
        }
    }
}