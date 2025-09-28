using System.Text.Json.Serialization;

namespace ChatbotAIService.Models
{

    public enum Rating
    {
        ThumbsUp = 0,
        ThumbsDown = 1
    }

    public class MessageRating
    {
        public int Id { get; set; }
        public int MessageId { get; set; }
        public Rating Value { get; set; }
        [JsonIgnore]
        public Message? Message { get; set; }
        public required string UserId { get; set; } = string.Empty;
        [JsonIgnore]
        public User User { get; set; } = null!;
    }


}