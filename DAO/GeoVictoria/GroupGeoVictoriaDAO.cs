using Common.DTO.GeoVictoria;
using Common.ViewModels.API;
using Helper;
using IDAO.GeoVictoria;
using Newtonsoft.Json;
using RestSharp;
using System.Collections.Generic;
using System;
using Common.ViewModels;
using Helpers;
using System.Net;

namespace DAO.GeoVictoria
{
    public class GroupGeoVictoriaDAO : IGroupGeoVictoriaDAO
    {
        private readonly bool EXECUTE = false;

        public GroupGeoVictoriaDAO()
        {
            bool.TryParse(ConfigurationHelper.Value("Execute"), out EXECUTE);
        }

        public List<GroupApiVM> GetAll(GeoVictoriaConnectionVM gvConnection)
        {
            try
            {
                IRestClient client = ConnectionHelper.ConnectGeoVictoria(gvConnection);
                IRestRequest request = new RestRequest("/Group/ListGroup", Method.POST);
                request.RequestFormat = DataFormat.Json;
                IRestResponse response = client.Execute(request);
                var content = response.Content;
                List<GroupApiVM> groups = JsonConvert.DeserializeObject<List<GroupApiVM>>(content);
                return groups;
            }
            catch (Exception e)
            {
                LogHelper.Log("ERROR: " + e.Message);
                throw;
            }
        }

        /// <summary>
        /// El json a enviar debe ser: 
        /// "PATH":"Nombre Empresa\\Nombre Grupo(CentroCosto)"
        /// </summary>
        /// <param name="groupPath"></param>
        /// <returns></returns>
        public (bool success, string message) Add(string groupPath, GeoVictoriaConnectionVM gvConnection)
        {
            bool success = true;
            string message = $"Nuevo grupo desde Rex+: {groupPath}";
            if (EXECUTE)
            {
                GroupContract newGroup = new GroupContract()
                {
                    PATH = groupPath
                };
                IRestClient client = ConnectionHelper.ConnectGeoVictoria(gvConnection);
                IRestRequest request = new RestRequest("/Group/AddPath", Method.POST);
                request.RequestFormat = DataFormat.Json;
                request.AddBody(newGroup);
                IRestResponse response = client.Execute(request);
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    success = false;
                    message = $"{response.Content} - {groupPath}";
                }
            }

            return (success, message);
        }

        public (bool success, string message) Delete(string costCenter, GeoVictoriaConnectionVM gvConnection)
        {
            bool success = true;
            string message = $"Item {costCenter} no existe en Rex+";
            if (EXECUTE)
            {
                GroupApiVM newGroup = new GroupApiVM()
                {
                    CostCenter = costCenter
                };
                IRestClient client = ConnectionHelper.ConnectGeoVictoria(gvConnection);
                IRestRequest request = new RestRequest("/Group/DeactivateGroup", Method.POST);
                request.RequestFormat = DataFormat.Json;
                request.AddBody(newGroup);
                IRestResponse response = client.Execute(request);

                if (response.StatusCode != HttpStatusCode.OK)
                {
                    success = false;
                    message = $"{response.Content} - {costCenter}";
                }
            }

            return (success, message);
        }

    }
}
