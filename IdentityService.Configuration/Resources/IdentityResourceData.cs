using System.Collections.Generic;
using Duende.IdentityServer.Models;

namespace IdentityService.Configuration.Resources
{
    public static class IdentityResourceData
    {
        public static IEnumerable<IdentityResource> Resources()
        {
            return new IdentityResource[]
            {
                    new IdentityResources.OpenId(),
                    new IdentityResources.Profile(),
                    new IdentityResources.Email(),
                    new IdentityResource(name: "idresource1", userClaims: new string[] {"claim_idresource1"}),
                    new IdentityResource(name: "employee", userClaims: new string[] {
                        "employmentid",
                        "employeetype",
                        "admin"
                    }),
            };
        }
    }
}
