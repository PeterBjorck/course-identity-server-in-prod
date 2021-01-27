using System;
using Azure.Identity;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Infrastructure.DataProtection
{
    /// <summary>
    /// Extension method for configuring the data protection API
    /// </summary>
    public static class DataProtectionExtensions
    {
        public static void AddDataProtectionWithSqlServer(this IServiceCollection services, IConfiguration configuration)
        {
            Log.Information("Configuring Data Protection with SQL Server started");
            string connectionString = configuration.GetConnectionString("ConnectionString") ?? "";
            string encryptionKeyUrl = configuration["DataProtection:EncryptionKeyUrl"] ??"";
            string tenantId = configuration["Vault:TenantId"] ?? "";
            string clientId = configuration["Vault:ClientId"] ?? "";
            string clientSecret  = configuration["Vault:Secret"] ?? "";


            CheckIfValueIsProvided(connectionString, nameof(connectionString), sensitiveValue: true);
            CheckIfValueIsProvided(encryptionKeyUrl, nameof(encryptionKeyUrl), sensitiveValue: false);
            CheckIfValueIsProvided(clientId, nameof(clientId), sensitiveValue: true);
            CheckIfValueIsProvided(clientSecret, nameof(clientSecret), sensitiveValue: true);

            services.AddDbContext<DataProtectionContext>(options =>
                options.UseSqlServer(connectionString));

            services.AddDataProtection()
                .PersistKeysToDbContext<DataProtectionContext>()
                .ProtectKeysWithAzureKeyVault(keyIdentifier: new Uri(encryptionKeyUrl),
                                              tokenCredential: new ClientSecretCredential(tenantId, clientId, clientSecret));

            Log.Information("Configuring Data Protection with SQL Server completed");
        }

        private static void CheckIfValueIsProvided(string value, string parameterName, bool sensitiveValue)
        {
            if (string.IsNullOrEmpty(value))
            {
                string msg =$"{parameterName} not found when configuring Data Protection with SQL Server";
                Log.Fatal(msg);
                throw new Exception(msg);
            }
            else
            {
                //To assist troubleshooting in production, we display the first character for each sensitive config value
                if (sensitiveValue)
                    Log.Information($"- {parameterName}: {value.Substring(0, 1)}...");
                else
                    Log.Information($"- {parameterName}: {value}");
            }
        }
    }
}