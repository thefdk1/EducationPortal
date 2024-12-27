using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EducationPortal.Models
{
    public class AppDbContext : IdentityDbContext<UserAccount, UserRole, string>
    {
        public DbSet<Course> Courses { get; set; }
        public DbSet<Lesson> Lessons { get; set; }

        public DbSet<UserAccount> UserAccounts { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Kurs-Öğretmen ilişkisi (1-n)
            modelBuilder.Entity<Course>()
                .HasOne(c => c.Teacher)
                .WithMany(t => t.TaughtCourses)
                .HasForeignKey(c => c.TeacherId)
                .OnDelete(DeleteBehavior.Restrict);

            // Kurs-Öğrenci ilişkisi (n-n)
            modelBuilder.Entity<Course>()
                .HasMany(c => c.EnrolledStudents)
                .WithMany(s => s.EnrolledCourses)
                .UsingEntity(j => j.ToTable("CourseEnrollments"));

            // Kurs-Ders ilişkisi (1-n)
            modelBuilder.Entity<Course>()
                .HasMany(c => c.Lessons)
                .WithOne(l => l.Course)
                .HasForeignKey(l => l.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            // String alanların uzunlukları için varsayılan değerleri belirlemek
            modelBuilder.Entity<Course>(entity =>
            {
                entity.Property(c => c.Title).HasMaxLength(100).IsRequired();
                entity.Property(c => c.Description).HasMaxLength(500).IsRequired();
                entity.Property(c => c.ThumbnailImagePath).IsRequired();
            });

            modelBuilder.Entity<Lesson>(entity =>
            {
                entity.Property(l => l.Title).HasMaxLength(100).IsRequired();
                entity.Property(l => l.VideoUrl).IsRequired();
            });
        }
    }
}

