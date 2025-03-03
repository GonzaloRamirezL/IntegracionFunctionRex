using Common.ViewModels;
using System.Collections.Generic;
using Common.DTO.Rex;
using Common.ViewModels.API;
using System;

namespace IBusiness
{
    public interface IUserBusiness
    {
        /// <summary>
        /// Sincroniza empleados de Rex+ a usuarios de GeoVictoria
        /// </summary>
        void SynchronizeUsers(RexExecutionVM rexExecutionVM);

        /// <summary>
        /// Obtiene los datos completos de los empleados desde Rex+
        /// </summary>
        /// <param name="rexExecutionVM"></param>
        /// <returns></returns>
        List<Empleado> GetEmployeesData(RexExecutionVM rexExecutionVM);

        List<UserVM> CopyAdminsToSandbox(RexSandboxSyncVM rexSandboxSync);
        /// <summary>
        /// Obtiene los contratos vigentes, activos, considerando el filtro Utiliza Asistencia desde Rex+
        /// </summary>
        /// <param name="rexExecutionVM"></param>
        /// <param name="startProcessDate"></param>
        /// <param name="endProcessDate"></param>
        /// <param name="activeOnly"></param>
        List<Contrato> GetCurrentCompanyContractsByDate(RexExecutionVM rexExecutionVM, DateTime startProcessDate, DateTime endProcessDate, bool activeOnly = true);

    }
}
