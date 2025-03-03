using Common.ViewModels;
using System.Collections.Generic;

namespace IDAO.GeoVictoria
{
    public interface  IPositionGeoVictoriaDAO
    {
        /// <summary>
        /// Get all positions from GeoVictoria
        /// </summary>
        /// <returns></returns>
        List<PositionVM> GetAll(GeoVictoriaConnectionVM gvConnection);
        /// <summary>
        /// Add a new GV position 
        /// </summary>
        /// <param name="posName"></param>
        /// <returns></returns>
        (bool success, string message) AddPosition(string posName, GeoVictoriaConnectionVM gvConnection);
    }
}
