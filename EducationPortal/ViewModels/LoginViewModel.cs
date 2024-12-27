using System.ComponentModel.DataAnnotations;

namespace EducationPortal.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Kullanıcı Adı Zorunludur.")]
        [Display(Name = "Kullanıcı Adı")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Beni Hatırla")]
        public bool RememberMe { get; set; }
    }
}

