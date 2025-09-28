using MediatR;
using ChatbotAIService.Features.Messages.Commands;
using ChatbotAIService.Services;
using ChatbotAIService.Models;
using Microsoft.EntityFrameworkCore;

namespace ChatbotAIService.Features.Messages.Handlers
{
    public class ResumeStreamHandler : IRequestHandler<ResumeStreamCommand, IAsyncEnumerable<string>>
    {
        private readonly IChatStreamService _chatStreamService;
        private readonly ConversationContext _context;
        private readonly ICurrentUserService _currentUserService;

        public ResumeStreamHandler(IChatStreamService chatStreamService, ConversationContext context, ICurrentUserService currentUserService)
        {
            _chatStreamService = chatStreamService;
            _context = context;
            _currentUserService = currentUserService;
        }

        public async Task<IAsyncEnumerable<string>> Handle(ResumeStreamCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.GetUserId();
            
            var conversationExists = await _context.Conversations
                .AnyAsync(c => c.Id == request.ConversationId && c.UserId == userId, cancellationToken);

            if (!conversationExists)
            {
                throw new UnauthorizedAccessException($"Conversation {request.ConversationId} not found or access denied.");
            }

            return await _chatStreamService.StartStreamAsync(
                request.ConversationId, 
                request.Message, 
                isResume: true, 
                cancellationToken: cancellationToken);
        }
    }
}