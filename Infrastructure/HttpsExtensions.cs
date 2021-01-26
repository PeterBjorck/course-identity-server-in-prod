using System;
using System.Security.Cryptography.X509Certificates;
using Azure.Security.KeyVault.Secrets;
using Azure.Security.KeyVault.Certificates;
using Microsoft.Extensions.Configuration;
using Azure.Identity;
using Microsoft.AspNetCore.Hosting;
using System.Net;
using System.Security.Authentication;
using System.Net.Security;

namespace Infrastructure
{
    public static class HttpsExtensions
    {
        /// <summary>
        /// Adds HTTPS support to Kestrel using a certificate in Azure Key Vault
        /// </summary>
        /// <param name="webBuilder"></param>
        /// <param name="certName">Name of the certificate to load</param>
        /// <returns></returns>
        public static IWebHostBuilder AddHttpsSupport(this IWebHostBuilder webBuilder, string certName)
        {
            Console.WriteLine($"Adding HTTPS Support with the certificate named '{certName}' in Azure Key Vault");
            webBuilder.UseKestrel((context, serverOptions) =>
               {
                   serverOptions.Listen(IPAddress.Any, 80);

                   serverOptions.Listen(IPAddress.Any, 443,
                       options =>
                       {
                           var cert = KeyVaultExtensions.LoadCertificate(context.Configuration, certName);
                           options.UseHttps(serverCertificate: cert, configureOptions: httpsOptions =>
                           {

                               //Add HTTPS configuration here

                           });
                       });
               });

            return webBuilder;
        }
    }
}
