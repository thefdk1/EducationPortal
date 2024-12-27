using EducationPortal.Models;
using EducationPortal.ViewModels;
using EducationPortal.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EducationPortal.Controllers
{
    [Authorize(Roles = "Teacher")]
    public class TeacherController : Controller
    {
        private readonly AppDbContext _context;
        private readonly CourseRepository _courseRepository;

        public TeacherController(AppDbContext context, CourseRepository courseRepository)
        {
            _context = context;
            _courseRepository = courseRepository;
        }

        // Öğretmenin kurslarını listeler
        public async Task<IActionResult> MyCourses()
        {
            var teacherId = User.Identity.Name;
            var courses = await _context.Courses
                .Include(c => c.Teacher)
                .Include(c => c.Lessons)
                .Include(c => c.EnrolledStudents)
                .Where(c => c.TeacherId == teacherId)
                .OrderByDescending(c => c.CreatedDate)
                .ToListAsync();

            return View(courses);
        }

        // Yeni kurs oluşturma sayfası
        public IActionResult CreateCourse()
        {
            return View(new CourseViewModel());
        }

        // Yeni kurs oluşturma işlemi
        [HttpPost]
        public async Task<IActionResult> CreateCourse(CourseViewModel model, IFormFile thumbnailImage)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    TempData["ErrorMessage"] = "Lütfen tüm zorunlu alanları doldurun.";
                    return View(model);
                }

                if (thumbnailImage != null)
                {
                    var fileName = $"{Guid.NewGuid()}{Path.GetExtension(thumbnailImage.FileName)}";
                    var filePath = Path.Combine("wwwroot", "images", "courses", fileName);
                    
                    Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await thumbnailImage.CopyToAsync(stream);
                    }
                    
                    model.ThumbnailImagePath = $"/images/courses/{fileName}";
                }

                model.TeacherId = User.Identity.Name;
                model.CreatedDate = DateTime.Now;

                await _courseRepository.AddAsync(model);

                TempData["SuccessMessage"] = "Kurs başarıyla oluşturuldu.";
                return RedirectToAction(nameof(MyCourses));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Kurs oluşturulurken bir hata oluştu.";
                return View(model);
            }
        }

        // Kurs düzenleme sayfası
        public async Task<IActionResult> EditCourse(int id)
        {
            var course = await _context.Courses
                .Include(c => c.Lessons)
                .Include(c => c.Teacher)
                .Include(c => c.EnrolledStudents)
                .FirstOrDefaultAsync(c => c.CourseId == id && c.TeacherId == User.Identity.Name);

            if (course == null)
                return NotFound();

            var viewModel = new CourseViewModel
            {
                CourseId = course.CourseId,
                Title = course.Title,
                Description = course.Description,
                ThumbnailImagePath = course.ThumbnailImagePath,
                TeacherId = course.TeacherId,
                Teacher = course.Teacher,
                CreatedDate = course.CreatedDate,
                Lessons = course.Lessons.Select(l => new LessonViewModel
                {
                    LessonId = l.LessonId,
                    Title = l.Title,
                    Description = l.Description,
                    VideoUrl = l.VideoUrl,
                    OrderNumber = l.OrderNumber,
                    CourseId = l.CourseId
                }).ToList(),
                EnrolledStudents = course.EnrolledStudents
            };

            return View(viewModel);
        }

        // Kurs düzenleme işlemi
        [HttpPost]
        public async Task<IActionResult> EditCourse(CourseViewModel model, IFormFile thumbnailImage)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    TempData["ErrorMessage"] = "Lütfen tüm zorunlu alanları doldurun.";
                    return View(model);
                }

                var existingCourse = await _context.Courses
                    .FirstOrDefaultAsync(c => c.CourseId == model.CourseId && c.TeacherId == User.Identity.Name);

                if (existingCourse == null)
                {
                    TempData["ErrorMessage"] = "Kurs bulunamadı.";
                    return NotFound();
                }

                if (thumbnailImage != null)
                {
                    // Eski resmi sil
                    if (!string.IsNullOrEmpty(existingCourse.ThumbnailImagePath))
                    {
                        var oldImagePath = Path.Combine("wwwroot", existingCourse.ThumbnailImagePath.TrimStart('/'));
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    // Yeni resmi kaydet
                    var fileName = $"{Guid.NewGuid()}{Path.GetExtension(thumbnailImage.FileName)}";
                    var filePath = Path.Combine("wwwroot", "images", "courses", fileName);
                    
                    Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await thumbnailImage.CopyToAsync(stream);
                    }
                    
                    existingCourse.ThumbnailImagePath = $"/images/courses/{fileName}";
                }

                existingCourse.Title = model.Title;
                existingCourse.Description = model.Description;

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Kurs başarıyla güncellendi.";
                return RedirectToAction(nameof(MyCourses));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Kurs güncellenirken bir hata oluştu.";
                return View(model);
            }
        }

        // Ders ekleme sayfası
        public async Task<IActionResult> AddLesson(int courseId)
        {
            var course = await _context.Courses
                .FirstOrDefaultAsync(c => c.CourseId == courseId && c.TeacherId == User.Identity.Name);

            if (course == null)
                return NotFound();

            ViewBag.CourseId = courseId;
            ViewBag.CourseTitle = course.Title;

            return View(new Lesson { CourseId = courseId });
        }

        // Ders ekleme işlemi
        [HttpPost]
        public async Task<IActionResult> AddLesson(Lesson lesson)
        {
            if (!ModelState.IsValid)
                return View(lesson);

            var course = await _context.Courses
                .FirstOrDefaultAsync(c => c.CourseId == lesson.CourseId && c.TeacherId == User.Identity.Name);

            if (course == null)
                return NotFound();

            _context.Lessons.Add(lesson);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(EditCourse), new { id = lesson.CourseId });
        }

        // Öğrenci listesi görüntüleme
        public async Task<IActionResult> CourseStudents(int courseId)
        {
            var course = await _context.Courses
                .Include(c => c.EnrolledStudents)
                .FirstOrDefaultAsync(c => c.CourseId == courseId && c.TeacherId == User.Identity.Name);

            if (course == null)
                return NotFound();

            return View(course);
        }

        // Kurs silme işlemi
        [HttpPost]
        public async Task<IActionResult> DeleteCourse(int id)
        {
            try
            {
                var course = await _context.Courses
                    .Include(c => c.Lessons)
                    .FirstOrDefaultAsync(c => c.CourseId == id && c.TeacherId == User.Identity.Name);

                if (course == null)
                {
                    TempData["ErrorMessage"] = "Kurs bulunamadı.";
                    return NotFound();
                }

                // Kurs kapak resmini sil
                if (!string.IsNullOrEmpty(course.ThumbnailImagePath))
                {
                    var imagePath = Path.Combine("wwwroot", course.ThumbnailImagePath.TrimStart('/'));
                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }
                }

                _context.Courses.Remove(course);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Kurs başarıyla silindi.";
                return RedirectToAction(nameof(MyCourses));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Kurs silinirken bir hata oluştu.";
                return RedirectToAction(nameof(MyCourses));
            }
        }

        // Ders düzenleme sayfası
        public async Task<IActionResult> EditLesson(int id)
        {
            var lesson = await _context.Lessons
                .Include(l => l.Course)
                .FirstOrDefaultAsync(l => l.LessonId == id && l.Course.TeacherId == User.Identity.Name);

            if (lesson == null)
            {
                TempData["ErrorMessage"] = "Ders bulunamadı.";
                return NotFound();
            }

            ViewBag.CourseId = lesson.CourseId;
            ViewBag.CourseTitle = lesson.Course.Title;

            return View(lesson);
        }

        // Ders düzenleme işlemi
        [HttpPost]
        public async Task<IActionResult> EditLesson(Lesson lesson)
        {
            try
            {
                if (!ModelState.IsValid)
                    return View(lesson);

                var existingLesson = await _context.Lessons
                    .Include(l => l.Course)
                    .FirstOrDefaultAsync(l => l.LessonId == lesson.LessonId && 
                                            l.Course.TeacherId == User.Identity.Name);

                if (existingLesson == null)
                {
                    TempData["ErrorMessage"] = "Ders bulunamadı.";
                    return NotFound();
                }

                existingLesson.Title = lesson.Title;
                existingLesson.Description = lesson.Description;
                existingLesson.VideoUrl = lesson.VideoUrl;
                existingLesson.OrderNumber = lesson.OrderNumber;

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Ders başarıyla güncellendi.";
                return RedirectToAction(nameof(EditCourse), new { id = existingLesson.CourseId });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Ders güncellenirken bir hata oluştu.";
                return View(lesson);
            }
        }

        // Ders silme işlemi
        [HttpPost]
        public async Task<IActionResult> DeleteLesson(int id)
        {
            try
            {
                var lesson = await _context.Lessons
                    .Include(l => l.Course)
                    .FirstOrDefaultAsync(l => l.LessonId == id && l.Course.TeacherId == User.Identity.Name);

                if (lesson == null)
                {
                    TempData["ErrorMessage"] = "Ders bulunamadı.";
                    return NotFound();
                }

                var courseId = lesson.CourseId;

                _context.Lessons.Remove(lesson);
                await _context.SaveChangesAsync();

                // Kalan derslerin sıra numaralarını güncelle
                var remainingLessons = await _context.Lessons
                    .Where(l => l.CourseId == courseId)
                    .OrderBy(l => l.OrderNumber)
                    .ToListAsync();

                for (int i = 0; i < remainingLessons.Count; i++)
                {
                    remainingLessons[i].OrderNumber = i + 1;
                }

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Ders başarıyla silindi.";
                return RedirectToAction(nameof(EditCourse), new { id = courseId });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Ders silinirken bir hata oluştu.";
                return RedirectToAction(nameof(MyCourses));
            }
        }
    }
} 