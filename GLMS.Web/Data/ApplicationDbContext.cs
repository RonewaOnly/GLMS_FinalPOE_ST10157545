using GLMS.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace GLMS.Web.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }
        public DbSet<Client> Clients => Set<Client>();
        public DbSet<Contract> Contracts => Set<Contract>();
        public DbSet<ServiceRequest> ServiceRequests => Set<ServiceRequest>();
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Client
            modelBuilder.Entity<Client>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Name).IsRequired().HasMaxLength(150);
                entity.Property(c => c.ContractDetails).HasMaxLength(500);
                entity.Property(c => c.Region).HasMaxLength(100);
            });
            //Contract
            modelBuilder.Entity<Contract>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.Property(c => c.ServiceLevel).HasMaxLength(200);
                entity.Property(c => c.SignedAgreementPath).HasMaxLength(500);
                entity.Property(c => c.SignedAgreementFileName).HasMaxLength(260);
                entity.Property(c => c.Status)
                      .HasConversion<string>(); // store enum as string for readability
                                                // One Client → many Contracts
                entity.HasOne(c => c.Client)
                      .WithMany(cl => cl.Contracts)
                      .HasForeignKey(c => c.ClientId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
            // ServiceRequest 
            modelBuilder.Entity<ServiceRequest>(entity =>
            {
                entity.HasKey(sr => sr.Id);
                entity.Property(sr => sr.Description).HasMaxLength(1000);
                entity.Property(sr => sr.CostUsd).HasColumnType("decimal(18,2)");
                entity.Property(sr => sr.CostZar).HasColumnType("decimal(18,2)");
                entity.Property(sr => sr.ExchangeRateUsed).HasColumnType("decimal(18,4)");
                entity.Property(sr => sr.Status)
                      .HasConversion<string>();
                // One Contract → many ServiceRequests
                entity.HasOne(sr => sr.Contract)
                      .WithMany(c => c.ServiceRequests)
                      .HasForeignKey(sr => sr.ContractId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
            // Seed data for testing/demo purposes
            modelBuilder.Entity<Client>().HasData(
     new Client { Id = 1, Name = "Acme Freight Ltd", ContractDetails = "Air & sea freight", Region = "EMEA", CreatedOn = new DateTime(2024, 1, 1) },
     new Client { Id = 2, Name = "FastTrack Logistics", ContractDetails = "Road haulage", Region = "SADC", CreatedOn = new DateTime(2024, 1, 1) },
     new Client { Id = 3, Name = "Global Ship Co", ContractDetails = "Ocean freight", Region = "APAC", CreatedOn = new DateTime(2024, 1, 1) }
 );
            modelBuilder.Entity<Contract>().HasData(
                new Contract
                {
                    Id = 1,
                    ClientId = 1,
                    StartDate = new DateTime(2024, 1, 1),
                    EndDate = new DateTime(2025, 1, 1),
                    Status = ContractStatus.Active,
                    ServiceLevel = "Priority 1 — 4-hour response",
                    CreatedOn = new DateTime(2024, 1, 1)
                },
                new Contract
                {
                    Id = 2,
                    ClientId = 2,
                    StartDate = new DateTime(2023, 6, 1),
                    EndDate = new DateTime(2024, 6, 1),
                    Status = ContractStatus.Expired,
                    ServiceLevel = "Standard — 24-hour response",
                    CreatedOn = new DateTime(2023, 6, 1)
                }
            );
        }

    }
}
