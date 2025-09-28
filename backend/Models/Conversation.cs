namespace ChatbotAIService.Models
{
    public enum ConversationStatus
    {
        Streaming,
        Completed,
        Stopped
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
        User,
        AI
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
        public Conversation? Conversation { get; set; }
    }



}