using System.Collections.Generic;
using Duende.IdentityServer;
using Duende.IdentityServer.Models;

namespace IdentityService.Configuration.Clients
{
    public class ClientData
    {
        public static IEnumerable<Client> GetClients()
        {

            var clientDev = new Client
            {
                ClientId = "authcodeflowclient_dev",        //Unique ID of the client
                ClientName = "My Client application",   //Client display name (used for logging and consent screen)
                ClientUri = "https://www.edument.se",   //URI to further information about client (used on consent screen)
                RequirePkce = true,
                AllowOfflineAccess = true, //Accept refresh tokens

                ClientSecrets = new List<Secret>
                {
                    new Secret
                    {
                        Value = "mysecret".Sha512()
                    }
                },

                AllowedGrantTypes = GrantTypes.Code,

                // When requesting both an id token and access token, should the user claims always
                // be added to the id token instead of requiring the client to use the UserInfo endpoint.
                // Defaults to false.
                AlwaysIncludeUserClaimsInIdToken = false,

                //Specifies whether this client is allowed to receive access tokens via the browser. 
                //This is useful to harden flows that allow multiple response types 
                //(e.g. by disallowing a hybrid flow client that is supposed to  use code id_token to add the token response type and thus leaking the token to the browser.
                AllowAccessTokensViaBrowser = false,

                RedirectUris =
                {
                    "https://localhost:5001/signin-oidc",
                    "https://localhost:5002/signin-oidc"
                },

                PostLogoutRedirectUris =
                {
                    "https://localhost:5001/signout-callback-oidc"
                },
                FrontChannelLogoutUri = "https://localhost:5001/signout-oidc",

                // By default a client has no access to any resources
                // specify the allowed resources by adding the corresponding scopes names.
                // If empty, the client can't access any scope
                AllowedScopes =
                {
                    //Standard scopes
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Email,
                    IdentityServerConstants.StandardScopes.Profile,
                    IdentityServerConstants.StandardScopes.Phone,
                },

                AllowedCorsOrigins =
                {
                    "https://localhost:5001"
                }
            };

            var clientProd = new Client
            {
                ClientId = "authcodeflowclient_prod",        //Unique ID of the client
                ClientName = "My Client application",   //Client display name (used for logging and consent screen)
                ClientUri = "https://www.edument.se",   //URI to further information about client (used on consent screen)
                RequirePkce = true,
                AllowOfflineAccess = true, //Accept refresh tokens

                ClientSecrets = new List<Secret>
                {
                    new Secret
                    {
                        Value = "mysecret".Sha512()
                    }
                },

                AllowedGrantTypes = GrantTypes.Code,

                // When requesting both an id token and access token, should the user claims always
                // be added to the id token instead of requiring the client to use the UserInfo endpoint.
                // Defaults to false.
                AlwaysIncludeUserClaimsInIdToken = false,

                //Specifies whether this client is allowed to receive access tokens via the browser. 
                //This is useful to harden flows that allow multiple response types 
                //(e.g. by disallowing a hybrid flow client that is supposed to  use code id_token to add the token response type and thus leaking the token to the browser.
                AllowAccessTokensViaBrowser = false,

                RedirectUris =
                {
                    "https://student4-client.webapi.se/signin-oidc"
                },

                PostLogoutRedirectUris =
                {
                    "https://student4-client.webapi.se/signout-callback-oidc"
                },
                FrontChannelLogoutUri = "https://student4-client.webapi.se/signout-oidc",

                // By default a client has no access to any resources
                // specify the allowed resources by adding the corresponding scopes names.
                // If empty, the client can't access any scope
                AllowedScopes =
                {
                    //Standard scopes
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Email,
                    IdentityServerConstants.StandardScopes.Profile,
                    IdentityServerConstants.StandardScopes.Phone,
                    IdentityServerConstants.StandardScopes.OfflineAccess,
                },

                AllowedCorsOrigins =
                {
                    "https://student4-client.webapi.se"
                }
            };


            return new List<Client>()
            {
                clientDev,
                clientProd
            };
        }

    }
}
