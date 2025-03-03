using Common.DTO.Rex;
using Helper;
using System;
using System.Collections.Generic;
using System.Text;

namespace DAO.Rex
{
    public class TurnosRexDAO : BaseRexDAO
    {
        public List<TurnosRex> GetShifts (List<string> urls, string token, VersionConfiguration versionConfiguration)
        {
            LogHelper.Log("START: GET SHIFT OF REX MAS");
            if (urls.Count > 1)
            {
                List<TurnosRex> result = new List<TurnosRex>();
                foreach (var url in urls)
                {
                    var turnosRex = this.GetAllByUrl<TurnosRex>(url, versionConfiguration.ALIADOS_TURNOS_URL, token, versionConfiguration, "");
                    result.AddRange(turnosRex);
                }

                return result;
            }
            else
            {
                return this.GetAll<TurnosRex>(urls, versionConfiguration.ALIADOS_TURNOS_URL, token,versionConfiguration, "");
            }
        }
    }
}
