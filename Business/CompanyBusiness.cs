using System.Collections.Generic;
using IBusiness;
using Common.ViewModels;
using DAO.Rex;
using Helper;

namespace Business
{
    public class CompanyBusiness : BaseRexDAO, ICompanyBusiness
    {
        public List<RexCompanyVM> GetRexCompanies(RexExecutionVM filter, VersionConfiguration versionConfiguration)
        {
            List<RexCompanyVM> rexCompanies = new List<RexCompanyVM>();
            List<string> urls = filter.RexCompanyDomains;

            foreach (var url in urls)
            {
                var rexCompany = this.GetAllByUrl<RexCompanyVM>(url, versionConfiguration.EMPRESAS_URL, filter.RexToken, versionConfiguration);

                foreach (var item in rexCompany)
                {
                    //Dentro del listado de empresas desde Rex+ viene una llamada "Todas las empresas",
                    //esta no es utilizada en la integracion y dejarla en el listado puede causar confusión
                    if (item.rut != "1-9" && filter.RexCompanyCodes.Contains(item.empresa))
                    {
                        rexCompanies.Add(item);
                    }
                }

            }

            return rexCompanies;
        }
    }
}
