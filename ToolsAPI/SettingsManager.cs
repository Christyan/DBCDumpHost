using System.IO;
using System.Reflection;
using Microsoft.Extensions.Configuration;

namespace ToolsAPI
{
    public static class SettingsManager
    {
        public static string definitionDir;
        public static string dbcDir;
        public static string cascToolHost;
        public static string siteHost;
        public static string connectionString;
        public static string apiKey;

        static SettingsManager()
        {
            LoadSettings();
        }

        public static void LoadSettings()
        {
            var config = new ConfigurationBuilder().SetBasePath(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)).AddJsonFile("config.json", optional: false, reloadOnChange: false).Build();
            definitionDir = config.GetSection("config")["definitionsdir"];
            dbcDir = config.GetSection("config")["dbcdir"];
            cascToolHost = config.GetSection("config")["casctoolhost"];
            siteHost = config.GetSection("config")["sitehost"];
            connectionString = config.GetSection("config")["connectionstring"];
            apiKey = config.GetSection("config")["apikey"];
        }
    }
}
