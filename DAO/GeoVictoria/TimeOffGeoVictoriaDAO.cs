using Common.DTO.GeoVictoria;
using Common.ViewModels;
using Helper;
using Helpers;
using IDAO.GeoVictoria;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace DAO.GeoVictoria
{
    public class TimeOffGeoVictoriaDAO : ITimeOffGeoVictoriaDAO
    {
        private const int DEFAULT_DEGREE_OF_PARALLELLISM = 10;

        private readonly int MAX_PROCESS;
        private readonly bool EXECUTE = false;
        
        public TimeOffGeoVictoriaDAO()
        {
            bool success = int.TryParse(ConfigurationHelper.Value("maxParallel"), out int maxProcess);
            MAX_PROCESS = success ? maxProcess : DEFAULT_DEGREE_OF_PARALLELLISM;

            bool.TryParse(ConfigurationHelper.Value("Execute"), out EXECUTE);
        }

        public List<TimeOffTypeVM> GetAllTypes(GeoVictoriaConnectionVM gvConnection)
        {
            try
            {
                IRestClient client = ConnectionHelper.ConnectGeoVictoria(gvConnection);
                IRestRequest request = new RestRequest("/Permit/List", Method.POST);
                request.RequestFormat = DataFormat.Json;
                IRestResponse response = client.Execute(request);
                var content = response.Content;
                List<PermissionTypeContract> permitTypes = JsonConvert.DeserializeObject<List<PermissionTypeContract>>(content);
                List<TimeOffTypeVM> timesOffTypeVM = ParsePermissionTypesContractToTimesOffTypeVM(permitTypes);
                return timesOffTypeVM;
            }
            catch (Exception e)
            {
                LogHelper.Log("ERROR: " + e.Message);
                throw;
            }
        }
        
        public List<TimeOffVM> GetAll(GeoVictoriaConnectionVM gvConnection, FilterContractRex filter)
        {
            try 
            {
                FilterContract FilterGV = new FilterContract
                {
                    from = DateTimeHelper.DateTimeToStringGeoVictoria(filter.StartDateRequest),
                    to = DateTimeHelper.DateTimeToStringGeoVictoria(filter.EndDateRequest)
                };
                IRestClient client = ConnectionHelper.ConnectGeoVictoria(gvConnection);
                IRestRequest request = new RestRequest("/Permit/getPermissions", Method.POST);
                request.RequestFormat = DataFormat.Json;
                request.AddBody(FilterGV);
                IRestResponse response = client.Execute(request);
                var content = response.Content;
                List<PermissionContract> permissions = JsonConvert.DeserializeObject<List<PermissionContract>>(content);
                List<TimeOffVM> timesOffVM = ParsePermissionsContractToTimesOffVM(permissions);
                return timesOffVM;
            }
            catch (Exception e)
            {
                LogHelper.Log("ERROR: " + e.Message);
                throw;
            }
        }
        
        public (bool success, string message) Add(TimeOffVM timeOff, GeoVictoriaConnectionVM gvConnection)
        {
            PermissionContract permission = ParseTimesOffVMToPermissionsContract(new List<TimeOffVM> { timeOff }).First();
            string message = $"Nuevo Permiso desde Rex+: {permission.Identifier} - {permission.PERMISSION_DESCRIPTION} - starting at: {permission.PERMISSION_START} -  finishing at: {permission.PERMISSION_END}";
            LogHelper.Log("Add: User " + permission.Identifier + " " + permission.Email + " start: " + permission.PERMISSION_START + " end: " + permission.PERMISSION_END + " description: " + permission.PERMISSION_DESCRIPTION);
            bool status = true;
            if (EXECUTE)
            {
                IRestClient client = ConnectionHelper.ConnectGeoVictoria(gvConnection);
                IRestRequest request = new RestRequest("/Permit/Add", Method.POST);
                request.RequestFormat = DataFormat.Json;
                request.AddBody(permission);
                IRestResponse response = client.Execute(request);

                if (response.StatusCode != HttpStatusCode.OK)
                {
                    LogHelper.Log("Error ADDING permission: " + permission.ID_PERMISSION_TYPE + "to user: " + permission.Identifier +
                        "starting at: " + permission.PERMISSION_START + " finishing at: " + permission.PERMISSION_END);
                    LogHelper.Log(response.Content.Length > 100 ? response.Content.Substring(0, 100) : response.Content);
                    status = false;
                    message = $"{(response.Content.Length > 100 ? response.Content.Substring(0, 100) : response.Content)} - {permission.Identifier} - {permission.PERMISSION_DESCRIPTION} - starting at: {permission.PERMISSION_START} -  finishing at: {permission.PERMISSION_END}";
                }

            }

            return (status, message);
        }
        
        public (bool success, string message) Delete(TimeOffVM timeOff, GeoVictoriaConnectionVM gvConnection)
        {
            PermissionContract permission = ParseTimesOffVMToPermissionsContract(new List<TimeOffVM> { timeOff }).First();
            LogHelper.Log("Delete: User " + permission.Identifier + " " + permission.Email + " start: " + permission.PERMISSION_START + " end: " + permission.PERMISSION_END + " description: " + permission.PERMISSION_DESCRIPTION);
            string message = $"Nuevo Permiso desde Rex+: {permission.Identifier} - {permission.PERMISSION_DESCRIPTION} - from: {permission.PERMISSION_START} -  to: {permission.PERMISSION_END}";
            bool status = true;
            if (EXECUTE)
            {
                IRestClient client = ConnectionHelper.ConnectGeoVictoria(gvConnection);
                IRestRequest request = new RestRequest("/Permit/Delete", Method.POST);
                request.RequestFormat = DataFormat.Json;
                request.AddBody(permission);
                IRestResponse response = client.Execute(request);

                if (response.StatusCode != HttpStatusCode.OK)
                {
                    LogHelper.Log("Error DELETING permission: " + permission.ID_PERMISSION_TYPE + "to user: " + permission.Identifier +
                        "starting at: " + permission.PERMISSION_START + " finishing at: " + permission.PERMISSION_END + " Error: " + response.Content);
                    status = false;
                    message = $"{(response.Content.Length > 100 ? response.Content.Substring(0, 100) : response.Content)} - {permission.Identifier} - {permission.PERMISSION_DESCRIPTION} - from: {permission.PERMISSION_START} -  to: {permission.PERMISSION_END}";

                }
            }

            return (status, message);
        }

        public bool AddType(PermissionTypeContract timeOffType, GeoVictoriaConnectionVM gvConnection)
        {
            LogHelper.Log("Add: Type: " + timeOffType.DESCRIPCION_TIPO_PERMISO);
            bool status = true;
            if (EXECUTE)
            {
                IRestClient client = ConnectionHelper.ConnectGeoVictoria(gvConnection);
                IRestRequest request = new RestRequest("/Permit/addType", Method.POST);
                request.RequestFormat = DataFormat.Json;
                request.AddBody(timeOffType);
                IRestResponse response = client.Execute(request);

                if (response.StatusCode != HttpStatusCode.OK)
                {
                    LogHelper.Log("Error Adding TimeOff Type: " + timeOffType.DESCRIPCION_TIPO_PERMISO);
                    LogHelper.Log(response.Content.Length > 100 ? response.Content.Substring(0, 100) : response.Content);
                    status = false;
                }
            }

            return status;
        }

        private List<TimeOffTypeVM> ParsePermissionTypesContractToTimesOffTypeVM(List<PermissionTypeContract> permissionTypesContract)
        {
            ConcurrentBag<TimeOffTypeVM> timesOffType = new ConcurrentBag<TimeOffTypeVM>();

            if (permissionTypesContract != null)
            {
                permissionTypesContract.AsParallel().WithDegreeOfParallelism(MAX_PROCESS)
                    .ForAll(permissionTypeContract =>
                    {
                        TimeOffTypeVM timeOffType = new TimeOffTypeVM
                        {
                            TimeOffTypeId = permissionTypeContract.ID_TIPO_PERMISO,
                            TimeOffDescription = permissionTypeContract.DESCRIPCION_TIPO_PERMISO,
                            permisoParcial = permissionTypeContract.PERMISO_PARCIAL
                        };

                        timesOffType.Add(timeOffType);
                    }
                ); 
            }

            return timesOffType.ToList();
        }
        

        private List<TimeOffVM> ParsePermissionsContractToTimesOffVM(List<PermissionContract> permissionsContract)
        {
            ConcurrentBag<TimeOffVM> timesOff = new ConcurrentBag<TimeOffVM>();

            if (permissionsContract != null)
            {
                permissionsContract.AsParallel().WithDegreeOfParallelism(MAX_PROCESS)
                    .ForAll(permissionContract =>
                    {
                        TimeOffVM timeOff = new TimeOffVM()
                        {
                            TimeOffTypeId = permissionContract.ID_PERMISSION_TYPE,
                            TimeOffId = permissionContract.ID_PERMISSION,
                            Identifier = permissionContract.Identifier,
                            Email = (permissionContract.Email != null) ? (GeneralHelper.ValidateEmail(permissionContract.Email.Trim()) ? permissionContract.Email.Trim() : string.Empty) : permissionContract.Email,
                            TimeOffDescription = permissionContract.PERMISSION_DESCRIPTION,
                            CreatedBy = permissionContract.CREATED_BY,
                            StartDateTime = DateTimeHelper.StringGeoVictoriaToDateTime(permissionContract.PERMISSION_START),
                            EndDateTime = DateTimeHelper.StringGeoVictoriaToDateTime(permissionContract.PERMISSION_END),
                            StartTime = DateTimeHelper.StringToTimeSpan(permissionContract.PERMISSION_START_HOUR),
                            EndTime = DateTimeHelper.StringToTimeSpan(permissionContract.PERMISSION_END_HOUR),
                            Hours = permissionContract.PERMISSION_HOURS
                        };

                        timesOff.Add(timeOff);
                    }
                ); 
            }

            return timesOff.ToList();
        }

        private List<PermissionContract> ParseTimesOffVMToPermissionsContract(List<TimeOffVM> timesOff)
        {
            ConcurrentBag<PermissionContract> permissionsContract = new ConcurrentBag<PermissionContract>();

            if (timesOff != null)
            {
                timesOff.AsParallel().WithDegreeOfParallelism(MAX_PROCESS)
                    .ForAll(timeOff =>
                    {
                        PermissionContract permissionContract = new PermissionContract()
                        {
                            ID_PERMISSION_TYPE = timeOff.TimeOffTypeId.ToString(),
                            Identifier = timeOff.Identifier,
                            Email = timeOff.Email,
                            PERMISSION_DESCRIPTION = timeOff.TimeOffDescription,
                            CREATED_BY = timeOff.CreatedBy,
                            PERMISSION_START = DateTimeHelper.DateTimeToStringGeoVictoria(timeOff.StartDateTime),
                            PERMISSION_END = DateTimeHelper.DateTimeToStringGeoVictoria(timeOff.EndDateTime),
                            PERMISSION_START_HOUR = DateTimeHelper.TimeSpanToString(timeOff.StartTime),
                            PERMISSION_END_HOUR = DateTimeHelper.TimeSpanToString(timeOff.EndTime),
                            PERMISSION_HOURS = timeOff.Hours
                        };

                        permissionsContract.Add(permissionContract);
                    }
                ); 
            }

            return permissionsContract.ToList();
        }
    }
}
