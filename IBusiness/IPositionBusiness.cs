using Common.ViewModels;
using System.Collections.Generic;
using Common.DTO.Rex;

namespace IBusiness
{
    public interface IPositionBusiness
    {
        /// <summary>
        /// Method to homologate positions by its description
        /// </summary>
        /// <param name="users"></param>
        /// <param name="gvConnection"></param>
        /// <returns></returns>
        List<UserVM> HomologateUsersPositions(List<UserVM> users, GeoVictoriaConnectionVM gvConnection);
        /// <summary>
        /// Method that compares existing positions in Geovictoria and creates new ones from Rex+
        /// </summary>
        /// <param name="rexExecutionVM"></param>
        /// <param name="contract"></param>
        /// <param name="items"></param>
        /// <returns>All positions homologated in Geovictoria</returns>
        List<PositionVM> ProcessPosition(RexExecutionVM rexExecutionVM,List<Contrato> contract, List<ObjetoCatalogo> items);
    }
}
