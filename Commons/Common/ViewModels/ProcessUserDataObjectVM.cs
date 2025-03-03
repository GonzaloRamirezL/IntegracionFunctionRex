using Common.DTO.Rex;
using Common.ViewModels.API;
using System.Collections.Generic;

namespace Common.ViewModels
{
    public class ProcessUserDataObjectVM
    {
        public List<Empleado> RexUsers { get; set; }
        public List<UserVM> GeoVictoriaUsers { get; set; }
        public List<Contrato> Contracts { get; set; }
        public List<Cotizaciones> Quotes { get; set; }
        public List<GroupApiVM> GeoVictoriaGroups { get; set; }
        public List<ObjetoCatalogo> RexGroups { get; set; }
        public List<PositionVM> GeoVictoriaPositions { get; set; }
        public List<ObjetoCatalogo> RexPositions { get; set; }
        public List<RexCompanyVM> RexCompanies { get; set; }
    }
}
