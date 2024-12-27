using EducationPortal.Models;
using EducationPortal.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace EducationPortal.Controllers
{
    [Authorize(Roles = "Admin")]
    public class LessonController : Controller
    {
        private readonly AppDbContext _context;

        public LessonController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var lessons = await _context.Lessons
                .Include(l => l.Course)
                .OrderBy(l => l.OrderNumber)
                .Select(l => new LessonViewModel
                {
                    LessonId = l.LessonId,
                    Title = l.Title,
                    Description = l.Description,
                    VideoUrl = l.VideoUrl,
                    OrderNumber = l.OrderNumber,
                    CourseId = l.CourseId,
                })
                .ToListAsync();

            return View(lessons);
        }

        public async Task<IActionResult> Add(int courseId)
        {
            ViewBag.CourseId = courseId;
            var course = await _context.Courses.FindAsync(courseId);
            if (course == null)
            {
                return NotFound();
            }

            var model = new LessonViewModel
            {
                CourseId = courseId,
                OrderNumber = await _context.Lessons.Where(l => l.CourseId == courseId).CountAsync() + 1
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(LessonViewModel model)
        {
            if (ModelState.IsValid)
            {
                var lesson = new Lesson
                {
                    Title = model.Title,
                    Description = model.Description,
                    VideoUrl = model.VideoUrl,
                    OrderNumber = model.OrderNumber,
                    CourseId = model.CourseId,
                };

                _context.Lessons.Add(lesson);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Ders başarıyla eklendi.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.CourseId = model.CourseId;
            return View(model);
        }

        public async Task<IActionResult> Update(int id)
        {
            var lesson = await _context.Lessons.FindAsync(id);
            if (lesson == null)
            {
                return NotFound();
            }

            var model = new LessonViewModel
            {
                LessonId = lesson.LessonId,
                Title = lesson.Title,
                Description = lesson.Description,
                VideoUrl = lesson.VideoUrl,
                OrderNumber = lesson.OrderNumber,
                CourseId = lesson.CourseId
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(LessonViewModel model)
        {
            if (ModelState.IsValid)
            {
                var lesson = await _context.Lessons.FindAsync(model.LessonId);
                if (lesson == null)
                {
                    return NotFound();
                }

                lesson.Title = model.Title;
                lesson.Description = model.Description;
                lesson.VideoUrl = model.VideoUrl;
                lesson.OrderNumber = model.OrderNumber;

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Ders başarıyla güncellendi.";
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var lesson = await _context.Lessons.FindAsync(id);
            if (lesson == null)
            {
                return NotFound();
            }

            _context.Lessons.Remove(lesson);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Ders başarıyla silindi.";
            return RedirectToAction(nameof(Index));
        }
    }
}

