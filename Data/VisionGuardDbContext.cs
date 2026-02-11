using Microsoft.EntityFrameworkCore;
using visionguard.Models;

namespace visionguard.Data
{
    /// <summary>
    /// EF Core DbContext for Vision Guard
    /// Contains DbSet for Users, Workers, Cameras, Violations
    /// Configure relationships and indexes in OnModelCreating
    /// </summary>
    public class VisionGuardDbContext : DbContext
    {
        public VisionGuardDbContext(DbContextOptions<VisionGuardDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Worker> Workers { get; set; } = null!;
        public DbSet<Camera> Cameras { get; set; } = null!;
        public DbSet<Violation> Violations { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.Id);
                entity.HasIndex(u => u.Username).IsUnique();
                entity.HasIndex(u => u.Email).IsUnique();
                entity.Property(u => u.Role).HasConversion<string>();
            });

            // Worker
            modelBuilder.Entity<Worker>(entity =>
            {
                entity.HasKey(w => w.Id);
                entity.HasIndex(w => w.EmployeeId);
                entity.Property(w => w.Name).IsRequired();
            });

            // Camera
            modelBuilder.Entity<Camera>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.HasIndex(c => c.CameraId).IsUnique();
                entity.HasIndex(c => c.Zone);
                entity.Property(c => c.Zone).IsRequired();
            });

            // Violation
            modelBuilder.Entity<Violation>(entity =>
            {
                entity.HasKey(v => v.Id);

                entity.HasOne(v => v.Worker)
                    .WithMany(w => w.Violations)
                    .HasForeignKey(v => v.WorkerId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(v => v.Camera)
                    .WithMany(c => c.Violations)
                    .HasForeignKey(v => v.CameraId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.Property(v => v.ViolationType).HasConversion<string>();
                entity.Property(v => v.Status).HasConversion<string>();

                // Indexes for performance on filters
                entity.HasIndex(v => v.DetectedAt);
                entity.HasIndex(v => v.WorkerId);
                entity.HasIndex(v => v.CameraId);
                entity.HasIndex(v => v.ViolationType);
                entity.HasIndex(v => new { v.WorkerId, v.CameraId, v.DetectedAt });
            });
        }
    }
}
