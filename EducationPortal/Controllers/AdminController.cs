using EducationPortal.Models;
using EducationPortal.ViewModels;
using EducationPortal.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using EducationPortal.Hubs;
using System.Security.Claims;

namespace EducationPortal.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;
        private readonly CourseRepository _courseRepository;
        private readonly UserManager<UserAccount> _userManager;
        private readonly IHubContext<NotificationHub> _hubContext;

        public AdminController(
            AppDbContext context,
            CourseRepository courseRepository,
            UserManager<UserAccount> userManager,
            IHubContext<NotificationHub> hubContext)
        {
            _context = context;
            _courseRepository = courseRepository;
            _userManager = userManager;
            _hubContext = hubContext;
        }

        // Ana sayfa
        public async Task<IActionResult> Index()
        {
            ViewBag.CourseCount = await _context.Courses.CountAsync();
            return View();
        }

        // Yeni kurs oluşturma sayfası
        public async Task<IActionResult> CreateCourse()
        {
            // Öğretmenleri getir
            var teachers = await _userManager.GetUsersInRoleAsync("Teacher");
            ViewBag.Teachers = new SelectList(teachers, "Id", "FullName");

            return View(new CourseViewModel());
        }

        // Yeni kurs oluşturma işlemi
        [HttpPost]
        public async Task<IActionResult> CreateCourse(CourseViewModel model, IFormFile thumbnailImage)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Resim yükleme işlemi
                    if (thumbnailImage != null)
                    {
                        var uniqueFileName = Guid.NewGuid().ToString() + "_" + thumbnailImage.FileName;
                        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "courses");
                        
                        if (!Directory.Exists(uploadsFolder))
                            Directory.CreateDirectory(uploadsFolder);
                        
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await thumbnailImage.CopyToAsync(fileStream);
                        }
                        
                        model.ThumbnailImagePath = "/uploads/courses/" + uniqueFileName;
                    }

                    await _courseRepository.AddAsync(model);

                    // SignalR ile bildirim gönder
                    await _hubContext.Clients.All.SendAsync("ReceiveNotification", $"Yeni kurs eklendi: {model.Title}");

                    return Json(new { success = true });
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = "Kurs oluşturulurken bir hata oluştu: " + ex.Message });
                }
            }

            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            return Json(new { success = false, message = "Lütfen tüm zorunlu alanları doldurun.", errors = errors });
        }

        // Kursları listele
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

        // Kurs düzenleme sayfası
        public async Task<IActionResult> EditCourse(int id)
        {
            var course = await _context.Courses
                .Include(c => c.Lessons)
                .Include(c => c.Teacher)
                .Include(c => c.EnrolledStudents)
                .FirstOrDefaultAsync(c => c.CourseId == id);

            if (course == null)
                return NotFound();

            var teachers = await _userManager.GetUsersInRoleAsync("Teacher");
            ViewBag.Teachers = new SelectList(teachers, "Id", "FullName", course.TeacherId);

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
                    var teachers = await _userManager.GetUsersInRoleAsync("Teacher");
                    ViewBag.Teachers = new SelectList(teachers, "Id", "FullName", model.TeacherId);
                    TempData["ErrorMessage"] = "Lütfen tüm zorunlu alanları doldurun.";
                    return View(model);
                }

                var existingCourse = await _context.Courses
                    .FirstOrDefaultAsync(c => c.CourseId == model.CourseId);

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
                existingCourse.TeacherId = model.TeacherId;

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Kurs başarıyla güncellendi.";
                return RedirectToAction(nameof(Courses));
            }
            catch (Exception ex)
            {
                var teachers = await _userManager.GetUsersInRoleAsync("Teacher");
                ViewBag.Teachers = new SelectList(teachers, "Id", "FullName", model.TeacherId);
                TempData["ErrorMessage"] = "Kurs güncellenirken bir hata oluştu.";
                return View(model);
            }
        }

        // Kurs silme işlemi
        [HttpPost]
        public async Task<IActionResult> DeleteCourse(int id)
        {
            try
            {
                var course = await _context.Courses.FindAsync(id);
                if (course == null)
                {
                    return Json(new { success = false, message = "Kurs bulunamadı." });
                }

                _context.Courses.Remove(course);
                await _context.SaveChangesAsync();

                // SignalR ile bildirim gönder
                await _hubContext.Clients.All.SendAsync("ReceiveNotification", $"{course.Title} kursu silindi.");

                return Json(new { success = true });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Kurs silinirken bir hata oluştu." });
            }
        }

        // Kullanıcıları listele
        public async Task<IActionResult> Users()
        {
            var users = await _userManager.Users.ToListAsync();
            
            foreach (var user in users)
            {
                user.Roles = await _userManager.GetRolesAsync(user);
            }
            
            return View(users);
        }

        // Kullanıcı rollerini düzenleme sayfası
        public async Task<IActionResult> EditUserRoles(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Kullanıcı bulunamadı.";
                return RedirectToAction(nameof(Users));
            }

            var model = new List<UserRoleViewModel>();
            var roles = await _context.Roles.ToListAsync();

            foreach (var role in roles)
            {
                var userRoleViewModel = new UserRoleViewModel
                {
                    RoleId = role.Id,
                    RoleName = role.Name,
                    IsSelected = await _userManager.IsInRoleAsync(user, role.Name)
                };
                model.Add(userRoleViewModel);
            }

            ViewBag.UserName = $"{user.FirstName} {user.LastName}";
            ViewBag.UserId = user.Id;

            return View(model);
        }

        // Kullanıcı rollerini güncelleme işlemi
        [HttpPost]
        public async Task<IActionResult> EditUserRoles(string userId, string selectedRole)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Kullanıcı bulunamadı.";
                return RedirectToAction(nameof(Users));
            }

            try
            {
                // Mevcut tüm rolleri kaldır
                var currentRoles = await _userManager.GetRolesAsync(user);
                if (currentRoles.Any())
                {
                    await _userManager.RemoveFromRolesAsync(user, currentRoles);
                }

                // Yeni rolü ata
                if (!string.IsNullOrEmpty(selectedRole))
                {
                    var result = await _userManager.AddToRoleAsync(user, selectedRole);
                    if (!result.Succeeded)
                    {
                        TempData["ErrorMessage"] = "Rol atanırken bir hata oluştu.";
                        return RedirectToAction(nameof(Users));
                    }
                }

                TempData["SuccessMessage"] = "Kullanıcı rolü başarıyla güncellendi.";
                return RedirectToAction(nameof(Users));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Rol güncellenirken bir hata oluştu: " + ex.Message;
                return RedirectToAction(nameof(Users));
            }
        }

        // Dersleri listele
        public async Task<IActionResult> Lessons(int courseId)
        {
            var course = await _context.Courses
                .Include(c => c.Teacher)
                .Include(c => c.Lessons)
                .FirstOrDefaultAsync(c => c.CourseId == courseId);

            if (course == null)
            {
                TempData["ErrorMessage"] = "Kurs bulunamadı.";
                return RedirectToAction(nameof(Courses));
            }

            ViewBag.CourseId = courseId;
            ViewBag.CourseTitle = course.Title;
            return View(course.Lessons);
        }

        // Yeni ders oluşturma sayfası
        public IActionResult CreateLesson(int courseId)
        {
            var course = _context.Courses.FirstOrDefault(c => c.CourseId == courseId);
            if (course == null)
            {
                TempData["ErrorMessage"] = "Kurs bulunamadı.";
                return RedirectToAction(nameof(Courses));
            }

            ViewBag.CourseId = courseId;
            ViewBag.CourseTitle = course.Title;
            return View(new LessonViewModel { CourseId = courseId });
        }

        // Yeni ders oluşturma işlemi
        [HttpPost]
        public async Task<IActionResult> CreateLesson(int courseId, string title, string description, string videoUrl)
        {
            try
            {
                // Gelen verileri kontrol et
                System.Diagnostics.Debug.WriteLine($"Gelen veriler: courseId={courseId}, title={title}, description={description}, videoUrl={videoUrl}");

                if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(description) || string.IsNullOrEmpty(videoUrl))
                {
                    TempData["ErrorMessage"] = "Lütfen tüm alanları doldurun.";
                    return RedirectToAction(nameof(CreateLesson), new { courseId });
                }

                // Kursu kontrol et
                var course = await _context.Courses.FindAsync(courseId);
                if (course == null)
                {
                    TempData["ErrorMessage"] = "Kurs bulunamadı.";
                    return RedirectToAction(nameof(Courses));
                }

                // Son sıra numarasını bul
                var lastOrderNumber = 0;
                try
                {
                    lastOrderNumber = await _context.Lessons
                        .Where(l => l.CourseId == courseId)
                        .MaxAsync(l => (int?)l.OrderNumber) ?? 0;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Sıra numarası hesaplama hatası: {ex.Message}");
                    // Hata olsa bile devam et, varsayılan 0 kullanılacak
                }

                // Yeni ders oluştur
                var lesson = new Lesson
                {
                    Title = title?.Trim(),
                    Description = description?.Trim(),
                    VideoUrl = videoUrl?.Trim(),
                    CourseId = courseId,
                    OrderNumber = lastOrderNumber + 1
                };

                try
                {
                    _context.Lessons.Add(lesson);
                    await _context.SaveChangesAsync();
                    
                    TempData["SuccessMessage"] = "Ders başarıyla oluşturuldu.";
                    return RedirectToAction(nameof(Lessons), new { courseId });
                }
                catch (DbUpdateException dbEx)
                {
                    System.Diagnostics.Debug.WriteLine($"Veritabanı güncelleme hatası: {dbEx.Message}");
                    System.Diagnostics.Debug.WriteLine($"İç hata: {dbEx.InnerException?.Message}");
                    TempData["ErrorMessage"] = $"Veritabanı hatası: {dbEx.InnerException?.Message ?? dbEx.Message}";
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Kaydetme hatası: {ex.Message}");
                    TempData["ErrorMessage"] = $"Kaydetme hatası: {ex.Message}";
                }

                return RedirectToAction(nameof(CreateLesson), new { courseId });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Genel hata: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                TempData["ErrorMessage"] = $"Ders oluşturulurken bir hata oluştu: {ex.Message}";
                return RedirectToAction(nameof(CreateLesson), new { courseId });
            }
        }

        // Ders düzenleme sayfası
        public async Task<IActionResult> EditLesson(int id)
        {
            var lesson = await _context.Lessons
                .Include(l => l.Course)
                .FirstOrDefaultAsync(l => l.LessonId == id);

            if (lesson == null)
            {
                TempData["ErrorMessage"] = "Ders bulunamadı.";
                return RedirectToAction(nameof(Courses));
            }

            var viewModel = new LessonViewModel
            {
                LessonId = lesson.LessonId,
                Title = lesson.Title,
                Description = lesson.Description,
                VideoUrl = lesson.VideoUrl,
                OrderNumber = lesson.OrderNumber,
                CourseId = lesson.CourseId,
                Course = lesson.Course
            };

            ViewBag.CourseTitle = lesson.Course?.Title;
            return View(viewModel);
        }

        // Ders düzenleme işlemi
        [HttpPost]
        public async Task<IActionResult> EditLesson(int lessonId, int courseId, string title, string description, string videoUrl, int orderNumber)
        {
            try
            {
                if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(description) || string.IsNullOrEmpty(videoUrl))
                {
                    TempData["ErrorMessage"] = "Lütfen tüm alanları doldurun.";
                    return RedirectToAction(nameof(EditLesson), new { id = lessonId });
                }

                var lesson = await _context.Lessons.FindAsync(lessonId);
                if (lesson == null)
                {
                    TempData["ErrorMessage"] = "Ders bulunamadı.";
                    return RedirectToAction(nameof(Courses));
                }

                lesson.Title = title;
                lesson.Description = description;
                lesson.VideoUrl = videoUrl;
                lesson.OrderNumber = orderNumber;

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Ders başarıyla güncellendi.";
                return RedirectToAction(nameof(Lessons), new { courseId });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Ders güncellenirken bir hata oluştu: " + ex.Message;
                return RedirectToAction(nameof(EditLesson), new { id = lessonId });
            }
        }

        // Ders silme işlemi
        [HttpPost]
        public async Task<IActionResult> DeleteLesson(int id)
        {
            var lesson = await _context.Lessons.FindAsync(id);
            if (lesson == null)
            {
                TempData["ErrorMessage"] = "Ders bulunamadı.";
                return RedirectToAction(nameof(Courses));
            }

            try
            {
                var courseId = lesson.CourseId;
                _context.Lessons.Remove(lesson);
                await _context.SaveChangesAsync();

                // Sıra numaralarını güncelle
                var remainingLessons = await _context.Lessons
                    .Where(l => l.CourseId == courseId && l.OrderNumber > lesson.OrderNumber)
                    .ToListAsync();

                foreach (var remainingLesson in remainingLessons)
                {
                    remainingLesson.OrderNumber--;
                }
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Ders başarıyla silindi.";
                return RedirectToAction(nameof(Lessons), new { courseId });
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Ders silinirken bir hata oluştu.";
                return RedirectToAction(nameof(Lessons), new { courseId = lesson.CourseId });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUser(string id)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    return Json(new { success = false, message = "Kullanıcı bulunamadı." });
                }

                // Admin kendisini silemesin
                if (User.FindFirst(ClaimTypes.NameIdentifier)?.Value == id)
                {
                    return Json(new { success = false, message = "Kendinizi silemezsiniz." });
                }

                // Kullanıcının kurslarını kontrol et
                var hasTeacherCourses = await _context.Courses.AnyAsync(c => c.TeacherId == id);
                if (hasTeacherCourses)
                {
                    return Json(new { success = false, message = "Bu öğretmenin aktif kursları olduğu için silinemez." });
                }

                // Kullanıcıyı sil
                var result = await _userManager.DeleteAsync(user);
                if (result.Succeeded)
                {
                    // SignalR ile bildirim gönder
                    await _hubContext.Clients.All.SendAsync("ReceiveNotification", $"{user.FullName} kullanıcısı silindi.");
                    return Json(new { success = true });
                }
                else
                {
                    return Json(new { success = false, message = "Kullanıcı silinirken bir hata oluştu." });
                }
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Kullanıcı silinirken bir hata oluştu." });
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Kullanıcı bulunamadı.";
                return RedirectToAction(nameof(Users));
            }

            var model = new EditUserViewModel
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                CurrentRole = (await _userManager.GetRolesAsync(user)).FirstOrDefault()
            };

            // Rolleri getir
            ViewBag.Roles = new SelectList(new[] { "Admin", "Teacher", "Student" });

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditUser(EditUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Roles = new SelectList(new[] { "Admin", "Teacher", "Student" });
                return View(model);
            }

            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Kullanıcı bulunamadı.";
                return RedirectToAction(nameof(Users));
            }

            try
            {
                // Kullanıcı bilgilerini güncelle
                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.Email = model.Email;
                user.UserName = model.Email; // Email'i username olarak da kullan

                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    ModelState.AddModelError("", "Kullanıcı güncellenirken bir hata oluştu.");
                    ViewBag.Roles = new SelectList(new[] { "Admin", "Teacher", "Student" });
                    return View(model);
                }

                // Rol güncelleme
                var currentRoles = await _userManager.GetRolesAsync(user);
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
                
                if (!string.IsNullOrEmpty(model.NewRole))
                {
                    await _userManager.AddToRoleAsync(user, model.NewRole);
                }

                // SignalR ile bildirim gönder
                await _hubContext.Clients.All.SendAsync("ReceiveNotification", $"{user.FullName} kullanıcısının bilgileri güncellendi.");

                TempData["SuccessMessage"] = "Kullanıcı başarıyla güncellendi.";
                return RedirectToAction(nameof(Users));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Bir hata oluştu: " + ex.Message);
                ViewBag.Roles = new SelectList(new[] { "Admin", "Teacher", "Student" });
                return View(model);
            }
        }
    }
}

