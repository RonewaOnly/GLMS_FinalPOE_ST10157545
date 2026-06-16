using GLMS.Shared.Models;
using GLMS.Shared.Models.DTOs;
using GLMS.Web.Models.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GLMS.Web.Mapping
{
    public static class ServiceRequestMapper
    {
        public static ServiceRequestFormViewModel ToFormModel(
            ServiceRequestDto dto,
            IEnumerable<SelectListItem> contractList)
        {
            return new ServiceRequestFormViewModel
            {
                Id = dto.Id,
                ContractId = dto.ContractId,
                Description = dto.Description,
                CostUsd = dto.CostUsd,
                CostZar = dto.CostZar,
                ExchangeRate = dto.ExchangeRateUsed,
                Status = Enum.Parse<ServiceRequestStatus>(dto.Status),
                ContractList = contractList,
                ContractInfo = dto.ClientName != null
                    ? $"{dto.ClientName} (Contract #{dto.ContractId})"
                    : null
            };
        }

        public static ServiceRequest ToModel(ServiceRequestDto dto)
        {
            return new ServiceRequest
            {
                Id = dto.Id,
                ContractId = dto.ContractId,
                Description = dto.Description,
                CostUsd = dto.CostUsd,
                CostZar = dto.CostZar,
                ExchangeRateUsed = dto.ExchangeRateUsed,
                Status = Enum.Parse<ServiceRequestStatus>(dto.Status),
                CreatedOn = dto.CreatedOn,
                Contract = new Contract
                {
                    Id = dto.ContractId,
                    Client = new Client { Name = dto.ClientName }
                }
            };
        }
    }
}
