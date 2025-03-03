using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.DTO.GeoVictoria;
using IDAO.GeoVictoria;
using Helper;
using System.Net.Http;
using System.Configuration;
using Newtonsoft.Json;
using RestSharp;
using Helpers;

namespace DAO.GeoVictoria
{
    public class ConsolidatedGeoVictoriaDAO : IConsolidatedGeoVictoriaDAO
    {
        private const int DEFAULT_DEGREE_OF_PARALLELLISM = 10;
        public const int TIMEOUT = 30000;
        public DateTime startDateRequest = DateTimeHelper.FirstDayWeekBefore(DateTime.Today.AddDays(-7));
        public DateTime endDateRequest = DateTimeHelper.FirstDayWeekBefore(DateTime.Today.AddDays(-7)).AddDays(6);

        private readonly int MAX_PROCESS;
        public ConsolidatedGeoVictoriaDAO()
        {
            bool success = int.TryParse(ConfigurationHelper.Value("maxParallel"), out int maxProcess);
            MAX_PROCESS = success ? maxProcess : DEFAULT_DEGREE_OF_PARALLELLISM;
        }
        public List<ConsolidatedContract> GetConsolidatedGV(string key, string secret, List<string> users)
        {
            List<ConsolidatedContract> result = new List<ConsolidatedContract>();            
            try
            {
                var request = new RestRequest("/v1/Consolidated", RestSharp.Method.POST);
                request.RequestFormat = DataFormat.Json;
                FilterCustomerContract filter = new FilterCustomerContract
                {                    
                    StartDate = DateTimeHelper.DateTimeToStringGeoVictoria(startDateRequest),
                    EndDate = DateTimeHelper.DateTimeToStringGeoVictoria(endDateRequest),
                    UserIds = string.Join(",", users)
                };
                request.AddHeader("Authorization", ConfigurationHelper.Value("consumerToken"));
                request.AddJsonBody(filter);
                var response = ConnectionHelper.GetResponse(request, ConfigurationHelper.Value("UrlCustomerService"), "Customer API", "Consolidated", key, secret);
                result = JsonConvert.DeserializeObject<List<ConsolidatedContract>> (response.Content);
            }
            catch (Exception e)
            {
                LogHelper.Log("ERROR: " + e.Message);
            }

            return result;
        }
    }
}
