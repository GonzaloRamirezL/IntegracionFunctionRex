using System;
using Common.ViewModels;
using Common.DTO.Rex;
using System.Collections.Generic;
using Helper;


namespace IDAO.Rex
{
    public interface IConceptoAsistenciaRexDAO
    {
        List<Concepto> GetConceptosAsistencia(string baseURL, string token, string attendanceProcess, VersionConfiguration versionConfiguration);
        ResponseRexMessage AddConceptoAsistencia(string baseURL, string token, List<Concepto> concepts, VersionConfiguration versionConfiguration);
        ResponseRexMessage ModifyProcesoAsistencia(string baseURL, string token, List<Concepto> concepts, VersionConfiguration versionConfiguration);
    }
}
