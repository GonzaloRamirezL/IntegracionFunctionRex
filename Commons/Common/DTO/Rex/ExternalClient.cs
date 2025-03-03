using System;
using System.Collections.Generic;
using System.Text;

namespace Common.DTO.Rex
{
    public class ExternalClient
    {
        public string CustomerReference { get; set; }
        public string ProjectContractId { get; set; }
        public string ProjectId { get; set; }
        public DateTime ContractDate { get; set; }
        public string CustomerName { get; set; }
        public string SalesCurrency { get; set; }
        public string InvoiceName { get; set; }
        public string PaymentTerms { get; set; }
        public DateTime ActualEndDate { get; set; }
        public DateTime ActualStartDate { get; set; }
        public string CustomerAccount { get; set; }
        public DateTime? ExtensionDate { get; set; }
        public DateTime ProjectEndDate { get; set; }
        public DateTime ProjectStartDate { get; set; }
        public string ProjectGroup { get; set; }
        public string ProjectName { get; set; }
        public DateTime StartDate { get; set; }
        public string DimensionDisplayValue { get; set; }
        public Guid Guid { get; set; }
        public string OrganizationName { get; set; }
    }
}
