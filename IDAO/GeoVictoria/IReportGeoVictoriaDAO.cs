using Common.ViewModels;
using System;
using System.Collections.Generic;

namespace IDAO.GeoVictoria
{
    public interface IReportGeoVictoriaDAO
    {
        /// <summary>
        /// Request report to report services from GeoVictoria
        /// </summary>
        /// <param name="users"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="reportIndetifier"></param>
        /// <param name="reportFormat"></param>
        /// <returns></returns>
        string RequestReport(List<string> users, DateTime start, DateTime end, string reportIndetifier, string reportFormat, GeoVictoriaConnectionVM gvConnection);

        /// <summary>
        /// Get status of repor request from GeoVictoria
        /// </summary>
        /// <param name="reportIdentifier"></param>
        /// <returns></returns>
        string GetStatus(string reportIdentifier, GeoVictoriaConnectionVM gvConnection);
    }
}
