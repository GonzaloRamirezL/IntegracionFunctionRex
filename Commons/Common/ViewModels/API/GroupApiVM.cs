using System;
using System.Collections.Generic;

namespace Common.ViewModels.API
{
    public class GroupApiVM
    {
        public short? CallSource { get; set; }
        public byte? PunchFromCrewOnly { get; set; }
        public bool? CrewEnabled { get; set; }
        public bool? UseCasino { get; set; }
        public bool? BlockUnwantedApps { get; set; }
        public byte? BlockByLocation { get; set; }
        public byte? UseProyectsAndTasks { get; set; }
        public byte? ValidationType { get; set; }
        public bool? AppEnabled { get; set; }
        public List<UserApiVM> Supervisors { get; set; }
        public string CostCenter { get; set; }
        public int? GpsAccuracy { get; set; }
        public string GpsLongitude { get; set; }
        public string GpsLatitude { get; set; }
        public string Address { get; set; }
        public string Path { get; set; }
        public string Description { get; set; }
        public string Identifier { get; set; }
        public string Custom1 { get; set; }
        public byte? UseWebPunching { get; set; }
    }
}
