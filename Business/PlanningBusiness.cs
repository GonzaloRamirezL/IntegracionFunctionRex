using System;
using System.Collections.Generic;
using System.Text;
using Common.DTO.Rex;
using Common.ViewModels;
using DAO.GeoVictoria;
using DAO.Rex;
using Helper;
using IBusiness;
using IDAO.GeoVictoria;
using IDAO.Rex;
using System.Linq;
using Common.DTO.GeoVictoria;
using Common.Helper;
using Helpers;
using System.Threading;
using Common.Entity;
using Common.Enum;

namespace Business
{
    public class PlanningBusiness : IPlanningBusiness
    {   
        private readonly int MAX_RECORDS_TO_REQUEST;
        private readonly IUserGeoVictoriaDAO UserGeoVictoriaDAO;
        private readonly IPersonaRexDAO PersonaRexDAO;
        private readonly TurnosRexDAO turnosRexDAO;
        private readonly IUserBusiness UserBusiness;
        private readonly VersionConfiguration versionConfiguration;
        private readonly int BREAK_MINUTES_FOR_DEFAULT;
        private readonly int MINIMUM_STATE;
        private readonly int CHANGE_DAY_STATE;
        private readonly string ID_NOT_WORKING;
        private int QUANTITY_OF_WEEKS; //Este valor corresponde a la cantidad de semanas que se van a planificar en Geovictoria
        private readonly int MAX_QUANTITY_OF_WEEKS; //Este valor corresponde a la cantidad de semanas que se van a planificar en Geovictoria
        private readonly int DEFAULT_QUANTITY_OF_WEEKS;
        private List<string> EXCLUDED_GROUPS; //Este string corresponde a un listado de centros de costo correspondientes a los grupos que no se deben planificar
        private readonly int TIME_INTERVAL_FOR_API;

