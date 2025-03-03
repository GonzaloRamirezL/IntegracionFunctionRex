using Business;
using Common.Entity;
using Common.Enum;
using Common.Helper;
using Common.ViewModels;
using Helper;
using IBusiness;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace RexFunction
{
    public static class RexFunction
    {
        [FunctionName("Execute")]
        public static async Task<HttpResponseMessage> RunAsync([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequestMessage request, ILogger log)
        {
            try
            {
                // Deserializa el objeto entregado en json.
                // Si faltan configuracion en el ojbejo, se asignarán los valores por defecto
                // que se encuentran seteados en la clase RexExecutionVM
                string requestBody = request.Content.ReadAsStringAsync().Result;
                log.LogInformation("Request Body for Execution: " + requestBody);
                var rexExecution = JsonConvert.DeserializeObject<RexExecutionVM>(requestBody);
                (bool isOK, string message) = IsRexExecutionOK(rexExecution);

                if (isOK)
                {
                    Task.Run(() => RexSync(rexExecution, log));
                    return request.CreateResponse(HttpStatusCode.OK);
                }
                else
                {
                    log.LogWarning(message);
                    HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.BadRequest);
                    response.Content = new StringContent(JsonConvert.SerializeObject(message));
                    response.RequestMessage = request;
                    return (response);
                }
            }
            catch (System.Exception ex)
            {
                log.LogError( ex.Message + "-" + ex.StackTrace);
                HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.BadRequest);
                response.Content = new StringContent(
                                    JsonConvert.SerializeObject("Request caused unexpected error."));
                response.RequestMessage = request;
                return (response);
            }
        }

        private static async Task RexSync(RexExecutionVM rexExecution, ILogger log)
        {
            var now = DateTime.UtcNow;
            rexExecution.ExecutionDateTime = now.ToString("yyyyMMddHHmm");
            log.LogInformation($"{rexExecution.CompanyName} - Iniciando!");
            List<LogEntity> logEntities = new List<LogEntity>();
            var guid = Guid.NewGuid();

            foreach (var operation in rexExecution.Operations)
            {
                try
                {
                    switch (operation)
                    {
                        case Operation.USERS:
                            IUserBusiness userBusiness = new UserBusiness(rexExecution);
                            userBusiness.SynchronizeUsers(rexExecution);
                            log.LogInformation($"{rexExecution.CompanyName} - Usuarios finalizado");
                            break;
                        case Operation.TIMEOFFS:
                            ITimeOffBusiness timeOffBusiness = new TimeOffBusiness(rexExecution);
                            timeOffBusiness.SynchronizeAllTimesOff(rexExecution);
                            log.LogInformation($"{rexExecution.CompanyName} - Permisos finalizado");
                            break;
                        case Operation.ATTENDANCE_CONCEPTS:
                            IAttendanceBusiness attendanceBusiness = new AttendanceBusiness(rexExecution);
                            attendanceBusiness.SynchronizeAttendanceConcepts(rexExecution);
                            log.LogInformation($"{rexExecution.CompanyName} - Asistencia finalizado");
                            break;
                        case Operation.ABSENCES:
                            IAbsencesBusiness absencesBusiness = new AbsencesBusiness(rexExecution);
                            absencesBusiness.SynchronizeAbsences(rexExecution);
                            log.LogInformation($"{rexExecution.CompanyName} - Ausencias finalizado");
                            break;
                        case Operation.PLANNING:
                            IPlanningBusiness planningBusiness = new PlanningBusiness(rexExecution);
                            planningBusiness.SynchronizePlanning(rexExecution);
                            log.LogInformation($"{rexExecution.CompanyName} - Turnos y Planificaciones finalizado");
                            break;
                        default:
                            break;
                    }
                }
                catch (Exception ex)
                {
                    log.LogError(ex.Message + "-" + ex.StackTrace);
                    logEntities.Add(new LogEntity()
                    {
                        PartitionKey = rexExecution.CompanyName,
                        RowKey = $"{rexExecution.ExecutionDateTime}_{guid}",
                        LogType = LogType.Error,
                        Event = LogEvent.NONE,
                        Item = LogItem.EXECUTION,
                        Identifier = guid.ToString(),
                        Message = $"Exception: {ex.Message} - {ex.StackTrace}",
                        Timestamp = DateTime.UtcNow
                    });
                    new TableStorageHelper().Upsert<LogEntity>(logEntities, LogTable.NAME);
                }
                
            }

            logEntities.Add(new LogEntity()
            {
                PartitionKey = rexExecution.CompanyName,
                RowKey = $"{rexExecution.ExecutionDateTime}_{guid}",
                LogType = LogType.Info,
                Event = LogEvent.NONE,
                Item = LogItem.EXECUTION,
                Identifier = guid.ToString(),
                Message = $"Sincronizado {string.Join(", ", rexExecution.Operations)}",
                Timestamp = DateTime.UtcNow
            });
            new TableStorageHelper().Upsert<LogEntity>(logEntities, LogTable.NAME);
            var time = DateTime.UtcNow - now;
            log.LogInformation($"{rexExecution.CompanyName} - Finalizó en {time.ToString("h'h 'm'm 's's'")}");
        }

        private static (bool, string) IsRexExecutionOK(RexExecutionVM rexExecution)
        {
            if (rexExecution.RexCompanyDomains == null || rexExecution.RexCompanyDomains.Count == 0)
            {
                return (false, "RexCompanyDomains cannot be empty");
            }

            if (rexExecution.ApiKey == null)
            {
                return (false, "ApiKey cannot be null");
            }

            if (rexExecution.ApiSecret == null)
            {
                return (false, "ApiSecret cannot be null");
            }

            if (rexExecution.CompanyName == null)
            {
                return (false, "CompanyName cannot be null");
            }

            if (rexExecution.RexCompanyCodes == null || rexExecution.RexCompanyCodes.Count == 0)
            {
                return (false, "RexCompanyCodes cannot be empty");
            }

            return (true, string.Empty);

        }
    }
}
