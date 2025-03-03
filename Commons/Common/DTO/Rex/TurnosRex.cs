using System;
using System.Collections.Generic;
using System.Text;

namespace Common.DTO.Rex
{
    public class TurnosRex
    {
        public string id { get; set; }
        public string nombre { get; set; }
        public string marca_colacion { get; set; }
        public string duracion_colacion { get; set; }
        public string trabaja_festivos { get; set; }
        public string tolerancia_colacion { get; set; }
        public ScheduleOfTheDay horario_lunes { get; set; }
        public ScheduleOfTheDay horario_martes { get; set; }
        public ScheduleOfTheDay horario_miercoles { get; set; }
        public ScheduleOfTheDay horario_jueves { get; set; }
        public ScheduleOfTheDay horario_viernes { get; set; }
        public ScheduleOfTheDay horario_sabado { get; set; }
        public ScheduleOfTheDay horario_domingo { get; set; }

    }
    public class ScheduleOfTheDay
    {
        public string entrada { get; set; }
        public string salida { get; set; }
        public string tolerancia { get; set; }

    }

}