        public PlanningBusiness(RexExecutionVM rexExecution)
        {
            this.UserGeoVictoriaDAO = new UserGeoVictoriaDAO();
            this.PersonaRexDAO = new PersonaRexDAO();
            this.turnosRexDAO = new TurnosRexDAO();
            this.UserBusiness = new UserBusiness(rexExecution);
            this.versionConfiguration = new VersionConfiguration(rexExecution.RexVersion);
            MAX_RECORDS_TO_REQUEST = 1350;
            BREAK_MINUTES_FOR_DEFAULT = 0;
            MINIMUM_STATE = 0;
            CHANGE_DAY_STATE = 1;
            ID_NOT_WORKING = "EhKM7eLolc9WkWAmVCjZcw";
            MAX_QUANTITY_OF_WEEKS = 53; // Cantidad maxima de semanas que va a permitir ingresar la integración, se deja un valor de 53 semanas que es un valor aproximado al numero de semanas que tiene un año
            DEFAULT_QUANTITY_OF_WEEKS = 1; //
            TIME_INTERVAL_FOR_API = 500;
        }
        public void SynchronizePlanning(RexExecutionVM rexExecutionVM)
        {
            //Inicializar variables 
            QUANTITY_OF_WEEKS = (rexExecutionVM.QuantityOfWeeks > 0 && rexExecutionVM.QuantityOfWeeks <= MAX_QUANTITY_OF_WEEKS)? rexExecutionVM.QuantityOfWeeks : DEFAULT_QUANTITY_OF_WEEKS;
            EXCLUDED_GROUPS = GeneralHelper.ValidateStringList(rexExecutionVM.ExcludedGroups);
            List<Contrato> contractsClient = new List<Contrato>();
            List<Contrato> scheduleClientFiltered = new List<Contrato>();
            List<ShiftListGVContract> shiftGVList = new List<ShiftListGVContract>();
            List<string> usersGeovictoriaIdentifiers = new List<string>();
            List<string> users = new List<string>();
            AttendanceContract attendanceGV = new AttendanceContract();
            List<string> validatedUsersIdentifiers = new List<string>();
            DateTime startTime;
            DateTime endTime;
            List<PlannerContract> shiftGVFiltered = new List<PlannerContract>();
            List<PlannerContract> shiftToSend = new List<PlannerContract>();
            List<ShiftInsertGVContract> newShiftTypesInGVList = new List<ShiftInsertGVContract>();
            List<shiftRexVM> scheduleRex = new List<shiftRexVM>();
            List<TurnosRex> shiftListRex = new List<TurnosRex>();
            List<UserVM> usersGeovictoria = new List<UserVM>();
            //Obtener Turnos de Rex
            foreach (var url in rexExecutionVM.RexCompanyDomains)
            {                
                List<Contrato> contracts = UserBusiness.GetCurrentCompanyContractsByDate(rexExecutionVM, rexExecutionVM.ProcessStartDate, rexExecutionVM.ProcessEndDate);
                contractsClient = contracts.FindAll(a => a.turno != null);
            }
            if (contractsClient != null)
            {
                List<LogEntity> logEntities = new List<LogEntity>();
                startTime = new DateTime (DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day, 0, 0, 0);
                int dayOfWeek = (int)startTime.DayOfWeek;
                int totalDaysInWeeks = (QUANTITY_OF_WEEKS > 1)? ((QUANTITY_OF_WEEKS - 1) * 7) + (7 - dayOfWeek) : totalDaysInWeeks = (7 - dayOfWeek);
                endTime = new DateTime(DateTime.Today.AddDays(totalDaysInWeeks).Year, DateTime.Today.AddDays(totalDaysInWeeks).Month, DateTime.Today.AddDays(totalDaysInWeeks).Day, 23, 59, 59);
                //Ajustar y crear una lista de los identificadores para compararlos con Geovictoria y obtener un listado de los identificadores de Rex que estan en Geovictoria
                usersGeovictoria = UserGeoVictoriaDAO.GetAll(new GeoVictoriaConnectionVM() { TestEnvironment = rexExecutionVM.TestEnvironment, ApiKey = rexExecutionVM.ApiKey, ApiSecret = rexExecutionVM.ApiSecret });
                
                if (EXCLUDED_GROUPS.Count() > 0)
                {
                    usersGeovictoria = usersGeovictoria.FindAll(a => !EXCLUDED_GROUPS.Contains(a.GroupIdentifier));
                }
                validatedUsersIdentifiers = ValidateUsers(contractsClient, usersGeovictoria);
                //Filtrar unicamente los contratos con los identificadores comunes de ambas plataformas
                scheduleClientFiltered = FilterScheduler(contractsClient, validatedUsersIdentifiers);
                //Obtener los tipos de permisos de Geovictoria 
                shiftGVList = ShiftGeovictoriaDAO.GetShifts(new GeoVictoriaConnectionVM() { TestEnvironment = rexExecutionVM.TestEnvironment, ApiKey = rexExecutionVM.ApiKey, ApiSecret = rexExecutionVM.ApiSecret });
                //Obtener el libro de asistencia de Geovictoria
                attendanceGV = GetAttendanceBookWithPagination(validatedUsersIdentifiers, startTime, endTime, rexExecutionVM);
                shiftGVFiltered = FilterAttendanceBookGV(attendanceGV);
                //Método para obtener un listado de tipos de turnos de rex que se deben crear nuevos en Geovictoria
                shiftListRex = turnosRexDAO.GetShifts(rexExecutionVM.RexCompanyDomains, rexExecutionVM.RexToken, versionConfiguration);
                newShiftTypesInGVList = GetNewShiftTypes(shiftGVList, shiftListRex);
                //Método para crear los tipos de turnos nuevos en Geovictoria
               if (newShiftTypesInGVList != null && newShiftTypesInGVList.Count > 0)
                {
                    logEntities.AddRange(ShiftGeovictoriaDAO.InsertShift(newShiftTypesInGVList , rexExecutionVM));
                    shiftGVList = ShiftGeovictoriaDAO.GetShifts(new GeoVictoriaConnectionVM() { TestEnvironment = rexExecutionVM.TestEnvironment, ApiKey = rexExecutionVM.ApiKey, ApiSecret = rexExecutionVM.ApiSecret });
                }
                //Método que procese todos los turnos de Rex comparados con los de Geovictoria y los transforme en un formato para enviarlos 
                scheduleRex = ProcessShifts(shiftGVList , shiftListRex, scheduleClientFiltered);
                //Método para cargar la planificación de los días que no estan en Geovictoria con turno asignado. 
                shiftToSend.AddRange(CompleteSchedule(shiftGVFiltered, scheduleRex, startTime, endTime));
                //Asignar Planificación en Geovictoria

                if (shiftToSend.Any())
                {
                    logEntities.AddRange(SendSchedulers(shiftToSend, rexExecutionVM));
                }
                else
                {
                    LogHelper.Log("Log : Not exist plannings to send, The plannings in GV is updated.");
                }
                new TableStorageHelper().Upsert(logEntities, LogTable.NAME);
            }
            else
            {
                LogHelper.Log("Error : The client do not have planner");
            }
        }

