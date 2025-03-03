using Common.ViewModels;
using Common.ViewModels.API;
using System.Collections.Generic;

namespace IDAO.GeoVictoria
{
    public interface IGroupGeoVictoriaDAO
    {
        /// <summary>
        /// Get group list from GeoVictoria
        /// </summary>
        /// <returns></returns>
        List<GroupApiVM> GetAll(GeoVictoriaConnectionVM gvConnection);

        /// <summary>
        /// Add new group to GeoVictoria
        /// </summary>
        /// <param name="newGroupPath"></param>
        /// <returns></returns>
        (bool success, string message) Add(string newGroupPath, GeoVictoriaConnectionVM gvConnection);

        /// <summary>
        /// Add new group to GeoVictoria
        /// </summary>
        /// <param name="newGroupPath"></param>
        /// <returns></returns>
        (bool success, string message) Delete(string costCenter, GeoVictoriaConnectionVM gvConnection);
    }
}
