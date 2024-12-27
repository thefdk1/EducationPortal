using System.ComponentModel.DataAnnotations;
using EducationPortal.Models;

namespace EducationPortal.ViewModels
{
    public class CourseViewModel
    {
        public int CourseId { get; set; }

        [Required(ErrorMessage = "Kurs başlığı zorunludur.")]
        [Display(Name = "Kurs Başlığı")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Kurs açıklaması zorunludur.")]
        [Display(Name = "Açıklama")]
        public string Description { get; set; }

        [Display(Name = "Kapak Resmi")]
        public string? ThumbnailImagePath { get; set; }

        [Required(ErrorMessage = "Lütfen bir öğretmen seçin.")]
        [Display(Name = "Öğretmen")]
        public string TeacherId { get; set; }

        [Display(Name = "Öğretmen")]
        public UserAccount? Teacher { get; set; }

        [Display(Name = "Oluşturulma Tarihi")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public List<LessonViewModel> Lessons { get; set; } = new();

        public List<UserAccount> EnrolledStudents { get; set; } = new();
    }
}

