using System.Collections.Generic;
using System.Linq;
using Common.DTO.Rex;
using Helper;
using IDAO.Rex;
using Newtonsoft.Json;

namespace DAO.Rex
{
    public class ConceptoAsistenciaRexDAO : BaseRexDAO, IConceptoAsistenciaRexDAO
    {
        
        public List<Concepto> GetConceptosAsistencia(string baseURL, string token, string attendanceProcess, VersionConfiguration versionConfiguration)
        {
            string filter = "/valorizar?proceso_asistencia="+ attendanceProcess;

            return this.GetAllByUrl<Concepto>(baseURL, versionConfiguration.CONCEPTOS_ASISTENCIA_URL, token, versionConfiguration, filter);
        }

        public ResponseRexMessage AddConceptoAsistencia(string baseURL, string token, List<Concepto> concepts, VersionConfiguration versionConfiguration)
        {
            var rex = new ResponseRexLite<ConceptoBase>()
            {
                objetos = concepts.Select(c => new ConceptoBase()
                {
                    concepto = c.concepto,
                    contrato = c.contrato,
                    empleado = c.empleado,
                    proceso_asistencia = c.proceso_asistencia,
                    valor = c.valor
                }).ToList()
            };

            System.Console.WriteLine(JsonConvert.SerializeObject(rex));
            var response = this.Post<object>(baseURL, versionConfiguration.CONCEPTOS_ASITENCIA_VALORIZAR_URL, token, rex, versionConfiguration);

            if (response == null)
            {
                response = new ResponseRexMessage();
            }

            return response;
        }

        public ResponseRexMessage ModifyProcesoAsistencia(string baseURL, string token, List<Concepto> concepts, VersionConfiguration versionConfiguration)
        {
            var rex = new ResponseRexLite<ConceptoBase>()
            {
                objetos = concepts.Select(c => new ConceptoBase()
                {
                    concepto = c.concepto,
                    contrato = c.contrato,
                    empleado = c.empleado,
                    proceso_asistencia = c.proceso_asistencia,
                    valor = c.valor
                }).ToList()
            };
            var response = this.Put<ResponseRexLite<ConceptoBase>>(baseURL, versionConfiguration.CONCEPTOS_ASITENCIA_VALORIZAR_URL, token, rex, versionConfiguration);
            return response;
        }

    }    

}
