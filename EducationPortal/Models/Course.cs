using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EducationPortal.Models
{
    public class Course
    {
        [Key]
        public int CourseId { get; set; }

        [Required(ErrorMessage = "Kurs başlığı zorunludur.")]
        [Display(Name = "Kurs Başlığı")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Kurs açıklaması zorunludur.")]
        [Display(Name = "Açıklama")]
        public string Description { get; set; }

        [Display(Name = "Kapak Resmi")]
        public string? ThumbnailImagePath { get; set; }

        [Required(ErrorMessage = "Öğretmen seçimi zorunludur.")]
        [Display(Name = "Öğretmen")]
        public string TeacherId { get; set; }

        [ForeignKey("TeacherId")]
        public virtual UserAccount Teacher { get; set; }

        [Display(Name = "Oluşturulma Tarihi")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Kayıtlı öğrenciler
        public List<UserAccount> EnrolledStudents { get; set; } = new();

        // Dersler
        public List<Lesson> Lessons { get; set; } = new();
    }
}
