using System;

namespace Common.ViewModels
{
    public class UserVM
    {
        public string Identifier { get; set; }
        public string UserCompanyIdentifier { get; set; }
        public string Name { get; set; }
        public string LastName { get; set; }
        public short Enabled { get; set; }
        public DateTime? StartContractDate { get; set; }
        public DateTime? EndContractDate { get; set; }
        public string Causal { get; set; }
        public string PositionIdentifier { get; set; }
        public string PositionName { get; set; }
        public string Email { get; set; }
        public string Adress { get; set; }
        public string GroupIdentifier { get; set; }
        public string GroupDescription { get; set; }
        public string Phone { get; set; }
        public string Custom1 { get; set; }
        public string Custom2 { get; set; }
        public string Custom3 { get; set; }
        public string UserProfile { get; set; }
        public string OldGroupDescription { get; set; }
        public string ContractGroupCode { get; set; }
        public string DisableCause { get; set; }
        public string UpdateCause { get; set; }

    }
}
