using System;
using Common.ViewModels;
using Common.DTO.Rex;
using System.Collections.Generic;
using Helper;


namespace IDAO.Rex
{
    public interface IInasistenciaRexDAO
    {
        ResponseRexMessage AddInasistencia(string baseURL, string token, string employee, Inasistencia absence, VersionConfiguration versionConfiguration, bool HasIDSeparator);
        List<PlantillaInasistencia> GetInasistencia(string baseURL, string token, DateTime startDate, DateTime endDate, VersionConfiguration versionConfiguration);
        ResponseRexMessage DeleteInasistencia(string baseURL, string token, int absenceId, VersionConfiguration versionConfiguration);
    }
}
