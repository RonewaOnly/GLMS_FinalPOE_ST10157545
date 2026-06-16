using GLMS.API.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GLMS.TEST
{
    public class GlmsApiFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<DbContextOptions<ApplicationDbAPIContext>>();
                services.RemoveAll<ApplicationDbAPIContext>();
                services.AddDbContext<ApplicationDbAPIContext>(opts =>
                    opts.UseInMemoryDatabase("GlmsTestDb_" + Guid.NewGuid()));
            });
            builder.UseEnvironment("Development");
        }
    }
}
