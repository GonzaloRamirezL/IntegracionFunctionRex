namespace Common.Enum
{
    public static class LogType
    {
        public const string Info = "Información";
        public const string Warning = "Advertencia";
        public const string Error = "Error";
    }

    public static class LogEvent
    {
        public const string ADD = "Agregar";
        public const string EDIT = "Editar";
        public const string DISABLE = "Deshabilitar";
        public const string ENABLE = "Habilitar";
        public const string MOVE = "Mover";
        public const string DELETE = "Eliminar";
        public const string NONE = "N/A";
        public const string GET = "Obtener";
    }
    public static class LogItem
    {
        public const string USER = "Usuario";
        public const string GROUP = "Grupo";
        public const string POSITION = "Cargo";
        public const string SHIFT = "Turno";
        public const string TIMEOFF = "Permiso";
        public const string DELAY = "Atraso";
        public const string EARLY_LEAVE = "Adelanto";
        public const string OVERTIME = "Horas Extras";
        public const string WORKED_TIME = "Horas Trabajadas";
        public const string WORKED_DAYS = "Días Trabajados";
        public const string ABSENT_DAYS = "Días Ausentes";
        public const string ABSENCE = "Ausencia";
        public const string EXECUTION = "Ejecución";
        public const string NON_WORKED_TIME = "Horas no Trabajadas";
        public const string PLANNING = "Planificaciones";
        public const string ACTUALLY_NIGHT_WORKED_TIME = "Horas Nocturnas Realmente Trabajadas";
        public const string COTIZACIONES = "Cotizaciones";
        public const string URL = "UrlRex+";
    }

    public static class LogTable
    {
        public const string NAME = "SynchronizationLog";
    }


}
