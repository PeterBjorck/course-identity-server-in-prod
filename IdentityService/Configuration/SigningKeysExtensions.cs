using System.Security.Cryptography.X509Certificates;
using Duende.IdentityServer;
using Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Serilog;

namespace IdentityService.Configuration
{
    public static class SigningKeysExtensions
    {
        /// <summary>
        /// Add the production signing keys to IdentityServer
        ///
        /// For more details, see RFC 7518 JSON Web Algorithms (JWA)
        /// https://tools.ietf.org/html/rfc7518
        ///
        /// Do notice that RSxxx and PSxxx all uses the same private key
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        public static IIdentityServerBuilder AddProductionSigningCredential(this IIdentityServerBuilder builder, IConfiguration config)
        {
            Log.Information("Adding token signing credentials/keys");

            // First we load the certificates (that contains the private keys) from Azure Key Vault
            var rsaCert = KeyVaultExtensions.LoadCertificate(config, "rsa");
            var p256Cert = KeyVaultExtensions.LoadCertificate(config, "p256");
            var p384Cert = KeyVaultExtensions.LoadCertificate(config, "p384");
            var p521Cert = KeyVaultExtensions.LoadCertificate(config, "p521");

          
            //Add RS256 (RSASSA-PKCS1-v1_5 using SHA-256)
            builder.AddSigningCredential(rsaCert, "RS256");

            //Add RS384 (RSASSA-PKCS1-v1_5 using SHA-384)
            builder.AddSigningCredential(rsaCert, "RS384");

            //Add RS512 (RSASSA-PKCS1-v1_5 using SHA-512)
            builder.AddSigningCredential(rsaCert, "RS512");

            //Add PS256 (RSASSA-PSS using SHA-256 and MGF1 with SHA-256)
            builder.AddSigningCredential(rsaCert, SecurityAlgorithms.RsaSsaPssSha256); 

            //Add PS384 (RSASSA-PSS using SHA-384 and MGF1 with SHA-384)
            builder.AddSigningCredential(rsaCert, SecurityAlgorithms.RsaSsaPssSha384);

            //Add PS512 (RSASSA-PSS using SHA-512 and MGF1 with SHA-512)
            builder.AddSigningCredential(rsaCert, SecurityAlgorithms.RsaSsaPssSha512);

            // Add ES256 (ECDSA using P-256 and SHA-256)
            builder.AddSigningCredential(GetECDsaPrivateKey(p256Cert), IdentityServerConstants.ECDsaSigningAlgorithm.ES256);

            // Add ES384 (ECDSA using P-384 and SHA-384)
            builder.AddSigningCredential(GetECDsaPrivateKey(p384Cert), IdentityServerConstants.ECDsaSigningAlgorithm.ES384);

            // Add ES512 (ECDSA using P-521 and SHA-512)
            builder.AddSigningCredential(GetECDsaPrivateKey(p521Cert), IdentityServerConstants.ECDsaSigningAlgorithm.ES512);

            return builder;
        }


        /// <summary>
        /// Get the private ECDSA key from a certificate and set the KeyID
        ///
        /// For ECDSA certificates we need to set the KeyID manually according to a comment here
        /// https://github.com/IdentityServer/IdentityServer4/blob/main/src/IdentityServer4/host/Startup.cs
        /// (directly using the certificate is not support by Microsoft right now)
        /// 
        /// So we set the KeyID to something that is consistent and tied to the key,
        /// the first part of the thumbprint is a good starting point. 
        /// </summary>
        /// <param name="ecCert"></param>
        /// <returns></returns>
        private static ECDsaSecurityKey GetECDsaPrivateKey(X509Certificate2 ecCert)
        {
            var privateKey = new ECDsaSecurityKey(ecCert.GetECDsaPrivateKey())
            {
                KeyId = ecCert.Thumbprint.Substring(0, 16)
            };

            return privateKey;
        }
    }
}