        public List<string> ValidateUsers(List<Contrato> contractsClient, List<UserVM> usersGeovictoria)
        {
            List<string> validatedUsersIdentifiers = new List<string>();
            LogHelper.Log("START: Validate identifiers");

            foreach (Contrato contract in contractsClient)
            {
                contract.empleado = contract.empleado.Replace("-", "").ToUpper().Trim();
                //Crear un listado de los identificadores de Rex
                bool exist = usersGeovictoria.Where(x => x.Enabled == 1).ToList().Any(y => y.Identifier == contract.empleado);
                if (exist)
                {
                    validatedUsersIdentifiers.Add(contract.empleado);
                    LogHelper.Log($"The user with identifier: {contract.empleado} exist in Geovictoria");
                }
                else
                {
                    LogHelper.Log($"The user with identifier: {contract.empleado} do not exist in Geovictoria");
                }
            }
            LogHelper.Log("FINISH: Validate identifiers");
            return validatedUsersIdentifiers.Distinct().ToList();
        }
        public List<Contrato> FilterScheduler(List<Contrato> contractsClient, List<string> validatedUsersIdentifiers)
        {
            LogHelper.Log("START: Filter planning for Rex users");
            List<Contrato> scheduleClientFiltered = new List<Contrato>();
            List<string> identifiersToExclude = new List<string>();
            LogHelper.Log("Users to planning ");
            foreach (Contrato contract in contractsClient)
            {
                foreach (string identifier in validatedUsersIdentifiers)
                {
                    if (contract.empleado == identifier && contract.empleado != null && !identifiersToExclude.Contains(identifier))
                    {
                        bool existOtherContract = scheduleClientFiltered.Any(x => x.empleado == identifier);
                        if (existOtherContract)
                        {
                            List<Contrato> contracstWithSameIdentifier = contractsClient.FindAll(x => x.empleado == identifier);
                            List<DateTime?> datesStartContracts = new List<DateTime?>();
                            foreach (Contrato contractWithSameIdentifier in contracstWithSameIdentifier)
                            {
                                datesStartContracts.Add(DateTimeHelper.StringDateTimeFileToDateTime(contractWithSameIdentifier.fechaInic));
                            }
                            string dateContractSelected = DateTimeHelper.DateTimeToStringRex(datesStartContracts.OrderByDescending(x => x).FirstOrDefault());
                            Contrato contractUnique = contracstWithSameIdentifier.Find(x => x.fechaInic == dateContractSelected);
                            List<Contrato> contractsToRemove = scheduleClientFiltered.FindAll(x => x.empleado == identifier);
                            foreach (Contrato contractToRemove in contractsToRemove)
                            {
                                scheduleClientFiltered.Remove(contractToRemove);
                            }
                            scheduleClientFiltered.Add(contractUnique);
                            identifiersToExclude.Add(identifier);
                        }
                        else
                        { 
                            scheduleClientFiltered.Add(contract); 
                        }
                        LogHelper.Log($"User identifier: {contract.empleado}");
                    }
                }
            }
            LogHelper.Log("FINISH: Filter planning for Rex users");
            return scheduleClientFiltered;
        }
        public AttendanceContract GetAttendanceBookWithPagination(List<string> identifiers, DateTime? startDateTime, DateTime? endDateTime, RexExecutionVM rexExecutionVM)
        {
            AttendanceContract book = new AttendanceContract();
            AttendanceGeoVictoriaDAO attendanceBookGeoVictoriaDAO = new AttendanceGeoVictoriaDAO();

            if (startDateTime == null || endDateTime == null)
                return book;

            DateTime startDate = startDateTime.GetValueOrDefault();
            DateTime endDate = endDateTime.GetValueOrDefault();
            DateTime firstDateToRequest = startDate;
            DateTime lastDateToRequest = endDate.AddDays(1);

            LogHelper.Log($"Get Attendance Book Geovictoria since: {firstDateToRequest} To: {lastDateToRequest}");

            int numberOfUsers = identifiers != null ? identifiers.Count() : 0;

            if (numberOfUsers > 0)
            {
                DateTime startOfMonth = firstDateToRequest;
                while (startOfMonth < lastDateToRequest)
                {
                    int diffDynamic = (lastDateToRequest - startOfMonth).Days;
                    int increment = diffDynamic > 7 ? 8 : diffDynamic;
                    DateTime endOfMonth = startOfMonth.AddDays(diffDynamic).AddSeconds(-1); 
                    TimeSpan diffDates = endOfMonth - startDate;
                    int numberOfDaysOfMonth = diffDates.Days;

                    int numberOfRequestsForThisMonth = (numberOfUsers * numberOfDaysOfMonth) / MAX_RECORDS_TO_REQUEST;
                    if ((numberOfUsers * numberOfDaysOfMonth) % MAX_RECORDS_TO_REQUEST != 0)
                    {
                        numberOfRequestsForThisMonth++;
                    }

                    int usersPerRequest = numberOfUsers / numberOfRequestsForThisMonth;
                    if (numberOfUsers % numberOfRequestsForThisMonth != 0)
                    {
                        usersPerRequest++;
                    }

                    for (int i = 0; i < numberOfRequestsForThisMonth; i++)
                    {
                        List<string> identifiersForRequest = identifiers.Skip(usersPerRequest * i).Take(usersPerRequest).ToList();
                        if (!identifiersForRequest.Any())
                            continue;

                        string idsString = string.Join(",", identifiersForRequest);
                        FilterCustomerContract filter = new FilterCustomerContract
                        {
                            StartDate = DateTimeHelper.DateTimeToStringGeoVictoria(startOfMonth),
                            EndDate = DateTimeHelper.DateTimeToStringGeoVictoria(endOfMonth),
                            UserIds = idsString
                        };

                        AttendanceContract currentBook = attendanceBookGeoVictoriaDAO.GetAttendanceBook(new GeoVictoriaConnectionVM()
                        {
                            ApiKey = rexExecutionVM.ApiKey,
                            ApiSecret = rexExecutionVM.ApiSecret,
                            ApiToken = rexExecutionVM.ApiToken,
                            TestEnvironment = rexExecutionVM.TestEnvironment
                        }, filter);

                        if (book == null)
                        {
                            book = currentBook;
                        }
                        else if (book.Users == null)
                        {
                            book.Users = currentBook.Users;
                        }
                        else
                        {
                            foreach (CalculatedUser user in currentBook.Users)
                            {
                                CalculatedUser userObtainedBefore = book.Users.FirstOrDefault(u => u.Identifier == user.Identifier);
                                if (userObtainedBefore == null)
                                {
                                    book.Users.Add(user);
                                }
                                else
                                {
                                    foreach (TimeIntervalContract plannedDate in user.PlannedInterval)
                                    {
                                        TimeIntervalContract existingPlannedDate = userObtainedBefore.PlannedInterval.FirstOrDefault(pi => pi.Date == plannedDate.Date);
                                        if (existingPlannedDate == null)
                                            userObtainedBefore.PlannedInterval.Add(plannedDate);
                                    }
                                }

                            }
                        }
                    }

                    startOfMonth = startOfMonth.AddMonths(1);
                }
            }

            return book;
        }
        public List<PlannerContract> FilterAttendanceBookGV(AttendanceContract attendanceBook)
        {
            LogHelper.Log("START: FILTER PROCCESS FOR ATTENDANCE BOOK");
            List<PlannerContract> result = new List<PlannerContract>();

            if (attendanceBook.Users != null && attendanceBook.Users.Count > 0)
            {
                foreach (CalculatedUser user in attendanceBook.Users)
                {
                    if (user.PlannedInterval != null && user.PlannedInterval.Count > 0)
                    {
                        PlannerContract plannerGV = new PlannerContract();
                        plannerGV.User = user.Identifier.ToUpper();
                        List<ShiftContract> shiftContracts = new List<ShiftContract>();
                        foreach (TimeIntervalContract planner in user.PlannedInterval)
                        {
                            if (planner.Shifts != null && planner.Shifts.Count > 0)
                            {
                                foreach (Shift shift in planner.Shifts)
                                {

                                    shiftContracts.Add(new ShiftContract
                                    {
                                        ShiftId = shift.Id,
                                        Date = planner.Date
                                    });
                                }
                            }
                            else
                            {
                                LogHelper.Log("The user does not have a shift");
                            }
                        }
                        plannerGV.Shift = shiftContracts;
                        result.Add(plannerGV);
                    }
                    else
                    {
                        LogHelper.Log("The user does not have a planning");
                    }
                }
            }
            else
            {
                LogHelper.Log("Attendance Book without users");
            }
            LogHelper.Log($"Exist {result.Count} days with a shift in GV");
            LogHelper.Log("FINISH: FILTER PROCCESS FOR ATTENDANCE BOOK");
            return result;
        }   
        
