using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using TradingApp.API.Models;

namespace TradingApp.API.Data;

public partial class MyDbContext : DbContext
{
    public MyDbContext(DbContextOptions<MyDbContext> options)
        : base(options)
    {
    }   
    public virtual DbSet<DailyTFData> DailyTFData{ get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DailyTFData>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("v_dailytf_data");

            entity.Property(e => e.time).HasColumnType("timestamp without time zone");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
