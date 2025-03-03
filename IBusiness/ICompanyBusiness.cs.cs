using Common.ViewModels;
using System.Collections.Generic;
using Common.DTO.Rex;
using Common.ViewModels.API;
using System;
using Helper;

namespace IBusiness
{
    public interface ICompanyBusiness
    {
        List<RexCompanyVM> GetRexCompanies(RexExecutionVM filter, VersionConfiguration versionConfiguration);
    }
}
