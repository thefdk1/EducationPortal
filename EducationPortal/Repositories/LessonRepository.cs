using EducationPortal.Models;
using Microsoft.EntityFrameworkCore;
using EducationPortal.ViewModels;

namespace EducationPortal.Repositories
{
    public class LessonRepository
    {
        private readonly AppDbContext _context;

        public LessonRepository(AppDbContext context)
        {
            _context = context;
        }

        public List<LessonViewModel> GetList()
        {
            var lessons = _context.Lessons.Select(x => new LessonViewModel()
            {
                LessonId = x.LessonId,
                Title = x.Title,
                VideoUrl = x.VideoUrl,
                CourseId = x.CourseId // CourseId ekleniyor
            }).ToList();
            return lessons;
        }

        public LessonViewModel GetById(int id)
        {
            var lesson = _context.Lessons.Where(s => s.LessonId == id).Select(x => new LessonViewModel()
            {
                LessonId = x.LessonId,
                Title = x.Title,
                VideoUrl = x.VideoUrl,
                CourseId = x.CourseId // CourseId ekleniyor
            }).FirstOrDefault();

            return lesson;
        }

        public void Add(LessonViewModel model)
        {
            // CourseId'nin geçerli bir kursa işaret ettiğinden emin olun
            var courseExists = _context.Courses.Any(c => c.CourseId == model.CourseId);
            if (!courseExists)
            {
                throw new Exception("Geçersiz CourseId. Bu CourseId'ye sahip bir kurs bulunamadı.");
            }

            var lesson = new Lesson()
            {
                Title = model.Title,
                VideoUrl = model.VideoUrl,
                CourseId = model.CourseId // Dersin hangi kursa ait olduğunu belirtin
            };

            _context.Lessons.Add(lesson);
            _context.SaveChanges();
        }

        public void Update(LessonViewModel model)
        {
            var lesson = _context.Lessons.Where(l => l.LessonId == model.LessonId).FirstOrDefault();
            if (lesson != null)
            {
                lesson.Title = model.Title;
                lesson.VideoUrl = model.VideoUrl;
                lesson.CourseId = model.CourseId; // CourseId ekleniyor

                // CourseId'nin geçerli bir kursa işaret ettiğinden emin olun
                var courseExists = _context.Courses.Any(c => c.CourseId == model.CourseId);
                if (!courseExists)
                {
                    throw new Exception("Geçersiz CourseId. Bu CourseId'ye sahip bir kurs bulunamadı.");
                }

                _context.Lessons.Update(lesson);
                _context.SaveChanges();
            }
        }

        public void Delete(int id)
        {
            var lesson = _context.Lessons.Where(l => l.LessonId == id).FirstOrDefault();
            if (lesson != null)
            {
                _context.Lessons.Remove(lesson);
                _context.SaveChanges();
            }
        }
    }
}



