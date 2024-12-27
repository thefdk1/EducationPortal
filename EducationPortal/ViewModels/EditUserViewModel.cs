using System.ComponentModel.DataAnnotations;

namespace EducationPortal.ViewModels
{
    public class EditUserViewModel
    {
        public string Id { get; set; }

        [Required(ErrorMessage = "Ad alanı zorunludur.")]
        [Display(Name = "Ad")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Soyad alanı zorunludur.")]
        [Display(Name = "Soyad")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Email alanı zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir email adresi giriniz.")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Display(Name = "Mevcut Rol")]
        public string CurrentRole { get; set; }

        [Display(Name = "Yeni Rol")]
        public string NewRole { get; set; }
    }
} 