using System;
using System.Collections.Generic;
using Common.DTO.Rex;
using Helper;
using IDAO.Rex;
using Newtonsoft.Json;

namespace DAO.Rex
{
    public class InasistenciaRexDAO : BaseRexDAO, IInasistenciaRexDAO
    {
        
        public ResponseRexMessage AddInasistencia(string baseURL, string token, string employee, Inasistencia absence, VersionConfiguration versionConfiguration, bool HasIDSeparator)
        {
            if (absence.contrato == null)
            {
                return new ResponseRexMessage()
                {
                    informacion = new List<string>() { "Usuario no tiene contrato" }
                };
            }
            if (!HasIDSeparator)
            {
                employee = employee.Replace("-", "");
            }

            var response = this.Post<object>(baseURL, string.Format(versionConfiguration.EMPLEADOS_EMPLEADO_PLANTILLA_INASISTENCIAS_URL, employee), token, absence, versionConfiguration);
            return response;
        }

        public List<PlantillaInasistencia> GetInasistencia(string baseURL, string token, DateTime startDate, DateTime endDate, VersionConfiguration versionConfiguration)
        {
            string filter = $"?fecha_inicio={DateTimeHelper.DateTimeToStringRex(startDate.Date)}&fecha_fin={DateTimeHelper.DateTimeToStringRex(endDate.Date)}";
            var result = this.GetAllByUrl<PlantillaInasistencia>(baseURL, versionConfiguration.EMPLEADOS_PLANTILLAS_INASISTENCIAS_URL, token, versionConfiguration, filter);
            result = result.FindAll(x => x.concepto == "faltaDias");
            return result;
        }

        public ResponseRexMessage DeleteInasistencia(string baseURL, string token, int absenceId, VersionConfiguration versionConfiguration)
        {

            var response = this.Delete(baseURL, string.Format(versionConfiguration.EMPLEADOS_PLANTILLA_INASISTENCIAS_AUSENCIA_URL, absenceId), token, versionConfiguration);

            return response;
        }

    }    

}
