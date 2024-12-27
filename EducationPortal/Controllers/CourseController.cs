using EducationPortal.Models;
using EducationPortal.Repositories;
using EducationPortal.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EducationPortal.Controllers
{
    public class CourseController : Controller
    {
        private readonly CourseRepository _courseRepository;
        private readonly AppDbContext _context;

        public CourseController(CourseRepository courseRepository, AppDbContext context)
        {
            _courseRepository = courseRepository;
            _context = context;
        }

        public async Task<IActionResult> Index(string search, string sort, string[] teachers)
        {
            var courses = _courseRepository.GetList();

            // Arama filtresi
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.ToLower();
                courses = courses.Where(c => 
                    c.Title.ToLower().Contains(search) || 
                    c.Description.ToLower().Contains(search) ||
                    c.Teacher.FirstName.ToLower().Contains(search) ||
                    c.Teacher.LastName.ToLower().Contains(search)).ToList();
            }

            // Eğitmen filtresi
            if (teachers != null && teachers.Length > 0)
            {
                courses = courses.Where(c => teachers.Contains(c.TeacherId)).ToList();
            }

            // Sıralama
            courses = sort switch
            {
                "oldest" => courses.OrderBy(c => c.CreatedDate).ToList(),
                "name" => courses.OrderBy(c => c.Title).ToList(),
                _ => courses.OrderByDescending(c => c.CreatedDate).ToList() // "newest" veya varsayılan
            };

            return View(courses);
        }

        public async Task<IActionResult> Details(int id)
        {
            var course = _courseRepository.GetById(id);
            if (course == null)
            {
                return NotFound();
            }

            return View(course);
        }

        [HttpPost]
        public async Task<IActionResult> Enroll(int courseId)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            var userId = User.Identity.Name;
            var success = await _courseRepository.EnrollStudent(courseId, userId);

            if (!success)
            {
                return NotFound();
            }

            return RedirectToAction(nameof(Details), new { id = courseId });
        }
    }
}

