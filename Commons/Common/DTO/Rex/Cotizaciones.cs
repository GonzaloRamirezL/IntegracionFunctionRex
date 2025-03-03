using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Common.DTO.Rex
{
    public class Cotizaciones

    {
        public string RexDomain { get; set; }
        public int legal_entity { get; set; }
        public string customer_reference { get; set; }
        public string project_contract_id { get; set; }
        public string project_id { get; set; }
        public DateTime? contract_date { get; set; }
        public string customer_name { get; set; }
        public string sales_currency { get; set; }
        public string invoice_name { get; set; }
        public string payment_terms { get; set; }
        public DateTime? actual_end_date { get; set; }
        public DateTime? actual_start_date { get; set; }
        public string customer_account { get; set; }
        public DateTime? extension_date { get; set; }
        public DateTime? project_end_date { get; set; }
        public DateTime? project_start_date { get; set; }
        public string project_group { get; set; }
        public string project_name { get; set; }
        public DateTime? start_date { get; set; }
        public string dimension_display_value { get; set; }
        public Guid guid { get; set; }
        public string organization_name { get; set; }
    }
}
