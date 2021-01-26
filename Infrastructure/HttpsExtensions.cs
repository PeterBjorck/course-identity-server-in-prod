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
                               httpsOptions.SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13;

                               //Only do this on Linux, different operating systems handles this differently
                               if (!System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
                               {
                                   httpsOptions.OnAuthenticate = (conContext, sslAuthOptions) =>
                                   {
                                       sslAuthOptions.CipherSuitesPolicy = cipherSuitesPolicy;
                                   };
                               }

                           });
                       });
               });

            return webBuilder;
        }

        private static readonly CipherSuitesPolicy cipherSuitesPolicy = new CipherSuitesPolicy
        (
            new System.Net.Security.TlsCipherSuite[]
            {
                // Cipher suits as recommended by: https://wiki.mozilla.org/Security/Server_Side_TLS
                // Listed in preferred order.
                // From: https://en.internet.nl
                // High

                TlsCipherSuite.TLS_ECDHE_ECDSA_WITH_CHACHA20_POLY1305_SHA256,
                TlsCipherSuite.TLS_ECDHE_ECDSA_WITH_AES_128_GCM_SHA256,
                TlsCipherSuite.TLS_ECDHE_RSA_WITH_AES_256_GCM_SHA384,
                TlsCipherSuite.TLS_ECDHE_RSA_WITH_CHACHA20_POLY1305_SHA256,
                TlsCipherSuite.TLS_ECDHE_RSA_WITH_AES_128_GCM_SHA256,
                // Medium
                TlsCipherSuite.TLS_ECDHE_ECDSA_WITH_AES_256_CBC_SHA,
                TlsCipherSuite.TLS_ECDHE_ECDSA_WITH_AES_128_CBC_SHA256,
                TlsCipherSuite.TLS_ECDHE_ECDSA_WITH_AES_128_CBC_SHA,
                TlsCipherSuite.TLS_ECDHE_ECDSA_WITH_AES_256_CBC_SHA384,
                TlsCipherSuite.TLS_ECDHE_ECDSA_WITH_AES_128_GCM_SHA256,
                TlsCipherSuite.TLS_ECDHE_ECDSA_WITH_AES_256_GCM_SHA384,
                TlsCipherSuite.TLS_ECDHE_ECDSA_WITH_CHACHA20_POLY1305_SHA256,
                TlsCipherSuite.TLS_ECDHE_RSA_WITH_CHACHA20_POLY1305_SHA256,
                TlsCipherSuite.TLS_ECDHE_RSA_WITH_AES_256_GCM_SHA384,
                TlsCipherSuite.TLS_ECDHE_RSA_WITH_AES_128_GCM_SHA256,
                TlsCipherSuite.TLS_DHE_RSA_WITH_AES_256_GCM_SHA384,
                TlsCipherSuite.TLS_DHE_RSA_WITH_CHACHA20_POLY1305_SHA256,
                TlsCipherSuite.TLS_DHE_RSA_WITH_AES_128_GCM_SHA256,
                TlsCipherSuite.TLS_DHE_RSA_WITH_AES_128_GCM_SHA256,
                TlsCipherSuite.TLS_DHE_RSA_WITH_AES_256_CBC_SHA256,
                TlsCipherSuite.TLS_DHE_RSA_WITH_AES_256_CBC_SHA,
                TlsCipherSuite.TLS_DHE_RSA_WITH_AES_128_CBC_SHA256,
                TlsCipherSuite.TLS_DHE_RSA_WITH_AES_128_CBC_SHA,
                TlsCipherSuite.TLS_DHE_RSA_WITH_AES_256_GCM_SHA384,
                TlsCipherSuite.TLS_AES_128_GCM_SHA256,
                TlsCipherSuite.TLS_AES_256_GCM_SHA384,
                TlsCipherSuite.TLS_CHACHA20_POLY1305_SHA256
            }
        );

    }

}
