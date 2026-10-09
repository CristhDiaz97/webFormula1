using Microsoft.EntityFrameworkCore;
using WebFormula1.Core.Models;

namespace WebFormula1.Infrastructure.Data;

public class Formula1DbContext : DbContext
{
    public Formula1DbContext(DbContextOptions<Formula1DbContext> options) : base(options) { }

    public DbSet<Driver> Drivers => Set<Driver>();
    public DbSet<Team> Teams => Set<Team>();
    public DbSet<Race> Races => Set<Race>();
    public DbSet<DriverResult> DriverResults => Set<DriverResult>();
    public DbSet<DriverStanding> DriverStandings => Set<DriverStanding>();
    public DbSet<TeamStanding> TeamStandings => Set<TeamStanding>();
    public DbSet<RaceNotification> RaceNotifications => Set<RaceNotification>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Driver>(e =>
        {
            e.Ignore(d => d.FullName);
            e.HasIndex(d => d.ExternalId).IsUnique();
            e.Property(d => d.ExternalId).HasMaxLength(100);
            e.HasOne(d => d.Team).WithMany(t => t.Drivers).HasForeignKey(d => d.TeamId).OnDelete(DeleteBehavior.SetNull);
        });

        b.Entity<Team>(e =>
        {
            e.HasIndex(t => t.ExternalId).IsUnique();
            e.Property(t => t.ExternalId).HasMaxLength(100);
        });

        b.Entity<Race>(e =>
        {
            e.HasIndex(r => new { r.SeasonYear, r.Round }).IsUnique();
        });

        b.Entity<DriverResult>(e =>
        {
            e.HasIndex(r => new { r.RaceId, r.DriverId }).IsUnique();
            e.HasOne(r => r.Race).WithMany(r => r.Results).HasForeignKey(r => r.RaceId);
            e.HasOne(r => r.Driver).WithMany(d => d.Results).HasForeignKey(r => r.DriverId);
            e.HasOne(r => r.Team).WithMany().HasForeignKey(r => r.TeamId).OnDelete(DeleteBehavior.NoAction);
        });

        b.Entity<DriverStanding>(e =>
        {
            e.HasIndex(s => new { s.SeasonYear, s.DriverId }).IsUnique();
            e.HasOne(s => s.Driver).WithMany(d => d.Standings).HasForeignKey(s => s.DriverId);
        });

        b.Entity<TeamStanding>(e =>
        {
            e.HasIndex(s => new { s.SeasonYear, s.TeamId }).IsUnique();
            e.HasOne(s => s.Team).WithMany(t => t.Standings).HasForeignKey(s => s.TeamId);
        });

        b.Entity<RaceNotification>(e =>
        {
            e.HasIndex(n => n.RaceId).IsUnique();
            e.HasOne(n => n.Race).WithMany().HasForeignKey(n => n.RaceId);
        });
    }
}
