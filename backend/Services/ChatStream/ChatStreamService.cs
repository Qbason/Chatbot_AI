using MediatR;
using OpenAI.Chat;
using ChatbotAIService.Models;
using ChatbotAIService.Features.Messages.Commands;
using ChatbotAIService.Features.Conversations.Commands;
using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;

namespace ChatbotAIService.Services
{
    public class ChatStreamService : IChatStreamService
    {
        private readonly ChatClient _chatClient;
        private readonly ConversationContext _context;
        private readonly IStreamingConversationService _streamingService;
        private readonly ILogger<ChatStreamService> _logger;
        private readonly ICurrentUserService _currentUserService;

        public ChatStreamService(
            ChatClient chatClient,
            ConversationContext context,
            ICurrentUserService currentUserService,
            IStreamingConversationService streamingService,
            ILogger<ChatStreamService> logger)
        {
            _chatClient = chatClient;
            _context = context;
            _streamingService = streamingService;
            _logger = logger;
            _currentUserService = currentUserService;
        }

        public async Task<IAsyncEnumerable<string>> StartStreamAsync(
            int conversationId,
            string message,
            bool isResume = false,
            CancellationToken cancellationToken = default)
        {
            await GetAndValidateConversation(conversationId, isResume, cancellationToken);

            //! THIS IS NO REDUNDANT CODE - it is to decouple service with mediator and  
            //! avoid circular dependencies where handler calls service and service calls handler
            await CreateMessage(conversationId, Role.User, message, cancellationToken);

            var aiMessage = await CreateMessage(conversationId, Role.AI, string.Empty, cancellationToken);

            var conversationHistory = await GetConversationHistory(conversationId, cancellationToken);

            await UpdateConversationStatus(conversationId, ConversationStatus.Streaming, cancellationToken);

            //TODO: check if it will cancel stream when the connection drops
            var streamTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _streamingService.RegisterStreamingConversation(conversationId, streamTokenSource);

            return StreamOpenAIResponse(conversationHistory, aiMessage.Id, conversationId, streamTokenSource.Token);
        }

        private async Task<bool> UpdateConversationStatus(
            int conversationId,
            ConversationStatus status,
            CancellationToken cancellationToken = default
        )
        {
            var conversation = await _context.Conversations
                .FirstOrDefaultAsync(c => c.Id == conversationId, cancellationToken);

            if (conversation == null)
                return false;

            conversation.Status = status;
            await _context.SaveChangesAsync(cancellationToken);

            return true;
        }

        private async Task<Message> CreateMessage(int conversationId, Role role, string content, CancellationToken cancellationToken)
        {
            var message = new Message
            {
                Role = role,
                Content = content,
                ConversationId = conversationId
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync(cancellationToken);

            return message;
        }

        private async Task<Conversation> GetAndValidateConversation(int conversationId, bool isResume, CancellationToken cancellationToken)
        {
            var conversation = await _context.Conversations
                .Include(c => c.Messages)
                .FirstOrDefaultAsync(c => c.Id == conversationId, cancellationToken);

            if (conversation == null)
            {
                throw new ArgumentException($"Conversation with ID {conversationId} not found");
            }

            if (_streamingService.IsConversationStreaming(conversationId))
            {
                throw new InvalidOperationException($"Conversation {conversationId} is already streaming");
            }

            if (isResume)
            {
                if (conversation.Status != ConversationStatus.Stopped &&
                    conversation.Status != ConversationStatus.Completed)
                {
                    throw new InvalidOperationException(
                        $"Conversation {conversationId} is not in a resumable state. Current status: {conversation.Status}");
                }

            }
            else
            {
                _logger.LogInformation("Starting new stream for existing conversation {ConversationId}", conversationId);
            }

            return conversation;
        }

        private async Task<List<ChatMessage>> GetConversationHistory(int conversationId, CancellationToken cancellationToken)
        {
            var conversation = await _context.Conversations
                .Include(c => c.Messages)
                .FirstOrDefaultAsync(c => c.Id == conversationId, cancellationToken);

            if (conversation == null)
                return new List<ChatMessage>();

            var messages = new List<ChatMessage>();

            foreach (var message in conversation.Messages.OrderBy(m => m.Id))
            {
                if (message.Role == Role.User)
                {
                    messages.Add(ChatMessage.CreateUserMessage(message.Content));
                }
                else if (message.Role == Role.AI)
                {
                    messages.Add(ChatMessage.CreateAssistantMessage(message.Content));
                }
                else
                {
                    _logger.LogWarning("Unknown message role {Role} in message ID {MessageId}", message.Role, message.Id);
                }
            }

            return messages;
        }

        private async IAsyncEnumerable<string> StreamOpenAIResponse(
            List<ChatMessage> conversationHistory,
            int aiMessageId,
            int conversationId,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var completionUpdates = _chatClient.CompleteChatStreamingAsync(conversationHistory, cancellationToken: cancellationToken);
            var fullResponse = "";
            var chunkCount = 0;

            await foreach (var completionUpdate in completionUpdates.WithCancellation(cancellationToken))
            {
                if (cancellationToken.IsCancellationRequested)
                {

                    await UpdateConversationStatus(conversationId, ConversationStatus.Stopped, CancellationToken.None);

                    _streamingService.UnregisterStreamingConversation(conversationId);
                    yield break;
                }

                if (completionUpdate.ContentUpdate.Count > 0)
                {
                    var chunk = completionUpdate.ContentUpdate[0].Text;
                    fullResponse += chunk;
                    chunkCount++;

                    await UpdateMessage(new UpdateMessageCommand
                    {
                        MessageId = aiMessageId,
                        Content = fullResponse
                    }, CancellationToken.None);


                    yield return chunk;
                }
            }

            await UpdateConversationStatus(conversationId, ConversationStatus.Completed, CancellationToken.None);

            _streamingService.UnregisterStreamingConversation(conversationId);
        }

        private async Task<bool> UpdateConversationStatus(UpdateConversationStatusCommand request, CancellationToken cancellationToken)
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

        private async Task<bool> UpdateMessage(UpdateMessageCommand request, CancellationToken cancellationToken)
        {
            var message = await _context.Messages.FirstOrDefaultAsync(m => m.Id == request.MessageId, cancellationToken);

            if (message == null)
                return false;

            message.Content = request.Content;
            await _context.SaveChangesAsync(cancellationToken);

            return true;
        }

    }
}