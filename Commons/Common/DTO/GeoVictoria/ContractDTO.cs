using System;

namespace Common.DTO.GeoVictoria
{
    public class ContractDTO
    {
        public long ContractId { get; set; }
        public long CompanyId { get; set; }
        public long ClientId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string ExternalContractId { get; set; }
        public string ExternalClientId { get; set; }
        public byte Status { get; set; }
        public string UserIdentifier { get; set; }
    }
}