        public List<ShiftInsertGVContract> GetNewShiftTypes (List<ShiftListGVContract> shiftGVList, List<TurnosRex> ShiftListRex)
        {
            LogHelper.Log("START: GET LIST WITH NEW SHIFT FOR TO CREATE IN GV ");
            List<ShiftInsertGVContract> shiftToInsertInGV = new List<ShiftInsertGVContract>();
            List<ShiftInsertGVContract> shiftToInsertInGVFiltered = new List<ShiftInsertGVContract>();
            foreach (TurnosRex planner in ShiftListRex)
            {
                List<ShiftListGVContract> shiftListMonday = shiftGVList.Where(shift => (planner.horario_lunes != null && planner.horario_lunes.entrada.Substring(0, 5) == shift.START_HOUR) && (planner.horario_lunes.salida.Substring(0, 5) == shift.END_HOUR) && (planner.duracion_colacion == shift.BREAK_MINUTES)).ToList();
                List<ShiftListGVContract> shiftListTuesday = shiftGVList.Where(shift => (planner.horario_martes != null && planner.horario_martes.entrada.Substring(0, 5) == shift.START_HOUR) && (planner.horario_martes.salida.Substring(0, 5) == shift.END_HOUR) && (planner.duracion_colacion == shift.BREAK_MINUTES)).ToList();
                List<ShiftListGVContract> shiftListWednesday = shiftGVList.Where(shift => (planner.horario_miercoles != null && planner.horario_miercoles.entrada.Substring(0, 5) == shift.START_HOUR) && (planner.horario_miercoles.salida.Substring(0, 5) == shift.END_HOUR) && (planner.duracion_colacion == shift.BREAK_MINUTES)).ToList();
                List<ShiftListGVContract> shiftListThursday = shiftGVList.Where(shift => (planner.horario_jueves != null && planner.horario_jueves.entrada.Substring(0, 5) == shift.START_HOUR) && (planner.horario_jueves.salida.Substring(0, 5) == shift.END_HOUR) && (planner.duracion_colacion == shift.BREAK_MINUTES)).ToList();
                List<ShiftListGVContract> shiftListFriday = shiftGVList.Where(shift => (planner.horario_viernes != null && planner.horario_viernes.entrada.Substring(0, 5) == shift.START_HOUR) && (planner.horario_viernes.salida.Substring(0, 5) == shift.END_HOUR) && (planner.duracion_colacion == shift.BREAK_MINUTES)).ToList();
                List<ShiftListGVContract> shiftListSaturday = shiftGVList.Where(shift => (planner.horario_sabado != null && planner.horario_sabado.entrada.Substring(0, 5) == shift.START_HOUR) && (planner.horario_sabado.salida.Substring(0, 5) == shift.END_HOUR) && (planner.duracion_colacion == shift.BREAK_MINUTES)).ToList();
                List<ShiftListGVContract> shiftListSunday = shiftGVList.Where(shift => (planner.horario_domingo != null && planner.horario_domingo.entrada.Substring(0, 5) == shift.START_HOUR) && (planner.horario_domingo.salida.Substring(0, 5) == shift.END_HOUR) && (planner.duracion_colacion == shift.BREAK_MINUTES)).ToList();
                if (planner.horario_lunes != null && shiftListMonday.Count == 0)
                {
                    shiftToInsertInGV.Add(FillListOfShiftTypes(planner.horario_lunes.entrada, planner.horario_lunes.salida, planner.duracion_colacion));
                }
                if (planner.horario_martes != null && shiftListTuesday.Count == 0)
                {
                    shiftToInsertInGV.Add(FillListOfShiftTypes(planner.horario_martes.entrada, planner.horario_martes.salida, planner.duracion_colacion));
                }
                if (planner.horario_miercoles != null && shiftListWednesday.Count == 0)
                {
                    shiftToInsertInGV.Add(FillListOfShiftTypes(planner.horario_miercoles.entrada, planner.horario_miercoles.salida, planner.duracion_colacion));
                }
                if (planner.horario_jueves != null && shiftListThursday.Count == 0)
                {
                    shiftToInsertInGV.Add(FillListOfShiftTypes(planner.horario_jueves.entrada, planner.horario_jueves.salida, planner.duracion_colacion));
                }
                if (planner.horario_viernes != null && shiftListFriday.Count == 0)
                {
                    shiftToInsertInGV.Add(FillListOfShiftTypes(planner.horario_viernes.entrada, planner.horario_viernes.salida, planner.duracion_colacion));
                }
                if (planner.horario_sabado != null && shiftListSaturday.Count == 0)
                {
                    shiftToInsertInGV.Add(FillListOfShiftTypes(planner.horario_sabado.entrada, planner.horario_sabado.salida, planner.duracion_colacion));
                }
                if (planner.horario_domingo != null && shiftListSunday.Count == 0)
                {
                    shiftToInsertInGV.Add(FillListOfShiftTypes(planner.horario_domingo.entrada, planner.horario_domingo.salida, planner.duracion_colacion));
                }
            }
            LogHelper.Log($"EXIST {shiftToInsertInGV.Count} NEW SHIFTS FOR TO CREATE IN GV ");
            LogHelper.Log("FINISH: GET LIST WITH NEW SHIFT FOR TO CREATE IN GV ");

            var parameterOfList = shiftToInsertInGV.GroupBy(x => new { x.StartHour, x.EndHour, x.BreakMinutes, x.ShiftDay });

            foreach (var u in parameterOfList)
            {
                shiftToInsertInGVFiltered.Add(new ShiftInsertGVContract
                {
                    StartHour = u.Key.StartHour.Substring(0,5),
                    EndHour = u.Key.EndHour.Substring(0, 5),
                    BreakMinutes = u.Key.BreakMinutes,
                    ShiftDay = u.Key.ShiftDay
                });
            }
            return shiftToInsertInGVFiltered;
        }

