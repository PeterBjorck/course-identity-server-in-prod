using System;
using System.Linq;
using System.Text.RegularExpressions;
using Azure.Identity;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Context;

namespace Infrastructure.DataProtection
{
    /// <summary>
    /// Extension method for configuring the data protection API
    /// </summary>
    public static class DataProtectionExtensions
    {
        public static void AddDataProtectionWithSqlServerForClient(this IServiceCollection services,
            IConfiguration configuration)
        {
            Config config = GetConfig(configuration);

            AddLogEntry("Configuring Data Protection with SQL Server", config);

            ValidateAndLogParameters(config);

            services.AddDbContext<ClientDataProtectionContext>(options =>
                options.UseSqlServer(config.ConnectionString));

            services.AddDataProtection()
                .PersistKeysToDbContext<ClientDataProtectionContext>()
                .ProtectKeysWithAzureKeyVault(keyIdentifier: new Uri(config.EncryptionKeyUrl),
                new ClientSecretCredential(config.TenantId, config.ClientId, config.Secret));

            Log.Information("Configuring Client Data Protection with SQL Server completed");
        }


        public static void AddDataProtectionWithSqlServerForIdentityService(this IServiceCollection services,
            IConfiguration configuration)
        {
            var config = GetConfig(configuration);

            AddLogEntry("Configuring IdentityService Data Protection with SQL Server", config);

            ValidateAndLogParameters(config);

            services.AddDbContext<IdentityServiceDataProtectionContext>(options =>
                options.UseSqlServer(config.ConnectionString));

            services.AddDataProtection()
                .PersistKeysToDbContext<IdentityServiceDataProtectionContext>()
                .ProtectKeysWithAzureKeyVault(keyIdentifier: new Uri(config.EncryptionKeyUrl),
                new ClientSecretCredential(config.TenantId, config.ClientId, config.Secret));

            Log.Information("Configuring IdentityService Data Protection with SQL Server completed");
        }


        public static void AddDataProtectionWithSqlServerForPaymentApi(this IServiceCollection services,
            IConfiguration configuration)
        {
            var config = GetConfig(configuration);

            AddLogEntry("Configuring PaymentAPI Data Protection with SQL Server", config);

            ValidateAndLogParameters(config);

            services.AddDbContext<PaymentApiDataProtectionContext>(options =>
                options.UseSqlServer(config.ConnectionString));

            services.AddDataProtection()
                .PersistKeysToDbContext<PaymentApiDataProtectionContext>()
                .ProtectKeysWithAzureKeyVault(keyIdentifier: new Uri(config.EncryptionKeyUrl),
                new ClientSecretCredential(config.TenantId, config.ClientId, config.Secret));

            Log.Information("Configuring PaymentAPI Data Protection with SQL Server completed");
        }


        private static void ValidateAndLogParameters(Config config)
        {
            //Validate that all input is there
            if (string.IsNullOrEmpty(config.ConnectionString) ||
                string.IsNullOrEmpty(config.EncryptionKeyUrl) ||
                string.IsNullOrEmpty(config.ClientId) ||
                string.IsNullOrEmpty(config.Secret))
            {
                var exc = new NullReferenceException("Data Protection API configuration error, some or all config-parameters are missing");
                Log.Fatal(exc, "Data Protection API configuration error");
                throw exc;
            }
        }

        private static Config GetConfig(IConfiguration configuration)
        {
            return new Config(connectionString: configuration.GetConnectionString("ConnectionString") ?? "",
                encryptionKeyUrl: configuration["DataProtection:EncryptionKeyUrl"] ?? "",
                tenantId: configuration["Vault:TenantId"] ?? "",
                clientId: configuration["Vault:ClientId"] ?? "",
                secret: configuration["Vault:Secret"] ?? "");
        }


        private static void AddLogEntry(string message, Config config)
        {
            var consStr = Regex.Replace(config.ConnectionString, "Password=.*?;", "Password=********");
            var keyUrl = config.EncryptionKeyUrl;
            var clientId = config.ClientId;
            var clientSecret = config.Secret;

            if (String.IsNullOrEmpty(clientId) == false)
                clientId = clientId.Substring(0, 1) + "...";

            if (String.IsNullOrEmpty(clientSecret) == false)
                clientSecret = clientSecret.Substring(0, 1) + "...";

            Log.ForContext("ConnectionString", consStr)
                .ForContext("encryptionKeyUrl", keyUrl)
                .ForContext("vaultClientId", clientId)
                .ForContext("vaultClientSecret", clientSecret)
                .Information(message);
        }

        private class Config
        {
            public readonly string ConnectionString;
            public readonly string EncryptionKeyUrl;
            public readonly string TenantId;
            public readonly string ClientId;
            public readonly string Secret;

            public Config(string connectionString, string encryptionKeyUrl, string tenantId, string clientId, string secret)
            {
                ConnectionString = connectionString;
                EncryptionKeyUrl = encryptionKeyUrl;
                TenantId = tenantId;
                ClientId = clientId;
                Secret = secret;
            }
        }
    }
}
