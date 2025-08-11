using Microsoft.EntityFrameworkCore;
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
                entity.ToTable("Fifteentf_data");
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

            modelBuilder.Entity<Trendline>(entity =>
            {
                entity.ToTable("trendline_data");
                entity.HasKey(e => e.id);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
