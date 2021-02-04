using System.Collections.Generic;
using Duende.IdentityServer;
using Duende.IdentityServer.Models;

namespace IdentityService.Configuration.Clients
{
    public class ClientData
    {
        public static IEnumerable<Client> GetClients()
        {
            //Define the development client
            var clientDev = ClientFactory(clientId: "authcodeflowclient_dev");

            clientDev.ClientSecrets = new List<Secret> { new Secret("mysecret".Sha512()) };

            clientDev.RedirectUris = new List<string>()
            {
                "https://localhost:5001/signin-oidc",
                "https://localhost:5002/signin-oidc",
                "https://localhost:8001/authcode/callback"
            };

            clientDev.PostLogoutRedirectUris = new List<string>()
            {
                "https://localhost:5001/signout-callback-oidc"
            };

            clientDev.FrontChannelLogoutUri = "https://localhost:5001/signout-oidc";

            clientDev.AllowedCorsOrigins = new List<string>()
            {
                "https://localhost:5001"
            };


            //Define the production client
            var clientProd = ClientFactory(clientId: "authcodeflowclient_prod");

            clientProd.ClientSecrets = new List<Secret> { new Secret("mysecret".Sha512()) };

            clientProd.RedirectUris = new List<string>()
            {
                "https://student4-client.webapi.se/signin-oidc"
            };

            clientProd.PostLogoutRedirectUris = new List<string>()
            {
                "https://student4-client.webapi.se/signout-callback-oidc"
            };

            clientProd.FrontChannelLogoutUri = "https://student4-client.webapi.se/signout-oidc";

            clientProd.AllowedCorsOrigins = new List<string>()
            {
                "https://student4-client.webapi.se"
            };

            return new List<Client>()
            {
                clientDev,
                clientProd
            };

        }


        /// <summary>
        /// Create an instance of a client and populate it with data that should be the same for all clients
        /// </summary>
        /// <param name="clientId"></param>
        /// <returns></returns>
        private static Client ClientFactory(string clientId)
        {
            return new Client()
            {
                ClientId = clientId,
                ClientName = "My Client application",
                ClientUri = "https://www.edument.se",
                RequirePkce = true,
                AllowOfflineAccess = true, //Accept refresh tokens
                AllowedGrantTypes = GrantTypes.CodeAndClientCredentials,

                // When requesting both an id token and access token, should the user claims always
                // be added to the id token instead of requiring the client to use the UserInfo endpoint.
                // Defaults to false.
                AlwaysIncludeUserClaimsInIdToken = false,

                //Specifies whether this client is allowed to receive access tokens via the browser. 
                //This is useful to harden flows that allow multiple response types 
                //(e.g. by disallowing a hybrid flow client that is supposed to  use code id_token to add the token response type and thus leaking the token to the browser.
                AllowAccessTokensViaBrowser = false,

                AllowedScopes =
                    {
                        //Standard scopes
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Email,
                        IdentityServerConstants.StandardScopes.Profile,
                        "employee",
                        "payment"
                    },

                AlwaysSendClientClaims = false,
                ClientClaimsPrefix = "client_",
            };
        }
    }
}
