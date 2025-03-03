using Common.Enum;
using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Text;

namespace Helper
{
    public class VersionConfiguration
    {
        public string BASE_URL = "https://{0}.rexmas.cl";
        public string CATALOGO_CARGO_URL = "/api/v2/catalogo/cargos";
        public string CATALOGO_CENTRO_COSTOS_URL = "/api/v2/catalogo/centroCost";
        public string CATALOGO_SEDES_URL = "/api/v2/catalogo/sedes";
        public string CONCEPTOS_ASISTENCIA_URL = "/api/v2/conceptos_asistencia";
        public string CONCEPTOS_ASITENCIA_VALORIZAR_URL = "/api/v2/conceptos_asistencia/valorizar";
        public string CONTRATOS_URL = "/api/v2/contratos";
        public string PROCESOS_ASISTENCIA_URL = "/api/v2/procesos_asistencia";
        public string EMPLEADOS_URL = "/api/v2/empleados";
        public string EMPLEADOS_PLANTILLAS_INASISTENCIAS_URL = "/api/v2/empleados/plantillas_inasistencias";
        public string EMPLEADOS_EMPLEADO_PLANTILLA_INASISTENCIAS_URL = "/api/v2/empleados/{0}/plantilla_inasistencias";
        public string EMPLEADOS_PLANTILLA_INASISTENCIAS_AUSENCIA_URL = "/api/v2/empleados/plantilla_inasistencias/{0}";
        public string EMPLEADOS_PLANTILLAS_INASISTENCIAS_SPYL_URL = "/api/v2/empleados/plantillas_inasistencias?solo_permisos_y_licencias=1";
        public string EMPRESAS_URL = "/api/v2/empresas";
        public string PERMISOS_ADMINISTRATIVOS_URL = "/api/v2/permisos_administrativos";
        public string VACACIONES_URL = "/api/v2/vacaciones";

        public string ALIADOS_CLIENTE_EXTERNO_COTIZACION = "/api/v3/aliados/cliente_externo/cotizacion";
        public string ALIADOS_TURNOS_URL = "/api/v3/aliados/turnos";
        public string DESARROLLO_ORGANIZACIONAL_CARGOS_URL = "/api/v3/desarrollo_organizacional/cargos";

        public int VERSION = RexVersions.DEFAULT;

        public VersionConfiguration(int RexMasVersion)
        {
            if ( RexMasVersion == RexVersions.DEFAULT )
            {
                LogHelper.Log($"Company Configuration Version: " + RexMasVersion);
            }
            else if ( RexMasVersion == RexVersions.V3 )
            {
                LogHelper.Log($"Company Configuration Version: " + RexMasVersion);
                BASE_URL = "https://{0}.rexmas.com";
                CATALOGO_CARGO_URL = "/remuneraciones/es-PE/api/v3/catalogo/cargos";
                CATALOGO_CENTRO_COSTOS_URL = "/remuneraciones/es-PE/api/v3/catalogo/centroCost";
                CATALOGO_SEDES_URL = "/remuneraciones/es-PE/api/v3/catalogo/sedes";
                CONCEPTOS_ASISTENCIA_URL = "/remuneraciones/es-PE/api/v3/conceptos_asistencia";
                CONCEPTOS_ASITENCIA_VALORIZAR_URL = "/remuneraciones/es-PE/api/v3/conceptos_asistencia/valorizar";
                CONTRATOS_URL = "/remuneraciones/es-PE/api/v3/contratos";
                PROCESOS_ASISTENCIA_URL = "/remuneraciones/es-PE/api/v3/procesos_asistencia";
                EMPLEADOS_URL = "/remuneraciones/es-PE/api/v3/empleados";
                EMPLEADOS_PLANTILLAS_INASISTENCIAS_URL = "/remuneraciones/es-PE/api/v3/empleados/plantillas_inasistencias";
                EMPLEADOS_EMPLEADO_PLANTILLA_INASISTENCIAS_URL = "/remuneraciones/es-PE/api/v3/empleados/{0}/plantilla_inasistencias";
                EMPLEADOS_PLANTILLA_INASISTENCIAS_AUSENCIA_URL = "/remuneraciones/es-PE/api/v3/empleados/plantilla_inasistencias/{0}";
                EMPLEADOS_PLANTILLAS_INASISTENCIAS_SPYL_URL = "/remuneraciones/es-PE/api/v3/empleados/plantillas_inasistencias?solo_permisos_y_licencias=1";
                EMPRESAS_URL = "/remuneraciones/es-PE/api/v3/empresas";
                PERMISOS_ADMINISTRATIVOS_URL = "/remuneraciones/es-PE/api/v3/permisos_administrativos";
                VACACIONES_URL = "/remuneraciones/es-PE/api/v3/vacaciones";

                VERSION = RexVersions.V3;
            }
            else
            {
                throw new Exception("Rex+ version can't be configurable, recived version: "+ RexMasVersion);
            }
        }
    }
}
