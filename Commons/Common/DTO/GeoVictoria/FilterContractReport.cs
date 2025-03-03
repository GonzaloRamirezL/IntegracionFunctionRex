namespace Common.DTO.GeoVictoria
{
    public class FilterContractReport
    {
        public string Range { get; set; }
        public string from { get; set; }
        public string to { get; set; }
        public int includeAll { get; set; }
        public string format { get; set; }
        public string identifierReporte { get; set; }
    }
}
