using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TekLabServiceCore.Extensions
{
    public static class ConfigurationExtensions
    {
        public static string GetMongoDBConnectionString(this IConfiguration configuration)
        {
            return configuration.GetSection("MongoDB").GetValue<string>("Connectionstring");

        }

        public static string GetMongoDBPort(this IConfiguration configuration)
        {
            return configuration.GetSection("MongoDB").GetValue<string>("Port");

        }
        public static string GetMongoDBName(this IConfiguration configuration)
        {
            return configuration.GetSection("MongoDB").GetValue<string>("DBName");

        }
        public static string GetMongoDBUsername(this IConfiguration configuration)
        {
            return configuration.GetSection("MongoDB").GetValue<string>("Username");

        }
        public static string GetMongoDBPassword(this IConfiguration configuration)
        {
            return configuration.GetSection("MongoDB").GetValue<string>("Password");

        }
        public static string GetMongoDBSecurity(this IConfiguration configuration)
        {
            return configuration.GetSection("MongoDB").GetValue<string>("DbSecurity");

        }

        /*Email*/
        public static string GetSMTPServer(this IConfiguration configuration)
        {
            return configuration.GetSection("Email").GetValue<string>("SMTPServer");

        }
        public static string GetSMTPPort(this IConfiguration configuration)
        {
            return configuration.GetSection("Email").GetValue<string>("SMTPPort");

        }
        public static string GetSMTPUserName(this IConfiguration configuration)
        {
            return configuration.GetSection("Email").GetValue<string>("UserName");

        }
        public static string GetSMTPPassword(this IConfiguration configuration)
        {
            return configuration.GetSection("Email").GetValue<string>("Password");

        }
        public static string GetPortalDomain(this IConfiguration configuration)
        {
            return configuration.GetValue<string>("portalUrl");

        }
        public static string GetEndpointSecret(this IConfiguration configuration)
        {
            return configuration.GetValue<string>("endpointSecret");

        }
        public static List<string> GetEmailRecipients(this IConfiguration configuration)
        {
            return configuration.GetSection("Email:EmailRecipients").Get<List<string>>();

        }

    }
}
