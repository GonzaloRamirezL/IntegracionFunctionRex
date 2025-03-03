using Common.DTO.GeoVictoria;
using Common.Entity;
using Common.Enum;
using Common.Helper;
using Common.ViewModels;
using Helper;
using Helpers;
using IDAO.GeoVictoria;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DAO.GeoVictoria
{
    public class PlannerGeovictoriaDAO
    {
        public static List<LogEntity> InsertPlanner(List<PlannerContract> newShiftList, RexExecutionVM gvConnection)
        {
            List<LogEntity> logEntities = new List<LogEntity>();
            bool execute = false;
            bool.TryParse(ConfigurationHelper.Value("Execute"), out execute);
            string identifiers = String.Join(", ", newShiftList.Select(x => x.User).ToList());
            string authToken = gvConnection.ApiToken;
            string urlService = string.Empty;

            if (gvConnection.TestEnvironment)
            {
                urlService = ConfigurationHelper.Value("UrlCustomerSandboxService");
            }
            else
            {
                urlService = ConfigurationHelper.Value("UrlCustomerService");
            }

            foreach (PlannerContract newShift in newShiftList)
            {
                LogHelper.Log($"Send Planning for user identifier: {newShift.User}");
                foreach (ShiftContract s in newShift.Shift)
                {
                    string message = string.Empty;
                    message = $"Load - Shift = {s.ShiftId} - Date = {s.Date}";
                    LogHelper.Log(message);
                    logEntities.Add(LogEntityHelper.Planning(gvConnection, LogEvent.ADD, newShift.User , message , LogType.Info));
                }
            }

            if (execute)
            {
                RestClient client = new RestClient(urlService);
                var request = new RestRequest("/v1/Planning", RestSharp.Method.POST);
                request.AddHeader("Authorization", authToken);
                request.AddJsonBody(newShiftList);
                var response = client.Execute(request);

                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    string message = string.Empty;
                    message = ($"Error loading planning - Identifiers : {identifiers} - Details : {response.StatusDescription} ");
                    LogHelper.Log(message);
                    logEntities.Add(LogEntityHelper.Planning(gvConnection, LogEvent.ADD, "Lista Usuarios", message, LogType.Error));
                }
                else
                {
                    LogHelper.Log($"The planning created successfully");
                }
            }
            return logEntities;
        }
    }
}
