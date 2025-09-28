using Microsoft.EntityFrameworkCore;

namespace ChatbotAIService.Models;

public class ConversationContext : DbContext
{
    public ConversationContext(DbContextOptions<ConversationContext> options)
        : base(options)
    {
    }

    public DbSet<Conversation> Conversations { get; set; } = null!;
    public DbSet<Message> Messages { get; set; } = null!;
    public DbSet<MessageRating> MessageRatings { get; set; } = null!;

    //! For simplicity only for show case
    public DbSet<User> Users { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        
        modelBuilder.Entity<User>()
            .HasMany(u => u.Conversations)
            .WithOne(c => c.User)
            .HasForeignKey(c => c.UserId);

        modelBuilder.Entity<Conversation>()
            .HasKey(c => c.Id);

        modelBuilder.Entity<Message>()
            .HasOne(m => m.Conversation)
            .WithMany(c => c.Messages)
            .HasForeignKey(m => m.ConversationId);

        modelBuilder.Entity<MessageRating>()
            .HasOne(mr => mr.Message)
            .WithMany(m => m.Ratings)
            .HasForeignKey(mr => mr.MessageId);

        modelBuilder.Entity<MessageRating>()
            .HasOne(mr => mr.User)
            .WithMany()
            .HasForeignKey(mr => mr.UserId);


        base.OnModelCreating(modelBuilder);
    }
}