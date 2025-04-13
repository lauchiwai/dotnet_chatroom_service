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

    public virtual DbSet<Authenticate> Authenticates { get; set; }

    public virtual DbSet<ChatSession> ChatSessions { get; set; }

    public virtual DbSet<OutboxMessage> OutboxMessage { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
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
            entity.Property(e => e.UpdateTime).HasColumnType("datetime");
            entity.Property(e => e.UserId).HasMaxLength(50);
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

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
