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
    public class UserGeoVictoriaDAO : IUserGeoVictoriaDAO
    {
        private const int DEFAULT_DEGREE_OF_PARALLELLISM = 10;

        private readonly int MAX_PROCESS;
        private readonly bool EXECUTE = false;

        public UserGeoVictoriaDAO()
        {
            bool success = int.TryParse(ConfigurationHelper.Value("maxParallel"), out int maxProcess);
            MAX_PROCESS = success ? maxProcess : DEFAULT_DEGREE_OF_PARALLELLISM;

            bool.TryParse(ConfigurationHelper.Value("Execute"), out EXECUTE);
        }
        
        public List<UserVM> GetAll(GeoVictoriaConnectionVM gvConnection)
        {
            try
            {
                IRestClient client = ConnectionHelper.ConnectGeoVictoria(gvConnection);
                IRestRequest request = new RestRequest("/User/List", Method.POST);
                request.RequestFormat = DataFormat.Json;
                IRestResponse response = client.Execute(request);
                var content = response.Content;
                List<UserContract> usersContract = JsonConvert.DeserializeObject<List<UserContract>>(content);
                List<UserVM> users = ParseUsersContractToUsersVM(usersContract);
                LogHelper.Log("API GeoVictoria --- Get Usuarios: " + users.Count + " registros");
                return users;
            }
            catch (Exception e)
            {
                LogHelper.Log("ERROR: " + e.Message);
                throw;
            }            
        }
        
        public (bool success, string message) Add(UserVM newUser, GeoVictoriaConnectionVM gvConnection)
        {
            UserContract user = ParseUsersVMToUsersContract(new List<UserVM> { newUser }).First();
            bool success = true;
            string message = $"Nuevo empleado desde Rex+: {newUser.Name} {newUser.LastName} {newUser.Email}";
            if (EXECUTE)
            {
                IRestClient client = ConnectionHelper.ConnectGeoVictoria(gvConnection);
                IRestRequest request = new RestRequest("/User/Add", Method.POST);
                request.RequestFormat = DataFormat.Json;
                request.AddBody(user);
                IRestResponse response = client.Execute(request);

                if (response.StatusCode != HttpStatusCode.OK)
                {
                    success = false;
                    message = $"{MessageHelper.UserError(response.Content)} - {newUser.Name} {newUser.LastName} {newUser.Email}";
                }
            }
            
            return (success, message);
        }
                
        public (bool success, string message) Update(UserVM updateUser, GeoVictoriaConnectionVM gvConnection)
        {
            UserContract user = ParseUsersVMToUsersContract(new List<UserVM> { updateUser }).First();
            bool success = true;
            string message = updateUser.UpdateCause;
            if (EXECUTE)
            {
                IRestClient client = ConnectionHelper.ConnectGeoVictoria(gvConnection);
                IRestRequest request = new RestRequest("/User/Edit", Method.POST);
                request.RequestFormat = DataFormat.Json;
                request.AddBody(user);
                IRestResponse response = client.Execute(request);

                if (response.StatusCode != HttpStatusCode.OK)
                {
                    success = false;
                    message = $"{MessageHelper.UserError(response.Content)} - {updateUser.Name} {updateUser.LastName} {updateUser.Email}";
                }
            }
            
            return (success, message);
        }
                
        public (bool success, string message) MoveOfGroup(UserVM updateUser, GeoVictoriaConnectionVM gvConnection)
        {
            UserContract user = ParseUsersVMToUsersContract(new List<UserVM> { updateUser }).First();
            bool success = true;
            string message = $"Se mueve usuario de {updateUser.OldGroupDescription} a {updateUser.GroupDescription}";
            if (EXECUTE)
            {
                IRestClient client = ConnectionHelper.ConnectGeoVictoria(gvConnection);
                IRestRequest request = new RestRequest("/User/moveGeneral", Method.POST);
                request.RequestFormat = DataFormat.Json;
                request.AddBody(user);
                IRestResponse response = client.Execute(request);

                if (response.StatusCode != HttpStatusCode.OK)
                {
                    success = false;
                    message = $"{MessageHelper.UserError(response.Content)} - {updateUser.Name} {updateUser.LastName} {updateUser.Email}";
                }
            }

            return (success, message);
        }
                
        public (bool success, string message) Enable(UserVM updateUser, GeoVictoriaConnectionVM gvConnection)
        {
            UserContract user = ParseUsersVMToUsersContract(new List<UserVM> { updateUser }).First();
            bool success = true;
            string message = "Usuario desactivado en GeoVictoria se encuentra activo en Rex+";
            if (EXECUTE)
            {
                IRestClient client = ConnectionHelper.ConnectGeoVictoria(gvConnection);
                IRestRequest request = new RestRequest("/User/Enable", Method.POST);
                request.RequestFormat = DataFormat.Json;
                request.AddBody(user);
                IRestResponse response = client.Execute(request);

                if (response.StatusCode != HttpStatusCode.OK)
                {
                    success = false;
                    message = $"{MessageHelper.UserError(response.Content)} - {updateUser.Name} {updateUser.LastName} {updateUser.Email}";
                }
            }

            return (success, message);
        }
                
        public (bool success, string message) Disable(UserVM updateUser, GeoVictoriaConnectionVM gvConnection)
        {
            UserContract user = ParseUsersVMToUsersContract(new List<UserVM> { updateUser }).First();
            bool success = true;
            string message = updateUser.DisableCause;
            if (EXECUTE)
            {
                IRestClient client = ConnectionHelper.ConnectGeoVictoria(gvConnection);
                IRestRequest request = new RestRequest("/User/Disable", Method.POST);
                request.RequestFormat = DataFormat.Json;
                request.AddBody(user);
                IRestResponse response = client.Execute(request);

                if (response.StatusCode != HttpStatusCode.OK)
                {
                    success = false;
                    message = $"{MessageHelper.UserError(response.Content)} - {updateUser.Name} {updateUser.LastName} {updateUser.Email}";
                }
            }

            return (success, message);
        }

        public bool EditProfile(ProfileUserContract profile, GeoVictoriaConnectionVM gvConnection)
        {
            bool success = true;
            IRestClient client = ConnectionHelper.ConnectGeoVictoria(gvConnection);
            IRestRequest request = new RestRequest("/User/EditProfile", Method.POST);
            request.RequestFormat = DataFormat.Json;
            request.AddBody(profile);
            IRestResponse response = client.Execute(request);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                success = false;
            }

            return success;
        }

        private List<UserVM> ParseUsersContractToUsersVM(List<UserContract> usersContract)
        {
            ConcurrentBag<UserVM> users = new ConcurrentBag<UserVM>();

            if (usersContract != null)
            {
                usersContract.AsParallel().WithDegreeOfParallelism(MAX_PROCESS)
                    .ForAll(userContract =>
                    {
                        UserVM user = new UserVM()
                        {
                            Identifier = userContract.Identifier.ToUpper(),
                            UserCompanyIdentifier = userContract.userCompanyIdentifier,
                            Name = userContract.Name,
                            LastName = userContract.LastName,
                            Enabled = userContract.Enabled ?? userContract.Enabled.Value,
                            StartContractDate = DateTimeHelper.StringGeoVictoriaToDateTime(userContract.ContractDate),
                            EndContractDate = DateTimeHelper.StringGeoVictoriaToDateTime(userContract.endContractDate),
                            PositionIdentifier = userContract.positionIdentifier,
                            PositionName = userContract.positionName,
                            Email = (userContract.Email != null) ? (GeneralHelper.ValidateEmail(userContract.Email.Trim()) ? userContract.Email.Trim() : string.Empty) : userContract.Email,
                            Adress = userContract.Adress,
                            GroupIdentifier = userContract.GroupIdentifier,
                            GroupDescription = userContract.GroupDescription,
                            Custom1 = userContract.Custom1,
                            Custom2 = userContract.Custom2,
                            Custom3 = userContract.Custom3,
                            UserProfile = userContract.UserProfile,
                            Phone = userContract.Phone
                        };

                        users.Add(user);
                    }
                ); 
            }

            return users.ToList();
        }

        private List<UserContract> ParseUsersVMToUsersContract(List<UserVM> users)
        {
            ConcurrentBag<UserContract> usersContract = new ConcurrentBag<UserContract>();

            if (users != null)
            {
                users.AsParallel().WithDegreeOfParallelism(MAX_PROCESS)
                    .ForAll(user =>
                    {
                        UserContract userContract = new UserContract()
                        {
                            Identifier = user.Identifier.ToUpper(),
                            userCompanyIdentifier = user.UserCompanyIdentifier,
                            Name = user.Name,
                            LastName = user.LastName,
                            Enabled = user.Enabled,
                            ContractDate = DateTimeHelper.DateTimeToStringGeoVictoria(user.StartContractDate),
                            endContractDate = DateTimeHelper.DateTimeToStringGeoVictoria(user.EndContractDate),
                            positionIdentifier = user.PositionIdentifier,
                            positionName = user.PositionName,
                            Email = (user.Email != null) ? (GeneralHelper.ValidateEmail(user.Email.Trim()) ? user.Email.Trim() : string.Empty) : user.Email,
                            Adress = user.Adress,
                            GroupIdentifier = user.GroupIdentifier,
                            GroupDescription = user.GroupDescription,
                            Custom1 = user.Custom1,
                            Custom2 = user.Custom2,
                            Custom3 = user.Custom3
                        };

                        usersContract.Add(userContract);
                    }
                ); 
            }

            return usersContract.ToList();
        }
    }
}
