using Common.DTO.GeoVictoria;
using Common.ViewModels;
using Helper;
using Helpers;
using IDAO.GeoVictoria;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;

namespace DAO.GeoVictoria
{
    public class ContractGeoVictoriaDAO : IContractGeoVictoriaDAO
    {

        // Encontrar los contratos del usuario
        public List<ContractDTO> FindUserContracts(GeoVictoriaConnectionVM gvConnection, ContractFilterVM filter)
        {
            try
            {
                var request = new RestRequest("/v1/contract/Find", Method.POST);
                request.RequestFormat = DataFormat.Json;
                request.AddHeader("Authorization", gvConnection.ApiToken);
                request.AddJsonBody(new { userIdentifiers = filter.UserIdentifiers });

                string baseUrl;
                if (gvConnection.TestEnvironment)
                {
                    baseUrl = ConfigurationHelper.Value("UrlCustomerSandboxService");
                }
                else
                {
                    baseUrl = ConfigurationHelper.Value("UrlCustomerService");
                }

                var response = ConnectionHelper.GetResponse(request, baseUrl, "Customer API", "FindUserContracts", gvConnection.ApiKey, gvConnection.ApiSecret);

                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    LogHelper.Log("ERROR: " + response.Content);
                    throw new Exception("Error in response: " + response.Content);
                }

                LogHelper.Log(response.Content);
                return JsonConvert.DeserializeObject<List<ContractDTO>>(response.Content);
            }
            catch (Exception ex)
            {
                LogHelper.Log("ERROR: " + ex.Message);
                return null;
            }
        }

        //Agregar los contratos 
        public void ProcessContracts(GeoVictoriaConnectionVM gvConnection, ContractContainerVM contracts)
        {
            try
            {
                var request = new RestRequest("/v1/contract/ProcessContracts", Method.POST)
                {
                    RequestFormat = DataFormat.Json
                };

                string jsonBody = JsonConvert.SerializeObject(contracts);
                request.AddParameter("application/json", jsonBody, ParameterType.RequestBody);
                request.AddHeader("Authorization", gvConnection.ApiToken);

                string baseUrl;
                if (gvConnection.TestEnvironment)
                {
                    baseUrl = ConfigurationHelper.Value("UrlCustomerSandboxService");
                }
                else
                {
                    baseUrl = ConfigurationHelper.Value("UrlCustomerService");
                }

                var response = ConnectionHelper.GetResponse(request, baseUrl, "Customer API", "ProcessContracts", gvConnection.ApiKey, gvConnection.ApiSecret);

                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    LogHelper.Log("ERROR: " + response.Content);
                    throw new Exception("Error in response: " + response.Content);
                }

                LogHelper.Log(response.Content);
            }
            catch (Exception ex)
            {
                LogHelper.Log("ERROR: " + ex.Message);
            }
        }

    }
}
