using GLMS.Web.Data;
using GLMS.Web.Services;

using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ========================================
// Add Services to the Container
// ========================================

// MVC
builder.Services.AddControllersWithViews();

// ========================================
// Database Configuration
// ========================================

builder.Services.AddDbContext<ApplicationDbContext>(options =>
options.UseSqlServer(
builder.Configuration.GetConnectionString("DefaultConnection"),
sqlOptions =>
{
    sqlOptions.EnableRetryOnFailure(3);
}));

// ========================================
// Session Configuration
// ========================================

builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(8);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddHttpContextAccessor();

// ========================================
// Application Services
// ========================================

builder.Services.AddScoped<IFileService, FileService>();

// ========================================
// HTTP Clients
// ========================================

// API Client
builder.Services.AddHttpClient<ApiClient>(client =>
{
    var apiUrl = builder.Configuration["ApiBaseUrl"]
    ?? "https://localhost:7291";

client.BaseAddress = new Uri(apiUrl);
    client.Timeout = TimeSpan.FromSeconds(30);

    client.DefaultRequestHeaders.Add("Accept", "application/json");

});

// Currency Service Client
builder.Services.AddHttpClient<ICurrencyService, CurrencyService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(10);

client.DefaultRequestHeaders.Add("Accept", "application/json");

});

// ========================================
// File Upload Configuration
// ========================================

builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
{
    // 10 MB upload limit
    options.MultipartBodyLengthLimit = 10 * 1024 * 1024;
});

// ========================================
// Build Application
// ========================================

var app = builder.Build();

// ========================================
// Configure HTTP Request Pipeline
// ========================================

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthorization();

// ========================================
// Route Configuration
// ========================================

app.MapControllerRoute(
name: "default",
pattern: "{controller=Home}/{action=Index}/{id?}");

// ========================================
// Auto Apply Migrations
// ========================================

using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider
        .GetRequiredService<ApplicationDbContext>();

    db.Database.Migrate();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Database migration error: {ex.Message}");
    }
}

// ========================================
// Run Application
// ========================================

app.Run();
