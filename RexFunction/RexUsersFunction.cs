using Business;
using Common.Enum;
using Common.ViewModels;
using IBusiness;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace RexFunction
{
    public static class RexUsersFunction
    {
        [FunctionName("Users")]
        public static HttpResponseMessage Users([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequestMessage request, ILogger log)
        {
            try
            {
                // Deserializa el objeto entregado en json.
                // Si faltan configuracion en el ojbejo, se asignarán los valores por defecto
                // que se encuentran seteados en la clase RexExecutionVM
                string requestBody = request.Content.ReadAsStringAsync().Result;
                var rexExecution = JsonConvert.DeserializeObject<RexExecutionVM>(requestBody);
                (bool isOK, string message) = IsRexExecutionOK(rexExecution);

                

                if (isOK)
                {
                    IUserBusiness userBusiness = new UserBusiness(rexExecution);
                    var employees = userBusiness.GetEmployeesData(rexExecution);
                    log.LogInformation($"{rexExecution.CompanyName} - {employees.Count} Usuarios obtenidos");

                    HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
                    response.Content = new StringContent(JsonConvert.SerializeObject(employees));
                    response.RequestMessage = request;
                    return (response);

                }
                else
                {
                    HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.BadRequest);
                    response.Content = new StringContent(JsonConvert.SerializeObject(message));
                    response.RequestMessage = request;
                    return (response);
                }
            }
            catch (System.Exception ex)
            {
                HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.BadRequest);
                response.Content = new StringContent(
                                    JsonConvert.SerializeObject("Request caused unexpected error."));
                response.RequestMessage = request;
                return (response);
            }
        }


        private static (bool, string) IsRexExecutionOK(RexExecutionVM rexExecution)
        {
            if (rexExecution.RexCompanyDomains == null || rexExecution.RexCompanyDomains.Count == 0)
            {
                return (false, "RexCompanyDomains cannot be empty");
            }

            if (rexExecution.RexCompanyCodes == null || rexExecution.RexCompanyCodes.Count == 0)
            {
                return (false, "RexCompanyCodes cannot be empty");
            }

            return (true, string.Empty);

        }
    }
}
