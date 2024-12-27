using EducationPortal.Models;
using EducationPortal.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EducationPortal.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentController : Controller
    {
        private readonly AppDbContext _context;
        private readonly CourseRepository _courseRepository;

        public StudentController(AppDbContext context, CourseRepository courseRepository)
        {
            _context = context;
            _courseRepository = courseRepository;
        }

        // Tüm kursları listele
        public async Task<IActionResult> Courses()
        {
            var courses = await _context.Courses
                .Include(c => c.Teacher)
                .Include(c => c.Lessons)
                .Include(c => c.EnrolledStudents)
                .OrderByDescending(c => c.CreatedDate)
                .ToListAsync();

            return View(courses);
        }

        // Kayıtlı kursları listele
        public async Task<IActionResult> MyCourses()
        {
            var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var courses = await _context.Courses
                .Include(c => c.Teacher)
                .Include(c => c.Lessons)
                .Include(c => c.EnrolledStudents)
                .Where(c => c.EnrolledStudents.Any(s => s.Id == studentId))
                .OrderByDescending(c => c.CreatedDate)
                .ToListAsync();

            return View(courses);
        }

        // Kurs detayı
        public async Task<IActionResult> CourseDetails(int id)
        {
            var course = await _context.Courses
                .Include(c => c.Teacher)
                .Include(c => c.Lessons)
                .Include(c => c.EnrolledStudents)
                .FirstOrDefaultAsync(c => c.CourseId == id);

            if (course == null)
            {
                TempData["ErrorMessage"] = "Kurs bulunamadı.";
                return RedirectToAction(nameof(Courses));
            }

            return View(course);
        }

        // Kursa kayıt ol
        [HttpPost]
        public async Task<IActionResult> EnrollCourse(int courseId)
        {
            try
            {
                var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var result = await _courseRepository.EnrollStudent(courseId, studentId);
                if (result)
                {
                    TempData["SuccessMessage"] = "Kursa başarıyla kaydoldunuz.";
                    return RedirectToAction(nameof(MyCourses));
                }
                else
                {
                    TempData["ErrorMessage"] = "Kursa kayıt olurken bir hata oluştu. Lütfen daha sonra tekrar deneyin.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Kursa kayıt olurken bir hata oluştu: " + ex.Message;
            }

            return RedirectToAction(nameof(CourseDetails), new { id = courseId });
        }
    }
} 