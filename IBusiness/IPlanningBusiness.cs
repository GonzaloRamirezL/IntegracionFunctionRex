using Common.DTO.GeoVictoria;
using Common.DTO.Rex;
using Common.Entity;
using Common.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace IBusiness
{
    public interface IPlanningBusiness
    {
        /// <summary>
        /// Método principal que decide el orden de ejecución de los diferentes módulos del proceso de Turnos y Planificaciones
        /// </summary>
        /// <param name="rexExecutionVM">Objeto con toda la información para la autenticación</param>
        void SynchronizePlanning(RexExecutionVM rexExecutionVM);
        /// <summary>
        /// Este método valida que los clientes de Rex se encuentren en el listado de clientes activos de Geovictoria. 
        /// </summary>
        /// <param name="contractsClient">Listado de usuarios de Rex</param>
        /// <param name="usersGeovictoria">Listado de usuarios de Geovictoria</param>
        /// <returns></returns>
        public List<string> ValidateUsers(List<Contrato> contractsClient, List<UserVM> usersGeovictoria);
        /// <summary>
        /// Filtrar unicamente los contratos con los identificadores que existen en ambas plataformas
        /// </summary>
        /// <param name="contractsClient"> Contratos de Rex</param>
        /// <param name="validatedUsersIdentifiers"> Usuarios que existen en Geovictoria</param>
        /// <returns></returns>
        List<Contrato> FilterScheduler(List<Contrato> contractsClient, List<string> validatedUsersIdentifiers);
        /// <summary>
        /// Obtener el libro de asistencia de Geovictoria
        /// </summary>
        /// <param name="identifiers">Identificadores de los usuarios que se van a solicitar</param>
        /// <param name="startDateTime">Fecha inicio para la busqueda del libro de asistencia</param>
        /// <param name="endDateTime">Fecha de fin para la busqueda del libro de asistencia</param>
        /// <param name="rexExecutionVM">Objeto con toda la información para la autenticación</param>
        /// <returns></returns>
        AttendanceContract GetAttendanceBookWithPagination(List<string> identifiers, DateTime? startDateTime, DateTime? endDateTime, RexExecutionVM rexExecutionVM);
        /// <summary>
        /// Método que organiza la información del libro de asistencia en un objeto facíl de comparar con Rex +
        /// </summary>
        /// <param name="attendanceBook">Libro de asistencia</param>
        /// <returns></returns>
        List<PlannerContract> FilterAttendanceBookGV(AttendanceContract attendanceBook);
        /// <summary>
        /// Método para obtener un listado de tipos de turnos de rex que se deben crear nuevos en Geovictoria
        /// </summary>
        /// <param name="shiftGVList">Lista de turnos de Geovictoria</param>
        /// <param name="ShiftListRex">Lista de turnos de Rex +</param>
        /// <returns></returns>
        List<ShiftInsertGVContract> GetNewShiftTypes(List<ShiftListGVContract> shiftGVList, List<TurnosRex> ShiftListRex);
        /// <summary>
        /// Método utilizado por el método  GetNewShiftTypes, para crear el objeto con el que se van a crear los turnos.
        /// </summary>
        /// <param name="startHour">Hora de inicio del turno en formato HH:MM</param>
        /// <param name="endHour">Hora de fin del turno en formato HH:MM</param>
        /// <param name="breakMinutes">Cantidad de minutos de colación</param>
        /// <returns></returns>
        ShiftInsertGVContract FillListOfShiftTypes(string startHour, string endHour, string breakMinutes);
        /// <summary>
        /// Método que procese todos los turnos de Rex comparados con los de Geovictoria y los transforme en un formato para enviarlos 
        /// </summary>
        /// <param name="shiftGVList">Lista de turnos de Geovictoria</param>
        /// <param name="shiftListRex">Lista de turnos de Rex +</param>
        /// <param name="contracts">Lista de contratos de Rex +</param>
        /// <returns></returns>
        List<shiftRexVM> ProcessShifts(List<ShiftListGVContract> shiftGVList, List<TurnosRex> shiftListRex, List<Contrato> contracts);
        /// <summary>
        /// Método para cargar la planificación de los días que no estan en Geovictoria con turno asignado. 
        /// </summary>
        /// <param name="shiftToCreate"> Turnos que se van a crear</param>
        /// <param name="scheduleRex">Turnos en Rex mas</param>
        /// <param name="startTime">Fecha de inicio</param>
        /// <param name="endTime">Fecha de fin</param>
        /// <returns></returns>
        List<PlannerContract> CompleteSchedule(List<PlannerContract> shiftToCreate, List<shiftRexVM> scheduleRex, DateTime startTime, DateTime endTime);
        /// <summary>
        /// Asignar Planificación en Geovictoria
        /// </summary>
        /// <param name="shiftToSend">Lista de turnos a enviar a Geovictoria</param>
        /// <param name="rexExecutionVM">Objeto con las credenciales de Geovictoria y Rex +</param>
        List<LogEntity> SendSchedulers(List<PlannerContract> shiftToSend, RexExecutionVM rexExecutionVM);

    }
}
