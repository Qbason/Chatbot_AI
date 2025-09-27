using Microsoft.EntityFrameworkCore;

namespace Chatbot_AI.Models;

public class ConversationContext : DbContext
{
    public ConversationContext(DbContextOptions<ConversationContext> options)
        : base(options)
    {
    }

    public DbSet<Conversation> Conversations { get; set; } = null!;
}