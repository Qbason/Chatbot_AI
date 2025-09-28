namespace ChatbotAIService.Models
{

    public enum Rating
    {
        ThumbsUp,
        ThumbsDown
    }

    public class MessageRating
    {
        public int Id { get; set; }
        public int MessageId { get; set; }
        public Rating Value { get; set; }
        public Message? Message { get; set; }
        public string UserId
        {
            get; set;
        } = string.Empty;
        public User User { get; set; } = null!;
    }


}