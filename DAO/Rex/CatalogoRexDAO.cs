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
    public class CatalogoRexDAO : BaseRexDAO, ICatalogoRexDAO
    {
        public List<ObjetoCatalogo> GetSedes(List<string> urls, string token, VersionConfiguration versionConfiguration)
        {
            return this.GetAll<ObjetoCatalogo>(urls, versionConfiguration.CATALOGO_SEDES_URL, token, versionConfiguration, "?detalle=True");
        }

        public List<ObjetoCatalogo> GetCentrosCosto(List<string> urls, string token, VersionConfiguration versionConfiguration)
        {
            return this.GetAll<ObjetoCatalogo>(urls, versionConfiguration.CATALOGO_CENTRO_COSTOS_URL, token, versionConfiguration, "?detalle=True");
        }

        public List<ObjetoCatalogo> GetCargos(List<string> urls, string token, VersionConfiguration versionConfiguration)
        {
            return this.GetAll<ObjetoCatalogo>(urls, versionConfiguration.CATALOGO_CARGO_URL, token, versionConfiguration, "?detalle=True");
        }
    }    
}
