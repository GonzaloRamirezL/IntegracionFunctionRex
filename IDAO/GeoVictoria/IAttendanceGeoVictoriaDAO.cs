using Common.DTO.GeoVictoria;
using Common.ViewModels;

namespace IDAO.GeoVictoria
{
    public interface IAttendanceGeoVictoriaDAO
    {
        AttendanceContract GetAttendanceBook(GeoVictoriaConnectionVM gvConnection, FilterCustomerContract filter);
    }
}
