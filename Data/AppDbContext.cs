using ArogyaPulse.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace ArogyaPulse.Api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(
            DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Patient> Patients => Set<Patient>();

        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

        public DbSet<SyncLog> SyncLogs => Set<SyncLog>();

        protected override void OnModelCreating(
            ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Patient>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(x => x.Village)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(x => x.Bp)
                    .IsRequired()
                    .HasMaxLength(20);

                entity.HasIndex(x => x.Village);

                entity.HasIndex(x => x.RiskLevel);

                entity.HasIndex(x => x.Timestamp);
            });

            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.HasIndex(x => x.PatientId);

                entity.HasIndex(x => x.Timestamp);
            });

            modelBuilder.Entity<SyncLog>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.HasIndex(x =>
                    new
                    {
                        x.DeviceId,
                        x.LocalRecordId
                    })
                    .IsUnique();

                entity.HasIndex(x => x.Status);

                entity.HasIndex(x => x.CreatedAt);
            });
        }
    }
}