        public ShiftInsertGVContract FillListOfShiftTypes(string startHour, string endHour, string breakMinutes)
        {
            TimeSpan startHourInTime = new TimeSpan();
            TimeSpan endHourInTime = new TimeSpan();
            int shifDayValue;

            if (startHourInTime != null && endHourInTime != null)
            {
                startHourInTime = DateTimeHelper.StringToTimeSpanNoNull(startHour);
                endHourInTime = DateTimeHelper.StringToTimeSpanNoNull(endHour);
                shifDayValue = TimeSpan.Compare(startHourInTime, endHourInTime);
            }
            else
            {
                shifDayValue = MINIMUM_STATE;
            }

            return new ShiftInsertGVContract
            {
                StartHour = startHour,
                EndHour = endHour,
                BreakMinutes = Int32.TryParse(breakMinutes, out int breakMinutesRex) ? breakMinutesRex : BREAK_MINUTES_FOR_DEFAULT,
                ShiftDay = shifDayValue == CHANGE_DAY_STATE ? "" : "fin"
            };
        }

        public List<shiftRexVM> ProcessShifts(List<ShiftListGVContract> shiftGVList, List<TurnosRex> shiftListRex, List<Contrato> contracts)
        {
            LogHelper.Log("START: PROCESS SHIFT OF REX AND GV");
            List<shiftRexVM> shiftList = new List<shiftRexVM>();
            List<shiftRexVM> shiftListWithId = new List<shiftRexVM>();
            foreach (TurnosRex planner in shiftListRex)
            {
                List<ShiftListGVContract> shiftListMonday = shiftGVList.Where(shift => (planner.horario_lunes != null && planner.horario_lunes.entrada.Substring(0,5) == shift.START_HOUR) && (planner.horario_lunes.salida.Substring(0,5) == shift.END_HOUR) && (planner.duracion_colacion == shift.BREAK_MINUTES)).ToList();
                List<ShiftListGVContract> shiftListTuesday = shiftGVList.Where(shift => (planner.horario_martes != null && planner.horario_martes.entrada.Substring(0,5) == shift.START_HOUR) && (planner.horario_martes.salida.Substring(0,5) == shift.END_HOUR) && (planner.duracion_colacion == shift.BREAK_MINUTES)).ToList();
                List<ShiftListGVContract> shiftListWednesday = shiftGVList.Where(shift => (planner.horario_miercoles != null && planner.horario_miercoles.entrada.Substring(0,5) == shift.START_HOUR) && (planner.horario_miercoles.salida.Substring(0,5) == shift.END_HOUR) && (planner.duracion_colacion == shift.BREAK_MINUTES)).ToList();
                List<ShiftListGVContract> shiftListThursday = shiftGVList.Where(shift => (planner.horario_jueves != null && planner.horario_jueves.entrada.Substring(0,5) == shift.START_HOUR) && (planner.horario_jueves.salida.Substring(0,5) == shift.END_HOUR) && (planner.duracion_colacion == shift.BREAK_MINUTES)).ToList();
                List<ShiftListGVContract> shiftListFriday = shiftGVList.Where(shift => (planner.horario_viernes != null && planner.horario_viernes.entrada.Substring(0,5) == shift.START_HOUR) && (planner.horario_viernes.salida.Substring(0,5) == shift.END_HOUR) && (planner.duracion_colacion == shift.BREAK_MINUTES)).ToList();
                List<ShiftListGVContract> shiftListSaturday = shiftGVList.Where(shift => (planner.horario_sabado != null && planner.horario_sabado.entrada.Substring(0,5) == shift.START_HOUR) && (planner.horario_sabado.salida.Substring(0,5) == shift.END_HOUR) && (planner.duracion_colacion == shift.BREAK_MINUTES)).ToList();
                List<ShiftListGVContract> shiftListSunday = shiftGVList.Where(shift => (planner.horario_domingo != null && planner.horario_domingo.entrada.Substring(0,5) == shift.START_HOUR) && (planner.horario_domingo.salida.Substring(0,5) == shift.END_HOUR) && (planner.duracion_colacion == shift.BREAK_MINUTES)).ToList();

                string tokenshiftMonday = shiftListMonday.Count > 0 ? shiftListMonday.FirstOrDefault().ID_SHIFT : ID_NOT_WORKING;
                string tokenshiftTuesday = shiftListTuesday.Count > 0 ? shiftListTuesday.FirstOrDefault().ID_SHIFT : ID_NOT_WORKING;
                string tokenshiftWednesday = shiftListWednesday.Count > 0 ? shiftListWednesday.FirstOrDefault().ID_SHIFT : ID_NOT_WORKING;
                string tokenshiftThursday = shiftListThursday.Count > 0 ? shiftListThursday.FirstOrDefault().ID_SHIFT : ID_NOT_WORKING;
                string tokenshiftFriday = shiftListFriday.Count > 0 ? shiftListFriday.FirstOrDefault().ID_SHIFT : ID_NOT_WORKING;
                string tokenshiftSaturday = shiftListSaturday.Count > 0 ? shiftListSaturday.FirstOrDefault().ID_SHIFT : ID_NOT_WORKING;
                string tokenshiftSunday = shiftListSunday.Count > 0 ? shiftListSunday.FirstOrDefault().ID_SHIFT : ID_NOT_WORKING;

                shiftList.Add(new shiftRexVM
                {
                    idUser = string.Empty,
                    idShiftRex = planner.id,
                    shifts = new Dictionary<int, string>() {
                        {0 , tokenshiftSunday},
                        {1 , tokenshiftMonday},
                        {2 , tokenshiftTuesday},
                        {3 , tokenshiftWednesday},
                        {4 , tokenshiftThursday}, 
                        {5 , tokenshiftFriday}, 
                        {6 , tokenshiftSaturday}
                    }
                });
            }

            foreach (Contrato contract in contracts)
            {
                foreach (shiftRexVM shift in shiftList)
                {
                    if (contract.turno == shift.idShiftRex)
                    {
                        shiftListWithId.Add(new shiftRexVM
                        {
                            idUser = contract.empleado,
                            idShiftRex = shift.idShiftRex,
                            shifts = shift.shifts
                        });
                    }    
                }
            }
            LogHelper.Log($"EXIST {shiftListWithId} users with planning");
            LogHelper.Log("FINISH: PROCESS SHIFT OF REX AND GV");
            return shiftListWithId;
        }
        public List<PlannerContract> CompleteSchedule(List<PlannerContract> shiftInGV, List<shiftRexVM> scheduleRex, DateTime startTime , DateTime endTime)
        {
            LogHelper.Log("START: DAYS FOR PLANNING");
            List<PlannerContract> response = new List<PlannerContract>();
            TimeSpan diffDays = endTime - startTime;
            int numberOfDays = diffDays.Days;

            for (int i = 0; i < (numberOfDays + 1); i++)
            {
                DateTime dateCounter = startTime.AddDays(i);
                string startTimeStr = DateTimeHelper.DateTimeToStringGeoVictoria(dateCounter);
                int dayOfWeek = (int)dateCounter.DayOfWeek;
                foreach (shiftRexVM shift in scheduleRex)
                {
                    bool exist = shiftInGV.Any(x => x.User == shift.idUser && x.Shift.Any(y => y.Date == startTimeStr && y.ShiftId == shift.shifts[dayOfWeek]));
                    
                    if (!exist)
                    {
                        if (response.Any(x => x.User == shift.idUser))
                        {
                            PlannerContract user = response.Find(x => x.User == shift.idUser);
                            user.Shift.Add(new ShiftContract
                            {
                                ShiftId = shift.shifts[dayOfWeek],
                                Date = startTimeStr
                            });

                        }
                        else 
                        {
                            List<ShiftContract> shiftContracts = new List<ShiftContract>();
                            shiftContracts.Add(new ShiftContract
                            {
                                ShiftId = shift.shifts[dayOfWeek],
                                Date = startTimeStr
                            });
                            response.Add(new PlannerContract { User = shift.idUser, Shift = shiftContracts });
                        }
                    }
                }
                
            }
            LogHelper.Log($"{response.Count} USERS FOR PLANNING");
            LogHelper.Log("FINISH: DAYS FOR PLANNING");
            return response;
        }
        public List<LogEntity> SendSchedulers(List<PlannerContract> shiftToSend, RexExecutionVM rexExecutionVM)
        {
            List<LogEntity> logEntities = new List<LogEntity>();
            LogHelper.Log("START: Operation --- Send Shifts");
            int numberOfUser = shiftToSend.Count;
            int numberOfDays = shiftToSend.Select(x => x.Shift.Count()).Max(y => y);
            //Con esta ecuación se calcula el total de registro a enviar a Rex
            int totalRegisters = numberOfDays * numberOfUser;
            //Asegurar que el valor de registros sea un número par
            totalRegisters = (totalRegisters % 2 != 0) ? totalRegisters + 1: totalRegisters;
            //Este condicional permite asegurar el envío de los registros sin bloquear a la API de Geovictoria por un gran volumen de consultas
            int postRegisters = (totalRegisters >= MAX_RECORDS_TO_REQUEST) ? MAX_RECORDS_TO_REQUEST / numberOfDays : numberOfUser;
            //Obtenemos el número de paginas para enviar los turnos 
            int pages = numberOfUser / postRegisters;
            //Si el número de paginas no es un entero adicionamos una unidad para asegurar que se envien todos los permisos
            if (numberOfUser % postRegisters != 0) {pages++;}
            LogHelper.Log($"The creation of shifts is divided in {pages} pages");
            for (int i = 0; i < pages; i++)
            {
                LogHelper.Log("Insert " + (i + 1) + "/" + pages);
                int iteration = postRegisters * i;
                int diff = numberOfUser - iteration;
                int registerToTake = (diff < postRegisters) ? diff : postRegisters;
                List<PlannerContract> auxSchedules = shiftToSend.Skip(iteration).Take(registerToTake).ToList();
                //PlannerGeovictoriaDAO.InsertPlanner(auxSchedules, new GeoVictoriaConnectionVM() { TestEnvironment = rexExecutionVM.TestEnvironment, ApiKey = rexExecutionVM.ApiKey, ApiSecret = rexExecutionVM.ApiSecret, ApiToken = rexExecutionVM.ApiToken });
                logEntities.AddRange(PlannerGeovictoriaDAO.InsertPlanner(auxSchedules, rexExecutionVM));
                Thread.Sleep(TIME_INTERVAL_FOR_API);
            }
            LogHelper.Log("FINISH: Operation --- Send Shifts");
            return logEntities;
        }
    }
}
