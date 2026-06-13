using GLMS.Web.Data;
using GLMS.Web.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace GLMS.TEST
{
    public class ContractFilterTests : IDisposable
    {
        private readonly ApplicationDbContext _db;
        public ContractFilterTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            _db = new ApplicationDbContext(options);
            // Seed test data (no seed data from OnModelCreating in InMemory)
            _db.Clients.AddRange(
                new Client { Id = 10, Name = "TestCo", ContractDetails = "Test", Region = "ZA", CreatedOn = DateTime.UtcNow },
                new Client { Id = 11, Name = "TestCo2", ContractDetails = "Test", Region = "ZA", CreatedOn = DateTime.UtcNow }
            );
            _db.Contracts.AddRange(
                new Contract { Id = 10, ClientId = 10, StartDate = new DateTime(2024, 1, 1), EndDate = new DateTime(2025, 1, 1), Status = ContractStatus.Active, ServiceLevel = "P1" },
                new Contract { Id = 11, ClientId = 10, StartDate = new DateTime(2023, 6, 1), EndDate = new DateTime(2024, 6, 1), Status = ContractStatus.Expired, ServiceLevel = "P2" },
                new Contract { Id = 12, ClientId = 11, StartDate = new DateTime(2024, 3, 15), EndDate = new DateTime(2025, 3, 15), Status = ContractStatus.Draft, ServiceLevel = "P3" },
                new Contract { Id = 13, ClientId = 11, StartDate = new DateTime(2024, 7, 1), EndDate = new DateTime(2025, 7, 1), Status = ContractStatus.OnHold, ServiceLevel = "P4" }
            );
            _db.SaveChanges();
        }
        [Fact]
        public void FilterByStatus_Active_ReturnsOnlyActiveContracts()
        {
            var results = _db.Contracts
                .Where(c => c.Status == ContractStatus.Active)
                .ToList();
            Assert.Single(results);
            Assert.Equal(10, results[0].Id);
        }
        [Fact]
        public void FilterByStatus_Expired_ReturnsOnlyExpiredContracts()
        {
            var results = _db.Contracts
                .Where(c => c.Status == ContractStatus.Expired)
                .ToList();
            Assert.Single(results);
            Assert.Equal(11, results[0].Id);
        }
        [Fact]
        public void FilterByDateRange_2024_ReturnsCorrectContracts()
        {
            var from = new DateTime(2024, 1, 1);
            var to = new DateTime(2024, 12, 31);
            var results = _db.Contracts
                .Where(c => c.StartDate >= from && c.StartDate <= to)
                .ToList();
            // Contracts 10 (Jan 2024), 12 (Mar 2024), 13 (Jul 2024) qualify
            Assert.Equal(3, results.Count);
        }
        [Fact]
        public void FilterByDateRange_BeforeAll_ReturnsEmpty()
        {
            var results = _db.Contracts
                .Where(c => c.StartDate >= new DateTime(2020, 1, 1)
                         && c.StartDate <= new DateTime(2020, 12, 31))
                .ToList();
            Assert.Empty(results);
        }
        [Fact]
        public void FilterByStatusAndDate_ActiveIn2024_ReturnsSingleResult()
        {
            var results = _db.Contracts
                .Where(c => c.Status == ContractStatus.Active
                         && c.StartDate >= new DateTime(2024, 1, 1)
                         && c.StartDate <= new DateTime(2024, 12, 31))
                .ToList();
            Assert.Single(results);
            Assert.Equal(ContractStatus.Active, results[0].Status);
        }
        [Fact]
        public void GetAllContracts_ReturnsFourRecords()
        {
            Assert.Equal(4, _db.Contracts.Count());
        }
        [Fact]
        public void FilterActiveOrDraft_ForServiceRequestDropdown_ReturnsTwoContracts()
        {
            // Mirrors the query used in ServiceRequestsController dropdown
            var results = _db.Contracts
                .Where(c => c.Status == ContractStatus.Active
                         || c.Status == ContractStatus.Draft)
                .ToList();
            Assert.Equal(2, results.Count);
        }
        public void Dispose() => _db.Dispose();
    }
}
