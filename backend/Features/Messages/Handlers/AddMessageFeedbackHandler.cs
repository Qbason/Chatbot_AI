using MediatR;
using Microsoft.EntityFrameworkCore;
using ChatbotAIService.Models;
using ChatbotAIService.Features.Messages.Commands;
using ChatbotAIService.Services;

namespace ChatbotAIService.Features.Messages.Handlers
{
    public class AddMessageFeedbackHandler : IRequestHandler<AddMessageFeedbackCommand, bool>
    {
        private readonly ConversationContext _context;
        private readonly ICurrentUserService _currentUserService;

        public AddMessageFeedbackHandler(ConversationContext context, ICurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        public async Task<bool> Handle(AddMessageFeedbackCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.GetUserId();

            var message = await _context.Messages
                .FirstOrDefaultAsync(m => m.Id == request.MessageId
                    && m.ConversationId == request.ConversationId
                    && m.Conversation != null
                    && m.Conversation.UserId == userId,
                    cancellationToken);

            if (message == null)
                return false;

            var existingRating = await _context.MessageRatings
                .FirstOrDefaultAsync(r => r.MessageId == request.MessageId, cancellationToken);

            if (existingRating != null)
            {
                existingRating.Value = request.Rating;
            }
            else
            {
                var newRating = new MessageRating
                {
                    MessageId = request.MessageId,
                    Value = request.Rating,
                    UserId = userId
                };
                _context.MessageRatings.Add(newRating);
            }

            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}