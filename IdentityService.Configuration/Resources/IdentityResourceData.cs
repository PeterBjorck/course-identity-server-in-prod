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
            };
        }
    }
}
