using System;
using System.Text.RegularExpressions;
using System.Linq;
using System.Collections.Generic;

namespace Helper
{
    public class GeneralHelper
    {
        public static bool ValidateEmail(string email)
        {
            bool isValid;
            try
            {
                isValid = Regex.IsMatch(email,
                    @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
                    @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-0-9a-z]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$",
                    RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(500));
            }
            catch (RegexMatchTimeoutException)
            {
                isValid = false;
            }

            return isValid;
        }

        public static string ParseIdentifier(string identifier)
        {
            string response = identifier;
            if (!string.IsNullOrEmpty(response) &&!string.IsNullOrWhiteSpace(response))
            {
                response = response.Trim().ToUpper();
                int indexOfseparator = response.IndexOf('-');
                if (indexOfseparator > 0)
                {
                    response = response.Remove(indexOfseparator, 1);
                }
            }            

            return response;
        }

        public static string GroupName(string name, string costCenter)
        {
            var groupName = $"{name}({costCenter})";
            if (groupName.Length > 100)
            {
                groupName = name.Substring(0, 100 - (costCenter.Length + 5));
                groupName += $"...({costCenter})";
            }

            return groupName;
        }
        public static List<string> ValidateStringList(string inputParameter)
        {
            List<string> parametersList = new List<string>();
            parametersList = string.IsNullOrEmpty(inputParameter) || string.IsNullOrWhiteSpace(inputParameter) ? new List<string>() : inputParameter.Split(',').ToList();
            parametersList.RemoveAll(x => string.IsNullOrEmpty(x));
            parametersList.RemoveAll(x => string.IsNullOrWhiteSpace(x));
            return parametersList;
        }
    }
}
