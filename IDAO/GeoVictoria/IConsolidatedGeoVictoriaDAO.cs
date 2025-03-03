using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.DTO.GeoVictoria;

namespace IDAO.GeoVictoria
{
    public interface IConsolidatedGeoVictoriaDAO
    {
        /// <summary>
        /// Obtiene datos consolidados para una lista de usaurios
        /// </summary>
        /// <param name="users"></param>
        /// <returns></returns>
        List<ConsolidatedContract> GetConsolidatedGV(string key, string secret, List<string> users);
    }
}
