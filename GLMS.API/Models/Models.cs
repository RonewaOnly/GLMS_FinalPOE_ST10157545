using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GLMS.API.Models
{
    public enum ContractStatus { Draft, Active, Expired, OnHold }
    public enum ServiceRequestStatus { Pending, InProgress, Completed, Cancelled }
    public class Client
    {
        public int Id { get; set; }
        [Required, StringLength(150)] public string Name { get; set; } = string.Empty;
        [Required, StringLength(500)] public string ContractDetails { get; set; } = string.Empty;
        [Required, StringLength(100)] public string Region { get; set; } = string.Empty;
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
        public ICollection<Contract> Contracts { get; set; } = new List<Contract>();
    }
    public class Contract
    {
        public int Id { get; set; }
        [Required] public int ClientId { get; set; }
        [Required] public DateTime StartDate { get; set; }
        [Required] public DateTime EndDate { get; set; }
        public ContractStatus Status { get; set; } = ContractStatus.Draft;
        [Required, StringLength(200)] public string ServiceLevel { get; set; } = string.Empty;
        public string? SignedAgreementPath { get; set; }
        public string? SignedAgreementFileName { get; set; }
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
        public Client? Client { get; set; }
        public ICollection<ServiceRequest> ServiceRequests { get; set; } = new List<ServiceRequest>();
        [NotMapped]
        public bool CanCreateServiceRequest =>
            Status != ContractStatus.Expired && Status != ContractStatus.OnHold;
    }
    public class ServiceRequest
    {
        public int Id { get; set; }
        [Required] public int ContractId { get; set; }
        [Required, StringLength(1000)] public string Description { get; set; } = string.Empty;
        [Column(TypeName = "decimal(18,2)")] public decimal CostUsd { get; set; }
        [Column(TypeName = "decimal(18,2)")] public decimal CostZar { get; set; }
        [Column(TypeName = "decimal(18,4)")] public decimal ExchangeRateUsed { get; set; }
        public ServiceRequestStatus Status { get; set; } = ServiceRequestStatus.Pending;
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
        public Contract? Contract { get; set; }
    }
    public class AppUser
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Role { get; set; } = "Admin";
    }
}
