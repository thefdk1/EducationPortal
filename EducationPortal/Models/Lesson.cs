using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EducationPortal.Models
{
    public class Lesson
    {
        [Key]
        public int LessonId { get; set; }

        [Required(ErrorMessage = "Ders başlığı zorunludur.")]
        [StringLength(200, ErrorMessage = "Ders başlığı en fazla 200 karakter olabilir.")]
        [Display(Name = "Ders Başlığı")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Ders açıklaması zorunludur.")]
        [Display(Name = "Açıklama")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Video URL'si zorunludur.")]
        [Url(ErrorMessage = "Geçerli bir URL giriniz.")]
        [Display(Name = "Video URL")]
        public string VideoUrl { get; set; }

        [Display(Name = "Sıra No")]
        public int OrderNumber { get; set; }

        [Required]
        public int CourseId { get; set; }

        [ForeignKey("CourseId")]
        public virtual Course Course { get; set; }

        [Display(Name = "Oluşturulma Tarihi")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public Lesson()
        {
            Title = string.Empty;
            Description = string.Empty;
            VideoUrl = string.Empty;
        }
    }
}
