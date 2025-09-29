using ChatbotAIService.Models;

namespace ChatbotAIService.DTOs
{
    public class StreamChatDto
    {
        public required string Message { get; set; }
        public int ConversationId { get; set; }
    }

    public class MessageFeedbackDto
    {
        public int ConversationId { get; set; }
        public int MessageId { get; set; }
        public Rating Rating { get; set; }
    }

    public class StopConversationDto
    {
        public int ConversationId { get; set; }
    }
}