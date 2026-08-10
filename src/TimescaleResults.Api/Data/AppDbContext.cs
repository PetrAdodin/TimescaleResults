using Microsoft.EntityFrameworkCore;
using TimescaleResults.Api.Data.Entities;

namespace TimescaleResults.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options)
    : DbContext(options)
{
    public DbSet<ResultEntity> Results => Set<ResultEntity>();

    public DbSet<ValueEntity> Values => Set<ValueEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureResultEntity(modelBuilder);
        ConfigureValueEntity(modelBuilder);
    }

    private static void ConfigureResultEntity(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<ResultEntity>();

        entity.ToTable("Results");

        entity.HasKey(result => result.Id);

        entity.Property(result => result.FileName)
            .IsRequired();

        entity.HasIndex(result => result.FileName)
            .IsUnique();

        entity.Property(result => result.MinDate)
            .HasColumnType("timestamp with time zone");
    }

    private static void ConfigureValueEntity(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<ValueEntity>();

        entity.ToTable("Values");

        entity.HasKey(value => value.Id);

        entity.Property(value => value.Date)
            .HasColumnType("timestamp with time zone");

        entity.HasOne(value => value.Result)
            .WithMany(result => result.Values)
            .HasForeignKey(value => value.ResultId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasIndex(value => new
        {
            value.ResultId,
            value.Date
        })
            .IsDescending(false, true);
    }
}