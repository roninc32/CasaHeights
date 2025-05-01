using CasaHeights.Models;
using CasaHeights.Models.Forum;
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
        public DbSet<ForumPost> ForumPosts { get; set; }
        public DbSet<ForumComment> ForumComments { get; set; }
        public DbSet<ForumCategory> ForumCategories { get; set; }
        public DbSet<ForumTag> ForumTags { get; set; }
        public DbSet<ForumReport> ForumReports { get; set; }

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

            // Forum Post relationships
            modelBuilder.Entity<ForumPost>(entity =>
            {
                entity.HasMany(p => p.Comments)
                    .WithOne(c => c.Post)
                    .HasForeignKey(c => c.PostId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(p => p.Categories)
                    .WithMany(c => c.Posts);

                entity.HasMany(p => p.Tags)
                    .WithMany(t => t.Posts);

                // Add author relationship with restrict delete
                entity.HasOne(p => p.Author)
                    .WithMany()
                    .HasForeignKey(p => p.AuthorId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasQueryFilter(p => !p.IsDeleted);
                
                entity.HasIndex(p => p.CreatedAt);
                
                entity.HasIndex(p => p.Status);
            });
            
            // Forum Comment relationships
            modelBuilder.Entity<ForumComment>(entity =>
            {
                // This is the key change - change the Author relationship to Restrict instead of Cascade
                entity.HasOne(c => c.Author)
                    .WithMany()
                    .HasForeignKey(c => c.AuthorId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasQueryFilter(c => !c.IsDeleted);
                
                entity.HasIndex(c => c.CreatedAt);
            });

            // Forum Report relationships
            modelBuilder.Entity<ForumReport>(entity =>
            {
                entity.HasOne(r => r.Reporter)
                    .WithMany()
                    .HasForeignKey(r => r.ReporterId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Ensure ForumTag is properly configured
            modelBuilder.Entity<ForumTag>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Description).IsRequired();
            });
        }
    }
}