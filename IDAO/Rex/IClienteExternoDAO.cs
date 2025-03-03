using Common.DTO.Rex;
using Helper;
using System;
using System.Collections.Generic;

namespace IDAO.Rex
{
    public interface IClienteExternoDAO
    {
        /// <summary>
        /// Método asincrónico para obtener solicitudes de cotizaciones de múltiples URLs.
        /// </summary>
        /// <param name="urls"></param>
        /// <param name="token"></param>
        /// /// <param name="versionConfiguration"></param>
        /// <returns></returns>
        List<Cotizaciones> GetPriceRequests(List<string> urls, string token, VersionConfiguration versionConfiguration);

    }
}
