using Common.ViewModels;
using Helper;
using IDAO.Rex;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Common.DTO.Rex;
using System.Threading.Tasks;

namespace DAO.Rex
{
    public class DesarrolloOrganizacionalRexDAO : BaseRexDAO, IDesarrolloOrganizacionalRexDAO
    {
        public List<ObjetoCatalogo> GetCargos(List<string> urls, string token, VersionConfiguration versionConfiguration)
        {
            return this.GetAll<ObjetoCatalogo>(urls, versionConfiguration.DESARROLLO_ORGANIZACIONAL_CARGOS_URL, token, versionConfiguration, "?detalle=True");
        }
    }    
}
