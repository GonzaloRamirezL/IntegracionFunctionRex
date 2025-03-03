using Common.DTO.Rex;
using Common.Entity;
using Common.Enum;
using Common.Helper;
using Common.ViewModels;
using Common.ViewModels.API;
using DAO.GeoVictoria;
using Helper;
using IBusiness;
using IDAO.GeoVictoria;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace Business
{
    public class GroupBusiness : IGroupBusiness
    {
        private readonly IGroupGeoVictoriaDAO GroupGeoVictoriaDAO;

        public GroupBusiness()
        {
            this.GroupGeoVictoriaDAO = new GroupGeoVictoriaDAO();
        }

        public List<GroupApiVM> ProcessGroups(RexExecutionVM rexExecutionVM, List<ObjetoCatalogo> catalogItems)
        {
            List<LogEntity> logEntities = new List<LogEntity>();
            List<GroupApiVM> gvGroups = this.GetAll(new GeoVictoriaConnectionVM() { TestEnvironment = rexExecutionVM.TestEnvironment, ApiKey = rexExecutionVM.ApiKey, ApiSecret = rexExecutionVM.ApiSecret });
            List<string> rexGroups = new List<string>();

            if (rexExecutionVM.CreateGroups || rexExecutionVM.DisableGroupIfNotFound)
            {
                string groupName = string.Empty;
                foreach (var catalog in catalogItems)
                {
                    groupName = GeneralHelper.GroupName(catalog.nombre, catalog.item);
                    if (rexExecutionVM.CreateGroups && !gvGroups.Any(x => x.Description == groupName))
                    {
                        (bool success, string message) = this.GroupGeoVictoriaDAO.Add($"{rexExecutionVM.CompanyName}\\{groupName}", new GeoVictoriaConnectionVM() { TestEnvironment = rexExecutionVM.TestEnvironment, ApiKey = rexExecutionVM.ApiKey, ApiSecret = rexExecutionVM.ApiSecret });
                        logEntities.Add(LogEntityHelper.Group(
                            rexExecutionVM,
                            LogEvent.ADD,
                            catalog.item,
                            message,
                            success ? LogType.Info : LogType.Error));
                    }
                    rexGroups.Add(groupName);
                }

                foreach (var group in gvGroups)
                {
                    if (group.CostCenter == null)
                    {
                        logEntities.Add(LogEntityHelper.Group(
                            rexExecutionVM,
                            LogEvent.NONE,
                            group.CostCenter,
                            $"Grupo {group.Description} no tiene código centro costo",
                            LogType.Warning));
                    }
                    else if (!rexGroups.Contains(group.Description) && rexExecutionVM.DisableGroupIfNotFound)
                    {
                        (bool success, string message) = this.GroupGeoVictoriaDAO.Delete(group.CostCenter, new GeoVictoriaConnectionVM() { TestEnvironment = rexExecutionVM.TestEnvironment, ApiKey = rexExecutionVM.ApiKey, ApiSecret = rexExecutionVM.ApiSecret });
                        logEntities.Add(LogEntityHelper.Group(
                            rexExecutionVM, 
                            LogEvent.DELETE, 
                            group.CostCenter, 
                            message,
                            success ? LogType.Info : LogType.Error));
                    }
                }

                gvGroups = this.GetAll(new GeoVictoriaConnectionVM() { TestEnvironment = rexExecutionVM.TestEnvironment, ApiKey = rexExecutionVM.ApiKey, ApiSecret = rexExecutionVM.ApiSecret });
            }

            if (rexExecutionVM.AssignNewUsersToTempGroup && !gvGroups.Any(x => x.CostCenter == DefaultGroup.Identifier))
            {
                (bool success, string message) = this.GroupGeoVictoriaDAO.Add($"{rexExecutionVM.CompanyName}\\{GeneralHelper.GroupName(DefaultGroup.Description, DefaultGroup.Identifier)}", new GeoVictoriaConnectionVM() { TestEnvironment = rexExecutionVM.TestEnvironment, ApiKey = rexExecutionVM.ApiKey, ApiSecret = rexExecutionVM.ApiSecret });
                logEntities.Add(LogEntityHelper.Group(
                    rexExecutionVM,
                    LogEvent.ADD,
                    DefaultGroup.Identifier,
                    message,
                    success ? LogType.Info : LogType.Error));
            }

            new TableStorageHelper().Upsert<LogEntity>(logEntities, LogTable.NAME);

            return gvGroups;
        }

        private List<GroupApiVM> GetAll(GeoVictoriaConnectionVM gvConnection)
        {
            List<GroupApiVM> groups = this.GroupGeoVictoriaDAO.GetAll(gvConnection);

            return groups;
        }

        public string GetBaseFolder(string groupPath)
        {
            string[] totalPath = groupPath.Split('\\');
            string baseFolder = totalPath[0];

            return baseFolder;
        }

        public List<string> GetNewGroups(List<Tuple<string, string>> groups, GeoVictoriaConnectionVM gvConnection)
        {
            List<string> newGroups = new List<string>();
            List<string> victoriaGroups = GetAll(gvConnection).Select(x => x.Path).ToList();
            string baseFolder = GetBaseFolder(victoriaGroups.FirstOrDefault());
            foreach (var group in groups)
            {
                bool existGroup = victoriaGroups.Exists(g => g.Contains("(" + group.Item2 + ")"));
                if (!existGroup)
                {
                    string newGroup = baseFolder + "\\" + group.Item1 + "(" + group.Item2 + ")";
                    newGroups.Add(newGroup);
                }
            }

            return newGroups;
        }

        public List<Tuple<string, string>> GetGroupsFromUsers(List<UserVM> users)
        {
            List<Tuple<string, string>> groups = new List<Tuple<string, string>>();
            foreach (UserVM user in users)
            {
                groups.Add(Tuple.Create(user.GroupDescription, user.GroupIdentifier));
            }

            return groups;
        }

    }
}
