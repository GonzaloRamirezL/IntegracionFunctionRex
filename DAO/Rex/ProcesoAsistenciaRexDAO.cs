using Common.ViewModels;
using Helper;
using IDAO.Rex;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Common.DTO.Rex;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Common.Enum;

namespace DAO.Rex
{
    public class ProcesoAsistenciaRexDAO : BaseRexDAO, IProcesoAsistenciaRexDAO
    {
        
        public List<ProcesoAsistencia> GetProcesoAsistencia(string baseURL, string token, VersionConfiguration versionConfiguration)
        {
            string filter = string.Empty;

            switch (versionConfiguration.VERSION)
            {
                case RexVersions.V3:
                    return this.GetAllByUrl<ProcesoAsistencia>(baseURL, versionConfiguration.PROCESOS_ASISTENCIA_URL, token, versionConfiguration, filter);
                default:
                    return this.GetList<ProcesoAsistencia>(baseURL, versionConfiguration.PROCESOS_ASISTENCIA_URL, token, versionConfiguration, filter);
            }
        }

        public void AddProcesoAsistencia(string baseURL, string token, ProcesoAsistencia attendanceProcess, VersionConfiguration versionConfiguration)
        {
            var response = this.Post<CreacionProcesoAsistencia>(
                baseURL, 
                versionConfiguration.PROCESOS_ASISTENCIA_URL, 
                token,
                new CreacionProcesoAsistencia()
                { 
                    fecha_inicio=attendanceProcess.fecha_inicio.ToString("yyyy-MM-dd"), 
                    fecha_fin=attendanceProcess.fecha_fin.ToString("yyyy-MM-dd"), 
                    proceso=attendanceProcess.proceso,
                    empresa=attendanceProcess.empresa
                },
                versionConfiguration);
        }

    }    

}
