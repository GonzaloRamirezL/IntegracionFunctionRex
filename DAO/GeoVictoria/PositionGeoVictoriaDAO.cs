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
    public class PositionGeoVictoriaDAO : IPositionGeoVictoriaDAO
    {
        private const int DEFAULT_DEGREE_OF_PARALLELLISM = 10;

        private readonly int MAX_PROCESS;
        private readonly bool EXECUTE = false;

        public PositionGeoVictoriaDAO()
        {
            bool success = int.TryParse(ConfigurationHelper.Value("maxParallel"), out int maxProcess);
            MAX_PROCESS = success ? maxProcess : DEFAULT_DEGREE_OF_PARALLELLISM;

            bool.TryParse(ConfigurationHelper.Value("Execute"), out EXECUTE);
        }

        public List<PositionVM> GetAll(GeoVictoriaConnectionVM gvConnection)
        {
            try
            {
                IRestClient client = ConnectionHelper.ConnectGeoVictoria(gvConnection);
                IRestRequest request = new RestRequest("/Position/List", Method.POST);
                request.RequestFormat = DataFormat.Json;
                IRestResponse response = client.Execute(request);
                var content = response.Content;
                List<PositionContract> positionsContract = JsonConvert.DeserializeObject<List<PositionContract>>(content);
                List<PositionVM> positions = ParsePositionsContractToPositionVM(positionsContract);
                return positions;
            }
            catch (Exception e)
            {
                LogHelper.Log("ERROR: " + e.Message);
                throw;
            }
        }

        public (bool success, string message) AddPosition(string posName, GeoVictoriaConnectionVM gvConnection)
        {
            PositionContract newPos = new PositionContract { DESCRIPCION_CARGO = posName };
            bool success = true;
            string message = $"Nuevo cargo desde Rex+: {posName}";
            if (EXECUTE)
            {
                IRestClient client = ConnectionHelper.ConnectGeoVictoria(gvConnection);
                IRestRequest request = new RestRequest("/Position/Add", Method.POST);
                request.RequestFormat = DataFormat.Json;
                request.AddBody(newPos);
                IRestResponse response = client.Execute(request);

                if (response.StatusCode != HttpStatusCode.OK)
                {
                    success = false;
                    message = $"{response.Content} - {posName}";
                }
            }

            return (success, message);
        }
        private List<PositionVM> ParsePositionsContractToPositionVM(List<PositionContract> positionsContract)
        {
            ConcurrentBag<PositionVM> positions = new ConcurrentBag<PositionVM>();

            if (positionsContract != null)
            {
                positionsContract.AsParallel().WithDegreeOfParallelism(MAX_PROCESS)
                    .ForAll(positionContract =>
                    {
                        PositionVM position = new PositionVM()
                        {
                            Identifier = positionContract.IDENTIFICADOR,
                            PositionDescription = positionContract.DESCRIPCION_CARGO,
                            Critic = positionContract.CRITICO,
                            Status = positionContract.ESTADO_CARGO,
                            Prioritary = positionContract.CARGO_PRIORITARIO
                        };

                        positions.Add(position);
                    }
                ); 
            }

            return positions.ToList();
        }
    }
}
