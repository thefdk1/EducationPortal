using System.ComponentModel.DataAnnotations;
using EducationPortal.Models;

namespace EducationPortal.ViewModels
{
    public class LessonViewModel
    {
        public int LessonId { get; set; }

        [Required(ErrorMessage = "Ders başlığı zorunludur.")]
        [Display(Name = "Ders Başlığı")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Ders açıklaması zorunludur.")]
        [Display(Name = "Açıklama")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Video URL'si zorunludur.")]
        [Display(Name = "Video URL")]
        [Url(ErrorMessage = "Geçerli bir URL giriniz.")]
        public string VideoUrl { get; set; }

        [Display(Name = "Sıra No")]
        public int OrderNumber { get; set; }

        public int CourseId { get; set; }
        public Course Course { get; set; }
    }
}
