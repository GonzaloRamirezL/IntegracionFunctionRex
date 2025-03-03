using System;
using System.Collections.Generic;

namespace Common.ViewModels.API
{
    public class UserApiVM
    {
        public string CODIGO_INTEGRACION { get; set; }
        public string PIS { get; set; }
        public string TIPO_LOGIN_SSO { get; set; }
        public int? ID_GRUPO { get; set; }
        public string CODIGO_CENTRO_COSTOS { get; set; }
        public string NOMBRE_GRUPO { get; set; }
        public DateTime? FECHA_DESACTIVACION_AUTOMATICA { get; set; }
        public string NOMBRE_PERFIL { get; set; }
        public string RUT_RAZON_SOCIAL { get; set; }
        public string NOMBRE_RAZON_SOCIAL { get; set; }
        public string NOMBRE_CODIGO_JORNADA { get; set; }
        public string NOMBRE_CARGO { get; set; }
        public string TARJETA_USUARIO { get; set; }
        public int? APLICA_DOMINGOS_LEGALES { get; set; }
        public string NOMBRE_PLANIFICADOR { get; set; }
        public bool OCULTAR_EN_REPORTES { get; set; }
        public int? ID_CODIGO_JORNADA_TRABAJO { get; set; }
        public string COL_PERSONALIZADA_3 { get; set; }
        public int ID_USUARIO { get; set; }
        public int? ID_CARGO { get; set; }
        public int ID_PERFIL { get; set; }
        public string IDENTIFICADOR_USUARIO { get; set; }
        public string NOMBRE { get; set; }
        public string APELLIDO { get; set; }
        public DateTime? FECHA_CONTRATO { get; set; }
        public short? HABILITADO { get; set; }
        public string FONO_USUARIO { get; set; }
        public string DIRECCION_USUARIO { get; set; }
        public long? EMPRESA_BASE { get; set; }
        public int? RAZON_SOCIAL { get; set; }
        public string COL_PERSONALIZADA_1 { get; set; }
        public string COL_PERSONALIZADA_2 { get; set; }
        public string CORREO_ELECTRONICO { get; set; }
        public bool FUERZA_CREACION_SI_EXISTE_DESACTIVO { get; set; }
    }
}
