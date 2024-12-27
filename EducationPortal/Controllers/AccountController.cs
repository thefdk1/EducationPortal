using System.Security.Claims;
using EducationPortal.Models;
using EducationPortal.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EducationPortal.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<UserAccount> _userManager;
        private readonly SignInManager<UserAccount> _signInManager;
        private readonly RoleManager<UserRole> _roleManager;

        public AccountController(
            AppDbContext appDbContext,
            UserManager<UserAccount> userManager,
            SignInManager<UserAccount> signInManager,
            RoleManager<UserRole> roleManager)
        {
            _context = appDbContext ?? throw new ArgumentNullException(nameof(appDbContext));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
            _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
        }

        public IActionResult Index()
        {
            return View(_context.UserAccounts.ToList());
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new UserAccount
                {
                    UserName = model.UserName,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    // Kullanıcıya Student rolü atama
                    var roleResult = await _userManager.AddToRoleAsync(user, "Student");

                    if (roleResult.Succeeded)
                    {
                        TempData["SuccessMessage"] = $"{user.FullName} başarıyla kayıt oldu. Lütfen giriş yapınız!";
                        return RedirectToAction("Login");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Kullanıcı oluşturuldu fakat rol ataması başarısız oldu.");
                    }
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            return View(model);
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ModelState.AddModelError(string.Empty, "Kullanıcı adı ve şifre gereklidir.");
                return View();
            }

            var user = await _userManager.FindByNameAsync(username);
            if (user != null && await _userManager.CheckPasswordAsync(user, password))
            {
                // Kullanıcı rolünü kontrol et
                var roles = await _userManager.GetRolesAsync(user);

                // Kullanıcıyı giriş yap
                await _signInManager.SignInAsync(user, isPersistent: false);

                if (roles.Contains("Admin"))
                {
                    return RedirectToAction("Index", "Admin");
                }
                else
                {
                    return RedirectToAction("Index", "Home");
                }
            }

            // Geçersiz kullanıcı adı veya şifre
            ModelState.AddModelError(string.Empty, "Geçersiz kullanıcı adı veya şifre.");
            return View();
        }

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}