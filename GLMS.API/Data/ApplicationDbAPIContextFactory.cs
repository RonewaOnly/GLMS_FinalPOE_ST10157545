using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace GLMS.API.Data
{
    public class ApplicationDbAPIContextFactory : IDesignTimeDbContextFactory<ApplicationDbAPIContext>
    {
        public ApplicationDbAPIContext CreateDbContext(string[] args)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", optional: true)
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbAPIContext>();

            optionsBuilder.UseSqlServer(
                config.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly("GLMS.API")
                );

            return new ApplicationDbAPIContext(optionsBuilder.Options);
        }
    }
}
