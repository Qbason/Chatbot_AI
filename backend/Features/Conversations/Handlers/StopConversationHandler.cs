using MediatR;
using Microsoft.EntityFrameworkCore;
using ChatbotAIService.Models;
using ChatbotAIService.Features.Conversations.Commands;
using ChatbotAIService.Services;

namespace ChatbotAIService.Features.Conversations.Handlers
{
    public class StopConversationHandler : IRequestHandler<StopConversationCommand, bool>
    {
        private readonly ConversationContext _context;
        private readonly IStreamingConversationService _streamingService;
        private readonly ICurrentUserService _currentUserService;

        public StopConversationHandler(ConversationContext context, IStreamingConversationService streamingService, ICurrentUserService currentUserService)
        {
            _context = context;
            _streamingService = streamingService;
            _currentUserService = currentUserService;
        }

        public async Task<bool> Handle(StopConversationCommand request, CancellationToken cancellationToken)
        {

            var conversation = await _context.Conversations
                .FirstOrDefaultAsync(c => c.Id == request.ConversationId && c.UserId == _currentUserService.GetUserId() , cancellationToken);

            if (conversation == null)
                return false;

            var wasStreaming = _streamingService.TryStopStreamingConversation(request.ConversationId);

            conversation.Status = ConversationStatus.Stopped;
            await _context.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}