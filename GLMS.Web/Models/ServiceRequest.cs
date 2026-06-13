using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GLMS.Web.Models
{
    public enum ServiceRequestStatus
    {
        Pending,
        InProgress,
        Completed,
        Cancelled
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
}
