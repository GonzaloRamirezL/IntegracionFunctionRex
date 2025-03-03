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
    public static class RexAdminFunction
    {
        [FunctionName("SandboxAdmins")]
        public static HttpResponseMessage SandboxAdmins([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequestMessage request, ILogger log)
        {
            try
            {
                // Deserializa el objeto entregado en json.
                // Si faltan configuracion en el objeto, se asignarán los valores por defecto
                // que se encuentran seteados en la clase RexExecutionVM
                string requestBody = request.Content.ReadAsStringAsync().Result;
                var rexSandboxSync = JsonConvert.DeserializeObject<RexSandboxSyncVM>(requestBody);
                RexExecutionVM rexExecution = new RexExecutionVM() { RexVersion = rexSandboxSync.RexVersion, HasIDSeparator = rexSandboxSync.HasSeparator};
                    IUserBusiness userBusiness = new UserBusiness(rexExecution);
                    var employees = userBusiness.CopyAdminsToSandbox(rexSandboxSync);

                    HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
                    response.Content = new StringContent(JsonConvert.SerializeObject(employees));
                    response.RequestMessage = request;
                    return (response);
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


    }
}
