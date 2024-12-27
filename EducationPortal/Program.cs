using AspNetCoreHero.ToastNotification;
using EducationPortal.Hubs;
using EducationPortal.Models;
using EducationPortal.Repositories;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "UserAuthCookie"; // Cookie adı
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30); // Süre
        options.SlidingExpiration = true; // Süreyi yenileme
    });

builder.Services.AddDbContext<AppDbContext>(opt =>
{
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// ASP.NET Identity yapılandırması
// Configure Identity for Authentication and Authorization
builder.Services.AddIdentity<UserAccount, UserRole>(options =>
{
    // Parola gereksinimleri için örnek ayarlar
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;

    // Kullanıcı oturum süresi ayarları
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(0.1);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    options.SignIn.RequireConfirmedEmail = false; // İsteğe bağlı: E-posta onayı zorunlu değil
})
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// RoleManager<UserRole> hizmetini kaydetme
builder.Services.AddScoped<RoleManager<UserRole>>();

// Dependency Injection (DI) için Repository'leri tanıtma
builder.Services.AddScoped<CourseRepository>();
builder.Services.AddScoped<LessonRepository>();

// SignalR servisini ekle
builder.Services.AddSignalR();

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts(); // Production ortamında HSTS kullanılır
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Enable Authentication and Authorization middleware
app.UseAuthentication();
app.UseAuthorization();

// Default routing
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Rolleri oluşturma
await SeedRolesAsync(app.Services);

// SignalR endpoint'ini ekle
app.MapHub<NotificationHub>("/notificationHub");

app.Run();

/// <summary>
/// Rolleri oluşturur.
/// </summary>
async Task SeedRolesAsync(IServiceProvider services)
{
    using (var scope = services.CreateScope())
    {
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<UserRole>>();

        // Rolleri oluştur
        var roles = new[] { "Admin", "Teacher", "Student" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new UserRole(role));
            }
        }
    }
}
