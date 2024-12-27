using EducationPortal.Models;
using EducationPortal.Repositories;
using EducationPortal.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace EducationPortal.Controllers
{
    public class HomeController : Controller
    {
        private readonly CourseRepository _courseRepository;

        public HomeController(CourseRepository courseRepository)
        {
            _courseRepository = courseRepository;
        }

        public IActionResult Index()
        {
            var courses = _courseRepository.GetList();
            return View(courses);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
