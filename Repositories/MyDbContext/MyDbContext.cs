using Microsoft.EntityFrameworkCore;
using Common.Models;

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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Authenticate>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Authenti__3213E83FF2FA5720");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
