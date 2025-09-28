using System.Text.Json.Serialization;

namespace ChatbotAIService.Models
{
    public enum ConversationStatus
    {
        Streaming = 0,
        Completed = 1,
        Stopped = 2
    }
    public class Conversation
    {
        public int Id { get; set; }
        public required string Title { get; set; }
        public List<Message> Messages { get; set; } = new List<Message>();
        public string UserId { get; set; } = string.Empty;
        public User User { get; set; } = null!;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public ConversationStatus Status { get; set; } = ConversationStatus.Completed;
    }

    public enum Role
    {
        User = 0,
        AI = 1
        //later there could be more roles like assistant and tool
    }

    public class Message
    {
        public int Id { get; set; }
        public Role Role { get; set; }
        public required string Content { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public List<MessageRating> Ratings { get; set; } = new List<MessageRating>();
        public int ConversationId { get; set; }
        [JsonIgnore]
        public Conversation? Conversation { get; set; }
    }



}