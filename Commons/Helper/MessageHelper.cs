using Common.Entity;
using Common.Enum;
using Common.ViewModels;
using Helpers;
using System;
using System.IO;

namespace Helper
{
    public static class MessageHelper
    {
        public static string UserError(string message)
        {
            switch (message)
            {
                case "ERROR:user email is already used in your company":
                    return "El email ya esta siendo utilizado por otro usuario en su empresa";

                case "ERROR:user email is invalid":
                    return "Email del usuario no es válido";

                case "ERROR:user doesn't exist in your company":
                    return "Usuario no existe en su empresa";

                case "ERROR:username must contain a value and must have at least three characters":
                    return "Nombre de usuario debe contener por lo menos 3 caracteres";

                case "ERROR:lastname must contain a value and must have at least three characters":
                    return "Apellido de usuario debe contener por lo menos 3 caracteres";

                case "ERROR:problem trying to complete the operation":
                    return "Ha ocurrido un error en completar la operación";

                case "ERROR:The user already exist on Geovictoria, but is disabled":
                    return "El usuario existe en GeoVictoria, pero esta deshabilitado";

                case "ERROR:The user doesn't exist in GeoVictoria":
                    return "El usuario no existe en GeoVictoria";

                case "ERROR:user is active in another company":
                    return "El usuario se encuentra activado en otra empresa";

                case "ERROR:invalid email address or there is another user with that email address":
                    return "Email del usuario es inválido o ya esta siendo utilizado por otro usuario";

                case "ERROR:there is a user active with the same identifiers":
                    return "Existe un usuario activo con el mismo identificador";

                default:
                    return message;
            }
        }


    }
}
