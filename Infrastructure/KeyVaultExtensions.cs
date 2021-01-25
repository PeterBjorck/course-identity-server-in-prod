using System;
using System.Security.Cryptography.X509Certificates;
using Azure.Identity;
using Azure.Security.KeyVault.Certificates;
using Azure.Security.KeyVault.Secrets;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Infrastructure
{
    /// <summary>
    /// Extension methods and helper methods to access the Azure Key Vault
    /// </summary>
    public static class KeyVaultExtensions
    {
        /// <summary>
        /// Add Azure Key vault to the ASP.NET Configuration system
        /// </summary>
        /// <param name="config"></param>
        public static void AddAzureKeyVaultSupport(this IConfigurationBuilder config)
        {
            var builtConfig = config.Build();

            string vaultUrl = builtConfig["Vault:Url"] ?? "";
            string clientId = builtConfig["Vault:ClientId"] ?? "";
            string tenantId = builtConfig["Vault:TenantId"] ?? "";
            string secret = builtConfig["Vault:Secret"] ?? "";

            Console.WriteLine("Adding AzureKeyVault Support");

            CheckIfValueIsProvided(vaultUrl, nameof(vaultUrl), sensitiveValue: false);
            CheckIfValueIsProvided(clientId, nameof(clientId), sensitiveValue: true);
            CheckIfValueIsProvided(tenantId, nameof(tenantId), sensitiveValue: true);
            CheckIfValueIsProvided(secret, nameof(secret), sensitiveValue: true);

            config.AddAzureKeyVault(new Uri(vaultUrl), new ClientSecretCredential(tenantId, clientId, secret));

            Console.WriteLine("- Vault configured");
        }

        private static void CheckIfValueIsProvided(string value, string parameterName, bool sensitiveValue)
        {
            if (string.IsNullOrEmpty(value))
            {
                Console.WriteLine($"Fatal: - {parameterName} not found");
                throw new Exception($"Fatal: {parameterName} not found");
            }
            else
            {
                //To assist troubleshooting in production, we print out the first character for each config value
                if (sensitiveValue)
                    Console.WriteLine($"- {parameterName}: {value.Substring(0, 1)}...");
                else
                    Console.WriteLine($"- {parameterName}: {value}");
            }
        }


        /// <summary>
        /// Load a certificate (with private key) from Azure Key Vault
        ///
        /// Getting a certificate with private key is a bit of a pain, but the code below solves it.
        /// 
        /// Get the private key for Key Vault certificate
        /// https://github.com/heaths/azsdk-sample-getcert
        /// 
        /// See also these GitHub issues: 
        /// https://github.com/Azure/azure-sdk-for-net/issues/12742
        /// https://github.com/Azure/azure-sdk-for-net/issues/12083
        /// </summary>
        /// <param name="config"></param>
        /// <param name="certificateName"></param>
        /// <returns></returns>
        public static X509Certificate2 LoadCertificate(IConfiguration config, string certificateName)
        {
            string vaultUrl = config["Vault:Url"] ?? "";
            string clientId = config["Vault:ClientId"] ?? "";
            string tenantId = config["Vault:TenantId"] ?? "";
            string secret = config["Vault:Secret"] ?? "";

            Console.WriteLine($"Loading certificate '{certificateName}' from Azure Key Vault");

            var credentials = new ClientSecretCredential(tenantId: tenantId, clientId: clientId, clientSecret: secret);
            var certClient = new CertificateClient(new Uri(vaultUrl), credentials);
            var secretClient = new SecretClient(new Uri(vaultUrl), credentials);

            var cert = GetCertificateAsync(certClient, secretClient, certificateName);

            Console.WriteLine("Certificate loaded");
            return cert;
        }


        /// <summary>
        /// Helper method to get a certificate
        /// 
        /// Source https://github.com/heaths/azsdk-sample-getcert/blob/master/Program.cs
        /// </summary>
        /// <param name="certificateClient"></param>
        /// <param name="secretClient"></param>
        /// <param name="certificateName"></param>
        /// <returns></returns>
        private static X509Certificate2 GetCertificateAsync(CertificateClient certificateClient,
                                                                SecretClient secretClient,
                                                                string certificateName)
        {

            KeyVaultCertificateWithPolicy certificate = certificateClient.GetCertificate(certificateName);

            // Return a certificate with only the public key if the private key is not exportable.
            if (certificate.Policy?.Exportable != true)
            {
                return new X509Certificate2(certificate.Cer);
            }

            // Parse the secret ID and version to retrieve the private key.
            string[] segments = certificate.SecretId.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (segments.Length != 3)
            {
                throw new InvalidOperationException($"Number of segments is incorrect: {segments.Length}, URI: {certificate.SecretId}");
            }

            string secretName = segments[1];
            string secretVersion = segments[2];

            KeyVaultSecret secret = secretClient.GetSecret(secretName, secretVersion);

            // For PEM, you'll need to extract the base64-encoded message body.
            // .NET 5.0 preview introduces the System.Security.Cryptography.PemEncoding class to make this easier.
            if ("application/x-pkcs12".Equals(secret.Properties.ContentType, StringComparison.InvariantCultureIgnoreCase))
            {
                byte[] pfx = Convert.FromBase64String(secret.Value);
                return new X509Certificate2(pfx);
            }

            throw new NotSupportedException($"Only PKCS#12 is supported. Found Content-Type: {secret.Properties.ContentType}");
        }
    }
}