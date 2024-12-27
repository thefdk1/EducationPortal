using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace EducationPortal.Models
{
    [Index(nameof(UserName), IsUnique = true)]
    [Index(nameof(Email), IsUnique = true)]

    public class UserAccount : IdentityUser
    {
        [Required]
        [StringLength(50)]
        [Display(Name = "Ad")]
        public string FirstName { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Soyad")]
        public string LastName { get; set; }

        [Display(Name = "Profil Resmi")]
        public string? ProfileImagePath { get; set; }

        [Display(Name = "Kayıt Tarihi")]
        public DateTime RegisterDate { get; set; } = DateTime.Now;

        // Öğretmen olarak verdiği kurslar
        public List<Course> TaughtCourses { get; set; } = new();

        // Öğrenci olarak kayıtlı olduğu kurslar
        public List<Course> EnrolledCourses { get; set; } = new();

        [Display(Name = "Ad Soyad")]
        public string FullName => $"{FirstName} {LastName}";

        [NotMapped]
        public IList<string> Roles { get; set; } = new List<string>();

        public UserAccount()
        {
            Roles = new List<string>();
            TaughtCourses = new List<Course>();
            EnrolledCourses = new List<Course>();
        }
    }
}
