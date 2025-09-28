namespace ChatbotAIService.Models
{
    public class User
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public List<Conversation> Conversations { get; set; } = new List<Conversation>();
    }

}