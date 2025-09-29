using MediatR;
using OpenAI.Chat;
using ChatbotAIService.Models;
using ChatbotAIService.Features.Messages.Commands;
using ChatbotAIService.Features.Conversations.Commands;
using ChatbotAIService.Services;
using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;

namespace ChatbotAIService.Features.Messages.Handlers
{
    public class StreamChatHandler : IRequestHandler<StreamChatCommand, IAsyncEnumerable<string>>
    {
        private readonly IMediator _mediator;
        private readonly IChatStreamService _chatStreamService;
        private readonly ICurrentUserService _currentUserService;
        private readonly ConversationContext _context;

        public StreamChatHandler(IMediator mediator, IChatStreamService chatStreamService, ICurrentUserService currentUserService, ConversationContext context)
        {
            _currentUserService = currentUserService;
            _context = context;
            _mediator = mediator;
            _chatStreamService = chatStreamService;
        }

        public async Task<IAsyncEnumerable<string>> Handle(StreamChatCommand request, CancellationToken cancellationToken)
        {
            int conversationId;

            conversationId = request.ConversationId;
            var userId = _currentUserService.GetUserId();

            var conversationExists = await _context.Conversations
                .AnyAsync(c => c.Id == conversationId && c.UserId == userId, cancellationToken);

            if (!conversationExists)
            {
                throw new UnauthorizedAccessException($"Conversation {conversationId} not found or access denied.");
            }

            return await _chatStreamService.StartStreamAsync(
                conversationId,
                request.Message,
                cancellationToken: cancellationToken);
        }
    }
}