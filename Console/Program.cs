using Business;
using Helper;
using IBusiness;
using System.Collections.Generic;
using System;
using Common.ViewModels;
using Common.Enum;
using System.Configuration;
using System.Linq;
using Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Console
{
    class Program
    {
        static void Main(string[] args)
        { 


            string a1 = "ApiKey";
            string a2 = "'8e70e7'";
            string b1 = "ApiSecret";
            string b2 = "'a932acd5'";
            string c1 = "RexCompanyDomains";
            string c2 = "['berrieschile']";

            JObject j1 = new JObject();
            j1.Add(a1, JToken.Parse(a2));
            j1.Add(c1, JToken.Parse(c2));

            var executionVM = new { ApiKey = "8e70e7", ApiSecret = "a932acd5", RexCompanyDomains = new List<string>() { "berrieschile" } };
            var j2 = JsonConvert.SerializeObject(executionVM);

            Dictionary<string, string> ps = new Dictionary<string, string>()
            {
                {a1, a2},
                {b1, b2},
                {c1, c2}
            };

            var json = JsonConvert.SerializeObject(ps);
            System.Console.WriteLine(j1);
            System.Console.WriteLine(j2);

            #region old
            /*
            LogHelper.Log("---INICIO DE LA INTEGRACION---");
            try
            {
                List<string> ops = new List<string>();
                if (args != null && args.Length > 0)
                {
                    foreach (string s in args)
                    {
                        switch (s.ToLower())
                        {
                            case "syncusuarios": if (!ops.Contains(Operation.USERS)) ops.Add(Operation.USERS); break;
                            case "syncpermisos": if (!ops.Contains(Operation.TIMEOFFS)) ops.Add(Operation.TIMEOFFS); break;
                            case "syncasistencia": if (!ops.Contains(Operation.ATTENDANCE_CONCEPTS)) ops.Add(Operation.ATTENDANCE_CONCEPTS); break;
                            case "syncinasistencia": if (!ops.Contains(Operation.ABSENCES)) ops.Add(Operation.ABSENCES); break;
                            default: continue;
                        }
                    }
                }
                else
                {
                    ops.Add(Operation.USERS);
                    ops.Add(Operation.TIMEOFFS);
                    //ops.Add(Operation.ATTENDANCE_CONCEPTS);
                    //ops.Add(Operation.ABSENCES);
                }

                foreach (string op in ops)
                {
                    ExcecuteOperations(op);
                }
                LogHelper.Log("---FIN DE LA INTEGRACION---");
            }
            catch (Exception e)
            {
                LogHelper.Log("ERROR: " + e.Message);
                LogHelper.Log(e.StackTrace);
            }
            */
            #endregion
        }
        private static void ExcecuteOperations(string op)
        {
            RexExecutionVM rexExecutionVM = new RexExecutionVM()
            {
                ApiKey = ConfigurationHelper.Value("consumerkey"),
                ApiSecret = ConfigurationHelper.Value("consumersecret"),
                //CompanyId = ConfigurationHelper.Value(""),
                CompanyName = ConfigurationHelper.Value("companyName"),
                DaysSinceUsersUpdated = int.Parse(ConfigurationHelper.Value("daysSinceUsersUpdated")),
                RexCompanyDomains = ConfigurationHelper.Value("UrlRexService").Split(';').ToList(),
                ProcessStartDate = DateTimeHelper.StringDateTimeFileToDateTime(ConfigurationHelper.Value("processStartDate")).Value,
                ProcessEndDate = DateTimeHelper.StringDateTimeFileToDateTime(ConfigurationHelper.Value("processEndDate")).Value,
                //TimeOffsStartDate = DateTimeHelper.StringDateTimeFileToDateTime(ConfigurationHelper.Value("timeOffsStartDate")),
                //TimeOffsEndDate = DateTimeHelper.StringDateTimeFileToDateTime(ConfigurationHelper.Value("timeOffsEndDate")),
                MatchGVGroupsWith = ConfigurationHelper.Value("userGroupOrigin"),
                CreateGroups = Convert.ToBoolean(ConfigurationHelper.Value("createGroups")),
                DisableGroupIfNotFound = Convert.ToBoolean(ConfigurationHelper.Value("disableGroupIfNotFound")),
                RexCompanyCodes = ConfigurationHelper.Value("rexCompanyCode").Split(';').ToList(),
                DisableUserIfContractEnds = Convert.ToBoolean(ConfigurationHelper.Value("disableUserIfContractEnds")),
                DisableUserIfContractIsZero = Convert.ToBoolean(ConfigurationHelper.Value("disableUserIfContractIsZero")),
                DisableUserIfSituacionF = Convert.ToBoolean(ConfigurationHelper.Value("disableUserIfSituacionF")),
                DisableUserIfSituacionS = Convert.ToBoolean(ConfigurationHelper.Value("disableUserIfSituacionS")),
                DisableUserIfModalidadContratoS = Convert.ToBoolean(ConfigurationHelper.Value("disableUserIfModalidadContratoS")),
                CreateAllPositions = Convert.ToBoolean(ConfigurationHelper.Value("createAllPositions")),
                ExcludeFromDisable = ConfigurationHelper.Value("excludeCustum3").Split(';').ToList(),
                UtilizaAsistencia = Convert.ToBoolean(ConfigurationHelper.Value("utilizaAsistencia")),
                CausalCode = ConfigurationHelper.Value("causalCode"),
                RexVersion = int.Parse(ConfigurationHelper.Value("RexVersion")),
                HasIDSeparator = Convert.ToBoolean(ConfigurationHelper.Value("HasIDSeparator")),

            };

            //rexExecutionVM.JsonConcepts.Add("{\"Name\":\"Hheedomingo\",\"Type\":4,\"AllCommonDays\":false,\"CommonDays\":[0],\"AllHolidays\":false,\"Holidays\":[0],\"AllowTimeOffs\":2,\"AllowOvertime\":0,\"AllowOvertimeType\":0,\"OvertimeValues\":[]}");
            //rexExecutionVM.JsonConcepts.Add("{\"Name\":\"horasEx50\",\"Type\":1,\"AllCommonDays\":true,\"CommonDays\":[],\"AllHolidays\":true,\"Holidays\":[],\"AllowTimeOffs\":2,\"AllowOvertime\":0,\"AllowOvertimeType\":0,\"OvertimeValues\":[50]}");
            //rexExecutionVM.JsonConcepts.Add("{\"Name\":\"Hheesabado\",\"Type\":4,\"AllCommonDays\":false,\"CommonDays\":[6],\"AllHolidays\":false,\"Holidays\":[6],\"AllowTimeOffs\":2,\"AllowOvertime\":0,\"AllowOvertimeType\":0,\"OvertimeValues\":[]}");
            //rexExecutionVM.JsonConcepts.Add("{\"Name\":\"faltasDias\",\"Type\":0,\"AllCommonDays\":true,\"CommonDays\":[],\"AllHolidays\":true,\"Holidays\":[],\"AllowTimeOffs\":2,\"AllowOvertime\":0,\"AllowOvertimeType\":0,\"OvertimeValues\":[]}");

            switch (op)
            {
                case Operation.USERS:
                    {
                        //Users' synchronization
                        //LogHelper.Log("START: MAIN --- Users synchronization");
                        IUserBusiness userBusiness = new UserBusiness(rexExecutionVM);
                        userBusiness.SynchronizeUsers(rexExecutionVM);
                        //LogHelper.Log("FINISH: MAIN --- Users synchronization");
                        break;
                    }
                case Operation.TIMEOFFS:
                    {

                        //TimeOffs
                        //LogHelper.Log("START: MAIN --- Synchronize All Ausentism");
                        ITimeOffBusiness timeOffBusiness = new TimeOffBusiness(rexExecutionVM);
                        timeOffBusiness.SynchronizeAllTimesOff(rexExecutionVM);
                        //LogHelper.Log("FINISH: MAIN --- Synchronize All Ausentism");

                        break;
                    }
                case Operation.ATTENDANCE_CONCEPTS:
                    {
                        //consolidate Atrasos y HHEE' export
                        //LogHelper.Log("START: MAIN --- AttendanceConcept report export");
                        IAttendanceBusiness attendanceBusiness = new AttendanceBusiness(rexExecutionVM);
                        attendanceBusiness.SynchronizeAttendanceConcepts(rexExecutionVM);
                        //LogHelper.Log("FINISH: MAIN --- AttendanceConcept report export");
                        break;
                    }
                case Operation.ABSENCES:
                    {
                        //Inasistencias
                        //LogHelper.Log("START: MAIN --- Absences report export");
                        IAbsencesBusiness absencesBusiness = new AbsencesBusiness(rexExecutionVM);
                        absencesBusiness.SynchronizeAbsences(rexExecutionVM);
                        //LogHelper.Log("FINISH: MAIN --- Absences report export");
                        break;
                    }

                default: break;
            }
        }
    }
}
