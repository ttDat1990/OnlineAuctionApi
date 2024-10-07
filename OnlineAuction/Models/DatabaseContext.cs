using Microsoft.EntityFrameworkCore;

namespace OnlineAuction.Models;

public partial class DatabaseContext : DbContext
{
    public DatabaseContext()
    {
    }

    public DatabaseContext(DbContextOptions<DatabaseContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Bid> Bids { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Document> Documents { get; set; }

    public virtual DbSet<Favorite> Favorites { get; set; }

    public virtual DbSet<Image> Images { get; set; }

    public virtual DbSet<Item> Items { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<Rating> Ratings { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Bid>(entity =>
        {
            entity.HasKey(e => e.BidId).HasName("PK__Bids__4A733D9264E24FE4");

            entity.Property(e => e.BidAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.BidDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Bidder).WithMany(p => p.Bids)
                .HasForeignKey(d => d.BidderId)
                .HasConstraintName("FK__Bids__BidderId__4E88ABD4");

            entity.HasOne(d => d.Item).WithMany(p => p.Bids)
                .HasForeignKey(d => d.ItemId)
                .HasConstraintName("FK__Bids__ItemId__4D94879B");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("PK__Categori__19093A0B49B19080");

            entity.HasIndex(e => e.CategoryName, "UQ__Categori__8517B2E0D925C14A").IsUnique();

            entity.Property(e => e.CategoryName).HasMaxLength(100);
        });

        modelBuilder.Entity<Document>(entity =>
        {
            entity.HasKey(e => e.DocumentId).HasName("PK__Document__1ABEEF0F5C86E284");

            entity.Property(e => e.DocumentUrl).HasMaxLength(255);

            entity.HasOne(d => d.Item).WithMany(p => p.Documents)
                .HasForeignKey(d => d.ItemId)
                .HasConstraintName("FK__Documents__ItemI__4AB81AF0");
        });

        modelBuilder.Entity<Favorite>(entity =>
        {
            entity.HasKey(e => e.FavoriteId).HasName("PK__Favorite__CE74FAD5213753F9");

            entity.HasIndex(e => new { e.UserId, e.ItemId }, "UC_UserItem").IsUnique();

            entity.Property(e => e.FavoriteDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Status).HasDefaultValue(true);

            entity.HasOne(d => d.Item).WithMany(p => p.Favorites)
                .HasForeignKey(d => d.ItemId)
                .HasConstraintName("FK__Favorites__ItemI__04E4BC85");

            entity.HasOne(d => d.User).WithMany(p => p.Favorites)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__Favorites__UserI__03F0984C");
        });

        modelBuilder.Entity<Image>(entity =>
        {
            entity.HasKey(e => e.ImageId).HasName("PK__Images__7516F70C38BCEAAE");

            entity.Property(e => e.ImageUrl).HasMaxLength(255);

            entity.HasOne(d => d.Item).WithMany(p => p.Images)
                .HasForeignKey(d => d.ItemId)
                .HasConstraintName("FK__Images__ItemId__47DBAE45");
        });

        modelBuilder.Entity<Item>(entity =>
        {
            entity.HasKey(e => e.ItemId).HasName("PK__Items__727E838B4A628CB9");

            entity.Property(e => e.BidEndDate).HasColumnType("datetime");
            entity.Property(e => e.BidIncrement).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.BidStartDate).HasColumnType("datetime");
            entity.Property(e => e.BidStatus)
                .HasMaxLength(1)
                .IsUnicode(false)
                .HasDefaultValue("I")
                .IsFixedLength();
            entity.Property(e => e.CurrentBid).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.ItemDescription).HasMaxLength(4000);
            entity.Property(e => e.ItemTitle).HasMaxLength(100);
            entity.Property(e => e.MinimumBid).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Category).WithMany(p => p.Items)
                .HasForeignKey(d => d.CategoryId)
                .HasConstraintName("FK__Items__CategoryI__44FF419A");

            entity.HasOne(d => d.Seller).WithMany(p => p.Items)
                .HasForeignKey(d => d.SellerId)
                .HasConstraintName("FK__Items__SellerId__440B1D61");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.NotificationId).HasName("PK__Notifica__20CF2E12A827D9ED");

            entity.Property(e => e.IsRead).HasDefaultValue(false);
            entity.Property(e => e.Message).HasMaxLength(1000);
            entity.Property(e => e.NotificationDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.User).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__Notificat__UserI__52593CB8");
        });

        modelBuilder.Entity<Rating>(entity =>
        {
            entity.HasKey(e => e.RatingId).HasName("PK__Ratings__FCCDF87CEB04BF8F");

            entity.ToTable(tb => tb.HasTrigger("UpdateRatingScore"));

            entity.Property(e => e.Comments).HasMaxLength(1000);
            entity.Property(e => e.RatingDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Item).WithMany(p => p.Ratings)
                .HasForeignKey(d => d.ItemId)
                .HasConstraintName("FK__Ratings__ItemId__59063A47");

            entity.HasOne(d => d.RatedByUser).WithMany(p => p.RatingRatedByUsers)
                .HasForeignKey(d => d.RatedByUserId)
                .HasConstraintName("FK__Ratings__RatedBy__5812160E");

            entity.HasOne(d => d.RatedUser).WithMany(p => p.RatingRatedUsers)
                .HasForeignKey(d => d.RatedUserId)
                .HasConstraintName("FK__Ratings__RatedUs__571DF1D5");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__1788CC4C6A79FB3A");

            entity.HasIndex(e => e.Username, "UQ__Users__536C85E4BC820718").IsUnique();

            entity.HasIndex(e => e.Email, "UQ__Users__A9D10534CC438CF3").IsUnique();

            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.IsBlocked).HasDefaultValue(false);
            entity.Property(e => e.Password).HasMaxLength(100);
            entity.Property(e => e.RatingScore).HasDefaultValue(0);
            entity.Property(e => e.Role).HasMaxLength(50);
            entity.Property(e => e.Username).HasMaxLength(100);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
