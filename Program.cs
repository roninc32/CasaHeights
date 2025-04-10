using CasaHeights.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using CasaHeights.Models;
using CasaHeights.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<Users, IdentityRole>(options =>
{
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireDigit = true;
    
    // Make sign-in not require confirmed account
    options.SignIn.RequireConfirmedAccount = false;
    options.SignIn.RequireConfirmedEmail = false;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// Configure cookie settings to not redirect to login
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromDays(30);
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.SlidingExpiration = true;
    
    // This prevents automatic redirects to the login page
    options.Events = new CookieAuthenticationEvents
    {
        OnRedirectToLogin = context =>
        {
            // Instead of redirecting to login, just return 401 status
            if (context.Request.Path.StartsWithSegments("/api"))
            {
                context.Response.StatusCode = 401;
            }
            else
            {
                // For non-API requests, we'll still allow the redirect
                context.Response.Redirect(context.RedirectUri);
            }
            return Task.CompletedTask;
        }
    };
});

// Set up a fallback authorization policy that allows anonymous access
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAssertion(_ => true) // This always evaluates to true
        .Build();
        
    // Add this policy to protect admin areas
    options.AddPolicy("RequireAdminRole", policy => 
        policy.RequireRole("Admin"));
});

// Register SeedService
builder.Services.AddScoped<SeedService>();
builder.Services.AddScoped<INotificationService, NotificationService>();

var app = builder.Build();

// Get the service and call the instance method
using (var scope = app.Services.CreateScope())
{
    var seedService = scope.ServiceProvider.GetRequiredService<SeedService>();
    await seedService.SeedDatabase(scope.ServiceProvider);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();