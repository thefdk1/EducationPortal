using EducationPortal.Models;
using EducationPortal.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace EducationPortal.Repositories
{
    public class CourseRepository
    {
        private readonly AppDbContext _context;

        public CourseRepository(AppDbContext context)
        {
            _context = context;
        }

        public List<CourseViewModel> GetList()
        {
            var courses = _context.Courses
                .Include(c => c.Teacher)
                .Include(c => c.Lessons)
                .Include(c => c.EnrolledStudents)
                .Select(x => new CourseViewModel()
                {
                    CourseId = x.CourseId,
                    Title = x.Title,
                    Description = x.Description,
                    ThumbnailImagePath = x.ThumbnailImagePath,
                    CreatedDate = x.CreatedDate,
                    TeacherId = x.TeacherId,
                    Teacher = x.Teacher,
                    Lessons = x.Lessons.Select(l => new LessonViewModel
                    {
                        LessonId = l.LessonId,
                        Title = l.Title,
                        Description = l.Description,
                        VideoUrl = l.VideoUrl,
                        OrderNumber = l.OrderNumber,
                        CourseId = l.CourseId
                    }).ToList(),
                    EnrolledStudents = x.EnrolledStudents
                }).ToList();
            return courses;
        }

        public CourseViewModel GetById(int id)
        {
            var course = _context.Courses
                .Include(c => c.Teacher)
                .Include(c => c.Lessons)
                .Include(c => c.EnrolledStudents)
                .Where(s => s.CourseId == id)
                .Select(x => new CourseViewModel()
                {
                    CourseId = x.CourseId,
                    Title = x.Title,
                    Description = x.Description,
                    ThumbnailImagePath = x.ThumbnailImagePath,
                    CreatedDate = x.CreatedDate,
                    TeacherId = x.TeacherId,
                    Teacher = x.Teacher,
                    Lessons = x.Lessons.Select(l => new LessonViewModel
                    {
                        LessonId = l.LessonId,
                        Title = l.Title,
                        Description = l.Description,
                        VideoUrl = l.VideoUrl,
                        OrderNumber = l.OrderNumber,
                        CourseId = l.CourseId
                    }).ToList(),
                    EnrolledStudents = x.EnrolledStudents
                }).FirstOrDefault();

            return course;
        }

        public async Task AddAsync(CourseViewModel model)
        {
            var course = new Course()
            {
                Title = model.Title,
                Description = model.Description,
                ThumbnailImagePath = model.ThumbnailImagePath,
                TeacherId = model.TeacherId,
                CreatedDate = model.CreatedDate,
                Lessons = model.Lessons?.Select(l => new Lesson
                {
                    Title = l.Title,
                    Description = l.Description,
                    VideoUrl = l.VideoUrl,
                    OrderNumber = l.OrderNumber,
                    CourseId = l.CourseId
                }).ToList() ?? new List<Lesson>()
            };
            
            await _context.Courses.AddAsync(course);
            await _context.SaveChangesAsync();
        }

        public void Update(CourseViewModel model)
        {
            var course = _context.Courses.Where(c => c.CourseId == model.CourseId).FirstOrDefault();
            if (course != null)
            {
                course.Title = model.Title;
                course.Description = model.Description;
                course.ThumbnailImagePath = model.ThumbnailImagePath;
                course.Lessons = model.Lessons.Select(l => new Lesson
                {
                    LessonId = l.LessonId,
                    Title = l.Title,
                    VideoUrl = l.VideoUrl,
                    CourseId = l.CourseId
                }).ToList();

                _context.Courses.Update(course);
                _context.SaveChanges();
            }
        }

        public void Delete(int id)
        {
            var course = _context.Courses.Where(c => c.CourseId == id).FirstOrDefault();
            if (course != null)
            {
                _context.Courses.Remove(course);
                _context.SaveChanges();
            }
        }

        public async Task<bool> EnrollStudent(int courseId, string userId)
        {
            var course = await _context.Courses
                .Include(c => c.EnrolledStudents)
                .FirstOrDefaultAsync(c => c.CourseId == courseId);

            if (course == null)
                return false;

            var student = await _context.Users.FindAsync(userId);
            if (student == null)
                return false;

            if (!course.EnrolledStudents.Any(s => s.Id == userId))
            {
                course.EnrolledStudents.Add(student);
                await _context.SaveChangesAsync();
            }

            return true;
        }
    }
}


