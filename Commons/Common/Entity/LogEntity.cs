using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Entity
{
    public class LogEntity : TableEntity
    {
        /// <summary>
        /// Tipo de registro. Ej: Información, Advertencia o Error
        /// </summary>
        public string LogType { get; set; }

        /// <summary>
        /// Evento o transacción realizada. Ej: Agregar, Editar, Deshabilitar
        /// </summary>
        public string Event { get; set; }

        /// <summary>
        /// Mensaje o resumen de la acción. Ej: Fecha de contrato expirada, Centro de Costo no existe
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Objeto target de la transacción. Ej: Usuario, Grupo, Turno, Permiso
        /// </summary>
        public string Item { get; set; }

        /// <summary>
        /// Identificador del objeto target de la transacción. Ej: 12345678-K, Licencia Médica, Vacaciones
        /// </summary>
        public string Identifier { get; set; }
    }
}
