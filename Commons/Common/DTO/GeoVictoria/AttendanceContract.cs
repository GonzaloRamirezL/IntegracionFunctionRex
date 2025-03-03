using Common.DTO.Rex;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.DTO.GeoVictoria
{
    public class AttendanceContract
    {
        public List<CalculatedUser> Users { get; set; }
        public List<CompanyExtraTimeValues> ExtraTimeValues { get; set; }
    }
    public class CalculatedUser 
    {
        public List<TimeIntervalContract> PlannedInterval { get; set; }
        public string Identifier { get; set; }
        public string Name { get; set; }
        public string LastName { get; set; }
        public string Enabled { get; set; }
        public string GroupDescription { get; set; }
        public string PositionDescription { get; set; }
        public string Email { get; set; }
        public string CustomColumn1 { get; set; }
        public string CustomColumn2 { get; set; }

        /// <summary>
        /// Campo donde se mantener los contratos del usuario
        /// </summary>
        public List<Contrato> RexContracts { get; set; }
    }
    public class CompanyExtraTimeValues
    {
        public string ValueId { get; set; }
        public string Value { get; set; }
        public string IsActive { get; set; }
    }
}
