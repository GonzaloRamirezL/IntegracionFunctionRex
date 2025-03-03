using Common.DTO.GeoVictoria;
using System.Collections.Generic;

namespace Common.ViewModels
{
    public class ContractContainerVM
    {
        public List<ContractClientVM> ToCreate { get; set; }
        public List<ContractDTO> ToUpdate { get; set; }
    }
}
