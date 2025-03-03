using System;
using Common.ViewModels;
using Common.DTO.Rex;
using System.Collections.Generic;
using Helper;


namespace IDAO.Rex
{
    public interface IDesarrolloOrganizacionalRexDAO
    {

        /// <summary>
        /// Retorna los cargos desde Rex+.
        /// </summary>
        /// <returns></returns>
        List<ObjetoCatalogo> GetCargos(List<string> urls, string token, VersionConfiguration versionConfiguration);
    }
}
