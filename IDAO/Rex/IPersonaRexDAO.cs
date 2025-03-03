using System;
using Common.ViewModels;
using Common.DTO.Rex;
using System.Collections.Generic;
using Helper;


namespace IDAO.Rex
{
    public interface IPersonaRexDAO
    {
        /// <summary>
        /// Get all users from Rex+ API 1st load
        /// </summary>
        /// <returns></returns>
        List<Empleado> GetPersona(List<string> urls, string token, bool paginar, int daysSinceUpdate, VersionConfiguration versionConfiguration);        

        /// <summary>
        /// Get all contracts from Rex+ API 1st load
        /// </summary>
        /// <returns></returns>
        List<Contrato> GetContratos(List<string> urls, string token, bool paginar, int daysSinceUpdate, bool onlyActive, VersionConfiguration versionConfiguration);

        List<Contrato> GetContractsFromDates(List<string> urls, string token, bool paginar, DateTime startDate, DateTime endDate, VersionConfiguration versionConfiguration);
    }
}
