using System.Collections.Generic;
using Duende.IdentityServer.Models;

namespace IdentityService.Configuration.Resources
{
    public  class ApiScopeData
    {
        public static IEnumerable<ApiScope> Resources()
        {
            return new ApiScope[]
            {
                new ApiScope("scope1"),
                new ApiScope("scope2"),
                new ApiScope("apiscope1", new List<string>() { "claim_apiscope1" }),
                new ApiScope("payment", new List<string>() {
                    "creditlimit",
                    "paymentaccess",
                    "admin"
                }),
            };
        }
    }
}



