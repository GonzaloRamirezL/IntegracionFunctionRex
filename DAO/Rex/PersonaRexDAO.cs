using Helper;
using IDAO.Rex;
using System;
using System.Collections.Generic;
using Common.DTO.Rex;

namespace DAO.Rex
{
    public class PersonaRexDAO : BaseRexDAO, IPersonaRexDAO
    {
        
        public List<Empleado> GetPersona(List<string> urls, string token, bool paginated, int daysSinceUpdate, VersionConfiguration versionConfiguration)
        {
            List<string> filterList = new List<string>();

            if (daysSinceUpdate > 0)
            {
                DateTime dateFrom = DateTime.Today.AddDays(-daysSinceUpdate);
                filterList.Add("fechaCambio__gt=" + DateTimeHelper.DateTimeToStringRex(dateFrom));
            }

            if (!paginated)
            {
                filterList.Add("paginar=0");
            }

            if (urls.Count > 1)
            {
                List<Empleado> result = new List<Empleado>();
                foreach (var url in urls)
                {
                    var employees = this.GetAllByUrl<Empleado>(url, versionConfiguration.EMPLEADOS_URL, token, versionConfiguration, $"?{string.Join("&", filterList)}");
                    employees.ForEach(emp => { emp.RexDomain = url; });
                    result.AddRange(employees);
                }

                return result;
            }
            else
            {
                return this.GetAll<Empleado>(urls, versionConfiguration.EMPLEADOS_URL, token, versionConfiguration, $"?{string.Join("&", filterList)}");
            }
        }
    
        public List<Contrato> GetContratos(List<string> urls, string token, bool paginar, int daysSinceUpdate, bool onlyActive, VersionConfiguration versionConfiguration)
        {
            List<string> filterList = new List<string>();
            daysSinceUpdate = 0;
            if (daysSinceUpdate > 0)
            {
                DateTime dateFrom = DateTime.Today.AddDays(-daysSinceUpdate);
                filterList.Add("fechaCambio__gt=" + DateTimeHelper.DateTimeToStringRex(dateFrom));
            }
            if (!paginar)
            {
                filterList.Add("paginar=0");
            }
            if (onlyActive)
            {
                filterList.Add($"fechaTerm=__gte={DateTimeHelper.DateTimeToStringRex(DateTime.Today.Date)}");
            }

            if (urls.Count > 1)
            {
                List<Contrato> result = new List<Contrato>();
                foreach (var url in urls)
                {
                    var contracts = this.GetAllByUrl<Contrato>(url, versionConfiguration.CONTRATOS_URL, token, versionConfiguration, $"?{string.Join("&",filterList)}");
                    contracts.ForEach(cont => { cont.RexDomain = url; });
                    result.AddRange(contracts);
                }

                return result;
            }
            else
            {
                return this.GetAll<Contrato>(urls, versionConfiguration.CONTRATOS_URL, token, versionConfiguration, $"?{string.Join("&", filterList)}");
            }
            
        }

        public List<Contrato> GetContractsFromDates(List<string> urls, string token, bool paginar, DateTime startDate, DateTime endDate, VersionConfiguration versionConfiguration)
        {
            List<string> filterList = new List<string>();

            filterList.Add("paginar=0");
            filterList.Add($"fechaInic=__lte={DateTimeHelper.DateTimeToStringRex(endDate)}");
            filterList.Add($"fechaTerm=__gte={DateTimeHelper.DateTimeToStringRex(startDate)}");

            if (urls.Count > 1)
            {
                List<Contrato> result = new List<Contrato>();
                foreach (var url in urls)
                {
                    var contracts = this.GetAllByUrl<Contrato>(url, versionConfiguration.CONTRATOS_URL, token, versionConfiguration, $"?{string.Join("&", filterList)}");
                    contracts.ForEach(cont => { cont.RexDomain = url; });
                    result.AddRange(contracts);
                }

                return result;
            }
            else
            {
                return this.GetAll<Contrato>(urls, versionConfiguration.CONTRATOS_URL, token, versionConfiguration, $"?{string.Join("&", filterList)}");
            }

        }
    }    
}
