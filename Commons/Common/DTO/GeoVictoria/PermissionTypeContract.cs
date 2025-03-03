namespace Common.DTO.GeoVictoria
{
    public class PermissionTypeContract
    {
        public string ID_TIPO_PERMISO { get; set; }
        public string DESCRIPCION_TIPO_PERMISO { get; set; }
        public bool? CON_GOCE_SUELDO { get; set; }
        public bool PERMITE_MARCA { get; set; }
        public bool? PERMISO_PARCIAL { get; set; }
        public string CANTIDAD_HORAS { get; set; }
        public bool? PERMISO_POR_HORAS { get; set; }
        public string IDENTIFICADOR_EXTERNO_TIPO_PERMISO { get; set; }
        public string descripcion { get; set; }
        public string hashedId { get; set; }
    }
}
