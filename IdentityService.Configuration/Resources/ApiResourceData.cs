using System.Collections.Generic;
using Duende.IdentityServer.Models;

namespace IdentityService.Configuration.Resources
{
    public class ApiResourceData
    {
        public static IEnumerable<ApiResource> Resources()
        {
            return new ApiResource[]
            {
                new ApiResource()
                {
                    Name = "apiresource1",
                    ApiSecrets = new List<Secret>() { new Secret("myapisecret".Sha256()) },

                    Scopes = new List<string> { "apiscope1"},

                    UserClaims = new List<string>
                    {
                        "claim_apiresource1",
                    }
                },
                new ApiResource()
                {
                    Name = "apiresource2",
                    ApiSecrets = new List<Secret>() { new Secret("myapisecret".Sha256()) },

                    Scopes = new List<string> { "apiscope1"},

                    UserClaims = new List<string>
                    {
                        "claim_apiresource2",
                    }
                },
                new ApiResource()
                {
                    Name = "paymentapi",
                    ApiSecrets = new List<Secret>() { new Secret("myapisecret".Sha256()) },

                    Scopes = new List<string> { "payment"},

                    UserClaims = new List<string>
                    {
                        "creditlimit",
                        "paymentaccess",
                        "admin"
                    }
                }
            };
        }
    }
}
