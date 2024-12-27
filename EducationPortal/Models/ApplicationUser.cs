using Microsoft.AspNetCore.Identity;

namespace EducationPortal.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Role { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public string FullName => $"{FirstName} {LastName}";

        // Navigation properties
        public virtual ICollection<Course> TeacherCourses { get; set; }
        public virtual ICollection<Course> EnrolledCourses { get; set; }
    }
} 