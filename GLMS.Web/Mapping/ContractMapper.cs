using GLMS.Shared.Models;
using GLMS.Shared.Models.DTOs;
using GLMS.Web.Models.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GLMS.Web.Mapping
{
    public static class ContractMapper
    {
        public static Contract ToModel(ContractDto dto)
        {
            return new Contract
            {
                Id = dto.Id,
                ClientId = dto.ClientId,
                Client = new Client { Name = dto.ClientName },
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                Status = Enum.Parse<ContractStatus>(dto.Status),
                ServiceLevel = dto.ServiceLevel,
                SignedAgreementFileName = dto.SignedAgreementFileName,
                CreatedOn = dto.CreatedOn,
                ServiceRequests = dto.ServiceRequests.Select(sr => new ServiceRequest
                {
                    Id = sr.Id,
                    ContractId = sr.ContractId,
                    Description = sr.Description,
                    CostUsd = sr.CostUsd,
                    CostZar = sr.CostZar,
                    ExchangeRateUsed = sr.ExchangeRateUsed,
                    Status = Enum.Parse<ServiceRequestStatus>(sr.Status),
                    CreatedOn = sr.CreatedOn
                }).ToList()
            };
        }

        public static ContractFormViewModel ToFormModel(ContractDto dto, IEnumerable<SelectListItem> clients)
        {
            return new ContractFormViewModel
            {
                Id = dto.Id,
                ClientId = dto.ClientId,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                Status = Enum.Parse<ContractStatus>(dto.Status),
                ServiceLevel = dto.ServiceLevel,
                ExistingFileName = dto.SignedAgreementFileName,
                ClientList = clients
            };
        }


    }
}
