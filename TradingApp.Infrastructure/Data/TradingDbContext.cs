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
            });
            
            modelBuilder.Entity<Users>(entity =>
            {
                entity.ToTable("users");
                entity.HasKey(e => e.id);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
