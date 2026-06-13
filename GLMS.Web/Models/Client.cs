using System.ComponentModel.DataAnnotations;

namespace GLMS.Web.Models
{
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
}
