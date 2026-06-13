
using GLMS.API.Data;
using GLMS.API.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;


var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers().AddJsonOptions(o => o.JsonSerializerOptions.PropertyNamingPolicy = null);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "GLMS API", Version = "v1", Description = "Global Logistics Management System — Web API (Part 3)" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme { Name = "Authorization", Type = SecuritySchemeType.Http, Scheme = "bearer", BearerFormat = "JWT", In = ParameterLocation.Header, Description = "Enter your JWT token" });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement { { new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } }, Array.Empty<string>() } });
    var xmlPath = Path.Combine(AppContext.BaseDirectory, "GLMS.Api.xml");
    if (File.Exists(xmlPath)) c.IncludeXmlComments(xmlPath);
});


builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"), sql => sql.EnableRetryOnFailure(3)));


var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key not configured.");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opts => opts.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    });
builder.Services.AddAuthorization();
builder.Services.AddScoped<IJwtService, JWTService>();
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddHttpClient<ICurrencyService, CurrencyService>(c => { c.Timeout = TimeSpan.FromSeconds(10); c.DefaultRequestHeaders.Add("Accept", "application/json"); });
builder.Services.AddCors(o => o.AddPolicy("MvcFrontend", p => p.WithOrigins("http://localhost:5000", "http://glms-frontend-web", "https://glms-frontend-web").AllowAnyHeader().AllowAnyMethod().AllowCredentials()));
builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(o => o.MultipartBodyLengthLimit = 10 * 1024 * 1024);


var app = builder.Build();
if (app.Environment.IsDevelopment() || app.Environment.EnvironmentName == "Docker") { app.UseSwagger(); app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "GLMS API v1"); c.RoutePrefix = string.Empty; }); }
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCors("MvcFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();


using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var retries = 0;
    while (retries < 10) { try { db.Database.Migrate(); break; } catch { retries++; Thread.Sleep(3000); } }
}


app.Run();
public partial class Program { }

