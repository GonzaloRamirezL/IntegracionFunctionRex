using System;

namespace Common.ViewModels
{
    public class ContractClientVM
    {
        public string UserIdentifier { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string ExternalContractId { get; set; }
        public string ExternalClientId { get; set; }
        public string ClientName { get; set; }
        public byte Status { get; set; }

    }
}
