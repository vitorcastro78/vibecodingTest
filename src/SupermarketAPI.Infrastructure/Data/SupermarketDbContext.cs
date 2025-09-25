using Microsoft.EntityFrameworkCore;
using SupermarketAPI.Domain.Entities;

namespace SupermarketAPI.Infrastructure.Data
{
    public class SupermarketDbContext : DbContext
    {
        public SupermarketDbContext(DbContextOptions<SupermarketDbContext> options) : base(options)
        {
        }

        public DbSet<Supermarket> Supermarkets { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductPrice> ProductPrices { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UserFavorite> UserFavorites { get; set; }
        public DbSet<UserNotificationSettings> UserNotificationSettings { get; set; }
        public DbSet<DailyRanking> DailyRankings { get; set; }
        public DbSet<ScrapingLog> ScrapingLogs { get; set; }
        public DbSet<NotificationLog> NotificationLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Supermarket configuration
            modelBuilder.Entity<Supermarket>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.BaseUrl).IsRequired().HasMaxLength(500);
                entity.Property(e => e.LogoUrl).HasMaxLength(500);
                entity.Property(e => e.ScrapingConfiguration).HasColumnType("nvarchar(max)");
                entity.HasIndex(e => e.Name).IsUnique();
            });

            // Category configuration
            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.IconUrl).HasMaxLength(500);
                entity.HasIndex(e => e.Name);
                
                entity.HasOne(e => e.ParentCategory)
                    .WithMany(e => e.SubCategories)
                    .HasForeignKey(e => e.ParentCategoryId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Product configuration
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.NormalizedName).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Brand).HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.Property(e => e.ImageUrl).HasMaxLength(500);
                entity.Property(e => e.Barcode).HasMaxLength(50);
                entity.Property(e => e.Keywords).HasMaxLength(500);
                entity.Property(e => e.AveragePrice).HasColumnType("decimal(10,2)");
                entity.Property(e => e.MinPrice).HasColumnType("decimal(10,2)");
                entity.Property(e => e.MaxPrice).HasColumnType("decimal(10,2)");
                
                entity.HasIndex(e => e.NormalizedName);
                entity.HasIndex(e => e.Barcode);
                entity.HasIndex(e => new { e.CategoryId, e.NormalizedName });
                
                entity.HasOne(e => e.Category)
                    .WithMany(e => e.Products)
                    .HasForeignKey(e => e.CategoryId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ProductPrice configuration
            modelBuilder.Entity<ProductPrice>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Price).IsRequired().HasColumnType("decimal(10,2)");
                entity.Property(e => e.Unit).IsRequired().HasMaxLength(20);
                entity.Property(e => e.OriginalUnit).IsRequired().HasMaxLength(20);
                entity.Property(e => e.PricePerUnit).HasColumnType("decimal(10,2)");
                entity.Property(e => e.Url).HasMaxLength(1000);
                entity.Property(e => e.OriginalPrice).HasColumnType("decimal(10,2)");
                entity.Property(e => e.SaleDescription).HasMaxLength(200);
                
                entity.HasIndex(e => new { e.ProductId, e.SupermarketId, e.ScrapedAt });
                entity.HasIndex(e => e.ScrapedAt);
                
                entity.HasOne(e => e.Product)
                    .WithMany(e => e.ProductPrices)
                    .HasForeignKey(e => e.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);
                    
                entity.HasOne(e => e.Supermarket)
                    .WithMany(e => e.ProductPrices)
                    .HasForeignKey(e => e.SupermarketId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // User configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.Property(e => e.PasswordHash).IsRequired().HasMaxLength(255);
                entity.Property(e => e.PhoneNumber).HasMaxLength(20);
                entity.Property(e => e.WhatsAppNumber).HasMaxLength(20);
                entity.Property(e => e.FirstName).HasMaxLength(50);
                entity.Property(e => e.LastName).HasMaxLength(50);
                entity.Property(e => e.EmailVerificationToken).HasMaxLength(100);
                entity.Property(e => e.WhatsAppVerificationToken).HasMaxLength(100);
                
                entity.HasIndex(e => e.Email).IsUnique();
                entity.HasIndex(e => e.WhatsAppNumber);
            });

            // UserFavorite configuration
            modelBuilder.Entity<UserFavorite>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.PriceThreshold).HasColumnType("decimal(10,2)");
                
                entity.HasIndex(e => new { e.UserId, e.ProductId }).IsUnique();
                
                entity.HasOne(e => e.User)
                    .WithMany(e => e.UserFavorites)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
                    
                entity.HasOne(e => e.Product)
                    .WithMany(e => e.UserFavorites)
                    .HasForeignKey(e => e.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // UserNotificationSettings configuration
            modelBuilder.Entity<UserNotificationSettings>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.NotificationDays).HasMaxLength(20);
                entity.Property(e => e.PriceDropThreshold).HasColumnType("decimal(5,2)");
                entity.Property(e => e.NotificationTime).HasColumnType("time");
                
                entity.HasIndex(e => e.UserId).IsUnique();
                
                entity.HasOne(e => e.User)
                    .WithOne(e => e.NotificationSettings)
                    .HasForeignKey<UserNotificationSettings>(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // DailyRanking configuration
            modelBuilder.Entity<DailyRanking>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.RankingData).HasColumnType("nvarchar(max)");
                entity.Property(e => e.AveragePrice).HasColumnType("decimal(10,2)");
                entity.Property(e => e.MinPrice).HasColumnType("decimal(10,2)");
                entity.Property(e => e.MaxPrice).HasColumnType("decimal(10,2)");
                
                entity.HasIndex(e => new { e.Date, e.CategoryId });
                
                entity.HasOne(e => e.Category)
                    .WithMany(e => e.DailyRankings)
                    .HasForeignKey(e => e.CategoryId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ScrapingLog configuration
            modelBuilder.Entity<ScrapingLog>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(20);
                entity.Property(e => e.ErrorDetails).HasColumnType("nvarchar(max)");
                entity.Property(e => e.UserAgent).HasMaxLength(500);
                entity.Property(e => e.ProxyUsed).HasMaxLength(100);
                entity.Property(e => e.Duration).HasColumnType("time");
                
                entity.HasIndex(e => new { e.SupermarketId, e.StartedAt });
                
                entity.HasOne(e => e.Supermarket)
                    .WithMany(e => e.ScrapingLogs)
                    .HasForeignKey(e => e.SupermarketId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // NotificationLog configuration
            modelBuilder.Entity<NotificationLog>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Type).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Content).IsRequired().HasColumnType("nvarchar(max)");
                entity.Property(e => e.Status).IsRequired().HasMaxLength(20);
                entity.Property(e => e.WhatsAppMessageId).HasMaxLength(100);
                entity.Property(e => e.ErrorMessage).HasMaxLength(500);
                entity.Property(e => e.Channel).HasMaxLength(20);
                entity.Property(e => e.TotalSavings).HasColumnType("decimal(10,2)");
                
                entity.HasIndex(e => new { e.UserId, e.SentAt });
                entity.HasIndex(e => e.SentAt);
                
                entity.HasOne(e => e.User)
                    .WithMany(e => e.NotificationLogs)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
