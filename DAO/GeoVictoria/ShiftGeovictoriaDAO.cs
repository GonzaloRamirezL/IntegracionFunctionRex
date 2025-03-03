using Common.DTO.GeoVictoria;
using Common.Entity;
using Common.Enum;
using Common.Helper;
using Common.ViewModels;
using Helper;
using Helpers;
using IDAO.GeoVictoria;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Authenticators;
using System.Collections.Generic;

namespace DAO.GeoVictoria
{
    public class ShiftGeovictoriaDAO
    {
        public static List<ShiftListGVContract> GetShifts(GeoVictoriaConnectionVM gvConnection)
        {
            LogHelper.Log("START: Get list of shifts types");
            IRestClient client = ConnectionHelper.ConnectGeoVictoria(gvConnection);
            var request = new RestRequest("/Shift/List", Method.POST);
            request.RequestFormat = DataFormat.Json;
            var response = client.Execute(request);
            var content = response.Content;
            List<ShiftListGVContract> shifts = JsonConvert.DeserializeObject<List<ShiftListGVContract>>(content);
            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                LogHelper.Log("Error in process = " + response.StatusDescription);
            }
            else
            {
                foreach (ShiftListGVContract shifttype in shifts)
                {
                    LogHelper.Log($"Shift: {shifttype.DESCRIPTION}");
                }
            }
            LogHelper.Log("FINISH: Get list of shifts types");
            return shifts;
        }

        public static List<LogEntity> InsertShift(List<ShiftInsertGVContract> newShiftList, RexExecutionVM gvConnection)
        {
            List<LogEntity> logEntities = new List<LogEntity>();
            LogHelper.Log("START: CREATE SHIFT IN GV ");
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
            foreach (ShiftInsertGVContract newShift in newShiftList)
            {
                RestClient client = new RestClient(urlService);
                var request = new RestRequest("/v1/Shift/Insert", RestSharp.Method.POST);
                request.AddHeader("Authorization", authToken);
                request.AddJsonBody(newShift);
                var response = client.Execute(request);
                string message = string.Empty;
                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    message = $"Error Loading shift in Geovictoria - Start Hour: {newShift.StartHour} - End Hour : {newShift.EndHour} - Break Minutes: {newShift.BreakMinutes} - Details : {response.StatusDescription}";
                    LogHelper.Log(message);
                    logEntities.Add(LogEntityHelper.Planning(gvConnection, LogEvent.ADD, "Turno", message, LogType.Error));
                }
                else
                {
                    message = $"The shift is loaded successfully - Start Hour: {newShift.StartHour} - End Hour : {newShift.EndHour} - Break Minutes: {newShift.BreakMinutes}";
                    LogHelper.Log(message);
                    logEntities.Add(LogEntityHelper.Planning(gvConnection, LogEvent.ADD, "Turno", message, LogType.Info));
                }
            }
            LogHelper.Log("FINISH: CREATE SHIFT IN GV ");
            return logEntities;
        }
    }
}
