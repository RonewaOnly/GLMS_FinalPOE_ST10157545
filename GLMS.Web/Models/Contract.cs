using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GLMS.Web.Models
{
    public enum ContractStatus
    {

        Draft,
        Active,
        Expired,
        OnHold
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
}
