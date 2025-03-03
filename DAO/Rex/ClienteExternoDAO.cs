using Common.DTO.Rex;
using Helper;
using IDAO.Rex;
using System.Collections.Generic;


namespace DAO.Rex
{
    public class ClienteExternoDAO : BaseRexDAO, IClienteExternoDAO
    {
        public List<Cotizaciones> GetPriceRequests(List<string> urls, string token, VersionConfiguration versionConfiguration)
        {
            List<Cotizaciones> result = new List<Cotizaciones>();

            foreach (var url in urls)
            {
                List<Cotizaciones> listaCotizaciones = GetAllByUrl<Cotizaciones>(url, versionConfiguration.ALIADOS_CLIENTE_EXTERNO_COTIZACION, token, versionConfiguration);

                if (listaCotizaciones == null)
                {
                    LogHelper.Log("Obtención de contratos devolvió null. Verifique la respuesta de la API.");
                    continue;
                }

                listaCotizaciones.ForEach(contract => contract.RexDomain = url);
                lock (result)
                {
                    result.AddRange(listaCotizaciones);
                }
            }

            return result;
        }
    }
}
