using System;
using System.Globalization;

namespace Helper
{
    public class DateTimeHelper
    {
        public static string DateTimeToStringGeoVictoria(DateTime? dateTime)
        {
            string response = null;
            if (dateTime.HasValue)
            {
                response = dateTime.Value.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture);
            }

            return response;
        }
        public static string DateTimeToStringRex(DateTime? dateTime)
        {
            string response = null;
            if (dateTime.HasValue)
            {
                response = dateTime.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            }

            return response;
        }
        public static DateTime? StringGeoVictoriaToDateTime(string dateText)
        {
            try
            {
                DateTime dateTime = DateTime.ParseExact(dateText, "yyyyMMddHHmmss", CultureInfo.InvariantCulture);
                return dateTime;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static string DateTimeToStringFile(DateTime? dateTime)
        {
            string response = null;
            if (dateTime.HasValue)
            {
                response = dateTime.Value.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
            }

            return response;
        }
        
        public static DateTime? StringDateTimeFileToDateTime(string dateText)
        {
            try
            {
                DateTime dateTime = DateTime.ParseExact(dateText, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                return dateTime;
            }
            catch (Exception)
            {
                return null;
            }
        }
        public static DateTime? StringTDateTimeToDateTime(string dateText)
        {
            try
            {
                DateTime dateTime = DateTime.ParseExact(dateText, "yyyy-MM-dd'T'HH:mm:ss", CultureInfo.InvariantCulture);
                return dateTime;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static DateTime? StringDateTimeToDateTime(string dateText)
        {
            try
            {
                DateTime dateTime = DateTime.ParseExact(dateText, "yyyyMMddHHmmss", CultureInfo.InvariantCulture);
                return dateTime;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static string TimeSpanToString(TimeSpan? time)
        {
            string response = null;
            if (time.HasValue)
            {
                response = time.Value.ToString(@"hh\:mm", CultureInfo.InvariantCulture);
            }

            return response;
        }

        public static TimeSpan? StringToTimeSpan(string time)
        {
            try
            {
                TimeSpan timeSpan = TimeSpan.ParseExact(time, "hh\\:mm", CultureInfo.InvariantCulture);

                return timeSpan;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static TimeSpan StringToTimeSpanNoNull(string time)
        {
            TimeSpan timeSpan = TimeSpan.ParseExact(time, "g", CultureInfo.InvariantCulture);
            return timeSpan;
        }
        public static DateTime FirstDayWeekBefore(DateTime fecha)
        {
            try
            {
                //Obtengo el dia de la semana de la fecha(convertido a Int)
                int dia = Convert.ToInt32(fecha.DayOfWeek);
                //A la fecha actual le resto los dias de la semana que han pasado desde le inicio
                //  el valor de dia lo converito a negativo multiplicandolo por -1 
                //  y uso la funcion AddDays para restarle esos dias a la fecha Actual
                // y obtener la fecha del inicio de la semana OSEA el DOMINGO ANTERIOR al dia de hoy en este caso.
                //PARA obetner el LUNES anterior solo le resto un dia mas:
                dia = dia - 1;
                DateTime fechaInicioSemana = fecha.AddDays((dia) * (-1));
                //Regreso un data time con la fecha que necesitas
                return fechaInicioSemana;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        public static bool IsDateTimeInStringRange(string startDateString, string endDateString, DateTime intersectingDate, bool emptyEndDateIsValid)
        {
            var startDate = DateTimeHelper.StringDateTimeFileToDateTime(startDateString);
            var endDate = DateTimeHelper.StringDateTimeFileToDateTime(endDateString);

            if (startDate == null)
            {
                return false;
            }

            if (endDate == null && !emptyEndDateIsValid)
            {
                return false;
            }

            if (startDate <= intersectingDate)
            {
                if (endDate.HasValue && intersectingDate <= endDate)
                {
                    return true;
                }
                else if(!endDate.HasValue)
                {
                    return true;
                }
            }

            return false;

        }

    }
}
