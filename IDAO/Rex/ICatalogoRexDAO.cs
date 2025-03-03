using System;
using Common.ViewModels;
using Common.DTO.Rex;
using System.Collections.Generic;
using Helper;


namespace IDAO.Rex
{
    public interface ICatalogoRexDAO
    {
        /// <summary>
        /// Muestra las instancias de sedes registradas.
        /// </summary>
        /// <returns></returns>
        List<ObjetoCatalogo> GetSedes(List<string> urls, string token, VersionConfiguration versionConfiguration);

        /// <summary>
        /// Muestra las instancias de centro de costos registradas.
        /// </summary>
        /// <returns></returns>
        List<ObjetoCatalogo> GetCentrosCosto(List<string> urls, string token, VersionConfiguration versionConfiguration);

        /// <summary>
        /// Muestra las instancias de centro de costos registradas.
        /// </summary>
        /// <returns></returns>
        List<ObjetoCatalogo> GetCargos(List<string> urls, string token, VersionConfiguration versionConfiguration);
    }
}
