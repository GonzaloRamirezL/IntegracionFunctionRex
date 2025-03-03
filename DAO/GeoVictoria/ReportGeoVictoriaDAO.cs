using Common.DTO.GeoVictoria;
using Common.ViewModels;
using Helper;
using IDAO.GeoVictoria;
using RestSharp;
using System;
using System.Collections.Generic;

namespace DAO.GeoVictoria
{
    public class ReportGeoVictoriaDAO : IReportGeoVictoriaDAO
    {
        public string RequestReport(List<string> users, DateTime start, DateTime end, string reportIdentifier, string reportFormat, GeoVictoriaConnectionVM gvConnection)
        {
            IRestClient client = ConnectionHelper.ConnectGeoVictoria(gvConnection);
            IRestRequest request = new RestRequest("Report/AddReportExtens", Method.POST);
            request.RequestFormat = DataFormat.Json;
            FilterContractReport filter = new FilterContractReport
            {
                Range = string.Join(",", users),
                from = DateTimeHelper.DateTimeToStringGeoVictoria(start),
                to = DateTimeHelper.DateTimeToStringGeoVictoria(end),
                format = reportFormat,
                identifierReporte = reportIdentifier
            };
            request.AddBody(filter);
            IRestResponse response = client.Execute(request);

            string output;
            if(response.IsSuccessful && response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                output = response.Content;
            }
            else
            {
                LogHelper.Log("Error Requesting report: " + response.Content);
                output = null;
            }           

            return output;
        }

        public string GetStatus(string reportIdentifier, GeoVictoriaConnectionVM gvConnection)
        {
            IRestClient client = ConnectionHelper.ConnectGeoVictoria(gvConnection);
            IRestRequest request = new RestRequest("Report/StatusReport", Method.POST);
            request.RequestFormat = DataFormat.Json;
            ReportContract filter = new ReportContract
            {
                IDENTIFICADOR = reportIdentifier
            };
            request.AddBody(filter);
            IRestResponse response = client.Execute(request);
            string output = string.Empty;
            if (response.IsSuccessful && response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                output = response.Content; ;
            }
            else
            {
                LogHelper.Log("Error Requesting report: " + response.Content);
                output = "Error";
            }

            return output;
        }
    }
}
