namespace Chatbot_AI.Models
{
    public class Conversation
    {
        public int Id { get; set; }
        public required string Title { get; set; } 
        public List<Message> Messages { get; set; } = new List<Message>();
        public string UserId { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }

    public enum Role
    {
        User,
        AI
    }

    public class Message
    {
        public int Id { get; set; }
        public Role Role { get; set; }
        public required string Content { get; set; }
    }


}