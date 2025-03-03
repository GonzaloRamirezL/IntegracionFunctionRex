namespace Common.Enum
{
    public enum ConceptType
    {
        Absences,           //Ausencias
        Overtime,           //Horas Extras
        Delay,              //Atraso
        EarlyLeave,         //Adelanto
        WorkedTime,         //Horas Trabajadas
        WorkedDays,         //Dias Trabajadas
        NonWorkedHours,     //Horas No Trabajadas
        ActuallyWorkedNightHours    //Horas Nocturnas Realmente Trabajadas
    }

    public enum AllowTimeOffs
    {
        All,                //Considerar dias con y sin permisos
        Only,               //Solo considerar permisos
        None                //No considerar permisos
    }

    public enum AllowOvertime
    {
        FulfilledOvertime,  //Horas Extras Cumplidas
        AllExtraTime        //Se consideran todas las horas trabajadas fuera de turno
    }

    public enum AllowOvertimeType
    {
        Total,              //El total de Horas Extras
        Before,             //Solo Horas Extras antes del Turno
        After,              //Solo Horas Extras despues del Turno
    }

}
