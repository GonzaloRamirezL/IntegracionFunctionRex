using Common.DTO.Rex;
using Common.ViewModels;
using Common.ViewModels.API;
using System;
using System.Collections.Generic;

namespace IBusiness
{
    public interface IGroupBusiness
    {
        /// <summary>
        /// A partir de un listado de objetos de catalogos y dependiendo la configuracion de la integración, 
        /// se crean los grupos a partir de centro costos o sedes.
        /// Luego se obtiene el listado actualizado de los grupos de GV y se retorna.
        /// </summary>
        /// <param name="rexExecutionVM"></param>
        /// <param name="catalogItems"></param>
        /// <returns></returns>
        List<GroupApiVM> ProcessGroups(RexExecutionVM rexExecutionVM, List<ObjetoCatalogo> catalogItems);

        /// <summary>
        /// Get the base folder
        /// </summary>
        /// <returns></returns>
        string GetBaseFolder(string groupPath);

        /// <summary>
        /// Get the group that should be added to GeoVictoria
        /// </summary>
        /// <param name="groups"></param>
        /// <returns></returns>
        List<string> GetNewGroups(List<Tuple<string, string>> groups, GeoVictoriaConnectionVM gvConnection);

        /// <summary>
        /// Get group informations that a users list contains
        /// </summary>
        /// <param name="users"></param>
        /// <returns></returns>
        List<Tuple<string, string>> GetGroupsFromUsers(List<UserVM> users);
    }
}
