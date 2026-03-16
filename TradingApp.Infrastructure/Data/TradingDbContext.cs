using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using TradingApp.Core.Entities;

namespace TradingApp.Infrastructure.Data
{
    public partial class TradingDbContext : DbContext
    {
        public TradingDbContext(DbContextOptions<TradingDbContext> options) : base(options) { }

        public DbSet<DailyTF> DailyTF { get; set; }
        public DbSet<FifteenTF> FifteenTF { get; set; }
        public DbSet<HighLow> HighLow { get; set; }
        public DbSet<Trendline> Trendline { get; set; }
        public DbSet<Users> Users {get; set;}
        public DbSet<TrendlineScore> TrendlineScore {get; set;}
        public DbSet<UserTrendline> UserTrendlines { get; set; }
        public DbSet<UserBookmark> UserBookmarks { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DailyTF>(entity =>
            {
                entity.ToTable("dailytf_data");
                entity.HasKey(e => e.id);
                entity.Property(e => e.time)
                  .HasColumnType("timestamp without time zone");
            });

            modelBuilder.Entity<FifteenTF>(entity =>
            {
                entity.ToTable("fifteentf_data");
                entity.HasKey(e => e.id);
                entity.Property(e => e.time)
                  .HasColumnType("timestamp without time zone");
            });

            modelBuilder.Entity<HighLow>(entity =>
            {
                entity.ToTable("highlow_data");
                entity.HasKey(e => e.id);
                entity.Property(e => e.time)
                  .HasColumnType("timestamp without time zone");
            });
            modelBuilder.Entity<TrendlineScore>(entity =>
            {
                entity.ToTable("trendlinescore_data");
                entity.HasKey(e => e.Id);
            });

            modelBuilder.Entity<Trendline>(entity =>
            {
                entity.ToTable("trendline_data");
                entity.HasKey(e => e.id);
                var unspecified = new ValueConverter<DateTime, DateTime>(
                    v => DateTime.SpecifyKind(v, DateTimeKind.Unspecified),
                    v => DateTime.SpecifyKind(v, DateTimeKind.Unspecified)
                );


                entity.Property(e => e.starttime)
                    .HasColumnType("timestamp without time zone")
                    .HasConversion(unspecified);

                entity.Property(e => e.endtime)
                    .HasColumnType("timestamp without time zone")
                    .HasConversion(unspecified);
                
                entity.HasOne(t => t.Score)
                .WithOne(s => s.Trendline)
                .HasForeignKey<TrendlineScore>(s => s.Id)
                .OnDelete(DeleteBehavior.Cascade);
            });
            
            modelBuilder.Entity<Users>(entity =>
            {
                entity.ToTable("users");
                entity.HasKey(e => e.id);
            });
            // USER TRENDLINES
            modelBuilder.Entity<UserTrendline>(entity =>
            {
                entity.ToTable("user_trendlines");
                entity.HasKey(e => e.Id);

                entity.HasIndex(t => new { t.UserId, t.Token, t.Tf })
                      .HasDatabaseName("idx_user_trendlines_lookup");
            });


            // USER BOOKMARKS
            modelBuilder.Entity<UserBookmark>(entity =>
            {
                entity.ToTable("user_bookmarks");
                entity.HasKey(e => e.Id);

                entity.HasIndex(b => new { b.UserId, b.Token })
                      .IsUnique()
                      .HasDatabaseName("uniq_user_token");
                entity.Property(b => b.Color)
                .HasConversion<string>();
            });


            // REFRESH TOKENS
            modelBuilder.Entity<RefreshToken>(entity =>
            {
                entity.ToTable("refresh_tokens");
                entity.HasKey(e => e.Id);

                entity.HasIndex(r => r.Token)
                      .IsUnique()
                      .HasDatabaseName("idx_refresh_token");

                entity.HasIndex(r => r.UserId)
                      .IsUnique()
                      .HasDatabaseName("idx_refresh_user");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
