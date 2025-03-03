using System;
using System.Collections.Generic;

namespace Common.ViewModels
{
    public class RexExecutionVM
    {
        /****** Flag si es que la sincronización es de prueba (Sandbox) ******/
        public bool TestEnvironment { get; set; } = true;

        /****** Datos de la empresa ******/
        //public int CompanyId { get; set; }
        public string CompanyName { get; set; }
        public string ApiSecret { get; set; }
        public string ApiKey { get; set; }
        public string ApiToken { get; set; }
        public List<string> RexCompanyCodes { get; set; }
        public List<string> RexCompanyDomains { get; set; }
        public string RexToken { get; set; }

        /******* Datos a Integrar *******/
        public List<string> Operations { get; set; }

        /******* Usuarios *******/

        /// <summary>
        /// Dias hacia atras a sincronizar desde Rex+. 
        /// Aplica solo a los usuarios y sus ultimos cambios.
        /// Si es 0, se sincroniza usuarios completamente.
        /// </summary>
        public int DaysSinceUsersUpdated { get; set; }
        public bool CreateGroups { get; set; } = true;
        public bool MoveUsers { get; set; } = true;
        public bool AssignNewUsersToTempGroup { get; set; } = true;
        public bool DisableGroupIfNotFound { get; set; } = false;
        public bool DisableUserIfContractEnds { get; set; } = true;
        public bool DisableUserIfContractIsZero { get; set; } = true;
        public bool DisableUserIfSituacionF { get; set; } = true;
        public bool DisableUserIfSituacionS { get; set; } = true;
        public bool DisableUserIfModalidadContratoS { get; set; } = true;
        public string MatchGVGroupsWith { get; set; }
        public bool CreateAllPositions { get; set; } = true;
        public List<string> ExcludeFromDisable { get; set; } = new List<string>();
        public bool DisableNonIntegratedUser { get; set; } = true;
        public bool Paginated { get; set; } = false;
        public bool UtilizaAsistencia { get; set; } = true;
        public string CausalCode { get; set; } 
        public bool UseMultiContracts { get; set; }

        /******* Asistencia *******/
        public List<ConceptVM> JsonConcepts {  get; set; }
        public DateTime ProcessStartDate {  get; set; }
        public DateTime ProcessEndDate {  get; set; }
        public int TimeOffDaysRange { get; set; } = 30;
        public bool DeleteTimeOffs { get; set; } = true;
        public bool ExtraTimeOffs { get; set; } = false;
        public bool DelNotSyncTimeOff { get; set; } = true;

        /******* Inasistencias *******/
        public bool AbsentCalendarPeriod { get; set; } = false;

        /******* Fecha de Ejecución *******/
        public string ExecutionDateTime { get; set; }

        /******* Turnos y Planificaciones *******/
        public int QuantityOfWeeks { get; set; }
        public string ExcludedGroups { get; set; }

        /***** Desarrollo Organizacional *****/
        public bool OrgDevelopment { get; set; }

        /****** Sincronizar usuarios con mail repetido *****/
        public bool SyncRepeatedMailUsers { get; set; }

        /***** Versión de RexMas *****/
        public int RexVersion { get; set; } = 2;

        /***** Verificación si utiliza separador de ID (guión en RUT) *****/
        public bool HasIDSeparator { get; set; } = true;
    }
}
