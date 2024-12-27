using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace EducationPortal.Models
{
    public class UserRole : IdentityRole
    {
        [Display(Name = "Açıklama")]
        public string? Description { get; set; }

        public UserRole() : base() { }

        public UserRole(string roleName) : base(roleName) { }
    }
}
