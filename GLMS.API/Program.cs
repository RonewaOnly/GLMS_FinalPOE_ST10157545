using System.Text;
using GLMS.API.Data;
using GLMS.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ════════════════════════════════════════════════════════════════════════════
//  CONTROLLERS + JSON
// ════════════════════════════════════════════════════════════════════════════
builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.PropertyNamingPolicy = null); // PascalCase JSON

// ════════════════════════════════════════════════════════════════════════════
//  SWAGGER / OPENAPI
//  Requires the "Swashbuckle.AspNetCore" NuGet package — if you see errors like
//  "Microsoft.OpenApi.Models not found", run:
//    dotnet add package Swashbuckle.AspNetCore --version 6.5.0
// ════════════════════════════════════════════════════════════════════════════
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "GLMS API",
        Version = "v1",
        Description = "Global Logistics Management System — Web API (Part 3)"
    });

    // JWT bearer auth support in Swagger UI
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token (no 'Bearer ' prefix needed)."
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id   = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    // Include XML doc comments for richer Swagger descriptions (optional)
    var xmlPath = Path.Combine(AppContext.BaseDirectory, "GLMS.Api.xml");
    if (File.Exists(xmlPath))
        c.IncludeXmlComments(xmlPath);
});

//  DATABASE — SQL Server via EF Core
builder.Services.AddDbContext<ApplicationDbAPIContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sql => sql.EnableRetryOnFailure(3)));

//  JWT AUTHENTICATION
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("Jwt:Key is not configured in appsettings.json.");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opts =>
    {
        opts.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();

//  APPLICATION SERVICES
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IFileService, FileService>();

builder.Services.AddHttpClient<ICurrencyService, CurrencyService>(c =>
{
    c.Timeout = TimeSpan.FromSeconds(10);
    c.DefaultRequestHeaders.Add("Accept", "application/json");
});


//  CORS — allow the MVC frontend container/dev server to call this API
builder.Services.AddCors(o => o.AddPolicy("MvcFrontend", policy =>
{
    policy
        .WithOrigins(
            "http://localhost:5000",
            "https://localhost:5001",
            "http://glms-frontend-web",
            "https://glms-frontend-web")
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials();
}));

//  FILE UPLOAD SIZE LIMIT (10 MB — for signed agreement PDFs)
builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(o =>
    o.MultipartBodyLengthLimit = 10 * 1024 * 1024);

//  BUILD APP
var app = builder.Build();

//  Middleware pipeline
if (app.Environment.IsDevelopment() || app.Environment.EnvironmentName == "Docker")
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "GLMS API v1");
        c.RoutePrefix = string.Empty; // Swagger UI served at root "/"
    });
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCors("MvcFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// 
//  AUTO-APPLY EF CORE MIGRATIONS ON STARTUP
//  Retries because in Docker the SQL Server container may not be ready yet.
// 
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbAPIContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    const int maxRetries = 10;
    for (var attempt = 1; attempt <= maxRetries; attempt++)
    {
        try
        {
            db.Database.Migrate();
            logger.LogInformation("Database migration applied successfully.");
            break;
        }
        catch (Exception ex) when (attempt < maxRetries)
        {
            logger.LogWarning(ex,
                "Migration attempt {Attempt}/{Max} failed. Retrying in 3s...",
                attempt, maxRetries);
            Thread.Sleep(3000);
        }
    }
}

app.Run();

// Required so WebApplicationFactory<Program> works in integration tests
public partial class Program { }