using GLMS.Shared.Models;
using GLMS.Shared.Models.DTOs;

namespace GLMS.Web.Mapping
{
    public static class ClientMapper
    {
        public static Client ToModel(ClientDto dto)
        {
            return new Client
            {
                Id = dto.Id,
                Name = dto.Name,
                Region = dto.Region,
                ContractDetails = dto.ContractDetails,
                CreatedOn = dto.CreatedOn,
                // Contracts is NOT mapped because ClientDto does not contain it
                Contracts = dto.Contracts.Select(ct => new Contract
                {
                    Id = ct.Id,
                    ClientId = ct.ClientId,
                    StartDate = ct.StartDate,
                    EndDate = ct.EndDate,
                    Status = Enum.Parse<ContractStatus>(ct.Status),
                    ServiceLevel = ct.ServiceLevel,
                    CreatedOn = ct.CreatedOn
                }).ToList()

            };
        }
    }
}
