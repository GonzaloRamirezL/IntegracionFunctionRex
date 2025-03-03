using Common.DTO.Rex;
using Common.Entity;
using Common.Enum;
using Common.Helper;
using Common.ViewModels;
using DAO.GeoVictoria;
using DAO.Rex;
using Helper;
using IBusiness;
using IDAO.GeoVictoria;
using IDAO.Rex;
using System.Collections.Generic;
using System.Linq;

namespace Business
{
    public class PositionBusiness : IPositionBusiness
    {
        private const string NO_POSITION = "#Ninguno#";

        private readonly IPositionGeoVictoriaDAO PositionGeoVictoriaDAO;
        private readonly ICatalogoRexDAO CatalogoRexDAO;

        public PositionBusiness()
        {
            this.PositionGeoVictoriaDAO = new PositionGeoVictoriaDAO();
            this.CatalogoRexDAO = new CatalogoRexDAO();
        }

        /// <summary>
        /// Method to get all positions from GeoVictoria
        /// </summary>
        /// <returns></returns>
        private List<PositionVM> GetAll(GeoVictoriaConnectionVM gvConnection)
        {
            List<PositionVM> positions = this.PositionGeoVictoriaDAO.GetAll(gvConnection);

            return positions;
        }

        public List<UserVM> HomologateUsersPositions(List<UserVM> users, GeoVictoriaConnectionVM gvConnection)
        {
            List<UserVM> homologatedUsers = new List<UserVM>();
            List<PositionVM> positions = GetAll(gvConnection);
            if (positions.Count > 0)
            {
                foreach (UserVM user in users)
                {                    
                    PositionVM noPosition = positions.FirstOrDefault(p => p.PositionDescription.Trim() == NO_POSITION);
                    user.PositionName = noPosition.PositionDescription;
                    user.PositionIdentifier = noPosition.Identifier;
                    homologatedUsers.Add(user);
                }
            }
            return homologatedUsers;
        }

        public List<PositionVM> ProcessPosition(RexExecutionVM rexExecutionVM,List<Contrato> contract, List<ObjetoCatalogo> items)
        {
            if (rexExecutionVM.CreateAllPositions)
            {
                List<LogEntity> logEntities = new List<LogEntity>();
                List<PositionVM> positionsGV = GetAll(new GeoVictoriaConnectionVM() { TestEnvironment = rexExecutionVM.TestEnvironment, ApiKey = rexExecutionVM.ApiKey, ApiSecret = rexExecutionVM.ApiSecret});
                List<string> positionRex;

                if (rexExecutionVM.OrgDevelopment)
                {
                    positionRex = items.Where(a => contract.Any(b => b.cargo_id == a.id)).Select(c => c.nombre).ToList();
                }
                else
                {
                    positionRex = items.Where(a => contract.Any(b => b.cargo == a.item)).Select(c => c.nombre).ToList();
                }

                foreach (var pos in positionRex)
                {
                    var position = positionsGV.Any(p => !string.IsNullOrEmpty(pos) && p.PositionDescription.Trim().ToLower() == pos.Trim().ToLower());
                    if (!position)
                    {
                        (bool success, string message) = this.PositionGeoVictoriaDAO.AddPosition(pos, new GeoVictoriaConnectionVM() { TestEnvironment = rexExecutionVM.TestEnvironment, ApiKey = rexExecutionVM.ApiKey, ApiSecret = rexExecutionVM.ApiSecret });
                        logEntities.Add(LogEntityHelper.Position(
                            rexExecutionVM, 
                            LogEvent.ADD, 
                            pos, 
                            message,
                            success ? LogType.Info : LogType.Error));
                    }
                }
                new TableStorageHelper().Upsert(logEntities, LogTable.NAME);
            }           
            List<PositionVM> result = GetAll(new GeoVictoriaConnectionVM() { TestEnvironment = rexExecutionVM.TestEnvironment, ApiKey = rexExecutionVM.ApiKey, ApiSecret = rexExecutionVM.ApiSecret});
            return result;
        }
    }
}
