namespace Common.DTO.GeoVictoria
{
    public class UserContract
    {
        public string Name { get; set; }
        public string LastName { get; set; }
        public string Identifier { get; set; }
        public string Email { get; set; }
        public string Adress { get; set; }
        public string Phone { get; set; }
        public short? Enabled { get; set; }
        public string Custom1 { get; set; }
        public string Custom2 { get; set; }
        public string Custom3 { get; set; }
        public string GroupIdentifier { get; set; }
        public string GroupDescription { get; set; }
        public string ContractDate { get; set; }
        public string UserProfile { get; set; }
        public string userScheduler { get; set; }
        public string userCompanyIdentifier { get; set; }
        public string weeklyHoursCode { get; set; }
        public string endContractDate { get; set; }
        public string positionIdentifier { get; set; }
        public string positionName { get; set; }
        public string integrationCode { get; set; }
    }
}
