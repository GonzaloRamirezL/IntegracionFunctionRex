using System;

namespace Common.ViewModels
{
    public class TimeOffVM
    {
        public string TimeOffId { get; set; }

        public string TimeOffTypeId { get; set; }

        public string Identifier { get; set; }

        public string Custom1 { get; set; }

        public string Email { get; set; }

        public string TimeOffDescription { get; set; }

        public string CreatedBy { get; set; }

        public DateTime? EndDateTime { get; set; }

        public DateTime? StartDateTime { get; set; }

        public TimeSpan? StartTime { get; set; }

        public TimeSpan? EndTime { get; set; }

        public string Hours { get; set; }
    }
}
