using Microsoft.Extensions.Configuration;
using System.IO;

namespace Helpers
{
    public static class ConfigurationHelper
    {
        private static readonly IConfigurationRoot Configuration = null;

        static ConfigurationHelper()
        {
            var builder = new ConfigurationBuilder()
              .SetBasePath(Directory.GetCurrentDirectory())
              .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true) 
              .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
              .AddEnvironmentVariables();

            Configuration = builder.Build();
        }

        public static string Value(string key)
        {
            return Configuration[key];
        }

        public static string GetConnectionString(string key)
        {
            return Configuration.GetConnectionString(key);
        }
    }
}
