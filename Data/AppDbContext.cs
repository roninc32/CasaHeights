using CasaHeights.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CasaHeights.Data
{
    public class AppDbContext : IdentityDbContext<Users>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<ServiceRequest> ServiceRequests { get; set; }
        public DbSet<Facility> Facilities { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<Announcement> Announcements { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Service Request relationships
            modelBuilder.Entity<ServiceRequest>(entity =>
            {
                entity.HasOne(s => s.Resident)
                    .WithMany()
                    .HasForeignKey(s => s.ResidentId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(s => s.AssignedStaff)
                    .WithMany()
                    .HasForeignKey(s => s.StaffId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Reservation relationships
            modelBuilder.Entity<Reservation>(entity =>
            {
                entity.HasOne(r => r.User)
                    .WithMany()
                    .HasForeignKey(r => r.UserId)
                    .OnDelete(DeleteBehavior.Restrict);  // Changed from Cascade to Restrict

                entity.HasOne(r => r.Facility)
                    .WithMany(f => f.Reservations)
                    .HasForeignKey(r => r.FacilityId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(r => r.ProcessedBy)
                    .WithMany()
                    .HasForeignKey(r => r.ProcessedById)
                    .OnDelete(DeleteBehavior.Restrict);  // Changed from SetNull to Restrict
            });

            // Announcement relationships
            modelBuilder.Entity<Announcement>(entity =>
            {
                entity.HasOne(a => a.CreatedBy)
                    .WithMany()
                    .HasForeignKey(a => a.CreatedById)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
