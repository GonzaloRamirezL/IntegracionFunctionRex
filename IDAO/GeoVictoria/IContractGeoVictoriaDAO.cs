using Common.DTO.GeoVictoria;
using Common.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace IDAO.GeoVictoria
{
    public interface IContractGeoVictoriaDAO
    {
        /// <summary>
        /// Finds user contracts based on the specified filter criteria.
        /// </summary>
        /// <param name="filter"></param>
        /// <returns>
        /// </returns>
        List<ContractDTO> FindUserContracts(GeoVictoriaConnectionVM gvConnection, ContractFilterVM filter);

        /// <summary>
        /// Processes a list of contracts by sending them to the specified endpoint
        /// and returns the response content as a string.
        /// </summary>
        /// <param name="contracts"></param>
        /// <returns>
        /// </returns>
        void ProcessContracts(GeoVictoriaConnectionVM gvConnection, ContractContainerVM contracts);
    }
}

