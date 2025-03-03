using Common.DTO.Rex;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.ViewModels;
using Helper;

namespace IDAO.Rex
{
    public interface ITimeOffRexDAO
    {
        List<TimeOffVM> GetVacationsFromDateRange(List<string> urls, string token, FilterContractRex filter, VersionConfiguration versionConfiguration);
        List<TimeOffVM> GetAbsentismFromDateRange(List<string> urls, string token, FilterContractRex filter, VersionConfiguration versionConfiguration);
        List<TimeOffVM> GetAdministrativeDayFromDateRange(List<string> urls, string token, FilterContractRex filter, VersionConfiguration versionConfiguration);
    }
}
