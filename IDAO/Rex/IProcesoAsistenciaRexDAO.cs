using System;
using Common.ViewModels;
using Common.DTO.Rex;
using System.Collections.Generic;
using Helper;


namespace IDAO.Rex
{
    public interface IProcesoAsistenciaRexDAO
    {
        List<ProcesoAsistencia> GetProcesoAsistencia(string baseURL, string token, VersionConfiguration versionConfiguration);

        void AddProcesoAsistencia(string baseURL, string token, ProcesoAsistencia attendanceProcess, VersionConfiguration versionConfiguration);
    }
}
