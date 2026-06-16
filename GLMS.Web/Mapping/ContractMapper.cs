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
                ServiceLevel = dto.ServiceLevel,
                Status = Enum.Parse<ContractStatus>(dto.Status),
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                CreatedOn = dto.CreatedOn,
                SignedAgreementFileName = dto.SignedAgreementFileName,
                SignedAgreementPath = dto.SignedAgreementFileName, // adjust if needed
                
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
