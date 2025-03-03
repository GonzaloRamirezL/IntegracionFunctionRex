using Common.DTO.GeoVictoria;
using Common.ViewModels;
using System.Collections.Generic;

namespace IDAO.GeoVictoria
{
    public interface ITimeOffGeoVictoriaDAO
    {
        /// <summary>
        /// Get all permission types from GeoVictoria
        /// </summary>
        /// <returns></returns>
        List<TimeOffTypeVM> GetAllTypes(GeoVictoriaConnectionVM gvConnection);

        /// <summary>
        /// Get all permission within an date interval from GeoVictoria
        /// </summary>
        /// <returns></returns>
        List<TimeOffVM> GetAll(GeoVictoriaConnectionVM gvConnection, FilterContractRex filter);

        /// <summary>
        /// Add a new permission to GeoVictoria
        /// </summary>
        /// <param name="timeOff"></param>
        /// <returns></returns>
        (bool success, string message) Add(TimeOffVM timeOff, GeoVictoriaConnectionVM gvConnection);

        /// <summary>
        /// Add a new permission type to GeoVictoria
        /// </summary>
        /// <param name="timeOff"></param>
        /// <returns></returns>
        bool AddType(PermissionTypeContract timeOffType, GeoVictoriaConnectionVM gvConnection);

        /// <summary>
        /// Delete a permission in GeoVictoria
        /// </summary>
        /// <param name="timeOff"></param>
        /// <returns></returns>
        (bool success, string message) Delete(TimeOffVM timeOff, GeoVictoriaConnectionVM gvConnection);
    }
}
