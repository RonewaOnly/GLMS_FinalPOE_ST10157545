using GLMS.Shared.Models;
using Microsoft.EntityFrameworkCore;
namespace GLMS.API.Data
{

        public class ApplicationDbAPIContext : DbContext
        {
        public ApplicationDbAPIContext(DbContextOptions<ApplicationDbAPIContext> options)
            : base(options)
        {
        }

        public DbSet<Client> Clients => Set<Client>();
            public DbSet<Contract> Contracts => Set<Contract>();
            public DbSet<ServiceRequest> ServiceRequests => Set<ServiceRequest>();
            public DbSet<AppUser> Users => Set<AppUser>();

            protected override void OnModelCreating(ModelBuilder mb)
            {
                base.OnModelCreating(mb);
                mb.Entity<Client>(e => { e.HasKey(c => c.Id); e.Property(c => c.Name).IsRequired().HasMaxLength(150); e.Property(c => c.ContractDetails).HasMaxLength(500); e.Property(c => c.Region).HasMaxLength(100); });
                mb.Entity<Contract>(e => {
                    e.HasKey(c => c.Id);
                    e.Property(c => c.ServiceLevel).HasMaxLength(200);
                    e.Property(c => c.SignedAgreementPath).HasMaxLength(500);
                    e.Property(c => c.SignedAgreementFileName).HasMaxLength(260);
                    e.Property(c => c.Status).HasConversion<string>();
                    e.HasOne(c => c.Client).WithMany(cl => cl.Contracts).HasForeignKey(c => c.ClientId).OnDelete(DeleteBehavior.Restrict);
                });
                mb.Entity<ServiceRequest>(e => {
                    e.HasKey(sr => sr.Id);
                    e.Property(sr => sr.Description).HasMaxLength(1000);
                    e.Property(sr => sr.CostUsd).HasColumnType("decimal(18,2)");
                    e.Property(sr => sr.CostZar).HasColumnType("decimal(18,2)");
                    e.Property(sr => sr.ExchangeRateUsed).HasColumnType("decimal(18,4)");
                    e.Property(sr => sr.Status).HasConversion<string>();
                    e.HasOne(sr => sr.Contract).WithMany(c => c.ServiceRequests).HasForeignKey(sr => sr.ContractId).OnDelete(DeleteBehavior.Restrict);
                });
                mb.Entity<AppUser>(e => { e.HasKey(u => u.Id); e.Property(u => u.Username).HasMaxLength(100); });
                mb.Entity<AppUser>().HasData(new AppUser { Id = 1, Username = "admin", PasswordHash = "Admin@1234", Role = "Admin" });
                mb.Entity<Client>().HasData(
                    new Client { Id = 1, Name = "Acme Freight Ltd", ContractDetails = "Air & sea freight", Region = "EMEA", CreatedOn = new DateTime(2024, 1, 1) },
                    new Client { Id = 2, Name = "FastTrack Logistics", ContractDetails = "Road haulage", Region = "SADC", CreatedOn = new DateTime(2024, 1, 1) },
                    new Client { Id = 3, Name = "Global Ship Co", ContractDetails = "Ocean freight", Region = "APAC", CreatedOn = new DateTime(2024, 1, 1) }
                );
                mb.Entity<Contract>().HasData(
                    new Contract { Id = 1, ClientId = 1, StartDate = new DateTime(2024, 1, 1), EndDate = new DateTime(2025, 1, 1), Status = ContractStatus.Active, ServiceLevel = "Priority 1 — 4-hour response", CreatedOn = new DateTime(2024, 1, 1) },
                    new Contract { Id = 2, ClientId = 2, StartDate = new DateTime(2023, 6, 1), EndDate = new DateTime(2024, 6, 1), Status = ContractStatus.Expired, ServiceLevel = "Standard — 24-hour response", CreatedOn = new DateTime(2023, 6, 1) }
                );
            }
        }
    

}
