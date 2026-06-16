using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GLMS.Shared.Models
{
    public enum ContractStatus { Draft, Active, Expired, OnHold }
    public enum ServiceRequestStatus { Pending, InProgress, Completed, Cancelled }
    public class Client
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Client name is required.")]
        [StringLength(150, ErrorMessage = "Name cannot exceed 150 characters.")]
        [Display(Name = "Client Name")]
        public string Name { get; set; } = string.Empty;
        [Required(ErrorMessage = "Contract details are required.")]
        [StringLength(500)]
        [Display(Name = "Contract Details")]
        public string ContractDetails { get; set; } = string.Empty;
        [Required(ErrorMessage = "Region is required.")]
        [StringLength(100)]
        public string Region { get; set; } = string.Empty;
        [Display(Name = "Created On")]
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
        // Navigation property
        public ICollection<Contract> Contracts { get; set; } = new List<Contract>();
    }
    public class Contract
    {

        public int Id { get; set; }
        // Foreign key to Client
        [Required]
        [Display(Name = "Client")]
        public int ClientId { get; set; }
        [Required(ErrorMessage = "Start date is required.")]
        [DataType(DataType.Date)]
        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; }
        [Required(ErrorMessage = "End date is required.")]
        [DataType(DataType.Date)]
        [Display(Name = "End Date")]
        public DateTime EndDate { get; set; }
        [Required]
        [Display(Name = "Status")]
        public ContractStatus Status { get; set; } = ContractStatus.Draft;
        [Required(ErrorMessage = "Service level is required.")]
        [StringLength(200)]
        [Display(Name = "Service Level / SLA")]
        public string ServiceLevel { get; set; } = string.Empty;
        // Signed Agreement PDF — stored as a relative server path
        [Display(Name = "Signed Agreement (PDF)")]
        public string? SignedAgreementPath { get; set; }
        [Display(Name = "Original File Name")]
        public string? SignedAgreementFileName { get; set; }
        [Display(Name = "Created On")]
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
        // Navigation properties
        public Client? Client { get; set; }
        public ICollection<ServiceRequest> ServiceRequests { get; set; } = new List<ServiceRequest>();
        // Computed helper
        [NotMapped]
        public bool IsActive => Status == ContractStatus.Active;
        [NotMapped]
        public bool CanCreateServiceRequest =>
            Status != ContractStatus.Expired && Status != ContractStatus.OnHold;
    }
 
    public class ServiceRequest
    {
        public int Id { get; set; }
        // Foreign key to Contract
        [Required]
        [Display(Name = "Contract")]
        public int ContractId { get; set; }
        [Required(ErrorMessage = "Description is required.")]
        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;
        // Cost in USD as entered by the user
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Cost must be greater than 0.")]
        [Display(Name = "Cost (USD)")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal CostUsd { get; set; }
        // Cost converted and saved in ZAR
        [Display(Name = "Cost (ZAR)")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal CostZar { get; set; }
        // The exchange rate used at time of creation (audit trail)
        [Display(Name = "USD/ZAR Rate Used")]
        [Column(TypeName = "decimal(18,4)")]
        public decimal ExchangeRateUsed { get; set; }
        [Required]
        public ServiceRequestStatus Status { get; set; } = ServiceRequestStatus.Pending;
        [Display(Name = "Created On")]
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
        // Navigation property
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
