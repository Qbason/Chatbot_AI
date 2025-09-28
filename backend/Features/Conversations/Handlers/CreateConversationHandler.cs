using MediatR;
using ChatbotAIService.Models;
using ChatbotAIService.Features.Conversations.Commands;
using ChatbotAIService.Services;

namespace ChatbotAIService.Features.Conversations.Handlers
{
    public class CreateConversationHandler : IRequestHandler<CreateConversationCommand, Conversation>
    {
        private readonly ConversationContext _context;
        private readonly ICurrentUserService _currentUserService;

        public CreateConversationHandler(ConversationContext context, ICurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        public async Task<Conversation> Handle(CreateConversationCommand request, CancellationToken cancellationToken)
        {
            var conversation = new Conversation
            {
                Title = request.Title,
                UserId = _currentUserService.GetUserId(),
                Timestamp = DateTime.UtcNow
            };

            _context.Conversations.Add(conversation);
            await _context.SaveChangesAsync(cancellationToken);

            return conversation;
        }
    }
}