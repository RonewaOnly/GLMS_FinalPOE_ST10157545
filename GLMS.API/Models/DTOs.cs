using System.ComponentModel.DataAnnotations;
namespace GLMS.API.Models.DTOs
{

        public class LoginRequest { [Required] public string Username { get; set; } = ""; [Required] public string Password { get; set; } = ""; }
        public class LoginResponse { public string Token { get; set; } = ""; public string Username { get; set; } = ""; public string Role { get; set; } = ""; public DateTime Expires { get; set; } }
        public class ClientDto { public int Id { get; set; } public string Name { get; set; } = ""; public string ContractDetails { get; set; } = ""; public string Region { get; set; } = ""; public DateTime CreatedOn { get; set; } public int ContractCount { get; set; } }
        public class CreateClientRequest { [Required, StringLength(150)] public string Name { get; set; } = ""; [Required, StringLength(500)] public string ContractDetails { get; set; } = ""; [Required, StringLength(100)] public string Region { get; set; } = ""; }
        public class ContractDto { public int Id { get; set; } public int ClientId { get; set; } public string ClientName { get; set; } = ""; public DateTime StartDate { get; set; } public DateTime EndDate { get; set; } public string Status { get; set; } = ""; public string ServiceLevel { get; set; } = ""; public string? SignedAgreementFileName { get; set; } public bool HasAgreement { get; set; } public bool CanCreateServiceRequest { get; set; } public DateTime CreatedOn { get; set; } public List<ServiceRequestDto> ServiceRequests { get; set; } = new(); }
        public class CreateContractRequest { [Required] public int ClientId { get; set; } [Required] public DateTime StartDate { get; set; } [Required] public DateTime EndDate { get; set; } public ContractStatus Status { get; set; } = ContractStatus.Draft; [Required, StringLength(200)] public string ServiceLevel { get; set; } = ""; public string? AgreementPath { get; set; } public string? AgreementFileName { get; set; } }
        public class UpdateContractStatusRequest { [Required] public string Status { get; set; } = ""; }
        public class ServiceRequestDto { public int Id { get; set; } public int ContractId { get; set; } public string ClientName { get; set; } = ""; public string Description { get; set; } = ""; public decimal CostUsd { get; set; } public decimal CostZar { get; set; } public decimal ExchangeRateUsed { get; set; } public string Status { get; set; } = ""; public DateTime CreatedOn { get; set; } }
        public class CreateServiceRequestRequest { [Required] public int ContractId { get; set; } [Required, StringLength(1000)] public string Description { get; set; } = ""; [Range(0.01, double.MaxValue)] public decimal CostUsd { get; set; } public decimal ExchangeRate { get; set; } }
        public class CurrencyRateResponse { public decimal Rate { get; set; } public string Base { get; set; } = "USD"; public string Target { get; set; } = "ZAR"; public DateTime FetchedAt { get; set; } }
        public class DashboardDto { public int TotalClients { get; set; } public int TotalContracts { get; set; } public int ActiveContracts { get; set; } public int ExpiredContracts { get; set; } public int TotalServiceRequests { get; set; } public int PendingRequests { get; set; } }

}
