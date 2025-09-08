using Common.Models;
using Microsoft.EntityFrameworkCore;

namespace Repositories.MyDbContext;

public partial class MyDbContext : DbContext
{
    public MyDbContext()
    {
    }

    public MyDbContext(DbContextOptions<MyDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Article> Article { get; set; }

    public virtual DbSet<Article_Chat_Session> Article_Chat_Session { get; set; }

    public virtual DbSet<Article_User> Article_User { get; set; }

    public virtual DbSet<Authenticate> Authenticate { get; set; }

    public virtual DbSet<ChatSession> ChatSession { get; set; }

    public virtual DbSet<OutboxMessage> OutboxMessage { get; set; }

    public virtual DbSet<UserWords> UserWords { get; set; }

    public virtual DbSet<Words> Words { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Article>(entity =>
        {
            entity.HasKey(e => e.ArticleID).HasName("PK__Article__9C6270C8092B52A3");

            entity.Property(e => e.ArticleTitle).HasMaxLength(50);
            entity.Property(e => e.DeletedAt).HasColumnType("datetime");
            entity.Property(e => e.IsDeleted)
                 .HasDefaultValue(false)
                 .HasComment("1:true, 0: false")
                 .HasConversion(
                    v => v ? 1 : 0,
                    v => v == 1
                 );
            entity.Property(e => e.UpdateTime).HasColumnType("datetime");

            entity.HasOne(d => d.Owner).WithMany(p => p.Article)
                .HasForeignKey(d => d.OwnerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Article_Authenticate");
        });

        modelBuilder.Entity<Article_Chat_Session>(entity =>
        {
            entity.HasKey(e => e.SessionID).HasName("PK__Article___C9F492708DBE60F1");

            entity.Property(e => e.SessionID).ValueGeneratedNever();

            entity.HasOne(d => d.Article).WithMany(p => p.Article_Chat_Session)
                .HasForeignKey(d => d.ArticleId)
                .HasConstraintName("FK_ArticleChatSession_Article");

            entity.HasOne(d => d.Session).WithOne(p => p.Article_Chat_Session)
                .HasForeignKey<Article_Chat_Session>(d => d.SessionID)
                .HasConstraintName("FK_ArticleChatSession_ChatSession");
        });

        modelBuilder.Entity<Article_User>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.ArticleId });

            entity.HasOne(d => d.Article).WithMany(p => p.Article_User)
                .HasForeignKey(d => d.ArticleId)
                .HasConstraintName("FK_ArticleUser_Article");

            entity.HasOne(d => d.User).WithMany(p => p.Article_User)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_ArticleUser_Authenticate");
        });

        modelBuilder.Entity<Authenticate>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Authenti__3213E83FF2FA5720");

            entity.Property(e => e.Pw)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.RefreshToken)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.RefreshTokenExpiryTime).HasColumnType("datetime");
            entity.Property(e => e.UserName)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<ChatSession>(entity =>
        {
            entity.HasKey(e => e.SessionId).HasName("PK__Chat_Ses__C9F4927095D5C284");

            entity.Property(e => e.SessionName).HasMaxLength(50);
            entity.Property(e => e.DeletedAt).HasColumnType("datetime");
            entity.Property(e => e.IsDeleted)
                 .HasDefaultValue(false)
                 .HasComment("1:true, 0: false")
                 .HasConversion(
                    v => v ? 1 : 0,
                    v => v == 1
                 );
            entity.Property(e => e.UpdateTime).HasColumnType("datetime");

            entity.HasOne(d => d.User).WithMany(p => p.Chat_Session)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ChatSession_Authenticate");
        });

        modelBuilder.Entity<OutboxMessage>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("OutboxMessage_PK");

            entity.Property(e => e.Id)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.CreatedTime).HasColumnType("datetime");
            entity.Property(e => e.EventType)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.IsPublished)
                 .HasDefaultValue(false)
                 .HasComment("1:true, 0: false")
                 .HasConversion(
                    v => v ? 1 : 0,
                    v => v == 1
                 );
            entity.Property(e => e.Payload)
                 .HasMaxLength(4000)
                 .IsUnicode(false);
        });

        modelBuilder.Entity<UserWords>(entity =>
        {
            entity.HasKey(e => e.UserWordId).HasName("PK__UserWord__6E1BC12F6C089B33");

            entity.HasIndex(e => new { e.UserId, e.WordId }, "UQ_UserWord").IsUnique();

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.LastReviewed).HasColumnType("datetime");
            entity.Property(e => e.NextReviewDate)
                .HasDefaultValueSql("(dateadd(day,(1),getdate()))")
                .HasColumnType("datetime");

            entity.HasOne(d => d.User).WithMany(p => p.UserWords)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserWords_Users");

            entity.HasOne(d => d.Word).WithMany(p => p.UserWords)
                .HasForeignKey(d => d.WordId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserWords_Words");
        });

        modelBuilder.Entity<Words>(entity =>
        {
            entity.HasKey(e => e.WordId).HasName("PK__Words__2C20F066654E52E0");

            entity.HasIndex(e => e.Word, "UQ_Word").IsUnique();

            entity.Property(e => e.AddedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Word).HasMaxLength(100);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
