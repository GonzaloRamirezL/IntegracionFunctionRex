using Common.Enum;
using System;

namespace Common.ViewModels
{
    public class AttendanceProcessVM
    {
        public string AttendanceId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string ProcessName { get; set; }
        public string RexCompanyCode { get; set; }
    }
}
