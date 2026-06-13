using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace GLMS.Web.Models.ViewModels
{
    //Contract create/edit form 
    public class ContractFormViewModel
    {
        public int Id { get; set; }
        [Required]
        [Display(Name = "Client")]
        public int ClientId { get; set; }
        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; } = DateTime.Today;
        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "End Date")]
        public DateTime EndDate { get; set; } = DateTime.Today.AddYears(1);
        [Required]
        public ContractStatus Status { get; set; } = ContractStatus.Draft;
        [Required]
        [StringLength(200)]
        [Display(Name = "Service Level / SLA")]
        public string ServiceLevel { get; set; } = string.Empty;
        // File upload — only required on create
        [Display(Name = "Signed Agreement (PDF only)")]
        public IFormFile? SignedAgreement { get; set; }
        // Populated for edit so the UI can show the existing file name
        public string? ExistingFileName { get; set; }
        // Drop-down list for clients
        public IEnumerable<SelectListItem> ClientList { get; set; } = new List<SelectListItem>();
    }
    //Contract search/filter
    public class ContractFilterViewModel
    {
        [DataType(DataType.Date)]
        [Display(Name = "Start Date From")]
        public DateTime? StartDateFrom { get; set; }
        [DataType(DataType.Date)]
        [Display(Name = "Start Date To")]
        public DateTime? StartDateTo { get; set; }
        public ContractStatus? Status { get; set; }
        public IEnumerable<Contract> Results { get; set; } = new List<Contract>();
    }
    //Service Request create form
    public class ServiceRequestFormViewModel
    {
        public int Id { get; set; }
        [Required]
        [Display(Name = "Contract")]
        public int ContractId { get; set; }
        [Required]
        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Cost must be greater than 0.")]
        [Display(Name = "Amount (USD)")]
        public decimal CostUsd { get; set; }
        // Pre-calculated on the client and confirmed by the server
        [Display(Name = "Amount (ZAR) — auto-calculated")]
        public decimal CostZar { get; set; }
        [Display(Name = "USD/ZAR Rate")]
        public decimal ExchangeRate { get; set; }
        public ServiceRequestStatus Status { get; set; } = ServiceRequestStatus.Pending;
        // For the dropdown on the form
        public IEnumerable<SelectListItem> ContractList { get; set; } = new List<SelectListItem>();
        // Info about parent contract shown on form
        public string? ContractInfo { get; set; }
    }
    //Currency API response shape
    public class ExchangeRateResponse
    {
        public string? Base { get; set; }
        public Dictionary<string, decimal> Rates { get; set; } = new();
    }
}
