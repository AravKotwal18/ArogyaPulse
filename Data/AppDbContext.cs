using Microsoft.EntityFrameworkCore;
using ArogyaPulse.Api.Models;

namespace ArogyaPulse.Api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        { }

        public DbSet<Patient> Patients { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<SyncLog> SyncLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Patient>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Name).IsRequired();
                entity.Property(p => p.Village).IsRequired();
                entity.Property(p => p.Bp).IsRequired();
            });

            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.HasKey(a => a.Id);
                entity.HasIndex(a => a.PatientId);
                entity.HasIndex(a => a.Timestamp);
            });

            modelBuilder.Entity<SyncLog>(entity =>
            {
                entity.HasKey(s => s.Id);
                entity.HasIndex(s => s.DeviceId);
                entity.HasIndex(s => s.Status);
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}