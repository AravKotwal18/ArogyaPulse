using Microsoft.EntityFrameworkCore;
using ArogyaPulse.Api.Models;
namespace ArogyaPulse.Api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        { }
        public DbSet<Patient> Patients { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Patient>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Name).IsRequired();
                entity.Property(p => p.Village).IsRequired();
                entity.Property(p => p.Bp).IsRequired();
            });
            base.OnModelCreating(modelBuilder);
        }
    }
}