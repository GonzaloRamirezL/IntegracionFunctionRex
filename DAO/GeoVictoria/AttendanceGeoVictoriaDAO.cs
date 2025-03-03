using System;
using System.Collections.Generic;
using System.Linq;
using Common.DTO.GeoVictoria;
using IDAO.GeoVictoria;
using Helper;
using System.Configuration;
using Newtonsoft.Json;
using RestSharp;
using Helpers;
using Common.ViewModels;

namespace DAO.GeoVictoria
{
    public class AttendanceGeoVictoriaDAO : IAttendanceGeoVictoriaDAO
    {
        public AttendanceContract GetAttendanceBook(GeoVictoriaConnectionVM gvConnection, FilterCustomerContract filter)
        {
            AttendanceContract result = null;
            try
            {
                var request = new RestRequest("/v1/AttendanceBook", RestSharp.Method.POST);
                request.RequestFormat = DataFormat.Json;
                request.AddHeader("Authorization", gvConnection.ApiToken);
                request.AddJsonBody(filter);
                string urlCustomerService = String.Empty;
                if (gvConnection.TestEnvironment)
                {
                    urlCustomerService = ConfigurationHelper.Value("UrlCustomerSandboxService");
                }
                else
                {
                    urlCustomerService = ConfigurationHelper.Value("UrlCustomerService");
                }
                var response = ConnectionHelper.GetResponse(request, urlCustomerService, "Customer API", "AttendanceBook", gvConnection.ApiKey, gvConnection.ApiSecret);
                if(response.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    LogHelper.Log("ERROR: " + response.Content);
                    return null;
                }
                result = JsonConvert.DeserializeObject<AttendanceContract>(response.Content);
                        
            }
            catch (Exception e)
            {
                LogHelper.Log("ERROR: " + e.Message);
            }

            return result;
        }
    }
}
