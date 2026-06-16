using GLMS.Web.Controllers;
using GLMS.Web.Services;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.ApplicationParts;

var builder = WebApplication.CreateBuilder(args);

#region MVC
//builder.Services.AddControllersWithViews();
//builder.Services
//    .AddControllersWithViews()
//    .AddApplicationPart(typeof(HomeController).Assembly)
//    .AddControllersAsServices();
builder.Services.AddControllersWithViews()
    .AddRazorRuntimeCompilation()
    .ConfigureApplicationPartManager(apm =>
    {
        apm.ApplicationParts.Clear();
        apm.ApplicationParts.Add(new AssemblyPart(typeof(HomeController).Assembly));
    });




#endregion

#region Session Configuration
builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(8);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
#endregion

builder.Services.AddHttpContextAccessor();

#region Application Services
builder.Services.AddScoped<IFileService, FileService>();
#endregion

#region API Http Client
builder.Services.AddHttpClient<ApiClient>(client =>
{
    var apiUrl = builder.Configuration["ApiBaseUrl"]
        ?? "https://localhost:5291"; // Default to local API if not configured

    client.BaseAddress = new Uri(apiUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});
#endregion

#region Currency Service Client
builder.Services.AddHttpClient<ICurrencyService, CurrencyService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(10);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});
#endregion

#region File Upload Limits
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 10 * 1024 * 1024; // 10MB
});
#endregion

var app = builder.Build();

#region HTTP Pipeline
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
#endregion

#region Routing
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
#endregion

app.Run();