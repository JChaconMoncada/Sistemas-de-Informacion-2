using System;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace Sistema_contable.Services
{
    public static class ConfiguracionApp
    {
        private static IConfiguration _config;

        public static IConfiguration Config
        {
            get
            {
                if (_config == null)
                {
                    var builder = new ConfigurationBuilder()
                        .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false);
                    _config = builder.Build();
                }
                return _config;
            }
        }

        public static string SupabaseUrl => Config["Supabase:ProjectUrl"] ?? string.Empty;
        public static string SupabaseAnonKey => Config["Supabase:AnonKey"] ?? string.Empty;
    }
}