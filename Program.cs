using CehrHealthCommerce.Data;
using CehrHealthCommerce.Models;
using CehrHealthCommerce.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------------------------
// Database (EF Core + SQL Server)
// ---------------------------------------------------------------------------
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ---------------------------------------------------------------------------
// ASP.NET Core Identity (roles: Admin, Customer)
// ---------------------------------------------------------------------------
builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
        options.User.RequireUniqueEmail = true;

        // Password policy (security requirement).
        options.Password.RequiredLength = 6;
        options.Password.RequireDigit = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireNonAlphanumeric = false;

        // Lockout protects against brute-force login attempts.
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Secure authentication cookie configuration.
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.Cookie.HttpOnly = true;                       // not readable by JavaScript (XSS defence)
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // sent only over HTTPS
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.ExpireTimeSpan = TimeSpan.FromHours(2);
    options.SlidingExpiration = true;
});

// ---------------------------------------------------------------------------
// Application services (registered as they are introduced across phases)
// ---------------------------------------------------------------------------
builder.Services.AddScoped<ICitizenIdentityService, CitizenIdentityService>();
builder.Services.AddScoped<ICehrIntegrationService, CehrIntegrationService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IRecommendationService, ContentBasedRecommendationService>();

// eSewa sandbox payment gateway.
builder.Services.Configure<EsewaOptions>(builder.Configuration.GetSection("Esewa"));
builder.Services.AddHttpClient();
builder.Services.AddScoped<IPaymentService, EsewaPaymentService>();

builder.Services.AddHttpContextAccessor();

// Enforce anti-forgery (CSRF) validation on every non-GET request application-wide,
// in addition to the explicit [ValidateAntiForgeryToken] attributes on each POST action.
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
});

var app = builder.Build();

// ---------------------------------------------------------------------------
// Security response headers (applied to every response).
// ---------------------------------------------------------------------------
app.Use(async (context, next) =>
{
    var headers = context.Response.Headers;
    headers["X-Content-Type-Options"] = "nosniff";          // stop MIME-type sniffing
    headers["X-Frame-Options"] = "DENY";                     // clickjacking protection (legacy)
    headers["Referrer-Policy"] = "no-referrer";              // don't leak URLs to third parties
    headers["X-Permitted-Cross-Domain-Policies"] = "none";

    // Content Security Policy. 'unsafe-inline' is permitted for scripts/styles because the
    // demo uses small inline blocks (e.g. cascading dropdowns, eSewa auto-submit); everything
    // else is locked to self, the jsDelivr CDN, eSewa (payment form) and Google Analytics.
    headers["Content-Security-Policy"] =
        "default-src 'self'; " +
        "script-src 'self' 'unsafe-inline' https://cdn.jsdelivr.net https://www.googletagmanager.com; " +
        "style-src 'self' 'unsafe-inline' https://cdn.jsdelivr.net; " +
        "font-src 'self' https://cdn.jsdelivr.net; " +
        "img-src 'self' data: https://www.google-analytics.com; " +
        "connect-src 'self' https://www.google-analytics.com; " +
        "form-action 'self' https://rc-epay.esewa.com.np; " +
        "frame-ancestors 'none'; " +
        "base-uri 'self'";

    await next();
});

// ---------------------------------------------------------------------------
// HTTP request pipeline
// ---------------------------------------------------------------------------
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Area routes (Admin) first, then the default route.
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// ---------------------------------------------------------------------------
// Apply migrations and seed roles, demo users and catalog data at startup.
// ---------------------------------------------------------------------------
using (var scope = app.Services.CreateScope())
{
    await DbSeeder.SeedAsync(scope.ServiceProvider);
}

app.Run();
